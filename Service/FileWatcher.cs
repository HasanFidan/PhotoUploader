using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Threading;
using PhotoUploader.Common;

namespace PhotoUploader.Service
{
    public class FileWatcher
    {
        public delegate void delNotificationChanged(int state,string message);
        public delegate void delProgressBarChanged(int number,int allfiles);
        public delegate void delSendMessageCreated(int state,string message);
        public delegate void delRemainedFileChanged(int remaincount);

        public delProgressBarChanged ProgressBarChanged;
        public delNotificationChanged NotificationChanged;
        public delSendMessageCreated SendMessageCreated;

        public delRemainedFileChanged GetCountofRemainedFiles;

        string fileName = string.Empty;
         string imagedirectory = string.Empty;
         string samplestate = string.Empty;
         
         string imageExtensions = string.Empty;
         //Regex searchPattern = new Regex("\\d{4}-\\d{2}-\\d{2}_\\d{6}", RegexOptions.IgnoreCase);



        private void GetCountofRemained(int totalimg)
        {
            if(GetCountofRemainedFiles ==null)
                return;
            
            GetCountofRemainedFiles(totalimg);

        }

        private void SetProgressBar(int number,int allfiles)
        {

            if (ProgressBarChanged == null)
                return;

            ProgressBarChanged(number,allfiles);
           
        }


        private void MethodSendMessageCreated(int state,string message)
        {
            if (SendMessageCreated == null)
                return;

            SendMessageCreated(state,message);
        }

         private void SetNotification(int state, string message)
         {
             if (NotificationChanged == null)
                 return;

             NotificationChanged(state,message);

         }

        public FileWatcher()
        {
         
            imagedirectory = ConfigurationManager.AppSettings["ImageFolder"];
            imageExtensions = ConfigurationManager.AppSettings["ImageExtension"];
            
        }

        private static bool fetchingstate = false;
        private static bool disconnection = false;
        public void Reset()
        {
            disconnection = true;
            //isuploaded = false;
            totalfileCount = 0;
            fetchingstate = false;
            count = 0;
        }


        public void SetConnection()
        {
            disconnection = false;
        }
    
        private static int totalfileCount = 0;

        private static int count = 0;

        private static bool ntState = false;

   public void NetworkState(bool network)
   {
       if (ntState != network && network == true)
       {
           ntState = true;
           MethodSendMessageCreated(19,PhotoUploader.Properties.Resources.main_msg_ConnectionEstablished);
       }
       else if (ntState != network && network == false)
       {
           ntState = false;
           MethodSendMessageCreated(20,PhotoUploader.Properties.Resources.main_msg_connectionlost);
       }

   }

        public void FileNaming()
        {
            
            string[] ext = new string[] { imageExtensions };

            
            var listOfFiles = System.IO.Directory.GetFiles(imagedirectory).Where(f => !f.Contains("_ftp.") && ext.Any(a => f.Contains(a))).ToArray();


            totalfileCount = listOfFiles.Count();
            //if(!firstexecution)
            //{
            //    SetNotification(1,"File Uploading Operation has been started.");
            //    totalfileCount = listOfFiles.Count();
            //    //SetProgressBar(0,totalfileCount);
            //    firstexecution = true;
            //}

           
              
            
        

            
            UpdaloadAllFiles(listOfFiles);


            if (disconnection && !fetchingstate && isuploaded)
            {
                GetCountofRemained(-1);
            }
            else if (isuploaded == false && !fetchingstate)
                GetCountofRemained(-1);
            else if (isuploaded == true && listOfFiles.Count() == 0 && !fetchingstate)
            {
                GetCountofRemained(0);

            }
            else if (isuploaded == true && listOfFiles.Count() == 0 && fetchingstate)
            {
                GetCountofRemained(-1);
            }
        
        }// end of FileNaming()


        private static bool isuploaded = false;

        public void UpdaloadAllFiles(string[] listOfFiles)
        {
            string tmpfilename = string.Empty;
           

            foreach (var file in listOfFiles)
            {

                var fileNameNoEx = Path.GetFileName(file) ?? "";


                tmpfilename = fileNameNoEx;

                string Roottmpfilename = imagedirectory + "\\" + tmpfilename;

               
                CommonResultModel result = UploadFiles(Roottmpfilename, imagedirectory);

                if (result.IsOK == false)
                {
                    isuploaded = false;
                    break;
                }
                isuploaded = true;
                count++;

               
               GetCountofRemained(count);

         
                
            }

        }


        private bool _state = false;
        public void  GetFetchingState(bool state)
        {
            fetchingstate = state;
        }
  

        private CommonResultModel UploadFiles(string Roottmpfilename,string root)
        {
            // Upload File into FTP server
            var ftpClient = new FtpManager(Roottmpfilename,
                                             ConfigurationManager.AppSettings["ftpUrl"],
                                             ConfigurationManager.AppSettings["ftpUsername"],
                                             ConfigurationManager.AppSettings["ftpPassword"]);
            try
            {
                //var ftpResult = ftpClient.FileUpload(Path.GetFileName(Roottmpfilename));

                var ftpResult = ftpClient.FileUpload(Path.GetFileName(Roottmpfilename));

                if (!ftpResult.IsOK)
                {
                    Logger.Basic.Error(String.Format("FileWatcher.SendFile({0}): Ftp error - {1}", Roottmpfilename, ftpResult.Message));
                    return new CommonResultModel { IsOK = false, Message = "Ftp Error - " + ftpResult.Message, Result = null };
                }
            }
            catch (Exception ex)
            {
                Logger.Basic.Error("File Upload Error :" + ex.ToString(), ex);
                return new CommonResultModel { IsOK = false, Message = "Ftp Error Uploader throw exception ", Result = null };
            
            }


            

           
            string tmpfilename =  imagedirectory + Path.GetFileNameWithoutExtension(Roottmpfilename) + "_ftp.jpg";

            //SetNotification(Path.GetFileNameWithoutExtension(Roottmpfilename) + " named image has been uploaded ftp server.");
            //System.IO.File.Move(Roottmpfilename,tmpfilename);
            // Move the file to sent folder
            //if (System.IO.File.Exists(tmpfilename))
            //{
                var newFileName = "";
                var copied = false;
                try
                {
                    //var backupFolder = Path.Combine(root, "sent");
                    //System.IO.Directory.CreateDirectory(backupFolder);
                    //System.IO.File.Delete(e.FullPath);

                    //newFileName = Path.Combine(backupFolder, Path.GetFileName(tmpfilename) ?? "");

                    //System.IO.File.Move(fileName, newFileName);
                    System.IO.File.Copy(Roottmpfilename, tmpfilename);
                    copied = true;
                    System.IO.File.Delete(Roottmpfilename);
                }
                catch (System.IO.IOException ex)
                {
                    //if (!copied)
                    //    MessageBox.Show("Could not copy file to sent folder! Please move it manually.");
                    //else
                    //    MessageBox.Show("DX file is copied to sent folder but could not be deleted. Please delete it manually.");

                    Logger.Basic.Error(String.Format("FileWatcher.SendFile({0}): File could not be moved to sent folder.", tmpfilename));
                    return new CommonResultModel { IsOK = true, Message = "File could not be moved to sent folder.", Result = tmpfilename };
                }
            //}

            Logger.Basic.Error(String.Format("FileWatcher.SendFile({0}): Success", tmpfilename));
            return new CommonResultModel { IsOK = true, Message = "File uploaded.", Result = tmpfilename, extension = string.Empty };
        }  


    }
}
