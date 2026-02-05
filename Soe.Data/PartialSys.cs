using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Threading;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;

namespace SoftOne.Soe.Data
{
    #region Sys functions

    public partial class SOESysEntities
    {
        public bool IsReadOnly { get; set; }

        public DbConnection Connection
        {
            get { return this.Database.Connection; }
        }

        public int? CommandTimeout
        {
            get { return this.Database.CommandTimeout; }
            set { this.Database.CommandTimeout = value; }
        }

        public int ThreadId { get; set; }
        public bool IsTaskScoped { get; set; } = false;
    }

    public partial class Sys
    {
        private bool? fromCore;

        public bool? FromCore
        {
            get { return fromCore; }
            set { fromCore = value; }
        }

        public static SOESysEntities CreateSOESysEntities(bool isReadOnly = false, bool requestScoped = false)
        {
            SOESysEntities entities = new SOESysEntities();
            entities.IsReadOnly = isReadOnly;
            entities.RequestScoped = requestScoped;
            entities.Configuration.LazyLoadingEnabled = true;
            entities.ThreadId = Thread.CurrentThread.ManagedThreadId;
            if (isReadOnly)
            {
                // Disable tracking
                entities.Configuration.AutoDetectChangesEnabled = false;
                entities.Configuration.ValidateOnSaveEnabled = false;
                entities.Configuration.ProxyCreationEnabled = false;
            }
            return entities;
        }
    }

    #endregion

    #region Tables

    public partial class SysCountry
    {
        [NotMapped]
        public string Name { get; set; }
    }

    public partial class SysCurrency
    {
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public string Description { get; set; }
    }

    public partial class SysExtraField
    {
        [NotMapped]
        public string Name { get; set; }
    }

    public partial class SysGauge
    {
        [NotMapped]
        public string Name { get; set; }
    }

    public partial class SysGridState : IGridState
    {

    }

    public partial class SysHouseholdType
    {
        [NotMapped]
        public string Name { get; set; }
    }

    public partial class SysImportDefinitionLevel
    {
        [NotMapped]
        public List<SysImportDefinitionLevelColumnSettings> Columns { get; set; }
    }

    public partial class SysInformation
    {
        [NotMapped]
        public string SeverityName { get; set; }

        public bool ValidateSendPush(DateTime sendTime)
        {
            if (this.ShowInMobile && this.Notify && !this.NotificationSent.HasValue)
            {
                if (!this.ValidFrom.HasValue && !this.ValidTo.HasValue)
                    return true;

                DateTime from = this.ValidFrom ?? DateTime.MinValue;
                DateTime to = this.ValidTo ?? DateTime.MaxValue;

                if (to < sendTime || (from <= sendTime && to >= sendTime))
                    return true;
            }
            return false;
        }
    }

    public partial class SysInformationSysCompDb
    {
        [NotMapped]
        public string SiteName { get; set; }
    }

    public partial class SysImportSelect
    {
        [NotMapped]
        public List<SysImportSelectColumnSettings> SettingsObject { get; set; }
    }

    public partial class SysLog
    {
        [NotMapped]
        public int UniqueCounter { get; set; }
        [NotMapped]
        public string UniqueError { get { return $"{this.Message}-{this.Exception}"; } }
        [NotMapped]
        public string TaskWatchLogStart { get; set; }
        [NotMapped]
        public string TaskWatchLogStop { get; set; }
        [NotMapped]
        public string TaskWatchLogName { get; set; }
        [NotMapped]
        public string TaskWatchLogParameters { get; set; }
    }

    public partial class SysMedia
    {
        [NotMapped]
        public string TypeName { get; set; }
    }

    public partial class SysPageStatus
    {
        [NotMapped]
        public string PageName { get; set; }
        [NotMapped]
        public string BetaStatusName { get; set; }
        [NotMapped]
        public string LiveStatusName { get; set; }
    }

    public partial class SysPaymentMethod
    {
        [NotMapped]
        public string Name { get; set; }
    }

    public partial class SysPayrollPrice
    {
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public string TypeName { get; set; }
        [NotMapped]
        public string AmountTypeName { get; set; }
    }

    public partial class SysPayrollPriceInterval
    {
        [NotMapped]
        public string AmountTypeName { get; set; }
        [NotMapped]
        public string IntervalName
        {
            get
            {
                return String.Format("{0}-{1}", this.FromInterval.HasValue ? this.FromInterval.ToString() : "", this.ToInterval.HasValue ? this.ToInterval.ToString() : "");
            }
        }
    }

    public partial class SysPosition
    {
        [NotMapped]
        public bool IsLinked { get; set; }
    }

    public partial class SysReportTemplate
    {
        [NotMapped]
        public List<int> SysCountryIds { get; set; }
    }
    public partial class SysReportGroup
    {
        [NotMapped]
        public string TemplateType { get; set; }
    }
    public partial class SysReportHeader
    {
        [NotMapped]
        public string TemplateType { get; set; }
    }
    public partial class SysScheduledJob
    {
        [NotMapped]
        public string StateName { get; set; }
    }

    public partial class SysScheduledJobLog
    {
        [NotMapped]
        public string LogLevelName { get; set; }
    }

    public partial class SysTimeInterval
    {
        [NotMapped]
        public string Name { get; set; }
    }

    public partial class SysVatAccount
    {
        [NotMapped]
        private string description = "";
        [NotMapped]
        public string Description
        {
            get
            {
                // Format = 'VatNr1+VatNr2. Name'
                if (String.IsNullOrEmpty(description))
                {
                    // Add VatNr1 if it contains anything
                    if (VatNr1.HasValue)
                    {
                        description += VatNr1.Value.ToString();
                    }


                    // Add VatNr2 if it contains anything
                    if (VatNr2.HasValue)
                    {
                        // If we already added VatNr1, insert a + between them
                        description += description?.Length > 0 ? "+" : String.Empty;
                        description += VatNr2.Value.ToString();
                    }

                    // If we added VatNr1 or VatNr2 (or both) before, insert a . between them
                    description += description?.Length > 0 ? ". " : String.Empty;

                    // Add Name
                    description += Name;
                }

                return description;
            }
        }
    }

    #endregion

    #region Views

    public partial class SysReportTemplateView
    {
        [NotMapped]
        public string SysReportTemplateTypeName { get; set; }
        [NotMapped]
        public string GroupName { get; set; }
    }

    #endregion
}
