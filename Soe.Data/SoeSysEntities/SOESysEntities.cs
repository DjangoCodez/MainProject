using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Diagnostics;

namespace SoftOne.Soe.Data
{
    public partial class SOESysEntities : DbContext
    {
        public SOESysEntities() : base(GetConnectionString())
        {
            // Added this since we are not using migrations and this expects the migrations table to exist.
            Database.SetInitializer<SOESysEntities>(null);
        }

        public static SqlConnectionStringBuilder _connectionStringBuilder;

        public static void SetSqlConnectionStringBuilder(SqlConnectionStringBuilder builder)
        {
            _connectionStringBuilder = _connectionStringBuilder == null ? builder : _connectionStringBuilder;
        }

        public static string GetConnectionString()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["SOESysEntities"];
            if (connectionStringSettings != null)
            {
                return connectionStringSettings.ConnectionString;
            }
            else if (_connectionStringBuilder != null)
            {
                return _connectionStringBuilder.ConnectionString;
            }

            throw new Exception("Connection string not found in the configuration file.");
        }

        public static bool HasValidConnectionString() => !string.IsNullOrEmpty(GetConnectionString());

        public SOESysEntities(string connectionString)
        {
            this.Database.Connection.ConnectionString = connectionString;
        }

        public bool RequestScoped { get; set; }
        public bool IsDisposed { get; private set; } = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                base.Dispose(false);
                return;
            }

            if (IsTaskScoped)
                return; // Caller must call DisposeNow

            if (RequestScoped)
                return; // EndRequest will call DisposeNow

            IsDisposed = true;
            base.Dispose(true);        
        }

        public void DisposeNow()
        {
            try
            {
                IsDisposed = true;
                base.Dispose(true);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to dispose CompEntities instance. Exception: " + ex.Message);
            }
        }

        public virtual DbSet<SysAccountSruCode> SysAccountSruCode { get; set; }
        public virtual DbSet<SysAccountStd> SysAccountStd { get; set; }
        public virtual DbSet<SysAccountStdType> SysAccountStdType { get; set; }
        public virtual DbSet<SysBank> SysBank { get; set; }
        public virtual DbSet<SysCompany> SysCompany { get; set; }
        public virtual DbSet<SysCompanySetting> SysCompanySetting { get; set; }
        public virtual DbSet<SysCompanyBankAccount> SysCompanyBankAccount { get; set; }
        public virtual DbSet<SysCompanyUniqueValue> SysCompanyUniqueValue { get; set; }
        public virtual DbSet<SysCompDb> SysCompDb { get; set; }
        public virtual DbSet<SysCompServer> SysCompServer { get; set; }
        public virtual DbSet<SysConnectApiKey> SysConnectApiKey { get; set; }
        public virtual DbSet<SysContactAddressRowType> SysContactAddressRowType { get; set; }
        public virtual DbSet<SysContactAddressType> SysContactAddressType { get; set; }
        public virtual DbSet<SysContactEComType> SysContactEComType { get; set; }
        public virtual DbSet<SysContactType> SysContactType { get; set; }
        public virtual DbSet<SysCountry> SysCountry { get; set; }
        public virtual DbSet<SysCurrency> SysCurrency { get; set; }
        public virtual DbSet<SysCurrencyRate> SysCurrencyRate { get; set; }
        public virtual DbSet<SysDayType> SysDayType { get; set; }
        public virtual DbSet<SysEdiMessageHead> SysEdiMessageHead { get; set; }
        public virtual DbSet<SysEdiMessageRaw> SysEdiMessageRaw { get; set; }
        public virtual DbSet<SysEdiMessageRow> SysEdiMessageRow { get; set; }
        public virtual DbSet<SysEdiMsg> SysEdiMsg { get; set; }
        public virtual DbSet<SysEdiType> SysEdiType { get; set; }
        public virtual DbSet<SysExportDefinition> SysExportDefinition { get; set; }
        public virtual DbSet<SysExportDefinitionLevel> SysExportDefinitionLevel { get; set; }
        public virtual DbSet<SysExportHead> SysExportHead { get; set; }
        public virtual DbSet<SysExportRelation> SysExportRelation { get; set; }
        public virtual DbSet<SysExportSelect> SysExportSelect { get; set; }
        public virtual DbSet<SysExtraField> SysExtraField { get; set; }
        public virtual DbSet<SysFeature> SysFeature { get; set; }
        public virtual DbSet<SysGauge> SysGauge { get; set; }
        public virtual DbSet<SysGaugeModule> SysGaugeModule { get; set; }
        public virtual DbSet<SysGridState> SysGridState { get; set; }
        public virtual DbSet<SysHelp> SysHelp { get; set; }
        public virtual DbSet<SysHoliday> SysHoliday { get; set; }
        public virtual DbSet<SysHolidayType> SysHolidayType { get; set; }
        public virtual DbSet<SysHouseholdType> SysHouseholdType { get; set; }
        public virtual DbSet<SysImportDefinition> SysImportDefinition { get; set; }
        public virtual DbSet<SysImportDefinitionLevel> SysImportDefinitionLevel { get; set; }
        public virtual DbSet<SysImportHead> SysImportHead { get; set; }
        public virtual DbSet<SysImportHeadType> SysImportHeadType { get; set; }
        public virtual DbSet<SysImportRelation> SysImportRelation { get; set; }
        public virtual DbSet<SysImportSelect> SysImportSelect { get; set; }
        public virtual DbSet<SysInformation> SysInformation { get; set; }
        public virtual DbSet<SysInformationFeature> SysInformationFeature { get; set; }
        public virtual DbSet<SysInformationSysCompDb> SysInformationSysCompDb { get; set; }
        public virtual DbSet<SysIntrastatCode> SysIntrastatCode { get; set; }
        public virtual DbSet<SysIntrastatText> SysIntrastatText { get; set; }
        public virtual DbSet<SysJob> SysJob { get; set; }
        public virtual DbSet<SysJobSetting> SysJobSetting { get; set; }
        public virtual DbSet<SysJobSettingJob> SysJobSettingJob { get; set; }
        public virtual DbSet<SysJobSettingScheduledJob> SysJobSettingScheduledJob { get; set; }
        public virtual DbSet<SysLanguage> SysLanguage { get; set; }
        public virtual DbSet<SysLbError> SysLbError { get; set; }
        public virtual DbSet<SysLinkTable> SysLinkTable { get; set; }
        public virtual DbSet<SysLog> SysLog { get; set; }
        public virtual DbSet<SysMedia> SysMedia { get; set; }
        public virtual DbSet<SysMicroService> SysMicroService { get; set; }
        public virtual DbSet<SysNameCollision> SysNameCollision { get; set; }
        public virtual DbSet<SysNews> SysNews { get; set; }
        public virtual DbSet<SysPageStatus> SysPageStatus { get; set; }
        public virtual DbSet<SysParameter> SysParameter { get; set; }
        public virtual DbSet<SysParameterType> SysParameterType { get; set; }
        public virtual DbSet<SysPaymentMethod> SysPaymentMethod { get; set; }
        public virtual DbSet<SysPaymentService> SysPaymentService { get; set; }
        public virtual DbSet<SysPaymentType> SysPaymentType { get; set; }
        public virtual DbSet<SysPayrollPrice> SysPayrollPrice { get; set; }
        public virtual DbSet<SysPayrollPriceInterval> SysPayrollPriceInterval { get; set; }
        public virtual DbSet<SysPayrollStartValue> SysPayrollStartValue { get; set; }
        public virtual DbSet<SysPayrollType> SysPayrollType { get; set; }
        public virtual DbSet<SysPerformanceMonitor> SysPerformanceMonitor { get; set; }
        public virtual DbSet<SysPermission> SysPermission { get; set; }
        public virtual DbSet<SysPosition> SysPosition { get; set; }
        public virtual DbSet<SysPriceList> SysPriceList { get; set; }
        public virtual DbSet<SysPriceListHead> SysPriceListHead { get; set; }
        public virtual DbSet<SysPriceListTempHead> SysPriceListTempHead { get; set; }
        public virtual DbSet<SysPriceListTempItem> SysPriceListTempItem { get; set; }
        public virtual DbSet<SysProduct> SysProduct { get; set; }
        public virtual DbSet<SysProductGroup> SysProductGroup { get; set; }
        public virtual DbSet<SysReportGroup> SysReportGroup { get; set; }
        public virtual DbSet<SysReportGroupHeaderMapping> SysReportGroupHeaderMapping { get; set; }
        public virtual DbSet<SysReportGroupMapping> SysReportGroupMapping { get; set; }
        public virtual DbSet<SysReportHeader> SysReportHeader { get; set; }
        public virtual DbSet<SysReportHeaderInterval> SysReportHeaderInterval { get; set; }
        public virtual DbSet<SysReportTemplate> SysReportTemplate { get; set; }
        public virtual DbSet<SysReportTemplateType> SysReportTemplateType { get; set; }
        public virtual DbSet<SysReportTemplateSetting> SysReportTemplateSetting { get; set; }
        public virtual DbSet<SysReportType> SysReportType { get; set; }
        public virtual DbSet<SysScheduledJob> SysScheduledJob { get; set; }
        public virtual DbSet<SysScheduledJobLog> SysScheduledJobLog { get; set; }
        public virtual DbSet<SysServer> SysServer { get; set; }
        public virtual DbSet<SysServerLogin> SysServerLogin { get; set; }
        public virtual DbSet<SysSetting> SysSetting { get; set; }
        public virtual DbSet<SysSettingType> SysSettingType { get; set; }
        public virtual DbSet<SystemAdminMessage> SystemAdminMessage { get; set; }
        public virtual DbSet<SysTerm> SysTerm { get; set; }
        public virtual DbSet<SysTermGroup> SysTermGroup { get; set; }
        public virtual DbSet<SysTimeInterval> SysTimeInterval { get; set; }
        public virtual DbSet<SysVatAccount> SysVatAccount { get; set; }
        public virtual DbSet<SysVatRate> SysVatRate { get; set; }
        public virtual DbSet<SysVehicleType> SysVehicleType { get; set; }
        public virtual DbSet<SysWholeseller> SysWholeseller { get; set; }
        public virtual DbSet<SysWholesellerEdi> SysWholesellerEdi { get; set; }
        public virtual DbSet<SysWholesellerSetting> SysWholesellerSetting { get; set; }
        public virtual DbSet<SysXEArticle> SysXEArticle { get; set; }
        public virtual DbSet<SysXEArticleFeature> SysXEArticleFeature { get; set; }
        public virtual DbSet<SysPayrollPriceView> SysPayrollPriceView { get; set; }
        public virtual DbSet<SysPayrollTypeView> SysPayrollTypeView { get; set; }
        public virtual DbSet<SysPriceListHeadView> SysPriceListHeadView { get; set; }
        public virtual DbSet<SysProductPriceSearchView> SysProductPriceSearchView { get; set; }
        public virtual DbSet<SysProductWholeSellerMappingView> SysProductWholeSellerMappingView { get; set; }
        public virtual DbSet<SysReportTemplateView> SysReportTemplateView { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SysAccountSruCode>()
                .HasMany(e => e.SysAccountStd)
                .WithMany(e => e.SysAccountSruCode)
                .Map(m => m.ToTable("SysAccountStdSruCodeMapping").MapLeftKey("SysAccountSruCodeId").MapRightKey("SysAccountStdId"));

            modelBuilder.Entity<SysAccountStdType>()
                .HasMany(e => e.SysAccountStd)
                .WithRequired(e => e.SysAccountStdType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysAccountStdType>()
                .HasMany(e => e.SysAccountStdType1)
                .WithOptional(e => e.SysAccountStdType2)
                .HasForeignKey(e => e.SysAccountStdTypeParentId);

            modelBuilder.Entity<SysBank>()
                .HasMany(e => e.SysCompanyBankAccounts)
                .WithRequired(e => e.SysBank)
                .HasForeignKey(e => e.SysBankId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCompany>()
                .HasMany(e => e.SysCompanySetting)
                .WithRequired(e => e.SysCompany)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCompany>()
                .HasMany(e => e.SysCompanyUniqueValues)
                .WithRequired(e => e.SysCompany)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCompany>()
                .HasMany(e => e.SysCompanyBankAccounts)
                .WithRequired(e => e.SysCompany)
                .HasForeignKey(e => e.SysCompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCompanySetting>()
                .Property(e => e.DecimalValue)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysCompDb>()
                .HasMany(e => e.SysCompany)
                .WithRequired(e => e.SysCompDb)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCompDb>()
                .HasMany(e => e.SysInformationSysCompDb)
                .WithRequired(e => e.SysCompDb)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCompServer>()
                .HasMany(e => e.SysCompDb)
                .WithRequired(e => e.SysCompServer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysContactAddressType>()
                .HasMany(e => e.SysContactAddressRowType)
                .WithRequired(e => e.SysContactAddressType)
                .HasForeignKey(e => new { e.SysContactAddressTypeId, e.SysContactTypeId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysContactType>()
                .HasMany(e => e.SysContactAddressType)
                .WithRequired(e => e.SysContactType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysContactType>()
                .HasMany(e => e.SysContactEComType)
                .WithRequired(e => e.SysContactType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCountry>()
                .HasMany(e => e.SysBank)
                .WithRequired(e => e.SysCountry)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCountry>()
                .HasMany(e => e.SysPayrollPrice)
                .WithRequired(e => e.SysCountry)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCountry>()
                .HasMany(e => e.SysPosition)
                .WithRequired(e => e.SysCountry)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCountry>()
                .HasMany(e => e.SysExtraField)
                .WithRequired(e => e.SysCountry)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCountry>()
                .HasMany(e => e.SysHolidayType)
                .WithRequired(e => e.SysCountry)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCountry>()
                .HasMany(e => e.SysHouseholdType)
                .WithRequired(e => e.SysCountry)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCurrency>()
                .HasMany(e => e.SysCurrencyRate)
                .WithRequired(e => e.SysCurrency)
                .HasForeignKey(e => e.SysCurrencyToId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCurrency>()
                .HasMany(e => e.SysCurrencyRate1)
                .WithRequired(e => e.SysCurrency1)
                .HasForeignKey(e => e.SysCurrencyFromId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysCurrencyRate>()
                .Property(e => e.Rate)
                .HasPrecision(15, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadVatPercentage)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadInvoiceGrossAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadInvoiceNetAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadVatBasisAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadVatAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadFreightFeeAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadHandlingChargeFeeAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadInsuranceFeeAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadRemainingFeeAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadDiscountAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadRoundingAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .Property(e => e.HeadBonusAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageHead>()
                .HasMany(e => e.SysEdiMessageRow)
                .WithRequired(e => e.SysEdiMessageHead)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysEdiMessageRow>()
                .Property(e => e.RowQuantity)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageRow>()
                .Property(e => e.RowDiscountPercent)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageRow>()
                .Property(e => e.RowDiscountAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageRow>()
                .Property(e => e.RowDiscountPercent1)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageRow>()
                .Property(e => e.RowDiscountAmount1)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageRow>()
                .Property(e => e.RowDiscountPercent2)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageRow>()
                .Property(e => e.RowDiscountAmount2)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageRow>()
                .Property(e => e.RowNetAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageRow>()
                .Property(e => e.RowVatAmount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMessageRow>()
                .Property(e => e.RowVatPercentage)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysEdiMsg>()
                .Property(e => e.SenderSenderNr)
                .IsUnicode(false);

            modelBuilder.Entity<SysEdiMsg>()
                .Property(e => e.SenderType)
                .IsUnicode(false);

            modelBuilder.Entity<SysEdiType>()
                .Property(e => e.TypeCode)
                .IsUnicode(false);

            modelBuilder.Entity<SysEdiType>()
                .Property(e => e.TypeName)
                .IsUnicode(false);

            modelBuilder.Entity<SysEdiType>()
                .Property(e => e.TypeFolder)
                .IsUnicode(false);

            modelBuilder.Entity<SysEdiType>()
                .HasMany(e => e.SysEdiMsg)
                .WithRequired(e => e.SysEdiType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysExportDefinition>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportDefinition>()
                .Property(e => e.Separator)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportDefinition>()
                .Property(e => e.XmlTagHead)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportDefinition>()
                .Property(e => e.SpecialFunctionality)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportDefinition>()
                .HasMany(e => e.SysExportDefinitionLevel)
                .WithRequired(e => e.SysExportDefinition)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysExportDefinitionLevel>()
                .Property(e => e.Xml)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportHead>()
                .HasMany(e => e.SysExportDefinition)
                .WithRequired(e => e.SysExportHead)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysExportHead>()
                .HasMany(e => e.SysExportRelation)
                .WithRequired(e => e.SysExportHead)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysExportHead>()
                .HasMany(e => e.SysExportSelect)
                .WithRequired(e => e.SysExportHead)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysExportRelation>()
                .Property(e => e.FieldParent)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportRelation>()
                .Property(e => e.FieldChild)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportSelect>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportSelect>()
                .Property(e => e.Select)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportSelect>()
                .Property(e => e.Where)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportSelect>()
                .Property(e => e.GroupBy)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportSelect>()
                .Property(e => e.OrderBy)
                .IsUnicode(false);

            modelBuilder.Entity<SysExportSelect>()
                .Property(e => e.Settings)
                .IsUnicode(false);

            modelBuilder.Entity<SysFeature>()
                .HasMany(e => e.SysHelp)
                .WithRequired(e => e.SysFeature)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysFeature>()
                .HasMany(e => e.SysInformationFeature)
                .WithRequired(e => e.SysFeature)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysFeature>()
                .HasMany(e => e.SysPageStatus)
                .WithRequired(e => e.SysFeature)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysFeature>()
                .HasMany(e => e.SysXEArticleFeature)
                .WithRequired(e => e.SysFeature)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysFeature>()
                .HasMany(e => e.SysFeature1)
                .WithOptional(e => e.SysFeature2)
                .HasForeignKey(e => e.ParentFeatureId);

            modelBuilder.Entity<SysGauge>()
                .HasMany(e => e.SysGaugeModule)
                .WithRequired(e => e.SysGauge)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysHelp>()
                .HasMany(e => e.SysHelp1)
                .WithMany(e => e.SysHelp2)
                .Map(m => m.ToTable("SysHelpMapping").MapLeftKey("SourceSysHelpId").MapRightKey("TargetSysHelpId"));

            modelBuilder.Entity<SysImportDefinition>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportDefinition>()
                .Property(e => e.Separator)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportDefinition>()
                .Property(e => e.XmlTagHead)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportDefinition>()
                .Property(e => e.SpecialFunctionality)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportDefinition>()
                .HasMany(e => e.SysImportDefinitionLevel)
                .WithRequired(e => e.SysImportDefinition)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysImportDefinitionLevel>()
                .Property(e => e.Xml)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportHead>()
                .HasMany(e => e.SysImportRelation)
                .WithRequired(e => e.SysImportHead)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysImportHead>()
                .HasMany(e => e.SysImportSelect)
                .WithRequired(e => e.SysImportHead)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysImportRelation>()
                .Property(e => e.TableParent)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportRelation>()
                .Property(e => e.TableChild)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportSelect>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportSelect>()
                .Property(e => e.Select)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportSelect>()
                .Property(e => e.Where)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportSelect>()
                .Property(e => e.GroupBy)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportSelect>()
                .Property(e => e.OrderBy)
                .IsUnicode(false);

            modelBuilder.Entity<SysImportSelect>()
                .Property(e => e.Settings)
                .IsUnicode(false);

            modelBuilder.Entity<SysInformation>()
                .Property(e => e.TextHash)
                .IsFixedLength();

            modelBuilder.Entity<SysInformation>()
                .HasMany(e => e.SysInformationFeature)
                .WithRequired(e => e.SysInformation)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysInformation>()
                .HasMany(e => e.SysInformationSysCompDb)
                .WithRequired(e => e.SysInformation)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysJob>()
                .HasMany(e => e.SysScheduledJob)
                .WithRequired(e => e.SysJob)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysJobSetting>()
                .Property(e => e.DecimalData)
                .HasPrecision(8, 5);

            modelBuilder.Entity<SysLanguage>()
                .HasMany(e => e.SysHelp)
                .WithRequired(e => e.SysLanguage)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysLanguage>()
                .HasMany(e => e.SysInformation)
                .WithRequired(e => e.SysLanguage)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysLanguage>()
                .HasMany(e => e.SysNews)
                .WithRequired(e => e.SysLanguage)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysLanguage>()
                .HasMany(e => e.SysPosition)
                .WithRequired(e => e.SysLanguage)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysLbError>()
                .Property(e => e.LbErrorCode)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<SysParameterType>()
                .HasOptional(e => e.SysParameterType1)
                .WithRequired(e => e.SysParameterType2);

            modelBuilder.Entity<SysPayrollPrice>()
                .Property(e => e.Amount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SysPayrollPrice>()
                .HasMany(e => e.SysPayrollPriceInterval)
                .WithRequired(e => e.SysPayrollPrice)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysPayrollPriceInterval>()
                .Property(e => e.FromInterval)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SysPayrollPriceInterval>()
                .Property(e => e.ToInterval)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SysPayrollPriceInterval>()
                .Property(e => e.Amount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SysPriceList>()
                .Property(e => e.GNP)
                .HasPrecision(12, 4);
            modelBuilder.Entity<SysPriceList>()
                .Property(e => e.NetPrice)
                .HasPrecision(12, 4);
            modelBuilder.Entity<SysPriceList>()
                .Property(e => e.SalesPrice)
                .HasPrecision(12, 4);

            modelBuilder.Entity<SysPriceList>()
                .Property(e => e.PackageSizeMin)
                .HasPrecision(20, 6);

            modelBuilder.Entity<SysPriceList>()
                .Property(e => e.PackageSize)
                .HasPrecision(20, 6);

            modelBuilder.Entity<SysPriceListHead>()
                .HasMany(e => e.SysWholeseller)
                .WithMany(e => e.SysPriceListHead)
                .Map(m => m.ToTable("SysPriceListHeadWholeSellerMapping").MapLeftKey("SysPriceListHeadId").MapRightKey("SysWholeSellerId"));

            modelBuilder.Entity<SysPriceListHead>()
                .HasMany(e => e.SysPriceLists)
                .WithRequired(e => e.SysPriceListHead)
                .HasForeignKey(e => e.SysPriceListHeadId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysPriceListTempHead>()
                .HasMany(e => e.SysPriceListTempItem)
                .WithRequired(e => e.SysPriceListTempHead)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysPriceListTempItem>()
                .Property(e => e.GNP)
                .HasPrecision(12, 4);
            modelBuilder.Entity<SysPriceListTempItem>()
                .Property(e => e.SalesPrice)
                .HasPrecision(12, 4);
            modelBuilder.Entity<SysPriceListTempItem>()
                .Property(e => e.NetPrice)
                .HasPrecision(12, 4);

            modelBuilder.Entity<SysPriceListTempItem>()
                .Property(e => e.PackageSizeMin)
                .HasPrecision(20, 6);

            modelBuilder.Entity<SysPriceListTempItem>()
                .Property(e => e.PackageSize)
                .HasPrecision(20, 6);

            modelBuilder.Entity<SysReportType>()
                .HasMany(e => e.SysReportTemplate)
                .WithRequired(e => e.SysReportType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysReportTemplate>()
                .HasMany(e => e.SysReportGroups)
                .WithMany(e => e.SysReportTemplates);

            modelBuilder.Entity<SysReportTemplate>()
                .HasMany(e => e.SysReportTemplateSettings)
                .WithRequired(e => e.SysReportTemplate)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysReportTemplateSetting>()
                .HasRequired(e => e.SysReportTemplate)
                .WithMany(e => e.SysReportTemplateSettings)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysReportGroup>()
                .HasMany(e => e.SysReportHeaders)
                .WithMany(e => e.SysReportGroups);

            modelBuilder.Entity<SysScheduledJob>()
                .HasMany(e => e.SysScheduledJobLog)
                .WithRequired(e => e.SysScheduledJob)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysServer>()
                .HasMany(e => e.SysServerLogin)
                .WithRequired(e => e.SysServer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysServerLogin>()
                .Property(e => e.passwordhash)
                .IsFixedLength();

            modelBuilder.Entity<SysSettingType>()
                .HasMany(e => e.SysSetting)
                .WithRequired(e => e.SysSettingType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysVatAccount>()
                .HasOptional(e => e.SysVatRate)
                .WithRequired(e => e.SysVatAccount);

            modelBuilder.Entity<SysWholeseller>()
                .HasMany(e => e.SysEdiMessageHead)
                .WithRequired(e => e.SysWholeseller)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysWholeseller>()
                .HasMany(e => e.SysWholesellerSetting)
                .WithRequired(e => e.SysWholeseller)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysWholesellerEdi>()
                .Property(e => e.SenderId)
                .IsUnicode(false);

            modelBuilder.Entity<SysWholesellerEdi>()
                .Property(e => e.SenderName)
                .IsUnicode(false);

            modelBuilder.Entity<SysWholesellerEdi>()
                .Property(e => e.EdiFolder)
                .IsUnicode(false);

            modelBuilder.Entity<SysWholesellerEdi>()
                .Property(e => e.FtpUser)
                .IsUnicode(false);

            modelBuilder.Entity<SysWholesellerEdi>()
                .Property(e => e.FtpPassword)
                .IsUnicode(false);

            modelBuilder.Entity<SysWholesellerEdi>()
                .HasMany(e => e.SysEdiMsg)
                .WithRequired(e => e.SysWholesellerEdi)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysWholesellerSetting>()
                .Property(e => e.DecimalValue)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SysXEArticle>()
                .Property(e => e.StartPrice)
                .HasPrecision(7, 2);

            modelBuilder.Entity<SysXEArticle>()
                .Property(e => e.MonthlyPrice)
                .HasPrecision(7, 2);

            modelBuilder.Entity<SysXEArticle>()
                .HasMany(e => e.SysXEArticleFeature)
                .WithRequired(e => e.SysXEArticle)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SysPayrollPriceView>()
                .Property(e => e.Amount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SysPayrollPriceView>()
                .Property(e => e.FromInterval)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SysPayrollPriceView>()
                .Property(e => e.ToInterval)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SysPayrollPriceView>()
                .Property(e => e.IntervalAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SysProductPriceSearchView>()
                .Property(e => e.GNP)
                .HasPrecision(12, 4);
            modelBuilder.Entity<SysProductPriceSearchView>()
                .Property(e => e.SalesPrice)
                .HasPrecision(12, 4);
            modelBuilder.Entity<SysProductPriceSearchView>()
                .Property(e => e.NetPrice)
                .HasPrecision(12, 4);


        }
    }
}
