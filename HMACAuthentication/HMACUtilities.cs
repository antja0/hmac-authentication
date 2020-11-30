using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Antja.Authentication.HMAC
{
    public static class HMACUtilities
    {
        public static string GetSignaturePrefix(HMACHashFunctions hashFunction)
        {
            return $"sha{(int)hashFunction}=";
        }

        public static byte[] ComputeHash(HMACHashFunctions hashFunction, string secret, byte[] buffer)
        {
            return hashFunction switch
            {
                HMACHashFunctions.SHA1 => new HMACSHA1(Encoding.ASCII.GetBytes(secret)).ComputeHash(buffer),
                HMACHashFunctions.SHA256 => new HMACSHA256(Encoding.ASCII.GetBytes(secret)).ComputeHash(buffer),
                HMACHashFunctions.SHA512 => new HMACSHA512(Encoding.ASCII.GetBytes(secret)).ComputeHash(buffer),
                _ => throw new ArgumentOutOfRangeException(nameof(hashFunction), hashFunction, "Hash function not supported")
            };
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
