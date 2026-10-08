using System.Text.Json.Serialization;

namespace EtaInvoice.Models
{
    /// <summary>
    /// Document type codes accepted by the eInvoicing APIs
    /// (see the Search Documents API: the <c>documentType</c> filter).
    /// </summary>
    public static class DocumentTypes
    {
        /// <summary>Invoice (<c>i</c>).</summary>
        public const string Invoice = "i";

        /// <summary>Credit note (<c>c</c>).</summary>
        public const string CreditNote = "c";

        /// <summary>Debit note (<c>d</c>).</summary>
        public const string DebitNote = "d";

        /// <summary>Import invoice (<c>ii</c>).</summary>
        public const string ImportInvoice = "ii";

        /// <summary>Export invoice (<c>ei</c>).</summary>
        public const string ExportInvoice = "ei";

        /// <summary>Export credit note (<c>ec</c>).</summary>
        public const string ExportCreditNote = "ec";

        /// <summary>Export debit note (<c>ed</c>).</summary>
        public const string ExportDebitNote = "ed";
    }

    /// <summary>
    /// Document statuses reported by the eInvoicing APIs.
    /// </summary>
    public static class DocumentStatuses
    {
        public const string Submitted = "submitted";
        public const string Valid = "valid";
        public const string Invalid = "invalid";
        public const string Rejected = "rejected";
        public const string Cancelled = "cancelled";
    }

    /// <summary>
    /// Taxpayer type codes: business in Egypt, natural person, foreigner.
    /// </summary>
    public static class TaxpayerTypes
    {
        /// <summary>Business in Egypt.</summary>
        public const string Business = "B";

        /// <summary>Natural person.</summary>
        public const string Person = "P";

        /// <summary>Foreigner.</summary>
        public const string Foreigner = "F";
    }

    /// <summary>
    /// An e-invoice document (invoice v1.0 structure) as expected by
    /// <c>POST /api/v1.0/documentsubmissions/</c>.
    /// Field names follow the official ETA document structure
    /// (https://sdk.invoicing.eta.gov.eg/documents/invoice-v1-0/).
    /// </summary>
    public class EtaDocument
    {
        [JsonPropertyName("issuer")]
        public Taxpayer Issuer { get; set; }

        [JsonPropertyName("receiver")]
        public Taxpayer Receiver { get; set; }

        /// <summary>Document type code, e.g. <c>i</c> for invoice. See <see cref="DocumentTypes"/>.</summary>
        [JsonPropertyName("documentType")]
        public string DocumentType { get; set; } = DocumentTypes.Invoice;

        /// <summary>Document type version, e.g. <c>1.0</c>.</summary>
        [JsonPropertyName("documentTypeVersion")]
        public string DocumentTypeVersion { get; set; } = "1.0";

        /// <summary>Issuance date/time in UTC. Cannot be in the future.</summary>
        [JsonPropertyName("dateTimeIssued")]
        public string DateTimeIssued { get; set; }

        [JsonPropertyName("taxpayerActivityCode")]
        public string TaxpayerActivityCode { get; set; }

        /// <summary>Internal document id used to link back to the ERP document number.</summary>
        [JsonPropertyName("internalId")]
        public string InternalId { get; set; }

        [JsonPropertyName("purchaseOrderReference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PurchaseOrderReference { get; set; }

        [JsonPropertyName("purchaseOrderDescription")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PurchaseOrderDescription { get; set; }

        [JsonPropertyName("salesOrderReference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SalesOrderReference { get; set; }

        [JsonPropertyName("salesOrderDescription")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SalesOrderDescription { get; set; }

        [JsonPropertyName("proformaInvoiceNumber")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ProformaInvoiceNumber { get; set; }

        [JsonPropertyName("payment")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DocumentPayment Payment { get; set; }

        [JsonPropertyName("delivery")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DocumentDelivery Delivery { get; set; }

        /// <summary>At least one invoice line is required.</summary>
        [JsonPropertyName("invoiceLines")]
        public System.Collections.Generic.List<InvoiceLine> InvoiceLines { get; set; }
            = new System.Collections.Generic.List<InvoiceLine>();

        [JsonPropertyName("totalSalesAmount")]
        public decimal TotalSalesAmount { get; set; }

        [JsonPropertyName("totalDiscountAmount")]
        public decimal TotalDiscountAmount { get; set; }

        [JsonPropertyName("netAmount")]
        public decimal NetAmount { get; set; }

        [JsonPropertyName("taxTotals")]
        public System.Collections.Generic.List<TaxTotal> TaxTotals { get; set; }
            = new System.Collections.Generic.List<TaxTotal>();

        [JsonPropertyName("extraDiscountAmount")]
        public decimal ExtraDiscountAmount { get; set; }

        [JsonPropertyName("totalItemsDiscountAmount")]
        public decimal TotalItemsDiscountAmount { get; set; }

        /// <summary>Total = netAmount + sum of tax amounts. Up to 5 decimal digits.</summary>
        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Digital signatures. At least the issuer signature (<c>I</c>) must be
        /// present for production submission. See <see cref="Signing.IDocumentSigner"/>.
        /// Omitted from the payload when null (e.g. preprod testing without signing).
        /// </summary>
        [JsonPropertyName("signatures")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public System.Collections.Generic.List<DocumentSignature> Signatures { get; set; }

        [JsonPropertyName("serviceDeliveryDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ServiceDeliveryDate { get; set; }
    }

    /// <summary>Issuer or receiver taxpayer block.</summary>
    public class Taxpayer
    {
        /// <summary><c>B</c> business, <c>P</c> natural person, <c>F</c> foreigner.</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>Registration number (business), national id (person) or VAT id (foreigner).</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public TaxpayerAddress Address { get; set; }
    }

    /// <summary>
    /// Address block. Issuer addresses additionally require <c>branchId</c>
    /// when the issuer is of type <c>B</c>.
    /// </summary>
    public class TaxpayerAddress
    {
        /// <summary>Mandatory for issuer addresses of type B.</summary>
        [JsonPropertyName("branchId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string BranchId { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("governate")]
        public string Governate { get; set; }

        [JsonPropertyName("regionCity")]
        public string RegionCity { get; set; }

        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonPropertyName("buildingNumber")]
        public string BuildingNumber { get; set; }

        [JsonPropertyName("postalCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PostalCode { get; set; }

        [JsonPropertyName("floor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Floor { get; set; }

        [JsonPropertyName("room")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Room { get; set; }

        [JsonPropertyName("landmark")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Landmark { get; set; }

        [JsonPropertyName("additionalInformation")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string AdditionalInformation { get; set; }
    }

    public class DocumentPayment
    {
        [JsonPropertyName("bankName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string BankName { get; set; }

        [JsonPropertyName("bankAddress")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string BankAddress { get; set; }

        [JsonPropertyName("bankAccountNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string BankAccountNo { get; set; }

        [JsonPropertyName("bankAccountIBAN")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string BankAccountIBAN { get; set; }

        [JsonPropertyName("swiftCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SwiftCode { get; set; }

        [JsonPropertyName("terms")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Terms { get; set; }
    }

    public class DocumentDelivery
    {
        [JsonPropertyName("approach")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Approach { get; set; }

        [JsonPropertyName("packaging")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Packaging { get; set; }

        [JsonPropertyName("dateValidity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DateValidity { get; set; }

        [JsonPropertyName("exportPort")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ExportPort { get; set; }

        [JsonPropertyName("countryOfOrigin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string CountryOfOrigin { get; set; }

        [JsonPropertyName("grossWeight")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? GrossWeight { get; set; }

        [JsonPropertyName("netWeight")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? NetWeight { get; set; }

        [JsonPropertyName("terms")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Terms { get; set; }
    }

    public class InvoiceLine
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>Coding schema: <c>GS1</c> or <c>EGS</c>.</summary>
        [JsonPropertyName("itemType")]
        public string ItemType { get; set; }

        [JsonPropertyName("itemCode")]
        public string ItemCode { get; set; }

        [JsonPropertyName("unitType")]
        public string UnitType { get; set; }

        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        [JsonPropertyName("unitValue")]
        public UnitValue UnitValue { get; set; }

        [JsonPropertyName("salesTotal")]
        public decimal SalesTotal { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("valueDifference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? ValueDifference { get; set; }

        [JsonPropertyName("totalTaxableFees")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? TotalTaxableFees { get; set; }

        [JsonPropertyName("netTotal")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? NetTotal { get; set; }

        [JsonPropertyName("itemsDiscount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? ItemsDiscount { get; set; }

        [JsonPropertyName("discount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public LineDiscount Discount { get; set; }

        [JsonPropertyName("taxableItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public System.Collections.Generic.List<TaxableItem> TaxableItems { get; set; }

        [JsonPropertyName("internalCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string InternalCode { get; set; }
    }

    public class UnitValue
    {
        /// <summary>ISO 4217 currency code.</summary>
        [JsonPropertyName("currencySold")]
        public string CurrencySold { get; set; }

        /// <summary>Unit price in EGP (max 5 decimal digits).</summary>
        [JsonPropertyName("amountEGP")]
        public decimal AmountEGP { get; set; }

        /// <summary>Mandatory when <c>currencySold</c> is not EGP.</summary>
        [JsonPropertyName("amountSold")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? AmountSold { get; set; }

        /// <summary>Mandatory when <c>currencySold</c> is not EGP.</summary>
        [JsonPropertyName("currencyExchangeRate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? CurrencyExchangeRate { get; set; }
    }

    public class LineDiscount
    {
        [JsonPropertyName("rate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Rate { get; set; }

        [JsonPropertyName("amount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Amount { get; set; }
    }

    public class TaxableItem
    {
        /// <summary>Tax type code, e.g. <c>T1</c> (VAT). Must be unique per invoice line.</summary>
        [JsonPropertyName("taxType")]
        public string TaxType { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        /// <summary>Tax subtype, e.g. <c>V009</c>.</summary>
        [JsonPropertyName("subType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SubType { get; set; }

        [JsonPropertyName("rate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Rate { get; set; }
    }

    public class TaxTotal
    {
        [JsonPropertyName("taxType")]
        public string TaxType { get; set; }

        /// <summary>Sum of all amounts of this tax across all invoice lines.</summary>
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    public class DocumentSignature
    {
        /// <summary>Signature type: <c>I</c> issuer, <c>S</c> service provider.</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>Base64-encoded CAdES-BES signature value.</summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
