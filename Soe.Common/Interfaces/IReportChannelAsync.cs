using System;
using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract(Name = "IReportChannel")]
    public interface IReportChannelAsync
    {
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginHeartbeat(AsyncCallback callback, object asyncState);
        bool EndHeartbeat(IAsyncResult result);
    }
}

