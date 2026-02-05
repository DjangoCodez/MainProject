using SoftOne.Soe.Business.Core.CrGen;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SoftOne.Soe.Business.Util
{
    public static class FileUtil
    {
        public static byte[] ConvertToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        public static List<byte[]> ConvertToStream(byte[] file, bool useCompressed)
        {
            List<byte[]> contents = new List<byte[]>();

            try
            {
                MemoryStream fileStream = new MemoryStream(file);
                Byte[] array = null;

                try
                {
                    //Perferred way to create array
                    BinaryReader br = new BinaryReader(fileStream);
                    long numBytes = fileStream.Length;
                    array = br.ReadBytes((int)numBytes);
                }
                catch
                {
                    //Try this i other fail
                    MemoryStream stream = new MemoryStream();
                    stream.Position = 0;
                    fileStream.CopyTo(stream, 1024);
                    array = stream.ToArray();
                }

                if (useCompressed)
                    array = compressByteArray(array);

                contents.Add(array);
            }
            catch (IOException ex)
            {
                throw new SoeGeneralException("Could not read file", ex.InnerException, "FileUtil");
            }

            return contents;
        }

        //sets the encoding a byte[] based on a string with the encoding name and returns a new byte[] with the new encoding  
        public static byte[] ConvertTextFileEncoding(byte[] bytes, string fromEncoding, string toEncoding)
        {
            try
            {
                Encoding from = Encoding.GetEncoding(fromEncoding);
                Encoding to = Encoding.GetEncoding(toEncoding);
                return ConvertTextFileEncoding(bytes, from, to);
            }
            catch
            {
                return bytes;
            }
        }

        //converts the encoding a byte[] and return a new byte[] with the new encoding  
        public static byte[] ConvertTextFileEncoding(byte[] bytes, Encoding fromEncoding, Encoding toEncoding)
        {
            try
            {
                string str = fromEncoding.GetString(bytes);
                return toEncoding.GetBytes(str);
            }
            catch
            {
                return bytes;
            }
        }

        public static byte[] compressByteArray(byte[] array)
        {
            return CompressionUtil.Compress(array);
            //MemoryStream msCompressed = new MemoryStream();
            //GZipOutputStream gzCompressed = new GZipOutputStream(msCompressed);
            //gzCompressed.SetLevel(9);
            //gzCompressed.Write(array, 0, array.Length);
            //gzCompressed.Finish();
            //gzCompressed.IsStreamOwner = false;
            //gzCompressed.Close();
            //msCompressed.Seek(0, SeekOrigin.Begin);
            //byte[] compresseddata = new byte[msCompressed.Length];
            //msCompressed.Read(compresseddata, 0, compresseddata.Length);

            //return compresseddata;
        }

        public static string AddSuffix(string filename, string suffix)
        {
            string fDir = Path.GetDirectoryName(filename);
            string fName = Path.GetFileNameWithoutExtension(filename);
            string fExt = Path.GetExtension(filename);
            return Path.Combine(fDir, string.Concat(fName, suffix, fExt));
        }

        public static void DeleteOldFiles(string path, DateTime cutoffDate)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);

                if (!dirInfo.Exists)
                {
                    throw new DirectoryNotFoundException($"Directory '{path}' not found.");
                }

                foreach (FileInfo fileInfo in dirInfo.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        try
                        {
                            fileInfo.Attributes &= ~FileAttributes.ReadOnly; // remove read-only attribute if present
                            fileInfo.Delete();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deleting file '{fileInfo.FullName}': {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting old files: {ex.Message}");
            }
        }

        public static bool IsImageFile(string fileName)
        {
            return GetFileType(fileName) == SoeFileType.Image;
        }

        public static SoeFileType GetFileType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return SoeFileType.Unknown;

            string ext = Path.GetExtension(fileName).ToLowerInvariant();

            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    return SoeFileType.Image;
                case ".txt":
                    return SoeFileType.Txt;
                case ".pdf":
                    return SoeFileType.Pdf;
                case ".xml":
                    return SoeFileType.Xml;
                case ".xls":
                case ".xlsx":
                    return SoeFileType.Excel;
                case ".zip":
                    return SoeFileType.Zip;
                default:
                    return SoeFileType.Unknown;
            }
        }
    }
}
