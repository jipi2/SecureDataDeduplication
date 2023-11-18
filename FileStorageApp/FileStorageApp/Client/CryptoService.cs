using CryptoLib;
using FileStorageApp.Shared.Dto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using System.Formats.Asn1;
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

        public async Task<byte[]> GetDecryptedFile(string encBase64File, string base64FileKey, string base64IvFile)
        {
            byte[] key = Convert.FromBase64String(base64FileKey);
            byte[] iv = Convert.FromBase64String(base64IvFile);

           string fileContent = Utils.DecryptAesWithIv(encBase64File, key, iv);
           return await Task.FromResult(Convert.FromBase64String(fileContent));
        }

        public async Task<Dictionary<string, string>> GetEncryptedFileParameters(string base64Tag, string base64Key, string base64Iv, string base64SymKey, string fileName)
        {
            Dictionary<string, string> fileParams = new Dictionary<string, string>();
            string base64TagEnc = Utils.EncryptAes(Convert.FromBase64String(base64Tag), Convert.FromBase64String(base64SymKey));
            string base64KeyEnc = Utils.EncryptAes(Convert.FromBase64String(base64Key), Convert.FromBase64String(base64SymKey));
            string base64IvEnc = Utils.EncryptAes(Convert.FromBase64String(base64Iv), Convert.FromBase64String(base64SymKey));
            string encFileName = Utils.EncryptAes(Encoding.UTF8.GetBytes(fileName), Convert.FromBase64String(base64SymKey));

            fileParams.Add("base64TagEnc", base64TagEnc);
            fileParams.Add("base64KeyEnc", base64KeyEnc);
            fileParams.Add("base64IvEnc", base64IvEnc);
            fileParams.Add("encFileName", encFileName);

            return await Task.FromResult(fileParams);
        }

        public string? DecryptString(byte[] ciphertext, byte[] key, byte[] iv = null)
        {

            try
            {
                string plaintext;
                if (iv != null)
                    plaintext = Utils.DecryptAes(ciphertext, key, iv);
                else
                    plaintext = Utils.DecryptAes(ciphertext, key);

                return plaintext;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            
        }
        private async Task<MerkleTree> GetMerkleTree(string base64EncFile)
        {
            byte[] bytesEncFile = Convert.FromBase64String(base64EncFile);
            Stream stream = new MemoryStream(bytesEncFile);
            MerkleTree mt = Utils.GetMerkleTree(stream);
            Console.WriteLine(mt.HashTree.Count);
            return mt;
        }
        public async Task<FileResp> GetFileResp(FileMetaChallenge fmc, string encBase64File, string fileName, string base64SymKey)
        {
            MerkleTree mt = await GetMerkleTree(encBase64File);
            byte[] answ = new byte[mt.HashTree[0]._hash.Length];
            byte[] byteSymKey = Convert.FromBase64String(base64SymKey);

            int Id = BitConverter.ToInt32(Convert.FromBase64String( Utils.DecryptAes(Convert.FromBase64String(fmc.id), byteSymKey)));
            int n1 = BitConverter.ToInt32(Convert.FromBase64String( Utils.DecryptAes(Convert.FromBase64String(fmc.n1), byteSymKey)));
            int n2 = BitConverter.ToInt32(Convert.FromBase64String( Utils.DecryptAes(Convert.FromBase64String(fmc.n2), byteSymKey)));
            int n3 = BitConverter.ToInt32(Convert.FromBase64String(Utils.DecryptAes(Convert.FromBase64String(fmc.n3), byteSymKey)));

            for (int i = 0; i < answ.Length; i++)
            {
                answ[i] = (byte)(mt.HashTree[n1]._hash[i] ^ mt.HashTree[n2]._hash[i] ^ mt.HashTree[n3]._hash[i]);
            }

            FileResp fr = new FileResp
            {
                Answer = Utils.EncryptAes(answ, byteSymKey),
                Id = fmc.id,
                FileName = Utils.EncryptAes(Encoding.UTF8.GetBytes(fileName), byteSymKey)
            };
            return fr;
        }

    }
}
