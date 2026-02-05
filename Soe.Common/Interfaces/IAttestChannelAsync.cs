using System;
using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract(Name = "IAttestChannel")]
    public interface IAttestChannelAsync
    {
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginHeartbeat(AsyncCallback callback, object asyncState);
        bool EndHeartbeat(IAsyncResult result);
    }
}

