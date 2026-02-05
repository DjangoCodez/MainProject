using SoftOne.Soe.Common.Attributes;
using System;

namespace SoftOne.Soe.Common.DTO
{

    [TSInclude]
    public class SoeBankerDownloadFileDTO
    {
        public int AvaloDownloadFileId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
        public string CompanyName { get; set; }
        public int? ActorCompanyId { get; set; }
    }
    [TSInclude]
    public class SoeBankerDownloadRequestDTO
    {
        public int AvaloDownloadRequestId { get; set; }
        public string Material { get; set; }
        public int MaterialCode { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string BankName { get; set; }
    }

    [TSInclude]
    public class SoeBankerOnboardingDTO
    {
        public int OnBoardingRequestId { get; set; }
        public int AvaloDownloadFileId { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string CompanyName { get; set; }
        public string CompanyOrgNr { get; set; }
        public string CompanyMasterOrgNr { get; set; }
        public string RegAction { get; set; }
        public int RegActionCode { get; set; }
        public string BankAccounts { get; set; }
        public string Emails { get; set; }
        public string BankName { get; set; }
        public string SigningTypeName { get; set; }
    }
    [TSInclude]
    public class SoeBankerRequestFilterDTO
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? OnlyError { get; set; }
        public int? MaterialType { get; set; }
        public int[] StatusCodes { get; set; } = new int[0];
    }
}
