using SoftOne.EdiAdmin.Business.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SoftOne.EdiAdmin.Business.Senders
{
    public abstract class EdiSenderXML<IN> : EdiSenderBase where IN : class
    {
        protected IN input;
        protected const string ENCODING = "ISO-8859-1"; //"UTF-8";
        protected abstract bool ParseInput();

        public override bool ConvertFile(string InputFolderFileName)
        {
            string fileName = InputFolderFileName;
            //Try parse as is
            bool success = TryParseFile(fileName, sendMailOnError: false);
            if (!success)
                success = TryParseFile(fileName, sendMailOnError: true, trimFile: true);

            return success;
        }

        public override bool ConvertMessage(string content)
        {
            return this.TryParseContent(content);
        }

        private bool TryParseContent(string content)
        {
            var ser = new XmlSerializer(typeof(IN));
            content = content.Replace("&#14;", string.Empty);
            var bytes = Encoding.UTF8.GetBytes(content);
            input = ser.Deserialize(new MemoryStream(bytes)) as IN;

            if (input == null)
            {
                Console.Error.WriteLine("[SB-1] Fel vid överföring från grossist: Meddelandefilen innehåller felaktigt Xml-format");
            }
            else
            {
                return this.ParseInput();
            }

            return false;
        }

        private bool TryParseFile(string fileName, bool sendMailOnError = false, bool trimFile = false)
        {
            try
            {
                Encoding encoding = Encoding.GetEncoding(ENCODING);

                using (var r = new StreamReader(fileName, encoding, true))
                {
                    if (trimFile)
                    {
                        var content = r.ReadToEnd();
                        // Remove unallowed characters
                        content = content.Replace("&#14;", string.Empty); //new MemoryStream(encoding.GetBytes(content))
                        return TryParseContent(content);
                    }
                    else
                    {
                        var content = r.ReadToEnd();
                        return this.TryParseContent(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write("[SB-2] Fel vid överföring från grossist: Meddelandefilen: " + fileName + ". EX: {0}", ex);

                return false;
            }
        }
    }
}
