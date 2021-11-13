using Microsoft.Extensions.DependencyInjection;
using NzCovidPass.Core.Cbor;
using NzCovidPass.Core.Verification;

namespace NzCovidPass.Core.Shared
{
    /// <summary>
    /// Extension methods to configure an <see cref="IServiceCollection" /> for the verifier.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="PassVerifier" /> and related services to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" />.</param>
        /// <param name="configureOptions">The action used to configure the <see cref="PassVerifierOptions" />.</param>
        /// <param name="configureClient">The action used to configure the <see cref="HttpClient" />.</param>
        /// <returns>The updated <see cref="IServiceCollection" />.</returns>
        public static IServiceCollection AddNzCovidPassVerifier(
            this IServiceCollection services,
            Action<PassVerifierOptions>? configureOptions = null,
            Action<HttpClient>? configureClient = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.Configure<PassVerifierOptions>(configureOptions ?? ConfigureDefaultOptions);

            services.AddHttpClient(nameof(HttpDecentralizedIdentifierDocumentRetriever), configureClient ?? ConfigureDefaultClient);

            services.AddSingleton<ICborWebTokenReader, CborWebTokenReader>();
            services.AddSingleton<ICborWebTokenValidator, CborWebTokenValidator>();
            services.AddSingleton<IVerificationKeyProvider, VerificationKeyProvider>();
            services.AddSingleton<IDecentralizedIdentifierDocumentRetriever, HttpDecentralizedIdentifierDocumentRetriever>();
            services.AddSingleton<PassVerifier>();

            return services;
        }

        private static void ConfigureDefaultOptions(PassVerifierOptions options)
        {
            options.Prefix = PassVerifierOptions.Defaults.Prefix;
            options.Version = PassVerifierOptions.Defaults.Version;
            options.ValidIssuers = PassVerifierOptions.Defaults.ValidIssuers.ToHashSet();
            options.ValidAlgorithms = PassVerifierOptions.Defaults.ValidAlgorithms.ToHashSet();
        }

        private static void ConfigureDefaultClient(HttpClient client)
        {
        }
}
}
