using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace PhotoUploader.Service
{
    public class CheckFileDirectory
    {
       private static System.Timers.Timer aTimer = new System.Timers.Timer();

      
        public CheckFileDirectory()
        {
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Enabled = false;
           
        }

        private static object _lock = new object();
        static FileWatcher filewatcher = null;
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                filewatcher = (FileWatcher)App.Current.Properties["FileWatcher"];
                
                 CheckInternetIsAvailable();
                 filewatcher.NetworkState(networkIsAvailable);

                 if (networkIsAvailable)
                 {
                     filewatcher.FileNaming();
                     System.Timers.Timer tmp = (System.Timers.Timer)source;
                     tmp.Interval = 1000 * 10;
                 }
                 else
                 {
                     System.Timers.Timer tmp = (System.Timers.Timer)source;
                     tmp.Interval = 1000 * 2;
                 }
                //tmp.Enabled = false;
                //tmp.Close();
                
            }
            

        }

        public void StartCheck(int timeinterval)
        {
            aTimer.Interval = timeinterval;
            aTimer.Enabled = true;
          

        }

        public void StopCheck()
        {
            aTimer.Enabled = false;
            
        }

        public bool IsTimerEnable()
        {
            return aTimer.Enabled;
        }

        public void SetConnection()
        {
            if (filewatcher != null)
                filewatcher.SetConnection();
        }

        public void NotifyFetchingState(bool state)
        {
            if (filewatcher != null)
                filewatcher.GetFetchingState(state);
        }


        public bool GetNetWorkState()
        {
            return networkIsAvailable;
        }

        private static bool networkIsAvailable = true;

        private static void CheckInternetIsAvailable()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            //bool isok =  NetworkInterface.GetIsNetworkAvailable();

            foreach (NetworkInterface nic in nics)
            {
                networkIsAvailable = false;
                // discard because of standard reasons
                if ((nic.OperationalStatus != OperationalStatus.Up) ||
                    (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) ||
                    (nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
                    continue;

                // this allow to filter modems, serial, etc.
                // I use 10000000 as a minimum speed for most cases
                //if (nic.Speed < minimumSpeed)
                //    continue;

                // discard virtual cards (virtual box, virtual pc, etc.)
                if ((nic.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (nic.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0))
                    continue;

                // discard "Microsoft Loopback Adapter", it will not show as NetworkInterfaceType.Loopback but as Ethernet Card.
                if (nic.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
                    continue;



                networkIsAvailable = true;
                break;
            }



         
        }


        public void Reset()
        {
            if(filewatcher != null)
            filewatcher.Reset();
        }

        private static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            networkIsAvailable = e.IsAvailable;

            //Console.Write("Network availability: ");
            //Console.WriteLine(networkIsAvailable);
        }
        
       
    }
}
