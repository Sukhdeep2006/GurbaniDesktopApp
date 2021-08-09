using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace GurbaniDesktopApp
{
    public class Logger
    {
        private bool IsLogEnabled = true;
        private const char strLogSeparator = '|';
        private volatile static Logger loggerObject;
        // Starts the worker thread that gets rid of the queue:
        private Logger()
        {
           
            //if (IsLogEnabled)
            //{
            //    loggingWorker = new Thread(LogHandler)
            //    {
            //        Name = "GurbaniDesktopApp",
            //        IsBackground = true,
            //        Priority = ThreadPriority.BelowNormal
            //    };
            //    loggingWorker.Start();
            //}
        }

        public static Logger InstanceCreate()
        {
            object lockingObject = new object();
            if (loggerObject == null)
            {
                lock (lockingObject)
                {
                    loggerObject = new Logger();
                }
            }
            return loggerObject;
        }
        internal void LogMessage(LogMessageType logType, string strMsg)
        {
            if (IsLogEnabled)
            {
                LogMessage(logType, string.Empty, strMsg);
            }
        }
        internal void LogMessage(LogMessageType logType, string strIdentifier, string strMsg)
        {
            if (IsLogEnabled)
            {
                LogMessageInFile(GenerateErrorMessage(logType, strIdentifier, strMsg));               
            }
        }
        private void LogMessageInFile(string logMsgItem)
        {
            try
            {
                string strFilePath = string.Empty; //Convert.ToString(ConfigurationManager.AppSettings["LogFilePath"]);
                if (string.IsNullOrWhiteSpace(strFilePath))
                {
                    strFilePath = string.Format("{0}\\Log.txt", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + @"\GurbaniApp");
                    //strFilePath = string.Format("{0}\\Log.txt", AppDomain.CurrentDomain.BaseDirectory.ToString());
                    //CurrentDir = AppDomain.CurrentDomain.BaseDirectory
                }

                DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(strFilePath));
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                using (FileStream fs = File.Open(strFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(Convert.ToString(logMsgItem));
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                    fs.Close();

                    FileInfo logFile = new FileInfo(strFilePath);
                    if (logFile != null && logFile.Exists && logFile.Length > 0 && ((logFile.Length / 1024f) / 1024f) > 5)
                    {
                        string strFileExtension = logFile.Extension;
                        string newFilename = string.Format("{0}\\{1}_{2}{3}", logFile.Directory.FullName, logFile.Name.Substring(0, logFile.Name.IndexOf(strFileExtension)), DateTime.Now.ToString("yyyyMMddHHmmssFFF"), strFileExtension);
                        logFile.MoveTo(newFilename);
                    }
                }
               // MessageBox.Show("Error Occured. Please check the log File at: " + strFilePath);

            }
            catch(Exception ex)
            {
                MessageBox.Show("Error Occured in Logging. " + ex.Message);
            }

        }
        private string GenerateErrorMessage(LogMessageType logType, string strIdentifier, string strLogMsg)
        {

            StringBuilder sbMessage = new StringBuilder();

            sbMessage.Append(Environment.NewLine);
            if (!string.IsNullOrWhiteSpace(strIdentifier))
            {
                sbMessage.Append(strIdentifier);
                sbMessage.Append(strLogSeparator);
            }
            sbMessage.Append(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
            sbMessage.Append(strLogSeparator);
            sbMessage.Append(logType.ToString());
            sbMessage.Append(strLogSeparator);
            sbMessage.Append(strLogMsg);

            return sbMessage.ToString();
        }
    }
    public enum LogMessageType
    {
        Info,
        Error,
        Warning,
        Validation,
        Exception,
        Debug
    }
}