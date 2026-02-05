
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SoftOne.Soe.Util
{
    public class ZipUtility
    {
        static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        public static bool IsZipFile(byte[] data, string filename)
        {
            if (data?.Length < 4)
                return false;

            string[] zipExtensions = { ".zip" };
            string extension = Path.GetExtension(filename).ToLower();
            if (Array.Exists(zipExtensions, ext => ext == extension))
                return true;

            return false;
        }

        #region Zip

        public static bool ZipDirectory(string directorySourcePath, string zipFileName)
        {
            return ZipDirectory(directorySourcePath, zipFileName, String.Empty);
        }

        public static bool ZipDirectory(string directorySourcePath, string zipFileName, string searchPattern)
        {
            if (String.IsNullOrEmpty(directorySourcePath))
                return false;

            DirectoryInfo directoryInfo = new DirectoryInfo(directorySourcePath);
            if (directoryInfo.Exists)
                return ZipDirectory(directoryInfo, zipFileName, searchPattern);
            return false;
        }

        public static bool ZipDirectory(DirectoryInfo directoryInfo, string zipFileName)
        {
            return ZipDirectory(directoryInfo, zipFileName, String.Empty);
        }

        public static bool ZipDirectory(DirectoryInfo directoryInfo, string zipFileName, string searchPattern)
        {
            if (!directoryInfo.Exists)
                return false;

            FileInfo[] fileInfos = null;
            if (!String.IsNullOrEmpty(searchPattern))
                fileInfos = directoryInfo.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);
            else
                fileInfos = directoryInfo.GetFiles();

            return ZipFiles(zipFileName, fileInfos);
        }

        public static bool ZipFiles(string zipFileName, Dictionary<string, string> pathStringDict)
        {
            bool result = false;
            try
            {

                List<FileInfo> fileinfos = new List<FileInfo>();
                foreach (var item in pathStringDict)
                {
                    File.WriteAllText(item.Key, item.Value);
                    fileinfos.Add(new FileInfo(item.Key));
                }

                Stream dummy;
                result = ZipFiles(zipFileName, out dummy, false, fileinfos.ToArray());

                try
                {
                    foreach (var item in pathStringDict)
                    {
                        File.Delete(item.Key);
                        fileinfos.Add(new FileInfo(item.Key));
                    }
                }
                catch
                {
                    // Intentionally ignored, safe to continue
                    // NOSONAR
                }
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }

            return result;
        }



        public static bool ZipFiles(string zipFileName, Dictionary<string, byte[]> pathDataDict)
        {
            bool result = false;
            try
            {

                List<FileInfo> fileinfos = new List<FileInfo>();
                foreach (var item in pathDataDict)
                {
                    File.WriteAllBytes(item.Key, item.Value);
                    fileinfos.Add(new FileInfo(item.Key));
                }

                Stream dummy;
                result = ZipFiles(zipFileName, out dummy, false, fileinfos.ToArray());

                try
                {
                    foreach (var item in pathDataDict)
                    {
                        File.Delete(item.Key);
                        fileinfos.Add(new FileInfo(item.Key));
                    }
                }
                catch
                {
                    // Intentionally ignored, safe to continue
                    // NOSONAR
                }
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }

            return result;
        }

        public static bool ZipFiles(string zipFileName, params FileInfo[] fileInfos)
        {
            Stream dummy;
            return ZipFiles(zipFileName, out dummy, false, fileInfos);
        }



        public static bool ZipFiles(string zipFileName, out Stream zipOutPutStream, bool useZipOutPutStream, params FileInfo[] fileInfos)
        {
            using (ZipOutputStream zos = new ZipOutputStream(File.Create(zipFileName)))
            {
                zos.SetLevel(9);

                foreach (FileInfo fileInfo in fileInfos)
                {
                    //Dont zip itself
                    if (fileInfo.Extension == ".zip")
                        continue;

                    FileStream fs = File.OpenRead(fileInfo.FullName);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);

                    ZipEntry entry = new ZipEntry(fileInfo.Name)
                    {
                        DateTime = DateTime.Now,
                        Size = fs.Length,
                    };

                    fs.Close();

                    zos.PutNextEntry(entry);
                    zos.Write(buffer, 0, buffer.Length);
                }

                zos.Finish();
                zos.Close();

                if (useZipOutPutStream)
                    zipOutPutStream = zos;
                else
                    zipOutPutStream = null;
            }

            return true;
        }

        public static bool ZipFiles(string zipFileName, List<Tuple<string, byte[]>> files)
        {
            using (ZipOutputStream zos = new ZipOutputStream(File.Create(zipFileName)))
            {
                zos.SetLevel(9);

                foreach (var file in files)
                {
                    ZipEntry entry = new ZipEntry(file.Item1)
                    {
                        DateTime = DateTime.Now,
                        Size = file.Item2.Length,
                    };

                    zos.PutNextEntry(entry);
                    zos.Write(file.Item2, 0, file.Item2.Length);
                }

                zos.Finish();
                zos.Close();
            }

            return true;
        }

        #endregion

        public static byte[] CompressString(string str)
        {
            if (str == null)
                return null;

            using (Stream memOutput = new MemoryStream())
            {
                using (GZipOutputStream zipOut = new GZipOutputStream(memOutput))
                {
                    using (StreamWriter writer = new StreamWriter(zipOut))
                    {
                        writer.Write(str);

                        writer.Flush();
                        zipOut.Finish();

                        byte[] bytes = new byte[memOutput.Length];
                        memOutput.Seek(0, SeekOrigin.Begin);
                        memOutput.Read(bytes, 0, bytes.Length);

                        return bytes;
                    }
                }
            }
        }

        #region Unzip
        public static Dictionary<string, byte[]> UnzipFilesInZipFile(byte[] zipFile)
        {
            var extractedFiles = new Dictionary<string, byte[]>();

            using (var memoryStream = new MemoryStream(zipFile))
            {
                try
                {
                    using (var zipInputStream = new ZipInputStream(memoryStream))
                    {
                        ZipEntry entry;
                        while ((entry = zipInputStream.GetNextEntry()) != null)
                        {
                            if (!entry.IsDirectory && !string.IsNullOrEmpty(entry.Name) && !entry.Name.Contains("/"))
                            {
                                using (var extractedFileStream = new MemoryStream())
                                {
                                    zipInputStream.CopyTo(extractedFileStream);
                                    string fileName = entry.Name;
                                    if (extractedFiles.ContainsKey(fileName))
                                    {
                                        fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                                    }
                                    extractedFiles.Add(fileName, extractedFileStream.ToArray());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred, zip file: " + ex.Message);
                }
            }

            return extractedFiles;
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static string UnzipString(byte[] bytes)
        {
            try
            {
                using (var msi = new MemoryStream(bytes))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        //gs.CopyTo(mso);
                        CopyTo(gs, mso);
                    }

                    return Encoding.UTF8.GetString(mso.ToArray());
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }

        public static byte[] DecompressGZip(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }


        public static bool Unzip(Stream stream, string directoryDestination)
        {
            if (stream == null || !Directory.Exists(directoryDestination))
                return false;

            ZipInputStream zis = new ZipInputStream(stream);
            ZipEntry entry;
            while ((entry = zis.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(entry.Name);
                string fileName = Path.GetFileName(entry.Name);

                Directory.CreateDirectory(directoryDestination + directoryName);
                if (!String.IsNullOrEmpty(fileName))
                {
                    FileStream streamWriter = File.Create(directoryDestination + "\\" + entry.Name);

                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = zis.Read(data, 0, data.Length);
                        if (size > 0)
                            streamWriter.Write(data, 0, size);
                        else
                            break;
                    }

                    streamWriter.Close();

                    // Set date and time
                    File.SetCreationTime(directoryDestination + "\\" + entry.Name, entry.DateTime);
                    File.SetLastAccessTime(directoryDestination + "\\" + entry.Name, entry.DateTime);
                    File.SetLastWriteTime(directoryDestination + "\\" + entry.Name, entry.DateTime);
                }
            }
            zis.Close();

            return true;
        }

        public static byte[] Compress(string path, string tempPath)
        {
            byte[] data = null;
            byte[] buffer = new byte[1024 * 1024]; // 1MB

            using (FileStream input = new FileStream(path, FileMode.Open))
            {
                using (FileStream output = new FileStream(tempPath, FileMode.Create))
                {
                    using (GZipStream dstream = new GZipStream(output, CompressionMode.Compress))
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

                    data = File.ReadAllBytes(tempPath);
                }
            }

            try
            {
                File.Delete(tempPath);
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
            return data;
        }

        #endregion

        #region Byte array

        public static MemoryStream CreateToMemoryStream(MemoryStream memStreamIn, string zipEntryName)
        {

            MemoryStream outputMemStream = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

            zipStream.SetLevel(7);

            ZipEntry newEntry = new ZipEntry(zipEntryName);
            newEntry.DateTime = DateTime.Now;

            zipStream.PutNextEntry(newEntry);

            StreamUtils.Copy(memStreamIn, zipStream, new byte[4096]);
            zipStream.CloseEntry();

            zipStream.IsStreamOwner = false;
            zipStream.Close();

            outputMemStream.Position = 0;
            return outputMemStream;
        }

        public static byte[] GetDataFromStream(Stream stream)
        {
            byte[] data = null;

            try
            {
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                data = memoryStream.ToArray();

                //data = new byte[stream.Length];
                //stream.Read(data, 0, (int)stream.Length);
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }

            return data;
        }

        public static byte[] GetDataFromFile(string fileName)
        {
            byte[] data = null;
            FileStream fs = null;

            try
            {
                FileInfo fi = new FileInfo(fileName);
                fs = fi.OpenRead();
                data = GetDataFromStream(fs);

            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
            finally
            {
                if (fs != null)
                {
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                }
            }

            return data;
        }

        #endregion

        public static IEnumerable<Assembly> GetAssembliesFromArchive(string filePath)
        {
            List<Assembly> assemblies = new List<Assembly>();

            var zipFile = Ionic.Zip.ZipFile.Read(new MemoryStream(File.ReadAllBytes(filePath)));

            foreach (var entry in zipFile.Entries)
            {
                if (entry.FileName.EndsWith(".dll"))
                {
                    using (var stream = new MemoryStream())
                    {
                        entry.Extract(stream);
                        var file = stream.ToArray();
                        assemblies.Add(Assembly.Load(file));
                    }
                }
            }

            return assemblies;
        }
    }
}
