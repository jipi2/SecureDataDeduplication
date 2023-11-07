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

        public async Task<string> GetHashInBase64(byte[] text, int hashSize)
        {
            byte[] h = Utils.ComputeHash(text, hashSize);
            return await Task.FromResult(Convert.ToBase64String(h));
        }

        public async Task<string> ExtractIv(byte[] keyAndIv, int lenKey, int lenIv)
        {
            byte[] iv = new byte[lenIv];
            Array.Copy(keyAndIv, lenKey, iv, 0, lenIv);
            return await Task.FromResult(Convert.ToBase64String(iv));
        }
        public async Task<string> ExtractKey(byte[] keyAndIv, int lenKey)
        {
            byte[] key = new byte[lenKey];
            Array.Copy(keyAndIv, key, lenKey);
            return await Task.FromResult(Convert.ToBase64String(key));
        }
        public async Task<string> GetEcnryptedFileBase64(string base64FileKey, string base64IvFile,byte[] byteFile)     //generam aici iv-ul
        {
            byte[] key = Convert.FromBase64String(base64FileKey);
            byte[] iv = Convert.FromBase64String(base64IvFile);

            string cipherText = Utils.EncryptAesWithIv(byteFile, key, iv);
            return await Task.FromResult(cipherText);
        }

        public async Task<Dictionary<string, string>> GetEncryptedFileParameters(string base64Tag, string base64Key, string base64Iv, string base64SymKey)
        {
            Dictionary<string, string> fileParams = new Dictionary<string, string>();
            string base64TagEnc = Utils.EncryptAes(Convert.FromBase64String(base64Tag), Convert.FromBase64String(base64SymKey));
            string base64KeyEnc = Utils.EncryptAes(Convert.FromBase64String(base64Key), Convert.FromBase64String(base64SymKey));
            string base64IvEnc = Utils.EncryptAes(Convert.FromBase64String(base64Iv), Convert.FromBase64String(base64SymKey));

            fileParams.Add("base64TagEnc", base64TagEnc);
            fileParams.Add("base64KeyEnc", base64KeyEnc);
            fileParams.Add("base64IvEnc", base64IvEnc);

            return await Task.FromResult(fileParams);
        }
    }
}
