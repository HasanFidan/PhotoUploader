using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;

namespace PhotoUploader.Service
{
    public class CameraDeviceManager
    {
        static ManagementEventWatcher m_mewWatcher;
        static readonly Guid GUID_DEVCLASS_DIGITALCAMERA = new Guid("{eec5ad98-8080-425f-922a-dabf3de3f69a}");
        static readonly Guid GUID_DEVCLASS_USBSTORAGE = new Guid("{36fc9e60-c465-11cf-8056-444553540000}");
        static Guid GUID_DEVCLASS_ACTIVEDEVICE;
        static bool ConnectionState = false;

        public delegate void DigitalDeviceAttached(bool status);
        public  DigitalDeviceAttached CameraConnectionStatusChanged;

        public delegate void DeviceErrorMessageThrowed(int state, string message);
        public DeviceErrorMessageThrowed DeviceErrorMessageThrow;
        public CameraDeviceManager() {

            try
            {
                WqlEventQuery weqQuery = new WqlEventQuery();
                weqQuery.EventClassName = "__InstanceOperationEvent";
                weqQuery.WithinInterval = new TimeSpan(0, 0, 3);
                weqQuery.Condition = @"TargetInstance ISA 'Win32_USBControllerDevice'";

                ManagementScope msScope = new ManagementScope("root\\CIMV2");
                msScope.Options.EnablePrivileges = true;
                m_mewWatcher = new ManagementEventWatcher(msScope, weqQuery);
                m_mewWatcher.EventArrived += new EventArrivedEventHandler(m_mewWatcher_EventArrived);
                m_mewWatcher.Start();
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
            //finally
            //{
            //    m_mewWatcher.Stop();
            //}
        
        }


        private void GetDeviceErrorMessageThrow(int state, string message)
        {
            if (DeviceErrorMessageThrow == null)
                return;

            DeviceErrorMessageThrow(state,message);
        }


        private  void StatusChanged(bool status)
        {
            try
            {
                if (CameraConnectionStatusChanged == null)
                    return;


                CameraConnectionStatusChanged(status);
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("210005"))
                {
                    GetDeviceErrorMessageThrow(0,"Error : The device is offline. Make sure the device is powered on and connected to the PC.");
                }
                else if (ex.ToString().Contains("210006"))
                {
                    GetDeviceErrorMessageThrow(0,"Error : The device is busy. Close any apps that are using this device or wait for it to finish and then try again.");
                }
                else if (ex.ToString().Contains("210009"))
                {
                    GetDeviceErrorMessageThrow(0,"Error : The WIA device was deleted. It's no longer available.");
                }
                else if(ex.ToString().ToLower().Contains("xml"))
                {
                    GetDeviceErrorMessageThrow(0,"ReStart Application");
                }
                
                Logger.Basic.Error("StatusChanged : " + ex.ToString(), ex);
            }
        }


        private  void m_mewWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            //bool bUSBEvent = false;
            bool CameraEvent = false;
            string deviceName = string.Empty;
           
           PropertyData pdata = (PropertyData) e.NewEvent.Properties["TargetInstance"];
           if (pdata != null)
           {
               //foreach (PropertyData pdData in e.NewEvent.Properties)
               //{
               try
               {
                   ManagementBaseObject mbo = (ManagementBaseObject)pdata.Value;


                   if (mbo != null)
                   {
                       foreach (PropertyData pdDataSub in mbo.Properties)
                       {


                           if (pdDataSub.Name == "Dependent")
                           {
                               deviceName = pdDataSub.Value.ToString();
                               char strQuotes = '"';
                               //string backslash = "\\\\";
                               char[] charsToTrim = { ' ', '\t' };
                               deviceName = deviceName.Replace(strQuotes, ' ');
                               //deviceName = deviceName.Replace(backslash, "\\");
                               string[] arrdeviceNames = deviceName.Split('=');
                               deviceName = arrdeviceNames[1];
                               deviceName = deviceName.Trim(charsToTrim);
                               CameraEvent = true;


                               break;

                           }

                       }

                       if (CameraEvent)
                       {
                           if (e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent")
                           {
                               ManagementObjectSearcher searcher =
                                   new ManagementObjectSearcher("root\\CIMV2", "Select * From Win32_PnPEntity where DeviceID ='" + deviceName + "'");

                               ManagementObjectCollection allVolumes = searcher.Get();

                               foreach (ManagementObject vol in allVolumes)
                               {


                                 uint valu =  Convert.ToUInt32( (uint)vol["ConfigManagerErrorCode"]);
                                   GUID_DEVCLASS_ACTIVEDEVICE = new Guid((string)vol["ClassGuid"]);
                                   if (GUID_DEVCLASS_ACTIVEDEVICE == GUID_DEVCLASS_DIGITALCAMERA)
                                   {
                                       StatusChanged(true);


                                   }

                               }
                           }
                           else if (e.NewEvent.ClassPath.ClassName == "__InstanceDeletionEvent")
                           {

                               if (GUID_DEVCLASS_ACTIVEDEVICE == GUID_DEVCLASS_DIGITALCAMERA)
                               
                                   StatusChanged(false);

                               GUID_DEVCLASS_ACTIVEDEVICE = new Guid();


                           }
                       }


                   }
               }
               catch (Exception ex)
               { }

               //}

           }//if (pdata != null)

            
        }

        public void CheckPnPPoint()
        {
           ManagementObjectSearcher searcher =
                                  new ManagementObjectSearcher("root\\CIMV2", "Select * From Win32_USBControllerDevice");

            ManagementObjectCollection allVolumes = searcher.Get();
            string deviceName = string.Empty;

            
            foreach (ManagementObject manob in allVolumes)
            {
                
                deviceName = (string)manob["Dependent"];


                char strQuotes = '"';

                char[] charsToTrim = { ' ', '\t' };
                deviceName = deviceName.Replace(strQuotes, ' ');
                string[] arrdeviceNames = deviceName.Split('=');
                deviceName = arrdeviceNames[1];
                deviceName = deviceName.Trim(charsToTrim);


                searcher = new ManagementObjectSearcher("root\\CIMV2", "Select * From Win32_PnPEntity where DeviceID ='" + deviceName + "'");

                allVolumes = searcher.Get();

                foreach (ManagementObject vol in allVolumes)
                {
                    if (vol["ClassGuid"] == null)
                    {
                        Logger.Basic.Debug("ClassGuid is null");
                        return;

                    }//end of  if (vol["ClassGuid"] == null)
 
                    GUID_DEVCLASS_ACTIVEDEVICE = new Guid((string)vol["ClassGuid"]);
                    if (GUID_DEVCLASS_ACTIVEDEVICE == GUID_DEVCLASS_DIGITALCAMERA)
                    {
                        string status = Convert.ToUInt16((string)vol["StatusInfo"]).ToString();
                        StatusChanged(true);
                        break;
                    }
                    else
                        StatusChanged(false);

                }//end of foreach (ManagementObject vol in allVolumes)

            }// end of foreach (ManagementObject manob in allVolumes)

           


        }

    
       public void  FinalizeAllResource()
       {
           m_mewWatcher.Stop();
           m_mewWatcher.Dispose();
       }

    }
}
