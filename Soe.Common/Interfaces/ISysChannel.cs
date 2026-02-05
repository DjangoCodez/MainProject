using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract]
    public interface ISysChannel
    {
        #region Monitor

        [OperationContract]
        bool Heartbeat();

        #endregion
    }
}
