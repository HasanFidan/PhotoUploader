using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace PhotoUploader.Service
{
    public class ExifReader
    {
        public void SetUpMetadataOnImage(string filename)
        {
            string tempName = Path.Combine(Path.GetDirectoryName(filename), Guid.NewGuid().ToString());
            // open image file to read
            using (Stream file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // create the decoder for the original file.  The BitmapCreateOptions and BitmapCacheOption denote
                // a lossless transocde.  We want to preserve the pixels and cache it on load.  Otherwise, we will lose
                // quality or even not have the file ready when we save, resulting in 0b of data written
                BitmapDecoder original = BitmapDecoder.Create(file, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                // create an encoder for the output file
                BitmapEncoder output = null;
                string ext = Path.GetExtension(filename);
                switch (ext)
                {
                    case ".png":
                        output = new PngBitmapEncoder();
                        break;
                    case ".jpg":
                        output = new JpegBitmapEncoder();
                        break;
                    case ".tif":
                        output = new TiffBitmapEncoder();
                        break;
                }

                if (original.Frames[0] != null && original.Frames[0].Metadata != null)
                {
                    // So, we clone the object since it's frozen.
                    BitmapFrame frameCopy = (BitmapFrame)original.Frames[0].Clone();
                    BitmapMetadata metadata = original.Frames[0].Metadata.Clone() as BitmapMetadata;

                    StripMeta(metadata);

                    // finally, we create a new frame that has all of this new metadata, along with the data that was in the original message
                    output.Frames.Add(BitmapFrame.Create(frameCopy, frameCopy.Thumbnail, metadata, frameCopy.ColorContexts));
                }
                // finally, save the new file over the old file                
                using (Stream outputFile = File.Open(tempName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    output.Save(outputFile);
                }
            }
            File.Delete(filename);
            File.Move(tempName, filename);
        }

        public void StripMeta(BitmapMetadata metaData)
        {
            for (int i = 270; i < 42016; i++)
            {
                if (i == 274 || i == 277 || i == 284 || i == 530 || i == 531 || i == 282 || i == 283 || i == 296) continue;

                string query = "/app1/ifd/exif:{uint=" + i + "}";
                BlankMetaInfo(query, metaData);

                query = "/app1/ifd/exif/subifd:{uint=" + i + "}";
                BlankMetaInfo(query, metaData);

                query = "/ifd/exif:{uint=" + i + "}";
                BlankMetaInfo(query, metaData);

                query = "/ifd/exif/subifd:{uint=" + i + "}";
                BlankMetaInfo(query, metaData);
            }

            for (int i = 0; i < 4; i++)
            {
                string query = "/app1/ifd/gps/{ulong=" + i + "}";
                BlankMetaInfo(query, metaData);
                query = "/ifd/gps/{ulong=" + i + "}";
                BlankMetaInfo(query, metaData);
            }
        }

        private void BlankMetaInfo(string query, BitmapMetadata metaData)
        {
            object obj = metaData.GetQuery(query);
            if (obj != null)
            {
                if (obj is string)
                    metaData.SetQuery(query, string.Empty);
                else
                {
                    ulong dummy;
                    if (ulong.TryParse(obj.ToString(), out dummy))
                    {
                        metaData.SetQuery(query, 0);
                    }

                }
            }
        }
    }
}

