using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class EdiErrorsGridDTO
    {
        public int EdiTransferId { get; set; }
        public int EdiRecivedId { get; set; }
        public DateTime? MsgDate { get; set; }
        public string EdiRecivedMsgStateName { get; set; }
        public string WholesellerCode { get; set; }
        public string WholesellerName { get; set; }
        public string CompanyName { get; set; }
        public string FileInName { get; set; }
        public string FileOutName { get; set; }
        public string EdiTransferStateName { get; set; }
        public bool HasFileOut
        {
            get
            {
                return EdiRecivedId > 0 || !string.IsNullOrEmpty(FileOutName);
            }
        }
        public bool IsVisible { get; set; }
        public bool IsSelected { get; set; }
        public bool IsModified { get; set; }
        public DateTime Created { get; set; }
        public EdiRecivedMsgState EdiRecivedMsgState { get; set; }
        public EdiTransferState EdiTransferState { get; set; }
        public string EdiMessageTypeName { get; set; }
        public int? SysWholesellerEdiId { get; set; }
        public int? ActorCompanyId { get; set; }
        public DateTime? TransferDate { get; set; }
        public string WholesellerCustNrFile { get; set; }
        public string NewCustNr { get; set; }
        public string WholesellerOrderNrFile { get; set; }
        public string WholesellerNameFile { get; set; }
        public string CompanyNameFile { get; set; }

        public string EdiRecivedMsgErrorMessage { get; set; }

        public string EdiTransferErrorMessage { get; set; }
    }
}
