using CommunityToolkit.Maui.Core.Primitives;
using CryptoLib;
using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using DesktopApp.KeysService;
using DesktopApp.Models;
using FileStorageApp.Shared.Dto;
using Python.Runtime;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Folder = DesktopApp.Models.Folder;


namespace DesktopApp.ViewModels
{
    public class MainWindowViewModel : BindableObject, INotifyPropertyChanged
    {
        private int dateSort = 0;
        private int sizeSort = 0;
        private List<LabelFileNames> _labelFileNames;
        private List<Folder> _foldersList;
        private bool isFirst = true;
        public string PrevFolder = "";
        public ObservableCollection<SimpleFileHierarchyModel> Nodes { get; set; } = new();
        public MainWindowViewModel()
        {
            dateSort = 0;
            _files = new ObservableCollection<Models.File>();
            _labels = new ObservableCollection<LabelModel>();
            _filteredFiles = new ObservableCollection<Models.File>();
            _recFiles = new ObservableCollection<RecievedFilesDto>();
            _searchText = "";
            _labelFileNames = new List<LabelFileNames>();
            _labeledFiles = new ObservableCollection<Models.File>();
            _foldersList = new List<Folder>();
            _selectedItem = new SimpleFileHierarchyModel();
            //Nodes.Add(new MyItem("A")
            //{
            //    Children =
            //{
            //    new MyItem("A.1"),
            //    new MyItem("A.2"),
            //}
            //});
            //Nodes.Add(new MyItem("B")
            //{
            //    Children =
            //{
            //    new MyItem("B.1")
            //    {
            //        Children =
            //        {
            //            new MyItem("B.1.a"),
            //            new MyItem("B.1.b"),
            //            new MyItem("B.1.c"),
            //            new MyItem("B.1.d"),

            //        }
            //    },
            //    new MyItem("B.2"),
            //}
            //});
            //Nodes.Add(new MyItem("C"));
            //Nodes.Add(new MyItem("D"));

        }

        private SimpleFileHierarchyModel _selectedItem;
        public SimpleFileHierarchyModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    FilterFilesBySearch(); // Filter files whenever search text changes
                }
            }
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

       

        private ObservableCollection<LabelModel> _labels;
        public ObservableCollection<LabelModel> Labels
        {
            get { return _labels; }
            set
            {
                if (_labels != value)
                {
                    _labels = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<Models.File> _files ;
        public ObservableCollection<Models.File> Files
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

        private ObservableCollection<Models.File> _labeledFiles;

        private ObservableCollection<Models.File> _filteredFiles;
        public ObservableCollection<Models.File> FilteredFiles
        {
            get => _filteredFiles;
            set
            {
                _filteredFiles = value;
                OnPropertyChanged(nameof(FilteredFiles));
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
                Models.File f = new Models.File
                {
                    fileName = "file" + i,
                    uploadDate = "2021-10-1" + i
                };
                _files.Add(f);
            }


            //for (int i = 0; i < 15; i++)
            //{
            //    TagModel tag = new TagModel
            //    {
            //        tagName = "Characteristic" + i,
            //        isChecked = false
            //    };
            //    _tags.Add(tag);
            //}

        }

        public async Task GetFolderWithFilesHierarchy()
        {
            try
            {
                string? jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
                if (jwt == null)
                {

                    return;
                }
                Nodes.Clear();
                var httpClient = HttpServiceCustom.GetApiClient(jwt);
                SimpleFileHierarchyModel? node = await httpClient.GetFromJsonAsync<SimpleFileHierarchyModel>("/api/FileFolder/getAllFolderWithFilesForUser");
                if (node != null)
                {
                    Nodes.Add(node);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private string getHumanSize(float size)
        {
            float value;
            if (size < 999999)
            {
                value = size / 1000;
                return value.ToString("0.00") + " KB";
            }
            if(size < 999999999)
            {
                value = size / 1000000;
                return value.ToString("0.00") + " MB";
            }
            value = size / 1000000000;
            return value.ToString("0.00") + " GB";
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
                    Models.File f = new Models.File
                    {
                        fileName = file.FileName,
                        fileSize = file.fileSize,
                        fileSizeStr = getHumanSize(file.fileSize),
                        uploadDate = file.UploadDate.ToString()
                    };
                    _files.Add(f);
                }
            }

        }

        public string GetParentFolderFormPath(string path)
        {
            string[] segments = path.Split('/');
            string folderPath = string.Join("/", segments.Take(segments.Length - 1));
            if (folderPath == "") folderPath = "/";
            return folderPath;
        }
        public async Task RenewAllFolders()
        {
            try
            {
                _foldersList.Clear();
                string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
                var httpClient = HttpServiceCustom.GetApiClient(jwt);
                string path = "%2F";
                Folder? f = await httpClient.GetFromJsonAsync<Folder?>("/api/FileFolder/getFolderWithFiles?path=" + path);
                if (f != null)
                {
                    _files.Clear();
                    foreach (var file in f.folderFiles)
                    {
                        Models.File fileModel = new Models.File
                        {
                            fileName = file.fileName,
                            fileSize = file.fileSize,
                            fileSizeStr = getHumanSize(file.fileSize),
                            uploadDate = file.uploadDate,
                            fullPath = file.fullPath,
                            isFolder = file.isFolder
                        };
                        _files.Add(fileModel);
                    }
                    _foldersList.Add(f);
                }
                PrevFolder = "";

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task RenewFolder(string path)
        {
            try
            {
                string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
                var httpClient = HttpServiceCustom.GetApiClient(jwt);
                string aux = path;
                path = path.Replace("/", "%2F");
                Folder? f = await httpClient.GetFromJsonAsync<Folder?>("/api/FileFolder/getFolderWithFiles?path=" + path);
                if (f != null)
                {
                    _files.Clear();
                    foreach (var file in f.folderFiles)
                    {
                        Models.File fileModel = new Models.File
                        {
                            fileName = file.fileName,
                            fileSize = file.fileSize,
                            fileSizeStr = getHumanSize(file.fileSize),
                            uploadDate = file.uploadDate,
                            fullPath = file.fullPath,
                            isFolder = file.isFolder
                        };
                        _files.Add(fileModel);
                    }
                    _foldersList.Add(f);

                    if (aux == "/")
                        PrevFolder = "";
                    else
                    {
                        string[] segments = aux.Split('/');
                        string folderPath = string.Join("/", segments.Take(segments.Length - 1));
                        if (folderPath == "")
                            PrevFolder = "/";
                        else
                            PrevFolder = folderPath;
                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }

        }


        public async Task GetFolderFiles(string path)
        {

            if (_foldersList.Any(f => f.fullpath == path))
            {
                Folder? f = _foldersList.FirstOrDefault(f => f.fullpath == path);
                if (f != null)
                {
                    _files.Clear();
                    foreach (var file in f.folderFiles)
                    {
                        Models.File fileModel = new Models.File
                        {
                            fileName = file.fileName,
                            fileSize = file.fileSize,
                            fileSizeStr = getHumanSize(file.fileSize),
                            uploadDate = file.uploadDate,
                            fullPath = file.fullPath,
                            isFolder = file.isFolder
                        };
                        _files.Add(fileModel);
                    }
                    if (path == "/")
                        PrevFolder = "";
                    else
                    {
                        string[] segments = path.Split('/');
                        string folderPath = string.Join("/", segments.Take(segments.Length - 1));
                        if (folderPath == "")
                            PrevFolder = "/";
                        else
                            PrevFolder = folderPath;
                    }
                }
            }
            else
            {

                string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
                if (jwt == null)
                {
                    throw new Exception("Your session expired!");
                }
                var httpClient = HttpServiceCustom.GetApiClient();
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
                try
                {
                    string aux = path;
                    path = path.Replace("/", "%2F");
                    Folder? f = await httpClient.GetFromJsonAsync<Folder?>("/api/FileFolder/getFolderWithFiles?path=" + path);
                    if (f != null)
                    {
                        _files.Clear();
                        foreach (var file in f.folderFiles)
                        {
                            Models.File fileModel = new Models.File
                            {
                                fileName = file.fileName,
                                fileSize = file.fileSize,
                                fileSizeStr = getHumanSize(file.fileSize),
                                uploadDate = file.uploadDate,
                                fullPath = file.fullPath,
                                isFolder = file.isFolder
                            };
                            _files.Add(fileModel);
                        }
                        _foldersList.Add(f);

                        if (aux == "/")
                            PrevFolder = "";
                        else
                        {
                            string[] segments = aux.Split('/');
                            string folderPath = string.Join("/", segments.Take(segments.Length - 1));
                            if (folderPath == "")
                                PrevFolder = "/";
                            else
                                PrevFolder = folderPath;
                        }
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            
        }


        public async Task<string> DownloadFile(string fileName, string acutalFileNameWithoutPath) 
        {
            try
            {
                string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
                var httpClient = HttpServiceCustom.GetProxyClient();
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

                var response = await httpClient.GetStreamAsync("getFileFromStorage/?filename=" + fileName);
                //var response = await httpClient.GetStreamAsync("testBACK");
                string saveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, acutalFileNameWithoutPath + "_enc");
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
                string filePath = Path.Combine(downloadFolder, acutalFileNameWithoutPath);
                var encFileStream = System.IO.File.OpenRead(saveFilePath);

                Utils.DecryptAndSaveFileWithAesGCM(encFileStream, filePath,
                                                    RSAKeyService.rsaPrivateKey.Decrypt(Convert.FromBase64String(keyAndIvDto.base64key), RSAEncryptionPadding.OaepSHA256),
                                                    RSAKeyService.rsaPrivateKey.Decrypt(Convert.FromBase64String(keyAndIvDto.base64iv), RSAEncryptionPadding.OaepSHA256));
                encFileStream.Close();
                System.IO.File.Delete(saveFilePath);
                return downloadFolder;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not download file");
            }
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
            //string base64PrivKey = System.IO.File.ReadAllText("D:\\LicentaProiect\\DesktopApp\\DesktopApp\\.keys" + "\\" + email + "_privateKey.priv");

            string base64PrivKey = await SecureStorage.GetAsync(Enums.Symbol.ECPrivateKeyBase64.ToString());


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

            if(keyAndIvDto == null)
            {
                throw new Exception("Could not get File key and File iv");
            }
/*
            byte[] fileKey = Convert.FromBase64String(keyAndIvDto.base64key);
            byte[] fileIv = Convert.FromBase64String(keyAndIvDto.base64iv);*/

            //string userEmail = await SecureStorage.GetAsync(Enums.Symbol.Email.ToString());
            //string base64sendkey = System.IO.File.ReadAllText("D:\\LicentaProiect\\DesktopApp\\DesktopApp\\.keys" + "\\" + userEmail + "_privateKey.priv");

            string? base64sendkey = await SecureStorage.GetAsync(Enums.Symbol.ECPrivateKeyBase64.ToString());

            if(base64sendkey == null)
            {
                throw new Exception("Could not get private key");
            }

            byte[] fileEncKey = Convert.FromBase64String(keyAndIvDto.base64key);
            byte[] fileEncIv = Convert.FromBase64String(keyAndIvDto.base64iv);

            string base64KeyCapsule = getBase64CapsuleAndCiphertext(base64sendkey, Convert.ToBase64String(RSAKeyService.rsaPrivateKey.Decrypt(fileEncKey, RSAEncryptionPadding.OaepSHA256)));
            string base64IvCapsule = getBase64CapsuleAndCiphertext(base64sendkey, Convert.ToBase64String(RSAKeyService.rsaPrivateKey.Decrypt(fileEncIv, RSAEncryptionPadding.OaepSHA256)));

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
                //string base64PrivKey = System.IO.File.ReadAllText("D:\\LicentaProiect\\DesktopApp\\DesktopApp\\.keys" + "\\" + email + "_privateKey.priv");
                string? base64PrivKey = await SecureStorage.GetAsync(Enums.Symbol.ECPrivateKeyBase64.ToString());
                if (base64PrivKey == null)
                {
                    throw new Exception("Could not accept file");
                }
                string base64key = decryptKFrag(fileInf.base64EncKey, base64PrivKey, base64PubKey);
                string base64iv = decryptKFrag(fileInf.base64EncIv, base64PrivKey, base64PubKey);

                byte[] fileKey = Convert.FromBase64String(base64key);
                byte[] fileIv = Convert.FromBase64String(base64iv);

                AcceptFileTransferDto dto = new AcceptFileTransferDto
                {
                    senderEmail = fileInf.senderEmail,
                    receiverEmail = email,
                    fileName = fileInf.fileName,
                    base64FileKey = Convert.ToBase64String(RSAKeyService.rsaPublicKey.Encrypt(fileKey, RSAEncryptionPadding.OaepSHA256)),
                    base64FileIv = Convert.ToBase64String(RSAKeyService.rsaPublicKey.Encrypt(fileIv, RSAEncryptionPadding.OaepSHA256))
                };

                var httpClient2 = HttpServiceCustom.GetProxyClient();
                httpClient2.DefaultRequestHeaders.Remove("Authorization");
                httpClient2.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
                var response2 = await httpClient2.PostAsJsonAsync("acceptReceivedFile", dto);
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

        public int GetTags()
        {
            for (int i = 0; i < 15; i++)
            {
                LabelModel label = new LabelModel
                {
                    labelName = "Label_" + i,
                    isChecked = false
                };
                _labels.Add(label);
            }
            return _labels.Count();
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

            //await GetFilesAndNames();
        }

        public async Task SortByUploadDate()
        {
            if(dateSort == 0)
            {
                _files = new ObservableCollection<Models.File>(_files.OrderBy(f => f.uploadDate));
                dateSort = 1;
            }
            else
            {
                _files = new ObservableCollection<Models.File>(_files.OrderByDescending(f => f.uploadDate));
                dateSort = 0;
            }
        }
        public async Task<int> GetLabelFileNames()
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            LabelsDto labelsDto = await httpClient.GetFromJsonAsync<LabelsDto>("/api/Label/GetLabelsForUser");
            if (labelsDto.list != null)
            {
                _labelFileNames = labelsDto.list;
                _labels.Clear();
                foreach (var label in _labelFileNames)
                {
                    LabelModel labelModel = new LabelModel
                    {
                        labelName = label.labelName,
                        isChecked = false
                    };
                    _labels.Add(labelModel);
                }
                return _labels.Count();
            }
            return 0;
                
        }

        public async Task CreateLabel(string labelName)
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            var response = await httpClient.PostAsJsonAsync("/api/Label/createLabel", labelName);
            if (response.IsSuccessStatusCode)
            {
                LabelModel labelModel = new LabelModel
                {
                    labelName = labelName,
                    isChecked = false
                };
                _labels.Add(labelModel);
            }
            else
            {
                throw new Exception("Could not create label");
            }
        }

        public async Task<List<string>> GetCandidateLabels(string fileName)
        {
            List<string> candidateLabels = new List<string>();
            foreach (var label in _labelFileNames)
            {
                if(!label.fileNames.Contains(fileName))
                {
                    candidateLabels.Add(label.labelName);
                }
            }
            return candidateLabels;
        }

        public async Task<List<string>> GetCandidateLabelsForRemoving(string fileName)
        {
            List<string> candidateLabels = new List<string>();
            foreach (var label in _labelFileNames)
            {
                if (label.fileNames.Contains(fileName))
                {
                    candidateLabels.Add(label.labelName);
                }
            }
            return candidateLabels;
        }
        public async Task AddLabelToFile(string fileName, string labelName)
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            AddLabelToFileDto dto = new AddLabelToFileDto
            {
                fileName = fileName,
                labelName = labelName
            };

            var response = await httpClient.PostAsJsonAsync("/api/Label/addLabelToFile", dto);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Could not add label to file");
            }
        }
        public void FilterFilesBySearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                if (_labeledFiles.Count() == 0)
                    _filteredFiles = _files; // If search text is empty, show all files
                else
                    _filteredFiles = _labeledFiles;
            }
            else
            {
                // Filter files based on search text
                if(_labeledFiles.Count() > 0)
                    _filteredFiles = new ObservableCollection<Models.File>(
                        _labeledFiles.Where(file => file.fileName.Contains(_searchText)));
                else
                    _filteredFiles = new ObservableCollection<Models.File>(
                        _files.Where(file => file.fileName.Contains(_searchText)));
            }
        }

        public async Task FilterFilesByLabel(string labelName)
        {
            var lf = _labelFileNames.FirstOrDefault(lf => lf.labelName == labelName);
            foreach (var file in _files)
            {
                if (lf.fileNames.Contains(file.fileName))
                    _labeledFiles.Add(file);
            }
            FilteredFiles = _labeledFiles;
        }

        public async Task RemoveLabelFromFiltering(string labelName)
        {
            var lf = _labelFileNames.FirstOrDefault(lf => lf.labelName == labelName);
            foreach (var file in _files)
            {
                if (lf.fileNames.Contains(file.fileName))
                    _labeledFiles.Remove(file);
            }
            if (_labeledFiles.Count() == 0)
            {
                FilteredFiles = _files;
            }
        }

        public async Task RemoveLabelFile(string fileName, string label)
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            RemoveLabelFileDto dto = new RemoveLabelFileDto
            {
                fileName = fileName,
                labelName = label
            };

            var response = await httpClient.PostAsJsonAsync("/api/Label/removeLabelFile", dto);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Could not add label to file");
            }
        }

        public async Task DeleteLabel(string labelName)
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetApiClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            var response = await httpClient.PostAsJsonAsync("/api/Label/deleteLabel", labelName);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Could not delete label");
            }
        }

        public async Task SortBySize()
        {
            if (sizeSort == 0)
            {
                _files = new ObservableCollection<Models.File>(_files.OrderBy(f => f.fileSize));
                sizeSort = 1;
            }
            else
            {
                _files = new ObservableCollection<Models.File>(_files.OrderByDescending(f => f.fileSize));
                sizeSort = 0;
            }
        }
    }
}
