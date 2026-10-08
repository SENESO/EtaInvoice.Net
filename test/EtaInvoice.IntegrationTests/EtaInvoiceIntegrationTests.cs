using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EtaInvoice.Models;
using EtaInvoice.Validation;
using NUnit.Framework;

namespace EtaInvoice.IntegrationTests
{
    /// <summary>
    /// Integration tests against the ETA e-invoicing pre-production environment
    /// (<c>https://api.preprod.invoicing.eta.gov.eg</c>).
    /// <para>
    /// These tests call the real API. They are skipped (not failed) when the
    /// required environment variables are absent, so a plain
    /// <c>dotnet test</c> stays green in CI without credentials.
    /// </para>
    /// <para>
    /// Signing scope note: production e-invoices require a CAdES-BES signature
    /// from the taxpayer's USB eSeal token, which cannot be automated here.
    /// The optional write test submits an unsigned document with obviously
    /// test-only data; the SDK documents that unsigned submission is for
    /// preprod testing only.
    /// </para>
    /// </summary>
    [TestFixture]
    public class EtaInvoiceIntegrationTests
    {
        private const string DefaultBaseUrl = "https://api.preprod.invoicing.eta.gov.eg";

        /// <summary>
        /// Creates an <see cref="EtaClient"/> pointed at preprod from environment
        /// variables. Skips the test with the missing variable's name when any
        /// required credential is absent.
        /// </summary>
        private static EtaClient CreateClient()
        {
            var clientId = Environment.GetEnvironmentVariable("ETA_CLIENT_ID");
            if (string.IsNullOrWhiteSpace(clientId))
                Assert.Ignore("Skipped: ETA_CLIENT_ID is not set.");

            var clientSecret = Environment.GetEnvironmentVariable("ETA_CLIENT_SECRET");
            if (string.IsNullOrWhiteSpace(clientSecret))
                Assert.Ignore("Skipped: ETA_CLIENT_SECRET is not set.");

            var options = new EtaClientOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            var baseUrl = Environment.GetEnvironmentVariable("ETA_BASE_URL");
            if (!string.IsNullOrWhiteSpace(baseUrl))
                options.BaseUrl = baseUrl;

            // Guardrail: these tests must never hit the production environment.
            Assert.That(options.BaseUrl.TrimEnd('/'), Is.Not.EqualTo("https://api.invoicing.eta.gov.eg"),
                "Integration tests must not run against the ETA production environment.");

            return new EtaClient(options);
        }

        [Test]
        public async Task GetAccessToken_AgainstPreprod_ReturnsNonEmptyToken()
        {
            using var client = CreateClient();

            var token = await client.GetAccessTokenAsync();

            Assert.That(token, Is.Not.Null.And.Not.Empty,
                "Preprod issued an empty access token.");
        }

        [Test]
        public async Task SearchDocuments_AgainstPreprod_ReturnsResult()
        {
            using var client = CreateClient();

            // Narrow one-week window: read-only and cheap on the API.
            var to = DateTime.UtcNow.Date.AddDays(1);
            var from = to.AddDays(-7);

            var result = await client.SearchDocumentsAsync(new SearchDocumentsQuery
            {
                SubmissionDateFrom = from,
                SubmissionDateTo = to,
                PageSize = 10
            });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.Not.Null);
        }

        [Test]
        public async Task GetSubmission_AgainstPreprod_ReturnsInfo()
        {
            var submissionUuid = Environment.GetEnvironmentVariable("ETA_TEST_SUBMISSION_UUID");
            if (string.IsNullOrWhiteSpace(submissionUuid))
                Assert.Ignore("Skipped: ETA_TEST_SUBMISSION_UUID is not set. Set it to a submission uuid from the preprod taxpayer profile to exercise this test.");

            using var client = CreateClient();

            var info = await client.GetSubmissionAsync(submissionUuid);

            Assert.That(info, Is.Not.Null);
            Assert.That(info.Uuid, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task SubmitDocuments_AgainstPreprod_RequiresExplicitOptIn()
        {
            var allowSubmit = Environment.GetEnvironmentVariable("ETA_ALLOW_SUBMIT");
            if (!string.Equals(allowSubmit, "true", StringComparison.OrdinalIgnoreCase))
                Assert.Ignore("Skipped: set ETA_ALLOW_SUBMIT=true to run this write test against preprod.");

            using var client = CreateClient();

            // Obviously test-only data. The document is submitted unsigned;
            // production signing requires a USB eSeal token, so real signatures
            // are outside the scope of automated integration tests.
            var document = new EtaDocument
            {
                Issuer = new Taxpayer
                {
                    Type = TaxpayerTypes.Business,
                    Id = "000000000",
                    Name = "Integration Test Issuer",
                    Address = new TaxpayerAddress
                    {
                        BranchId = "0",
                        Country = "EG",
                        Governate = "Cairo",
                        RegionCity = "Nasr City",
                        Street = "Test Street",
                        BuildingNumber = "1"
                    }
                },
                Receiver = new Taxpayer
                {
                    Type = TaxpayerTypes.Business,
                    Id = "000000000",
                    Name = "Integration Test Receiver",
                    Address = new TaxpayerAddress
                    {
                        Country = "EG",
                        Governate = "Cairo",
                        RegionCity = "Nasr City",
                        Street = "Test Street",
                        BuildingNumber = "2"
                    }
                },
                DocumentType = DocumentTypes.Invoice,
                DocumentTypeVersion = "1.0",
                DateTimeIssued = DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                TaxpayerActivityCode = "0000",
                InternalId = "ETAINVOICE-INTEGRATION-TEST-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"),
                InvoiceLines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Description = "Integration test line item",
                        ItemType = "EGS",
                        ItemCode = "EG-TEST-001",
                        UnitType = "EA",
                        Quantity = 1,
                        UnitValue = new UnitValue { CurrencySold = "EGP", AmountEGP = 100m },
                        SalesTotal = 100m,
                        Total = 114m,
                        TaxableItems = new List<TaxableItem>
                        {
                            new TaxableItem { TaxType = "T1", SubType = "V009", Amount = 14m, Rate = 14m }
                        }
                    }
                },
                TotalSalesAmount = 100m,
                NetAmount = 100m,
                TaxTotals = new List<TaxTotal>
                {
                    new TaxTotal { TaxType = "T1", Amount = 14m }
                },
                TotalAmount = 114m
            };

            var validationErrors = EtaDocumentValidator.Validate(document);
            Assert.That(validationErrors, Is.Empty,
                "Test document failed SDK validation: " + string.Join("; ", validationErrors));

            var result = await client.SubmitDocumentsAsync(new[] { document });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.SubmissionUuid, Is.Not.Null.And.Not.Empty);
        }
    }
}
