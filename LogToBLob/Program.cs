using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage.Common;
namespace LogToBLob
{
    class Program
    {
        private static Common _AzureLogger = new Common();
    
        static void Main(string[] args)
        {
            var msg = MainAsync(args).GetAwaiter().GetResult();
            Console.WriteLine(msg);
            Console.Read();
        }

        static async Task<string> MainAsync(string[] args)
        {
            var result =  await  _AzureLogger.LogToAzure(@"C:\\AzureLogs\\tesLog.log");
            return result;
        }
    }
}
