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

        [JsonPropertyName("longId")]
        public string LongId { get; set; }

        [JsonPropertyName("dateTimeReceived")]
        public string DateTimeReceived { get; set; }

        /// <summary><c>submitted</c>, <c>valid</c>, <c>invalid</c>, <c>rejected</c>, <c>cancelled</c>.</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary><c>original</c> or <c>transformed</c>.</summary>
        [JsonPropertyName("transformationStatus")]
        public string TransformationStatus { get; set; }

        [JsonPropertyName("validationResults")]
        public DocumentValidationResults ValidationResults { get; set; }

        /// <summary>
        /// The submitted document. Kept as a <see cref="JsonElement"/> because its
        /// shape depends on the document type and version.
        /// </summary>
        [JsonPropertyName("document")]
        public JsonElement Document { get; set; }

        [JsonPropertyName("cancelRequestDate")]
        public string CancelRequestDate { get; set; }

        [JsonPropertyName("rejectRequestDate")]
        public string RejectRequestDate { get; set; }

        [JsonPropertyName("cancelRequestDelayedDate")]
        public string CancelRequestDelayedDate { get; set; }

        [JsonPropertyName("rejectRequestDelayedDate")]
        public string RejectRequestDelayedDate { get; set; }

        [JsonPropertyName("declineCancelRequestDate")]
        public string DeclineCancelRequestDate { get; set; }

        [JsonPropertyName("declineRejectRequestDate")]
        public string DeclineRejectRequestDate { get; set; }

        [JsonPropertyName("freezeStatus")]
        public FreezeStatus FreezeStatus { get; set; }

        [JsonPropertyName("additionalMetadata")]
        public List<DocumentMetadata> AdditionalMetadata { get; set; }

        [JsonPropertyName("lateSubmissionRequestNumber")]
        public string LateSubmissionRequestNumber { get; set; }
    }

    public class DocumentValidationResults
    {
        /// <summary><c>In progress</c>, <c>Valid</c>, <c>Invalid</c>.</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("validationSteps")]
        public List<ValidationStepResult> ValidationSteps { get; set; } = new List<ValidationStepResult>();
    }

    public class ValidationStepResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("error")]
        public EtaError Error { get; set; }
    }

    public class FreezeStatus
    {
        [JsonPropertyName("frozen")]
        public bool Frozen { get; set; }

        [JsonPropertyName("type")]
        public int? Type { get; set; }

        [JsonPropertyName("scope")]
        public int? Scope { get; set; }

        [JsonPropertyName("actionDate")]
        public string ActionDate { get; set; }

        [JsonPropertyName("auCode")]
        public string AuCode { get; set; }

        [JsonPropertyName("auName")]
        public string AuName { get; set; }
    }

    public class DocumentMetadata
    {
        [JsonPropertyName("fieldName")]
        public string FieldName { get; set; }

        [JsonPropertyName("fieldValue")]
        public string FieldValue { get; set; }

        [JsonPropertyName("fieldType")]
        public string FieldType { get; set; }

        [JsonPropertyName("fieldNameDescEn")]
        public string FieldNameDescEn { get; set; }

        [JsonPropertyName("fieldNameDescAr")]
        public string FieldNameDescAr { get; set; }
    }
}
