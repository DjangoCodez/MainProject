using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SieExportDTO
    {
        public SieExportType ExportType { get; set; }
        public string Comment { get; set; }
        public bool ExportPreviousYear { get; set; }
        public bool ExportObject { get; set; }
        public bool ExportAccount { get; set; }
        public bool ExportAccountType { get; set; }
        public bool ExportSruCodes { get; set; }        
        public bool OnlyActiveAccounts { get; set; }
        public int AccountingYearId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int VoucherSeriesId { get; set; }
        public int VoucherNoFrom { get; set; }
        public int VoucherNoTo { get; set; }
        public List<SieExportVoucherSelectionDTO> VoucherSelection { get; set; } = new List<SieExportVoucherSelectionDTO>();
		public List<SieExportAccountSelectionDTO> AccountSelection { get; set; } = new List<SieExportAccountSelectionDTO>();
        public int? BudgetHeadId { get; set; }
        public TermGroup_SieExportVoucherSort sortVoucherBy { get; set; }


        [TSIgnore]
        public string LoginName { get; set; }
        [TSIgnore]
        public int ActorCompanyId { get; set; }
        [TSIgnore]
        public string Program { get; set; }
        [TSIgnore]
        public string Version { get; set; }
    }

    [TSInclude]
    public class SieExportAccountSelectionDTO
    {
        public int AccountDimId { get; set; }
        public string AccountNrFrom { get; set; }
        public string AccountNrTo { get; set; }
    }

	[TSInclude]
	public class SieExportVoucherSelectionDTO
	{
		public int VoucherSeriesId { get; set; }
		public long VoucherNoFrom { get; set; }
		public long VoucherNoTo { get; set; }
	}

	[TSInclude]
    public class SieExportResultDTO
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public string FileType { get; set; }
    }

    [TSInclude]
    public class SieExportConflictDTO
    {
        public string Label { get; set; }
        public string Message { get; set; }
    }

    public static class SieExportExtension
    {
        public static List<AccountIntervalDTO> ToAccountIntervalDTOs(this List<SieExportAccountSelectionDTO> l)
        {
            List<AccountIntervalDTO> lst = new List<AccountIntervalDTO>();
            if (!(l is null))
            {
                foreach (SieExportAccountSelectionDTO item in l)
                {
                    lst.Add(item.ToAccountIntervalDTO());
                }
            }
            return lst;
;       }

        public static AccountIntervalDTO ToAccountIntervalDTO(this SieExportAccountSelectionDTO s)
        { 
            return new AccountIntervalDTO
            {
                AccountDimId = s.AccountDimId,
                AccountNrFrom = s.AccountNrFrom,
                AccountNrTo = s.AccountNrTo,
            };
        }

		public static List<VoucherIntervalDTO> ToVoucherIntervalDTOs(this List<SieExportVoucherSelectionDTO> l)
		{
			List<VoucherIntervalDTO> lst = new List<VoucherIntervalDTO>();
			if (!(l is null))
			{
				foreach (SieExportVoucherSelectionDTO item in l)
				{
					lst.Add(item.ToVoucherIntervalDTO());
				}
			}
			return lst;
			;
		}

		public static VoucherIntervalDTO ToVoucherIntervalDTO(this SieExportVoucherSelectionDTO s)
		{
            return new VoucherIntervalDTO
            {
                VoucherSerieId = s.VoucherSeriesId,
                VoucherNoFrom = s.VoucherNoFrom,
                VoucherNoTo = s.VoucherNoTo == 0 ? long.MaxValue : s.VoucherNoTo,
            };
		}
	}
}
