using System;


namespace PhotoUploader.Common
{
    public class ImageItem
    {
        //CanonTagInfo tagInfo = null;

        //public CanonTagInfo TagInfo
        //{
        //    get { return tagInfo; }
        //    set { tagInfo = value; }
        //}
      
        int index = -1;

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        bool transferedToPc = false;

        public bool TransferedToPc
        {
            get { return transferedToPc; }
            set { transferedToPc = value; }
        }

        string uniqueIdentifier = "";

        public string UniqueIdentifier
        {
            get { return uniqueIdentifier; }
            set { uniqueIdentifier = value; }
        }

        string filename;

        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        string cameraNumber;

        public string CameraNumber
        {
            get { return cameraNumber; }
            set { cameraNumber = value; }
        }


        private string _takenTime;
        public string TakenTime {
            get { return _takenTime; }
            set { _takenTime = value; }
        
        }

        


        public ImageItem(int index, string identifier, string filename,DateTime takentime)
        {
            //this.TagInfo = tagInfo;
            this.index = index;
            this.uniqueIdentifier = identifier;
            this.filename = filename;
            this._takenTime = String.Format("{0}_{1:D2}_{2:D2}_{3:D2}_{4:D2}_{5:D2}", takentime.Year, takentime.Month, takentime.Day, takentime.Hour, takentime.Minute, takentime.Second);

           
            //this.cameraNumber = tagInfo.SerialNumber;
        }

        public ImageItem(int index, string identifier, string filename, string time,string date)
        {
            //this.TagInfo = tagInfo;
            this.index = index;
            this.uniqueIdentifier = identifier;
            this.filename = filename;
            this._takenTime = String.Format("{0}_{1}", date,time);


            //this.cameraNumber = tagInfo.SerialNumber;
        }

        //public override string ToString()
        //{
        //    if (tagInfo != null)
        //    {
        //        DateTime dt = tagInfo.DateTimeOfImage;

        //        return Helper.GetFileName(tagInfo.SerialNumber, tagInfo.DateTimeOfImage, filename);
        //    }

        //    Helper.Log("ImageItem-ToString", "CanonTagInfo is null", Helper.LOG_ERROR);
        //    throw new Exception("CanonTagInfo is null in ImageItem.ToString()");
        //}
    }
}
