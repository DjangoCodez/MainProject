using System;
using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract(Name = "ITimeChannel")]
    public interface ITimeChannelAsync
    {
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginHeartbeat(AsyncCallback callback, object asyncState);
        bool EndHeartbeat(IAsyncResult result);
    }
}

