using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EtaInvoice.Models;
using EtaInvoice.Signing;

namespace EtaInvoice
{
    /// <summary>
    /// Client for the Egyptian Tax Authority e-invoicing APIs
    /// (<c>/api/v1.0/…</c>).
    /// <para>
    /// Bring your own <see cref="HttpClient"/> (recommended: via
    /// <c>IHttpClientFactory</c>), or let the client create one.
    /// Access tokens are fetched with OAuth2 client-credentials and cached
    /// until shortly before expiry.
    /// </para>
    /// </summary>
    public class EtaClient : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = false
        };

        private readonly EtaClientOptions _options;
        private readonly HttpClient _http;
        private readonly bool _ownsHttp;

        private readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);
        private string _cachedToken;
        private DateTimeOffset _tokenExpiresAt;

        public EtaClient(EtaClientOptions options, HttpClient httpClient = null)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Validate();

            _options = options;
            _http = httpClient ?? new HttpClient();
            _ownsHttp = httpClient == null;
        }

        /// <summary>
        /// Gets a bearer access token, reusing the cached one until shortly
        /// before it expires.
        /// </summary>
        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (_cachedToken != null && DateTimeOffset.UtcNow < _tokenExpiresAt)
                return _cachedToken;

            await _tokenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_cachedToken != null && DateTimeOffset.UtcNow < _tokenExpiresAt)
                    return _cachedToken;

                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _options.ClientId),
                    new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
                    new KeyValuePair<string, string>("scope", _options.Scope),
                });

                using (var response = await _http.PostAsync(_options.AuthUrl, form, cancellationToken).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        throw new EtaApiException((int)response.StatusCode, body, message: "Failed to acquire ETA access token.");

                    using (var doc = JsonDocument.Parse(body))
                    {
                        var root = doc.RootElement;
                        _cachedToken = root.GetProperty("access_token").GetString();

                        var expiresIn = 3600;
                        if (root.TryGetProperty("expires_in", out var exp) && exp.TryGetInt32(out var seconds))
                            expiresIn = seconds;

                        // Refresh a minute early so we never send an expired token.
                        _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(expiresIn - 60, 30));
                        return _cachedToken;
                    }
                }
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        /// <summary>
        /// Submits one or more signed documents
        /// (<c>POST /api/v1.0/documentsubmissions/</c>).
        /// </summary>
        /// <param name="documents">Documents to submit (at least one).</param>
        /// <param name="signer">
        /// Optional signer. When provided, each document is canonicalized and an
        /// issuer (<c>I</c>) signature is attached before submission.
        /// When null, documents are submitted as-is — use this only for preprod
        /// testing or when documents already carry signatures.
        /// </param>
        public async Task<SubmissionResult> SubmitDocumentsAsync(
            IEnumerable<EtaDocument> documents,
            IDocumentSigner signer = null,
            CancellationToken cancellationToken = default)
        {
            if (documents == null) throw new ArgumentNullException(nameof(documents));
            var list = documents.ToList();
            if (list.Count == 0) throw new ArgumentException("At least one document is required.", nameof(documents));

            if (signer != null)
            {
                foreach (var document in list)
                {
                    var canonical = EtaCanonicalJson.ToCanonicalJson(document);
                    var signatureValue = await signer.SignAsync(canonical, cancellationToken).ConfigureAwait(false);
                    document.Signatures = new List<DocumentSignature>
                    {
                        new DocumentSignature { Type = "I", Value = signatureValue }
                    };
                }
            }

            var payload = JsonSerializer.Serialize(new { documents = list }, JsonOptions);
            var body = await PostJsonAsync("/api/v1.0/documentsubmissions/", payload, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<SubmissionResult>(body, JsonOptions);
        }

        /// <summary>
        /// Gets submission details and the (paged) document summary list
        /// (<c>GET /api/v1.0/documentsubmissions/{uuid}</c>).
        /// </summary>
        public async Task<SubmissionInfo> GetSubmissionAsync(
            string submissionUuid, int pageNo = 1, int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(submissionUuid)) throw new ArgumentException("Submission uuid is required.", nameof(submissionUuid));
            var body = await GetJsonAsync(
                $"/api/v1.0/documentsubmissions/{Uri.EscapeDataString(submissionUuid)}?pageNo={pageNo}&pageSize={pageSize}",
                cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<SubmissionInfo>(body, JsonOptions);
        }

        /// <summary>
        /// Gets full document details including validation results
        /// (<c>GET /api/v1.0/documents/{uuid}/details</c>).
        /// </summary>
        public async Task<DocumentDetails> GetDocumentDetailsAsync(
            string uuid, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(uuid)) throw new ArgumentException("Document uuid is required.", nameof(uuid));
            var body = await GetJsonAsync($"/api/v1.0/documents/{Uri.EscapeDataString(uuid)}/details", cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<DocumentDetails>(body, JsonOptions);
        }

        /// <summary>
        /// Gets the document source JSON with tax-authority metadata
        /// (<c>GET /api/v1.0/documents/{uuid}/raw</c>).
        /// </summary>
        public Task<string> GetDocumentRawAsync(string uuid, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(uuid)) throw new ArgumentException("Document uuid is required.", nameof(uuid));
            return GetJsonAsync($"/api/v1.0/documents/{Uri.EscapeDataString(uuid)}/raw", cancellationToken);
        }

        /// <summary>
        /// Downloads the PDF printout of a document
        /// (<c>GET /api/v1.0/documents/{uuid}/pdf</c>).
        /// </summary>
        public async Task<byte[]> GetDocumentPdfAsync(string uuid, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(uuid)) throw new ArgumentException("Document uuid is required.", nameof(uuid));
            var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

            using (var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl($"/api/v1.0/documents/{Uri.EscapeDataString(uuid)}/pdf")))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                using (var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw CreateApiException(response, body);
                    }
                    return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Cancels a previously issued document
        /// (<c>PUT /api/v1.0/documents/state/{uuid}/state</c> with <c>cancelled</c>).
        /// Only possible within the ETA-configured cancellation window.
        /// </summary>
        public Task CancelDocumentAsync(string uuid, string reason, CancellationToken cancellationToken = default)
            => ChangeDocumentStateAsync(uuid, "cancelled", reason, cancellationToken);

        /// <summary>
        /// Rejects a received document
        /// (<c>PUT /api/v1.0/documents/state/{uuid}/state</c> with <c>rejected</c>).
        /// Only the receiver can reject, within the ETA-configured rejection window.
        /// </summary>
        public Task RejectDocumentAsync(string uuid, string reason, CancellationToken cancellationToken = default)
            => ChangeDocumentStateAsync(uuid, "rejected", reason, cancellationToken);

        /// <summary>
        /// Declines a cancellation requested by the issuer
        /// (<c>PUT /api/v1.0/documents/state/{uuid}/decline/cancelation</c>).
        /// </summary>
        public async Task DeclineCancellationAsync(string uuid, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(uuid)) throw new ArgumentException("Document uuid is required.", nameof(uuid));
            var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

            using (var request = new HttpRequestMessage(HttpMethod.Put, BaseUrl($"/api/v1.0/documents/{Uri.EscapeDataString(uuid)}/decline/cancelation")))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                using (var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        throw CreateApiException(response, body);
                }
            }
        }

        /// <summary>
        /// Searches sent/received documents
        /// (<c>GET /api/v1.0/documents/search</c>).
        /// Provide <c>submissionDateFrom/To</c> or <c>issueDateFrom/To</c>
        /// (max 30 days apart). Paginate with the metadata continuation token
        /// (<c>EndofResultSet</c> means done).
        /// </summary>
        public async Task<SearchDocumentsResult> SearchDocumentsAsync(
            SearchDocumentsQuery query, CancellationToken cancellationToken = default)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            var qs = query.ToQueryString();
            var body = await GetJsonAsync("/api/v1.0/documents/search" + (qs.Length > 0 ? "?" + qs : string.Empty), cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<SearchDocumentsResult>(body, JsonOptions);
        }

        private async Task ChangeDocumentStateAsync(
            string uuid, string status, string reason, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(uuid)) throw new ArgumentException("Document uuid is required.", nameof(uuid));
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("A reason is required.", nameof(reason));

            var payload = JsonSerializer.Serialize(new { status = status, reason = reason }, JsonOptions);
            await PutJsonAsync($"/api/v1.0/documents/{Uri.EscapeDataString(uuid)}/state", payload, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> GetJsonAsync(string path, CancellationToken cancellationToken)
        {
            var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            using (var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl(path)))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                using (var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        throw CreateApiException(response, body);
                    return body;
                }
            }
        }

        private async Task<string> PostJsonAsync(string path, string json, CancellationToken cancellationToken)
        {
            var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            using (var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl(path)))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                using (var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        throw CreateApiException(response, body);
                    return body;
                }
            }
        }

        private async Task PutJsonAsync(string path, string json, CancellationToken cancellationToken)
        {
            var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            using (var request = new HttpRequestMessage(HttpMethod.Put, BaseUrl(path)))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                using (var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        throw CreateApiException(response, body);
                }
            }
        }

        private string BaseUrl(string path)
        {
            return _options.BaseUrl.TrimEnd('/') + path;
        }

        private static EtaApiException CreateApiException(HttpResponseMessage response, string body)
        {
            var code = TryExtractErrorCode(body);
            return new EtaApiException((int)response.StatusCode, body, code);
        }

        private static string TryExtractErrorCode(string body)
        {
            try
            {
                using (var doc = JsonDocument.Parse(body))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String)
                        return code.GetString();
                    if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object &&
                        error.TryGetProperty("code", out var nested) && nested.ValueKind == JsonValueKind.String)
                        return nested.GetString();
                }
            }
            catch (JsonException)
            {
                // Not JSON — leave the code empty.
            }
            return null;
        }

        public void Dispose()
        {
            if (_ownsHttp)
                _http.Dispose();
            _tokenLock.Dispose();
        }
    }
}
