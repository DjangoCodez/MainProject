using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;
using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ICrystalChannel" in both code and config file together.
    [ServiceContract]
    public interface ICrystalChannel
    {
        [OperationContract]
        string DoWork();

        [OperationContract]
        int PrintReportPackageData(List<EvaluatedSelection> evaluatedSelections, int actorCompanyId, int userId, string culture);

        [OperationContract]
        int PrintReport(EvaluatedSelection es, int actorCompanyId, int userId, string culture);

        [OperationContract]
        byte[] PrintReportGetData(EvaluatedSelection es, int actorCompanyId, int userId, string culture);

        [OperationContract]
        void GenerateReportForEdi(List<int> ediEntryIds, int actorCompanyId, int userId, string culture);

        [OperationContract]
        int GetNrOfLoads();

        [OperationContract]
        int GetNrOfDispose();
    }
}
