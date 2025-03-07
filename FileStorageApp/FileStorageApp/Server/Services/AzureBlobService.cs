﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
namespace FileStorageApp.Server.Services
{
    public class AzureBlobService
    {
        
        private readonly BlobContainerClient _blobContainerClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly IConfiguration _configuration;
        

        public AzureBlobService(IConfiguration configuration)
        {
            _configuration = configuration;
            _containerName = _configuration["ContainerName"];
            _blobServiceClient = new BlobServiceClient(_configuration["AzureBlobConnectionString"]);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
             
        }

        public BlobContainerClient GetBlobContainerClient() { return _blobContainerClient; }
        public async Task<string?> UploadFileToCloud(string encFileBase64, string tag)
        {
            try
            {
                BlobClient bclient = _blobContainerClient.GetBlobClient(tag);

                //generam un strem din string-ul "encFileBase64"
                Stream stream = Utils.Utils.GenerateStreamFromString(encFileBase64);

                await bclient.UploadAsync(stream, true);
          

                var url = bclient.Uri.AbsoluteUri;
                return url;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<string?> UploadFileToCloud_v2(IFormFile encFile, string tag)
        {
            try
            {
                AppendBlobClient apb_client = _blobContainerClient.GetAppendBlobClient(tag);
                if (!await apb_client.ExistsAsync())
                {
                    await apb_client.CreateAsync();
                }

                using (Stream fileStream = encFile.OpenReadStream())
                {
                    await apb_client.AppendBlockAsync(fileStream);
                }
                return apb_client.Uri.AbsoluteUri;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<string?> GetUri(string tag)
        {
            try
            {
                AppendBlobClient apb_client = _blobContainerClient.GetAppendBlobClient(tag);
                if (!await apb_client.ExistsAsync())
                    return null;
                return apb_client.Uri.AbsoluteUri;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
/*
        public async Task<string?> UpdateBlob(Stream encFile, string tag)
        {
            try
            {
                AppendBlobClient apb_client = _blobContainerClient.GetAppendBlobClient(tag);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }*/

        public async Task<string>? GetContentFileFromBlob(string tag)
        {
            try
            {
                BlobClient blobClient = _blobContainerClient.GetBlobClient(tag);
                BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                string blobContents = downloadResult.Content.ToString();

                return blobContents;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<bool> DeleteFileFromCloud(string tag)
        {
            try
            {
                BlobClient blobClient = _blobContainerClient.GetBlobClient(tag);
                await blobClient.DeleteIfExistsAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

    }
    
}
