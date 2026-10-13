using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EtaInvoice.Models
{
    /// <summary>
    /// Full details of a document: <c>GET /api/v1.0/documents/{uuid}/details</c>,
    /// including validation results and tax-authority metadata.
    /// </summary>
    public class DocumentDetails
    {
        /// <summary>Unique id of the submission the document was part of.</summary>
        [JsonPropertyName("submissionUUID")]
        public string SubmissionUuid { get; set; }

        /// <summary>Long temporary id usable for anonymous queries.</summary>
        [JsonPropertyName("longId")]
        public string LongId { get; set; }

        /// <summary>Date and time the document was submitted.</summary>
        [JsonPropertyName("dateTimeReceived")]
        public string DateTimeReceived { get; set; }

        /// <summary><c>submitted</c>, <c>valid</c>, <c>invalid</c>, <c>rejected</c>, <c>cancelled</c>.</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary><c>original</c> or <c>transformed</c>.</summary>
        [JsonPropertyName("transformationStatus")]
        public string TransformationStatus { get; set; }

        /// <summary>Full validation results of the document.</summary>
        [JsonPropertyName("validationResults")]
        public DocumentValidationResults ValidationResults { get; set; }

        /// <summary>
        /// The submitted document. Kept as a <see cref="JsonElement"/> because its
        /// shape depends on the document type and version.
        /// </summary>
        [JsonPropertyName("document")]
        public JsonElement Document { get; set; }

        /// <summary>Cancellation request date initiated by the issuer, in UTC (when present).</summary>
        [JsonPropertyName("cancelRequestDate")]
        public string CancelRequestDate { get; set; }

        /// <summary>Rejection request date initiated by the receiver, in UTC (when present).</summary>
        [JsonPropertyName("rejectRequestDate")]
        public string RejectRequestDate { get; set; }

        /// <summary>
        /// Date when the document will be marked cancelled if the receiver does
        /// not decline the cancellation request, in UTC (when present).
        /// </summary>
        [JsonPropertyName("cancelRequestDelayedDate")]
        public string CancelRequestDelayedDate { get; set; }

        /// <summary>
        /// Date when the document will be marked rejected if the issuer does
        /// not decline the rejection request, in UTC (when present).
        /// </summary>
        [JsonPropertyName("rejectRequestDelayedDate")]
        public string RejectRequestDelayedDate { get; set; }

        /// <summary>Decline cancellation request date initiated by the receiver, in UTC (when present).</summary>
        [JsonPropertyName("declineCancelRequestDate")]
        public string DeclineCancelRequestDate { get; set; }

        /// <summary>Decline rejection request date initiated by the issuer, in UTC (when present).</summary>
        [JsonPropertyName("declineRejectRequestDate")]
        public string DeclineRejectRequestDate { get; set; }

        /// <summary>Freeze status information.</summary>
        [JsonPropertyName("freezeStatus")]
        public FreezeStatus FreezeStatus { get; set; }

        /// <summary>Tax-authority metadata fields attached to the document.</summary>
        [JsonPropertyName("additionalMetadata")]
        public List<DocumentMetadata> AdditionalMetadata { get; set; }

        /// <summary>Late submission request identifier, when the document was submitted late.</summary>
        [JsonPropertyName("lateSubmissionRequestNumber")]
        public string LateSubmissionRequestNumber { get; set; }
    }

    /// <summary>Validation outcome of a document.</summary>
    public class DocumentValidationResults
    {
        /// <summary><c>In progress</c>, <c>Valid</c>, <c>Invalid</c>.</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>Per-step validation results.</summary>
        [JsonPropertyName("validationSteps")]
        public List<ValidationStepResult> ValidationSteps { get; set; } = new List<ValidationStepResult>();
    }

    /// <summary>Result of a single validation step.</summary>
    public class ValidationStepResult
    {
        /// <summary>Name of the validation step, e.g. <c>GS1 code validator</c>.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>Step outcome, e.g. <c>Valid</c>.</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>Step error details, when the step failed.</summary>
        [JsonPropertyName("error")]
        public EtaError Error { get; set; }
    }

    /// <summary>Freeze status information of a document.</summary>
    public class FreezeStatus
    {
        /// <summary>Whether the document is currently frozen.</summary>
        [JsonPropertyName("frozen")]
        public bool Frozen { get; set; }

        /// <summary>Freeze type: 0/null = unfrozen, 1 = temporary freeze, 2 = permanent.</summary>
        [JsonPropertyName("type")]
        public int? Type { get; set; }

        /// <summary>Freeze scope: 0/null = not set, 1 = full, 2 = cancellation.</summary>
        [JsonPropertyName("scope")]
        public int? Scope { get; set; }

        /// <summary>Date of the last freeze/unfreeze action, in UTC.</summary>
        [JsonPropertyName("actionDate")]
        public string ActionDate { get; set; }

        /// <summary>Accounting unit code.</summary>
        [JsonPropertyName("auCode")]
        public string AuCode { get; set; }

        /// <summary>Accounting unit name.</summary>
        [JsonPropertyName("auName")]
        public string AuName { get; set; }
    }

    /// <summary>A tax-authority metadata field attached to a document.</summary>
    public class DocumentMetadata
    {
        /// <summary>Metadata field name, e.g. <c>PaymentNo</c>.</summary>
        [JsonPropertyName("fieldName")]
        public string FieldName { get; set; }

        /// <summary>Metadata field value.</summary>
        [JsonPropertyName("fieldValue")]
        public string FieldValue { get; set; }

        /// <summary>Metadata field type: <c>Text</c>, <c>Boolean</c>, <c>DateTime</c>, <c>Number</c>.</summary>
        [JsonPropertyName("fieldType")]
        public string FieldType { get; set; }

        /// <summary>Field name description in English.</summary>
        [JsonPropertyName("fieldNameDescEn")]
        public string FieldNameDescEn { get; set; }

        /// <summary>Field name description in Arabic.</summary>
        [JsonPropertyName("fieldNameDescAr")]
        public string FieldNameDescAr { get; set; }
    }
}
