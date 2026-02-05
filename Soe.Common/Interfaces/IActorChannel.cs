using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract]
    public interface IActorChannel
    {
        #region Monitor

        [OperationContract]
        bool Heartbeat();

        #endregion
    }
}
