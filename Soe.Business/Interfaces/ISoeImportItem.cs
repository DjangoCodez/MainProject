using System.Data;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Interfaces
{
    public interface ISoeImportItem
    {
        DataSet ToDataSet();
        XDocument ToXDocument();
    }
}
