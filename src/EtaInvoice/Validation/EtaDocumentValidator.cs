using System;
using System.Collections.Generic;
using EtaInvoice.Models;

namespace EtaInvoice.Validation
{
    /// <summary>
    /// Client-side structural validation of an <see cref="EtaDocument"/> against
    /// the core field rules published in the ETA SDK documentation.
    /// <para>
    /// This catches obvious mistakes before a network call; it is <b>not</b> a
    /// substitute for ETA server-side validation (tax codes, signatures,
    /// taxpayer status, duplicates, …).
    /// </para>
    /// </summary>
    public static class EtaDocumentValidator
    {
        /// <summary>
        /// Validates the document structure. Returns the list of problems found
        /// (empty means the document looks structurally valid).
        /// </summary>
        public static IReadOnlyList<string> Validate(EtaDocument document)
        {
            var errors = new List<string>();

            if (document == null)
            {
                errors.Add("Document is required.");
                return errors;
            }

            ValidateTaxpayer(document.Issuer, "issuer", errors, requireBranchId: true);
            ValidateTaxpayer(document.Receiver, "receiver", errors, requireBranchId: false);

            if (string.IsNullOrWhiteSpace(document.DocumentType))
                errors.Add("documentType is required (e.g. 'i' for invoice).");

            if (string.IsNullOrWhiteSpace(document.DocumentTypeVersion))
                errors.Add("documentTypeVersion is required (e.g. '1.0').");

            if (string.IsNullOrWhiteSpace(document.InternalId))
                errors.Add("internalId is required.");

            if (string.IsNullOrWhiteSpace(document.TaxpayerActivityCode))
                errors.Add("taxpayerActivityCode is required.");

            if (string.IsNullOrWhiteSpace(document.DateTimeIssued))
            {
                errors.Add("dateTimeIssued is required.");
            }
            else if (DateTime.TryParse(document.DateTimeIssued, out var issued))
            {
                if (issued.ToUniversalTime() > DateTime.UtcNow.AddMinutes(5))
                    errors.Add("dateTimeIssued cannot be in the future.");
            }
            else
            {
                errors.Add("dateTimeIssued is not a valid date/time.");
            }

            if (document.InvoiceLines == null || document.InvoiceLines.Count == 0)
            {
                errors.Add("At least one invoice line is required.");
            }
            else
            {
                for (var i = 0; i < document.InvoiceLines.Count; i++)
                    ValidateLine(document.InvoiceLines[i], i, errors);
            }

            if (document.TaxTotals == null || document.TaxTotals.Count == 0)
                errors.Add("taxTotals is required (at least one tax total).");

            if (document.TotalAmount <= 0)
                errors.Add("totalAmount must be greater than zero.");

            return errors;
        }

        private static void ValidateTaxpayer(Taxpayer taxpayer, string role, List<string> errors, bool requireBranchId)
        {
            if (taxpayer == null)
            {
                errors.Add($"{role} is required.");
                return;
            }

            if (taxpayer.Type != TaxpayerTypes.Business &&
                taxpayer.Type != TaxpayerTypes.Person &&
                taxpayer.Type != TaxpayerTypes.Foreigner)
                errors.Add($"{role}.type must be one of 'B', 'P', 'F'.");

            if (string.IsNullOrWhiteSpace(taxpayer.Id))
                errors.Add($"{role}.id is required.");
            else if (!IsAllDigits(taxpayer.Id))
                errors.Add($"{role}.id must contain digits only.");

            if (string.IsNullOrWhiteSpace(taxpayer.Name))
                errors.Add($"{role}.name is required.");

            if (taxpayer.Address == null)
            {
                errors.Add($"{role}.address is required.");
                return;
            }

            var address = taxpayer.Address;
            if (string.IsNullOrWhiteSpace(address.Country))
                errors.Add($"{role}.address.country is required.");
            if (string.IsNullOrWhiteSpace(address.Governate))
                errors.Add($"{role}.address.governate is required.");
            if (string.IsNullOrWhiteSpace(address.RegionCity))
                errors.Add($"{role}.address.regionCity is required.");
            if (string.IsNullOrWhiteSpace(address.Street))
                errors.Add($"{role}.address.street is required.");
            if (string.IsNullOrWhiteSpace(address.BuildingNumber))
                errors.Add($"{role}.address.buildingNumber is required.");

            if (requireBranchId && taxpayer.Type == TaxpayerTypes.Business &&
                string.IsNullOrWhiteSpace(address.BranchId))
                errors.Add($"{role}.address.branchId is required when issuer type is 'B'.");
        }

        private static void ValidateLine(InvoiceLine line, int index, List<string> errors)
        {
            var prefix = $"invoiceLines[{index}]";
            if (line == null)
            {
                errors.Add($"{prefix} is null.");
                return;
            }

            if (string.IsNullOrWhiteSpace(line.Description))
                errors.Add($"{prefix}.description is required.");

            if (line.ItemType != "GS1" && line.ItemType != "EGS")
                errors.Add($"{prefix}.itemType must be 'GS1' or 'EGS'.");

            if (string.IsNullOrWhiteSpace(line.ItemCode))
                errors.Add($"{prefix}.itemCode is required.");

            if (line.Quantity <= 0)
                errors.Add($"{prefix}.quantity must be greater than zero.");

            if (line.UnitValue == null)
            {
                errors.Add($"{prefix}.unitValue is required.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(line.UnitValue.CurrencySold))
                    errors.Add($"{prefix}.unitValue.currencySold is required.");
                else if (!string.Equals(line.UnitValue.CurrencySold, "EGP", StringComparison.OrdinalIgnoreCase) &&
                         (!line.UnitValue.AmountSold.HasValue || !line.UnitValue.CurrencyExchangeRate.HasValue))
                    errors.Add($"{prefix}.unitValue.amountSold and currencyExchangeRate are required when currencySold is not EGP.");
            }

            if (line.TaxableItems != null)
            {
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var tax in line.TaxableItems)
                {
                    if (tax == null || string.IsNullOrWhiteSpace(tax.TaxType)) continue;
                    if (!seen.Add(tax.TaxType))
                        errors.Add($"{prefix}: taxType '{tax.TaxType}' must be unique within the invoice line.");
                }
            }
        }

        private static bool IsAllDigits(string value)
        {
            foreach (var c in value)
                if (!char.IsDigit(c)) return false;
            return value.Length > 0;
        }
    }
}
