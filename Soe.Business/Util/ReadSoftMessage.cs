using ReadSoft.Services.Entities;
using System;

namespace SoftOne.Soe.Business.Util
{
    public class ReadSoftMessage
    {
        #region Variables

        public string BatchId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ReceiveTime { get; set; }
        public Document Document { get; set; }
        public byte[] Image { get; set; }

        #endregion
    }
}
