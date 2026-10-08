using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Registers <see cref="EtaInvoice.EtaClient"/> in the DI container.
    /// </summary>
    public static class EtaServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="EtaInvoice.EtaClient"/> as a singleton.
        /// Prefer <see cref="AddEtaInvoice(IServiceCollection, Action{EtaInvoice.EtaClientOptions})"/>
        /// with <c>IHttpClientFactory</c>-managed <see cref="System.Net.Http.HttpClient"/>
        /// for production use.
        /// </summary>
        public static IServiceCollection AddEtaInvoice(
            this IServiceCollection services, EtaInvoice.EtaClientOptions options)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (options == null) throw new ArgumentNullException(nameof(options));

            services.TryAddSingleton(options);
            services.TryAddSingleton<EtaInvoice.EtaClient>();
            return services;
        }

        public static IServiceCollection AddEtaInvoice(
            this IServiceCollection services, Action<EtaInvoice.EtaClientOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var options = new EtaInvoice.EtaClientOptions();
            configure(options);
            return services.AddEtaInvoice(options);
        }
    }
}
