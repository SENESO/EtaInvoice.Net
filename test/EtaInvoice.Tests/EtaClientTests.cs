using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EtaInvoice.Models;
using EtaInvoice.Signing;
using EtaInvoice.Validation;
using NUnit.Framework;

namespace EtaInvoice.Tests
{
    [TestFixture]
    public class EtaClientTests
    {
        private const string TokenJson = @"{ ""access_token"": ""test-token"", ""token_type"": ""Bearer"", ""expires_in"": 3600 }";

        private static EtaClientOptions Options() => new EtaClientOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret"
        };

        private static EtaDocument ValidDocument() => new EtaDocument
        {
            Issuer = new Taxpayer
            {
                Type = TaxpayerTypes.Business,
                Id = "123456789",
                Name = "Test Company",
                Address = new TaxpayerAddress
                {
                    BranchId = "0",
                    Country = "EG",
                    Governate = "Cairo",
                    RegionCity = "Nasr City",
                    Street = "Main St",
                    BuildingNumber = "10"
                }
            },
            Receiver = new Taxpayer
            {
                Type = TaxpayerTypes.Business,
                Id = "987654321",
                Name = "Buyer Co",
                Address = new TaxpayerAddress
                {
                    Country = "EG",
                    Governate = "Giza",
                    RegionCity = "Dokki",
                    Street = "Side St",
                    BuildingNumber = "5"
                }
            },
            DocumentType = DocumentTypes.Invoice,
            DocumentTypeVersion = "1.0",
            DateTimeIssued = DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            TaxpayerActivityCode = "1234",
            InternalId = "INV-001",
            InvoiceLines = new List<InvoiceLine>
            {
                new InvoiceLine
                {
                    Description = "Consulting service",
                    ItemType = "EGS",
                    ItemCode = "EG-1234",
                    UnitType = "EA",
                    Quantity = 1,
                    UnitValue = new UnitValue { CurrencySold = "EGP", AmountEGP = 1000m },
                    SalesTotal = 1000m,
                    Total = 1140m,
                    TaxableItems = new List<TaxableItem>
                    {
                        new TaxableItem { TaxType = "T1", SubType = "V009", Amount = 140m, Rate = 14m }
                    }
                }
            },
            TotalSalesAmount = 1000m,
            NetAmount = 1000m,
            TaxTotals = new List<TaxTotal> { new TaxTotal { TaxType = "T1", Amount = 140m } },
            TotalAmount = 1140m
        };

        private class SequencedHandler : HttpMessageHandler
        {
            private readonly Queue<(string Body, HttpStatusCode Status)> _responses;
            public List<(string Method, string Url, string Body, string AuthHeader)> Requests { get; }
                = new List<(string, string, string, string)>();

            public SequencedHandler(IEnumerable<string> bodies, HttpStatusCode status = HttpStatusCode.OK)
                : this(bodies.Select(b => (b, status)))
            {
            }

            public SequencedHandler(IEnumerable<(string Body, HttpStatusCode Status)> responses)
            {
                _responses = new Queue<(string, HttpStatusCode)>(responses);
            }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var body = request.Content == null ? null : await request.Content.ReadAsStringAsync();
                string auth = null;
                if (request.Headers.Authorization != null)
                    auth = request.Headers.Authorization.Scheme + " " + request.Headers.Authorization.Parameter;
                Requests.Add((request.Method.Method, request.RequestUri.ToString(), body, auth));
                var (responseBody, status) = _responses.Dequeue();
                return new HttpResponseMessage(status)
                {
                    Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
                };
            }
        }

        private class StubSigner : IDocumentSigner
        {
            public string SeenCanonicalJson { get; private set; }
            public Task<string> SignAsync(string canonicalDocumentJson, CancellationToken cancellationToken = default)
            {
                SeenCanonicalJson = canonicalDocumentJson;
                return Task.FromResult("stub-signature-base64");
            }
        }

        [Test]
        public void MissingCredentials_Throw()
        {
            Assert.Throws<ArgumentException>(() => new EtaClient(new EtaClientOptions()));
        }

        [Test]
        public async Task AccessToken_IsCached()
        {
            var handler = new SequencedHandler(new[] { TokenJson });
            var client = new EtaClient(Options(), new HttpClient(handler));

            var first = await client.GetAccessTokenAsync();
            var second = await client.GetAccessTokenAsync();

            Assert.AreEqual("test-token", first);
            Assert.AreEqual(first, second);
            Assert.AreEqual(1, handler.Requests.Count); // token endpoint hit only once
            Assert.IsTrue(handler.Requests[0].Url.EndsWith("/connect/token"));
            StringAssert.Contains("grant_type=client_credentials", handler.Requests[0].Body);
            StringAssert.Contains("scope=InvoicingAPI", handler.Requests[0].Body);
        }

        [Test]
        public async Task SubmitDocuments_PostsCorrectShape_AndAttachesSignature()
        {
            var handler = new SequencedHandler(new[]
            {
                TokenJson,
                @"{ ""submissionUUID"": ""SUB123"",
                     ""acceptedDocuments"": [ { ""uuid"": ""DOC1"", ""longId"": ""LONG1"", ""internalId"": ""INV-001"" } ],
                     ""rejectedDocuments"": [] }"
            }, HttpStatusCode.Accepted);
            var client = new EtaClient(Options(), new HttpClient(handler));
            var signer = new StubSigner();

            var result = await client.SubmitDocumentsAsync(new[] { ValidDocument() }, signer);

            Assert.AreEqual("SUB123", result.SubmissionUuid);
            Assert.AreEqual(1, result.AcceptedDocuments.Count);
            Assert.AreEqual("DOC1", result.AcceptedDocuments[0].Uuid);
            Assert.AreEqual(0, result.RejectedDocuments.Count);

            Assert.AreEqual(2, handler.Requests.Count);
            var submit = handler.Requests[1];
            Assert.AreEqual("POST", submit.Method);
            StringAssert.Contains("/api/v1.0/documentsubmissions/", submit.Url);
            Assert.AreEqual("Bearer test-token", submit.AuthHeader);

            using (var doc = JsonDocument.Parse(submit.Body))
            {
                var documents = doc.RootElement.GetProperty("documents");
                Assert.AreEqual(1, documents.GetArrayLength());
                var first = documents[0];
                Assert.AreEqual("i", first.GetProperty("documentType").GetString());
                var signatures = first.GetProperty("signatures");
                Assert.AreEqual(1, signatures.GetArrayLength());
                Assert.AreEqual("I", signatures[0].GetProperty("type").GetString());
                Assert.AreEqual("stub-signature-base64", signatures[0].GetProperty("value").GetString());
            }

            // The signer received canonical JSON without the signatures section.
            Assert.IsNotNull(signer.SeenCanonicalJson);
            StringAssert.DoesNotContain("signatures", signer.SeenCanonicalJson);
            StringAssert.Contains(@"""documentType"":""i""", signer.SeenCanonicalJson);
        }

        [Test]
        public async Task SubmitDocuments_ParsesRejectedDocuments()
        {
            var handler = new SequencedHandler(new[]
            {
                TokenJson,
                @"{ ""submissionUUID"": ""SUB123"",
                     ""acceptedDocuments"": [],
                     ""rejectedDocuments"": [ { ""internalId"": ""INV-002"",
                        ""error"": { ""code"": ""BadStructure"", ""message"": ""Invalid line"", ""target"": ""invoiceLines"" } } ] }"
            }, HttpStatusCode.Accepted);
            var client = new EtaClient(Options(), new HttpClient(handler));

            var result = await client.SubmitDocumentsAsync(new[] { ValidDocument() });

            Assert.AreEqual(1, result.RejectedDocuments.Count);
            Assert.AreEqual("INV-002", result.RejectedDocuments[0].InternalId);
            Assert.AreEqual("BadStructure", result.RejectedDocuments[0].Error.Code);
        }

        [Test]
        public async Task GetDocumentDetails_ParsesValidationResults()
        {
            var handler = new SequencedHandler(new[]
            {
                TokenJson,
                @"{ ""submissionUUID"": ""SUB123"", ""status"": ""valid"",
                     ""validationResults"": { ""status"": ""Valid"",
                       ""validationSteps"": [ { ""name"": ""GS1 code validator"", ""status"": ""Valid"" } ] },
                     ""document"": { ""documentType"": ""i"" } }"
            });
            var client = new EtaClient(Options(), new HttpClient(handler));

            var details = await client.GetDocumentDetailsAsync("DOC1");

            Assert.AreEqual("valid", details.Status);
            Assert.AreEqual("Valid", details.ValidationResults.Status);
            Assert.AreEqual(1, details.ValidationResults.ValidationSteps.Count);
            Assert.AreEqual("i", details.Document.GetProperty("documentType").GetString());
            StringAssert.Contains("/api/v1.0/documents/DOC1/details", handler.Requests[1].Url);
        }

        [Test]
        public async Task GetSubmission_ParsesSummary()
        {
            var handler = new SequencedHandler(new[]
            {
                TokenJson,
                @"{ ""uuid"": ""SUB123"", ""documentCount"": 1, ""overallStatus"": ""valid"",
                     ""documentSummary"": [ { ""uuid"": ""DOC1"", ""internalId"": ""INV-001"",
                        ""typeName"": ""i"", ""status"": ""valid"", ""total"": 1140 } ],
                     ""documentSummaryMetadata"": { ""totalPages"": 1, ""totalCount"": 1 } }"
            });
            var client = new EtaClient(Options(), new HttpClient(handler));

            var info = await client.GetSubmissionAsync("SUB123");

            Assert.AreEqual("valid", info.OverallStatus);
            Assert.AreEqual(1, info.DocumentSummary.Count);
            Assert.AreEqual("DOC1", info.DocumentSummary[0].Uuid);
            StringAssert.Contains("pageNo=1", handler.Requests[1].Url);
        }

        [Test]
        public async Task CancelDocument_SendsPutWithCancelledStatus()
        {
            var handler = new SequencedHandler(new[] { TokenJson, @"{}" });
            var client = new EtaClient(Options(), new HttpClient(handler));

            await client.CancelDocumentAsync("DOC1", "Wrong buyer details");

            Assert.AreEqual(2, handler.Requests.Count);
            var req = handler.Requests[1];
            Assert.AreEqual("PUT", req.Method);
            StringAssert.Contains("/api/v1.0/documents/DOC1/state", req.Url);
            using (var doc = JsonDocument.Parse(req.Body))
            {
                Assert.AreEqual("cancelled", doc.RootElement.GetProperty("status").GetString());
                Assert.AreEqual("Wrong buyer details", doc.RootElement.GetProperty("reason").GetString());
            }
        }

        [Test]
        public async Task RejectDocument_SendsPutWithRejectedStatus()
        {
            var handler = new SequencedHandler(new[] { TokenJson, @"{}" });
            var client = new EtaClient(Options(), new HttpClient(handler));

            await client.RejectDocumentAsync("DOC1", "Received incorrect invoice");

            var req = handler.Requests[1];
            Assert.AreEqual("PUT", req.Method);
            using (var doc = JsonDocument.Parse(req.Body))
                Assert.AreEqual("rejected", doc.RootElement.GetProperty("status").GetString());
        }

        [Test]
        public async Task SearchDocuments_BuildsQueryString()
        {
            var handler = new SequencedHandler(new[]
            {
                TokenJson,
                @"{ ""result"": [], ""metadata"": { ""continuationToken"": ""EndofResultSet"" } }"
            });
            var client = new EtaClient(Options(), new HttpClient(handler));

            var result = await client.SearchDocumentsAsync(new SearchDocumentsQuery
            {
                SubmissionDateFrom = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                SubmissionDateTo = new DateTime(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc),
                DocumentType = DocumentTypes.Invoice,
                Status = "Valid"
            });

            Assert.AreEqual("EndofResultSet", result.Metadata.ContinuationToken);
            var url = handler.Requests[1].Url;
            StringAssert.Contains("/api/v1.0/documents/search?", url);
            StringAssert.Contains("submissionDateFrom=2026-10-01T00%3A00%3A00", url);
            StringAssert.Contains("documentType=i", url);
        }

        [Test]
        public void ApiError_ThrowsTypedException_WithErrorCode()
        {
            var handler = new SequencedHandler(
                new[]
                {
                    (TokenJson, HttpStatusCode.OK),
                    (@"{ ""code"": ""DuplicateSubmission"", ""message"": ""Already submitted"" }",
                        HttpStatusCode.UnprocessableEntity)
                });
            var client = new EtaClient(Options(), new HttpClient(handler));

            var ex = Assert.ThrowsAsync<EtaApiException>(() =>
                client.SubmitDocumentsAsync(new[] { ValidDocument() }));

            Assert.AreEqual(422, ex.StatusCode);
            Assert.AreEqual("DuplicateSubmission", ex.ErrorCode);
        }

        [Test]
        public void Validator_AcceptsValidDocument()
        {
            var errors = EtaDocumentValidator.Validate(ValidDocument());
            Assert.IsEmpty(errors);
        }

        [Test]
        public void Validator_RejectsBrokenDocument()
        {
            var doc = ValidDocument();
            doc.Issuer = null;
            doc.InvoiceLines.Clear();
            doc.TotalAmount = 0;

            var errors = EtaDocumentValidator.Validate(doc);

            CollectionAssert.IsNotEmpty(errors);
            StringAssert.Contains("issuer is required", string.Join(";", errors));
        }

        [Test]
        public void CanonicalJson_IsDeterministic_AndSorted()
        {
            var first = EtaCanonicalJson.ToCanonicalJson(ValidDocument());
            var second = EtaCanonicalJson.ToCanonicalJson(ValidDocument());

            Assert.AreEqual(first, second);
            // No whitespace between JSON tokens (values may still contain spaces).
            StringAssert.DoesNotContain("\": ", first);
            StringAssert.DoesNotContain("\", ", first);
            // issuer comes before receiver alphabetically
            Assert.Less(first.IndexOf("\"issuer\"", StringComparison.Ordinal),
                        first.IndexOf("\"receiver\"", StringComparison.Ordinal));
        }
    }
}
