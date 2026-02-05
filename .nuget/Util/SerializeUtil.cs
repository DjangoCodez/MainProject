using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SoftOne.EdiAdmin.Business.Util
{
    public static class SerializeUtil
    {
        public static Encoding DefaultEncoding = SoftOne.Soe.Common.Util.Constants.ENCODING_LATIN1;

        public static string ToXml(object output)
        {
            string xml = null;
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false,
                Encoding = Encoding.Unicode
            };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);

                var x = new System.Xml.Serialization.XmlSerializer(output.GetType());
                x.Serialize(xmlWriter, output);

                memoryStream.Position = 0; // rewind the stream before reading back.
                using (StreamReader sr = new StreamReader(memoryStream))
                {
                    xml = sr.ReadToEnd();
                }
            }

            return xml;
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static Encoding GetEncoding(byte[] data)
        {
            Encoding encoding;
            using (var memoryStream = new MemoryStream(data))
            {
                encoding = GetEncoding(memoryStream);
            }
            return encoding;
        }

        public static Encoding GetEncoding(string fileName)
        {
            Encoding encoding;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                encoding = GetEncoding(fileStream);
            }
            return encoding;
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to SerializeUtil.Default when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(Stream stream)
        {
            // Read the BOM
            var bom = new byte[4];
            stream.Read(bom, 0, 4);

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;

            // Default is ISO-8859-1
            return DefaultEncoding;
        }
    }
}
