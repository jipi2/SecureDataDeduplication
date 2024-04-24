using CryptoLib;
using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using DesktopApp.Models;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.ViewModels
{
    public class MainWindowViewModel :INotifyPropertyChanged
    {

        public class File
        {
            public string fileName { get; set; }
            public string uploadDate { get; set; }
        }

        public MainWindowViewModel()
        {
            _files = new ObservableCollection<File>();
            _tags = new ObservableCollection<TagModel>();
            _recFiles = new ObservableCollection<RecievedFilesDto>();
        }

        private ObservableCollection<RecievedFilesDto> _recFiles;
        public ObservableCollection<RecievedFilesDto> RecFiles
        {
            get { return _recFiles; }
            set
            {
                if (_recFiles != value)
                {
                    _recFiles = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<TagModel> _tags;
        public ObservableCollection<TagModel> Tags
        {
            get { return _tags; }
            set
            {
                if (_tags != value)
                {
                    _tags = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<File> _files ;
        public ObservableCollection<File> Files
        {
            get { return _files; }
            set
            {
                if (_files != value)
                {
                    _files = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Test()
        {
 

            for (int i = 0; i < 15; i++)
            {
                File f = new File
                {
                    fileName = "file" + i,
                    uploadDate = "2021-10-1" + i
                };
                _files.Add(f);
            }


            for (int i = 0; i < 15; i++)
            {
                TagModel tag = new TagModel
                {
                    tagName = "Characteristic" + i,
                    isChecked = false
                };
                _tags.Add(tag);
            }

        }

        public async Task GetFilesAndNames()
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            List<FilesNameDate>? userFileNames;

            if (jwt == null)
            {
                throw new Exception("Your session expired!");
            }
            var httpClient = HttpServiceCustom.GetProxyClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            userFileNames = await httpClient.GetFromJsonAsync<List<FilesNameDate>?>("getFileNamesAndDates");
            _files.Clear();
            if (userFileNames != null)
            {
                foreach (var file in userFileNames)
                {
                    File f = new File
                    {
                        fileName = file.FileName,
                        uploadDate = file.UploadDate.ToString()
                    };
                    _files.Add(f);
                }
            }

        }

        public async Task DownloadFile(string fileName) //ceva nu merge cu stersul fisiereulor
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetProxyClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            var response = await httpClient.GetStreamAsync("getFileFromStorage/?filename=" + fileName);
            //var response = await httpClient.GetStreamAsync("testBACK");
            string saveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName + "_enc");
            Debug.WriteLine(saveFilePath);
            if (response != null && response.CanRead)
            {
                using (FileStream fs = new FileStream(saveFilePath, FileMode.OpenOrCreate))
                {
                    await response.CopyToAsync(fs);
                }

                Debug.WriteLine("File downloaded successfully");
            }


            httpClient = HttpServiceCustom.GetProxyClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
            FileKeyAndIvDto? keyAndIvDto = await httpClient.GetFromJsonAsync<FileKeyAndIvDto>("getKeyAndIvForFile/?filename=" + fileName);

            Debug.WriteLine(keyAndIvDto.base64key);
            Debug.WriteLine(keyAndIvDto.base64iv);

            string downloadFolder = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
            string filePath = Path.Combine(downloadFolder, fileName);
            var encFileStream = System.IO.File.OpenRead(saveFilePath);

            Utils.DecryptAndSaveFileWithAesGCM(encFileStream, filePath, Convert.FromBase64String(keyAndIvDto.base64key), Convert.FromBase64String(keyAndIvDto.base64iv));
            encFileStream.Close();
            System.IO.File.Delete(saveFilePath);
        }

        private string getBase64CapsuleAndCiphertext(string base64privKey, string base64plaintext)
        {
            string concatStr = "";
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                string code = @"
from umbral import SecretKey, Signer, PublicKey, encrypt
import base64

def encrypt_umbral(base64PrivKey, base64text):
    privKey = SecretKey.from_bytes(base64.b64decode(base64PrivKey))
    pubKey = privKey.public_key()
    plaintext = base64.b64decode(base64text)
    capsule, ciphertext = encrypt(pubKey, plaintext)
    base64Capsule = base64.b64encode(capsule.__bytes__()).decode('utf-8')
    base64Ciphertext = base64.b64encode(ciphertext).decode('utf-8')
    return base64Capsule+""#""+base64Ciphertext
";
                dynamic result = PyModule.FromString("encrypt_umbral", code);
                concatStr = result.encrypt_umbral(base64privKey, base64plaintext);
                return concatStr;
            }
        }

        private async Task<string> getKfrag(string destEmail)
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            var response = await httpClient.PostAsJsonAsync("/api/User/getKfragFromReciever", destEmail);
            if (response.IsSuccessStatusCode)
            {
                string kfrag = await response.Content.ReadAsStringAsync();
                return kfrag;
            }
            else
            {
                throw new Exception("Could not get kfrag");
            }
        }

        private async Task GenerateAndSaveKFrag(string destEmail)
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            //luam cheia privata a senderului
            string email = await SecureStorage.GetAsync(Enums.Symbol.Email.ToString());
            string base64PrivKey = System.IO.File.ReadAllText("D:\\LicentaProiect\\DesktopApp" + "\\" + email + "_privateKey.priv");

            var response = await httpClient.PostAsJsonAsync("/api/User/getPubKeyForFileTransfer", destEmail);
            if (response.IsSuccessStatusCode)
            {
                string base64PubKey = await response.Content.ReadAsStringAsync();
                string base64KFrag = "";
                PythonEngine.Initialize();
                using (Py.GIL())
                {
                    string code = @"
from umbral import SecretKey, Signer, encrypt, decrypt_original, generate_kfrags, reencrypt, decrypt_reencrypted, VerifiedCapsuleFrag, PublicKey
import base64

def generateKfrag(base64PrivKey, base64PubKey):
    user_signing_key = SecretKey.random()
    user_signer = Signer(user_signing_key)
    privKey = SecretKey.from_bytes(base64.b64decode(base64PrivKey))
    pubKey = PublicKey.from_bytes(base64.b64decode(base64PubKey))
    kfrags = generate_kfrags(delegating_sk=privKey,
                         receiving_pk=pubKey,
                         signer=user_signer,
                         threshold=1,
                         shares=1)
    kfrag = kfrags[0]
    return base64.b64encode(kfrag.__bytes__()).decode('utf-8')
";                    
                    dynamic result = PyModule.FromString("generateKfrag", code);
                    base64KFrag = result.generateKfrag(base64PrivKey, base64PubKey);
                }

                var resp2 = await httpClient.PostAsJsonAsync("/api/User/saveKFragForReceiver", new KFragDto { base64kfrag = base64KFrag, destEmail = destEmail });
                if (!resp2.IsSuccessStatusCode)
                {
                    throw new Exception("Could not save kfrag");
                }
            }
            else
            {
                throw new Exception("Could not get kfrag");
            }
        }
        public async Task SendFile(string fileName, string destEmail)
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetProxyClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            FileKeyAndIvDto? keyAndIvDto = await httpClient.GetFromJsonAsync<FileKeyAndIvDto>("getKeyAndIvForFile/?filename=" + fileName);
/*
            byte[] fileKey = Convert.FromBase64String(keyAndIvDto.base64key);
            byte[] fileIv = Convert.FromBase64String(keyAndIvDto.base64iv);*/

            string userEmail = await SecureStorage.GetAsync(Enums.Symbol.Email.ToString());
            string base64sendkey = System.IO.File.ReadAllText("D:\\LicentaProiect\\DesktopApp" + "\\" + userEmail + "_privateKey.priv");

            string base64KeyCapsule = getBase64CapsuleAndCiphertext(base64sendkey, keyAndIvDto.base64key);
            string base64IvCapsule = getBase64CapsuleAndCiphertext(base64sendkey, keyAndIvDto.base64iv);

            string base64kfrag = await getKfrag(destEmail);
            if (base64kfrag == "")
            {
                await GenerateAndSaveKFrag(destEmail);
            }

            CapsuleDto dto = new CapsuleDto
            {
                base64KeyCapsule = base64KeyCapsule,
                base64IvCapsule = base64IvCapsule,
                fileName = fileName,
                destEmail = destEmail
            };

            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
            var response = await httpClient.PostAsJsonAsync("sendFile", dto);
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("File sent successfully");
            }
            else
            {
                throw new Exception("Could not send file");
            }
        }
        public async Task<int> GetReceivedFilesFromUsers()
        {
            //Curatam _recFile
            _recFiles.Clear();


            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
            List<RecievedFilesDto> recFiles = await httpClient.GetFromJsonAsync<List<RecievedFilesDto>>("/api/File/getRecievedFiles");
            if (recFiles.Count == 0)
            {
                return 0;
            }
            foreach (RecievedFilesDto rf in recFiles)
            {
                _recFiles.Add(rf);
            }


            //for(int i=0;i<15;i++) 
            //{
            //    _recFiles.Add(
            //        new RecievedFilesDto
            //        {
            //            fileName = "file" + i,
            //            senderEmail = "sender" + i,
            //            base64EncIv = "base64EncIv" + i,
            //            base64EncKey = "base64EncKey" + i
            //        }
            //        );
            //}
            return _recFiles.Count;

        }

        private string decryptKFrag(string base64CFrag, string base64PrivKey, string base64PubKey)
        {
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                string code = @"
from umbral import SecretKey, Signer, PublicKey, encrypt, generate_kfrags, VerifiedCapsuleFrag, Capsule, decrypt_reencrypted, VerifiedKeyFrag, reencrypt
import base64

def decryptCapsule(base64PrivKey, base64PubKey, base64CFrag):
    privKey = SecretKey.from_bytes(base64.b64decode(base64PrivKey))
    pubKey = PublicKey.from_bytes(base64.b64decode(base64PubKey))
    
    split_str = base64CFrag.split(""#"")
    
    cfrag_bytes = base64.b64decode(split_str[0])
    capsule_bytes = base64.b64decode(split_str[1])
    ciphertext = base64.b64decode(split_str[2])
    
    cfrags = []
    cfrag = VerifiedCapsuleFrag.from_verified_bytes(cfrag_bytes.__bytes__())
    cfrags.append(cfrag)
    capsule = Capsule.from_bytes(capsule_bytes)
    plaintext = decrypt_reencrypted(receiving_sk=privKey,
                                        delegating_pk=pubKey,
                                        capsule=capsule,
                                        verified_cfrags=cfrags,
                                        ciphertext=ciphertext)
    
    return base64.b64encode(plaintext).decode('utf-8')
";
                dynamic result = PyModule.FromString("decryptCapsule", code);
                string base64plaintext = result.decryptCapsule(base64PrivKey, base64PubKey, base64CFrag);
                return base64plaintext;
            }
        }

        public async Task AcceptReceivedFile(RecievedFilesDto fileInf)
        {
            string email = await SecureStorage.GetAsync(Enums.Symbol.Email.ToString());
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
            var response = await httpClient.PostAsJsonAsync("/api/User/getPubKeyForFileTransfer", fileInf.senderEmail);
            if (response.IsSuccessStatusCode)
            {
                string base64PubKey = await response.Content.ReadAsStringAsync();
                string base64PrivKey = System.IO.File.ReadAllText("D:\\LicentaProiect\\DesktopApp" + "\\" + email + "_privateKey.priv");
                string base64key = decryptKFrag(fileInf.base64EncKey, base64PrivKey, base64PubKey);
                string base64iv = decryptKFrag(fileInf.base64EncIv, base64PrivKey, base64PubKey);

                AcceptFileTransferDto dto = new AcceptFileTransferDto
                {
                    senderEmail = fileInf.senderEmail,
                    fileName = fileInf.fileName,
                    base64FileKey = base64key,
                    base64FileIv = base64iv
                };

                var response2 = await httpClient.PostAsJsonAsync("/api/File/acceptRecievedFile", dto);
                if (response2.IsSuccessStatusCode)
                {
                    Debug.WriteLine("File accepted successfully");
                    _recFiles.Remove(fileInf);
                }
                else
                {
                    throw new Exception("Could not accept file");
                }
            }
            else
            {
                throw new Exception("Could not get public key");
            }
        }

        public async Task RemoveReceivedFile(RecievedFilesDto fileInf)
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            try
            {
                var response = await httpClient.PostAsJsonAsync("/api/File/removeRecievedFile", fileInf);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("The file could not be removed");
                }
                _recFiles.Remove(fileInf);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task test() 
        {
            string download = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
            Debug.WriteLine(download);
        }

        public  void GetTags()
        {
            for (int i = 0; i < 15; i++)
            {
                TagModel tag = new TagModel
                {
                    tagName = "Tag_" + i,
                    isChecked = false
                };
                _tags.Add(tag);
            }
        }

        public async Task DeleteFile(string fileName)
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetProxyClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            var response = await httpClient.PostAsJsonAsync("deleteFile", fileName);
            if(response.IsSuccessStatusCode)
            {
                Debug.WriteLine("File deleted successfully");
                _files.Remove(_files.FirstOrDefault(f => f.fileName == fileName));
            }
            else
            {
                throw new Exception("Could not delete file");
            }

            await GetFilesAndNames();
        }
    }
}
