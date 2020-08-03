using Geolocation.Constants;
using Geolocation.Utilities.Encryption;
using System;

namespace Geolocation.Utilities.Google
{
    internal static class GoogleUtil
    {
        public static readonly string googleApiKey;
        public const string output = "json";

        static GoogleUtil()
        {
            string encryptedApiKey = Environment.GetEnvironmentVariable(EnvironmentVariablesNames.ENCRYPTED_GOOGLE_API_KEY);
            googleApiKey = DesEncryptor.DecryptData(encryptedApiKey);
        }
    }
}
