using System;

namespace EtaInvoice
{
    /// <summary>
    /// Configuration for <see cref="EtaClient"/>.
    /// Get the client id / secret by registering your ERP system on the
    /// ETA e-invoicing portal (taxpayer profile → representatives → add ERP).
    /// </summary>
    public class EtaClientOptions
    {
        /// <summary>Client id issued by the ETA portal when registering your ERP system.</summary>
        public string ClientId { get; set; }

        /// <summary>Client secret issued by the ETA portal when registering your ERP system.</summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// OAuth2 token endpoint. Defaults to the pre-production environment.
        /// Production: <c>https://id.eta.gov.eg/connect/token</c>
        /// </summary>
        public string AuthUrl { get; set; } = "https://id.preprod.eta.gov.eg/connect/token";

        /// <summary>
        /// eInvoicing API base url. Defaults to the pre-production environment.
        /// Production: <c>https://api.invoicing.eta.gov.eg</c>
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.preprod.invoicing.eta.gov.eg";

        /// <summary>OAuth2 scope requested from the identity server.</summary>
        public string Scope { get; set; } = "InvoicingAPI";

        /// <summary>
        /// Creates options pointed at the production environment.
        /// Only switch to production after testing against preprod.
        /// </summary>
        public static EtaClientOptions Production(string clientId, string clientSecret)
        {
            return new EtaClientOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                AuthUrl = "https://id.eta.gov.eg/connect/token",
                BaseUrl = "https://api.invoicing.eta.gov.eg"
            };
        }

        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(ClientId))
                throw new ArgumentException("ClientId is required.", nameof(ClientId));
            if (string.IsNullOrWhiteSpace(ClientSecret))
                throw new ArgumentException("ClientSecret is required.", nameof(ClientSecret));
        }
    }
}
