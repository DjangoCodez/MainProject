using SoftOne.EdiAdmin.Business.Interfaces;
using SoftOne.EdiAdmin.Business.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Senders
{
    public abstract class EdiSenderOldBase : IEdiSenderOld
    {
        protected EdiSenderInputParams inputParams;
        private List<string> parsedMessages;

        protected List<string> ParsedMessages
        {
            get
            {
                if (this.parsedMessages == null)
                    this.parsedMessages = new List<string>();

                return parsedMessages;
            }
        }

        public void SetInputParams(EdiSenderInputParams inputParams)
        {
            this.inputParams = inputParams;
        }

        public bool ConvertMessage(string content)
        {
            this.parsedMessages = this.ConvertMessage(inputParams.InputFolderFileName, inputParams.WholesaleTempFolder, inputParams.dsStandardMall, inputParams.drEdiSettings, inputParams.SenderRow, content).ToList();
            return parsedMessages != null && parsedMessages.Count() > 0;
        }

        protected abstract IEnumerable<string> ConvertMessage(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, DataRow SenderRow, string fileContent);

        public IEnumerable<string> ToXmls()
        {
            return this.parsedMessages;
        }
    }
}
