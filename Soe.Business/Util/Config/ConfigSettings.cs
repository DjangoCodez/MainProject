using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.IO;
using System.Transactions;

namespace SoftOne.Soe.Business.Util.Config
{
    public static class ConfigSettings
    {
        #region Server directories

        //Default
        private static string soe_server_dir_default_physical;

        public static void SetCurrentDirectory(string currentDirectory)
        {
            _currentDirectory = currentDirectory;
        }

        private static string _currentDirectory { get; set; }

        public static string SOE_SERVER_DIR_DEFAULT_PHYSICAL
        {
            get
            {
                if (!string.IsNullOrEmpty(soe_server_dir_default_physical))
                {
                    // Return the previously set value
                    return soe_server_dir_default_physical;
                }

                string currentDirectory = string.IsNullOrEmpty(_currentDirectory) ? AppDomain.CurrentDomain.BaseDirectory : _currentDirectory;

                while (!string.IsNullOrEmpty(currentDirectory))
                {
                    // Check if the current directory is the Web directory
                    if (currentDirectory.EndsWith("Soe.Web", StringComparison.OrdinalIgnoreCase) || currentDirectory.EndsWith("Web", StringComparison.OrdinalIgnoreCase))
                    {
                        soe_server_dir_default_physical = currentDirectory;
                        return currentDirectory;
                    }

                    string webDirectory = Path.Combine(currentDirectory, "Soe.Web");

                    if (Directory.Exists(webDirectory))
                    {
                        // Store the path in soe_server_dir_default_physical
                        soe_server_dir_default_physical = webDirectory;
                        return webDirectory;
                    }

                    webDirectory = Path.Combine(currentDirectory, "Web");

                    if (Directory.Exists(webDirectory))
                    {
                        // Store the path in soe_server_dir_default_physical
                        soe_server_dir_default_physical = webDirectory;
                        return webDirectory;
                    }

                    currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                }
                soe_server_dir_default_physical = SOE_SERVER_DIR_DEFAULT_PHYSICAL_OLD;
                return soe_server_dir_default_physical;
            }
        }

        public static string SOE_SERVER_DIR_DEFAULT_PHYSICAL_OLD
        {
            get
            {

                string dir = string.Empty;
#if DEBUG

                if (!string.IsNullOrEmpty(soe_server_dir_default_physical))
                    dir = soe_server_dir_default_physical;

                if (string.IsNullOrEmpty(dir) && !string.IsNullOrEmpty(_currentDirectory))
                {
                    try
                    {

                        dir = _currentDirectory.ToLower().Replace(@"newsource\", "#");

                        var arr = dir.ToLower().Split('#');

                        dir = arr[0] + @"newsource\Soe.Web\";

                        soe_server_dir_default_physical = dir;

                    }
                    catch
                    {
                        // Intentionally ignored, safe to continue
                        // NOSONAR
                    }
                }
#endif

                if (string.IsNullOrEmpty(dir) && !string.IsNullOrEmpty(_currentDirectory))
                {
                    dir = _currentDirectory;
                    if (dir.EndsWith("WebApi"))
                        dir = dir.Substring(0, dir.Length - 3);
                    else if (dir.ToLower().Contains(@"apiinternal"))
                        dir = dir.ToLower().Replace(@"apiinternal", @"web");
                }

                if (string.IsNullOrEmpty(dir))
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_SUPPRESS, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        SettingManager sm = new SettingManager(null);
                        dir = sm.GetStringSetting(SettingMainType.Application, (int)ApplicationSettingType.AppDirectory, 0, 0, 0);
                    }
                }

                dir = StringUtility.GetValidFilePath(dir);
                CheckDir(dir);

                return dir;

            }
        }

        //Report
        public static string SOE_SERVER_DIR_REPORT_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_DEFAULT_PHYSICAL + @"\reports\";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_REPORT_CONFIGFILE
        {
            get
            {
                return SOE_SERVER_DIR_REPORT_PHYSICAL;
            }
        }

        public static string SOE_SERVER_DIR_REPORT_EXTERNAL_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_REPORT_PHYSICAL + "External/";
                CheckDir(dir);
                return dir;
            }
        }

        public static string SOE_SERVER_DIR_REPORT_EXTERNAL_AXFOOD_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_REPORT_EXTERNAL_PHYSICAL + "Axfood/";
                CheckDir(dir);
                return dir;
            }
        }

        public static string SOE_SERVER_DIR_REPORT_EXTERNAL_SKV_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_REPORT_EXTERNAL_PHYSICAL + "SKV/";
                CheckDir(dir);
                return dir;
            }
        }

        public static string SOE_SERVER_DIR_REPORT_EXTERNAL_COLLECTUM_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_REPORT_EXTERNAL_PHYSICAL + "Collectum/";
                CheckDir(dir);
                return dir;
            }
        }

        public static string SOE_SERVER_DIR_REPORT_EXTERNAL_BYGGLOSEN_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_REPORT_EXTERNAL_PHYSICAL + "bygglosen/";
                CheckDir(dir);
                return dir;
            }
        }

        public static string SOE_SERVER_DIR_REPORT_SVEFAKTURA_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_REPORT_PHYSICAL + "svefaktura/";
                CheckDir(dir);
                return dir;
            }
        }


        public static string SOE_SERVER_DIR_REPORT_SVEFAKTURA_SCHEMA_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_REPORT_SVEFAKTURA_PHYSICAL + "schema/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_REPORT_EXTERNAL_SKANDIAPENSION_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_REPORT_EXTERNAL_PHYSICAL + "SkandiaPension/";
                CheckDir(dir);
                return dir;
            }
        }

        //Temp (When adding new physical path, also add logic in Global.asax.cs SetupEnv() to check directory on server start)
        public static string SOE_SERVER_DIR_TEMP_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_DEFAULT_PHYSICAL + "temp/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_EXPORT_EMAIL_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "export/email/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_EXPORT_EMAIL_RELATIVE
        {
            get
            {
                return @"/temp/export/email/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_EXPORT_PAYMENT_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "export/payment/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_EXPORT_PAYMENT_RELATIVE
        {
            get
            {
                return @"/temp/export/payment/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_EXPORT_SALARY_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + @"export\salary\";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_EXPORT_SALARY_RELATIVE
        {
            get
            {
                return @"/temp/export/salary/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_EXCEL_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "excel/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_EXCEL_RELATIVE
        {
            get
            {
                return @"/temp/excel/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_FINVOICE_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "finvoice/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_FINVOICE_REPORT_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_FINVOICE_PHYSICAL + "report/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_IMPORT_EDI_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "import/edi/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_IMPORT_EDI_RELATIVE
        {
            get
            {
                return @"/temp/import/edi/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_LOGO_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "logo/";
                CheckDir(dir);
                return dir;
            }
        }

        public static string SOE_SERVER_DIR_TEMP_PRICELIST_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "pricelist/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_PRICELIST_RELATIVE
        {
            get
            {
                return @"/temp/pricelist/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "report/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_REPORT_RELATIVE
        {
            get
            {
                return @"/temp/report/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_SIE_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "sie/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_SIE_RELATIVE
        {
            get
            {
                return @"/temp/sie/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_UPLOADEDFILES_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "uploadedfiles/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_UPLOADEDFILES_RELATIVE
        {
            get
            {
                return @"/temp/uploadedfiles/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_FI_TAX_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "fi_tax/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_FI_TAX_RELATIVE
        {
            get
            {
                return @"/temp/fi_tax/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_GM_REPORT_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "gm_report/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_GM_REPORT_RELATIVE
        {
            get
            {
                return @"/temp/gm_report/";
            }
        }
        public static string SOE_SERVER_DIR_TEMP_EXPORT_ICA_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "export/ica/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_AUTOMASTER_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "automaster/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_ICABALANCE_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "icalbalance/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_PIRATVOUCHER_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "piratvoucher/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_SAFILOCUSTOMER_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "safilocustomer/";
                CheckDir(dir);
                return dir;
            }
        }
        public static string SOE_SERVER_DIR_TEMP_SAFILOINVOICE_PHYSICAL
        {
            get
            {
                string dir = SOE_SERVER_DIR_TEMP_PHYSICAL + "safiloinvoice/";
                CheckDir(dir);
                return dir;
            }
        }
        private static void CheckDir(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        #endregion

        #region Server files

        public static string SOE_SERVER_SITEMAP
        {
            get
            {
                return SOE_SERVER_DIR_DEFAULT_PHYSICAL + "Web.sitemap";
            }
        }

        #endregion

        #region Data / Transactions



        /// <summary>
        /// (Default) A transaction is required by the scope. It uses an ambient transaction if one already exists. Otherwise, it creates a new transaction before entering the scope. This is the default value.  
        /// </summary>
        public static TransactionScopeOption TRANSACTIONSCOPEOPTION_DEFAULT
        {
            get
            {
                return TransactionScopeOption.RequiresNew;
            }
        }
        /// <summary>
        /// A new transaction is always created for the scope.  
        /// </summary>
        public static TransactionScopeOption TRANSACTIONSCOPEOPTION_ATTACH
        {
            get
            {
                return TransactionScopeOption.Required;
            }
        }
        /// <summary>
        /// The ambient transaction context is suppressed when creating the scope. All operations within the scope are done without an ambient transaction context.  
        /// </summary>
        public static TransactionScopeOption TRANSACTIONSCOPEOPTION_SUPPRESS
        {
            get
            {
                return TransactionScopeOption.Suppress;
            }
        }

        public static TransactionOptions TRANSACTIONOPTION_DEFAULT
        {
            get
            {
                ///Serializable (HIGHEST, DEFAULT): Volatile data can be read but not modified, and no new data can be added during the transaction.  
                ///RepeatableRead: Volatile data can be read but not modified during the transaction. New data can be added during the transaction.  
                ///ReadCommitted: Volatile data cannot be read during the transaction, but can be modified.  
                ///ReadUncommitted: Volatile data can be read and modified during the transaction.  
                ///Snapshot: Volatile data can be read. Before a transaction modifies data, it verifies if another transaction has changed the data after it was initially read. If the data has been updated, an error is raised. This allows a transaction to get to the previously committed value of the data.  
                ///Chaos: The pending changes from more highly isolated transactions cannot be overwritten.  
                ///Unspecified (LOWEST): A different isolation level than the one specified is being used, but the level cannot be determined. An exception is thrown if this value is set.  

                //Default in database are READ_COMMITTED_SNAPSHOT
                return new TransactionOptions()
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                };
            }
        }

        public static TransactionOptions TRANSACTIONOPTION(TimeSpan timeOut)
        {
            var config = TRANSACTIONOPTION_DEFAULT;
            config.Timeout = timeOut;
            return config;
        }

        /// <summary>Set ReadUncommitted to be able to read transactions being generated</summary>
        public static TransactionOptions TRANSACTIONOPTION_READUNCOMMITED
        {
            get
            {
                //Default in database are READ_COMMITTED_SNAPSHOT
                return new TransactionOptions()
                {
                    IsolationLevel = IsolationLevel.ReadUncommitted,
                    Timeout = new TimeSpan(0, 5, 0),
                };
            }
        }

        #endregion
    }
}
