using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract]
    public interface IAttestChannel
    {
        #region Monitor

        [OperationContract]
        bool Heartbeat();

        #endregion
    }
}
