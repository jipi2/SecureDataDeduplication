using CryptoLib;
using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using DesktopApp.KeysService;
using DesktopApp.Models;
using FileStorageApp.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DesktopApp.ViewModels
{
    public class UploadPageViewModel : INotifyPropertyChanged
    {
        private List<Task<byte[]>> _computeHashTaskList;

        private List<FileModel> _fileModelList = new List<FileModel>();

        private CryptoService _cryptoService = new CryptoService();

        public ObservableCollection<FileType> FileTypes { get; set; }
        private ObservableCollection<string> _fileNamePickedList = new ObservableCollection<string>();

        public ObservableCollection<string> FileNamePickedList
        {
            get { return _fileNamePickedList; }
            set
            {
                if (_fileNamePickedList != value)
                {
                    _fileNamePickedList = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _uploadProgress;
        public double UploadProgress
        {
            get { return _uploadProgress; }
            set
            {
                if (_uploadProgress != value)
                {
                    _uploadProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public UploadPageViewModel()
        {
            //FileNamePicked = "";
            FileNamePickedList = new ObservableCollection<string>();
            _fileNamePickedList = new ObservableCollection<string>();
            _computeHashTaskList = new List<Task<byte[]>>();
            _uploadProgress = 0;

            FileTypes = new ObservableCollection<FileType>
            {
                new FileType { Name = "Pdf files", Icon = "\uf1c1" , IconColor = "#753B3B"},
                new FileType { Name = "Excel files", Icon = "\uf1c3", IconColor = "#548335" },
                new FileType { Name = "Word files", Icon = "\uf1c2", IconColor = "#6243DF" },
                new FileType { Name = "Jpg files", Icon = "\uf1c5", IconColor = "#6243DF" },
                new FileType { Name = "Png files", Icon = "\uf1c5", IconColor = "#6243DF" },
                new FileType { Name = "Csv files", Icon = "\uf1c3", IconColor = "#548335" },
                new FileType { Name = "Mp3 files", Icon = "\uf1c7", IconColor = "#BD7800" },
                new FileType { Name = "Mp4 files", Icon = "\uf1c7", IconColor = "#BD7800" },
                new FileType { Name = "Iso files", Icon = "\uf15b", IconColor = "White" },
                new FileType { Name = "Txt files", Icon = "\uf6dd", IconColor = "#548335" },
                new FileType { Name = "Ppt files", Icon = "\uf1c4", IconColor = "#BD7800" },
                new FileType { Name = "Zip files", Icon = "\uf1c6", IconColor = "#5200BD" },
                new FileType { Name = "Rar files", Icon = "\uf1c3", IconColor = "#5200BD" },
                new FileType { Name = "Apk files", Icon = "\uf3ce", IconColor = "#028749" },
                new FileType { Name = "Html files", Icon = "\uf1c9", IconColor = "#3150B7" },
                new FileType { Name = "Code files", Icon = "\uf1c9", IconColor = "#3150B7" },
                //new FileType { Name = "Excel files", Icon = "\uf1c3", IconColor = "#548335" },
                //new FileType { Name = "Excel files", Icon = "\uf1c3", IconColor = "#548335" },

            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task SelectFile()
        {
            var result = await FilePicker.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Please select file or more"
            });

            if (result == null)
                throw new Exception("You canceled the uploading process");

            foreach(var file in result)
            {
                FileModel fileModel = new FileModel();

                fileModel.filePath = file.FullPath;
                fileModel.fileName = file.FileName;
                fileModel.encFileName = file.FileName + "_enc_";
                fileModel.fileStream = new FileStream(file.FullPath, FileMode.Open);
                _fileModelList.Add(fileModel);

                _fileNamePickedList.Add(file.FileName);

                Task<byte[]> computeHashTask = Task.Run(async () => await _cryptoService.GetHashOfFile(fileModel.fileStream));
                _computeHashTaskList.Add(computeHashTask);
            }
        }

        public async Task EncryptFile(FileModel _fileModel, Task<byte[]> _computeHashTask)
        {
            Stopwatch encryptTime = new Stopwatch();
            encryptTime.Start();

            byte[] hash = await _computeHashTask;
            _fileModel.fileStream.Close();

            _fileModel.hash = hash;

            byte[] fileKey = await _cryptoService.ExtractKey(_fileModel.hash, Defines.CryptoDefines.AES_GCM_KEY_SIZE);
            byte[] fileIv = await _cryptoService.ExtractIv(_fileModel.hash, Defines.CryptoDefines.AES_GCM_KEY_SIZE, Defines.CryptoDefines.AES_GCM_IV_SIZE);

            _fileModel.key = fileKey;
            _fileModel.iv = fileIv;

            Debug.WriteLine(Utils.ByteToHex(_fileModel.key));

            _fileModel.fileStream = new FileStream(_fileModel.filePath, FileMode.Open);

            _fileModel.base64Tag = Convert.ToBase64String(await _cryptoService.EncryptFileStream(_fileModel.fileStream, _fileModel.key, _fileModel.iv, _fileModel.encFileName));
            _fileModel.fileStream.Close();

            encryptTime.Stop();
            Debug.WriteLine("Encrypt time: " + encryptTime.Elapsed);
        }


        public async Task UploadFile(string path, FileModel _fileModel)
        {
            if (path != "/") path += "/";

            if (_fileModel.key == null || _fileModel.iv == null || _fileModel.base64Tag == null
                || _fileModel.fileName == null || _fileModel.encFileName == null)
                throw new Exception("An error has occured, please try again!");

            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetProxyClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            FileParamsDto fileDto = new FileParamsDto
            {
                base64Key = Convert.ToBase64String(RSAKeyService.rsaPublicKey.Encrypt(_fileModel.key, RSAEncryptionPadding.OaepSHA256)),
                base64Iv = Convert.ToBase64String((RSAKeyService.rsaPublicKey.Encrypt(_fileModel.iv, RSAEncryptionPadding.OaepSHA256))),
                base64Tag = _fileModel.base64Tag,
                fileName = _fileModel.fileName
            };

            bool isNameDuplicate = true;
            int i = 0;
            var httpApiClient = HttpServiceCustom.GetApiClient(jwt);
            while (isNameDuplicate)
            {
                try
                {
                    var apiReponse = await httpApiClient.PostAsJsonAsync("/api/File/verifyNameDuplicate", path + fileDto.fileName);
                    if (apiReponse.IsSuccessStatusCode)
                    {
                        var resultString = await apiReponse.Content.ReadAsStringAsync();
                        bool.TryParse(resultString, out isNameDuplicate);
                        if (isNameDuplicate)
                        {
                            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_fileModel.fileName);
                            var extension = Path.GetExtension(_fileModel.fileName);
                            string newName;

                            if (i > 0)
                                newName = fileNameWithoutExtension + "_Copy" + $"({i})" + extension;
                            else
                                newName = fileNameWithoutExtension + "_Copy" + extension;
                            i++;
                            fileDto.fileName = newName;
                        }
                    }
                    else
                    {
                        throw new Exception("An error has occured, while sending the file, please try again!");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    throw e;
                }
            }

            string encFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _fileModel.encFileName);

            var progressableContent = new ProgressableStreamContent(new StreamContent(System.IO.File.OpenRead(encFilePath)), (uploaded, total) =>
            {
                // Progress callback - you can update UI or perform other actions based on the upload progress
                double progressPercentage = (double)uploaded / total;
                UploadProgress = progressPercentage;
                //Debug.WriteLine($"Uploaded: {uploaded} bytes of {total} bytes");
                Debug.WriteLine($"Progress: {UploadProgress}%");
            });

            var multipartContent = new MultipartFormDataContent();

            var fileStream = System.IO.File.OpenRead(encFilePath);
            multipartContent.Add(new StringContent(path + fileDto.fileName), "fileName");
            multipartContent.Add(new StringContent(fileDto.base64Key), "base64Key");
            multipartContent.Add(new StringContent(fileDto.base64Iv), "base64Iv");
            multipartContent.Add(new StringContent(fileDto.base64Tag), "base64Tag");
            //    multipartContent.Add(new StreamContent(fileStream), "file", fileDto.fileName);
            multipartContent.Add(progressableContent, "file", fileDto.fileName);

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            var response = await httpClient.PostAsync("uploadFile", multipartContent);
            if (!response.IsSuccessStatusCode)
            {
                string errorPhrase = await response.Content.ReadAsStringAsync();
                if (errorPhrase.Equals("{\"detail\":\"User already has this file\"}"))
                    throw new Exception("You already have this file in your storage");
                throw new Exception("An error has occured, while sending the file, please try again");
            }

            stopwatch.Stop();
            Debug.WriteLine("Challenge time: " + stopwatch.Elapsed);

            fileStream.Close();
            System.IO.File.Delete(encFilePath);

            UploadProgress = 0;
        }

        public async Task CancelBeforeUploading()
        {
            //if (_fileModel.fileStream != null)
            //{
            //    _fileModel.fileStream.Close();
            //    _fileModel.fileStream = null;
            //}

            foreach (var fileModel in _fileModelList)
            {
                if (fileModel.fileStream != null)
                {
                    fileModel.fileStream.Close();
                    fileModel.fileStream = null;
                }
            }
        }

        public FileModel GetFileModelAt(int pos)
        {
            return _fileModelList[pos];
        }

        public Task<byte[]> GetHashTaskAt(int pos)
        {
            return _computeHashTaskList[pos];
        }
        public int GetNumberOfFilesSelected()
        {
            return _fileModelList.Count;
        }

        public void ClearFiles()
        {
            _fileModelList.Clear();
            _fileNamePickedList.Clear();
            _computeHashTaskList.Clear();
        }
    }
}
