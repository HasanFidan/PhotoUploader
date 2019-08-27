using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace PhotoUploader.Service
{
    public class Logger
    {
        private static int exceptionid = 1;
        public static int NextExceptionID { get { return exceptionid++; } }
        static Logger()
        {
            FileInfo configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log4Net.config"));
            log4net.Config.XmlConfigurator.ConfigureAndWatch(configFile);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            int exceptionid = NextExceptionID;
            Basic.ErrorFormat("Unhandled exception occured. Error Number: {0}", exceptionid);
            Detail.Error(string.Format("Error Number: {0}", exceptionid), e.ExceptionObject as Exception);
        }

        public static void UnExpectedError(Exception exc, string message = "")
        {
            if (message == string.Empty)
                message = "Unexpected exception occured.";

            int exceptionid = NextExceptionID;
            Basic.ErrorFormat("{0} Error Number: {1}", message, exceptionid);
            Detail.Error(string.Format("Error Number: {0}", exceptionid), exc);
        }

        public static ILog Basic = LogManager.GetLogger("AppLog");
        public static ILog Detail = LogManager.GetLogger("AppLogDetail");

        public static void Section(string sectionName)
        {
            Basic.InfoFormat("---------------------- {0} ---------------------", sectionName);
            Detail.InfoFormat("---------------------- {0} ---------------------", sectionName);
        }
        public static void Module(string moduleName)
        {
            Basic.InfoFormat("======================= {0} =======================", moduleName);
            Detail.InfoFormat("======================= {0} =======================", moduleName);
        }

        public static string ExceptionToStr(Exception ex)
        {
            if (ex == null)
                return "";
            try
            {
                var str = ex.Message;
                while (ex.InnerException != null)
                {
                    str += String.Format("\n{0}", ex.InnerException.Message);
                    ex = ex.InnerException;
                }
                str += "\n\n" + ex.ToString();

                return str;
            }
            catch { }

            return "-";
        }

    }
}
