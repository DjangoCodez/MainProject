using SoftOne.Soe.Common.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoftOne.Soe.Common.DTO.Sie
{
    [TSInclude]
    public class SieImportPreviewDTO
    {
        public bool FileContainsAccountStd { get; set; }
        public bool FileContainsVouchers { get; set; }
        public bool FileContainsAccountBalances { get; set; }

        public DateTime? AccountingYearFrom { get; set; }
        public DateTime? AccountingYearTo { get; set; }
        public int? AccountingYearId { get; set; }
        public bool? AccountingYearIsClosed { get; set; }

        public List<SieVoucherSeriesMappingDTO> VoucherSeriesMappings { get; set; }

        public SieAccountDimMappingDTO AccountStd { get; set; }
        public List<SieAccountDimMappingDTO> AccountDims { get; set; } = new List<SieAccountDimMappingDTO>();


        public List<SieImportConflictDTO> Conflicts { get; set; } = null;
    }


    [TSInclude]
    public class SieVoucherSeriesMappingDTO
    {
        public string Number { get; set; }
        public long VoucherNrFrom { get; set; }
        public long VoucherNrTo { get; set; }
        public int VoucherSeriesTypeId { get; set; }
    }

    [TSInclude]
    public class SieAccountDimMappingDTO
    {
        public int DimNr { get; set; }
        public string Name { get; set; }

        public int? AccountDimId { get; set; }
        public bool IsAccountStd { get; set; }

        public List<SieAccountMappingDTO> AccountMappings { get; set; } = new List<SieAccountMappingDTO>();
        public bool IsImport { get; set; } = false;
    }

    [TSInclude]
    public class SieAccountMappingDTO
    {
        public string Name { get; set; }
        public string Number { get; set; }
        public int? AccountId { get; set; }
        public int Action { get; set; } = 0;
    }

    [TSInclude]
    public class SieImportDTO
    {
        //File
        public FileDTO File { get; set; }

        //AccountYear
        public int AccountYearId { get; set; }
        public bool AllowNotOpenAccountYear { get; set; }

        //Account import
        public bool ImportAccounts { get; set; }
        public bool OverrideNameConflicts { get; set; }
        public bool ApproveEmptyAccountNames { get; set; }
        public bool ImportAccountStd { get; set; }
        public bool ImportAccountInternal { get; set; }
        public string EmptyAccountName { get; set; }

        public SieImportPreviewDTO SieImportPreview { get; set; }

        //Voucher import
        public bool ImportVouchers { get; set; }
        public bool OverrideVoucherSeries { get; set; }
        public bool SkipAlreadyExistingVouchers { get; set; }
        public bool UseAccountDistribution { get; set; }
        public bool TakeVoucherNrFromSeries { get; set; }

        public bool OverrideVoucherSeriesDelete { get; set; }
        public List<int> VoucherSeriesDelete { get; set; }
        public bool HasVoucherSeriesDeletes
        {
            get
            {
                return VoucherSeriesDelete != null && VoucherSeriesDelete.Count > 0;
            }
        }

        public int? DefaultVoucherSeriesId { get; set; }
        public Dictionary<string, int> VoucherSeriesTypesMappingDict { get; set; }
        public bool HasVoucherSeriesTypesMapping
        {
            get
            {
                return VoucherSeriesTypesMappingDict != null && VoucherSeriesTypesMappingDict.Count > 0;
            }
        }

        //AccountBalance import
        public bool ImportAccountBalances { get; set; }
        public bool OverrideAccountBalance { get; set; }
        public bool UseUBInsteadOfIB { get; set; }
    }

    [TSInclude]
    public class SieImportResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public List<SieImportConflictDTO> ImportConflicts { get; set; }
    }

    [TSInclude]
    public class SieImportConflictDTO
    {
        public string Label { get; set; }
        public int RowNr { get; set; }
        public string Value { get; set; }
        public string Conflict { get; set; }
    }

    [TSInclude]
    public class SieReverseImportDTO
    {
        public int FileImportHeadId { get; set; }
        public string Comment { get; set; }
    }
}
