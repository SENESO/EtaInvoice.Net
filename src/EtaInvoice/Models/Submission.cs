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

        /// <summary>Documents accepted for processing by the API.</summary>
        [JsonPropertyName("acceptedDocuments")]
        public List<AcceptedDocument> AcceptedDocuments { get; set; } = new List<AcceptedDocument>();

        /// <summary>Documents rejected outright by the API (with per-document errors).</summary>
        [JsonPropertyName("rejectedDocuments")]
        public List<RejectedDocument> RejectedDocuments { get; set; } = new List<RejectedDocument>();
    }

    /// <summary>A document accepted for processing.</summary>
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

    /// <summary>A document rejected outright by the API.</summary>
    public class RejectedDocument
    {
        /// <summary>The internal id supplied in the submitted document.</summary>
        [JsonPropertyName("internalId")]
        public string InternalId { get; set; }

        /// <summary>Why the document was rejected.</summary>
        [JsonPropertyName("error")]
        public EtaError Error { get; set; }
    }

    /// <summary>Standard ETA error structure.</summary>
    public class EtaError
    {
        /// <summary>Machine-readable error code, e.g. <c>DuplicateSubmission</c>.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>Human-readable error message.</summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>Field the error relates to, when applicable.</summary>
        [JsonPropertyName("target")]
        public string Target { get; set; }

        /// <summary>Nested error details, when applicable.</summary>
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

        /// <summary>Date and time the submission was received (UTC).</summary>
        [JsonPropertyName("dateTimeReceived")]
        public string DateTimeReceived { get; set; }

        /// <summary>
        /// Overall status of the batch processing:
        /// <c>in progress</c>, <c>valid</c>, <c>partially valid</c>, <c>invalid</c>.
        /// </summary>
        [JsonPropertyName("overallStatus")]
        public string OverallStatus { get; set; }

        /// <summary>Document summaries for the requested page.</summary>
        [JsonPropertyName("documentSummary")]
        public List<DocumentSummary> DocumentSummary { get; set; } = new List<DocumentSummary>();

        /// <summary>Paging metadata for the document summary list.</summary>
        [JsonPropertyName("documentSummaryMetadata")]
        public PageMetadata DocumentSummaryMetadata { get; set; }
    }

    /// <summary>A document summary as returned by list APIs.</summary>
    public class DocumentSummary
    {
        /// <summary>Unique document id in eInvoicing.</summary>
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        /// <summary>Unique id of the submission the document was part of.</summary>
        [JsonPropertyName("submissionUUID")]
        public string SubmissionUuid { get; set; }

        /// <summary>Long temporary id usable for anonymous queries.</summary>
        [JsonPropertyName("longId")]
        public string LongId { get; set; }

        /// <summary>Public URL which can be used to access the document anonymously.</summary>
        [JsonPropertyName("publicUrl")]
        public string PublicUrl { get; set; }

        /// <summary>Internal id supplied in the submitted document.</summary>
        [JsonPropertyName("internalId")]
        public string InternalId { get; set; }

        /// <summary>Document type code: <c>i</c>, <c>c</c>, <c>d</c>, <c>ii</c>, <c>ei</c>, <c>ec</c>, <c>ed</c>.</summary>
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; }

        /// <summary>Document type version, e.g. <c>1.0</c>.</summary>
        [JsonPropertyName("typeVersionName")]
        public string TypeVersionName { get; set; }

        /// <summary>Issuer registration number.</summary>
        [JsonPropertyName("issuerId")]
        public string IssuerId { get; set; }

        /// <summary>Issuer company name.</summary>
        [JsonPropertyName("issuerName")]
        public string IssuerName { get; set; }

        /// <summary>Issuer type: <c>B</c> business, <c>F</c> foreigner.</summary>
        [JsonPropertyName("issuerType")]
        public string IssuerType { get; set; }

        /// <summary>Receiver registration number (when present).</summary>
        [JsonPropertyName("receiverId")]
        public string ReceiverId { get; set; }

        /// <summary>Receiver name (when present).</summary>
        [JsonPropertyName("receiverName")]
        public string ReceiverName { get; set; }

        /// <summary>Receiver type: <c>B</c> business, <c>P</c> person, <c>F</c> foreigner.</summary>
        [JsonPropertyName("receiverType")]
        public string ReceiverType { get; set; }

        /// <summary>Date and time the document was issued.</summary>
        [JsonPropertyName("dateTimeIssued")]
        public string DateTimeIssued { get; set; }

        /// <summary>Date and time the document was submitted to the API.</summary>
        [JsonPropertyName("dateTimeReceived")]
        public string DateTimeReceived { get; set; }

        /// <summary>Total sales amount in EGP.</summary>
        [JsonPropertyName("totalSales")]
        public decimal TotalSales { get; set; }

        /// <summary>Total discount amount in EGP.</summary>
        [JsonPropertyName("totalDiscount")]
        public decimal TotalDiscount { get; set; }

        /// <summary>Total net amount in EGP.</summary>
        [JsonPropertyName("netAmount")]
        public decimal NetAmount { get; set; }

        /// <summary>Total amount of the document in EGP.</summary>
        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        /// <summary><c>Submitted</c>, <c>Valid</c>, <c>Invalid</c>, <c>Rejected</c>, <c>Cancelled</c>.</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>Cancellation request date initiated by the issuer, in UTC (when present).</summary>
        [JsonPropertyName("cancelRequestDate")]
        public string CancelRequestDate { get; set; }

        /// <summary>Rejection request date initiated by the receiver, in UTC (when present).</summary>
        [JsonPropertyName("rejectRequestDate")]
        public string RejectRequestDate { get; set; }

        /// <summary>Optional: reason of the cancellation or rejection.</summary>
        [JsonPropertyName("documentStatusReason")]
        public string DocumentStatusReason { get; set; }

        /// <summary>User who created the document (ERP id or email).</summary>
        [JsonPropertyName("createdByUserId")]
        public string CreatedByUserId { get; set; }

        /// <summary>Freeze status information.</summary>
        [JsonPropertyName("freezeStatus")]
        public FreezeStatus FreezeStatus { get; set; }

        /// <summary>Late submission request identifier, when the document was submitted late.</summary>
        [JsonPropertyName("lateSubmissionRequestNumber")]
        public string LateSubmissionRequestNumber { get; set; }
    }

    /// <summary>Paging metadata returned by list APIs.</summary>
    public class PageMetadata
    {
        /// <summary>Total count of pages based on the supplied (or default) page size.</summary>
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        /// <summary>Total count of matching objects.</summary>
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Indicates whether the supplied filters return more records than the
        /// maximum number of documents the API can return. Returned by the
        /// Recent Documents API; when false, use more targeted filters.
        /// </summary>
        [JsonPropertyName("queryContainsCompleteResultSet")]
        public bool? QueryContainsCompleteResultSet { get; set; }

        /// <summary>
        /// Number of matching records beyond the maximum the API can return.
        /// Returned by the Recent Documents API; <c>0</c> means the filters
        /// cover all matching records.
        /// </summary>
        [JsonPropertyName("remainingRecordsCount")]
        public int? RemainingRecordsCount { get; set; }
    }
}
