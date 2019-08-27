using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortableDeviceApiLib;
using System.Windows;
using System.Collections.ObjectModel;
using System.Management;
using System.Windows.Threading;
using System.Threading;
using System.Configuration;
using PhotoUploader.Common;

namespace PhotoUploader.Service
{
    public class PWDManager : IDisposable
    {
        public delegate void DigitalDeviceAttached(bool status,string name);
        public DigitalDeviceAttached CameraConnectionStatusChanged;

        public delegate void DeviceErrorMessageThrowed(int state, string message);
        public DeviceErrorMessageThrowed DeviceErrorMessageThrow;

       public delegate void DeviceName(string name);
       public DeviceName ReturnDeviceName;

       public delegate void delFetchedImageCount(int number, int totalimagecount);
       public delFetchedImageCount FetchedImageCountChanged;

       public delegate void delGetImageCountChanged(int count);
       public delGetImageCountChanged GetImageCountChanged;

       public delegate void delImageNumberChanged(int number, int totalimgcount);
       public delImageNumberChanged ImageNumberChanged;

       public delegate void delNotificationChanged(int state, string message);
       public delNotificationChanged NotificationChanged;

       public delegate void delPlugAndPlayMessage(string message);
       public delPlugAndPlayMessage PlugAndPlayChanged;

       private bool devicestate = false;

       string ImageFolder = string.Empty;
       List<ImageItem> _imageList = null;
       ImageManager _imgManager = null;

      

       static ManagementEventWatcher m_mewWatcher;
        public PWDManager()
        {
            ImageFolder = (string)ConfigurationManager.AppSettings["ImageFolder"];
            _imageList = new List<ImageItem>();
            //imgManager = new ImageManager();
        }
        public void Dispose()
        {
            m_mewWatcher.Dispose();

        }

        public void CheckPnPPoint()
        {
            var watcherInserted = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
            watcherInserted.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);

            watcherInserted.Query = query;
            watcherInserted.Start();



            var watcherRemoval = new ManagementEventWatcher();
            query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
            watcherRemoval.EventArrived += new EventArrivedEventHandler(watcherRemoval_EventArrived);

            watcherRemoval.Query = query;
            watcherRemoval.Start();


            //WqlEventQuery weqQuery = new WqlEventQuery();
            //weqQuery.EventClassName = "__InstanceOperationEvent";
            //weqQuery.WithinInterval = new TimeSpan(0,0,0,0,200);
            //weqQuery.Condition = @"TargetInstance ISA 'Win32_PnPEntity'";

            //ManagementScope msScope = new ManagementScope("root\\CIMV2","SELECT * FROM __InstanceOperationEvent WITHIN 0.5 where TargetInstance ISA 'Win32_PnPEntity'"));
            //msScope.Options.EnablePrivileges = true;
            //m_mewWatcher = new ManagementEventWatcher("root\\CIMV2","SELECT * FROM Win32_PnPEntity where ClassGuid ='eec5ad98-8080-425f-922a-dabf3de3f69a'");
          
            //m_mewWatcher.EventArrived += new EventArrivedEventHandler(m_mewWatcher_EventArrived);
            //m_mewWatcher.Start();

            Thread.Sleep(300);
            if (devicestate == false)
            {
                GetDeviceName(PhotoUploader.Properties.Resources.main_hdr_disconnected);
                StatusChanged(false, PhotoUploader.Properties.Resources.main_hdr_disconnected);                
                GetPlugAndPlayChanged(PhotoUploader.Properties.Resources.main_msg_plugandunplug);
              
            }
            
        
        }


        private int FindDevice()
        {
            int deviceStatus = 0;
            ManagementObjectSearcher searcher =
                                new ManagementObjectSearcher("root\\CIMV2", "SELECT ClassGuid FROM Win32_PnPEntity");

            //get a collection of WMI objects
            ManagementObjectCollection queryCollection = searcher.Get();

            //enumerate the collection.
            foreach (ManagementObject m in queryCollection)
            {
                if (m["ClassGuid"] == null)
                {
                   
                    deviceStatus = 2;
                    Logger.Basic.Debug("FindDevice : " + deviceStatus.ToString());
                    break;
                }
                else if (m["ClassGuid"].ToString().Contains("eec5ad98-8080-425f-922a-dabf3de3f69a"))
                {
                    deviceStatus = 1;
                    Logger.Basic.Debug("FindDevice : " + deviceStatus.ToString());
                    break;
                }
                

            }

            return deviceStatus;
        }


        private void GetPlugAndPlayChanged(string message)
        {
            if (PlugAndPlayChanged == null)
                return;

            PlugAndPlayChanged(message);
        }
        private void GetImageCount(int imgCount)
        {
            if (GetImageCountChanged == null)
                return;

            GetImageCountChanged(imgCount);
        }
        private void FetchImageCount(int number,int totalcountof)
        {
            if (FetchedImageCountChanged == null)
                return;

            FetchedImageCountChanged(number,totalcountof);
        }

       private void GetDeviceName(string name)
       {
           if (ReturnDeviceName == null)
            return;

           ReturnDeviceName(name);


       }
        private void GetDeviceErrorMessageThrow(int state, string message)
        {
            if (DeviceErrorMessageThrow == null)
                return;

            DeviceErrorMessageThrow(state, message);
        }


        private void StatusChanged(bool status,string name)
        {
            try
            {
                if (CameraConnectionStatusChanged == null)
                    return;


                CameraConnectionStatusChanged(status,name);
            }
            catch (Exception ex)
            {
                Logger.Basic.Error("StatusChanged : " + ex.ToString(), ex);
            }
        }

        private  void m_mewWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {

            if (e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent")
            {
                //Console.WriteLine("USB was plugged in");
                foreach (PropertyData pdData in e.NewEvent.Properties)
                {
                   if( pdData.Value != null && pdData.Value.GetType() == typeof(ManagementBaseObject))
                   {
                       ManagementBaseObject mbo = (ManagementBaseObject)pdData.Value;
                       if (mbo != null)
                       {
                           foreach (PropertyData pdDataSub in mbo.Properties)
                           {
                               if (pdDataSub.Name == "ClassGuid" && pdDataSub.Value.ToString() == "{eec5ad98-8080-425f-922a-dabf3de3f69a}")
                               {
                                   var collection = new PortableDeviceCollection();

                                   collection.Refresh();

                                   foreach (var device in collection)
                                   {
                                       device.Connect();
                                       GetDeviceName("Connected " + device.FriendlyName);
                                       devicestate = true;
                                       StatusChanged(true, "Connected " + device.FriendlyName);
                                       device.Disconnect();
                                       break;
                                   }
                               }
                           }


                       }
                   }
                   
                }

            }
            else if (e.NewEvent.ClassPath.ClassName == "__InstanceDeletionEvent")
            {

                foreach (PropertyData pdData in e.NewEvent.Properties)
                {
                    if (pdData.Value != null &&  pdData.Value.GetType() == typeof(ManagementBaseObject))
                    {
                        ManagementBaseObject mbo = (ManagementBaseObject)pdData.Value;
                        if (mbo != null)
                        {
                            foreach (PropertyData pdDataSub in mbo.Properties)
                            {
                                if (pdDataSub.Name == "ClassGuid" && pdDataSub.Value.ToString() == "{eec5ad98-8080-425f-922a-dabf3de3f69a}")
                                {
                                    devicestate = false;
                                    countofimg = 0;
                                    deleteditem = 0;
                                    StatusChanged(false, "Disconnected");
                                    GetDeviceName("Disconnected");
                                }
                            }


                        }
                    }
                }

            }
        }


        public void watcherRemoval_EventArrived(object o, EventArrivedEventArgs e)
        {
            ManagementEventWatcher obj = (ManagementEventWatcher)o;
            obj.Stop();
            obj.EventArrived -= new EventArrivedEventHandler(watcherRemoval_EventArrived);
               
           
            //int deviceStatus = FindDevice();
           

            if (deviceStatus == 2 && devicestate == true)
               {
                   deviceStatus = 1;
                   devicestate = false;
                   countofimg = 0;
                   deleteditem = 0;
                   GetDeviceName(PhotoUploader.Properties.Resources.main_hdr_disconnected);
                   StatusChanged(false, PhotoUploader.Properties.Resources.main_hdr_disconnected);
               }
            obj.EventArrived += new EventArrivedEventHandler(watcherRemoval_EventArrived);
            obj.Start();
        }

        int deviceStatus = 1;

        public void watcher_EventArrived(object o, EventArrivedEventArgs e)
        {

            
            ManagementEventWatcher obj = (ManagementEventWatcher)o;

            obj.Stop();
            obj.EventArrived -= new EventArrivedEventHandler(watcher_EventArrived);

            //int deviceStatus = FindDevice();
           
            

            if (deviceStatus ==1 && devicestate == false)
            {

              
                var collection = new PortableDeviceCollection();

                collection.Refresh();

                foreach (var device in collection)
                {
                    device.Connect();
                    GetDeviceName(PhotoUploader.Properties.Resources.main_hdr_connected +" "+ device.FriendlyName);
                    devicestate = true;
                    deviceStatus = 2;
                    StatusChanged(true, PhotoUploader.Properties.Resources.main_hdr_connected + " " + device.FriendlyName);
                    device.Disconnect();
                    break;
                }

            }


            //obj = (ManagementEventWatcher)o;
            obj.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            obj.Start();

        }


        public void GetCountOfImage(ImageManager imgManager)
        {
            int total = 0;
            _imgManager = imgManager;

            try
            {


                if (devicestate == true)
                {

                    var collection = new PortableDeviceCollection();

                    collection.Refresh();

                    foreach (var device in collection)
                    {
                        device.Connect();

                        var folder = device.GetContents();
                        foreach (var item in folder.Files)
                        {

                            total = GetCountOfImageRec(device, item);
                        }



                        device.Disconnect();

                    }

                }
                else
                    total = -1;


                totalimagecount = total > -1 ? total : 0;
                GetImageCount(total);
                _imageList = _imgManager.GetImagesFromDirectory();

                if (total == 0)
                {
                    Action action = () => FetchImageCount(0, 0);
                    Dispatcher.CurrentDispatcher.Invoke(action);
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                Logger.Basic.Error("GetCountOfImage", ex);
            }

        }




        private int GetCountOfImageRec(PortableDevice device, PortableDeviceObject portableDeviceObject)
        {
            int count = 0;

            foreach (var item in ((PortableDeviceFolder)portableDeviceObject).Files)
            {
                if (item is PortableDeviceFolder)
                {
                    count = count + GetCountOfImageRec(device, (PortableDeviceFolder)item);
                }


                if (item is PortableDeviceFile)
                {
                    count++;
                }
            }

            return count;
           
        }

        public static bool isdeleteoperation = false;
        public static bool isfetchingrunning = false;

        public void GetAllImagesFromCamera()
        {
            lock (this)
            {
                if (isdeleteoperation)
                {
                    Monitor.Wait(this);


                }

                isfetchingrunning = true;
                if (totalimagecount > 0 && countofimg < totalimagecount)
                {
                    var collection = new PortableDeviceCollection();

                    collection.Refresh();

                    foreach (var device in collection)
                    {
                        
                        device.Connect();
                        var folder = device.GetContents();
                        foreach (var item in folder.Files)
                        {
                            DisplayObject(device, item);
                        }

                        device.Disconnect();

                    }
                }

                isfetchingrunning = false;
                Monitor.Pulse(this);
                Monitor.Wait(this);
            }//end of lock 

        }// end of  public void GetAllImagesFromCamera()
        private  void DisplayObject(PortableDevice device, PortableDeviceObject portableDeviceObject)
        {
           
            if (portableDeviceObject is PortableDeviceFolder)
            {
                DisplayFolderContents(device, (PortableDeviceFolder)portableDeviceObject);
            }
        }

        private int deletedmessagecount = 0;
        private  void DisplayFolderContents(PortableDevice device, PortableDeviceFolder folder)
        {
            if (isdeleteoperation == false && isfetchingrunning == true)
            {

                foreach (var item in folder.Files)
                {
                    //Console.WriteLine(item.Id);

                    if (item is PortableDeviceFolder)
                    {
                        DisplayFolderContents(device, (PortableDeviceFolder)item);
                    }
                    if (item is PortableDeviceFile)
                    {
                        if (!devicestate)
                            break;
                        RealDownloadOperation(device, (PortableDeviceFile)item);
                    }
                }
            }

            if (isdeleteoperation == true && isfetchingrunning == false)
            {
                

                foreach (var item in folder.Files)
                {
                    if (item is PortableDeviceFolder)
                    {
                        DisplayFolderContents(device, (PortableDeviceFolder)item);
                    }

                  
                   
                        if (item is PortableDeviceFile)
                        {

                            if (!devicestate)
                                break;
                       
                            RealDeleteOperation(device, (PortableDeviceFile)item,_deletedFiles,_willbeuploaded);


                        }
                    
                   
                    
                     
                    
                }
            }
           

        }
        private static int totalimagecount = 0;
        public static int countofimg = 0;
        private void RealDownloadOperation(PortableDevice device, PortableDeviceFile item)
        {
            ImageItem imageItem = null;
           
           IPortableDeviceContent content = device.GetContentofDevice();

           DateTime createddate = device.GetObjectCreationTime(content, item.Id);
           imageItem = new ImageItem(0, item.Id, item.Id, createddate);

           if (_imageList.Exists((ImageItem t) => { return t.UniqueIdentifier == item.Id; }))
           {
               countofimg++;
               Action action = () => FetchImageCount(countofimg, totalimagecount);
               Dispatcher.CurrentDispatcher.Invoke(action);
               Thread.Sleep(200);
           }else if ( (!_imageList.Exists((ImageItem t) => { return t.UniqueIdentifier == item.Id; }) ||
                                     _imageList.Count == 0))
            {

                countofimg++;
                _imageList.Add(imageItem);

               bool isok = device.DownloadFile((PortableDeviceFile)item, ImageFolder);

               if (isok)
               {
                   Action action = () => FetchImageCount(countofimg, totalimagecount);
                   Dispatcher.CurrentDispatcher.Invoke(action);
                   Thread.Sleep(200);
               }



            }

           if (countofimg >= totalimagecount)
           {
               countofimg = 0;
               totalimagecount = 0;
           }

            //else if (totalimagecount > 0 && countofimg < totalimagecount)
            //{
               
            //    countofimg++;
            //    Action action = () => FetchImageCount(countofimg, totalimagecount);
            //    Dispatcher.CurrentDispatcher.Invoke(action);
            //    Thread.Sleep(200);

            //    if (countofimg == totalimagecount)
            //    {
            //        countofimg = 0;
            //        totalimagecount = 0;
            //    }


            //}

            //if (totalimagecount > 0)
            //{
            //    if (countofimg == totalimagecount)
            //    {
            //        countofimg = 0;
            //        totalimagecount = 0;
            //    }
            //    device.DownloadFile((PortableDeviceFile)item, ImageFolder);
            //}


            
        }

        private bool deleteoperationcanbedone = false;
        public static int deleteditem = 0;
        private void RealDeleteOperation(PortableDevice device, PortableDeviceFile item, List<ImageItem> deletedFiles, List<ImageItem> willbeuploaded)
        {
            try
            {


                foreach (ImageItem imageItem in deletedFiles)
                {
                    if (imageItem.UniqueIdentifier == item.Id)
                    {
                        deleteoperationcanbedone = true;
                        _imgManager.DeleteImageFromDirectory(imageItem);
                        device.DeleteFile((PortableDeviceFile)item);
                        deleteditem++;
                        Action action = () => ImageNumber(deleteditem, deletedFiles.Count);
                        Dispatcher.CurrentDispatcher.Invoke(action);
                        Thread.Sleep(100);
                        break;
                    }
                }

                if (!deleteoperationcanbedone && willbeuploaded.Count > 0)
                {

                    SetNotification(1,PhotoUploader.Properties.Resources.main_msg_imagenoexist);

                    Action action1 = () => ImageNumber(-1, deletedFiles.Count);
                    Dispatcher.CurrentDispatcher.Invoke(action1);
                    Thread.Sleep(100);
                }

            }
            catch (Exception ex)
            {
                Logger.Basic.Error("PWDManager Delete Operation : " + ex.ToString(), ex);
            }
          
        }

        private void SetNotification(int state, string message)
        {
            if (NotificationChanged == null)
                return;

            NotificationChanged(state, message);

        }

        private void ImageNumber(int number, int totalimgcount)
        {
            if (ImageNumberChanged == null)
                return;

            ImageNumberChanged(number, totalimgcount);
        }


        List<ImageItem> _deletedFiles;
        List<ImageItem> _willbeuploaded;
        public void DeleteImagesFromCamera(ImageManager imgManager, List<ImageItem> deletedFiles,List<ImageItem> willbeuploaded)
        {
            lock (this)
            {
                if (isfetchingrunning)
                {
                    Monitor.Wait(this);
                }


                isdeleteoperation = true;
                _deletedFiles = deletedFiles;
                _willbeuploaded = willbeuploaded;
                _imgManager = imgManager;


                var collection = new PortableDeviceCollection();

                collection.Refresh();

                foreach (var device in collection)
                {
                    device.Connect();
                    var folder = device.GetContents();
                    foreach (var item in folder.Files)
                    {
                        DisplayObject(device, item);
                    }

                    device.Disconnect();

                }

                isdeleteoperation = false;
                Monitor.Pulse(this);
                Monitor.Wait(this);

            }//end of lock

        }//end of  public void DeleteImagesFromCamera()


    }



}
