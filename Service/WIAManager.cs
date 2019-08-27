using PhotoUploader.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WIA;

namespace PhotoUploader.Service
{
   
   

    public class CheckScoutboxResult
    {
        bool scoutboxFound = false;

        public bool ScoutboxFound
        {
            get { return scoutboxFound; }
            set { scoutboxFound = value; }
        }

        int imageCount = 0;

        public int ImageCount
        {
            get { return imageCount; }
            set { imageCount = value; }
        }

        public CheckScoutboxResult()
        {

        }
    }

    public class WIAManager : IDisposable
    {

        public delegate void delFindCameraModelChanged(string cameraName);
        public delegate void delGetImageCountChanged(int count);
        public delegate void delImageNumberChanged(int number,int totalimgcount);
        public delegate void delNotificationChanged(int state,string message);
        public delegate void delImageNumberTobeUploadFtp(int count);
        public delegate void delDeleteOperationCanBeDone(bool count,int countofImage);

        public delegate void delFetchedImageCount(int number,int totalimagecount);

        public delegate void delFetchIsDone(bool done);

        public delFindCameraModelChanged FindCameraModelMethodChanged;
        public delGetImageCountChanged GetImageCountChanged;
        public delImageNumberChanged ImageNumberChanged;
        public delNotificationChanged NotificationChanged;
        public delImageNumberTobeUploadFtp ImageNumberTobeUploadFtp;
        public delDeleteOperationCanBeDone DeleteOperationCanBeDone;

        public delFetchedImageCount FetchedImageCountChanged;

        public delFetchIsDone IsFetchDoneCreated;

        bool disposed = false;

        #region Variables

        string ImageFolder = string.Empty;

        //Control parent;
        private WIA.DeviceManager wiaDevManager = null;
        List<ImageItem> _imageList = null;

        public static Device device = null;
        ImageManager imgManager = null;

        public bool ScoutBoxSate = false; 
       
       //public BackgroundWorker worker = new BackgroundWorker();
       

        #endregion


        public void Dispose()
        {
            Dispose(true);
            GC.WaitForPendingFinalizers();
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (device != null && Marshal.IsComObject(device))
                {
                    Marshal.ReleaseComObject(device);
                }

                if(wiaDevManager != null && Marshal.IsComObject(wiaDevManager))
                {

                    if (wiaDevManager.DeviceInfos.Count > 0)
                    {
                        foreach (IDeviceInfo DevInfo in wiaDevManager.DeviceInfos)
                        {
                            if (DevInfo != null && Marshal.IsComObject(DevInfo))
                            {
                                Marshal.ReleaseComObject(DevInfo);
                            }

                          

                        }//end of foreach (IDeviceInfo DevInfo in wiaDevManager.DeviceInfos)
                    }// end of  if (wiaDevManager.DeviceInfos.Count > 0) 


                    Marshal.ReleaseComObject(wiaDevManager);
                }         
                
            }

            if(imgManager != null)
            imgManager.Close();

            //if (worker.IsBusy)
            //    worker.CancelAsync();
          
            // Free any unmanaged objects here.
            //
            disposed = true;
        }


          ~WIAManager()
           {
              Dispose(false);
           }


        #region Properties


          private void NotifyDeleteOperationDone(bool count,int countofImage)
          {
              if (DeleteOperationCanBeDone == null)
                  return;

              DeleteOperationCanBeDone(count, countofImage);

          }

        private void SetNotification(int state, string message)
        {
            if (NotificationChanged == null)
                return;
            
            NotificationChanged(state,message);
        
        }


        private void FetchImageCount(int number,int totalcountof)
        {
            if(FetchedImageCountChanged == null)
                return;

            FetchedImageCountChanged(number,totalcountof);
        }

        private void ImageNumber(int number,int totalimgcount)
        {
            if (ImageNumberChanged == null)
                return;

            ImageNumberChanged(number, totalimgcount);
        }

        private void GetCameraName(string CameraName)
        {
            if (FindCameraModelMethodChanged == null)
                return ;

            FindCameraModelMethodChanged(CameraName);
        }


        private void GetImageCountUploadedFtp(int imgCount)
        {
            if (ImageNumberTobeUploadFtp == null)
                return;

            ImageNumberTobeUploadFtp(imgCount);
        }
     
        private void GetImageCount(int imgCount)
        {
            if (GetImageCountChanged == null)
                return;

            GetImageCountChanged(imgCount);
        }


        private void IsFetchDonePrivate(bool isfetched)
        {
            if (IsFetchDoneCreated == null) return;

            IsFetchDoneCreated(isfetched);

        }


        #endregion

        #region Constructor

        public WIAManager()
        {

            ImageFolder = (string)ConfigurationManager.AppSettings["ImageFolder"];
            _imageList = new List<ImageItem>();
            imgManager = new ImageManager();

            //worker.WorkerReportsProgress = true;
            //worker.WorkerSupportsCancellation = true;
          

        }

      

        #endregion

       
        public void Initialize(string messages)
        {
            wiaDevManager = new WIA.DeviceManagerClass();

            SetNotification(0,"Wia Manager has been initialized");
           
        }
    

        #region Public Functions


        
        public void FindCameraDevice(int state,string messages)
        {

            try
            {
                //if (state == 1)
                //{
                //    Initialize(string.Empty);
                //    Logger.Basic.Debug("FindCameraDevice = state = 1 delete operation is started!!!");
                //}
              
                if (wiaDevManager.DeviceInfos.Count > 0)
                {
                    foreach (IDeviceInfo DevInfo in wiaDevManager.DeviceInfos)
                    {
                        
                      string deviceid = DevInfo.DeviceID.ToString();
                      string devicetype =  DevInfo.Type.ToString();

                      if (DevInfo.Type == WiaDeviceType.CameraDeviceType && DevInfo.DeviceID.Contains("{EEC5AD98-8080-425f-922A-DABF3DE3F69A}"))
                        {
                          
                            Device localDevice = device = DevInfo.Connect();
                            
                              if(state == 0)
                              SetNotification(0,"ScoutBox is connected.");
                              return;  
                          
                            //OnConnectStatusChanged(true, "ScoutBox Connected");

                        }// end of  if (DevInfo.Type == WiaDeviceType.CameraDeviceType && DevInfo.DeviceID.Contains("{EEC5AD98-8080-425f-922A-DABF3DE3F69A}"))

                    }//end of foreach (IDeviceInfo DevInfo in wiaDevManager.DeviceInfos)
                }// end of  if (wiaDevManager.DeviceInfos.Count > 0) 

            }
            catch (Exception ex)
            {
                Logger.Basic.Error("FindCameraDevice = " + ex.ToString(), ex);
                FindCameraDevice(2,"");
            }

            if (state == 0)
            {
                SetNotification(0,"Scoutbox is not connected. ");
              
            }
            return;
        }

       
      

        public static bool fetching = false;

        private static int totalimagecount = 0;

        public void FetchImagesFromCamera(int FileTransferValue)
        {
            string propertyValue = string.Empty;
            totalimagecount = 0;

            lock (this)
            {

                if (isdeleteoperation || isfetchingrunning)
                {
                    Monitor.Wait(this);
                }

                try
                {
                    if (device != null)
                    {

                        //while (!deleteoperationisdone)
                        //{
                        //    Thread.Sleep(2000);
                        //}

                        foreach (WIA.IProperty prop in device.Properties)
                        {
                            if (prop.Name == "Name" && fetching == false)
                            {

                                propertyValue = prop.get_Value().ToString();
                                GetCameraName(propertyValue);
                                int countofimage = totalimagecount= GetAllImageCount(device.Items);
                                GetImageCount(countofimage);
                                _imageList = imgManager.GetImagesFromDirectory();

                                if (countofimage == 0)
                                {
                                    Action action = () => FetchImageCount(0,0);
                                    Dispatcher.CurrentDispatcher.Invoke(action);
                                    Thread.Sleep(200);
                                }
                                //Action action = () =>  SetNotification("Fetching the images from Scoutbox has started.");
                                //Dispatcher.CurrentDispatcher.Invoke(action);   


                                imgManager.WriteAllImageList(_imageList);
                                //countofimagae = GetAllImageCount(device.Items);
                                //if (_imageList.Count > countofimage)
                                //{
                                //     countofimage = _imageList.Count;
                                    
                                //}
                                //GetImageCount(countofimage);
                                break;
                            }
                        }



                    }

                }
                catch (Exception ex)
                {
                    //FetchImagesFromCamera(1);

                }


                Monitor.PulseAll(this);
                
            
            }
         
           
         
            

          

            

           

        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                Logger.Basic.Error("Get Images :" + e.ToString(), e.Error);

        }// end of  public void FetchImagesFromCamera()





        public int GetAllImageCount(Items deviceItems)
        {
            int a = 0;
            foreach (Item item in deviceItems)
            {
                if (item.Items.Count == 0)
                {
                    foreach (Property prop in item.Properties)
                    {
                        //extension
                        if (prop.Name.ToLower().Contains("extension") && prop.get_Value().ToString().ToLower().Contains("jpg"))
                        {
                            a++;
                        }
                    }
                }
                else
                {
                    a = a + GetAllImageCount(item.Items);
                }
            }
            return a;
        }

        public static bool isfetchingrunning = false;

        public static int countofimg = 0;

        public void GetAllImagesFromCamera(Items deviceItems)
        {

            lock (this)
            {
                if (isdeleteoperation)
                {
                    Monitor.Wait(this);
                    

                }

                
                

                isfetchingrunning = true;
                FindCameraDevice(1, "");



                try
                {


                    //worker.DoWork += (sender, args) =>
                    //    {
                    foreach (Item item in deviceItems)
                    {



                        bool isImage = false;
                        string itemName = "";
                        string uniqueId = "";

                        DateTime takentime = DateTime.Now;
                        ImageItem imageItem = null;

                        if (item.Items.Count == 0)
                        {

                            foreach (Property prop in item.Properties)
                            {
                                if (prop.Name.ToLower().Contains("extension") && prop.get_Value().ToString().ToLower().Contains("jpg"))
                                {
                                    isImage = true;

                                }
                                
                                    if (prop.Name.Contains("Item Name"))
                                        itemName = prop.get_Value().ToString();
                                    if (prop.Name == "Full Item Name")
                                    {
                                        uniqueId = prop.get_Value().ToString();


                                    }
                                    if (prop.Name == "Item Time Stamp")
                                    {
                                        dynamic pdata = prop.get_Value();
                                        takentime = pdata.Date;
                                    }
                                


                            }





                            if (isImage)
                            {
                                imageItem = new ImageItem(0, uniqueId, itemName, takentime);
                            }



                            //try
                            //{
                            if (isImage == true &&
                                       (!_imageList.Exists((ImageItem t) => { return t.UniqueIdentifier == imageItem.UniqueIdentifier; }) ||
                                       _imageList.Count == 0)
                                       )
                            {

                                countofimg++;
                                _imageList.Add(imageItem);
                               
                                Action action = () => FetchImageCount(countofimg, totalimagecount);
                                Dispatcher.CurrentDispatcher.Invoke(action);
                                Thread.Sleep(200);

                             

                            }
                            else if(totalimagecount > 0 && countofimg < totalimagecount)
                            {
                                isImage = false;
                                countofimg++;
                                Action action = () => FetchImageCount(countofimg, totalimagecount);
                                Dispatcher.CurrentDispatcher.Invoke(action);
                                Thread.Sleep(200);

                                if (countofimg == totalimagecount)
                                {
                                    countofimg = 0;
                                    totalimagecount = 0;
                                }


                            }


                            //SetNotification("Check and Fetch Operation is starting");




                            if (isImage && totalimagecount > 0)
                            {
                                if (countofimg == totalimagecount)
                                {
                                    countofimg = 0;
                                    totalimagecount = 0;
                                }

                                object obj = item.Transfer(FormatID.wiaFormatJPEG);
                                var wiaImageFile = (WIA.ImageFile)obj;
                                var stream = new System.IO.MemoryStream((byte[])wiaImageFile.FileData.get_BinaryData());
                                var streamCopy = new System.IO.MemoryStream();
                                stream.Seek(0, System.IO.SeekOrigin.Begin);
                                stream.WriteTo(streamCopy);
                                streamCopy.Seek(0, System.IO.SeekOrigin.Begin);
                                string imgpath = ImageFolder + "IMG_1_" + imageItem.TakenTime.ToString() + "_" + imageItem.Filename + ".jpg";
                                System.IO.File.WriteAllBytes(imgpath, stream.ToArray());
                                //SetNotification(imageItem.Filename + " named image has been fetched from digital camera");

                                if(Marshal.IsComObject(obj))
                                   Marshal.ReleaseComObject(obj);
                                if(Marshal.IsComObject(item))
                                  Marshal.ReleaseComObject(item);


                            }
                            //}










                        }
                        else
                        {
                            GetAllImagesFromCamera(item.Items);
                        }


                    }//end of  foreach (Item item in deviceItems)
                    //}


                    //worker.ProgressChanged += (sender, args) =>
                    //{

                    //    ImageNumber(args.ProgressPercentage);
                    //};



                    //worker.RunWorkerAsync();




                }
                catch (Exception ex)
                {
                    if (ex.ToString().Contains("210005"))
                    {
                        //isThreadRunning = false;
                        //fetching = false;
                        //SetNotification("Error O: The device is offline.ScoutBox uploader is trying to re-open");
                        //FindCameraDevice(1, string.Empty);
                        //deviceItems = device.Items;
                        //GetAllImagesFromCamera(deviceItems);
                    }
                    else if (ex.ToString().Contains("210006"))
                    {
                        SetNotification(0,"Error B: The device is busy. Close any apps that are using this device.Please reconnect digital camera to computer.");
                    }
                    else if (ex.ToString().Contains("210009"))
                    {
                        SetNotification(0,"Error D: The WIA device was deleted. It's no longer available.Please reconnect digital camera to computer.");
                    }
                    else if (ex.ToString().ToLower().Contains("7001F"))
                    {
                        SetNotification(0,"Error: This error is caused by improper deletion of application.Please Restart Computer.");
                    }

                    //SetNotification(0,"countofimg : " + countofimg.ToString());
                    countofimg = 0;
                    //FindCameraDevice(1, string.Empty);
                    //FetchImagesFromCamera(0);

                    Logger.Basic.Error("GetAllImagesFromCamera = " + ex.ToString(), ex);


                }


               

                isfetchingrunning = false;
                Monitor.Pulse(this);
                Monitor.Wait(this);

            }
            
           
                //IsFetchDonePrivate(true);
           

           
        }// end of   public void GetAllImagesFromCamera(Items deviceItems)



       

        private int deletedmessagecount = 0;

        public static bool isdeleteoperation = false;

        private bool deleteoperationcanbedone = false;

        public static int deleteditem = 0;
        public void DeleteImagesFromCamera(string messages)
        {

            lock (this)
            {
                if (isfetchingrunning)
                {
                    Monitor.Wait(this);
                }


                isdeleteoperation = true;
                try
                {




                    FindCameraDevice(1, messages);
                    Items deviceItems = device.Items;

                    int count = deviceItems.Count;
                    int index = 1;
                    deleteoperationcanbedone = false;

                    List<ImageItem> deletedFiles = imgManager.GetImagesUploadedToFtp();
                    List<ImageItem> willbeuploaded = imgManager.GetImagesWatingToUploadToFtp();


                    if (deletedmessagecount != willbeuploaded.Count)
                    {
                        deletedmessagecount = willbeuploaded.Count;
                        //GetImageCountUploadedFtp(willbeuploaded.Count);
                    }
                    //else if (deletedmessagecount != deletedFiles.Count && deletedFiles.Count == 0)
                    //{
                    //    deletedmessagecount = deletedFiles.Count;
                    //    GetImageCount(0);
                    //}



                    if (deletedFiles.Count > 0)
                    {


                        for (index = 1; index <= count; index++)
                        {
                            string filename = "";
                            string uniqueId = "";

                            ItemClass item = (ItemClass)deviceItems[index];

                            foreach (WIA.IProperty prop in item.Properties)
                            {
                                if (prop.Name == "Item Name")
                                    filename = prop.get_Value().ToString();

                                if (prop.Name == "Full Item Name")
                                    uniqueId = prop.get_Value().ToString();
                            }

                            if (uniqueId != "")
                            {

                                foreach (ImageItem imageItem in deletedFiles)
                                {
                                    if (imageItem.UniqueIdentifier == uniqueId)
                                    {
                                        deleteoperationcanbedone = true;
                                        //deleteoperationisdone = false;
                                        deviceItems.Remove(index);

                                        imgManager.DeleteImageFromDirectory(imageItem);
                                        index = 1;
                                        FindCameraDevice(1, messages);
                                        deviceItems = device.Items;
                                        count = deviceItems.Count;
                                        //SetNotification(imageItem.UniqueIdentifier + " named image deleted");
                                        deleteditem++;

                                        Action action = () => ImageNumber(deleteditem,deletedFiles.Count);
                                        Dispatcher.CurrentDispatcher.Invoke(action);
                                        Thread.Sleep(100);

                                     

                                        break;

                                    }
                                }

                                //if (ScoutBoxSate)
                                //    NotifyDeleteOperationDone(deleteoperationcanbedone, deletedFiles.Count);



                            }// end of uniqueId != ""

                            if (!deleteoperationcanbedone && willbeuploaded.Count == 0)
                            {
                                Action action1 = () => ImageNumber(0, deletedFiles.Count);
                                Dispatcher.CurrentDispatcher.Invoke(action1);
                                Thread.Sleep(100);
                            }
                            else if (!deleteoperationcanbedone && willbeuploaded.Count > 0)
                            {

                                SetNotification(1,"Image(s) in C:\\ScoutboxImages folder does not exist in the camera of the connected Scoutbox, therefore can not be deleted. ");

                                Action action1 = () => ImageNumber(-1, deletedFiles.Count);
                                Dispatcher.CurrentDispatcher.Invoke(action1);
                                Thread.Sleep(100);
                            }


                        }// end of for (index = 1; index <= count; index++)

                    }
                    else
                    {
                        if (willbeuploaded.Count == 0)
                        {
                            Action action1 = () => ImageNumber(0, deletedFiles.Count);
                            Dispatcher.CurrentDispatcher.Invoke(action1);
                            Thread.Sleep(100);
                        }else if(willbeuploaded.Count > 0){
                        
                         Action action1 = () => ImageNumber(-1, deletedFiles.Count);
                            Dispatcher.CurrentDispatcher.Invoke(action1);
                            Thread.Sleep(100);
                        
                        }
                        
                        //if (ScoutBoxSate)
                        //    NotifyDeleteOperationDone(deleteoperationcanbedone, 0);
                        //deleteoperationisdone = true;
                    }





                    //end of  if (deletedFiles.Count > 0)

                }
                catch (Exception ex)
                {
                    SetNotification(0, "Delete Images Camera");
                    Logger.Basic.Error("DeleteImagesFromCamera = " + ex.ToString(), ex);
                }

                isdeleteoperation = false;
                Monitor.Pulse(this);
                Monitor.Wait(this);


            }


        }// end  of  public void DeleteImagesFromCamera(Items deviceItems,ImageItem imgItem)


       

        #endregion


    }
}
