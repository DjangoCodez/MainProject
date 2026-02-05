using SoftOne.Soe.Common.Attributes;
﻿using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class VoucherSeriesDTO
    {
        public int VoucherSeriesId { get; set; }
        public int VoucherSeriesTypeId { get; set; }
        public int AccountYearId { get; set; }

        public long? VoucherNrLatest { get; set; }
        public DateTime? VoucherDateLatest { get; set; }
        public TermGroup_AccountStatus? Status { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Extensions
        public string VoucherSeriesTypeName { get; set; }
        public bool VoucherSeriesTypeIsTemplate { get; set; }
        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
        public int VoucherSeriesTypeNr { get; set; }
    }

    public class VoucherSeriesIODTO
    {
        public int VoucherSeriesId { get; set; }
        public string Name { get; set; }
        public int Nr { get; set; }
        public string Description { get; set; }
        public long StartNumber { get; set; }
        public long VoucherNrLatest { get; set; }
        public DateTime? VoucherDateLatest { get; set; }
        public int Status { get; set; }

    }

    [TSInclude]
    public class VoucherSeriesTypeDTO
    {
        public int VoucherSeriesTypeId { get; set; }
        public int ActorCompanyId { get; set; }

        public string Name { get; set; }
        public int VoucherSeriesTypeNr { get; set; }
        public long StartNr { get; set; }
        public bool Template { get; set; }
        public bool YearEndSerie { get; set; }
        public bool ExternalSerie { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class VoucherSeriesTypeIODTO
    {

        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public int VoucherSeriesTypeNr { get; set; }
        public int StartNr { get; set; }
        public bool Template { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }
}
