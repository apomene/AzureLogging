using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage.Common;
using System.IO;

namespace LogToBLob
{
    class Program
    {
        private static Common _AzureLogger = new Common();
    
        static void Main(string[] args)
        {
            // var res = GetBLobAsync(args).GetAwaiter().GetResult(); 
            //if (res)
            //{
            //    Console.Write("File downloaded");
            //}
            //else
            //{
            //    Console.Write("Error On Download");
            //}
            //var res = ListBlobsAsync().GetAwaiter().GetResult();
            //// List the blobs in the container.
            //Console.WriteLine("Listing blobs in container.");
            //foreach (var blob in res)
            //{
            //    Console.WriteLine(blob);
            //}
           
            //Console.Read();
           // Environment.Exit(0);
            foreach (var file in GetFiles(""))
            {
                var msg = StoreFileAsync(file).GetAwaiter().GetResult();
                Console.WriteLine(msg);
            }
           // var msg = StoreFileAsync(args).GetAwaiter().GetResult();
           // Console.WriteLine(msg);
            Console.Read();
        }

        static async Task<string> StoreFileAsync(string filePath)
        {       
            var result =  await  _AzureLogger.UploadFileAsync(filePath);
            return result;
        }
    
        static async Task<bool> GetBLobAsync(string[] args)
        {
            var result = await _AzureLogger.DownLoadFileAsync("tesLog.log", @"C:\\AzureLogs\\tesLog2.log");
            return result;

        }

        static async Task<List<string>> ListBlobsAsync()
        {
            var result = await _AzureLogger.ListBlobsAsync();
            return result;
        }

        static List<string> GetFiles(string DirectoryPath)
        {
            try
            {
                return Directory.GetFiles(@"C:\\AzureLogs", "*.log").ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.Read();
                return new List<string>();
            }
        }
                 
    }
}
