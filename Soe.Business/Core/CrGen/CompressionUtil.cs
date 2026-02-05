using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.Util.Config;
using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.CrGen
{
    public static class CompressionUtil
    {
        #region Cregen

        public static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                try
                {
                    zipStream.CopyTo(resultStream);
                }
                catch (InvalidDataException ex)
                {
                    SysLoginConnector.LogErrorString("Decompress " + ex.ToString());
                }
                return resultStream.ToArray();
            }
        }

        static byte[] Decompress(string path)
        {
            var path2 = $@"{ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL}\{Guid.NewGuid()}";
            using (var compressedStream = new FileStream(path, FileMode.Open))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new FileStream(path2, FileMode.Create))
            {
                zipStream.CopyTo(resultStream);
            }

            var data = File.ReadAllBytes(path2);
            DeleteFile(path2);
            return data;
        }

        public static byte[] Compress(byte[] data)
        {
            if (data == null)
                return null;

            MemoryStream output = new MemoryStream();
            using (GZipStream dstream = new GZipStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Compress(string path)
        {
            byte[] data = null;
            var path2 = $@"{ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL}\{Guid.NewGuid()}";
            byte[] buffer = new byte[1024 * 1024]; // 1MB

            using (FileStream input = new FileStream(path, FileMode.Open))
            {
                using (FileStream output = new FileStream(path2, FileMode.Create))
                {
                    using (GZipStream dstream = new GZipStream(output, CompressionLevel.Optimal))
                    {
                        int bytesRead = 0;
                        while (bytesRead < input.Length)
                        {
                            int ReadLength = input.Read(buffer, 0, buffer.Length);
                            dstream.Write(buffer, 0, ReadLength);
                            dstream.Flush();
                            bytesRead += ReadLength;
                        }
                    }

                    data = File.ReadAllBytes(path2);
                }
            }

            DeleteFile(path2);
            return data;
        }

        public static byte[] CompressXDocument(XDocument document)
        {
            byte[] data = null;
            if (document == null)
                return data;

            var path = $@"{ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL}\{Guid.NewGuid()}";

            try
            {
                byte[] dataNotzipped = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    document.Save(ms);
                    ms.Position = 0;
                    dataNotzipped = ms.ToArray();
                }

                data = Compress(dataNotzipped);
            }
            catch (Exception ex)
            {
                SysLoginConnector.LogErrorString("Ex_CompressXDocument " + ex.ToString());

                try
                {
                    document.Save(path);
                    data = Compress(path);
                    DeleteFile(path);

                }
                catch (Exception ex2)
                {
                    SysLoginConnector.LogErrorString("Ex2_CompressXDocument " + ex2.ToString());

                    try
                    {
                        using (var stream = new FileStream(path + "temp", FileMode.CreateNew))
                        {
                            document.Save(stream);
                        }

                        data = Compress(path + "temp");
                        DeleteFile(path + "temp");
                    }
                    catch (Exception ex3)
                    {
                        SysLoginConnector.LogErrorString("Ex3_CompressXDocument " + ex3.ToString());
                    }
                }

            }

            return data;
        }

        public static XDocument DecompressFromBase64(string base64data)
        {
            XDocument doc = null;
            var path = $@"{ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL}\{Guid.NewGuid()}";
            var path2 = $@"{ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL}\{Guid.NewGuid()}";

            try
            {
                var unzippedData = Decompress(Convert.FromBase64String(base64data));

                using (var stream = new MemoryStream(unzippedData, false))
                {
                    doc = XDocument.Load(stream);
                }

            }
            catch (Exception ex)
            {
                SysConnectorBase.LogErrorString(ex.ToString());
                try
                {

                    var data = Convert.FromBase64String(base64data);
                    File.WriteAllBytes(path, data);
                    var unzippedDataFromFile = Decompress(path);
                    File.WriteAllBytes(path2, unzippedDataFromFile);

                    using (var stream = new FileStream(path2, FileMode.Open))
                    {
                        doc = XDocument.Load(stream);
                    }
                }
                catch (Exception ex2)
                {
                    SysLoginConnector.LogErrorString(ex2.ToString());
                }

                try
                {
                    DeleteFile(path);
                    DeleteFile(path2);
                }
                catch (Exception ex3)
                {
                    SysLoginConnector.LogErrorString(ex3.ToString());
                }
            }

            return doc;
        }

        private static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                SysLoginConnector.LogErrorString(ex.ToString());
            }
        }

        #endregion
    }
}
