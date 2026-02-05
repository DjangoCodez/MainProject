using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract]
    public interface IProductChannel
    {
        #region Monitor

        [OperationContract]
        bool Heartbeat();

        #endregion
    }
}
