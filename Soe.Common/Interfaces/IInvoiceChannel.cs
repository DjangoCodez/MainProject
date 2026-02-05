using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract]
    public interface IInvoiceChannel
    {
        #region Monitor

        [OperationContract]
        bool Heartbeat();

        #endregion
    }
}
