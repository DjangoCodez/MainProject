using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.Util
{
    public static class Constants
    {
        #region General

        //Version
        public const string APPLICATION_NAME = "SoftOne";
        public const string APPLICATION_VERSION = "1.0";
        public const decimal MOBILE_WS_CURRENT_VERSION = (decimal)45;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_0 = 0; //since we from ws version 7 has included version parameter in all ws calls, old apps will send a null value 
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_1 = 1; //Login method has been sending 1.0 before ws version 7
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_7 = 7;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_8 = 8;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_9 = 9;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_11 = 11;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_12 = 12;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_13 = 13;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_14 = 14;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_15 = 15;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_16 = 16;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_17 = 17;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_18 = 18;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_19 = 19;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_20 = 20;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_21 = 21;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_22 = 22;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_23 = 23;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_24 = 24;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_25 = 25;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_26 = 26;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_27 = 27;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_28 = 28;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_29 = 29;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_30 = 30;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_31 = 31;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_32 = 32;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_33 = 33;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_34 = 34;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_35 = 35;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_36 = 36;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_37 = 37;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_38 = 38;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_39 = 39;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_40 = 40;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_41 = 41;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_42 = 42;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_43 = 43;
        public const decimal MOBILE_WS_SUPPORTED_OLDVERSION_44 = 44;

        //everytime we increases MOBILE_WS_CURRENT_VERSION, we will have to add a MOBILE_WS_SUPPORTED_OLDVERSION_X = X with the old value the support old apps with IsCallerExpectedVersionOlderOrEqualToGivenVersion

        //Login
        public const string APPLICATION_SUPPORT_NAME = "SoftOne";
        public const string APPLICATION_LICENSEADMIN_LOGINNAME = "sys";
        public const string APPLICATION_LICENSEADMIN_NAME = "Admin";

        //Password
        public const int PASSWORD_DEFAULT_MIN_LENGTH = 6;
        public const int PASSWORD_DEFAULT_MAX_LENGTH = 20;

        //Naming
        public const NameStandard APPLICATION_NAMESTANDARD = NameStandard.FirstNameThenLastName;

        //Delimiter
        public const char Delimiter = '#';

        #endregion

        #region Settings

        public const bool USE_JS_NAVIGATION = true;

        #endregion

        #region Configuration

        //Path to file system for saving attachment files (key in web.config)
        public const string FILES_DIRECTORY_KEY = "FilesDirectory";

        //Section in web.config
        public const string SOE_CONFIGURATION_SETTINGS = "SoeConfigurationSettings";

        //Properties on section
        public const string SOE_CONFIGURATION_SETTING_LOGTOEVENTVIEWER = "logToEventViewer";
        public const string SOE_CONFIGURATION_SETTING_ENABLEPRINTENTITYFRAMEWORKINFO = "enablePrintEntityFrameworkInfo";
        public const string SOE_CONFIGURATION_SETTING_PRINTENTITYFRAMEWORKPATH = "printEntityFrameworkPath";
        public const string SOE_CONFIGURATION_SETTING_ENABLESERVICES = "enableServices";
        public const string SOE_CONFIGURATION_SETTING_ENABLEENCRYPTCONNECTIONSTRING = "enableEncryptConnectionString";
        public const string SOE_CONFIGURATION_SETTING_RELEASEMODE = "releaseMode";
        public const string SOE_CONFIGURATION_SETTING_ENABLEUSERSESSION = "enableUserSession";
        public const string SOE_CONFIGURATION_SETTING_SCRIPTVERSION = "scriptVersion";
        public const string SOE_CONFIGURATION_SETTING_NOOFSYSNEWSITEMS = "noOfSysNewsItems";

        //TimeOut
        public const int SOE_EXECUTION_TIMEOUT_SECONDS = 3600; //60 min (3600 seconds)
        public const int SOE_SESSION_TIMEOUT_MINUTES = 60;
        public const int SOE_SESSION_TIMEOUT_WARN_MINUTES = 2;
        public const int SOE_SESSION_TIMEOUT_LOGOUT_SECONDS = 2;

        //Web services
        public const string SOE_SERVICES_BING_APPID = "94B696E3D09BE586E0EFFF373CF1343554565697";
        public const string SOE_WEBAPI_STRING_EMPTY = "[empty]";

        //Crgen
        public const string SOE_CRGEN_PATH = @"c:\cgentemp\"; //NOSONAR

        //EDI

        public const string SOE_EDI_FTP_NELFO = "ftp://ftp.softone.se/Grossister/Nelfo/";
        public const string SOE_EDI_FTP_LVISNET = "ftp://ftp.softone.se/Grossister/LvisNet/";

        //Environment configuration
        public const char SOE_ENVIRONMENT_CONFIGURATION_SEPARATOR = '_';

        #endregion

        #region Logging

        public const int SOE_SYSLOG_MAX = 5000;

        #endregion

        #region Prefix/Suffix/Extensions

        public const string SOE_SERVER_FILENAME_PREFIX_OLD = "SOE_"; //Used for backward compabillity
        public const string SOE_SERVER_FILENAME_PREFIX = "GO_";
        public const string SOE_SERVER_SIE_PREFIX = "SIE_";
        public const string SOE_SERVER_PG_PREFIX = "PG_";
        public const string SOE_SERVER_SEPA_PREFIX = "SEPA_";
        public const string SOE_SERVER_ISO20022_PREFIX = "ISO20022_";
        public const string SOE_SERVER_FI_TAX_PREFIX = "KVI_";
        public const string SOE_SERVER_GM_REPORT_PREFIX = "GM_";
        public const string SOE_SERVER_LB_PREFIX = "LB_";
        public const string SOE_SERVER_NETS_PREFIX = "NETS_";
        public const string SOE_SERVER_CFP_PREFIX = "CFP_";
        public const string SOE_SERVER_ICA_PREFIX = "ICA_";
        public const string SOE_SERVER_FILE_PAYMENT_SUFFIX = @".txt";
        public const string SOE_SERVER_FILE_EF_SUFFIX = @".txt";
        public const string SOE_SERVER_FILE_SIE_SUFFIX = @".SE";
        public const string SOE_SERVER_FILE_FI_TAX_SUFFIX = @".txt";
        public const string SOE_SERVER_FILE_GM_REPORT_SUFFIX = @".txt";
        public const string SOE_SERVER_FILE_RPT_SUFFIX = @".rpt";
        public const string SOE_SERVER_FILE_PDF_SUFFIX = @".pdf";
        public const string SOE_SERVER_FILE_XML_SUFFIX = @".xml";
        public const string SOE_SERVER_FILE_JSON_SUFFIX = @".json";
        public const string SOE_SERVER_FILE_CSV_PREFIX = @".csv";
        public const string SOE_SERVER_FILE_EXCEL_SUFFIX = @".xls";
        public const string SOE_SERVER_FILE_EXCEL2007_SUFFIX = @".xlsx";
        public const string SOE_SERVER_FILE_WORD_SUFFIX = @".doc";
        public const string SOE_SERVER_FILE_WORD2007_SUFFIX = @".docx";
        public const string SOE_SERVER_FILE_RTF_SUFFIX = @".rtf";
        public const string SOE_SERVER_FILE_TEXT_SUFFIX = @".txt";
        public const string SOE_SERVER_FILE_ZIP_SUFFIX = @".zip";
        public const string SOE_SERVER_FILE_SI_SUFFIX = @".si";
        public const string SOE_SERVER_FILE_VISMA_SUFFIX = @".visma";
        public const string SOE_SERVER_FILE_EKS_SUFFIX = @".txt";
        public const string SOE_SERVER_FILE_TABBSEPARATED_SUFFIX = @".ttx";
        public const string SOE_SERVER_FILE_CHARACTERSEPARATED_SUFFIX = @".csv";
        public const string SOE_SERVER_FILE_GIF_SUFFIX = @".gif";
        public const string SOE_SERVER_FILE_PNG_SUFFIX = @".png";
        public const string SOE_SERVER_FILE_JPEG_SUFFIX = @".jpeg";
        public const string SOE_SERVER_FILE_WMF_SUFFIX = @".wmf";
        public const string SOE_SERVER_FILE_AVI_SUFFIX = @".avi";
        public const string SOE_SERVER_FILE_MPEG_SUFFIX = @".mpeg";
        public const string SOE_SERVER_FILE_DEF_SUFFIX = @".DEF";
        public const string SOE_SERVER_FILE_DAT_SUFFIX = @".dat";
        public const string SOE_SERVER_AUTOGIRO_PREFIX = "AG_";

        #endregion

        #region Internal URL/URI

        public const string SOE_URL_BASE = "/soe/";

        #endregion

        #region External URL/URI

        public const string CURRENCY_ECB_URI = "http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

        #endregion

        #region Modules and Sections

        public const string SOE_MODULE_BILLING = "billing";
        public const string SOE_MODULE_COMMUNICATION = "communication";
        public const string SOE_MODULE_ECONOMY = "economy";
        public const string SOE_MODULE_ESTATUS = "estatus";
        public const string SOE_MODULE_MANAGE = "manage";
        public const string SOE_MODULE_TIME = "time";
        public const string SOE_MODULE_ARCHIVE = "archive";
        public const string SOE_MODULE_STATISTICS = "statistics";
        public const string SOE_MODULE_CLIENTMANAGEMENT = "clientmanagement";

        public const string SOE_SECTION_ACCOUNTING = "accounting";
        public const string SOE_SECTION_ATTEST = "attest";
        public const string SOE_SECTION_ATTESTUSER = "attestuser";
        public const string SOE_SECTION_COMPANIES = "companies";
        public const string SOE_SECTION_CONTACTPERSONS = "contactpersons";
        public const string SOE_SECTION_CONTRACT = "contract";
        public const string SOE_SECTION_CONTRACTS = "contracts";
        public const string SOE_SECTION_CUSTOMER = "customer";
        public const string SOE_SECTION_DISTRIBUTION = "distribution";
        public const string SOE_SECTION_EMPLOYEE = "employee";
        public const string SOE_SECTION_EXPORT = "export";
        public const string SOE_SECTION_IMPORT = "import";
        public const string SOE_SECTION_INVENTORY = "inventory";
        public const string SOE_SECTION_INVOICE = "invoice";
        public const string SOE_SECTION_LANGUAGE = "language";
        public const string SOE_SECTION_OFFER = "offer";
        public const string SOE_SECTION_ORDER = "order";
        public const string SOE_SECTION_PREFERENCES = "preferences";
        public const string SOE_SECTION_PAYROLL = "payroll";
        public const string SOE_SECTION_PRODUCT = "product";
        public const string SOE_SECTION_STATISTICS = "statistics";
        public const string SOE_SECTION_PROJECT = "project";
        public const string SOE_SECTION_REGISTRY = "registry";
        public const string SOE_SECTION_ROLES = "roles";
        public const string SOE_SECTION_SCHEDULE = "schedule";
        public const string SOE_SECTION_SUPPLIER = "supplier";
        public const string SOE_SECTION_SUPPLIER_ATTESTFLOW = "supplierattestflow";
        public const string SOE_SECTION_SUPPORT = "support";
        public const string SOE_SECTION_STOCK = "stock";
        public const string SOE_SECTION_SYSTEM = "system";
        public const string SOE_SECTION_TIME = "time";
        public const string SOE_SECTION_USERS = "users";
        public const string SOE_SECTION_XEMAIL = "xemail";

        public const string SOE_STATEANALYSIS = "stateanalysis";
        public const string SOE_EXTERNALLINKS = "externalLinks";

        #endregion

        #region Language

        public const string SYSLANGUAGE_LANGCODE_SWEDISH = "sv-SE";
        public const string SYSLANGUAGE_LANGCODE_ENGLISH = "en-US";
        public const string SYSLANGUAGE_LANGCODE_FINISH = "fi-FI";
        public const string SYSLANGUAGE_LANGCODE_NORWEGIAN = "nb-NO";
        public const string SYSLANGUAGE_LANGCODE_DANISH = "da-DK";
        public const string SYSLANGUAGE_LANGCODE_DEFAULT = SYSLANGUAGE_LANGCODE_SWEDISH;
        public const int SYSLANGUAGE_SYSLANGUAGEID_DEFAULT = (int)TermGroup_Languages.Swedish;

        #endregion

        #region TimeZone

        public const string TIMEZONE_DEFAULT = "W. Europe Standard Time";

        #endregion

        #region Encoding

        public const string ENCODING_IBM437_NAME = "IBM437";
        public static Encoding ENCODING_IBM437
        {
            get
            {
                return Encoding.GetEncoding(ENCODING_IBM437_NAME);
            }
        }

        public const string ENCODING_IBM850_NAME = "IBM850";
        public static Encoding ENCODING_IBM850
        {
            get
            {
                return Encoding.GetEncoding(ENCODING_IBM850_NAME);
            }
        }

        public const string ENCODING_IBM865_NAME = "IBM865";
        public static Encoding ENCODING_IBM865
        {
            get
            {
                return Encoding.GetEncoding(ENCODING_IBM865_NAME);
            }
        }

        public const string ENCODING_LATIN1_NAME = "ISO-8859-1";
        public static Encoding ENCODING_LATIN1
        {
            get
            {
                return Encoding.GetEncoding(ENCODING_LATIN1_NAME);
            }
        }

		#endregion

		#region Session

		#region Web forms (will be removed continously when pages are migrated)

		//Page-living in aspx
		public const string SESSION_REFERRER_URL = "REFERRER_URL"; //Navigation prev url
        public const string SESSION_ACCOUNTPERIOD_OLDSTATUS = "ACCOUNTPERIOD_OLDSTATUS"; //Accountperiod (VoucherSeriesMapping)
        public const string SESSION_ACCOUNTYEARID = "CURRENT_YEAR"; //AccountYear (VoucherSeriesMapping)
        public const string SESSION_ACCOUNTYEAR_CHECKED = "ACCOUNTYEAR_CHECKED"; //Show create account year (modal)
        public const string SESSION_PRICELIST_CHECKED = "PRICELIST_CHECKED"; //Show import pricelist (modal)
        public const string SESSION_ACCOUNTYEAR = "CURRENT_ACCOUNTYEAR"; //AccountYear
        public const string SESSION_SYSTEM_ADMINTASK_WCF = "SYSTEM_ADMINTASK_WCF"; //System-task params
        public const string SESSION_DOWNLOAD_REPORT_ITEM = "DOWNLOAD_REPORT_ITEM"; //Print report object
        public const string SESSION_DOWNLOAD_SIE_ITEM = "DOWNLOAD_SIE_ITEM"; //SIE object
        public const string SESSION_DOWNLOAD_SIE_CONFLICTS = "DOWNLOAD_SIE_CONFLICTS"; //SIE conflicts object
        public const string SESSION_DOWNLOAD_FI_TAX_ITEM = "DOWNLOAD_FI_TAX_ITEM"; //FI tax object
        public const string SESSION_DOWNLOAD_GM_REPORT_ITEM = "DOWNLOAD_GM_REPORT_ITEM"; //GM report object
        public const string SESSION_DOWNLOAD_SCR_ITEM = "SESSION_DOWNLOAD_SCR_ITEM"; //SCR object
        public const string SESSION_ACTION_DOUBLECLICK = "ACTION_DOUBLECLICK"; //Prevent doubleclick
        public const string SESSION_SETACCOUNT_DIFF = "SETACCOUNT_DIFF"; //Modal - set account diff
        //Page-living in PageBase/Form
        public const string SESSION_SOEFORM_OBJECT = "SOEFORM_OBJECT"; //Webforms object
        public const string SESSION_SOEFORM_PREVIOUS_BUTTONTEXT = "SOEFORM_PREVIOUS_BUTTONTEXT"; //Webforms button text
        public const string SESSION_SOETAB_PREVIOUS_HEADERTEXT = "SOETAB_PREVIOUS_HEADERTEXT"; //Webforms header text
        public const string SESSION_MESSAGE_FROM_SELF = "MESSAGE_FROM_SELF"; //Webforms action message

        #endregion

        #endregion

        #region Cookies

        public const int COOKIE_EXPIRATIONDAYS = 30;
        public const string COOKIE_LANG = "lang";
        public const string COOKIE_CHECK = "COOKIE_CHECK";
        public const string COOKIE_USERSESSIONID = "LOGIN_USERSESSIONID"; //UserSession
        public const string COOKIE_USERSESSIONID_REMOTELOGIN = "REMOTELOGIN_USERSESSIONID"; //UserSession supportogin
        public const string COOKIE_KEEPSESSIONALIVE = "KeepSessionAlive"; //Keeps session alive from Angular
        public const string COOKIE_ACCOUNTHIERARCHY = "CURRENT_ACCOUNTHIERARCHY"; //AccountHierarchy
        public const string COOKIE_LASTMODULE = "LASTMODULE"; //Last module
        public const string COOKIE_MENU_COLLAPSED = "SESSION_MENU_COLLAPSED"; //Left-menu collapsed
        public const string COOKIE_PARAMETER_OBJECT = "PARAMETER_OBJECT"; //Login information
                                                                          //public CompanyDTO SoeCompany { get; set; }
                                                                          //LicenseId
                                                                          //ActorCompanyID
                                                                          //Name
                                                                          //public UserDTO SoeUser { get; set; }
                                                                          //UserId
                                                                          //DefaultRoleId
                                                                          //idLoginGuid
                                                                          //public CompanyDTO SoeSupportCompany { get; set; }
                                                                          //public UserDTO SoeSupportUser { get;_ set; }
                                                                          //public int? ActiveRoleId;
                                                                          //public bool IsSupportLoggedInByCompany { get; set; }
                                                                          //public bool IsSupportLoggedInByUser { get; set; }
                                                                          //public bool IsSuperAdminMode { get; set; }
                                                                          //public bool IncludeInactiveAccounts { get; set; } //Ugly solution to tell a common used cache-function to include inactive accounts in just this request (not present in SoeParameters)


        #endregion

        #region Cache

        /// <summary>
        /// OutputCacheDependecy key:
        /// - Is created in Global.asax Application_Start()
        /// - In PageBase Init() the OutputCache dependency key is added to all pages as CacheItemDependency
        /// - When this OutputCache dependency key is removed, all pages are removed from the OutputCache beacause of the dependency
        /// 
        /// The OutputCache dependecy key is removed when:
        /// - User is logged out
        /// - The languages is changed
        /// - A UserFavorite is added or removed
        /// </summary>
        public const string CACHE_OUTPUT_CACHE_DEPENDENCY = "OUTPUT_CACHE_DEPENDENCY";
        public const string CACHE_USERCACHECREDENTIALS_PREFIX = "User_";

        #region WCF

        public const int CACHE_XAPVERSION_IN_MINUTES = 480;
        public const int CACHE_EMPLOYEES_DICT_IN_MINUTES = 5;
        public const int CACHE_CUSTOMERS_IN_MINUTES = 5;
        public const int CACHE_INVOICEPRODUCTS_IN_MINUTES = 30;
        public const int CACHE_INVOICEPRODUCT_DTOS_IN_MINUTES = 1;
        public const int CACHE_SUPPLIERS_IN_MINUTES = 5;
        public const int CACHE_WHOLESELLERS_IN_MINUTES = 20;

        public const string CACHE_KEY_XAPVERSION = "xapversion_key";
        public const string CACHE_KEY_XAPVERSION_OLD = "xapversion_old_key";
        public const string CACHE_KEY_EMPLOYEES_DICT = "employees_dict_key_";
        public const string CACHE_KEY_CUSTOMERS = "customers_key_";
        public const string CACHE_KEY_INVOICEPRODUCTS = "invoice_products_key_";
        public const string CACHE_KEY_INVOICEPRODUCT_DTOS = "invoice_product_dtos_key_";
        public const string CACHE_KEY_SUPPLIERS = "suppliers_key_";
        public const string CACHE_KEY_WHOLESELLERS_DICT = "wholesellers_dict_key_";

        #endregion

        #endregion

        #region Crontab

        public const char CRONTAB_ALL_SELECTED = '*';
        public const char CRONTAB_ITEMTYPE_SEPARATOR = ' ';
        public const char CRONTAB_ITEM_SEPARATOR = ',';
        public const char CRONTAB_RANGE_SEPARATOR = '-';
        public const int CRONTAB_MINUTES_LOWER = 0;
        public const int CRONTAB_MINUTES_UPPER = 59;
        public const int CRONTAB_HOURS_LOWER = 0;
        public const int CRONTAB_HOURS_UPPER = 23;
        public const int CRONTAB_DAYS_LOWER = 1;
        public const int CRONTAB_DAYS_UPPER = 31;
        public const int CRONTAB_MONTHS_LOWER = 1;
        public const int CRONTAB_MONTHS_UPPER = 12;
        public const int CRONTAB_WEEKDAYS_LOWER = 1;
        public const int CRONTAB_WEEKDAYS_UPPER = 7;

        #endregion

        #region Entity / Rules

        public const int LINQMAXCOUNTCONTAINS = 100;
        public const int LINQMAXCOUNTCONTAINS_EXTENDED = 200;
        public const int LINQ_TO_SQL_MAXCONTAINS = 1500;

        #region SIE

        /// <summary>Importflagga</summary>
        public const string SIE_LABEL_FLAGGA = "#FLAGGA";
        /// <summary>Program som exporterat filen</summary>
        public const string SIE_LABEL_PROGRAM = "#PROGRAM";
        /// <summary>Teckenuppsättning som använts</summary>
        public const string SIE_LABEL_FORMAT = "#FORMAT";
        /// <summary>När och vem som generarat filen</summary>
        public const string SIE_LABEL_GEN = "#GEN";
        /// <summary>Filtyp som filen följer</summary>
        public const string SIE_LABEL_SIETYP = "#SIETYP";
        /// <summary>Kommentartext kring filens innehåll</summary>
        public const string SIE_LABEL_PROSA = "#PROSA";
        /// <summary>Internkod för exporterande företag</summary>
        public const string SIE_LABEL_FNR = "#FNR";
        /// <summary>Organisationsnummer för exporterande företag</summary>
        public const string SIE_LABEL_ORGNR = "#ORGNR";
        /// <summary>Adressuppgifter</summary>
        public const string SIE_LABEL_ADRESS = "#ADRESS";
        /// <summary>Fullständigt namn för exprterande företag</summary>
        public const string SIE_LABEL_FNAMN = "#FNAMN";
        /// <summary>Räkenskapsår för exporterat data</summary>
        public const string SIE_LABEL_RAR = "#RAR";
        /// <summary>Datum för periodsaldons omfattning</summary>
        public const string SIE_LABEL_OMFATTN = "#OMFATTN";
        /// <summary>Typ av kontoplan</summary>
        public const string SIE_LABEL_KPTYP = "#KPTYP";
        /// <summary>Kontouppgifter</summary>
        public const string SIE_LABEL_KONTO = "#KONTO";
        /// <summary>Internkonton</summary>
        public const string SIE_LABEL_OBJEKT = "#OBJEKT";
        /// <summary>Dimension (kontoroll)</summary>
        public const string SIE_LABEL_DIM = "#DIM";
        /// <summary>Kontotyp</summary>
        public const string SIE_LABEL_KTYP = "#KTYP";
        /// <summary>Verifikat</summary>
        public const string SIE_LABEL_VER = "#VER";
        /// <summary>Transaktion i verifikat</summary>
        public const string SIE_LABEL_TRANS = "#TRANS";
        /// <summary>Borttagen transaktion i verifikat</summary>
        public const string SIE_LABEL_BTRANS = "#BTRANS";
        /// <summary>Tillagd transaktion i verifikat</summary>
        public const string SIE_LABEL_RTRANS = "#RTRANS";
        /// <summary>?</summary>
        public const string SIE_LABEL_TRANSEXT = "#TRANSEXT";
        /// <summary>Ingående balans för balanskonto</summary>
        public const string SIE_LABEL_IB = "#IB";
        /// <summary>Ugående balans för balanskonto</summary>
        public const string SIE_LABEL_UB = "#UB";
        /// <summary>Ingående balans för balanskonto</summary>
        public const string SIE_LABEL_OIB = "#OIB";
        /// <summary>Ugående balans för balanskonto</summary>
        public const string SIE_LABEL_OUB = "#OUB";
        /// <summary>Sadldopost för resultatkonto</summary>
        public const string SIE_LABEL_RES = "#RES";
        /// <summary>Periodens saldo för ett visst konto</summary>
        public const string SIE_LABEL_PSALDO = "#PSALDO";
        /// <summary>Periodens budget för ett visst konto</summary>
        public const string SIE_LABEL_PBUDGET = "#PBUDGET";
        /// <summary>RSV-kod för standardiserat räkenskapsutdrag</summary>
        public const string SIE_LABEL_SRU = "#SRU";
        /// <summary>Starttecken</summary> 
        public const string SIE_LABEL_START = "{";
        /// <summary>Sluttecken</summary>
        public const string SIE_LABEL_END = "}";

        public const int PLACEMENT_DAYSTOPLACEDIRECTLY = 45;

        #endregion

        #region Reports

        public const int NOOFDIMENSIONS = 6;
        public const int NOOFTIMEACCUMULATORS = 10;

        #endregion

        #region AccountDim

        public const int ACCOUNTDIM_STANDARD = 1;
        public const int ACCOUNTDIM_NROFDIMENSIONS = 6;
        public const int ACCOUNTDIM_NROFDIMENSIONS_INTERNAL = 5;

        #endregion

        #region AttestState

        public static readonly string ATTESTSTATE_DEFAULTCOLOR = "FFFFFF";
        public static readonly int ATTESTSTATEID_SCHEDULEPLACEMENT_DUMMY = -5;

        public static readonly int ATTESTTREE_GROUP_ADDITIONAL = -1;
        public static readonly int ATTESTTREE_GROUP_ENDED = -2;
        public static readonly int ATTESTTREE_GROUP_ALL = -3;

        #endregion

        #region CompanyLogo

        public const int COMP_LOGO_MAX_HEIGHT_PX = 300;
        public const int COMP_LOGO_MAX_WIDTH_PX = 800;

        #endregion

        #region Currency

        public const int CURRENCY_SOURCE_DEFAULT = (int)TermGroup_CurrencySource.Manually;
        public const int CURRENCY_INTERVALTYPE_DEFAULT = (int)TermGroup_CurrencyIntervalType.FirstDayOfMonth;
        public const int CURRENCY_USESYSRATE_DEFAULT = 1;

        #endregion

        #region EntityHistory

        public const int ENTITY_HISTORY_INTERVAL_DEFAULT_MIN = 60;

        #endregion

        #region Employee

        public const int NO_REPLACEMENT_EMPLOYEEID = -1;
        public const int SEARCH_EMPLOYEE_EMPLOYEEID = -2;

        public const string NO_REPLACEMENT_EMPLOYEENR = "INGEN ERSÄTTARE";
        public const string HIDDENEMPLOYEENR = "LEDIG";
        public const string SOCIALSEC_ANONYMIZE = "****";

        #endregion

        #region ShiftType

        public const string SHIFT_TYPE_DEFAULT_COLOR = "#2984C3";
        public const string SHIFT_TYPE_UNSPECIFIED_COLOR = "#707070";

        #endregion

        #region TimeRule Engine

        //TimeRuleOperand
        public const string TIMERULEOPERAND_LABELPREFIX = "*";

        //TimeRuleOperand left
        public const int TIMERULEOPERAND_LEFTVALUEID_PRESENCEWITHINSCHEDULE = Int32.MaxValue - 1;
        public const int TIMERULEOPERAND_LEFTVALUEID_PRESENCE = Int32.MaxValue - 2;
        public const int TIMERULEOPERAND_LEFTVALUEID_SCHEDULE = Int32.MaxValue - 3;
        public const int TIMERULEOPERAND_LEFTVALUEID_PRESENCEBEFORESCHEDULE = Int32.MaxValue - 4;
        public const int TIMERULEOPERAND_LEFTVALUEID_PRESENCEAFTERSCHEDULE = Int32.MaxValue - 5;
        public const int TIMERULEOPERAND_LEFTVALUEID_PAYED = Int32.MaxValue - 6;
        public const int TIMERULEOPERAND_LEFTVALUEID_PAYEDBEFORESCHEDULE = Int32.MaxValue - 7;
        public const int TIMERULEOPERAND_LEFTVALUEID_PAYEDBEFORESCHEDULE_PLUS_SCHEDULE = Int32.MaxValue - 8;
        public const int TIMERULEOPERAND_LEFTVALUEID_PAYEDAFTERSCHEDULE = Int32.MaxValue - 9;
        public const int TIMERULEOPERAND_LEFTVALUEID_PAYEDAFTERSCHEDULE_PLUS_SCHEDULE = Int32.MaxValue - 10;
        public const int TIMERULEOPERAND_LEFTVALUEID_PRESENCEINSCHEDULEHOLE = Int32.MaxValue - 11;
        public const int TIMERULEOPERAND_LEFTVALUEID_SCHEDULE_PLUS_OVERTIME_OVERTIMEPERIOD = Int32.MaxValue - 12;
        public const int TIMERULEOPERAND_LEFTVALUEID_SCHEDULEANDBREAK = Int32.MaxValue - 13;

        //TimeRuleOperand right
        public const int TIMERULEOPERAND_RIGHTVALUEID_SCHEDULE = Int32.MaxValue;
        public const int TIMERULEOPERAND_RIGHTVALUEID_FULLTIMEWEEK = Int32.MaxValue - 1;

        //Absence sick
        public const int SICKNESS_RELAPSEDAYS = 5; //Återsjuknande sjukönelagen
        public const int VACATION_QUALIFYINGDAYS = 14; //Semestergrundande frånvaro semesterlagen
        public const int SICKNESS_QUALIFYINGDAYS_TO_REACH_HIGHRISCPROTECTION = 10;
        public static readonly DateTime SICKNESS_QUALIFYINGDAY_NEWRULESTART = new DateTime(2019, 1, 1);

        #endregion

        #region Stamping

        public const string CREATED_BY_AUTO_STAMP_OUT_JOB = "AutoStampOutJob";
        public const string MODIFIED_BY_AUTO_STAMP_OUT_JOB = "TimeStampConversionJob";

        #endregion

        #region Staffing

        public const int STAFFING_MAXBREAKS = 4;
        public const int STAFFING_MAXBREAKSMAJOR = 2;
        public const int STAFFING_MAXBREAKSMINOR = 4;

        #endregion

        #region TimeAccumulator

        public const int TIMEACCUMULATOR_TIMEWORKACCOUNT = -101;

        #endregion

        #region Voucher

        public const int VOUCHERROWHISTORY_MAXROWS = 1000;
        public const char VOUCHERROWHISTORY_EVENTTEXT_DELIMETER = '#';

        #endregion

        #endregion

        #region Mobile

        public const int MOBILE_NROFCLOSESTRELATIVES = 2;

        #endregion

        #region SMS

        public const int SINGLE_SMS_MAX_LENGTH = 160;

        #endregion

        #region EDI

        public const int EDI_TRANSFERTOORDER_WHOLESELLERS_NONE = -2;
        public const int EDI_TRANSFERTOORDER_WHOLESELLERS_ALL = -1;
        public const int EDI_TRANSFERTOORDER_TYPES_NONE = -2;
        public const int EDI_TRANSFERTOORDER_TYPES_ALL = 0;
        public const string EDI_TRANSFERTOORDER_NONE_STRING = "-2:-2";
        public const string EDI_TRANSFERTOORDER_ALL_STRING = "-1:-0";

        #endregion

        #region Scanning / Readsoft

        public const string READSOFT_PARTY_BUYER = "buyer";
        public const string READSOFT_PARTY_SUPPLIER = "supplier";

        public const string READSOFT_HEADERFIELD_ISCREDITINVOICE = "creditinvoice";
        public const string READSOFT_HEADERFIELD_INVOICENUMBER = "invoicenumber";
        public const string READSOFT_HEADERFIELD_INVOICEDATE = "invoicedate";
        public const string READSOFT_HEADERFIELD_INVOICEDUEDATE = "invoiceduedate";
        public const string READSOFT_HEADERFIELD_INVOICEORDERNR = "invoiceordernumber";
        public const string READSOFT_HEADERFIELD_YOURREFERENCE = "buyercontactpersonname";
        public const string READSOFT_HEADERFIELD_REFEERENCENR = "buyercontactreference";
        public const string READSOFT_HEADERFIELD_TOTALAMOUNTEXLUDEDVAT = "invoicetotalvatexcludedamount";
        public const string READSOFT_HEADERFIELD_TOTALAMOUNTVAT = "invoicetotalvatamount";
        public const string READSOFT_HEADERFIELD_TOTALAMOUNTINCUDEDVAT = "invoicetotalvatincludedamount";
        public const string READSOFT_HEADERFIELD_CURRENCY = "invoicecurrency";
        public const string READSOFT_HEADERFIELD_OCRNR = "paymentreferencenumber";
        public const string READSOFT_HEADERFIELD_PLUSGIRO = "supplieraccountnumber1";
        public const string READSOFT_HEADERFIELD_BANKGIRO = "supplieraccountnumber2";
        public const string READSOFT_HEADERFIELD_BANKNR = "supplieraccountnumber3";
        public const string READSOFT_HEADERFIELD_ORGNR = "suppliertaxnumber1";
        public const string READSOFT_HEADERFIELD_IBAN = "supplieriban1";
        public const string READSOFT_HEADERFIELD_VATRATE = "invoicetotalvatratepercent";
        public const string READSOFT_HEADERFIELD_VATNR = "suppliervatregistrationnumber";
        public const string READSOFT_HEADERFIELD_FREIGHTAMOUNT = "deliverycost";
        public const string READSOFT_HEADERFIELD_CENTROUNDING = "amountrounding";
        public const string READSOFT_HEADERFIELD_VATREGNUMBER_FIN = "vatregnumber_fin";
        public const string READSOFT_HEADERFIELD_SUPPLIERBANKCODENUMBER1 = "supplierbankcodenumber1";

        #endregion

        #region Inner class PaymentManager

        public static class PaymentManager
        {
            private static List<PaymentExportTypesDTO> supportedExportTypes;

            /// <summary>
            /// Static constructor. Fills the variable supportedExportTypes.
            /// Please read comment inside static constructorfor further info.
            /// </summary>
            static PaymentManager()
            {
                /* UPDATE THIS LIST WHEN CHANGED

                Currently the supported export types are:
                PaymentType
   PaymentMethod                LB      PG      SEPA    Autogiro    Kontant 
                PG Debit        Ok      Ok              OK          OK           
                PG Kredit               Ok              OK          OK           
                PG Interest     Ok      Ok                          OK           
                PG Claim        Ok      Ok                          OK           
                BG Debit        Ok      Ok              OK          OK           
                BG Kredit       Ok      OK              OK          OK           
                BG Interest     Ok      Ok                          OK           
                BG Claim        Ok      Ok      Ok                  OK           
                BIC Debit                       Ok                                
                BIC Kredit                      Ok                                
                BIC Interest                    Ok                                
                BIC Claim                       Ok                               
                AG Debit        Ok      Ok      Ok      OK          OK           
                AG Kredit       Ok      Ok      Ok      OK          OK           
                AG Interest     Ok      Ok      Ok                  OK           
                AG Claim        Ok      Ok      Ok                  Ok
                */

                supportedExportTypes = new List<PaymentExportTypesDTO>();

                #region PaymentMethod LB

                //PG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.PG });

                //BG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BG });

                //Bank
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.Bank });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.Bank });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.Bank });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.Bank });

                //BIC
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BIC });

                //SEPA
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.SEPA });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.SEPA });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.SEPA });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.SEPA });

                //Autogiro
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.LB, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.Autogiro });

                #endregion

                #region PaymentMethod PG

                //PG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.PG });

                //BG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BG });

                //Autogiro
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.PG, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.Autogiro });

                #endregion

                #region PaymentMethod CFP

                //PG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.PG });

                //BG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BG });

                //Bank
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.Bank });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.Bank });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.Bank });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cfp, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.Bank });

                #endregion

                #region PaymentMethod SEPA

                //BIC
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.SEPA, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.SEPA, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.SEPA, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.SEPA, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BIC });

                //Autogiro
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.SEPA, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.SEPA, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.SEPA, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.SEPA, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.Autogiro });

                #endregion

                #region PaymentMethod SEPA V3

                //PG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.PG });

                //BG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BG });

                //BIC
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.NordeaCA, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BIC });

                #endregion

                #region PaymentMethod ISO200222 (same as SEPAv3)

                //PG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.PG });

                //BG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BG });

                //BIC
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.ISO20022, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BIC });

                #endregion

                #region PaymentMethod Autogiro

                //Autogiro
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Autogiro, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Autogiro, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });

                //PG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Autogiro, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Autogiro, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.PG });

                //BG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Autogiro, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Autogiro, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BG });

                //BIC
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Autogiro, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Autogiro, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Autogiro, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Autogiro, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BIC });

                #endregion

                #region PaymentMethod Kontant

                //PG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.PG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.PG });

                //BG
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BG });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BG });

                //BIC
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.BIC });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.BIC });

                //Autogiro
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Cash, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.Autogiro });

                #endregion

                #region PaymentMethod Nets

                //Nets Direct Remittance
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Nets, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.Nets });
                //supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Nets, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.Nets });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Nets, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.Nets });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Nets, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.Nets });

                //Autogiro
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Nets, BillingType = TermGroup_BillingType.Debit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Nets, BillingType = TermGroup_BillingType.Credit, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Nets, BillingType = TermGroup_BillingType.Interest, SysPaymentType = TermGroup_SysPaymentType.Autogiro });
                supportedExportTypes.Add(new PaymentExportTypesDTO() { PaymentMethod = TermGroup_SysPaymentMethod.Nets, BillingType = TermGroup_BillingType.Reminder, SysPaymentType = TermGroup_SysPaymentType.Autogiro });

                #endregion
            }

            /// <summary>
            /// Gets a list of supported export types. Look inside the constructor for detailed comments.
            /// </summary>
            public static List<PaymentExportTypesDTO> SupportedExportTypes
            {
                get
                {
                    return supportedExportTypes;
                }
            }
        }

        #endregion

        #region WCF

        public const string WCF_CultureHeaderKey = "culture";
        public const string WCF_ActorCompanyIdHeaderKey = "actorCompanyId";
        public const string WCF_UserIdHeaderKey = "userId";
        public const string WCF_RoleIdHeaderKey = "roleId";
        public const string WCF_SupportUserIdHeaderKey = "supportUserId";
        public const string WCF_XapVersionHeaderKey = "xapVersion";

        public const string WCF_OidcToken = "oidcToken";
        public const string WCF_RequestParameters = "requestParameters";

        #endregion

        #region External WebApi

        public static readonly string SoftOneStage = "6573d15a-5875-4676-bdce-75c01a6a65c0";
        public static readonly string SoftOneUnknown = "fac09997-764f-47cd-9cfd-fd02302e3514";

        #endregion

        #region Axfood

        public static string AXFOODPRODUCTIONXMLDROP { get { return @"Axfood/Production/XML/Drop/"; } }
        public static string AXFOODPRODUCTIONSCHEDULEDROP { get { return @"Axfood/Production/Schedule/Drop/"; } }
        public static string AXFOODPRODUCTIONADATODROP { get { return @"Axfood/Production/Adato/Drop/"; } }
        public static string AXFOODTESTXMLDROP { get { return @"Axfood/Test/XML/Drop/"; } }
        public static string AXFOODPRODUCTIONAFROM { get { return @"Axfood/Production/LAS_Anstallningsformer/Drop/"; } }
        public static string AXFOODTESTAFROM { get { return @"Axfood/Test/LAS_Anstallningsformer/Drop/"; } }
        public static string AXFOODTESTSCHEDULEDROP { get { return @"Axfood/Test/Schedule/Drop/"; } }
        public static string AXFOODTESTADATODROP { get { return @"Axfood/Test/Adato/Drop/"; } }
        public static string AXFOODPRODUCTIONEDWDATATRANSFERRED { get { return @"Axfood/Production/EDWData/Transferred/"; } }
        public static string AXFOODTESTEDWDATATRANSFERRED { get { return @"Axfood/Test/EDWData/Transferred/"; } }
        public static string AXFOODPRODUCTIONEDWDATADROP { get { return @"Axfood/Production/EDWData/Drop/"; } }
        public static string AXFOODTESTEDWDATADROP { get { return @"Axfood/Test/EDWData/Drop/"; } }

        #endregion

        #region Threading

        public const int MaxDegreeOfParallelism = 2;

        #endregion
    }
}
