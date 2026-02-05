using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Util
{
    public class EdiSenderInputParams
    {
        public string InputFolderFileName;
        public string WholesaleTempFolder;
        public DataSet dsStandardMall;
        public Dictionary<string, string> drEdiSettings;
        public DataRow SenderRow;
    }
}
