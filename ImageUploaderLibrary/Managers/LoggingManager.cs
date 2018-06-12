using System;
using System.IO;
using Informanagement.Infrastructure.Helpers;

namespace ImageUploaderLibrary.Managers
{
    public class CustomLoggArgs : EventArgs
    {
        public Exception Exception { get; set; }
    }

    public class LoggingHelper
    {
        public LoggingHelper()
        {
            var logFile = "Loggs.txt";
            if (!new FileInfo(logFile).Exists) File.Create(logFile);
        }

        public event EventHandler OnNewLog;

        private string FormatLogg(string logg)
        {
            return $@"{Environment.NewLine}{Environment.NewLine} 
                      --------------{DateTime.Now}-------------- 
                                 {Environment.NewLine}
                                        {logg}";
        }

        public bool Save(Exception exception)
        {
            try
            {
                File.AppendAllText("Loggs.txt", FormatLogg(exception.ToFriendlyError()));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            finally
            {
                OnNewLog?.Invoke(this, new CustomLoggArgs {Exception = exception});
            }
        }
    }
}