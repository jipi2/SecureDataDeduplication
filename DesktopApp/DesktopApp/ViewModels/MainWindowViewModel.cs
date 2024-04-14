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
            File f1 = new File
            {
                fileName = "fileushabfgjhiabdjhfkbaskjhfbasjbfdkjhasbdjkfbaskjbdfbasjldfbjkahsbdfkjhbasdkjhhfbkajbdf1",
                uploadDate = "2021-10-10"
            };
            File f2 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f3 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f4 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f5 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f6 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f7 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"

            };
            File f8 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f9 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f10 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f11 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f12= new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f13 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            File f14 = new File
            {
                fileName = "fileultim",
                uploadDate = "2021-10-11"
            };
            _files.Add(f1);
            _files.Add(f2);
            _files.Add(f3);
            _files.Add(f4);
            _files.Add(f5);
            _files.Add(f6);
            _files.Add(f7);
            _files.Add(f8);
            _files.Add(f9);
            _files.Add(f10);
            _files.Add(f11);
            _files.Add(f12);
            _files.Add(f13);
            _files.Add(f14);


            for (int i = 0; i < 15; i++)
            {
                TagModel tag = new TagModel
                {
                    tagName = "Tag" + i,
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

        public async Task test() 
        {
            string download = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
            Debug.WriteLine(download);
        }


    }
}
