
using PhotoUploader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;



namespace PhotoUploader.Service
{
    /// <summary>
    /// FTP client class that encapsulates FTP Upload and Download operations.
    /// </summary>
    public class FtpManager
    {
        private string _url, _username, _password, _filePath;

        public FtpManager(string filePath, string url, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;
            _filePath = filePath;
        }


        public CommonResultModel NewFileUpload(string fname)
        {
            ManualResetEvent waitObject;
             FtpState state = new FtpState();
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_url + @"/" + fname);
            request.UseBinary = true;
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(_username, _password);
            string status = string.Empty;
            //byte[] b = File.ReadAllBytes(_filePath);
          
            state.Request = request;
            state.FileName = fname;
            
            waitObject = state.OperationComplete;

            request.BeginGetRequestStream(
               new AsyncCallback(EndGetStreamCallback),state
               
           );

            waitObject.WaitOne(new TimeSpan(0, 40, 0));

            if (state.OperationException != null)
            {
                throw state.OperationException;
            }
            else
            {
                //Console.WriteLine("The operation completed - {0}", state.StatusDescription);
            }

            

            return new CommonResultModel
            {
                IsOK = true,
                Message = string.Format("Upload File Complete, status {0}", status),
            };

        }


        private  void EndGetStreamCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState)ar.AsyncState;

            Stream requestStream = null;
            try
            {
                requestStream = state.Request.EndGetRequestStream(ar);
                const int bufferLength = 2048;
                byte[] buffer = new byte[bufferLength];
                int count = 0;
                int readBytes = 0;
                
                FileStream stream = File.OpenRead(this._filePath);
                do
                {
                    readBytes = stream.Read(buffer, 0, bufferLength);
                    requestStream.Write(buffer, 0, readBytes);
                    count += readBytes;
                }
                while (readBytes != 0);
                //Console.WriteLine("Writing {0} bytes to the stream.", count);
                requestStream.Close();
                stream.Close();
                state.Request.BeginGetResponse(
                    new AsyncCallback(EndGetResponseCallback),
                    state
                );
            }
            catch (Exception e)
            {
                //Console.WriteLine("Could not get the request stream.");
                state.OperationException = e;
                state.OperationComplete.Set();
                return;
            }

        }

        private  void EndGetResponseCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState)ar.AsyncState;
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)state.Request.EndGetResponse(ar);
                response.Close();
                state.StatusDescription = response.StatusDescription;
                state.OperationComplete.Set();
            }
            catch (Exception e)
            {
                state.OperationException = e;
                state.OperationComplete.Set();
            }
        }

        public CommonResultModel FileUpload(string fileName)
        {
            try


            {
                CommonResultModel resultofUpload = new CommonResultModel
                {
                    IsOK = true,
                    Message = string.Format("Upload File Complete, status {0}", 1),
                };

                 int bufferSize = 2048;
               
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_url + @"/" + fileName);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = true;

                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(_username, _password);
                string status = string.Empty;
                byte[] arr;

               //Stream ftpStream = request.GetRequestStream();
               
               Image img = Image.FromFile(_filePath);
               using (MemoryStream ms = new MemoryStream())
               {
                   img.Save(ms, ImageFormat.Jpeg);
                   arr = ms.ToArray();
               }


               //byte[] b = File.ReadAllBytes(_filePath);

               request.ContentLength = arr.Length;

               using (Stream s = request.GetRequestStream())
               {
                   s.Write(arr, 0, arr.Length);
               }

               FtpWebResponse response = (FtpWebResponse)request.GetResponse();
               status = response.StatusDescription;
               response.Close();
               img.Dispose();

               // /* Open a File Stream to Read the File for Upload */
               //FileStream localFileStream = new FileStream(_filePath,FileMode.OpenOrCreate);
               // /* Buffer for the Downloaded Data */
               // byte[] byteBuffer = new byte[bufferSize];
               // int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
               // /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
               // try
               // {
               //     while (bytesSent != 0)
               //     {
               //         ftpStream.Write(byteBuffer, 0, bytesSent);
               //         bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
               //     }
               // }
               // catch (Exception ex) {
               //     resultofUpload = new CommonResultModel
               //     { IsOK = false,
               //     Message = ex.Message
               //     };
               //     Logger.Basic.Error("Ftp Upload Error : ", ex);
                
               // }
               // /* Resource Cleanup */
               // localFileStream.Close();
               // ftpStream.Close();
               // request = null;


               

                return resultofUpload;

            }
            catch (Exception ex)
            {
                Logger.Basic.Error("FileWatcher : UploadFile = " + ex.ToString());

                return new CommonResultModel
                {
                    IsOK = false,
                    Message = ex.Message,
                    Result = ex
                };
            }
        }

        /// <summary>
        /// Download a file from FTP server
        /// </summary>
        /// <param name="fileName">Name of the file on remote server</param>
        /// <returns></returns>
        public CommonResultModel DownloadFile(string fileName)
        {
            try
            {
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_url + @"/" + fileName);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(_username, _password);

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                Console.WriteLine(reader.ReadToEnd());

                CommonResultModel result = new CommonResultModel
                 {
                     IsOK = true,
                     Message = string.Format("Download Complete, status {0}", response.StatusDescription)
                 };

                reader.Close();
                response.Close();

                return result;
            }
            catch (Exception ex)
            {
                Logger.Basic.Error("FtpManager : DownloadFile = " + ex.Message);
                return new CommonResultModel
                 {
                     IsOK = true,
                     Message = string.Format("Download Complete, status {0}", ex.Message),
                     Result = ex
                 };

            }
        }
    
    }


    public class FtpState
    {
        private ManualResetEvent wait;
        private FtpWebRequest request;
        private string fileName;
        private Exception operationException = null;
        string status;

        public FtpState()
        {
            wait = new ManualResetEvent(false);
        }

        public ManualResetEvent OperationComplete
        {
            get { return wait; }
        }

        public FtpWebRequest Request
        {
            get { return request; }
            set { request = value; }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        public Exception OperationException
        {
            get { return operationException; }
            set { operationException = value; }
        }
        public string StatusDescription
        {
            get { return status; }
            set { status = value; }
        }
    }

}
