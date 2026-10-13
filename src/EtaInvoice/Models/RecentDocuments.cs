using System;
using System.Text;

namespace EtaInvoice.Models
{
    /// <summary>
    /// Filters for the Recent Documents API
    /// (<c>GET /api/v1.0/documents/recent</c>).
    /// All parameters are optional.
    /// <para>
    /// Note: ETA recommends the <c>GET /api/v1.0/documents/search</c> API
    /// (see <see cref="SearchDocumentsQuery"/>) over this one — it has better
    /// performance, more filters, and no record-count limits.
    /// </para>
    /// </summary>
    public class RecentDocumentsQuery
    {
        /// <summary>Number of the page to retrieve.</summary>
        public int? PageNo { get; set; }

        /// <summary>Number of documents per page (cannot exceed the ETA-configured maximum).</summary>
        public int? PageSize { get; set; }

        /// <summary>Start of the submission date/time window (UTC). Mandatory when <see cref="SubmissionDateTo"/> is provided.</summary>
        public DateTime? SubmissionDateFrom { get; set; }

        /// <summary>End of the submission date/time window (UTC). Mandatory when <see cref="SubmissionDateFrom"/> is provided.</summary>
        public DateTime? SubmissionDateTo { get; set; }

        /// <summary>Start of the issuance date/time window (UTC). Mandatory when <see cref="IssueDateTo"/> is provided.</summary>
        public DateTime? IssueDateFrom { get; set; }

        /// <summary>End of the issuance date/time window (UTC). Mandatory when <see cref="IssueDateFrom"/> is provided.</summary>
        public DateTime? IssueDateTo { get; set; }

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

        internal string ToQueryString()
        {
            var sb = new StringBuilder();
            Append(sb, "pageNo", PageNo?.ToString());
            Append(sb, "pageSize", PageSize?.ToString());
            Append(sb, "submissionDateFrom", FormatDate(SubmissionDateFrom));
            Append(sb, "submissionDateTo", FormatDate(SubmissionDateTo));
            Append(sb, "issueDateFrom", FormatDate(IssueDateFrom));
            Append(sb, "issueDateTo", FormatDate(IssueDateTo));
            Append(sb, "direction", Direction);
            Append(sb, "status", Status);
            Append(sb, "documentType", DocumentType);
            Append(sb, "receiverType", ReceiverType);
            Append(sb, "receiverId", ReceiverId);
            Append(sb, "issuerType", IssuerType);
            Append(sb, "issuerId", IssuerId);
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

    /// <summary>Result of the Recent Documents API.</summary>
    public class RecentDocumentsResult
    {
        /// <summary>Document summaries for the requested page.</summary>
        [System.Text.Json.Serialization.JsonPropertyName("result")]
        public System.Collections.Generic.List<DocumentSummary> Result { get; set; }
            = new System.Collections.Generic.List<DocumentSummary>();

        /// <summary>Paging metadata (<c>totalPages</c>, <c>totalCount</c>).</summary>
        [System.Text.Json.Serialization.JsonPropertyName("metadata")]
        public PageMetadata Metadata { get; set; }
    }
}
