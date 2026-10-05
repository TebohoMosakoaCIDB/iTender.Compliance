using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System.Security.Cryptography;
using System.Text;

namespace iTender.Compliance.Infrastructure.CRM
{
    public class CrmServiceFactory : ICrmServiceFactory
    {
        private readonly CrmOptions _options;
        private readonly EncryptionOptions _encryptionOptions;

        public CrmServiceFactory(IOptions<CrmOptions> options, IOptions<EncryptionOptions> encryptionOptions)
        {
            _options = options.Value;
            _encryptionOptions = encryptionOptions.Value;
        }

        public IOrganizationService Create()
        {
            var decryptedSecret = Decrypt(_options.ClientSecret);

            var connectionString =
                $"AuthType=ClientSecret;" +
                $"Url={_options.CrmURL};" +
                $"ClientId={_options.ClientId};" +
                $"ClientSecret={decryptedSecret};";

            var client = new ServiceClient(connectionString);

            if (!client.IsReady)
            {
                throw new Exception($"CRM authentication failed for App: {_options.ClientId}");
            }

            return client;
        }

        private string Decrypt(string textToDecrypt)
        {
            try
            {
                byte[] array = new byte[0];
                array = Encoding.UTF8.GetBytes(_encryptionOptions.IV.Substring(5, 8));
                byte[] array2 = new byte[0];
                array2 = Encoding.UTF8.GetBytes(_encryptionOptions.Key.Substring(3, 8));
                byte[] array3 = new byte[textToDecrypt.Replace(" ", "+").Length];
                array3 = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    using (MemoryStream inputStream = new MemoryStream(array3))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(
                            inputStream,
                            des.CreateDecryptor(array2, array),
                            CryptoStreamMode.Read))
                        {
                            using (MemoryStream output = new MemoryStream())
                            {
                                cryptoStream.CopyTo(output);
                                return Encoding.UTF8.GetString(output.ToArray());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }
    }
}
