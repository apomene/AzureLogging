using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Configuration;


namespace AzureStorage.Common
{
    public class Common
    {
        private static string _blobConnString = ConfigurationManager.AppSettings["BlobConString"];
        private static CloudStorageAccount _storageAccount = null;
        private static CloudBlobContainer _cloudBlobContainer = null;
        private static string ErroOnConnection = "A connection string has not been defined in the system environment variables. " +
                    "Add a environment variable named 'storageconnectionstring' with your storage " +
                    "connection string as a value.";
        private static string UploadSucceeded = "File Uploaded to Azure BLob";

        public Common()
        {
            _storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=apobloblogs;AccountKey=T0VwUsUChxehS6ylEHeCHwijOx1Ql05wEJ2P4UDhBbrlh9Y1tE4HwbsMJpvyFn63TF5/NErBQ8t0zA6Pa7E80w==;EndpointSuffix=core.windows.net");//, out _storageAccount))          
        }

        private  async Task<CloudBlobContainer> CreateBlobContainerAsync (string ContainerName,CloudStorageAccount StorageAcount)
        {
            // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
            CloudBlobClient cloudBlobClient = StorageAcount.CreateCloudBlobClient();

            // Create a container called 'quickstartblobs' and append a GUID value to it to make the name unique. 
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName + Guid.NewGuid().ToString());
            await cloudBlobContainer.CreateAsync();
            //Console.WriteLine("Created container '{0}'", cloudBlobContainer.Name);
            //Console.WriteLine();

            // Set the permissions so the blobs are public. 
            BlobContainerPermissions permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            await cloudBlobContainer.SetPermissionsAsync(permissions);
            return cloudBlobContainer;
        }

        private  async Task UploadFileAsync (CloudBlobContainer BlobContainer,string filePath)
        {
            // Get a reference to the blob address, then upload the file to the blob.
            // Use the value of localFileName for the blob name.
            var fileName = Path.GetFileName(filePath);
            CloudBlockBlob cloudBlockBlob = BlobContainer.GetBlockBlobReference(fileName);
            await cloudBlockBlob.UploadFromFileAsync(filePath);
        }

        public async Task<bool> DownLoadFileAsync(string BlobName,string FilePath)
        {
            try
            {               
                // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                CloudBlobClient cloudBlobClient = _storageAccount.CreateCloudBlobClient();
                //var BlobContainer = await CreateBlobContainerAsync(BlobName, _storageAccount);
                CloudBlobContainer BlobContainer = cloudBlobClient.GetContainerReference("aposlogonblobfcfbd685-f461-4c86-ad5d-42a52e9942a1");
                CloudBlockBlob cloudBlockBlob = BlobContainer.GetBlockBlobReference(BlobName);
                await cloudBlockBlob.DownloadToFileAsync(FilePath, FileMode.Create);
                return true;              
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<string>> ListBlobsAsync()
        {
            BlobContinuationToken blobContinuationToken = null;
            var res = new List<string>();
            do
            {
                // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                CloudBlobClient cloudBlobClient = _storageAccount.CreateCloudBlobClient();
                CloudBlobContainer BlobContainer = cloudBlobClient.GetContainerReference("aposlogonblobfcfbd685-f461-4c86-ad5d-42a52e9942a1");

                var results = await BlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                foreach (IListBlobItem item in results.Results)
                {
                    res.Add(item.Uri.ToString());
                }
            } while (blobContinuationToken != null); // Loop while the continuation token is not null.
            return res;
        }
     
        public  async Task<string> UploadFileAsync(string filePath, bool deleteFile=false)
        {
            try
            {                  
                _cloudBlobContainer = await CreateBlobContainerAsync("AposLogOnBLob".ToLower(), _storageAccount);
                await UploadFileAsync(_cloudBlobContainer, filePath);
                return UploadSucceeded;
            }
            catch (StorageException ex)
            {
                return $"Error returned from the Sotorage service: {ex.Message}";
            }
            finally
            {
                if(deleteFile)
                File.Delete(filePath);
            }
        }

    }


}
