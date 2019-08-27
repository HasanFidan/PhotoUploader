using System;
using System.Collections.Generic;
using PortableDeviceTypesLib;
using PortableDeviceApiLib;

using _tagpropertykey = PortableDeviceApiLib._tagpropertykey;
using IPortableDeviceKeyCollection = PortableDeviceApiLib.IPortableDeviceKeyCollection;
using IPortableDeviceValues = PortableDeviceApiLib.IPortableDeviceValues;
using System.IO;
using System.Runtime.InteropServices;
using PhotoUploader.Common;

namespace PhotoUploader.Service
{
    public class PortableDevice
    {
        #region Fields

        private bool _isConnected;
        private readonly PortableDeviceClass _device;

        #endregion

        #region ctor(s)

        public PortableDevice(string deviceId)
        {
            this._device = new PortableDeviceClass();
            this.DeviceId = deviceId;
        }

        #endregion

        #region Properties

        public string DeviceId { get; set; }

        public string FriendlyName
        {
            get
            {
                if (!this._isConnected)
                {
                    throw new InvalidOperationException("Not connected to device.");
                }

                // Retrieve the properties of the device
                IPortableDeviceContent content;
                IPortableDeviceProperties properties;
                this._device.Content(out content);
                content.Properties(out properties);

                // Retrieve the values for the properties
                IPortableDeviceValues propertyValues;
                properties.GetValues("DEVICE", null, out propertyValues);

                // Identify the property to retrieve
                var property = new _tagpropertykey();
                property.fmtid = new Guid(0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B,
                                          0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC);
                property.pid = 12;

                // Retrieve the friendly name
                string propertyValue;
                propertyValues.GetStringValue(ref property, out propertyValue);

                return propertyValue;
            }
        }

        #endregion

        #region Methods

        public void Connect()
        {
            if (this._isConnected) { return; }

            var clientInfo = (IPortableDeviceValues) new PortableDeviceValuesClass();
            this._device.Open(this.DeviceId, clientInfo);
            this._isConnected = true;
        }

        public void Disconnect()
        {
            if (!this._isConnected) { return; }
            this._device.Close();
            this._isConnected = false;
        }

        public IPortableDeviceContent GetContentofDevice()
        {
            IPortableDeviceContent content;
            this._device.Content(out content);

            return content;
        }

        public PortableDeviceFolder GetContents()
        {
            var root = new PortableDeviceFolder("DEVICE", "DEVICE");

            IPortableDeviceContent content;
            this._device.Content(out content);

            string serialnumber =  GetDeviceSerialNumber();

            EnumerateContents(ref content, root);

            return root;
        }


        public string GetDeviceSerialNumber()
        {
            var WPD_DEVICE_SERIAL_NUMBER = new _tagpropertykey();
            WPD_DEVICE_SERIAL_NUMBER.fmtid = new Guid(0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC);
            WPD_DEVICE_SERIAL_NUMBER.pid = 9;

            IPortableDeviceContent content;
            this._device.Content(out content);
            var deviceValues = GetDeviceValues(content, WPD_DEVICE_SERIAL_NUMBER, "DEVICE");

            PortableDeviceApiLib.tag_inner_PROPVARIANT value;
            deviceValues.GetValue(ref WPD_DEVICE_SERIAL_NUMBER, out value);

            return PropVariant.FromValue(value).AsString();


        }


        public bool DownloadFile(PortableDeviceFile file, string saveToPath)
        {
            bool isdownload = false;
            IPortableDeviceContent content;
            this._device.Content(out content);

            IPortableDeviceResources resources;
            content.Transfer(out resources);

            PortableDeviceApiLib.IStream wpdStream;
            uint optimalTransferSize = 0;

            System.Runtime.InteropServices.ComTypes.IStream sourceStream = null;

            IPortableDeviceValues values =
                new PortableDeviceTypesLib.PortableDeviceValues() as IPortableDeviceValues;
            
            DateTime createdDate = GetObjectCreationTime(content, file.Id);
            ImageItem imageItem = null;
            imageItem = new ImageItem(0, file.Id, file.Name, createdDate);


            var property = new _tagpropertykey();
            property.fmtid = new Guid(0xE81E79BE, 0x34F0, 0x41BF, 0xB5, 0x3F, 0xF1, 0xA0, 0x6A, 0xE8, 0x78, 0x42);
            property.pid = 0;
            string imgpath = string.Empty;
             FileStream targetStream = null;
             try
            {
            resources.GetStream(file.Id, ref property, 0, ref optimalTransferSize, out wpdStream);

            sourceStream = (System.Runtime.InteropServices.ComTypes.IStream)wpdStream;

            var filename = Path.GetFileName(file.Id);
             imgpath = saveToPath + "IMG_1_" + imageItem.TakenTime.ToString() + "_" + file.Id + ".jpg";
             targetStream = new FileStream(imgpath, FileMode.Create, FileAccess.Write);

            unsafe
            {
                var buffer = new byte[2048];
                int bytesRead;
                do
                {
                    sourceStream.Read(buffer, 2048, new IntPtr(&bytesRead));
                    targetStream.Write(buffer, 0, 2048);
                } while (bytesRead > 0);
                targetStream.Close();
                

                isdownload = true;
            }
            }
             catch (Exception ex)
             {

                 isdownload = false;
                 Logger.Basic.Error("PortableDevice DownloadFile : " + ex.ToString(), ex);
             }
             finally
             {
                 if (resources != null && Marshal.IsComObject(resources))
                 {
                     Marshal.ReleaseComObject(resources);
                 }

                 if (content != null && Marshal.IsComObject(content))
                 {
                     Marshal.ReleaseComObject(content);
                 }

                 if (sourceStream != null && Marshal.IsComObject(sourceStream))
                 {
                     Marshal.ReleaseComObject(sourceStream);
                 }

                 if (!isdownload)
                 {
                     
                     if (File.Exists(imgpath))
                     {
                         if (targetStream != null)
                             targetStream.Close();
                         
                         File.Delete(imgpath);
                     }
                 }


             }

             return isdownload;
        }


        /// <summary>
        /// gets the create date time for the object
        /// </summary>
        /// <param name="deviceContent">unmanged device</param>
        /// <param name="objectId">object id</param>
        /// <returns>Try the creation date, fall back to modified date</returns>
        public DateTime GetObjectCreationTime(IPortableDeviceContent deviceContent, string objectId)
        {
            // Try the creation date, fall back to modified date
            var WPD_OBJECT_DATE_CREATED = new _tagpropertykey();
            WPD_OBJECT_DATE_CREATED.fmtid =
                new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA,
                         0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_DATE_CREATED.pid = 18;

            try
            {
                

                return GetDateProperty(
                    deviceContent,
                    objectId,
                    WPD_OBJECT_DATE_CREATED);
            }
            catch (Exception)
            {
                //return GetDateProperty(
                //    deviceContent,
                //    objectId,
                //    WPD_OBJECT_DATE_MODIFIED);
            }

            return DateTime.Now ;
        }

        /// <summary>
        /// lookup the given key and return the date property associated with the object
        /// </summary>
        /// <param name="deviceContent">unmanged device</param>
        /// <param name="objectId">object id</param>
        /// <param name="key">name of the property key</param>
        /// <returns>date value</returns>
        public DateTime GetDateProperty(IPortableDeviceContent deviceContent, string objectId, _tagpropertykey key)
        {
            var deviceValues = GetDeviceValues(deviceContent, key, objectId);

            PortableDeviceApiLib.tag_inner_PROPVARIANT value;
            deviceValues.GetValue(ref key, out value);

            return PropVariant.FromValue(value).ToDate();
        }

        //public static PropVariant FromValue(PortableDeviceApiLib.tag_inner_PROPVARIANT value)
        //{
        //    IntPtr ptrValue = Marshal.AllocHGlobal(Marshal.SizeOf(value));
        //    Marshal.StructureToPtr(value, ptrValue, false);

        //    //
        //    // Marshal the pointer into our C# object
        //    //
        //    return (PropVariant)Marshal.PtrToStructure(ptrValue, typeof(PropVariant));
        //}

        private static IPortableDeviceValues GetDeviceValues(IPortableDeviceContent deviceContent, _tagpropertykey key, string objectId)
        {
            IPortableDeviceProperties deviceProperties;
            deviceContent.Properties(out deviceProperties);

            var keyCollection = (IPortableDeviceKeyCollection)new PortableDeviceTypesLib.PortableDeviceKeyCollectionClass();
            keyCollection.Add(key);

            IPortableDeviceValues deviceValues;
            
            deviceProperties.GetValues(objectId, keyCollection, out deviceValues);
            return deviceValues;
        }


        private static void StringToPropVariant(
            string value,
            out PortableDeviceApiLib.tag_inner_PROPVARIANT propvarValue,uint pid)
        {
            PortableDeviceApiLib.IPortableDeviceValues pValues =
                (PortableDeviceApiLib.IPortableDeviceValues)
                    new PortableDeviceTypesLib.PortableDeviceValuesClass();

            var WPD_OBJECT_ID = new _tagpropertykey();
            WPD_OBJECT_ID.fmtid =
                new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA,
                         0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_ID.pid = pid;

       

            pValues.SetStringValue(ref WPD_OBJECT_ID, value);

            pValues.GetValue(ref WPD_OBJECT_ID, out propvarValue);
        }

        public void DeleteFile(PortableDeviceFile file)
        {

            var variant = new PortableDeviceApiLib.tag_inner_PROPVARIANT();
            IPortableDeviceContent content = null;
            try
            {
                
                this._device.Content(out content);

                StringToPropVariant(file.Id, out variant, 2);

                PortableDeviceApiLib.IPortableDevicePropVariantCollection objectIds =
                    new PortableDeviceTypesLib.PortableDevicePropVariantCollection()
                    as PortableDeviceApiLib.IPortableDevicePropVariantCollection;
                objectIds.Add(variant);


                content.Delete(0, objectIds, null);

            }
            catch (Exception ex)
            {
                Logger.Basic.Error("Portable Device DeleteFile : " + ex.ToString(),ex);
            }
            finally {
                if (Marshal.IsComObject(variant))
                {
                    Marshal.ReleaseComObject(variant);
                }

                if (content != null && Marshal.IsComObject(content))
                {
                    Marshal.ReleaseComObject(content);
                }
            }
          

           
        }

        private static void EnumerateContents(ref IPortableDeviceContent content, 
            PortableDeviceFolder parent)
        {
            // Get the properties of the object
            IPortableDeviceProperties properties;
            content.Properties(out properties);

            // Enumerate the items contained by the current object
            IEnumPortableDeviceObjectIDs objectIds;
            content.EnumObjects(0, parent.Id, null, out objectIds);

            uint fetched = 0;
            do
            {
                string objectId;

                objectIds.Next(1, out objectId, ref fetched);
                if (fetched > 0)
                {
                    var currentObject = WrapObject(properties, objectId);

                    parent.Files.Add(currentObject);

                    if (currentObject is PortableDeviceFolder)
                    {
                        EnumerateContents(ref content, (PortableDeviceFolder) currentObject);
                    }
                }
            } while (fetched > 0);
        }

        private static PortableDeviceObject WrapObject(IPortableDeviceProperties properties, 
            string objectId)
        {
            IPortableDeviceKeyCollection keys;
            properties.GetSupportedProperties(objectId, out keys);

            IPortableDeviceValues values;
            properties.GetValues(objectId, keys, out values);

            // Get the name of the object
            string name;
            var property = new _tagpropertykey();
            property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC,
                                      0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            property.pid = 4;
            values.GetStringValue(property, out name);

            // Get the type of the object
            Guid contentType;
            property = new _tagpropertykey();
            property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC,
                                      0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            property.pid = 7;
            values.GetGuidValue(property, out contentType);

            var folderType = new Guid(0x27E2E392, 0xA111, 0x48E0, 0xAB, 0x0C,
                                      0xE1, 0x77, 0x05, 0xA0, 0x5F, 0x85);
            var functionalType = new Guid(0x99ED0160, 0x17FF, 0x4C44, 0x9D, 0x98,
                                          0x1D, 0x7A, 0x6F, 0x94, 0x19, 0x21);

            if (contentType == folderType  || contentType == functionalType)
            {
                return new PortableDeviceFolder(objectId, name);
            }

            return new PortableDeviceFile(objectId, name);
        }

        #endregion
    }
}