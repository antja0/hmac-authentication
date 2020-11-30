using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Antja.Authentication.HMAC.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Antja.Authentication.HMAC
{
    public class HMACSignatureHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, HMACSignatureOptions> _options;

        public HMACSignatureHandler(IHttpContextAccessor httpContextAccessor, IOptions<Dictionary<string, HMACSignatureOptions>> options, IOptionsMonitor<AuthenticationSchemeOptions> authOptions, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(authOptions, logger, encoder, clock)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!_options.ContainsKey(Scheme.Name))
            {
                throw new ArgumentException($"{nameof(HMACSignatureOptions)} not configured for scheme '{Scheme.Name}'");
            }

            var options = _options[Scheme.Name];
            var httpContext = _httpContextAccessor.HttpContext;

            httpContext.Request.Headers.TryGetValue(options.Header, out var signatureWithPrefix);

            if (string.IsNullOrWhiteSpace(signatureWithPrefix))
            {
                return AuthenticateResult.Fail($"{options.Header} header not present or empty.");
            }

            // Verify SHA signature.
            var prefix = HMACUtilities.GetSignaturePrefix(options.HashFunction);
            var signature = (string)signatureWithPrefix;
            if (signature.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                signature = signature.Substring(prefix.Length);

                using var reader = new StreamReader(httpContext.Request.Body);
                var bodyAsBytes = Encoding.ASCII.GetBytes(await reader.ReadToEndAsync());
                httpContext.Request.Body = new MemoryStream(bodyAsBytes); // Without this body stream is already red in Controller and cannot be used.

                var hash = HMACUtilities.ComputeHash(options.HashFunction, options.Secret, bodyAsBytes);

                var hashString = HMACUtilities.ToHexString(hash);
                if (hashString.Equals(signature))
                {
                    var identity = new ClaimsIdentity(nameof(HMACSignatureHandler));
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
            }

            return AuthenticateResult.Fail($"Invalid {options.Header} header value.");
        }
    }
}
