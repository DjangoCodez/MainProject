using SoftOne.EdiAdmin.Business.Interfaces;
using SoftOne.EdiAdmin.Business.Util;
using SoftOne.Soe.EdiAdmin.Business.FileDefinitions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SoftOne.EdiAdmin.Business.Senders
{
    public abstract class EdiSenderBase : IEdiSender
    {
        protected List<Message> outputs = new List<Message>();
        private EdiDiverse EdiDiverseKlass = new EdiDiverse();

        public abstract bool ConvertFile(string inputFile);
        public abstract bool ConvertMessage(string content);

        public IEnumerable<string> ToXmls()
        {
            foreach (var message in this.outputs)
            {
                string xml = null;

                try
                {
                    xml = SerializeUtil.ToXml(message);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("EdiSenderXML[2]: Could not serialize XML to string: {0}", ex);
                }

                yield return xml;
            }
        }
    }
}
