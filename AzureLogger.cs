using System;
using NLog;
using System.ComponentModel.DataAnnotations;
using System.Data.Services.Client;
using Microsoft.WindowsAzure.ServiceRuntime;
using NLog.Common;
using NLog.Targets;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using AzureOuterLogs.Common;


namespace AzureOuterLogs.Common
{
    public static class AzureLogger
    {
        private static NLog.Logger _log = NLog.LogManager.GetLogger("123");

        public static void LogMsgToFile(string msg)
        {
            _log.Info(msg);
        }

        public static void LogErrorToFile(string msg)
        {
            _log.Error(msg);
        }

    }

    [Target("AzureStorage")]
    public class AzureStorageTarget : Target
    {
        //private LogServiceContext _ctx;
        private string _tableEndpoint;

        [Required]
        public string TableStorageConnectionStringName { get; set; }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            var cloudStorageAccount =
                CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue(TableStorageConnectionStringName));
            _tableEndpoint = cloudStorageAccount.TableEndpoint.AbsoluteUri;
            CloudTableClient.CreateTablesFromModel(typeof(LogServiceContext), _tableEndpoint, cloudStorageAccount.Credentials);
           // _ctx = new LogServiceContext(cloudStorageAccount.TableEndpoint.AbsoluteUri, cloudStorageAccount.Credentials);
        }

        protected override void Write(LogEventInfo loggingEvent)
        {
            try
            {
                var entry = new LogEntry()
                {

                }
                //_ctx.Log(new LogEntry
                //{
                //    RoleInstance = RoleEnvironment.CurrentRoleInstance.Id,
                //    DeploymentId = RoleEnvironment.DeploymentId,
                //    Timestamp = loggingEvent.TimeStamp,
                //    Message = loggingEvent.FormattedMessage,
                //    Level = loggingEvent.Level.Name,
                //    LoggerName = loggingEvent.LoggerName,
                //    StackTrace = loggingEvent.StackTrace != null ? loggingEvent.StackTrace.ToString() : null
                //});
            }
            catch (DataServiceRequestException e)
            {
                InternalLogger.Error(string.Format("{0}: Could not write log entry to {1}: {2}",
                    GetType().AssemblyQualifiedName, _tableEndpoint, e.Message), e);
            }
        }
    }

  
}
