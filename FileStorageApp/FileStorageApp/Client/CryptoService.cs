using CryptoLib;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FileStorageApp.Client
{
    public class CryptoService
    {
        public async Task<Dictionary<string, string>> GenerateKeys(Dictionary<string, string> stringParams)
        {

            DHParameters parameters = Utils.GenerateParameters(stringParams["G"], stringParams["P"]);
            AsymmetricCipherKeyPair keys = Utils.GenerateDFKeys(parameters);
            Dictionary<string, string> stringKeys = new Dictionary<string, string>();

            BigInteger bigSymKey = Utils.ComputeSharedSecret(stringParams["ServerPubKey"], keys.Private, parameters);
            byte[] byteSymKey = bigSymKey.ToByteArray();
           
            stringKeys.Add("SymKey", Convert.ToBase64String(Utils.ComputeHash(byteSymKey)));
            stringKeys.Add("Public",Utils.GetPemAsString(keys.Public));
            stringKeys.Add("Private", Utils.GetPemAsString(keys.Private));
            stringKeys.Add("A", Utils.GetPublicKey(keys));
            
            

            return stringKeys;
        }
    }
}
