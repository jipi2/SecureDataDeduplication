﻿using Microsoft.Win32.SafeHandles;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CryptoLib
{
    public class Utils
    {
        static public Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        static public byte[] GenerateRandomBytes(int length)
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
            byte[] h = new byte[hashSize / 8];
            var sha3_256 = new Sha3Digest(hashSize);

            sha3_256.BlockUpdate(array, 0, array.Length);
            sha3_256.DoFinal(h, 0);

            return h;
        }
        static public byte[] GetHashOfFile(FileStream file)
        {

            byte[] h = new byte[64];
            var sha3_512 = new Sha3Digest(512);

            long fileSize = file.Length;
            byte[] buffer = new byte[1048576];

            int bytesRead = 0;
            while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
            {
                sha3_512.BlockUpdate(buffer, 0, bytesRead);
            }

            sha3_512.DoFinal(h, 0);

            return h;
        }

        static private void GetLeaves(ref MerkleTree MT, int count)
        {
            try
            {
                while (count > 1)
                {
                    count = 0;
                    int level = MT.Levels + 1;
                    int n = MT.HashTree.Count;
                    MerkleTree aux = new MerkleTree();


                    for (int i = MT.IndexOfLevel[MT.Levels]; i < n; i += 2)
                    {
                        byte[] h = new byte[32];
                        var sha3_256 = new Sha3Digest(256);
                        sha3_256.BlockUpdate(MT.HashTree[i]._hash, 0, MT.HashTree[i]._hash.Length);
                        sha3_256.BlockUpdate(MT.HashTree[i + 1]._hash, 0, MT.HashTree[i + 1]._hash.Length);
                        sha3_256.DoFinal(h, 0);
                        aux.HashTree.Add(new MTMember(level, h));
                        count++;
                    }
                    foreach (MTMember m in aux.HashTree)
                    {
                        MT.HashTree.Add(m);
                    }
                    if (count % 2 != 0 && count != 1)
                    {
                        MT.HashTree.Add(MT.HashTree[MT.HashTree.Count - 1]);
                        count++;
                    }
                    MT.IndexOfLevel.Add(MT.HashTree.Count - count);
                    MT.Levels++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static public MerkleTree GetMerkleTree(Stream file)
        {
            MerkleTree MT = new MerkleTree();
            long fileSize = file.Length;
            //byte[] buffer = new byte[16384];
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
        static public byte[] GetSeedFromTag(byte[] tag, byte[] secret)
        {
            int len = tag.Length + secret.Length;
            byte[] aux = new byte[len];

            Array.Copy(aux, tag, tag.Length);
            Array.Copy(secret, 0, aux, tag.Length, secret.Length);

            var randomGenerator = new SecureRandom(aux);
            var randomBytes = new byte[4];

            randomGenerator.NextBytes(randomBytes);
            return randomBytes;
        }

        static public byte[] GenerateResp(MerkleTree mt, ref int n1, ref int n2, ref int n3)  //cele 3 pozitii din MT
        {
            int mtNodes = mt.HashTree.Count;
            byte[] Resp = new byte[mt.HashTree[0]._hash.Length];
            byte[] aux = new byte[4];

            var randomGen = new SecureRandom();

            randomGen.NextBytes(aux);
            n1 = Math.Abs(BitConverter.ToInt32(aux, 0) % mtNodes);

            randomGen.NextBytes(aux);
            n2 = Math.Abs(BitConverter.ToInt32(aux, 0) % mtNodes);

            randomGen.NextBytes(aux);
            n3 = Math.Abs(BitConverter.ToInt32(aux, 0) % mtNodes);

            for (int i = 0; i < Resp.Length; i++)
            {
                Resp[i] = (byte)(mt.HashTree[n1]._hash[i] ^ mt.HashTree[n2]._hash[i] ^ mt.HashTree[n3]._hash[i]);
            }

            return Resp;
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
        static public string EncryptAesWithIv(byte[] plainTextBytes, byte[] key, byte[] iv)
        {
            var cipher = new CbcBlockCipher(new AesEngine());
            var parameters = new ParametersWithIV(new KeyParameter(key), iv);
            cipher.Init(true, parameters);

            int blockSize = cipher.GetBlockSize();
            int padding = blockSize - (plainTextBytes.Length % blockSize);
            byte[] paddedInput = new byte[plainTextBytes.Length + padding];
            Array.Copy(plainTextBytes, paddedInput, plainTextBytes.Length);
            for (int i = plainTextBytes.Length; i < paddedInput.Length; i++)
            {
                paddedInput[i] = (byte)padding;
            }
            byte[] cipherText = new byte[paddedInput.Length];

            for (int i = 0; i < plainTextBytes.Length; i += 16)
                cipher.ProcessBlock(paddedInput, i, cipherText, i);


            return Convert.ToBase64String(cipherText);
        }

        static public string DecryptAesWithIv(string cipherText, byte[] key, byte[] iv)
        {
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            /*  var key = Convert.FromBase64String(keyString);*/
            /* var iv = Convert.FromBase64String(ivString)*/

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
        static public string EncryptAes(byte[] plainTextBytes, byte[] key, byte[] iv = null)
        {
            //var cipher = new CbcBlockCipher(new AesEngine());
            //var parameters = new KeyParameter(key);
            //cipher.Init(true, parameters);

            // int blockSize = cipher.GetBlockSize();
            // int padding = blockSize - (plainTextBytes.Length % blockSize);
            // byte[] paddedInput = new byte[plainTextBytes.Length + padding];
            // Array.Copy(plainTextBytes, paddedInput, plainTextBytes.Length);
            // for(int i=plainTextBytes.Length; i<paddedInput.Length; i++)
            // {
            //     paddedInput[i] = (byte)padding;
            // }
            // byte[] cipherText = new byte[paddedInput.Length];

            //for(int i = 0; i<plainTextBytes.Length; i+=16) 
            //   cipher.ProcessBlock(paddedInput, i, cipherText, i);


            //return Convert.ToBase64String(cipherText);
            try
            {
                var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
                if (iv == null)
                    cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), new byte[16]));
                else
                    cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

                var cipherTextBytes = cipher.DoFinal(plainTextBytes);
                return Convert.ToBase64String(cipherTextBytes);
            }
            catch (CryptoException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        static public string DecryptAes(byte[] cipherText, byte[] key, byte[] iv = null)
        {
            //  var cipherTextBytes = Convert.FromBase64String(cipherText);
            ///*  var key = Convert.FromBase64String(keyString);*/
            // /* var iv = Convert.FromBase64String(ivString)*/;
            //  var cipher = new CbcBlockCipher(new AesEngine());
            //  var parameters = new KeyParameter(key);
            //  cipher.Init(false, parameters);

            //  var plaintextBytes = new byte[cipherTextBytes.Length];

            //  for (int i = 0; i < cipherTextBytes.Length; i += 16)
            //      cipher.ProcessBlock(cipherTextBytes, i, plaintextBytes, i);

            //  byte paddingValue = plaintextBytes[plaintextBytes.Length - 1];
            //  int unpaddedLength = cipherTextBytes.Length - paddingValue;
            //  byte[] unpaddedOutput = new byte[unpaddedLength];
            //  Array.Copy(plaintextBytes, unpaddedOutput, unpaddedLength);

            //  return Convert.ToBase64String(unpaddedOutput);

            try
            {
                var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
                if (iv == null)
                    cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), new byte[16]));
                else
                    cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

                var plainTextBytes = cipher.DoFinal(cipherText);
                return Convert.ToBase64String(plainTextBytes);
            }
            catch (CryptoException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        //static public string EncryptWithGCM(string plaintext, byte[] key/*, byte[] nonce*/)
        //{
        //    try 
        //    { 
        //        var tagLength = 16;

        //        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        //        var ciphertextTagBytes = new byte[plaintextBytes.Length + tagLength];

        //        var cipher = new GcmBlockCipher(new AesEngine());
        //        var parameters = new AeadParameters(new KeyParameter(key), tagLength * 8, new byte[8]);  //al treilea parametru trebuia sa fie nonce
        //        cipher.Init(true, parameters);

        //        var offset = cipher.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, ciphertextTagBytes, 0);
        //        cipher.DoFinal(ciphertextTagBytes, offset); // create and append tag: ciphertext | tag

        //        return Convert.ToBase64String(ciphertextTagBytes);
        //    }
        //    catch (InvalidCipherTextException e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        return null;
        //    }
        //}

        //public static string DecryptWithGCM(string ciphertextTag, byte[] key/*, byte[] nonce*/)
        //{
        //    try
        //    {
        //        var tagLength = 16;

        //        var ciphertextTagBytes = Convert.FromBase64String(ciphertextTag);
        //        var plaintextBytes = new byte[ciphertextTagBytes.Length - tagLength];

        //        var cipher = new GcmBlockCipher(new AesEngine());
        //        var parameters = new AeadParameters(new KeyParameter(key), tagLength * 8, new byte[8]); //al treilea parametru trebuia sa fie nonce
        //        cipher.Init(false, parameters);

        //        var offset = cipher.ProcessBytes(ciphertextTagBytes, 0, ciphertextTagBytes.Length, plaintextBytes, 0);
        //        cipher.DoFinal(plaintextBytes, offset); // authenticate data via tag

        //        return Convert.ToBase64String(plaintextBytes);
        //    }
        //    catch (InvalidCipherTextException e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        return null;
        //    }
        //}

        static public byte[] GetDerEncodedRSAPrivateKey(AsymmetricCipherKeyPair keyPair)
        {
            try
            {
                RsaPrivateCrtKeyParameters rsaPrivateKeyParams = (RsaPrivateCrtKeyParameters)keyPair.Private;
                PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(rsaPrivateKeyParams);
                return privateKeyInfo.ToAsn1Object().GetDerEncoded();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static public string GetBase64RSAPublicKey(AsymmetricCipherKeyPair keyPair)
        {
            try
            {
                SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
                byte[] publicKeyBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
                return Convert.ToBase64String(publicKeyBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static public byte[] EncryptRSAwithPublicKey(byte[] plaintextbytes, string base64publicKey)
        {
            try
            {
                AsymmetricKeyParameter pubKeyParameter = PublicKeyFactory.CreateKey(Convert.FromBase64String(base64publicKey));
                IAsymmetricBlockCipher cipher = new RsaEngine();
                cipher.Init(true, pubKeyParameter);
                byte[] cipherBytes = cipher.ProcessBlock(plaintextbytes, 0, plaintextbytes.Length);
                return cipherBytes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

        static public byte[] DecryptRSAwithPrivateKey(byte[] ciphertextbytes, byte[] derEncodedPrivateKey)
        {
            try
            {
                RsaKeyParameters privateKeyParams2 = (RsaKeyParameters)PrivateKeyFactory.CreateKey(derEncodedPrivateKey);
                IAsymmetricBlockCipher cipher = new RsaEngine();
                cipher.Init(false, privateKeyParams2);
                byte[] plaintextbytes = cipher.ProcessBlock(ciphertextbytes, 0, ciphertextbytes.Length);
                return plaintextbytes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static public byte[] EncryptAndSaveFileWithAesGCM_AndGetTag(string encFileName,FileStream filstream, byte[] key, byte[] iv)
        {
            byte[] h = new byte[32];
            var sha3_256 = new Sha3Digest(256);

            var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
            cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            string encryptedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, encFileName);
            Debug.WriteLine(encryptedFilePath);

            using (FileStream encryptedFileStream = File.Create(encryptedFilePath))
            {
                byte[] buffer = new byte[1048576];
                int bytesRead;

                while ((bytesRead = filstream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] encryptedBlock = cipher.ProcessBytes(buffer, 0, bytesRead);
                    encryptedFileStream.Write(encryptedBlock, 0, encryptedBlock.Length);
                    sha3_256.BlockUpdate(encryptedBlock, 0, encryptedBlock.Length);
                }
                byte[] finalBlock = cipher.DoFinal(); // Generate authentication tag
                encryptedFileStream.Write(finalBlock, 0, finalBlock.Length); // Append authentication tag to the ciphertext
                sha3_256.BlockUpdate(finalBlock, 0, finalBlock.Length);
                sha3_256.DoFinal(h, 0);

            }
            return h;
        }

        static public void DecryptAndSaveFileWithAesGCM(FileStream filstream, string decFilePath, byte[] key, byte[] iv)
        {
            var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
            cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));
            using (FileStream decFileStream = File.Create(decFilePath))
            {
                byte[] buffer = new byte[1048576];
                int bytesRead;
                while ((bytesRead = filstream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] decryptedBlock = cipher.ProcessBytes(buffer, 0, bytesRead);
                    decFileStream.Write(decryptedBlock, 0, decryptedBlock.Length);
                }
                byte[] finalBlock = cipher.DoFinal();
                decFileStream.Write(finalBlock, 0, finalBlock.Length);
            }
        }

        //STACKOVERFLOW CODE START
        private static X509Certificate2 ImportExportable(byte[] pfxBytes, string password, bool machineScope)
        {
            X509KeyStorageFlags flags = X509KeyStorageFlags.Exportable;

            if (machineScope)
            {
                flags |= X509KeyStorageFlags.MachineKeySet;
            }
            else
            {
                flags |= X509KeyStorageFlags.UserKeySet;
            }

            X509Certificate2 cert = new X509Certificate2(pfxBytes, password, flags);

            try
            {
                bool gotKey = NativeMethods.Crypt32.CryptAcquireCertificatePrivateKey(
                    cert.Handle,
                    NativeMethods.Crypt32.AcquireCertificateKeyOptions.CRYPT_ACQUIRE_ONLY_NCRYPT_KEY_FLAG,
                    IntPtr.Zero,
                    out SafeNCryptKeyHandle keyHandle,
                    out int keySpec,
                    out bool callerFree);

                if (!gotKey)
                {
                    keyHandle.Dispose();
                    throw new InvalidOperationException("No private key");
                }

                if (!callerFree)
                {
                    keyHandle.SetHandleAsInvalid();
                    keyHandle.Dispose();
                    throw new InvalidOperationException("Key is not persisted");
                }

                using (keyHandle)
                {
                    // -1 == CNG, otherwise CAPI
                    if (keySpec == -1)
                    {
                        using (CngKey cngKey = CngKey.Open(keyHandle, CngKeyHandleOpenOptions.None))
                        {
                            // If the CNG->CAPI bridge opened the key then AllowPlaintextExport is already set.
                            if ((cngKey.ExportPolicy & CngExportPolicies.AllowPlaintextExport) == 0)
                            {
                                FixExportability(cngKey, machineScope);
                            }
                        }
                    }
                }
            }
            catch
            {
                cert.Reset();
                throw;
            }

            return cert;
        }

        internal static void FixExportability(CngKey cngKey, bool machineScope)
        {
            string password = nameof(NativeMethods.Crypt32.AcquireCertificateKeyOptions);
            byte[] encryptedPkcs8 = ExportEncryptedPkcs8(cngKey, password, 1);
            string keyName = cngKey.KeyName;

            using (SafeNCryptProviderHandle provHandle = cngKey.ProviderHandle)
            {
                ImportEncryptedPkcs8Overwrite(
                    encryptedPkcs8,
                    keyName,
                    provHandle,
                    machineScope,
                    password);
            }
        }

        internal const string NCRYPT_PKCS8_PRIVATE_KEY_BLOB = "PKCS8_PRIVATEKEY";
        private static readonly byte[] s_pkcs12TripleDesOidBytes =
            System.Text.Encoding.ASCII.GetBytes("1.2.840.113549.1.12.1.3\0");

        private static unsafe byte[] ExportEncryptedPkcs8(
            CngKey cngKey,
            string password,
            int kdfCount)
        {
            var pbeParams = new NativeMethods.NCrypt.PbeParams();
            NativeMethods.NCrypt.PbeParams* pbeParamsPtr = &pbeParams;

            byte[] salt = new byte[NativeMethods.NCrypt.PbeParams.RgbSaltSize];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            pbeParams.Params.cbSalt = salt.Length;
            Marshal.Copy(salt, 0, (IntPtr)pbeParams.rgbSalt, salt.Length);
            pbeParams.Params.iIterations = kdfCount;

            fixed (char* stringPtr = password)
            fixed (byte* oidPtr = s_pkcs12TripleDesOidBytes)
            {
                NativeMethods.NCrypt.NCryptBuffer* buffers =
                    stackalloc NativeMethods.NCrypt.NCryptBuffer[3];

                buffers[0] = new NativeMethods.NCrypt.NCryptBuffer
                {
                    BufferType = NativeMethods.NCrypt.BufferType.PkcsSecret,
                    cbBuffer = checked(2 * (password.Length + 1)),
                    pvBuffer = (IntPtr)stringPtr,
                };

                if (buffers[0].pvBuffer == IntPtr.Zero)
                {
                    buffers[0].cbBuffer = 0;
                }

                buffers[1] = new NativeMethods.NCrypt.NCryptBuffer
                {
                    BufferType = NativeMethods.NCrypt.BufferType.PkcsAlgOid,
                    cbBuffer = s_pkcs12TripleDesOidBytes.Length,
                    pvBuffer = (IntPtr)oidPtr,
                };

                buffers[2] = new NativeMethods.NCrypt.NCryptBuffer
                {
                    BufferType = NativeMethods.NCrypt.BufferType.PkcsAlgParam,
                    cbBuffer = sizeof(NativeMethods.NCrypt.PbeParams),
                    pvBuffer = (IntPtr)pbeParamsPtr,
                };

                var desc = new NativeMethods.NCrypt.NCryptBufferDesc
                {
                    cBuffers = 3,
                    pBuffers = (IntPtr)buffers,
                    ulVersion = 0,
                };

                using (var keyHandle = cngKey.Handle)
                {
                    int result = NativeMethods.NCrypt.NCryptExportKey(
                        keyHandle,
                        IntPtr.Zero,
                        NCRYPT_PKCS8_PRIVATE_KEY_BLOB,
                        ref desc,
                        null,
                        0,
                        out int bytesNeeded,
                        0);

                    if (result != 0)
                    {
                        throw new Win32Exception(result);
                    }

                    byte[] exported = new byte[bytesNeeded];

                    result = NativeMethods.NCrypt.NCryptExportKey(
                        keyHandle,
                        IntPtr.Zero,
                        NCRYPT_PKCS8_PRIVATE_KEY_BLOB,
                        ref desc,
                        exported,
                        exported.Length,
                        out bytesNeeded,
                        0);

                    if (result != 0)
                    {
                        throw new Win32Exception(result);
                    }

                    if (bytesNeeded != exported.Length)
                    {
                        Array.Resize(ref exported, bytesNeeded);
                    }

                    return exported;
                }
            }
        }

        private static unsafe void ImportEncryptedPkcs8Overwrite(
                                                byte[] encryptedPkcs8,
                                                string keyName,
                                                SafeNCryptProviderHandle provHandle,
                                                bool machineScope,
                                                string password)
        {
            SafeNCryptKeyHandle keyHandle;

            fixed (char* passwordPtr = password)
            fixed (char* keyNamePtr = keyName)
            fixed (byte* blobPtr = encryptedPkcs8)
            {
                NativeMethods.NCrypt.NCryptBuffer* buffers = stackalloc NativeMethods.NCrypt.NCryptBuffer[2];

                buffers[0] = new NativeMethods.NCrypt.NCryptBuffer
                {
                    BufferType = NativeMethods.NCrypt.BufferType.PkcsSecret,
                    cbBuffer = checked(2 * (password.Length + 1)),
                    pvBuffer = new IntPtr(passwordPtr),
                };

                if (buffers[0].pvBuffer == IntPtr.Zero)
                {
                    buffers[0].cbBuffer = 0;
                }

                buffers[1] = new NativeMethods.NCrypt.NCryptBuffer
                {
                    BufferType = NativeMethods.NCrypt.BufferType.PkcsName,
                    cbBuffer = checked(2 * (keyName.Length + 1)),
                    pvBuffer = new IntPtr(keyNamePtr),
                };

                NativeMethods.NCrypt.NCryptBufferDesc desc = new NativeMethods.NCrypt.NCryptBufferDesc
                {
                    cBuffers = 2,
                    pBuffers = (IntPtr)buffers,
                    ulVersion = 0,
                };

                NativeMethods.NCrypt.NCryptImportFlags flags =
                    NativeMethods.NCrypt.NCryptImportFlags.NCRYPT_OVERWRITE_KEY_FLAG |
                    NativeMethods.NCrypt.NCryptImportFlags.NCRYPT_DO_NOT_FINALIZE_FLAG;

                if (machineScope)
                {
                    flags |= NativeMethods.NCrypt.NCryptImportFlags.NCRYPT_MACHINE_KEY_FLAG;
                }

                int errorCode = NativeMethods.NCrypt.NCryptImportKey(
                    provHandle,
                    IntPtr.Zero,
                    NCRYPT_PKCS8_PRIVATE_KEY_BLOB,
                    ref desc,
                    out keyHandle,
                    new IntPtr(blobPtr),
                    encryptedPkcs8.Length,
                    flags);

                if (errorCode != 0)
                {
                    keyHandle.Dispose();
                    throw new Win32Exception(errorCode);
                }

                using (keyHandle)
                using (CngKey cngKey = CngKey.Open(keyHandle, CngKeyHandleOpenOptions.None))
                {
                    const CngExportPolicies desiredPolicies =
                        CngExportPolicies.AllowExport | CngExportPolicies.AllowPlaintextExport;

                    cngKey.SetProperty(
                        new CngProperty(
                            "Export Policy",
                            BitConverter.GetBytes((int)desiredPolicies),
                            CngPropertyOptions.Persist));

                    int error = NativeMethods.NCrypt.NCryptFinalizeKey(keyHandle, 0);

                    if (error != 0)
                    {
                        throw new Win32Exception(error);
                    }
                }
            }
        }
        internal static class NativeMethods
        {
            internal static class Crypt32
            {
                internal enum AcquireCertificateKeyOptions
                {
                    None = 0x00000000,
                    CRYPT_ACQUIRE_ONLY_NCRYPT_KEY_FLAG = 0x00040000,
                }

                [DllImport("crypt32.dll", SetLastError = true)]
                internal static extern bool CryptAcquireCertificatePrivateKey(
                    IntPtr pCert,
                    AcquireCertificateKeyOptions dwFlags,
                    IntPtr pvReserved,
                    out SafeNCryptKeyHandle phCryptProvOrNCryptKey,
                    out int dwKeySpec,
                    out bool pfCallerFreeProvOrNCryptKey);
            }

            internal static class NCrypt
            {
                [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
                internal static extern int NCryptExportKey(
                    SafeNCryptKeyHandle hKey,
                    IntPtr hExportKey,
                    string pszBlobType,
                    ref NCryptBufferDesc pParameterList,
                    byte[] pbOutput,
                    int cbOutput,
                    [Out] out int pcbResult,
                    int dwFlags);

                [StructLayout(LayoutKind.Sequential)]
                internal unsafe struct PbeParams
                {
                    internal const int RgbSaltSize = 8;

                    internal CryptPkcs12PbeParams Params;
                    internal fixed byte rgbSalt[RgbSaltSize];
                }

                [StructLayout(LayoutKind.Sequential)]
                internal struct CryptPkcs12PbeParams
                {
                    internal int iIterations;
                    internal int cbSalt;
                }

                [StructLayout(LayoutKind.Sequential)]
                internal struct NCryptBufferDesc
                {
                    public int ulVersion;
                    public int cBuffers;
                    public IntPtr pBuffers;
                }

                [StructLayout(LayoutKind.Sequential)]
                internal struct NCryptBuffer
                {
                    public int cbBuffer;
                    public BufferType BufferType;
                    public IntPtr pvBuffer;
                }

                internal enum BufferType
                {
                    PkcsAlgOid = 41,
                    PkcsAlgParam = 42,
                    PkcsName = 45,
                    PkcsSecret = 46,
                }

                [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
                internal static extern int NCryptOpenStorageProvider(
                    out SafeNCryptProviderHandle phProvider,
                    string pszProviderName,
                    int dwFlags);

                internal enum NCryptImportFlags
                {
                    None = 0,
                    NCRYPT_MACHINE_KEY_FLAG = 0x00000020,
                    NCRYPT_OVERWRITE_KEY_FLAG = 0x00000080,
                    NCRYPT_DO_NOT_FINALIZE_FLAG = 0x00000400,
                }

                [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
                internal static extern int NCryptImportKey(
                    SafeNCryptProviderHandle hProvider,
                    IntPtr hImportKey,
                    string pszBlobType,
                    ref NCryptBufferDesc pParameterList,
                    out SafeNCryptKeyHandle phKey,
                    IntPtr pbData,
                    int cbData,
                    NCryptImportFlags dwFlags);

                [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
                internal static extern int NCryptFinalizeKey(SafeNCryptKeyHandle hKey, int dwFlags);
            }
        }
        //STACKOVERFLOW CODE END

        public static byte[]? GenerateRSAandECDSAInTheSameTime(string password, out byte[] privateBytes, out string base64RSAPubKey)
        {
            try
            {
                using (ECDsa ec = ECDsa.Create(ECCurve.CreateFromFriendlyName("secP256k1")))
                using (RSA rsa = RSA.Create())
                {
                    System.Security.Cryptography.X509Certificates.CertificateRequest certRequestRsa = new System.Security.Cryptography.X509Certificates.CertificateRequest("CN=RsaCert", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                    // Create a self-signed X.509 certificate
                    X509Certificate2 certificateRsa = certRequestRsa.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

                    // Create a collection to hold the certificate and private key
                    X509Certificate2Collection certCollection = new X509Certificate2Collection();
                    certCollection.Add(certificateRsa);

                    System.Security.Cryptography.X509Certificates.CertificateRequest certRequestEC = new System.Security.Cryptography.X509Certificates.CertificateRequest("CN=EcCert", ec, HashAlgorithmName.SHA256);


                    X509Certificate2 certificateEC = certRequestEC.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

                    // Add the EC certificate to the collection
                    certCollection.Add(certificateEC);

                    // Export the certificates with private keys into a new PKCS#12 file
                    byte[] pkcs12Bytes = certCollection.Export(X509ContentType.Pkcs12, password);

                    ECParameters eCParameters = ec.ExportParameters(true);
                    privateBytes = eCParameters.D;
                    
                    base64RSAPubKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());


                    return pkcs12Bytes;
                }
               

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                privateBytes = new byte[0];
                base64RSAPubKey = "";
                return null;
            }
        }

        public static void ReadKeysFromPkcs12(byte[] pkcs12file,string password, out string? eccPrivateKeyBase64,
                                              out RSA? rsaPrivateKey, out RSA? rsaPublicKey)
        {
            try
            {
                X509Certificate2Collection certificates = new X509Certificate2Collection();
                certificates.Import(pkcs12file, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

                X509Certificate2 rsaCertificate = certificates[0];
                rsaPrivateKey = rsaCertificate.GetRSAPrivateKey();
                rsaPublicKey = rsaCertificate.GetRSAPublicKey();

                X509Certificate2 ecCertificate = certificates[1];
                ecCertificate = ImportExportable(ecCertificate.Export(X509ContentType.Pkcs12, password), password, false);
                ECDsa ecPrivateKey = ecCertificate.GetECDsaPrivateKey();
                ECParameters eCParameters = ecPrivateKey.ExportParameters(true);
                byte[] privateKeyBytes = eCParameters.D;
                eccPrivateKeyBase64 = Convert.ToBase64String(privateKeyBytes);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                eccPrivateKeyBase64 = null;
                rsaPrivateKey = null;
                rsaPublicKey = null;
            }
        }

        public static void ChangePkcs12Password(byte[] pkcs12file, string oldPassword, string newPassword, out byte[]? newPkcs12File)
        {
            try
            {
                X509Certificate2Collection certificates = new X509Certificate2Collection();
                certificates.Import(pkcs12file, oldPassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

                //X509Certificate2 rsaCertificate = certificates[0];
                //RSA rsaPrivateKey = rsaCertificate.GetRSAPrivateKey();

                //X509Certificate2 ecCertificate = certificates[1];
                //ECDsa ecPrivateKey = ecCertificate.GetECDsaPrivateKey();

                //certificates.Clear();

                //System.Security.Cryptography.X509Certificates.CertificateRequest certRequestRsa = new System.Security.Cryptography.X509Certificates.CertificateRequest("CN=RsaCert", rsaPrivateKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                //X509Certificate2 certificateRsa = certRequestRsa.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

                //System.Security.Cryptography.X509Certificates.CertificateRequest certRequestEC = new System.Security.Cryptography.X509Certificates.CertificateRequest("CN=EcCert", ecPrivateKey, HashAlgorithmName.SHA256);
                //X509Certificate2 certificateEC = certRequestEC.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

                //certificates.Add(certificateRsa);
                //certificates.Add(certificateEC);

                newPkcs12File = certificates.Export(X509ContentType.Pkcs12, newPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                newPkcs12File = null;
            }
        }
    }
}
