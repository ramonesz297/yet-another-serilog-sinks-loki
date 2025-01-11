using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Serilog.Sinks.Loki.Internal
{
    internal static partial class LokiHttpClientExtensions
    {
        private const string _tenantHeader = "X-Scope-OrgID";

#if NET7_0_OR_GREATER
        [GeneratedRegex("@\"^[a-zA-Z0-9]*$\"")]
        private static partial Regex TenantIdValueRegex();
#else

        private static readonly Regex _tenantIdValueRegex = new(@"^[a-zA-Z0-9]*$");
        private static Regex TenantIdValueRegex()
        {
            return _tenantIdValueRegex;
        }
#endif

        internal static void SetCredentials(this HttpClient httpClient, LokiCredentials? credentials)
        {
            if (credentials is null)
            {
                return;
            }

            if (httpClient.DefaultRequestHeaders.Authorization is null)
            {
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.Username}:{credentials.Password ?? string.Empty}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            }
        }

        internal static void SetTenant(this HttpClient httpClient, string? tenant)
        {
            if (string.IsNullOrEmpty(tenant))
            {
                return;
            }

            var headers = httpClient.DefaultRequestHeaders;

            if (headers.Contains(_tenantHeader))
            {
                return;
            }

            if (!TenantIdValueRegex().IsMatch(tenant))
            {
                throw new ArgumentException($"{tenant} argument does not follow rule for Tenant ID", nameof(tenant));
            }

            headers.Add(_tenantHeader, tenant);
        }
    }
}
