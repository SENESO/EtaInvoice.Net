using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EtaInvoice.Models
{
    /// <summary>
    /// Result of <c>POST /api/v1.0/documentsubmissions/</c> (HTTP 202).
    /// </summary>
    public class SubmissionResult
    {
        /// <summary>Unique id of the submission (26 latin alphanumeric symbols).</summary>
        [JsonPropertyName("submissionUUID")]
        public string SubmissionUuid { get; set; }

        [JsonPropertyName("acceptedDocuments")]
        public List<AcceptedDocument> AcceptedDocuments { get; set; } = new List<AcceptedDocument>();

        [JsonPropertyName("rejectedDocuments")]
        public List<RejectedDocument> RejectedDocuments { get; set; } = new List<RejectedDocument>();
    }

    public class AcceptedDocument
    {
        /// <summary>Unique document id assigned by eInvoicing.</summary>
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        /// <summary>Long temporary id usable for anonymous queries.</summary>
        [JsonPropertyName("longId")]
        public string LongId { get; set; }

        /// <summary>The internal id supplied in the submitted document.</summary>
        [JsonPropertyName("internalId")]
        public string InternalId { get; set; }
    }

    public class RejectedDocument
    {
        [JsonPropertyName("internalId")]
        public string InternalId { get; set; }

        [JsonPropertyName("error")]
        public EtaError Error { get; set; }
    }

    /// <summary>Standard ETA error structure.</summary>
    public class EtaError
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonPropertyName("details")]
        public List<EtaError> Details { get; set; }
    }

    /// <summary>
    /// Details of a submission: <c>GET /api/v1.0/documentsubmissions/{uuid}</c>.
    /// The document list is paged by the API.
    /// </summary>
    public class SubmissionInfo
    {
        /// <summary>Unique document submission id in eInvoicing.</summary>
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        /// <summary>Total count of documents in the submission accepted for processing.</summary>
        [JsonPropertyName("documentCount")]
        public int DocumentCount { get; set; }

        [JsonPropertyName("dateTimeReceived")]
        public string DateTimeReceived { get; set; }

        /// <summary>
        /// Overall status of the batch processing:
        /// <c>in progress</c>, <c>valid</c>, <c>partially valid</c>, <c>invalid</c>.
        /// </summary>
        [JsonPropertyName("overallStatus")]
        public string OverallStatus { get; set; }

        [JsonPropertyName("documentSummary")]
        public List<DocumentSummary> DocumentSummary { get; set; } = new List<DocumentSummary>();

        [JsonPropertyName("documentSummaryMetadata")]
        public PageMetadata DocumentSummaryMetadata { get; set; }
    }

    public class DocumentSummary
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("submissionUUID")]
        public string SubmissionUuid { get; set; }

        [JsonPropertyName("longId")]
        public string LongId { get; set; }

        [JsonPropertyName("publicUrl")]
        public string PublicUrl { get; set; }

        [JsonPropertyName("internalId")]
        public string InternalId { get; set; }

        /// <summary>Document type code: <c>i</c>, <c>c</c>, <c>d</c>, <c>ii</c>, <c>ei</c>, <c>ec</c>, <c>ed</c>.</summary>
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; }

        [JsonPropertyName("typeVersionName")]
        public string TypeVersionName { get; set; }

        [JsonPropertyName("issuerId")]
        public string IssuerId { get; set; }

        [JsonPropertyName("issuerName")]
        public string IssuerName { get; set; }

        [JsonPropertyName("issuerType")]
        public string IssuerType { get; set; }

        [JsonPropertyName("receiverId")]
        public string ReceiverId { get; set; }

        [JsonPropertyName("receiverName")]
        public string ReceiverName { get; set; }

        [JsonPropertyName("receiverType")]
        public string ReceiverType { get; set; }

        [JsonPropertyName("dateTimeIssued")]
        public string DateTimeIssued { get; set; }

        [JsonPropertyName("dateTimeReceived")]
        public string DateTimeReceived { get; set; }

        [JsonPropertyName("totalSales")]
        public decimal TotalSales { get; set; }

        [JsonPropertyName("totalDiscount")]
        public decimal TotalDiscount { get; set; }

        [JsonPropertyName("netAmount")]
        public decimal NetAmount { get; set; }

        /// <summary>Total amount of the document in EGP.</summary>
        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        /// <summary><c>Submitted</c>, <c>Valid</c>, <c>Invalid</c>, <c>Rejected</c>, <c>Cancelled</c>.</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("cancelRequestDate")]
        public string CancelRequestDate { get; set; }

        [JsonPropertyName("rejectRequestDate")]
        public string RejectRequestDate { get; set; }

        [JsonPropertyName("documentStatusReason")]
        public string DocumentStatusReason { get; set; }

        [JsonPropertyName("createdByUserId")]
        public string CreatedByUserId { get; set; }

        [JsonPropertyName("freezeStatus")]
        public FreezeStatus FreezeStatus { get; set; }

        [JsonPropertyName("lateSubmissionRequestNumber")]
        public string LateSubmissionRequestNumber { get; set; }
    }

    public class PageMetadata
    {
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }
}
