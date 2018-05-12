﻿using System;
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


                //var BlobContainer = await CreateBlobContainerAsync(BlobName, _storageAccount);
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

        public  async Task ProcessAsync()
        {
            CloudStorageAccount storageAccount = null;
            CloudBlobContainer cloudBlobContainer = null;
            string sourceFile = null;
            string destinationFile = null;

            // Retrieve the connection string for use with the application. The storage connection string is stored
            // in an environment variable on the machine running the application called storageconnectionstring.
            // If the environment variable is created after the application is launched in a console or with Visual
            // Studio, the shell needs to be closed and reloaded to take the environment variable into account.
            string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    // Create a file in your local MyDocuments folder to upload to a blob.
                    string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string localFileName = "QuickStart_" + Guid.NewGuid().ToString() + ".txt";
                    sourceFile = Path.Combine(localPath, localFileName);
                    // Write text to the file.
                    File.WriteAllText(sourceFile, "Hello, World!");

                    Console.WriteLine("Temp file = {0}", sourceFile);
                    Console.WriteLine("Uploading to Blob storage as blob '{0}'", localFileName);
                    Console.WriteLine();

                    // Get a reference to the blob address, then upload the file to the blob.
                    // Use the value of localFileName for the blob name.
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFileName);
                    await cloudBlockBlob.UploadFromFileAsync(sourceFile);

                    // List the blobs in the container.
                    Console.WriteLine("Listing blobs in container.");
                    BlobContinuationToken blobContinuationToken = null;
                    do
                    {
                        var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                        // Get the value of the continuation token returned by the listing call.
                        blobContinuationToken = results.ContinuationToken;
                        foreach (IListBlobItem item in results.Results)
                        {
                            Console.WriteLine(item.Uri);
                        }
                    } while (blobContinuationToken != null); // Loop while the continuation token is not null.
                    Console.WriteLine();

                    // Download the blob to a local file, using the reference created earlier. 
                    // Append the string "_DOWNLOADED" before the .txt extension so that you can see both files in MyDocuments.
                    destinationFile = sourceFile.Replace(".txt", "_DOWNLOADED.txt");
                    Console.WriteLine("Downloading blob to {0}", destinationFile);
                    Console.WriteLine();
                    await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);
                }
                catch (StorageException ex)
                {
                    Console.WriteLine("Error returned from the service: {0}", ex.Message);
                }
                finally
                {
                    Console.WriteLine("Press any key to delete the sample files and example container.");
                    Console.ReadLine();
                    // Clean up resources. This includes the container and the two temp files.
                    Console.WriteLine("Deleting the container and any blobs it contains");
                    if (cloudBlobContainer != null)
                    {
                        await cloudBlobContainer.DeleteIfExistsAsync();
                    }
                    Console.WriteLine("Deleting the local source file and local downloaded files");
                    Console.WriteLine();
                    File.Delete(sourceFile);
                    File.Delete(destinationFile);
                }
            }
            else
            {
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add a environment variable named 'storageconnectionstring' with your storage " +
                    "connection string as a value.");
            }
        }

        public  async Task<string> UploadFileAsync(string filePath)
        {
            // Retrieve the connection string for use with the application. The storage connection string is stored
            // in an environment variable on the machine running the application called storageconnectionstring.
            // If the environment variable is created after the application is launched in a console or with Visual
            // Studio, the shell needs to be closed and reloaded to take the environment variable into account.
            //_blobConnString ="DefaultEndpointsProtocol = https; AccountName = apobloblogs; AccountKey = T0VwUsUChxehS6ylEHeCHwijOx1Ql05wEJ2P4UDhBbrlh9Y1tE4HwbsMJpvyFn63TF5 / NErBQ8t0zA6Pa7E80w ==; EndpointSuffix = core.windows.net";
            // Check whether the connection string can be parsed.
           
            if (CloudStorageAccount.TryParse("DefaultEndpointsProtocol=https;AccountName=apobloblogs;AccountKey=T0VwUsUChxehS6ylEHeCHwijOx1Ql05wEJ2P4UDhBbrlh9Y1tE4HwbsMJpvyFn63TF5/NErBQ8t0zA6Pa7E80w==;EndpointSuffix=core.windows.net", out _storageAccount))
            {
                try
                {
                   
                    _cloudBlobContainer = await CreateBlobContainerAsync("AposLogOnBLob".ToLower(), _storageAccount);

                    // Create a file in your local MyDocuments folder to upload to a blob.
                  //  string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    //string localFileName = "QuickStart_" + Guid.NewGuid().ToString() + ".txt";
                    
                    // Write text to the file.
                   // File.WriteAllText(sourceFile, "Hello, World!");                  
                
                    await UploadFileAsync(_cloudBlobContainer, filePath);

                    // List the blobs in the container.

                    return UploadSucceeded;
                    //Console.WriteLine("Listing blobs in container.");
                    //BlobContinuationToken blobContinuationToken = null;
                    //do
                    //{
                    //    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    //    // Get the value of the continuation token returned by the listing call.
                    //    blobContinuationToken = results.ContinuationToken;
                    //    foreach (IListBlobItem item in results.Results)
                    //    {
                    //        Console.WriteLine(item.Uri);
                    //    }
                    //} while (blobContinuationToken != null); // Loop while the continuation token is not null.
                    //Console.WriteLine();

                    // Download the blob to a local file, using the reference created earlier. 
                    // Append the string "_DOWNLOADED" before the .txt extension so that you can see both files in MyDocuments.
                    //destinationFile = sourceFile.Replace(".txt", "_DOWNLOADED.txt");
                    //Console.WriteLine("Downloading blob to {0}", destinationFile);
                    //Console.WriteLine();
                    //await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);
                }
                catch (StorageException ex)
                {
                    return $"Error returned from the Sotorage service: {ex.Message}";
                }
                finally
                {
                   // Console.WriteLine("Press any key to delete the sample files and example container.");
                    //Console.ReadLine();
                    // Clean up resources. This includes the container and the two temp files.
                   // Console.WriteLine("Deleting the container and any blobs it contains");
                    //if (cloudBlobContainer != null)
                    //{
                    //    await cloudBlobContainer.DeleteIfExistsAsync();
                    //}
                   // Console.WriteLine("Deleting the local source file and local downloaded files");
                   // Console.WriteLine();
                    File.Delete(filePath);
                    //File.Delete(destinationFile);
                }
            }
            else
            {
                return ErroOnConnection;
            }
        }

    }


}
