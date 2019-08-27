using Microsoft.Win32;
using PhotoUploader.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PhotoUploader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    
    public partial class App : Application
    {
      
        public App()
        {

            Configuration configuration =
               ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (configuration.AppSettings.Settings["defaultlanguage"] == null)
            {
                configuration.AppSettings.Settings.Add("defaultlanguage", "nl");
                configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

            }




            string dlang = ConfigurationManager.AppSettings["defaultlanguage"];
            PhotoUploader.Properties.Resources.Culture = new CultureInfo(dlang);


            // Check for other instances
            var processArr = Process.GetProcessesByName("PhotoUploader");

            int coun = processArr.Count();

            if (processArr.Count() > 1)
            {
                DateTime mintime = DateTime.Now;
                Process tobekilled = null;

                foreach (Process p in processArr)
                {
                    //Logger.Basic.Debug("Process start time is :" + p.StartTime.ToString());
                    if (mintime > p.StartTime)
                    {
                        mintime = p.StartTime;
                        tobekilled = p;
                        //Logger.Basic.Debug("OLD Process is killed" + p.StartTime.ToString());
                    }
                }

                tobekilled.Kill();

            }// end of if (processArr.Count() > 1)

            Current.Properties["FileWatcher"] = new FileWatcher();
            Current.Properties["CheckFileDirectory"] = new CheckFileDirectory();

         
        }
    }
}
