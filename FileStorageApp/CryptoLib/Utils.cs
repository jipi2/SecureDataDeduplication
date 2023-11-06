using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib
{
    public class Utils
    {
        static public byte[] generateRandomBytes(int length)
        {
            var randomGenerator = new SecureRandom();
            var randomBytes = new byte[length];
            randomGenerator.NextBytes(randomBytes);
            return randomBytes;
        }

        static public byte[] HexToByte(string hexString)
        {
            return Hex.Decode(hexString);
        }

        static public string ByteToHex(byte[] bytes)
        {
            return Hex.ToHexString(bytes);
        }
        static public string HashTextWithSalt(string text, byte[] salt)
        {
            Encoding encoding = Encoding.UTF8;
            byte[] textBytes = encoding.GetBytes(text);

            var sha512 = new Sha512Digest();


            var combinedBytes = new byte[salt.Length + textBytes.Length];
            Array.Copy(salt, combinedBytes, salt.Length);
            Array.Copy(textBytes, 0, combinedBytes, salt.Length, text.Length);

            sha512.BlockUpdate(combinedBytes, 0, combinedBytes.Length);

            byte[] hash = new byte[sha512.GetDigestSize()];
            sha512.DoFinal(hash, 0);

            return Hex.ToHexString(hash);

        }
        static public byte[] ComputeHash(byte[] array, int hashSize = 256)
        {
            byte[] h = new byte[hashSize];
            var sha3_256 = new Sha3Digest(hashSize);

            sha3_256.BlockUpdate(array, 0, array.Length);
            sha3_256.DoFinal(h, 0);

            return h;
        }
        static public string GetHashOfFile(Stream file)
        {

            byte[] h = new byte[64];
            var sha3_512 = new Sha3Digest(512);

            long fileSize = file.Length;
            byte[] buffer = new byte[1024];

            int bytesRead = 0;
            while((bytesRead = file.Read(buffer, 0, buffer.Length))>0)
            {
                sha3_512.BlockUpdate(buffer, 0, bytesRead);
            }

            sha3_512.DoFinal(h, 0);
            
            return Hex.ToHexString(h);
        }

        static private void GetLeaves(ref MerkleTree MT, int count)
        {
            while (count > 1)
            {
                count = 0;
                int level = MT.Levels+1;
                int n = MT.HashTree.Count;
                MerkleTree aux = new MerkleTree();


                for(int i = MT.IndexOfLevel[MT.Levels]; i<n; i+=2)
                {
                    byte[] h = new byte[32];
                    var sha3_256 = new Sha3Digest(256);
                    sha3_256.BlockUpdate(MT.HashTree[i]._hash, 0, MT.HashTree[i]._hash.Length);
                    sha3_256.BlockUpdate(MT.HashTree[i + 1]._hash, 0, MT.HashTree[i + 1]._hash.Length);
                    sha3_256.DoFinal(h, 0);
                    aux.HashTree.Add(new MTMember(level, h));
                    count++;
                }
                foreach(MTMember m in aux.HashTree)
                {
                    MT.HashTree.Add(m);
                }
                MT.IndexOfLevel.Add(MT.HashTree.Count - count);
                MT.Levels++;
            }
        }

        static public MerkleTree GetMerkleTree(Stream file)
        {
            MerkleTree MT = new MerkleTree();
            long fileSize = file.Length;
            byte[] buffer = new byte[1024];

            int bytesRead = 0;
            int count = 0;
            while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
            {
                byte[] h = new byte[32];
                var sha3_256 = new Sha3Digest(256);
                sha3_256.BlockUpdate(buffer, 0, bytesRead);
                sha3_256.DoFinal(h, 0);
                MT.HashTree.Add(new MTMember(0, h));
                count++;
            }
            if (count % 2 != 0)
            {
                MT.HashTree.Add(MT.HashTree[0]);
                count++;
            }
            MT.IndexOfLevel.Add(0);
            GetLeaves(ref MT, count);

            return MT;
        }

        static public AsymmetricCipherKeyPair GenerateRSAKeyPair()
        {
            int modulus = 2048;
            RsaKeyPairGenerator g = new RsaKeyPairGenerator();
            g.Init(new KeyGenerationParameters(new SecureRandom(), modulus));
            AsymmetricCipherKeyPair keys = g.GenerateKeyPair();

            return keys;
        }
        static public string GetPemAsString(AsymmetricKeyParameter key)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(key);
            return textWriter.ToString();
        }
        static public void WritePrivateKeyToFile(AsymmetricKeyParameter privKey, string privFileName)
        {
            TextWriter textWriterPrivate = new StringWriter();
            PemWriter pemWriterPrivate = new PemWriter(textWriterPrivate);
            pemWriterPrivate.WriteObject(privKey);
            File.WriteAllText(privFileName, textWriterPrivate.ToString());
        }

        static public void WritePublicKeyToFile(AsymmetricKeyParameter pubKey, string pubFileName)
        {
            TextWriter textWriterPublic = new StringWriter();
            PemWriter pemWriterPublic = new PemWriter(textWriterPublic);
            pemWriterPublic.WriteObject(pubKey);
            File.WriteAllText(pubFileName, textWriterPublic.ToString());
        }

        static public DHPrivateKeyParameters ReadPrivateKeyFromPemString(string key)
        {
            TextReader textReaderPrivate = new StringReader(key);
            PemReader pemReaderPrivate = new PemReader(textReaderPrivate);
            DHPrivateKeyParameters keyPair = (DHPrivateKeyParameters)pemReaderPrivate.ReadObject();
            return keyPair;
        }

        static public AsymmetricKeyParameter ReadRSAPrivateKeyFromFile(string privFileName)
        {
            TextReader textReaderPrivate = new StringReader(File.ReadAllText(privFileName));
            PemReader pemReaderPrivate = new PemReader(textReaderPrivate);
            AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)pemReaderPrivate.ReadObject();
            return keyPair.Private;
        }

        static public AsymmetricKeyParameter ReadRSAPublicKeyFromFile(string pubFileName)
        {
            TextReader textReaderPublic = new StringReader(File.ReadAllText(pubFileName));
            PemReader pemReaderPublic = new PemReader(textReaderPublic);
            AsymmetricKeyParameter pubKey = (AsymmetricKeyParameter)pemReaderPublic.ReadObject();
            return pubKey;
        }

        static public DHParameters GenerateParameters()
        {
            var generator = new DHParametersGenerator();
            generator.Init(256, 30, new SecureRandom());
            return generator.GenerateParameters();
        }

        static public string GetG(DHParameters parameters)
        {
            return parameters.G.ToString();
        }

        static public string GetP(DHParameters parameters)
        {
            return parameters.P.ToString();
        }

        static public DHParameters GenerateParameters(string G, string P)
        {
            BigInteger GBig = new BigInteger(G);
            BigInteger PBig = new BigInteger(P);

            return new DHParameters(PBig, GBig);

        }

        static public AsymmetricCipherKeyPair GenerateDFKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        // This returns A
        static public string GetPublicKey(AsymmetricCipherKeyPair keyPair)
        {
            var dhPublicKeyParameters = keyPair.Public as DHPublicKeyParameters;
            if (dhPublicKeyParameters != null)
            {
                return dhPublicKeyParameters.Y.ToString();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }
        // This returns a
        static public string GetPrivateKey(AsymmetricCipherKeyPair keyPair)
        {
            var dhPrivateKeyParameters = keyPair.Private as DHPrivateKeyParameters;
            if (dhPrivateKeyParameters != null)
            {
                return dhPrivateKeyParameters.X.ToString();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }

        static public BigInteger ComputeSharedSecret(string A, AsymmetricKeyParameter bPrivateKey, DHParameters internalParameters)
        {
            var importedKey = new DHPublicKeyParameters(new BigInteger(A), internalParameters);
            var internalKeyAgree = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgree.Init(bPrivateKey);
            return internalKeyAgree.CalculateAgreement(importedKey);

            /* Exemplu de folosire
             
            DHParameters parameters = GenerateParameters();
            DHParameters parameters2 = GenerateParameters(GetG(parameters), GetP(parameters));

            AsymmetricCipherKeyPair bobKeys = GenerateKeys(parameters2);
            AsymmetricCipherKeyPair aliceKeys = GenerateKeys(parameters2);

            string bobPublicKey = GetPublicKey(bobKeys);
            string alicePublicKey = GetPublicKey(aliceKeys);

            BigInteger secretBob = ComputeSharedSecret(alicePublicKey, bobKeys.Private, parameters2);
            BigInteger secretAlice = ComputeSharedSecret(bobPublicKey, aliceKeys.Private, parameters2);

            if (secretBob.Equals(secretAlice))
                Console.WriteLine("The secret is oke");
            else
                Console.WriteLine("Error secret");
             */
        }



        //static public byte[] EncryptAesGcm(byte[] plainText, byte[] key)
        //{
        //    var nonce = new byte[8];
        //    var random = new SecureRandom();
        //    random.NextBytes(nonce, 0, nonce.Length);


        //    var tagLength = 16;
        //    var cipher = new GcmBlockCipher(new AesEngine());
        //    var parameters = new AeadParameters(new KeyParameter(key), tagLength*8, nonce);  //este folosit pt tagul de autenticitate\
        //    cipher.Init(true, parameters);

        //    var cipherText = new byte[cipher.GetOutputSize(plainText.Length)+16];
        //    var len = cipher.ProcessBytes(plainText, 0, plainText.Length, cipherText, 0);
        //    cipher.DoFinal(cipherText, len);

        //    Console.WriteLine("ciphertext: " + ByteToHex(cipherText));

        //    using (var combinedStream = new MemoryStream())
        //    {
        //        using (var binaryWriter = new BinaryWriter(combinedStream))
        //        {
        //            //Prepend Nonce
        //            binaryWriter.Write(nonce);
        //            //Write Cipher Text
        //            binaryWriter.Write(cipherText);
        //        }
        //        return combinedStream.ToArray();
        //    }
        //}

        //static public byte[] DecryptAesGcm(byte[] encBlob, byte[] key)
        //{

        //    var tageLength = 16;
        //    using (var cipherStream = new MemoryStream(encBlob))
        //    using (var cipherReader = new BinaryReader(cipherStream))
        //    {
        //        var nonce = cipherReader.ReadBytes(8);

        //        var cipher = new GcmBlockCipher(new AesEngine());
        //        var parameters = new AeadParameters(new KeyParameter(key), tageLength*8, nonce);
        //        cipher.Init(false, parameters);

        //        var cipherText = cipherReader.ReadBytes(encBlob.Length-nonce.Length);
        //        var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

        //        var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
        //        try
        //        {
        //            cipher.DoFinal(plainText, len);
        //        }
        //        catch (InvalidCipherTextException e)
        //        {
        //            Console.WriteLine(e.ToString());
        //            return null;
        //        }

        //        return plainText;
        //    }
        //}

        static public string EncryptAes(string plainText, string keyString, string ivString)
        {
           var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
           var key = Convert.FromBase64String(keyString);
           var iv = Convert.FromBase64String(ivString); 
           var cipher = new CbcBlockCipher(new AesEngine());
           var parameters = new ParametersWithIV(new KeyParameter(key), iv);
           cipher.Init(true, parameters);

            int blockSize = cipher.GetBlockSize();
            int padding = blockSize - (plainTextBytes.Length % blockSize);
            byte[] paddedInput = new byte[plainTextBytes.Length + padding];
            Array.Copy(plainTextBytes, paddedInput, plainTextBytes.Length);
            for(int i=plainTextBytes.Length; i<paddedInput.Length; i++)
            {
                paddedInput[i] = (byte)padding;
            }
            byte[] cipherText = new byte[paddedInput.Length];
           
           for(int i = 0; i<plainTextBytes.Length; i+=16) 
              cipher.ProcessBlock(paddedInput, i, cipherText, i);
           

           return Convert.ToBase64String(cipherText);
        }

        static public string DecryptAes(string cipherText, string keyString, string ivString)
        {
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var key = Convert.FromBase64String(keyString);
            var iv = Convert.FromBase64String(ivString);
            var cipher = new CbcBlockCipher(new AesEngine());
            var parameters = new ParametersWithIV(new KeyParameter(key), iv);
            cipher.Init(false, parameters);

            var plaintextBytes = new byte[cipherTextBytes.Length];

            for (int i = 0; i < cipherTextBytes.Length; i += 16)
                cipher.ProcessBlock(cipherTextBytes, i, plaintextBytes, i);

            byte paddingValue = plaintextBytes[plaintextBytes.Length - 1];
            int unpaddedLength = cipherTextBytes.Length - paddingValue;
            byte[] unpaddedOutput = new byte[unpaddedLength];
            Array.Copy(plaintextBytes, unpaddedOutput, unpaddedLength);

            return Convert.ToBase64String(unpaddedOutput);
        }
        static public string EncryptWithGCM(string plaintext, byte[] key, byte[] nonce)
        {
            var tagLength = 16;

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var ciphertextTagBytes = new byte[plaintextBytes.Length + tagLength];

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), tagLength * 8, nonce);
            cipher.Init(true, parameters);

            var offset = cipher.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, ciphertextTagBytes, 0);
            cipher.DoFinal(ciphertextTagBytes, offset); // create and append tag: ciphertext | tag

            return Convert.ToBase64String(ciphertextTagBytes);
        }

        public static string DecryptWithGCM(string ciphertextTag, byte[] key, byte[] nonce)
        {
            var tagLength = 16;

            var ciphertextTagBytes = Convert.FromBase64String(ciphertextTag);
            var plaintextBytes = new byte[ciphertextTagBytes.Length - tagLength];

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), tagLength * 8, nonce);
            cipher.Init(false, parameters);

            var offset = cipher.ProcessBytes(ciphertextTagBytes, 0, ciphertextTagBytes.Length, plaintextBytes, 0);
            cipher.DoFinal(plaintextBytes, offset); // authenticate data via tag

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}
