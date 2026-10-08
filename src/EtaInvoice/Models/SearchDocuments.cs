using System;
using System.Collections.Generic;
using System.Text;

namespace EtaInvoice.Models
{
    /// <summary>
    /// Filters for the Search Documents API
    /// (<c>GET /api/v1.0/documents/search</c>).
    /// </summary>
    public class SearchDocumentsQuery
    {
        public DateTime? SubmissionDateFrom { get; set; }
        public DateTime? SubmissionDateTo { get; set; }
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
        public string ContinuationToken { get; set; }
        public int? PageSize { get; set; }

        /// <summary><c>Sent</c> or <c>Received</c>.</summary>
        public string Direction { get; set; }

        /// <summary><c>Valid</c>, <c>Invalid</c>, <c>Rejected</c>, <c>Cancelled</c>, <c>Submitted</c>.</summary>
        public string Status { get; set; }

        /// <summary>Document type code — see <see cref="DocumentTypes"/>.</summary>
        public string DocumentType { get; set; }

        /// <summary>Only with Direction = Sent.</summary>
        public string ReceiverType { get; set; }

        /// <summary>Only with Direction = Sent.</summary>
        public string ReceiverId { get; set; }

        /// <summary>Only with Direction = Received.</summary>
        public string IssuerType { get; set; }

        /// <summary>Only with Direction = Received.</summary>
        public string IssuerId { get; set; }

        public string Uuid { get; set; }
        public string InternalId { get; set; }

        internal string ToQueryString()
        {
            var sb = new StringBuilder();
            Append(sb, "submissionDateFrom", FormatDate(SubmissionDateFrom));
            Append(sb, "submissionDateTo", FormatDate(SubmissionDateTo));
            Append(sb, "issueDateFrom", FormatDate(IssueDateFrom));
            Append(sb, "issueDateTo", FormatDate(IssueDateTo));
            Append(sb, "continuationToken", ContinuationToken);
            Append(sb, "pageSize", PageSize?.ToString());
            Append(sb, "direction", Direction);
            Append(sb, "status", Status);
            Append(sb, "documentType", DocumentType);
            Append(sb, "receiverType", ReceiverType);
            Append(sb, "receiverId", ReceiverId);
            Append(sb, "issuerType", IssuerType);
            Append(sb, "issuerId", IssuerId);
            Append(sb, "uuid", Uuid);
            Append(sb, "internalID", InternalId);
            return sb.ToString();
        }

        private static void Append(StringBuilder sb, string name, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            if (sb.Length > 0) sb.Append('&');
            sb.Append(name).Append('=').Append(Uri.EscapeDataString(value));
        }

        private static string FormatDate(DateTime? value)
        {
            return value?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss");
        }
    }

    /// <summary>Result of the Search Documents API.</summary>
    public class SearchDocumentsResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("result")]
        public List<DocumentSummary> Result { get; set; } = new List<DocumentSummary>();

        [System.Text.Json.Serialization.JsonPropertyName("metadata")]
        public SearchMetadata Metadata { get; set; }
    }

    public class SearchMetadata
    {
        /// <summary>
        /// Token for the next page. <c>EndofResultSet</c> means all results returned.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("continuationToken")]
        public string ContinuationToken { get; set; }
    }
}
