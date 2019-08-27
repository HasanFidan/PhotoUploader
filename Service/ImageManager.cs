using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using PhotoUploader.Common;
using System.Windows;


namespace PhotoUploader.Service
{
    public class ImageManager
    {
       private List<ImageItem> _imageList;
       private string ImageFolder;
       private XmlWriter xmlwriter;
        public ImageManager()
        {
            ImageFolder = (string)ConfigurationManager.AppSettings["ImageFolder"];


            if (!System.IO.Directory.Exists(ImageFolder))
            {
                //MessageBox.Show(ImageFolder + " path is not correct.Check config file.");
                DirectoryInfo info = System.IO.Directory.CreateDirectory(ImageFolder);
              
            }

            _imageList = new List<ImageItem>();
          
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            
            xmlwriter = XmlWriter.Create(ImageFolder + "ImagesMetadata.xml", settings);
            
            
        }
        public List<ImageItem> GetImagesFromDirectory()
        {

            _imageList = new List<ImageItem>();

            string[] ext = new string[] {".jpg"};
            var listOfFiles = System.IO.Directory.GetFiles(ImageFolder).Where(f => ext.Any(a => f.Contains(a))).ToArray();

            

            foreach (var filename in listOfFiles)
            {



                 string fname = Path.GetFileNameWithoutExtension(filename);
                 string[] fnameArray = fname.Split('_');
                 fname = fnameArray[8];

                  if(fname.Contains("_ftp"))
                      fname = fname.Remove(fname.IndexOf("_ftp"), 4);

                 ImageItem img = new ImageItem(0, fname, fname,DateTime.Now);
                _imageList.Add(img);
            
            }


            return _imageList;
  
        
        }


        public List<ImageItem> GetImagesWatingToUploadToFtp()
        {
            _imageList = new List<ImageItem>();

            string[] ext = new string[] { ".jpg" };
            var listOfFiles = System.IO.Directory.GetFiles(ImageFolder).Where(f => !f.Contains("_ftp") && ext.Any(a => f.Contains(a))).ToArray();

            foreach (var filename in listOfFiles)
            {
                string fname = Path.GetFileNameWithoutExtension(filename);
                string[] fnameArray = fname.Split('_');
                fname = fnameArray[8];
                //string fname = fnamewithftp.Remove(fnamewithftp.IndexOf("_ftp"), 4);
                ImageItem img = new ImageItem(0, fname, fname, DateTime.Now);
                _imageList.Add(img);

            }


            return _imageList;
        }


        public List<ImageItem> GetImagesUploadedToFtp()
        {
            _imageList = new List<ImageItem>();

            string[] ext = new string[] { ".jpg" };
            var listOfFiles = System.IO.Directory.GetFiles(ImageFolder).Where(f => f.Contains("_ftp") && ext.Any(a => f.Contains(a))).ToArray();

            foreach (var filename in listOfFiles)
            {
                string fnamewithftp = Path.GetFileNameWithoutExtension(filename);
                string fname = fnamewithftp.Remove(fnamewithftp.IndexOf("_ftp"), 4);
                string[] fnameArray = fname.Split('_');
                fname = fnameArray[8];
                string time = fnameArray[5] + "_" + fnameArray[6] + "_" + fnameArray[7];
                string date = fnameArray[2] +"_" +  fnameArray[3] + "_" +  fnameArray[4];
                ImageItem img = new ImageItem(0, fname, fname, time,date);
                _imageList.Add(img);

            }


            return _imageList; 
        }

        public void  WriteAllImageList(List<ImageItem> imagelist)
        {
            try
            {
                if (imagelist != null && imagelist.Count > 0)
                {
                    xmlwriter.WriteStartDocument();
                    xmlwriter.WriteStartElement("Images");

                    foreach (ImageItem item in imagelist)
                    {
                        WriteImageData(item.UniqueIdentifier, item.Filename, item.TakenTime);

                    }

                    xmlwriter.WriteEndElement();
                    xmlwriter.WriteEndDocument();
                    xmlwriter.Flush();
                }
            }
            catch (Exception ex)
            {
                Logger.Basic.Error("WriteAllImageList" + ex.ToString(),ex);
            }

        
        }// end of  public void  WriteAllImageList(List<ImageItem> imagelist)


        private void WriteImageData(string uniqueId, string imagefilename,string takentime)
        {
            xmlwriter.WriteStartElement("Image");
            xmlwriter.WriteAttributeString("UniqueID", uniqueId);
            xmlwriter.WriteAttributeString("TakenTime", takentime.ToString());
            xmlwriter.WriteString(imagefilename);
            xmlwriter.WriteEndElement();
        }


        public void AddImageToList() { }

        public void RemoveImageFromList() { }

        public List<ImageItem> ReadXmlDataFromFile()
        {
            string uniqueid = string.Empty;
            string fileName = string.Empty;
            DateTime takenTime = DateTime.Now;

            XmlTextReader reader = new XmlTextReader(ImageFolder + "ImagesMetadata.xml");

            while (!reader.IsEmptyElement && reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        fileName = reader.Value;
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        
                        break;
                    case XmlNodeType.Attribute:
                        if (reader.HasAttributes)
                        {
                           uniqueid = reader.GetAttribute("UniqueID");
                           takenTime = Convert.ToDateTime(reader.GetAttribute("TakenTime"));
                        }
                        break;
                }

                _imageList.Add(new ImageItem(0,uniqueid,fileName,takenTime));



            }

            reader.Close();

            return _imageList;


        }//end of  public List<ImageItem> ReadXmlDataFromFile()


        public void DeleteImageFromDirectory(ImageItem imgItem)
        {

            System.IO.File.Delete(ImageFolder + "IMG_1_" + imgItem.TakenTime + "_" + imgItem.Filename + "_ftp.jpg");
        }


        public void Close()
        {
            xmlwriter.Flush();
            xmlwriter.Close();
            xmlwriter.Dispose();
        }


    }
}
