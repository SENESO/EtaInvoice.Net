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
        /// <summary>Submitted, awaiting validation.</summary>
        public const string Submitted = "submitted";

        /// <summary>Validated successfully.</summary>
        public const string Valid = "valid";

        /// <summary>Failed validation.</summary>
        public const string Invalid = "invalid";

        /// <summary>Rejected by the receiver.</summary>
        public const string Rejected = "rejected";

        /// <summary>Cancelled by the issuer.</summary>
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
        /// <summary>Document issuer.</summary>
        [JsonPropertyName("issuer")]
        public Taxpayer Issuer { get; set; }

        /// <summary>Document receiver.</summary>
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

        /// <summary>Taxpayer activity code of the issuer.</summary>
        [JsonPropertyName("taxpayerActivityCode")]
        public string TaxpayerActivityCode { get; set; }

        /// <summary>Internal document id used to link back to the ERP document number.</summary>
        [JsonPropertyName("internalId")]
        public string InternalId { get; set; }

        /// <summary>Purchase order reference (optional).</summary>
        [JsonPropertyName("purchaseOrderReference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PurchaseOrderReference { get; set; }

        /// <summary>Purchase order description (optional).</summary>
        [JsonPropertyName("purchaseOrderDescription")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PurchaseOrderDescription { get; set; }

        /// <summary>Sales order reference (optional).</summary>
        [JsonPropertyName("salesOrderReference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SalesOrderReference { get; set; }

        /// <summary>Sales order description (optional).</summary>
        [JsonPropertyName("salesOrderDescription")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SalesOrderDescription { get; set; }

        /// <summary>Proforma invoice number (optional).</summary>
        [JsonPropertyName("proformaInvoiceNumber")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ProformaInvoiceNumber { get; set; }

        /// <summary>Payment details (optional).</summary>
        [JsonPropertyName("payment")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DocumentPayment Payment { get; set; }

        /// <summary>Delivery details (optional).</summary>
        [JsonPropertyName("delivery")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DocumentDelivery Delivery { get; set; }

        /// <summary>At least one invoice line is required.</summary>
        [JsonPropertyName("invoiceLines")]
        public System.Collections.Generic.List<InvoiceLine> InvoiceLines { get; set; }
            = new System.Collections.Generic.List<InvoiceLine>();

        /// <summary>Total sales amount of all invoice lines.</summary>
        [JsonPropertyName("totalSalesAmount")]
        public decimal TotalSalesAmount { get; set; }

        /// <summary>Total discount amount of all invoice lines.</summary>
        [JsonPropertyName("totalDiscountAmount")]
        public decimal TotalDiscountAmount { get; set; }

        /// <summary>Net amount (total sales − total discount).</summary>
        [JsonPropertyName("netAmount")]
        public decimal NetAmount { get; set; }

        /// <summary>Tax totals, one entry per tax type (sum across all lines).</summary>
        [JsonPropertyName("taxTotals")]
        public System.Collections.Generic.List<TaxTotal> TaxTotals { get; set; }
            = new System.Collections.Generic.List<TaxTotal>();

        /// <summary>Extra discount amount applied at document level.</summary>
        [JsonPropertyName("extraDiscountAmount")]
        public decimal ExtraDiscountAmount { get; set; }

        /// <summary>Total of item-level discounts.</summary>
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

        /// <summary>Service delivery date (optional).</summary>
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

        /// <summary>Taxpayer name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>Taxpayer address.</summary>
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

        /// <summary>Country code, e.g. <c>EG</c>.</summary>
        [JsonPropertyName("country")]
        public string Country { get; set; }

        /// <summary>Governate.</summary>
        [JsonPropertyName("governate")]
        public string Governate { get; set; }

        /// <summary>Region or city.</summary>
        [JsonPropertyName("regionCity")]
        public string RegionCity { get; set; }

        /// <summary>Street name.</summary>
        [JsonPropertyName("street")]
        public string Street { get; set; }

        /// <summary>Building number.</summary>
        [JsonPropertyName("buildingNumber")]
        public string BuildingNumber { get; set; }

        /// <summary>Postal code (optional).</summary>
        [JsonPropertyName("postalCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PostalCode { get; set; }

        /// <summary>Floor (optional).</summary>
        [JsonPropertyName("floor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Floor { get; set; }

        /// <summary>Room (optional).</summary>
        [JsonPropertyName("room")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Room { get; set; }

        /// <summary>Landmark (optional).</summary>
        [JsonPropertyName("landmark")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Landmark { get; set; }

        /// <summary>Additional address information (optional).</summary>
        [JsonPropertyName("additionalInformation")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string AdditionalInformation { get; set; }
    }

    /// <summary>Payment details block (optional).</summary>
    public class DocumentPayment
    {
        /// <summary>Bank name.</summary>
        [JsonPropertyName("bankName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string BankName { get; set; }

        /// <summary>Bank address.</summary>
        [JsonPropertyName("bankAddress")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string BankAddress { get; set; }

        /// <summary>Bank account number.</summary>
        [JsonPropertyName("bankAccountNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string BankAccountNo { get; set; }

        /// <summary>Bank account IBAN.</summary>
        [JsonPropertyName("bankAccountIBAN")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string BankAccountIBAN { get; set; }

        /// <summary>SWIFT code.</summary>
        [JsonPropertyName("swiftCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SwiftCode { get; set; }

        /// <summary>Payment terms.</summary>
        [JsonPropertyName("terms")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Terms { get; set; }
    }

    /// <summary>Delivery details block (optional).</summary>
    public class DocumentDelivery
    {
        /// <summary>Delivery approach.</summary>
        [JsonPropertyName("approach")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Approach { get; set; }

        /// <summary>Packaging description.</summary>
        [JsonPropertyName("packaging")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Packaging { get; set; }

        /// <summary>Delivery date validity.</summary>
        [JsonPropertyName("dateValidity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DateValidity { get; set; }

        /// <summary>Export port.</summary>
        [JsonPropertyName("exportPort")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ExportPort { get; set; }

        /// <summary>Country of origin.</summary>
        [JsonPropertyName("countryOfOrigin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string CountryOfOrigin { get; set; }

        /// <summary>Gross weight.</summary>
        [JsonPropertyName("grossWeight")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? GrossWeight { get; set; }

        /// <summary>Net weight.</summary>
        [JsonPropertyName("netWeight")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? NetWeight { get; set; }

        /// <summary>Delivery terms.</summary>
        [JsonPropertyName("terms")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Terms { get; set; }
    }

    /// <summary>A single invoice line.</summary>
    public class InvoiceLine
    {
        /// <summary>Line description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>Coding schema: <c>GS1</c> or <c>EGS</c>.</summary>
        [JsonPropertyName("itemType")]
        public string ItemType { get; set; }

        /// <summary>Item code in the coding schema.</summary>
        [JsonPropertyName("itemCode")]
        public string ItemCode { get; set; }

        /// <summary>Unit type, e.g. <c>EA</c>.</summary>
        [JsonPropertyName("unitType")]
        public string UnitType { get; set; }

        /// <summary>Quantity (must be greater than zero).</summary>
        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        /// <summary>Unit value (price).</summary>
        [JsonPropertyName("unitValue")]
        public UnitValue UnitValue { get; set; }

        /// <summary>Sales total of the line (quantity × unit value).</summary>
        [JsonPropertyName("salesTotal")]
        public decimal SalesTotal { get; set; }

        /// <summary>Total of the line including taxes.</summary>
        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        /// <summary>Value difference (optional).</summary>
        [JsonPropertyName("valueDifference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? ValueDifference { get; set; }

        /// <summary>Total taxable fees (optional).</summary>
        [JsonPropertyName("totalTaxableFees")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? TotalTaxableFees { get; set; }

        /// <summary>Net total of the line (optional).</summary>
        [JsonPropertyName("netTotal")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? NetTotal { get; set; }

        /// <summary>Items discount amount (optional).</summary>
        [JsonPropertyName("itemsDiscount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? ItemsDiscount { get; set; }

        /// <summary>Line discount (optional).</summary>
        [JsonPropertyName("discount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public LineDiscount Discount { get; set; }

        /// <summary>Taxes applied to the line. Each tax type must appear at most once.</summary>
        [JsonPropertyName("taxableItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public System.Collections.Generic.List<TaxableItem> TaxableItems { get; set; }

        /// <summary>Internal item code (optional).</summary>
        [JsonPropertyName("internalCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string InternalCode { get; set; }
    }

    /// <summary>Unit value (price) of an invoice line.</summary>
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

    /// <summary>Discount applied to an invoice line.</summary>
    public class LineDiscount
    {
        /// <summary>Discount rate percentage.</summary>
        [JsonPropertyName("rate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Rate { get; set; }

        /// <summary>Discount amount.</summary>
        [JsonPropertyName("amount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Amount { get; set; }
    }

    /// <summary>A tax applied to an invoice line.</summary>
    public class TaxableItem
    {
        /// <summary>Tax type code, e.g. <c>T1</c> (VAT). Must be unique per invoice line.</summary>
        [JsonPropertyName("taxType")]
        public string TaxType { get; set; }

        /// <summary>Tax amount.</summary>
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        /// <summary>Tax subtype, e.g. <c>V009</c>.</summary>
        [JsonPropertyName("subType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SubType { get; set; }

        /// <summary>Tax rate percentage.</summary>
        [JsonPropertyName("rate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Rate { get; set; }
    }

    /// <summary>Total of a tax type across all invoice lines.</summary>
    public class TaxTotal
    {
        /// <summary>Tax type code, e.g. <c>T1</c> (VAT).</summary>
        [JsonPropertyName("taxType")]
        public string TaxType { get; set; }

        /// <summary>Sum of all amounts of this tax across all invoice lines.</summary>
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    /// <summary>A digital signature attached to a submitted document.</summary>
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
