using EntityFramework.Extensions;
using log4net;
using SoftOne.Soe.Business.Core.Logger;
using SoftOne.Soe.Business.Core.PaymentIO.BgMax;
using SoftOne.Soe.Business.Core.PaymentIO.Cfp;
using SoftOne.Soe.Business.Core.PaymentIO.Lb;
using SoftOne.Soe.Business.Core.PaymentIO.Nets;
using SoftOne.Soe.Business.Core.PaymentIO.Pg;
using SoftOne.Soe.Business.Core.PaymentIO.SEPA;
using SoftOne.Soe.Business.Core.PaymentIO.SEPAV3;
using SoftOne.Soe.Business.Core.PaymentIO.SOP;
using SoftOne.Soe.Business.Core.PaymentIO.TotalIn;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Core.Reporting.Matrix;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Z.BulkOperations;

namespace SoftOne.Soe.Business.Core
{
	public abstract partial class ManagerBase
	{
		#region Constants

		protected const string THREAD = "WCF";

		#endregion

		#region Variables

		Guid logCorrelationId = Guid.NewGuid();

		// Create a logger for use in this class
		private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// Managers
		private SysLogManager slm;

		// Parameters
		protected ParameterObject parameterObject { get; set; }
		public int ActorCompanyId
		{
			get
			{
				return this.parameterObject?.ActorCompanyId ?? 0;
			}
		}
		public int LicenseId
		{
			get
			{
				return this.parameterObject?.LicenseId ?? 0;
			}
		}
		public int UserId
		{
			get
			{
				return this.parameterObject?.UserId ?? 0;
			}
		}
		public int RoleId
		{
			get
			{
				return this.parameterObject?.RoleId ?? 0;
			}
		}
		public int? SupportActorCompanyId
		{
			get
			{
				return this.parameterObject?.SupportActorCompanyId;
			}
		}
		public int? SupportUserId
		{
			get
			{
				return this.parameterObject?.SupportUserId;
			}
		}
		public int? SupportDefaultRoleId
		{
			get
			{
				return this.parameterObject?.SupportActiveRoleId ?? 0;
			}
		}
		public string LoginName
		{
			get
			{
				return this.parameterObject?.LoginName ?? string.Empty;
			}
		}
		public string FullName
		{
			get
			{
				return this.parameterObject?.FullName ?? string.Empty;
			}
		}
		protected bool IncludeInactiveAccounts
		{
			get
			{
				return this.parameterObject?.IncludeInactiveAccounts ?? false;
			}
		}
		protected void DoInlcudeInactiveAccounts()
		{
			//Only mathem for now..
			if (this.parameterObject != null && this.ActorCompanyId == 701609)
				this.parameterObject.SetIncludeInactiveAccounts(true);
		}

		//ObjectContexts
		// Previously know as SysEntities;
		//public SOESysEntities SysEntitiesReadOnly
		//{
		//	get
		//	{
		//		return SysEntitiesProvider.LeaseReadOnlyContext();
		//	}
		//}

		// Previously know as CompEntities;
		// Now replaced with factory method in Comp class
		//public CompEntities CompEntitiesReadOnly
		//{
		//    get
		//    {
		//        return Comp.CreateCompEntities(true);
		//    }
		//}

		//Task
		protected SoeTimeEngineTask currentTask;
		protected long? currentTaskWatchLogId;

		//Delegates
		protected delegate List<T> DelGetDataInBatches<T>(GetDataInBatchesModel model);

		#endregion

		#region Manager lazy loader methods

		private AccountBalanceManager accountBalanceManager;
		protected AccountBalanceManager AccountBalanceManager(int actorCompanyId)
		{
			if (accountBalanceManager == null || !accountBalanceManager.InstanceIsValid(actorCompanyId))
				accountBalanceManager = new AccountBalanceManager(parameterObject, actorCompanyId);

			return accountBalanceManager;
		}

		private TimeEngineManager timeEngineManager;
		protected TimeEngineManager TimeEngineManager(int actorCompanyId, int userId)
		{
			if (timeEngineManager == null || !timeEngineManager.IsValid(actorCompanyId, userId))
				timeEngineManager = new TimeEngineManager(parameterObject, actorCompanyId, userId);

			return timeEngineManager;
		}

		protected TimeEngineManager TimeEngineManager(int actorCompanyId, int userId, CompEntities entities)
		{
			if (timeEngineManager == null || !timeEngineManager.IsValid(actorCompanyId, userId))
				timeEngineManager = new TimeEngineManager(parameterObject, actorCompanyId, userId, entities);

			return timeEngineManager;
		}

		#endregion

		#region Manager lazy loader properties

		private AccountDistributionManager accountDistributionManager;
		protected AccountDistributionManager AccountDistributionManager
		{
			get
			{
				return accountDistributionManager ?? (accountDistributionManager = new AccountDistributionManager(parameterObject));
			}
		}

		private AccountManager accountManager;
		protected AccountManager AccountManager
		{
			get
			{
				return accountManager ?? (accountManager = new AccountManager(parameterObject));
			}
		}

		private ActorManager actorManager;
		protected ActorManager ActorManager
		{
			get
			{
				return actorManager ?? (actorManager = new ActorManager(parameterObject));
			}
		}

		private AnalysisManager analysisManager;
		protected AnalysisManager AnalysisManager
		{
			get
			{
				return analysisManager ?? (analysisManager = new AnalysisManager(parameterObject));
			}
		}

		private AnnualLeaveManager annualLeaveManager;
		protected AnnualLeaveManager AnnualLeaveManager
		{
			get
			{
				return annualLeaveManager ?? (annualLeaveManager = new AnnualLeaveManager(parameterObject));
			}
		}

		private ApiManager apiManager;
		protected ApiManager ApiManager
		{
			get
			{
				return apiManager ?? (apiManager = new ApiManager(parameterObject));
			}
		}

		private ApiDataManager apiDataManager;
		protected ApiDataManager ApiDataManager
		{
			get
			{
				return apiDataManager ?? (apiDataManager = new ApiDataManager(parameterObject));
			}
		}

		private AttestManager attestManager;
		protected AttestManager AttestManager
		{
			get
			{
				return attestManager ?? (attestManager = new AttestManager(parameterObject));
			}
		}

		private BridgeManager bridgeManager;
		protected BridgeManager BridgeManager
		{
			get
			{
				return bridgeManager ?? (bridgeManager = new BridgeManager(parameterObject));
			}
		}

		private BudgetManager budgetManager;
		protected BudgetManager BudgetManager
		{
			get
			{
				return budgetManager ?? (budgetManager = new BudgetManager(parameterObject));
			}
		}

		private CalendarManager calendarManager;
		protected CalendarManager CalendarManager
		{
			get
			{
				return calendarManager ?? (calendarManager = new CalendarManager(parameterObject));
			}
		}

		private CategoryManager categoryManager;
		protected CategoryManager CategoryManager
		{
			get
			{
				return categoryManager ?? (categoryManager = new CategoryManager(parameterObject));
			}
		}

		private ChecklistManager checklistManager;
		protected ChecklistManager ChecklistManager
		{
			get
			{
				return checklistManager ?? (checklistManager = new ChecklistManager(parameterObject));
			}
		}

		private ClientManagementManager clientManagementManager;
		protected ClientManagementManager ClientManagementManager
		{
			get
			{
				return clientManagementManager ?? (clientManagementManager = new ClientManagementManager(parameterObject));
			}
		}

		private CommentManager commentManager;
		protected CommentManager CommentManager
		{
			get
			{
				return commentManager ?? (commentManager = new CommentManager(parameterObject));
			}
		}

		private CommunicationManager communicationManager;
		protected CommunicationManager CommunicationManager
		{
			get
			{
				return communicationManager ?? (communicationManager = new CommunicationManager(parameterObject));
			}
		}

		private CompanyManager companyManager;
		protected CompanyManager CompanyManager
		{
			get
			{
				return companyManager ?? (companyManager = new CompanyManager(parameterObject));
			}
		}

		private ContactManager contactManager;
		protected ContactManager ContactManager
		{
			get
			{
				return contactManager ?? (contactManager = new ContactManager(parameterObject));
			}
		}

		private ContractManager contractManager;
		protected ContractManager ContractManager
		{
			get
			{
				return contractManager ?? (contractManager = new ContractManager(parameterObject));
			}
		}

		private CountryCurrencyManager countryCurrencyManager;
		protected CountryCurrencyManager CountryCurrencyManager
		{
			get
			{
				return countryCurrencyManager ?? (countryCurrencyManager = new CountryCurrencyManager(parameterObject));
			}
		}

		private CustomerManager customerManager;
		protected CustomerManager CustomerManager
		{
			get
			{
				return customerManager ?? (customerManager = new CustomerManager(parameterObject));
			}
		}

		private DashboardManager dashboardManager;
		protected DashboardManager DashboardManager
		{
			get
			{
				return dashboardManager ?? (dashboardManager = new DashboardManager(parameterObject));
			}
		}


		private DepreciationManager depreciationManager;
		protected DepreciationManager DepreciationManager
		{
			get
			{
				return depreciationManager ?? (depreciationManager = new DepreciationManager(parameterObject));
			}
		}

		private EdiManager ediManager;
		protected EdiManager EdiManager
		{
			get
			{
				return ediManager ?? (ediManager = new EdiManager(parameterObject));
			}
		}

		private ExpenseManager expenseManager;
		protected ExpenseManager ExpenseManager
		{
			get
			{
				return expenseManager ?? (expenseManager = new ExpenseManager(parameterObject));
			}
		}

		private EmailManager emailManager;
		protected EmailManager EmailManager
		{
			get
			{
				return emailManager ?? (emailManager = new EmailManager(parameterObject));
			}
		}

		private EmployeeManager employeeManager;
		protected EmployeeManager EmployeeManager
		{
			get
			{
				return employeeManager ?? (employeeManager = new EmployeeManager(parameterObject));
			}
		}

		private ExcelImportManager excelImportManager;
		protected ExcelImportManager ExcelImportManager
		{
			get
			{
				return excelImportManager ?? (excelImportManager = new ExcelImportManager(parameterObject));
			}
		}

		private ExtraFieldManager extraFieldManager;
		protected ExtraFieldManager ExtraFieldManager
		{
			get
			{
				return extraFieldManager ?? (extraFieldManager = new ExtraFieldManager(parameterObject));
			}
		}

		private FeatureManager featureManager;
		protected FeatureManager FeatureManager
		{
			get
			{
				return featureManager ?? (featureManager = new FeatureManager(parameterObject));
			}
		}

		private FieldSettingManager fieldSettingManager;
		protected FieldSettingManager FieldSettingManager
		{
			get
			{
				return fieldSettingManager ?? (fieldSettingManager = new FieldSettingManager(parameterObject));
			}
		}

		private GraphicsManager graphicsManager;
		protected GraphicsManager GraphicsManager
		{
			get
			{
				return graphicsManager ?? (graphicsManager = new GraphicsManager(parameterObject));
			}
		}

		private GeneralManager generalManager;
		protected GeneralManager GeneralManager
		{
			get
			{
				return generalManager ?? (generalManager = new GeneralManager(parameterObject));
			}
		}

		private GoTimeStampManager goTimeStampManager;
		protected GoTimeStampManager GoTimeStampManager
		{
			get
			{
				return goTimeStampManager ?? (goTimeStampManager = new GoTimeStampManager(parameterObject));
			}
		}

		private GrossProfitManager grossProfitManager;
		protected GrossProfitManager GrossProfitManager
		{
			get
			{
				return grossProfitManager ?? (grossProfitManager = new GrossProfitManager(parameterObject));
			}
		}

		private HelpManager helpManager;
		protected HelpManager HelpManager
		{
			get
			{
				return helpManager ?? (helpManager = new HelpManager(parameterObject));
			}
		}

		private ImportExportManager importExportManager;
		protected ImportExportManager ImportExportManager
		{
			get
			{
				return importExportManager ?? (importExportManager = new ImportExportManager(parameterObject));
			}
		}

		private InventoryManager inventoryManager;
		protected InventoryManager InventoryManager
		{
			get
			{
				return inventoryManager ?? (inventoryManager = new InventoryManager(parameterObject));
			}
		}

		private InvoiceManager invoiceManager;
		protected InvoiceManager InvoiceManager
		{
			get
			{
				return invoiceManager ?? (invoiceManager = new InvoiceManager(parameterObject));
			}
		}

		private SupplierInvoiceManager supplierInvoiceManager;
		protected SupplierInvoiceManager SupplierInvoiceManager
		{
			get
			{
				return supplierInvoiceManager ?? (supplierInvoiceManager = new SupplierInvoiceManager(parameterObject));
			}
		}

		private InvoiceDistributionManager invoiceDistributionManager;
		protected InvoiceDistributionManager InvoiceDistributionManager
		{
			get
			{
				return invoiceDistributionManager ?? (invoiceDistributionManager = new InvoiceDistributionManager(parameterObject));
			}
		}

		private InvoiceAttachmentManager invoiceAttachmentManager;
		protected InvoiceAttachmentManager InvoiceAttachmentManager
		{
			get
			{
				return invoiceAttachmentManager ?? (invoiceAttachmentManager = new InvoiceAttachmentManager(parameterObject));
			}
		}

		private HouseholdTaxDeductionManager householdTaxDeductionManager;
		protected HouseholdTaxDeductionManager HouseholdTaxDeductionManager
		{
			get
			{
				return householdTaxDeductionManager ?? (householdTaxDeductionManager = new HouseholdTaxDeductionManager(parameterObject));
			}
		}

		private LanguageManager languageManager;
		protected LanguageManager LanguageManager
		{
			get
			{
				return languageManager ?? (languageManager = new LanguageManager(parameterObject));
			}
		}

		private LicenseManager licenseManager;
		protected LicenseManager LicenseManager
		{
			get
			{
				return licenseManager ?? (licenseManager = new LicenseManager(parameterObject));
			}
		}

		private LoginManager loginManager;
		protected LoginManager LoginManager
		{
			get
			{
				return loginManager ?? (loginManager = new LoginManager(parameterObject));
			}
		}

		private LogoManager logoManager;
		protected LogoManager LogoManager
		{
			get
			{
				return logoManager ?? (logoManager = new LogoManager(parameterObject));
			}
		}

		private MarkupManager markupManager;
		protected MarkupManager MarkupManager
		{
			get
			{
				return markupManager ?? (markupManager = new MarkupManager(parameterObject));
			}
		}

		private TimeMatrixDataManager timeMatrixDataManager;
		protected TimeMatrixDataManager TimeMatrixDataManager
		{
			get
			{
				return timeMatrixDataManager ?? (timeMatrixDataManager = new TimeMatrixDataManager(parameterObject));
			}
		}
		private EconomyMatrixDataManager economyMatrixDataManager;
		protected EconomyMatrixDataManager EconomyMatrixDataManager
		{
			get
			{
				return economyMatrixDataManager ?? (economyMatrixDataManager = new EconomyMatrixDataManager(parameterObject));
			}
		}

		private BillingMatrixDataManager billingMatrixDataManager;
		protected BillingMatrixDataManager BillingMatrixDataManager
		{
			get
			{
				return billingMatrixDataManager ?? (billingMatrixDataManager = new BillingMatrixDataManager(parameterObject));
			}
		}

		private ManageMatrixDataManager manageMatrixDataManager;
		protected ManageMatrixDataManager ManageMatrixDataManager
		{
			get
			{
				return manageMatrixDataManager ?? (manageMatrixDataManager = new ManageMatrixDataManager(parameterObject));
			}
		}

		private OriginManager originManager;
		protected OriginManager OriginManager
		{
			get
			{
				return originManager ?? (originManager = new OriginManager(parameterObject));
			}
		}

		private PayrollManager payrollManager;
		protected PayrollManager PayrollManager
		{
			get
			{
				return payrollManager ?? (payrollManager = new PayrollManager(parameterObject));
			}
		}

		#region PaymentIO

		private PaymentIOManager paymentIOManager;
		protected PaymentIOManager PaymentIOManager
		{
			get
			{
				return paymentIOManager ?? (paymentIOManager = new PaymentIOManager(parameterObject));
			}
		}

		private BgMaxManager bgMaxManager;
		protected BgMaxManager BgMaxManager
		{
			get
			{
				return bgMaxManager ?? (bgMaxManager = new BgMaxManager(parameterObject));
			}
		}

		private TotalInManager totalInManager;
		protected TotalInManager TotalInManager
		{
			get
			{
				return totalInManager ?? (totalInManager = new TotalInManager(parameterObject));
			}
		}

		private LbManager lbManager;
		protected LbManager LbManager
		{
			get
			{
				return lbManager ?? (lbManager = new LbManager(parameterObject));
			}
		}

		private PgManager pgManager;
		protected PgManager PgManager
		{
			get
			{
				return pgManager ?? (pgManager = new PgManager(parameterObject));
			}
		}

		private SEPAManager sepaManager;
		protected SEPAManager SEPAManager
		{
			get
			{
				return sepaManager ?? (sepaManager = new SEPAManager(parameterObject));
			}
		}

		private SEPAV3Manager sepaV3Manager;
		protected SEPAV3Manager SEPAV3Manager
		{
			get
			{
				return sepaV3Manager ?? (sepaV3Manager = new SEPAV3Manager(parameterObject));
			}
		}

		private NetsManager netsManager;
		protected NetsManager NetsManager
		{
			get
			{
				return netsManager ?? (netsManager = new NetsManager(parameterObject));
			}
		}
		private SOPManager sopManager;
		protected SOPManager SOPManager
		{
			get
			{
				return sopManager ?? (sopManager = new SOPManager(parameterObject));
			}
		}
		private CfpManager cfpManager;
		protected CfpManager CfpManager
		{
			get
			{
				return cfpManager ?? (cfpManager = new CfpManager(parameterObject));
			}
		}
		#endregion

		private PaymentManager paymentManager;
		protected PaymentManager PaymentManager
		{
			get
			{
				return paymentManager ?? (paymentManager = new PaymentManager(parameterObject));
			}
		}

		private PriceRuleManager priceRuleManager;
		protected PriceRuleManager PriceRuleManager
		{
			get
			{
				return priceRuleManager ?? (priceRuleManager = new PriceRuleManager(parameterObject));
			}
		}
		private WholsellerNetPriceManager wholsellerNetPriceManager;
		protected WholsellerNetPriceManager WholsellerNetPriceManager
		{
			get
			{
				return wholsellerNetPriceManager ?? (wholsellerNetPriceManager = new WholsellerNetPriceManager(parameterObject));
			}
		}

		private ProductManager productManager;
		protected ProductManager ProductManager
		{
			get
			{
				return productManager ?? (productManager = new ProductManager(parameterObject));
			}
		}

		private ProductPricelistManager productPricelistManager;
		protected ProductPricelistManager ProductPricelistManager
		{
			get
			{
				return productPricelistManager ?? (productPricelistManager = new ProductPricelistManager(parameterObject));
			}
		}

		private ProductGroupManager productGroupManager;
		protected ProductGroupManager ProductGroupManager
		{
			get
			{
				return productGroupManager ?? (productGroupManager = new ProductGroupManager(parameterObject));
			}
		}

		private ProjectManager projectManager;
		protected ProjectManager ProjectManager
		{
			get
			{
				return projectManager ?? (projectManager = new ProjectManager(parameterObject));
			}
		}

		private ProjectBudgetManager projectBudgetManager;
		protected ProjectBudgetManager ProjectBudgetManager
		{
			get
			{
				return projectBudgetManager ?? (projectBudgetManager = new ProjectBudgetManager(parameterObject));
			}
		}

		private ReportDataManager reportDataManager;
		protected ReportDataManager ReportDataManager
		{
			get
			{
				return reportDataManager ?? (reportDataManager = new ReportDataManager(parameterObject));
			}
		}

		private TimeReportDataManager timeReportDataManager;
		protected TimeReportDataManager TimeReportDataManager
		{
			get
			{
				return timeReportDataManager ?? (timeReportDataManager = new TimeReportDataManager(parameterObject));
			}
		}

		private ReportGenManager reportGenManager;
		protected ReportGenManager ReportGenManager
		{
			get
			{
				return reportGenManager ?? (reportGenManager = new ReportGenManager(parameterObject));
			}
		}

		private ReportManager reportManager;
		protected ReportManager ReportManager
		{
			get
			{
				return reportManager ?? (reportManager = new ReportManager(parameterObject));
			}
		}

		private RoleManager roleManager;
		protected RoleManager RoleManager
		{
			get
			{
				return roleManager ?? (roleManager = new RoleManager(parameterObject));
			}
		}

		private ScheduledJobManager scheduledJobManager;
		protected ScheduledJobManager ScheduledJobManager
		{
			get
			{
				return scheduledJobManager ?? (scheduledJobManager = new ScheduledJobManager(parameterObject));
			}
		}

		private SequenceNumberManager sequenceNumberManager;
		protected SequenceNumberManager SequenceNumberManager
		{
			get
			{
				return sequenceNumberManager ?? (sequenceNumberManager = new SequenceNumberManager(parameterObject));
			}
		}

		private SettingManager settingManager;
		protected SettingManager SettingManager
		{
			get
			{
				return settingManager ?? (settingManager = new SettingManager(parameterObject));
			}
		}

		private StockManager stockManager;
		protected StockManager StockManager
		{
			get
			{
				return stockManager ?? (stockManager = new StockManager(parameterObject));
			}
		}

		private SupplierAgreementManager supplierAgreementManager;
		protected SupplierAgreementManager SupplierAgreementManager
		{
			get
			{
				return supplierAgreementManager ?? (supplierAgreementManager = new SupplierAgreementManager(parameterObject));
			}
		}

		private SupplierManager supplierManager;
		protected SupplierManager SupplierManager
		{
			get
			{
				return supplierManager ?? (supplierManager = new SupplierManager(parameterObject));
			}
		}

		private SupplierProductManager supplierProductManager;
		protected SupplierProductManager SupplierProductManager
		{
			get
			{
				return supplierProductManager ?? (supplierProductManager = new SupplierProductManager(parameterObject));
			}
		}

		private LoggerManager loggerManager;
		protected LoggerManager LoggerManager
		{
			get
			{
				return loggerManager ?? (loggerManager = new LoggerManager(parameterObject));
			}
		}

		private SignatoryContractManager signatoryContractManager;
		protected SignatoryContractManager SignatoryContractManager
		{
			get
			{
				return signatoryContractManager ?? (signatoryContractManager = new SignatoryContractManager(parameterObject));
			}
		}

		private SysLogManager sysLogManager;
		protected SysLogManager SysLogManager
		{
			get
			{
				return sysLogManager ?? (sysLogManager = new SysLogManager(parameterObject));
			}
		}

		private SysNewsManager sysNewsManager;
		protected SysNewsManager SysNewsManager
		{
			get
			{
				return sysNewsManager ?? (sysNewsManager = new SysNewsManager(parameterObject));
			}
		}

		private SysPriceListManager sysPriceListManager;
		protected SysPriceListManager SysPriceListManager
		{
			get
			{
				return sysPriceListManager ?? (sysPriceListManager = new SysPriceListManager(parameterObject));
			}
		}

		private SysScheduledJobManager sysScheduledJobManager;
		protected SysScheduledJobManager SysScheduledJobManager
		{
			get
			{
				return sysScheduledJobManager ?? (sysScheduledJobManager = new SysScheduledJobManager(parameterObject));
			}
		}

		private SysServiceManager sysServiceManager;
		protected SysServiceManager SysServiceManager
		{
			get
			{
				return sysServiceManager ?? (sysServiceManager = new SysServiceManager(parameterObject));
			}
		}

		private TermManager termManager;
		protected TermManager TermManager
		{
			get
			{
				return termManager ?? (termManager = new TermManager(parameterObject));
			}
		}

		private TimeAccumulatorManager timeAccumulatorManager;
		protected TimeAccumulatorManager TimeAccumulatorManager
		{
			get
			{
				return timeAccumulatorManager ?? (timeAccumulatorManager = new TimeAccumulatorManager(parameterObject));
			}
		}

		private TimeBlockManager timeBlockManager;
		protected TimeBlockManager TimeBlockManager
		{
			get
			{
				return timeBlockManager ?? (timeBlockManager = new TimeBlockManager(parameterObject));
			}
		}

		private TimeCodeManager timeCodeManager;
		protected TimeCodeManager TimeCodeManager
		{
			get
			{
				return timeCodeManager ?? (timeCodeManager = new TimeCodeManager(parameterObject));
			}
		}

		private TimeDeviationCauseManager timeDeviationCauseManager;
		protected TimeDeviationCauseManager TimeDeviationCauseManager
		{
			get
			{
				return timeDeviationCauseManager ?? (timeDeviationCauseManager = new TimeDeviationCauseManager(parameterObject));
			}
		}

		private TimeHibernatingManager timeHibernatingManager;
		protected TimeHibernatingManager TimeHibernatingManager
		{
			get
			{
				return timeHibernatingManager ?? (timeHibernatingManager = new TimeHibernatingManager(parameterObject));
			}
		}

		private TimePeriodManager timePeriodManager;
		protected TimePeriodManager TimePeriodManager
		{
			get
			{
				return timePeriodManager ?? (timePeriodManager = new TimePeriodManager(parameterObject));
			}
		}

		private TimeRuleManager timeRuleManager;
		protected TimeRuleManager TimeRuleManager
		{
			get
			{
				return timeRuleManager ?? (timeRuleManager = new TimeRuleManager(parameterObject));
			}
		}

		private TimeSalaryManager timeSalaryManager;
		protected TimeSalaryManager TimeSalaryManager
		{
			get
			{
				return timeSalaryManager ?? (timeSalaryManager = new TimeSalaryManager(parameterObject));
			}
		}

		private TimeScheduleManager timeScheduleManager;
		protected TimeScheduleManager TimeScheduleManager
		{
			get
			{
				return timeScheduleManager ?? (timeScheduleManager = new TimeScheduleManager(parameterObject));
			}
		}

		private TimeStampManager timeStampManager;
		protected TimeStampManager TimeStampManager
		{
			get
			{
				return timeStampManager ?? (timeStampManager = new TimeStampManager(parameterObject));
			}
		}

		private TimeTransactionManager timeTransactionManager;
		protected TimeTransactionManager TimeTransactionManager
		{
			get
			{
				return timeTransactionManager ?? (timeTransactionManager = new TimeTransactionManager(parameterObject));
			}
		}

		private TimeTreeAttestManager timeTreeAttestManager;
		protected TimeTreeAttestManager TimeTreeAttestManager
		{
			get
			{
				return timeTreeAttestManager ?? (timeTreeAttestManager = new TimeTreeAttestManager(parameterObject));
			}
		}

		private TimeTreePayrollManager timeTreePayrollManager;
		protected TimeTreePayrollManager TimeTreePayrollManager
		{
			get
			{
				return timeTreePayrollManager ?? (timeTreePayrollManager = new TimeTreePayrollManager(parameterObject));
			}
		}

		private TimeWorkAccountManager timeWorkAccountManager;
		protected TimeWorkAccountManager TimeWorkAccountManager
		{
			get
			{
				return timeWorkAccountManager ?? (timeWorkAccountManager = new TimeWorkAccountManager(parameterObject));
			}
		}

		private TimeWorkReductionManager timeWorkReductionManager;
		protected TimeWorkReductionManager TimeWorkReductionManager
		{
			get
			{
				return timeWorkReductionManager ?? (timeWorkReductionManager = new TimeWorkReductionManager(parameterObject));
			}
		}

		private TrackChangesManager trackChangesManager;
		protected TrackChangesManager TrackChangesManager
		{
			get
			{
				return trackChangesManager ?? (trackChangesManager = new TrackChangesManager(parameterObject));
			}
		}

		private UserManager userManager;
		protected UserManager UserManager
		{
			get
			{
				return userManager ?? (userManager = new UserManager(parameterObject));
			}
		}

		private VoucherManager voucherManager;
		protected VoucherManager VoucherManager
		{
			get
			{
				return voucherManager ?? (voucherManager = new VoucherManager(parameterObject));
			}
		}

		private WholeSellerManager wholeSellerManager;
		protected WholeSellerManager WholeSellerManager
		{
			get
			{
				return wholeSellerManager ?? (wholeSellerManager = new WholeSellerManager(parameterObject));
			}
		}

		private PriceOptimizationManager priceOptimizationManager;
		protected PriceOptimizationManager PriceOptimizationManager
		{
			get
			{
				return priceOptimizationManager ?? (priceOptimizationManager = new PriceOptimizationManager(parameterObject));
			}
		}

		private WtConvertManager wtConvertManager;
		protected WtConvertManager WtConvertManager
		{
			get
			{
				return wtConvertManager ?? (wtConvertManager = new WtConvertManager(parameterObject));
			}
		}

		private CommodityCodeManager commodityCodeManager;

		protected CommodityCodeManager CommodityCodeManager
		{
			get
			{
				return commodityCodeManager ?? (commodityCodeManager = new CommodityCodeManager(parameterObject));
			}
		}

		#endregion

		#region Ctor

		protected ManagerBase(ParameterObject parameterObject)
		{
			this.parameterObject = parameterObject;
		}

		#endregion

		#region CRUD

		//ObjectContext
		protected ActionResult AddEntityItem(ObjectContext context, EntityObject entity, string entitySetName, TransactionScope transaction = null, bool addToContext = true, bool useBulkSaveChanges = true)
		{
			return AddEntityItem(context, entity, entitySetName, transaction, out _, addToContext, useBulkSaveChanges);
		}
		protected ActionResult AddEntityItem(ObjectContext context, EntityObject entity, string entitySetName, TransactionScope transaction, out int? sysLogId, bool addToContext = true, bool useBulkSaveChanges = true)
		{
			var result = new ActionResult();
			sysLogId = null;

			try
			{

				SetCreatedProperties(entity);
				if (addToContext)
					context.AddObject(entitySetName, entity);

				result.ObjectsAffected = 1;
				if (useBulkSaveChanges)
					context.BulkSaveChanges(useEntityFrameworkPropagation: true);
				else
					result.ObjectsAffected = context.SaveChanges();

				if (result.ObjectsAffected == 0)
				{
					result.Success = true;
					result.ErrorNumber = (int)ActionResultSave.NothingSaved;
				}
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out sysLogId);
			}

			return result;
		}
		protected ActionResult UpdateEntityItem(ObjectContext context, EntityObject originalItem, EntityObject updatedItem, string entitySetName, TransactionScope transaction = null)
		{
			return UpdateEntityItem(context, originalItem, updatedItem, entitySetName, transaction, out _);
		}
		protected ActionResult UpdateEntityItem(ObjectContext context, EntityObject originalItem, EntityObject currentEntity, string entitySetName, TransactionScope transaction, out int? sysLogId)
		{
			ActionResult result = new ActionResult();
			sysLogId = null;

			try
			{
				SetModifiedProperties(currentEntity);

				//Get the entity parameters of the updated object.
				EntityKey key = context.CreateEntityKey(entitySetName, originalItem);

				//Preserve Created and CreatedByobject 
				object created = originalItem.GetEntityProperty("Created");
				object createdBy = originalItem.GetEntityProperty("CreatedBy");

				//Attach the original item to the object context if it is not already attached, or if the entry is for the relationship and not the object itself.
				if (!context.ObjectStateManager.TryGetObjectStateEntry(key, out ObjectStateEntry entry) || (entry.Entity == null))
					context.Attach(originalItem);

				//Call the ApplyCurrentValues method to apply changes from the updated item to the original version
				context.ApplyCurrentValues(key.EntitySetName, currentEntity);

				//Reset Created and CreatedBy
				if (created != null)
					originalItem.TrySetEntityProperty<EntityObject>("Created", created);
				if (createdBy != null)
					originalItem.TrySetEntityProperty<EntityObject>("CreatedBy", createdBy);

				result.ObjectsAffected = context.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
				if (result.ObjectsAffected == 0)
				{
					result.Success = true;
					result.ErrorNumber = (int)ActionResultSave.NothingSaved;
				}
			}
			catch (OptimisticConcurrencyException ocEx)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ocEx, this.log);
				this.SetCrudException(result, ocEx, out sysLogId);
				this.HandleOptimisticConcurrency(result, ocEx, context, originalItem);
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out sysLogId);
			}

			return result;
		}
		protected ActionResult DeleteEntityItem(ObjectContext context, EntityObject entity, TransactionScope transaction = null, bool useBulkSaveChanges = true)
		{
			return DeleteEntityItem(context, entity, transaction, out _, useBulkSaveChanges);
		}
		protected ActionResult DeleteEntityItem(ObjectContext context, EntityObject entity, TransactionScope transaction, out int? sysLogId, bool useBulkSaveChanges = true)
		{
			ActionResult result = new ActionResult();
			sysLogId = null;

			try
			{
				context.DeleteObject(entity);
				result.ObjectsAffected = 1;
				if (useBulkSaveChanges)
					context.BulkSaveChanges(useEntityFrameworkPropagation: true);
				else
					result.ObjectsAffected = context.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);

				if (result.ObjectsAffected == 0)
				{
					result.Success = false;
					result.ErrorNumber = (int)ActionResultDelete.NothingDeleted;
				}
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out sysLogId);

				// sql error number 547 means that it conflicted with an constraint
				const int CONSTRAINT_CONFLICT = 547;
				// The %ls statement conflicted with the %ls constraint "%.*ls". The conflict occurred in database "%.*ls", table "%.*ls"%ls%.*ls%ls
				var isUsedEx = ex.GetInnerExceptions<System.Data.SqlClient.SqlException>().FirstOrDefault(e => e.Number == CONSTRAINT_CONFLICT);
				if (isUsedEx != null)
				{
					result.ErrorNumber = (int)ActionResultDelete.EntityInUse;
					result.ErrorMessage = isUsedEx.Message;
				}
			}

			return result;
		}
		protected ActionResult SaveEntityItem(ObjectContext context, EntityObject entity, TransactionScope transaction = null)
		{
			return SaveEntityItem(context, entity, transaction, out _);
		}
		protected ActionResult SaveEntityItem(ObjectContext context, EntityObject entity, TransactionScope transaction, out int? sysLogId)
		{
			ActionResult result = new ActionResult();
			sysLogId = null;

			try
			{
				SetModifiedProperties(entity);
				result = SaveChanges(context, transaction);
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out sysLogId);
			}

			return result;
		}
		public ActionResult SaveChanges(ObjectContext context, TransactionScope transaction = null, bool useEntityFrameworkPropagation = true, bool useBulkSaveChanges = false)
		{
			ActionResult result = new ActionResult();
			if (context == null)
				return result;

			try
			{
				result.ObjectsAffected = 1;
				if (useBulkSaveChanges)
				{
					BulkOperation bulkOperation = new BulkOperation();
					bulkOperation.BatchTimeout = context.CommandTimeout ?? 120;
					bulkOperation.AllowConcurrency = true;
					context.BulkSaveChanges(useEntityFrameworkPropagation, operation =>
					{
						operation.BatchTimeout = context.CommandTimeout ?? 120;
						operation.AllowConcurrency = true;
					});
				}
				else
					result.ObjectsAffected = context.SaveChanges();

				if (result.ObjectsAffected == 0)
				{
					result.Success = true;
					result.ErrorNumber = (int)ActionResultSave.NothingSaved;
				}
			}
			catch (OptimisticConcurrencyException ocEx)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ocEx, this.log);
				this.SetCrudException(result, ocEx, out _);
				this.HandleOptimisticConcurrency(result, ocEx);
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out _);
			}

			return result;
		}
		protected ActionResult SaveChangesWithTransaction(ObjectContext context)
		{
			ActionResult result = new ActionResult();

			try
			{
				context.Connection.Open();

				using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
				{
					result.ObjectsAffected = 1;
					context.BulkSaveChanges(useEntityFrameworkPropagation: true);
					if (result.ObjectsAffected == 0)
					{
						result.Success = true;
						result.ErrorNumber = (int)ActionResultSave.NothingSaved;
					}
					else
						transaction.Complete();
				}
			}
			catch (Exception ex)
			{
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out _);
			}
			finally
			{
				context.Connection.Close();
			}

			return result;
		}
		protected ActionResult SaveDeletions(ObjectContext context, TransactionScope transaction = null)
		{
			ActionResult result = new ActionResult();

			try
			{

				result.ObjectsAffected = 1;
				context.BulkSaveChanges(useEntityFrameworkPropagation: true);
				if (result.ObjectsAffected == 0)
				{
					result.Success = true;
					result.ErrorNumber = (int)ActionResultDelete.NothingDeleted;
				}
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out _);
			}

			return result;
		}
		protected ActionResult BulkInsert(ObjectContext context, IEnumerable<Object> objects, TransactionScope transaction = null)
		{
			ActionResult result = new ActionResult();

			try
			{
				result.ObjectsAffected = 1;
				context.BulkInsert(objects, operation => operation.BatchTimeout = context.CommandTimeout ?? 120);


			}
			catch (OptimisticConcurrencyException ocEx)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ocEx, this.log);
				this.SetCrudException(result, ocEx, out _);
				this.HandleOptimisticConcurrency(result, ocEx);
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out _);
			}

			return result;
		}
		protected ActionResult BulkUpdate(ObjectContext context, IEnumerable<Object> objects, TransactionScope transaction = null)
		{
			ActionResult result = new ActionResult();

			try
			{
				result.ObjectsAffected = 1;
				context.BulkUpdate(objects, operation => operation.BatchTimeout = context.CommandTimeout ?? 120);
			}
			catch (OptimisticConcurrencyException ocEx)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ocEx, this.log);
				this.SetCrudException(result, ocEx, out _);
				this.HandleOptimisticConcurrency(result, ocEx);
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out _);
			}

			return result;
		}
		protected ActionResult BulkUpdateChanges<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, TEntity>> updateExpression) where TEntity : class
		{
			ActionResult result = new ActionResult();

			try
			{
				result.ObjectsAffected = query.Update(updateExpression);
				if (result.ObjectsAffected == 0)
				{
					result.Success = true;
					result.ErrorNumber = (int)ActionResultDelete.NothingDeleted;
				}
			}
			catch (Exception ex)
			{
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out _);
			}

			return result;
		}

		//DbContext
		protected ActionResult SaveChanges(DbContext context, TransactionScope transaction = null, bool useEntityFrameworkPropagation = true, bool useBulkSaveChanges = false)
		{
			ActionResult result = new ActionResult();

			try
			{
				result.ObjectsAffected = 1;

				if (useBulkSaveChanges)
				{
					BulkOperation bulkOperation = new BulkOperation();
					bulkOperation.BatchTimeout = context.Database.CommandTimeout ?? 120;
					bulkOperation.AllowConcurrency = true;
					context.BulkSaveChanges(useEntityFrameworkPropagation, operation =>
					{
						operation.BatchTimeout = context.Database.CommandTimeout ?? 120;
						operation.AllowConcurrency = true;
					});
				}
				else
					result.ObjectsAffected = context.SaveChanges();

				if (result.ObjectsAffected == 0)
				{
					result.Success = true;
					result.ErrorNumber = (int)ActionResultSave.NothingSaved;
				}
			}
			catch (OptimisticConcurrencyException ocEx)
			{
				if (transaction != null)
					transaction.Dispose();
				LogCollector.LogError(ocEx.ToString());
				this.SetCrudException(result, ocEx, out _);
				this.HandleOptimisticConcurrency(result, ocEx);
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				LogCollector.LogError(ex.ToString());
				this.SetCrudException(result, ex, out _);
			}

			return result;
		}
		protected ActionResult BulkInsert(DbContext context, IEnumerable<Object> objects, TransactionScope transaction = null)
		{
			ActionResult result = new ActionResult();

			try
			{
				result.ObjectsAffected = 1;
				context.BulkInsert(objects, operation => operation.BatchTimeout = context.Database.CommandTimeout ?? 120);
			}
			catch (OptimisticConcurrencyException ocEx)
			{
				if (transaction != null)
					transaction.Dispose();
				LogCollector.LogError(ocEx.ToString());
				this.SetCrudException(result, ocEx, out _);
				this.HandleOptimisticConcurrency(result, ocEx);
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				LogCollector.LogError(ex.ToString());
				this.SetCrudException(result, ex, out _);
			}

			return result;
		}
		protected ActionResult BulkUpdate(DbContext context, IEnumerable<Object> objects, TransactionScope transaction = null)
		{
			ActionResult result = new ActionResult();

			try
			{
				result.ObjectsAffected = 1;
				context.BulkUpdate(objects, operation => operation.BatchTimeout = context.Database.CommandTimeout ?? 120);
			}
			catch (OptimisticConcurrencyException ocEx)
			{
				if (transaction != null)
					transaction.Dispose();
				LogCollector.LogError(ocEx.ToString());
				this.SetCrudException(result, ocEx, out _);
				this.HandleOptimisticConcurrency(result, ocEx);
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				LogCollector.LogError(ex.ToString());
				this.SetCrudException(result, ex, out _);
			}

			return result;
		}
		protected ActionResult SaveDeletions(DbContext context, TransactionScope transaction = null)
		{
			ActionResult result = new ActionResult();

			try
			{
				result.ObjectsAffected = 1;
				context.BulkSaveChanges(useEntityFrameworkPropagation: true);
				if (result.ObjectsAffected == 0)
				{
					result.Success = true;
					result.ErrorNumber = (int)ActionResultDelete.NothingDeleted;
				}
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Dispose();
				LogCollector.LogError(ex.ToString());
				this.SetCrudException(result, ex, out _);
			}

			return result;
		}

		private void SetCrudException(ActionResult result, Exception ex, out int? sysLogId)
		{
			if (slm == null)
				slm = new SysLogManager(parameterObject);

			result.Exception = ex;
			sysLogId = slm.GetLastLogEntry()?.SysLogId;
			if (sysLogId.HasValue)
				result.IntegerValue = sysLogId.Value;
		}
		private void HandleOptimisticConcurrency(ActionResult result, OptimisticConcurrencyException ocEx, ObjectContext context = null, EntityObject originalItem = null)
		{
			if (result == null || ocEx?.StateEntries == null)
				return;

			foreach (ObjectStateEntry entry in ocEx.StateEntries)
			{
				StringBuilder builder = new StringBuilder();
				builder.Append($"Problem with an entity from entityset [{entry.EntitySet.Name}]. \r\n");
				builder.Append($"Key values are [{KeyValuesToString(entry.EntityKey)}]. \r\n");
				builder.Append($"Modified properties are [{GetModifiedPropertiesToString(entry.GetModifiedProperties())}]. \r\n");
				builder.Append("Original Values: \r\n");
				DbDataRecord originalRecords = entry.OriginalValues;
				for (int i = 0; i < entry.OriginalValues.FieldCount; i++)
					builder.Append($"\t Field [{originalRecords.GetName(i)}], Value [{originalRecords[i]}]. \r\n");
				builder.Append("New Values: \r\n");
				DbDataRecord currentRecords = entry.CurrentValues;
				for (int i = 0; i < currentRecords.FieldCount; i++)
					builder.Append($"\t Field [{currentRecords.GetName(i)}], Value [{currentRecords[i]}]. \r\n");
				LogCollector.LogError(builder.ToString());
			}

			if (context != null && originalItem != null)
			{
				context.Refresh(RefreshMode.StoreWins, originalItem);
				result.ObjectsAffected = context.SaveChanges();
				if (result.ObjectsAffected == 0)
				{
					result.Success = true;
					result.ErrorNumber = (int)ActionResultSave.NothingSaved;
					LogCollector.LogError("Conflict resolved with no changes");
				}
				else
					LogCollector.LogError("Conflict resolved");
			}
			else
				LogCollector.LogError("Conflict not resolved");
		}
		private string GetModifiedPropertiesToString(IEnumerable<string> strings)
		{
			List<string> list = new List<string>(strings);
			return (string.Concat(list.ToArray()));
		}
		private string KeyValuesToString(EntityKey key)
		{
			StringBuilder builder = new StringBuilder();
			if (key != null)
			{
				if (key.IsTemporary)
				{
					builder.Append("<Key=Temporary>");
				}
				else
				{
					foreach (EntityKeyMember keyMember in key.EntityKeyValues)
					{
						builder.AppendFormat("<Key={0},Value={1}>", keyMember.Key, keyMember.Value);
					}
				}
			}
			return (builder.ToString());
		}

		#endregion

		#region Entity helpers

		protected bool CanEntityLoadReferences(ObjectContext context, EntityObject entity)
		{
			return IsEntityAvailableInContext(context, entity) && !entity.IsAdded();
		}
		protected bool IsEntityAvailableInContext(ObjectContext context, EntityObject entity)
		{
			if (entity == null)
				return false;

			//if CompEntities was used, i.e. readonly - it will not be available due to MergeOption.False
			if (context is CompEntities && (context as CompEntities).IsReadOnly)
				return true;

			if (!(context is CompEntities))
				throw new Exception("Incorrect context");

			if (context.ObjectStateManager.TryGetObjectStateEntry(entity, out _))
				return true;

			return false;
		}
		protected void TryDetachEntitys<T>(CompEntities context, List<T> l) where T : EntityObject
		{
			if (l.IsNullOrEmpty())
				return;

			foreach (T e in l)
			{
				TryDetachEntity(context, e);
			}
		}
		protected bool TryDetachEntitys(ObjectContext context, List<EntityObject> entitys)
		{
			bool allSucceeded = true;
			foreach (EntityObject entity in entitys)
			{
				if (!TryDetachEntity(context, entity))
					allSucceeded = false;
			}
			return allSucceeded;
		}
		public bool TryDetachEntity(ObjectContext context, EntityObject entity)
		{
			if (entity != null && entity.EntityState != EntityState.Detached && IsEntityAvailableInContext(context, entity))
			{
				context.Detach(entity);
				return true;
			}

			return false;
		}
		protected T AddEntity<T>(ObjectContext context, T entity) where T : EntityObject
		{
			return this.AddEntity(context, entity, entity.GetType().Name);
		}
		protected T AddEntity<T>(ObjectContext context, T entity, string entitySet) where T : EntityObject
		{
			this.SetCreatedProperties(entity);
			context.AddObject(entitySet, entity);
			return entity;
		}
		protected bool IsEntityModified(CompEntities entities, int id, SoeEntityType type, DateTime? currentModified, out DateTime? modified, out string modifiedBy)
		{
			modified = null;
			modifiedBy = "";

			if (id == 0 || type == SoeEntityType.None || !currentModified.HasValue)
				return false;

			//Seems like when it is same dates, it is random which date that has most ticks. Add 1 second to remove this issue.
			currentModified = currentModified.Value.AddSeconds(1);

			GetEntityLastModified(entities, id, type, out modified, out modifiedBy);
			return modified.HasValue && modified.Value > currentModified.Value;
		}
		protected void GetEntityLastModified(CompEntities entities, int id, SoeEntityType type, out DateTime? modified, out string modifiedBy)
		{
			modified = null;
			modifiedBy = "";

			switch (type)
			{
				case SoeEntityType.SupplierInvoice:
				case SoeEntityType.CustomerInvoice:
				case SoeEntityType.Offer:
				case SoeEntityType.Order:
				case SoeEntityType.Contract:

					var invoice = entities.Invoice.Where(i => i.InvoiceId == id).OrderByDescending(i => i.Modified).FirstOrDefault();
					modified = invoice?.Modified;
					modifiedBy = invoice?.ModifiedBy;

					break;
				case SoeEntityType.SupplierPayment:
				case SoeEntityType.CustomerPayment:

					var paymentRow = entities.PaymentRow.Where(pr => pr.PaymentRowId == id).OrderByDescending(pr => pr.Modified).FirstOrDefault();
					modified = paymentRow?.Modified;
					modifiedBy = paymentRow?.ModifiedBy;

					break;
				case SoeEntityType.Voucher:

					var voucherHead = entities.VoucherHead.Where(vh => vh.VoucherHeadId == id).OrderByDescending(vh => vh.Modified).FirstOrDefault();
					modified = voucherHead?.Modified;
					modifiedBy = voucherHead?.ModifiedBy;

					break;
				case SoeEntityType.Inventory:

					var inventory = entities.Inventory.Where(i => i.InventoryId == id).OrderByDescending(i => i.Modified).FirstOrDefault();
					modified = inventory?.Modified;
					modifiedBy = inventory?.ModifiedBy;

					break;
				case SoeEntityType.TimeScheduleEmployeePeriod:

					var employeePeriod = entities.TimeScheduleEmployeePeriod.Where(ep => ep.TimeScheduleEmployeePeriodId == id).OrderByDescending(ep => ep.Modified).FirstOrDefault();
					modified = employeePeriod?.Modified;
					modifiedBy = employeePeriod?.ModifiedBy;

					break;
			}
		}
		public string GetCreatedModified(EntityObject entity)
		{
			if (entity == null)
				return String.Empty;

			StringBuilder status = new StringBuilder();

			object created = entity.GetEntityProperty("Created");
			object createdBy = entity.GetEntityProperty("CreatedBy");
			if (created != null && createdBy != null)
				status.Append($"{GetText(1922, "Skapad")} {Convert.ToDateTime(created).ToShortDateLongTimeString()} {GetText(1923, "av")} {createdBy}. ");

			object modified = entity.GetEntityProperty("Modified");
			object modifiedBy = entity.GetEntityProperty("ModifiedBy");
			if (modified != null && modifiedBy != null)
				status.Append($"{GetText(1924, "Ändrad")} {Convert.ToDateTime(modified).ToShortDateLongTimeString()} {GetText(1923, "av")} {modifiedBy.ToString()}");

			return status.ToString();
		}
		protected string GetUserDetails(IUser fallbackUser = null)
		{
			string value = null;
			if (parameterObject != null)
			{
				if (parameterObject.SupportUserId.HasValue)
					value = $"{Constants.APPLICATION_SUPPORT_NAME} ({parameterObject.SupportUserId})";
				else if (!string.IsNullOrEmpty(parameterObject.LoginName))
					value = parameterObject.LoginName;
				else if (fallbackUser != null)
					value = fallbackUser.LoginName;
				value = StringUtility.Left(value, 50);
			}
			return value;
		}
		protected SoeEntityStateTransition GetStateTransition(User user, EmployeeUserDTO employeeUser)
		{
			SoeEntityStateTransition stateTransition = GetStateTransition((SoeEntityState)user.State, employeeUser.State);
			if (stateTransition == SoeEntityStateTransition.None && user.LoginName != employeeUser.LoginName)
				stateTransition = SoeEntityStateTransition.ChangeLoginName;

			return stateTransition;
		}
		protected SoeEntityStateTransition GetStateTransition(SoeEntityState oldState, SoeEntityState newState)
		{
			SoeEntityStateTransition stateTransition = SoeEntityStateTransition.None;

			if (oldState == newState)
				stateTransition = SoeEntityStateTransition.None;
			else if (oldState == SoeEntityState.Active && newState == SoeEntityState.Inactive)
				stateTransition = SoeEntityStateTransition.ActiveToInactive;
			else if (oldState == SoeEntityState.Active && newState == SoeEntityState.Deleted)
				stateTransition = SoeEntityStateTransition.ActiveToDeleted;
			else if (oldState == SoeEntityState.Inactive && newState == SoeEntityState.Active)
				stateTransition = SoeEntityStateTransition.InactiveToActive;
			else if (oldState == SoeEntityState.Deleted && newState == SoeEntityState.Active)
				stateTransition = SoeEntityStateTransition.DeletedToActive;
			return stateTransition;
		}
		protected List<T> GetEmployeeDataInBatches<T>(GetDataInBatchesModel model, DelGetDataInBatches<T> callback, int? forceLength = null)
		{
			return GetDataInBatches(model, callback, forceLength ?? EmployeeManager.GetEmployeeIdsForQueryLength(model.Entities));
		}
		protected List<T> GetDataInBatches<T>(GetDataInBatchesModel model, DelGetDataInBatches<T> callback, int batchLength)
		{
			List<T> data = new List<T>();
			if (model.IsValid())
			{
				model.Start(batchLength);
				while (model.HasHasMoreBatches())
				{
					data.AddRange(callback(model));
					model.MoveToNextBatch();
				}
			}
			return data;
		}

		public ActionResult ChangeEntityState(EntityObject entity, SoeEntityState state, User user = null, DateTime? modified = null, bool discardCheckes = false)
		{
			return ChangeEntityState(null, entity, state, false, user, modified, discardCheckes);
		}
		public ActionResult ChangeEntityState(ObjectContext context, EntityObject entity, SoeEntityState state, bool saveChanges, User user = null, DateTime? modified = null, bool discardCheckes = false)
		{
			return ChangeEntityState(context, null, entity, state, saveChanges, user?.ToDTO() ?? parameterObject?.SoeUser, modified, discardCheckes, out _);
		}
		protected ActionResult ChangeEntityState(ObjectContext context, TransactionScope transaction, EntityObject entity, SoeEntityState state, bool saveChanges, UserDTO user, DateTime? modified, bool discardCheckes, out int? sysLogId)
		{
			ActionResult result = new ActionResult();
			sysLogId = null;

			try
			{
				if (TrySetStateProperty(entity, (int)state))
				{
					TrySetModifiedWithNoCheckesProperty(entity, discardCheckes);
					TrySetModifiedByTaskProperty(entity);
					SetModifiedProperties(entity, user, modified);
					if (saveChanges)
						result = SaveChanges(context, null);
				}
			}
			catch (Exception ex)
			{
				transaction?.Dispose();
				this.LogError(ex, this.log);
				this.SetCrudException(result, ex, out sysLogId);
			}

			return result;
		}
		protected ActionResult ChangeEntityStateOnEntity<T>(T entity, SoeEntityState state)
		{
			return ChangeEntityStateOnEntity(null, entity, state, false, null, false);
		}
		protected ActionResult ChangeEntityStateOnEntity<T>(DbContext context, T entity, SoeEntityState state, bool saveChanges, UserDTO user = null, bool discardCheckes = false)
		{
			return ChangeEntityStateOnEntity(context, null, entity, state, saveChanges, user, discardCheckes, out _);
		}
		protected ActionResult ChangeEntityStateOnEntity<T>(DbContext dbContext, TransactionScope transaction, T entity, SoeEntityState state, bool saveChanges, UserDTO user, bool discardCheckes, out int? sysLogId)
		{
			ActionResult result = new ActionResult();
			sysLogId = null;
			if (TrySetStateProperty(entity, (int)state))
			{
				TrySetModifiedWithNoCheckesProperty(entity, discardCheckes);
				TrySetModifiedByTaskProperty(entity);
				SetModifiedPropertiesOnEntity<T>(entity, user);
				if (saveChanges)
					result = SaveChanges(dbContext, transaction);
			}
			return result;
		}
		public virtual void SetCreatedProperties(EntityObject entity, IUser user = null, DateTime? created = null)
		{
			if (entity == null)
				return;

			if (!created.HasValue)
				created = DateTime.Now;

			if (entity is ICreated ent1)
				ent1.SetCreated(created.Value, GetUserDetails(user));
			else if (entity is ICreatedNotNull ent2)
				ent2.SetCreated(created.Value, GetUserDetails(user));
			else
				SetCreatedProperties(entity, created.Value, GetUserDetails(user));

			TrySetCreatedByTaskProperty(entity);
		}
		protected virtual void SetCreatedPropertiesOnEntity<T>(T entity, IUser user = null, DateTime? created = null)
		{
			if (entity == null)
				return;

			if (!created.HasValue)
				created = DateTime.Now;

			if (entity is ICreated ent)
				ent.SetCreated(created.Value, GetUserDetails(user));
			else
				SetCreatedOnType(entity, created.Value, GetUserDetails(user));

			TrySetCreatedByTaskProperty(entity);
		}
		protected virtual void SetModifiedProperties(EntityObject entity, IUser user = null, DateTime? modified = null)
		{
			if (entity == null)
				return;

			if (!modified.HasValue)
				modified = DateTime.Now;

			if (entity is IModified ent)
				ent.SetModified(modified.Value, GetUserDetails(user));
			else
				SetModifiedProperties(entity, modified.Value, GetUserDetails(user));

			TrySetModifiedByTaskProperty(entity);
		}
		protected virtual void SetModifiedPropertiesOnEntity<T>(T entity, IUser user = null, DateTime? modified = null)
		{
			if (entity == null)
				return;

			if (!modified.HasValue)
				modified = DateTime.Now;

			if (entity is IModified ent)
				ent.SetModified(modified.Value, GetUserDetails(user));
			else
				SetModifiedOnType(entity, modified.Value, GetUserDetails(user));

			TrySetModifiedByTaskProperty(entity);
		}
		protected virtual void SetDeletedProperties(EntityObject entity, User user = null, DateTime? deleted = null)
		{
			if (entity == null)
				return;

			SetDeletedProperties(entity, deleted ?? DateTime.Now, GetUserDetails(user));
		}
		protected virtual void TrySetCreatedByTaskProperty<T>(T entity)
		{
			if (this.currentTaskWatchLogId.HasValue && entity is ITask)
				(entity as ITask).SetCreatedByTask(this.currentTaskWatchLogId.Value);
		}
		protected virtual void TrySetModifiedByTaskProperty<T>(T entity)
		{
			if (this.currentTaskWatchLogId.HasValue && entity is ITask)
				(entity as ITask).SetModifiedByTask(this.currentTaskWatchLogId.Value);
		}
		protected virtual void TrySetModifiedWithNoCheckesProperty<T>(T entity, bool discardCheckes)
		{
			if (discardCheckes && entity is IModifiedWithNoCheckes)
				(entity as IModifiedWithNoCheckes).SetModifiedWithNoCheckes();
		}
		protected virtual bool TrySetStateProperty<T>(T entity, int state)
		{
			if (entity is IState ent)
			{
				ent.SetState(state);
				return true;
			}
			else
				return entity.TrySetEntityProperty("State", state);
		}

		private void SetCreatedProperties(EntityObject entity, DateTime created, string createdBy)
		{
			if (entity == null)
				return;

			string existingCreated = entity.GetEntityProperty("Created") as string;
			if (string.IsNullOrEmpty(existingCreated))
				entity.TrySetEntityProperty("Created", created);
			string existingCreatedBy = entity.GetEntityProperty("CreatedBy") as string;
			if (string.IsNullOrEmpty(existingCreatedBy))
				entity.TrySetEntityProperty("CreatedBy", createdBy);
		}
		private void SetCreatedOnType<T>(T entity, DateTime created, string createdBy)
		{
			if (entity == null)
				return;

			string existingCreated = entity.GetPropertyValue("Created")?.ToString();
			if (string.IsNullOrEmpty(existingCreated))
				entity.TrySetEntityProperty<T>("Created", created);
			string existingCreatedBy = entity.GetPropertyValue("CreatedBy")?.ToString();
			if (string.IsNullOrEmpty(existingCreatedBy))
				entity.TrySetEntityProperty<T>("CreatedBy", createdBy);
		}
		private void SetModifiedProperties(EntityObject entity, DateTime modified, string modifiedBy)
		{
			if (entity == null)
				return;

			entity.TrySetEntityProperty("Modified", modified);
			entity.TrySetEntityProperty("ModifiedBy", modifiedBy);
		}
		private void SetModifiedOnType<T>(T entity, DateTime modified, string modifiedBy)
		{
			if (entity == null)
				return;

			entity.TrySetEntityProperty<T>("Modified", modified);
			entity.TrySetEntityProperty<T>("ModifiedBy", modifiedBy);
		}
		private void SetDeletedProperties(EntityObject entity, DateTime deleted, string deletedBy)
		{
			if (entity == null)
				return;

			entity.TrySetEntityProperty("Deleted", deleted);
			entity.TrySetEntityProperty("DeletedBy", deletedBy);
		}

		#endregion

		#region Language cache

		protected string GetText(int sysTermId)
		{
			return GetText(sysTermId, (int)TermGroup.General);
		}
		protected string GetText(int sysTermId, string defaultTerm, bool forceDefaultTerm = false)
		{
			return forceDefaultTerm ? defaultTerm : GetText(sysTermId, (int)TermGroup.General, defaultTerm);
		}
		protected string GetText(int sysTermId, TermGroup termGroup, int langId = 0)
		{
			try
			{
				if (langId == 0)
					langId = GetLangId();
				return TermCacheManager.Instance.GetText(sysTermId, (int)termGroup, "", GetCulture(langId).Name);
			}
			catch (Exception ex)
			{
				LogError(ex, this.log);
				return string.Empty;
			}

		}
		protected string GetText(int sysTermId, int sysTermGroupId, int langId, string defaultTerm)
		{
			try
			{
				return TermCacheManager.Instance.GetText(sysTermId, sysTermGroupId, defaultTerm, GetCulture(langId).Name);
			}
			catch (Exception ex)
			{
				LogError(ex, this.log);
				return defaultTerm;
			}
		}
		public string GetText(int sysTermId, int sysTermGroupId, string defaultTerm = "")
		{
			try
			{
				return TermCacheManager.Instance.GetText(sysTermId, sysTermGroupId, defaultTerm, Thread.CurrentThread.CurrentCulture.Name);
			}
			catch (Exception ex)
			{
				LogError(ex, this.log);
				return defaultTerm;
			}
		}
		public List<GenericType> GetTermGroupContent(TermGroup termGroup, int langId = 0, bool addEmptyRow = false, bool skipUnknown = false, bool sortById = false)
		{
			try
			{
				return TermCacheManager.Instance.GetTermGroupContent(termGroup, langId, addEmptyRow, skipUnknown, sortById);
			}
			catch (Exception ex)
			{
				LogError(ex, this.log);
				return new List<GenericType>();
			}
		}
		public Dictionary<int, string> GetTermGroupDict(TermGroup termGroup, int langId = 0, bool addEmptyRow = false, bool includeKey = false)
		{
			try
			{
				return TermCacheManager.Instance.GetTermGroupDict(termGroup, langId, addEmptyRow, includeKey);
			}
			catch (Exception ex)
			{
				LogError(ex, this.log);
				return new Dictionary<int, string>();
			}
		}
		public SortedDictionary<int, string> GetTermGroupDictSorted(TermGroup termGroup, int langId = 0, bool addEmptyRow = false, bool includeKey = false, int? minKey = null, int? maxKey = null)
		{
			try
			{
				return TermCacheManager.Instance.GetTermGroupDictSorted(termGroup, langId, addEmptyRow, includeKey, minKey, maxKey);
			}
			catch (Exception ex)
			{
				LogError(ex, this.log);
				return new SortedDictionary<int, string>();
			}
		}
		protected int GetLangId()
		{
			string cultureCode = Thread.CurrentThread.CurrentCulture.Name;
			return GetLangId(cultureCode);
		}
		protected int GetLangId(string cultureCode)
		{
			if (string.IsNullOrEmpty(cultureCode))
				return (int)TermGroup_Languages.Unknown;

			//Default
			int langId = Constants.SYSLANGUAGE_SYSLANGUAGEID_DEFAULT;

			//Temp solution to prevent hitting SOESys database. Causes crasch i you are in a transaction
			switch (cultureCode)
			{
				case Constants.SYSLANGUAGE_LANGCODE_SWEDISH:
					langId = (int)TermGroup_Languages.Swedish;
					break;
				case Constants.SYSLANGUAGE_LANGCODE_ENGLISH:
					langId = (int)TermGroup_Languages.English;
					break;
				case Constants.SYSLANGUAGE_LANGCODE_FINISH:
					langId = (int)TermGroup_Languages.Finnish;
					break;
				case Constants.SYSLANGUAGE_LANGCODE_NORWEGIAN:
					langId = (int)TermGroup_Languages.Norwegian;
					break;
				case Constants.SYSLANGUAGE_LANGCODE_DANISH:
					langId = (int)TermGroup_Languages.Danish;
					break;
			}

			return langId;
		}
		protected CultureInfo GetCulture(int langId)
		{
			return GetCulture((TermGroup_Languages)langId);
		}
		protected CultureInfo GetCulture(TermGroup_Languages lang)
		{
			CultureInfo culture = new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_SWEDISH);

			switch (lang)
			{
				case TermGroup_Languages.Unknown:
					culture = new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_SWEDISH);
					break;
				case TermGroup_Languages.Swedish:
					culture = new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_SWEDISH);
					break;
				case TermGroup_Languages.English:
					culture = new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_ENGLISH);
					break;
				case TermGroup_Languages.Finnish:
					culture = new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_FINISH);
					break;
				case TermGroup_Languages.Norwegian:
					culture = new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_NORWEGIAN);
					break;
				case TermGroup_Languages.Danish:
					culture = new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_DANISH);
					break;
				default:
					break;
			}

			return culture;
		}

		protected void SetLanguageFromSysCountry(TermGroup_Country sysCountry)
		{
			switch (sysCountry)
			{
				case TermGroup_Country.SE:
					SetLanguage(TermGroup_Languages.Swedish);
					break;
				case TermGroup_Country.GB:
					SetLanguage(TermGroup_Languages.English);
					break;
				case TermGroup_Country.FI:
					SetLanguage(TermGroup_Languages.Finnish);
					break;
				case TermGroup_Country.NO:
					SetLanguage(TermGroup_Languages.Norwegian);
					break;
				case TermGroup_Country.DK:
					SetLanguage(TermGroup_Languages.Danish);
					break;
				default:
					SetLanguage(TermGroup_Languages.Swedish);
					break;
			}
		}
		protected void SetLanguage(int langId)
		{
			if (Enum.IsDefined(typeof(TermGroup_Languages), langId))
				SetLanguage((TermGroup_Languages)langId);
		}

		protected void SetLanguage(TermGroup_Languages lang)
		{
			var culture = GetCulture(lang);
			if (culture != null)
				Thread.CurrentThread.CurrentCulture = culture;
		}
		protected void SetLanguage(string cultureCode)
		{
			if (!String.IsNullOrEmpty(cultureCode))
				Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureCode);
		}

		#endregion

		#region Logging -> TaskWatchLogDTO

		protected SoeProgressInfo info;
		private readonly List<TaskWatchLogDTO> watchLogs = new List<TaskWatchLogDTO>();
		private readonly Dictionary<string, Stopwatch> stopWatchDict = new Dictionary<string, Stopwatch>();
		protected long? StartTask(string name, string className, string batch, string parameters, int? idCount = null, int? intervalCount = null)
		{
			using (CompEntities entities = new CompEntities())
			{
				TaskWatchLog taskWatchLog = StartWatch(
					name,
					className: className,
					batch: batch,
					parameters: parameters,
					idCount: idCount,
					intervalCount: intervalCount
				).FromDTO();

				if (taskWatchLog != null)
				{
					bool skipWatchLog = SettingManager.GetBoolSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.SkipTaskWatchLog, 0, 0, 0);
					if (!skipWatchLog)
					{
						entities.TaskWatchLog.AddObject(taskWatchLog);
						SaveChanges(entities);
					}
				}

				return taskWatchLog?.TaskWatchLogId;
			}
		}
		protected void CompleteTask(long? taskWatchLogId)
		{
			if (!taskWatchLogId.HasValue || taskWatchLogId <= 0)
				return;

			using (CompEntities entities = new CompEntities())
			{
				TaskWatchLog taskWatchLog = entities.TaskWatchLog.FirstOrDefault(i => i.TaskWatchLogId == taskWatchLogId);
				if (taskWatchLog != null)
				{
					TaskWatchLogDTO taskWatchLogDTO = StopWatch(taskWatchLog.Name);
					taskWatchLog.SetCompleted(taskWatchLogDTO);
					SaveChanges(entities);
				}
			}
		}
		protected void Watch(string task, string description = null, int iteration = 0)
		{
			if (!this.stopWatchDict.ContainsKey(task))
				this.StartWatch(task, description: description);
			else
				this.StopWatch(task, iteration: iteration);
		}
		protected TaskWatchLogDTO StartWatch(
			string name,
			string description = null,
			string className = null,
			string batch = null,
			string parameters = null,
			int? idCount = null,
			int? intervalCount = null,
			bool append = false
			)
		{
			TaskWatchLogDTO watchLog = null;
			if (append)
				watchLog = this.watchLogs.FirstOrDefault(i => i.Name == name);

			if (watchLog == null)
			{
				watchLog = TaskWatchLogDTO.StartTask(name, description, ActorCompanyId, UserId, RoleId, SupportActorCompanyId, SupportUserId, SupportDefaultRoleId, className, batch, parameters, idCount, intervalCount);
				this.watchLogs.Add(watchLog);
			}
			else
			{
				watchLog.SetAsRunning();
			}

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			if (this.stopWatchDict.ContainsKey(name))
				this.stopWatchDict.Remove(name);
			this.stopWatchDict.Add(name, stopwatch);

			//Uncomment to not include detailed description in progressbar
			if (this.info != null && !String.IsNullOrEmpty(description))
				this.info.Message = description;

			return watchLog;
		}
		protected TaskWatchLogDTO StopWatch(string task, int iteration = 0)
		{
			if (!this.stopWatchDict.ContainsKey(task))
				return null;

			TaskWatchLogDTO watchLog = GetRunningTask(task);
			if (watchLog == null)
				return null;

			Stopwatch stopwatch = this.stopWatchDict[task];
			stopwatch.Stop();

			watchLog.StopTask(stopwatch.Elapsed, iteration);

			return watchLog;
		}
		protected List<string> GetWatchLogs()
		{
			if (this.watchLogs.IsNullOrEmpty())
				return new List<string>();

			List<TaskWatchLogDTO> completedWatchLogs = this.watchLogs.Where(i => !i.IsRunning).ToList();

			double totalDurationMs = completedWatchLogs.First().Duration.TotalMilliseconds;
			foreach (TaskWatchLogDTO watchLog in completedWatchLogs)
			{
				watchLog.UpdatePercent(Decimal.Round((decimal)(watchLog.Duration.TotalMilliseconds / totalDurationMs) * 100, 2));
			}

			return this.watchLogs.Select(watchLog => watchLog.ToString()).ToList();
		}
		private TaskWatchLogDTO GetRunningTask(string task)
		{
			return this.watchLogs.FirstOrDefault(i => i.Name == task && i.IsRunning);
		}
		protected TaskWatchLog GetTaskWatchLog(long taskWatchLogId)
		{
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.TaskWatchLog.NoTracking();
			return entitiesReadOnly.TaskWatchLog.FirstOrDefault(i => i.TaskWatchLogId == taskWatchLogId);
		}

		#endregion

		#region Logging -> Log4net

		protected ActionResult LogPersonalData(object entity, TermGroup_PersonalDataActionType actionType, string url = "")
		{
			try
			{
				Task.Run(() => LogDTO(entity, actionType, url));
			}
			catch (Exception ex)
			{
				LogError(ex, log);
				return new ActionResult(ex);
			}

			return new ActionResult();
		}
		protected async Task LogDTO(object entity, TermGroup_PersonalDataActionType actionType, string url = "")
		{
			try
			{
				var lm = new LoggerManager(parameterObject);
				await lm.CreatePersonalDataLog(entity, logCorrelationId, actionType, url: url);
			}
			catch (Exception ex)
			{
				LogError(ex, log);
			}
		}
		protected void LogInfo(Exception ex, ILog log, long? taskWatchLogId = null)
		{
			SysLogManager.AddSysLog(ex, log4net.Core.Level.Info, log, taskWatchLogId: taskWatchLogId);
		}
		protected void LogInfo(string message, long? taskWatchLogId = null)
		{
			SysLogManager.AddSysLogInfoMessage(Environment.MachineName, THREAD, message, taskWatchLogId: taskWatchLogId);
		}
		protected void LogWarning(Exception ex, ILog log, long? taskWatchLogId = null)
		{
			SysLogManager.AddSysLog(ex, log4net.Core.Level.Warn, log, taskWatchLogId: taskWatchLogId);
		}
		protected void LogWarning(string message, long? taskWatchLogId = null)
		{
			SysLogManager.AddSysLogWarningMessage(Environment.MachineName, THREAD, message, taskWatchLogId: taskWatchLogId);
		}
		protected void LogError(string message, long? taskWatchLogId = null)
		{
			SysLogManager.AddSysLogErrorMessage(Environment.MachineName, THREAD, message, taskWatchLogId: taskWatchLogId);
		}
		protected void LogError(Exception ex, ILog log, long? taskWatchLogId = null)
		{
			try
			{
				AppInsightUtil.Log(ex, "", parameterObject);
			}
			catch { } //NOSONAR

			SysLogManager.AddSysLog(ex, log4net.Core.Level.Error, log, taskWatchLogId: taskWatchLogId);
		}
		protected void DebugWrite(string msg)
		{
			Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + ": " + msg);
		}
		protected void LogTransactionFailed(string source, ILog log)
		{
			//For now /Rickard  --- LogWarning(new SoeTransactionFailedException(source), log);
		}

		#endregion

		#region Method helpers

		private readonly Dictionary<string, HashSet<string>> methodKeys = new Dictionary<string, HashSet<string>>();

		protected bool HasMethodBeenExecutedBefore(string methodName, string parameters, bool keepOnlyLatest = false)
		{
			if (!methodKeys.ContainsKey(methodName))
				methodKeys[methodName] = new HashSet<string>();

			if (methodKeys[methodName].Contains(parameters))
				return true;

			if (keepOnlyLatest)
				methodKeys[methodName].Clear();

			methodKeys[methodName].Add(parameters);
			return false;
		}

		protected string GenerateParameterKey(params object[] parameters)
		{
			return string.Join("_", parameters.Select(p => p?.ToString() ?? "null"));
		}

		#endregion

		#region Threading

		public ParallelOptions GetDefaultParallelOptions(int maxDegreeOfParallelism = Constants.MaxDegreeOfParallelism)
		{
			return new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
		}

		#endregion

		#region Transactions

		protected TransactionScope CreateTransactionScope(TimeSpan timeout)
		{
			SetTransactionManagerField("_cachedMaxTimeout", true);
			SetTransactionManagerField("_maximumTimeout", timeout);
			return new TransactionScope(TransactionScopeOption.RequiresNew, timeout);
		}
		protected TransactionScope CreateTransactionScope(TimeSpan timeout, IsolationLevel isolationLevel)
		{
			TransactionOptions options = ConfigSettings.TRANSACTIONOPTION_DEFAULT;
			options.IsolationLevel = isolationLevel;
			options.Timeout = timeout;
			SetTransactionManagerField("_cachedMaxTimeout", true);
			SetTransactionManagerField("_maximumTimeout", timeout);
			return new TransactionScope(TransactionScopeOption.RequiresNew, options);
		}
		protected void SetTransactionManagerField(string fieldName, object value)
		{
			typeof(TransactionManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
		}

		#endregion

		#region WebPubSub

		protected void WebPubSubSendMessage(string group, string content)
		{
			try
			{
				WebPubSubUtil.SendMessage(group, content);
			}
			catch (Exception ex)
			{
				LogError(ex, this.log);
			}
		}

		#endregion
	}

	#region Help-classes

	public interface IRequestState
	{
		T Get<T>(string key);
		void Store(string key, object obj);
	}

	public class OperationContextExtension : IExtension<OperationContext>
	{
		public IDictionary<string, object> State { get; private set; }

		public OperationContextExtension()
		{
			State = new Dictionary<string, object>();
		}

		public void Attach(OperationContext owner) {  /**Implementations not needed **/}
		public void Detach(OperationContext owner) {  /**Implementations not needed **/ }
	}

	public class WcfRequestState : IRequestState
	{
		private static IDictionary<string, object> State
		{
			get
			{
				var extension = OperationContext.Current.Extensions.Find<OperationContextExtension>();
				if (extension == null)
				{
					extension = new OperationContextExtension();
					OperationContext.Current.Extensions.Add(extension);
				}

				return extension.State;
			}
		}

		public T Get<T>(string key)
		{
			if (State.ContainsKey(key))
				return (T)State[key];
			return (T)(object)null;
		}

		public void Store(string key, object obj)
		{
			if (!State.ContainsKey(key))
				State.Add(key, obj);
		}
	}

	#endregion
}
