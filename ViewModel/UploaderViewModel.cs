using PhotoUploader.Common;
using PhotoUploader.Service;
using PhotoUploader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WIA;

namespace PhotoUploader.ViewModel
{


    public class Language
    {
        public string Name { get; set; }
        public int LangID { get; set; }
        public string Abbrevation { get; set; }
    }
   
    public class UploaderViewModel : ObservableObject
    {
        private ICommand _checkScoutBoxCommand;
        private ICommand _questionCommand;
        public  PWDManager pwdManager;
        

      
       

        private string _scoutBoxState;
        private string _imageSource = "/Images/on.png";
        private string _selectedIcon = string.Empty;
        private string _stateColor = "Green";
        private string _imageCount = "";
        private int _fileTransferValue = 0;
        private string _messages;
        private int _maximumImageCount;

      
        public int FileTransferValue
        {
            get { return _fileTransferValue; }
            set { _fileTransferValue = value; OnPropertyChanged("FileTransferValue"); }
        }

        public int MaximumImageCount
        {
            get { return _maximumImageCount; }
            set { _maximumImageCount = value; OnPropertyChanged("MaximumImageCount"); }
        }


        public int WaitMaximumCount = 0;

        public string Messages {
            get { return _messages; }
            set { _messages = value; OnPropertyChanged("Messages"); }
        }

        public string ImageCount {

            get { return _imageCount; }
            set { _imageCount = value; OnPropertyChanged("ImageCount"); }
        
        }

        public string StateColor
        {
            get { return _stateColor; }
            set { _stateColor = value; OnPropertyChanged("StateColor"); }
        }

        public string SelectedIcon
        {
        get{ return _selectedIcon;}
            set { _selectedIcon = value; OnPropertyChanged("SelectedIcon");
            }
    }

        private string _versionNumber;
        public string VersionNumber {
            get { return _versionNumber; }
            set { _versionNumber = value; OnPropertyChanged("VersionNumber"); }
        }
        public string ScoutBoxState {

            get { return _scoutBoxState; }
            set { _scoutBoxState = value; 
                OnPropertyChanged("ScoutBoxState"); }
        }
        ImageManager imgManager = null;

        public UploaderViewModel() {
            string ImageFolder = (string)ConfigurationManager.AppSettings["ImageFolder"];
            imgManager = new ImageManager();


         
           Version ver = Assembly.GetEntryAssembly().GetName().Version;
           VersionNumber = PhotoUploader.Properties.Resources.main_footer_version + ver.ToString();

          
            SelectedIcon = "/Images/Logo.ico";
            FileWatcher filewatcher = (FileWatcher)App.Current.Properties["FileWatcher"];
            //filewatcher.NotificationChanged = new FileWatcher.delNotificationChanged(SetImageCountMessage);
            //filewatcher.ProgressBarChanged = new FileWatcher.delProgressBarChanged(SetProgressBar);
            filewatcher.SendMessageCreated = new FileWatcher.delSendMessageCreated(GetNotificationMessage);
            filewatcher.GetCountofRemainedFiles = new FileWatcher.delRemainedFileChanged(GetRemainedFiles);


          
            try
            {
                using(pwdManager = new PWDManager())
                {
                    pwdManager.CameraConnectionStatusChanged = new PWDManager.DigitalDeviceAttached(RefreshConnectionStatus);
                    pwdManager.DeviceErrorMessageThrow = new PWDManager.DeviceErrorMessageThrowed(GetNotificationMessage);
                    pwdManager.PlugAndPlayChanged = new PWDManager.delPlugAndPlayMessage(SetNotificationMessage);

                    pwdManager.CheckPnPPoint();
                }

            }
            catch (Exception ex)
            {
                Logger.Basic.Debug("Error : " + ex.ToString(), ex);
            }
           
           
        }


        private void SetNotificationMessage(string message)
        {
            Messages += message + Environment.NewLine;

        }

        private static bool iswritten = false;
        private static bool InternetIsExist = true;
        private void GetNotificationMessage(int state,string message)
        {

            if (!iswritten && state == 1)
            {

                Messages += message + Environment.NewLine;
                iswritten = true;
            }
            else if(state == 19)
            {
                InternetIsExist = true;
                Messages += message + Environment.NewLine;
            }
            else if (state == 20)
            {
                InternetIsExist = false;
                Messages += message + Environment.NewLine;
            }
        }

        private string _deletedImageCount;
        public string DeletedImageCount
        {
            get { return _deletedImageCount; }
            set { _deletedImageCount = value; OnPropertyChanged("DeletedImageCount"); }
        }

        bool iswrittenAllImagesDone = false;
       
        private void IncreaseImageNumberCount(int number,int totalimgcount)
        {
            //double oran = (double)number / MaximumImageCount;
            //FileTransferValue = (int)(oran * 100);

            if (number > 0 )
            {
               
                IsUploading = Visibility.Visible;
                DeletedImageCount = String.Format("{0} ",number);
            }
            else if (!fetchingdone && Isfetching == Visibility.Collapsed)
            {
                IsUploading = Visibility.Collapsed;
                UploadImagePath = "/Images/waiting.png";
            }
            else if (number == 0)
            {
                IsUploading = Visibility.Collapsed;
                UploadImagePath = "/Images/checkmark.png";
                if (!iswrittenAllImagesDone)
                {
                    Messages += PhotoUploader.Properties.Resources.main_msg_AllUploadSuccess + Environment.NewLine;
                    iswrittenAllImagesDone = true;
                }


            }
            else
            {
                IsUploading = Visibility.Collapsed;
                UploadImagePath = "/Images/waiting.png";
            }
          
        }

        private void GetRemainedFiles(int remainedfiles)
        {
         
            
            if (remainedfiles > 0)
            {
                IsFtpImageExist = Visibility.Visible;
                FtpImageCount = String.Format("{0} ", remainedfiles.ToString());
            }
            else if (remainedfiles == -1)
            {
                IsFtpImageExist = Visibility.Collapsed;
                FtpImagePath = "/Images/waiting.png";
            }
            else
            {
                IsFtpImageExist = Visibility.Collapsed;
                FtpImagePath = "/Images/checkmark.png";
            }
        }

        private bool fetchingdone = false;

        private  int justintimeIMGcount = 0;
        private  int totalIMGcOUNT = 0;

        private void FetchingOperation(int imgnumber,int total)
        {
           

            if (imgnumber < total)
            {
                fetchingdone = false;
                Isfetching = Visibility.Visible;
                FetchedImageCount = String.Format("{0} / {1}", imgnumber, total);
            }
            else if (imgnumber == 0 && total == 0)
            {
                fetchingdone = false;
                Isfetching = Visibility.Collapsed;
                FetchImagePath = "/Images/waiting.png";
            }
            else if (total == imgnumber)
            {
                fetchingdone = true;
                Isfetching = Visibility.Visible;
                FetchedImageCount = String.Format("{0} / {1}", imgnumber, total);
                Thread.Sleep(2000);
                Isfetching = Visibility.Collapsed;
                FetchImagePath = "/Images/checkmark.png";
            }
            else
            {
                Isfetching = Visibility.Collapsed;
                FetchImagePath = "/Images/checkmark.png";
            }
        }


        private string _fetchimagePath;

        public string FetchImagePath
        {
            get { return _fetchimagePath; }
            set
            {
                _fetchimagePath = value;
                OnPropertyChanged("FetchImagePath");
            }
        }

        private string _ftpimagePath;

        public string FtpImagePath
        {
            get { return _ftpimagePath; }
            set
            {
                _ftpimagePath = value;
                OnPropertyChanged("FtpImagePath");
            }
        }


        

           private string _uploadimagePath;

          public string UploadImagePath
        {
            get { return _uploadimagePath; }
            set
            {
                _uploadimagePath = value;
                OnPropertyChanged("UploadImagePath");
            }
        }


        private Visibility _isfetching =Visibility.Collapsed;
        public Visibility Isfetching {
            get { return _isfetching; }
            set { 
                _isfetching = value;
               
                OnPropertyChanged("Isfetching"); }
        }

        private Visibility _isuploading = Visibility.Collapsed;
        public Visibility IsUploading {
            get { return _isuploading; }
            set { _isuploading = value; OnPropertyChanged("IsUploading"); }
        }

        private Visibility _isFtpImageExist;
        public Visibility IsFtpImageExist
        {
            get { return _isFtpImageExist; }
            set { _isFtpImageExist = value; OnPropertyChanged("IsFtpImageExist"); }
        
        }
        private void RefreshConnectionStatus(bool status,string CameraName)
        {
            ScoutBoxState = CameraName;

            if (status)
            {
                ImageSource = "/Images/on.png";
                StateColor = "#69AC40";


               
                pwdManager.ReturnDeviceName = new PWDManager.DeviceName(RefreshCameraName);
                pwdManager.GetImageCountChanged = new PWDManager.delGetImageCountChanged(RefreshImageCount);
                 pwdManager.ImageNumberChanged = new PWDManager.delImageNumberChanged(IncreaseImageNumberCount);
                 pwdManager.NotificationChanged = new PWDManager.delNotificationChanged(GetNotificationMessage);
                pwdManager.FetchedImageCountChanged = new PWDManager.delFetchedImageCount(FetchingOperation);
           
                iswritten = false;
                Isfetching = Visibility.Visible;
                IsFtpImageExist = Visibility.Collapsed;
                IsUploading = Visibility.Collapsed;
                FetchImagePath = "/images/waiting.png";
                FtpImagePath = "/images/waiting.png";
                UploadImagePath = "/images/waiting.png";

                pwdManager.GetCountOfImage(imgManager);


                Action action = () => FileTransferValue = 0; 
                Dispatcher.CurrentDispatcher.Invoke(action);

                checkFileDir.StopCheck();
                checkFileDir.SetConnection();
      

                FetchDataSet(true);
                TimerSet(true);
                
             
              
            }
            else
            {
                iswrittenAllImagesDone = false;

                ImageSource = "/Images/off.png";
                //ScoutBoxState = "Disconnected";
                StateColor = "#D0021B";
                iswritten = false;

                TimerSet(false);
                FetchDataSet(false);
                checkFileDir.Reset();
                Messages = String.Empty;
                IsUploading = Visibility.Collapsed;
                Isfetching = Visibility.Collapsed;
                MaximumImageCount = 0;
                FetchedImageCount = string.Empty;
                DeletedImageCount = string.Empty;
                IsFtpImageExist = Visibility.Collapsed;
                FetchImagePath = "/Images/waiting.png";
                FtpImagePath = "/images/waiting.png";
                UploadImagePath = "/images/waiting.png";

                Logger.Basic.Debug("UnPlugged false");
             
            }
        }

        private bool ismessageshown = false;

        private void IsFetchDone(bool isdone)
        {
            FetchDataSet(false);
            //TimerSet(true);
            
        }

        private string _fetchedImageCount;
        public string FetchedImageCount
        {
            get { return _fetchedImageCount; }
            set { _fetchedImageCount = value; OnPropertyChanged("FetchedImageCount"); }
        }

        private string _ftpImageCount=string.Empty;
        public string FtpImageCount
        { get { return _ftpImageCount;} 
        set {_ftpImageCount=value;OnPropertyChanged("FtpImageCount");}
        }

       

        private void RefreshCameraName(string cameraname)
        {
                ScoutBoxState = cameraname;
        }

        private void RefreshImageCount(int count)
        {

            Logger.Basic.Error("Image count is :" + count.ToString());

            if (count > 0)
            {
                ImageCount = "Image Count is " + count.ToString() + " in digital camera.";
                MaximumImageCount = count;
            }
            if (count == 0)
            {
                Messages += PhotoUploader.Properties.Resources.main_msg_noImage + Environment.NewLine;
                Isfetching = Visibility.Collapsed;
                FetchImagePath = "/images/waiting.png";
                MaximumImageCount = 0;
                //Messages += "There is no images in camera storage" + Environment.NewLine;
            }
        }

        private static bool messageiswritten = false;
        private void NotifyMessage(bool isok,int countofimg)
        {

            if (!isok && !messageiswritten && countofimg > 0)
            {
                Logger.Basic.Debug("Some images not deleted from camera :" + countofimg.ToString());
                messageiswritten = true;
                Messages += "Some images are retrieved from another ScoutBox.Unless correct ScoutBox is not plugged,these images will not be deleted from folder." + Environment.NewLine;
                //Messages += "You have successfully uploaded all images.The Results, most recently, will be displayed on the ScoutBox website." + Environment.NewLine;
            }

            //if (countofimg == 0 && !messageiswritten)
            //{
            //    messageiswritten = true;
            //    Messages += "You have successfully uploaded all images.The Results, most recently, will be displayed on the ScoutBox website." + Environment.NewLine;
            //}

        }

        private void RefreshImageCountFTP(int count)
        {

            //if (count > 0)
            //{
            //    //Messages += count.ToString() + " images will be uploaded." + Environment.NewLine;
            //}
            if (count == 0)
            {
                if (ismessageshown)
                {
                    Messages += "You have successfully uploaded all images from Scoutbox. There are no images left to transfer.You may turn off Scoutbox and unplug from the computer." + Environment.NewLine;
                    Messages += "Please close Scoutbox Uploader Program.  Soon enough, the plate insect counts will be visible on Scoutbox Web." + Environment.NewLine;
                    Messages += "Website URL is login.scoutbox.com.Thank you for using Scoutbox Uploader." + Environment.NewLine;
                    ImageCount = "All images have been uploaded successfully.";
                    FileTransferValue = 100;
                    checkFileDir.Reset();
                    ismessageshown = false;
                }
            }
            else
            {
                ismessageshown = true;
                //Messages += "There are some images retrieved from another ScoutBox.Unless correct ScoutBox is plugged in,these images will not be deleted.Please check folder." + Environment.NewLine;
                FileTransferValue = 100;
                //ImageCount = "Some of the images have not been uploaded successfully.";
                checkFileDir.Reset();
            }
        }


        public string ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; OnPropertyChanged("ImageSource"); }
        }

        


        #region Commands
   

        public ICommand QuestionCommand
        {

            get
            {

                if (_questionCommand == null)
                    _questionCommand = new RelayCommand(param => GetHelpWindow());

                return _questionCommand;
            }
        }


        private void GetHelpWindow()
        {
            var helpWindow = new UploaderHelpView();

            helpWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            helpWindow.Closed += (o, args) =>
            {

            };
            helpWindow.ShowDialog();
        }



        #endregion

        #region TimerEvents
        private static CheckFileDirectory checkFileDir = (CheckFileDirectory)App.Current.Properties["CheckFileDirectory"];


        private System.Timers.Timer bTimer = null;

        private void FetchDataSet(bool state)
        {
            if (bTimer == null)
            {
                bTimer = new System.Timers.Timer();
                bTimer.Elapsed += new ElapsedEventHandler(FetchCameraEvent);
                bTimer.Interval = 1000 * 3;
                bTimer.Enabled = state;
            }
            else
            {
                bTimer.Enabled = state;

            }
        }


        private System.Timers.Timer aTimer = null;
        private void TimerSet(bool state)
        {
            if (aTimer == null)
            {
                aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(DeleteImagesEvent);
                aTimer.Interval = 1000 * 7;
                aTimer.Enabled = state;
            }
            else
            {
                aTimer.Enabled = state;

            }
        }

        private void DeleteImagesEvent(object source, ElapsedEventArgs e)
        {
            List<ImageItem> willbeuploaded = imgManager.GetImagesWatingToUploadToFtp();
            List<ImageItem> deletedFiles = imgManager.GetImagesUploadedToFtp();

            if (deletedFiles.Count > 0)
            {
                pwdManager.DeleteImagesFromCamera(imgManager, deletedFiles, willbeuploaded);
            }

            if(deletedFiles.Count == 0 && !InternetIsExist)
            {
                Action action1 = () => IncreaseImageNumberCount(-1, 0);
                Dispatcher.CurrentDispatcher.Invoke(action1);
                Thread.Sleep(100); 
            }

            if (deletedFiles.Count == 0 && willbeuploaded.Count == 0)
            {
                Action action1 = () => IncreaseImageNumberCount(0, deletedFiles.Count);
                Dispatcher.CurrentDispatcher.Invoke(action1);
                Thread.Sleep(100);
            }
            else if (deletedFiles.Count > 0 && willbeuploaded.Count == 0)
            {

                Action action1 = () => IncreaseImageNumberCount(-1, deletedFiles.Count);
                Dispatcher.CurrentDispatcher.Invoke(action1);
                Thread.Sleep(100);

            }
        }


        private void FetchCameraEvent(object source, ElapsedEventArgs e)
        {
          
             pwdManager.GetAllImagesFromCamera();


            //if (MaximumImageCount == 0)
            //{
            //    //WaitMaximumCount = 2000;
            //    //waitSecond = 2;
            //    //Messages += "There are no images in Scoutbox." + Environment.NewLine;
            //    Isfetching = Visibility.Collapsed;
            //    FetchImagePath = "/Images/checkmark.png";
            //}

            checkFileDir.NotifyFetchingState(PWDManager.isfetchingrunning);
            //wia.ScoutBoxSate = true;

            if (!checkFileDir.IsTimerEnable())
            {

                checkFileDir.StartCheck(1000 * 5);
               
            }

           
          

           

        }
        #endregion



    }
}
