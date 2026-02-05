using System;
using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract(Name = "ICrystalChannel")]
    public interface ICrystalChannelAsync
    {
        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDoWork(AsyncCallback callback, object asyncState);

        string EndDoWork(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginPrintReportPackageData(System.Collections.Generic.List<SoftOne.Soe.Common.DTO.EvaluatedSelection> evaluatedSelections, int actorCompanyId, int userId, string culture, AsyncCallback callback, object asyncState);

        int EndPrintReportPackageData(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginPrintReport(SoftOne.Soe.Common.DTO.EvaluatedSelection es, int actorCompanyId, int userId, string culture, AsyncCallback callback, object asyncState);

        int EndPrintReport(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGenerateReportForEdi(System.Collections.Generic.List<int> ediEntryIds, int actorCompanyId, int userId, string culture, AsyncCallback callback, object asyncState);

        void EndGenerateReportForEdi(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetNrOfLoads(AsyncCallback callback, object asyncState);

        int EndGetNrOfLoads(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetNrOfDispose(AsyncCallback callback, object asyncState);

        int EndGetNrOfDispose(IAsyncResult result);
    }
}

