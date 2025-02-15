using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace aidemo
{
    public static class KeyVault
    {
        public static async Task<string> RetrieveKeyFromKeyVault(string secretName)
        {
            var keyVaultName = "aidemokeyvault1";
            var kvUri = $"https://{keyVaultName}.vault.azure.net";

            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            secretName = secretName + "key";
            Console.WriteLine($"Retrieving your secret from {keyVaultName}.");
            var secret = await client.GetSecretAsync(secretName);

            return secret.Value.Value;
        }
    }
}
