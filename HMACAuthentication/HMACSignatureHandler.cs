using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Antja.Authentication.HMAC
{
    public class HMACSignatureHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string ShaPrefix = "sha256=";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HMACSignatureOptions _options;

        public HMACSignatureHandler(IHttpContextAccessor httpContextAccessor, IOptions<HMACSignatureOptions> options, IOptionsMonitor<AuthenticationSchemeOptions> authOptions, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(authOptions, logger, encoder, clock)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            httpContext.Request.Headers.TryGetValue(_options.Header, out var signatureWithPrefix);

            if (string.IsNullOrWhiteSpace(signatureWithPrefix))
            {
                return AuthenticateResult.Fail($"{_options.Header} header not present or empty.");
            }

            // Verify SHA signature.
            var signature = (string)signatureWithPrefix;
            if (signature.StartsWith(ShaPrefix, StringComparison.OrdinalIgnoreCase))
            {
                signature = signature.Substring(ShaPrefix.Length);

                using var reader = new StreamReader(httpContext.Request.Body);
                var bodyAsBytes = Encoding.ASCII.GetBytes(await reader.ReadToEndAsync());
                httpContext.Request.Body = new MemoryStream(bodyAsBytes); // Without this body stream is already red in Controller and cannot be used.

                using var sha = new HMACSHA256(Encoding.ASCII.GetBytes(_options.Secret));
                var hash = sha.ComputeHash(bodyAsBytes);

                var hashString = ToHexString(hash);
                if (hashString.Equals(signature))
                {
                    var identity = new ClaimsIdentity(nameof(HMACSignatureHandler));
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
            }

            return AuthenticateResult.Fail($"Invalid {_options.Header} header value.");
        }

        public static string ToHexString(IReadOnlyCollection<byte> bytes)
        {
            var builder = new StringBuilder(bytes.Count * 2);

            foreach (var b in bytes)
            {
                builder.AppendFormat("{0:x2}", b);
            }

            return builder.ToString();
        }
    }
}
