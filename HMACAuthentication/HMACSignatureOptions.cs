namespace Antja.Authentication.HMAC
{
    public class HMACSignatureOptions
    {
        public string Secret { get; set; }

        public string Header { get; set; } = "X-Hub-Signature"; // Default used by eg. Github webhooks.

        public HMACHashFunctions HashFunction { get; set; } = HMACHashFunctions.SHA1; // Default used by eg. Github webhooks.
    }
}
