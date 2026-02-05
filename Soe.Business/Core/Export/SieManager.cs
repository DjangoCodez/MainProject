using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.Sie;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class SieManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Parameters
        private SieImportContainer ic;
        private SieExportContainer ec;
        private int actorCompanyId;

        private List<VoucherHead> VoucherHeads;
        private Dictionary<int, string> accountTypes = null;
        private PerformanceMeasurer perfHelper = new PerformanceMeasurer($"SieManager {DateTime.UtcNow}");

        private bool UseOptimizedImport = false;
        #endregion

        #region Ctor

        public SieManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Import

        public SieImportResultDTO Import(int actorCompanyId, int userId, SieImportDTO dto)
        {
            this.UseOptimizedImport = true;

            perfHelper.Start();
            bool successAccounts = true;
            bool successVouchers = true;
            bool successAccountBalances = true;

            if (!FeatureManager.HasRolePermission(Feature.Economy_Import_Sie, Permission.Modify, RoleId, actorCompanyId))
                return null;

            var stream = new MemoryStream(dto.File.Bytes);
            var encoding = SieEncodingDetector.DetectTextFileEncoding(stream);
            StreamReader getReader() => new StreamReader(stream, encoding);
            FileImportHeadDTO fileImportHead = null;
            try
            {
                ic = new SieImportContainer();
                ic.StreamReader = getReader();

                if (ic.StreamReader != null)
                {
                    ic.UserId = UserId;
                    ic.ActorCompanyId = actorCompanyId;
                    ic.ImportType = SieImportType.Account_Voucher_AccountBalance;
                    ic.AllowNotOpenAccountYear = dto.AllowNotOpenAccountYear;

                    bool bVoucher = false;
                    bool bAccountBalance = false;

                    MemoryStream ms = new MemoryStream();

                    SetSieContainer(dto, ref ic);

                    #region Add FileImportHead

                    fileImportHead = AddFileImportHeadRecord(dto);

                    #endregion
                    if (dto.ImportAccounts)
                    {
                        stream.Position = ms.Position = 0;
                        ic.StreamReaderAccount = getReader();
                        ic.ImportType = SieImportType.Account;
                        successAccounts = Import(ic, true, false, false, dto.UseAccountDistribution, dto.SieImportPreview);
                        perfHelper.Checkpoint("ImportAccounts done");
                    }
                    if (dto.ImportVouchers)
                    {
                        stream.Position = ms.Position = 0;
                        ic.StreamReaderVoucher = getReader();
                        bVoucher = true;
                        ic.ImportType = SieImportType.Voucher;
                        successVouchers = Import(ic, false, bVoucher, false, dto.UseAccountDistribution, dto.SieImportPreview, fileImportHead?.BatchId);
                        perfHelper.Checkpoint("ImportVouchers done");
                    }
                    if (dto.ImportAccountBalances)
                    {
                        stream.Position = ms.Position = 0;
                        ic.StreamReaderAccountBalance = getReader();

                        bAccountBalance = true;
                        ic.ImportType = SieImportType.AccountBalance;
                        successAccountBalances = Import(ic, false, false, bAccountBalance, dto.UseAccountDistribution, dto.SieImportPreview);
                        perfHelper.Checkpoint("ImportAccountBalances done");
                    }

                    #region Update FileImportHead Success

                    UpdateFileImportHeadRecord(fileImportHead, TermGroup_FileImportStatus.Success, GetText(12551, "Importerad"));

                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region Update FileImportHead Error

                UpdateFileImportHeadRecord(fileImportHead, TermGroup_FileImportStatus.Error, ex.Message);

                #endregion
            }
            finally
            {
                //Error handling
                ic.CloseStream();
                ic.CloseStreamAccount();
                ic.CloseStreamVoucher();
                ic.CloseStreamAccountBalance();
            }

            bool success = successAccounts && successVouchers && successAccountBalances;
            var result = new SieImportResultDTO()
            {
                Success = success,
                Message = GetImportMessage(success, ic.ImportType, ic),
                ImportConflicts = GetConflicts(ic),
            };

            #region Update FileImportHead Error
            if (result.ImportConflicts != null && result.ImportConflicts.Count > 0)
            {
                UpdateFileImportHeadRecord(
                    fileImportHead,
                    ic.AvabSuccessImportVoucher ? TermGroup_FileImportStatus.Success : TermGroup_FileImportStatus.Error,
                    result.Message.Length > 255 ? $"{result.Message.Substring(0, 252)}..." : result.Message);
            }
            #endregion

            this.LogInfo(perfHelper.Done());

            return result;
        }

        private void UpdateFileImportHeadRecord(FileImportHeadDTO fileImportHead, TermGroup_FileImportStatus status, string systermMsg)
        {
            if (fileImportHead != null)
            {
                fileImportHead.SystemMessage = systermMsg;
                fileImportHead.Status = (int)status;

                ImportExportManager.SaveFileImportHead(fileImportHead);
            }
        }

        private FileImportHeadDTO AddFileImportHeadRecord(SieImportDTO dto)
        {
            var fileImportDto = new FileImportHeadDTO
            {
                ActorCompanyId = ActorCompanyId,
                EntityType = (int)SoeEntityType.Voucher,
                FileName = dto.File.Name,
                Status = (int)TermGroup_FileImportStatus.InProgress
            };

            var result = ImportExportManager.SaveFileImportHead(fileImportDto);
            if (result.Success)
                return ImportExportManager.GetFileImportHeadDTO(ActorCompanyId, result.IntegerValue);

            return null;
        }


        /// <summary>
        /// SIE import for AccountStd(#KONTO,#KTY), AccountInternal(#OBJEKT), AccountDim(#DIM) and Voucher(#VER,#TRANS)
        /// </summary>
        /// <param name="container">The SieImportContainer with all parameters</param>
        /// <returns>True if the Import was successfull, otherwise false</returns>
        public bool Import(SieImportContainer container, bool bAccount, bool bVoucher, bool bAccountBalance, bool useAccountDistribution = false, SieImportPreviewDTO preview = null, Guid? voucherBatchId = null)
        {
            try
            {
                #region Init

                this.ic = container;
                this.actorCompanyId = ic.ActorCompanyId;

                #endregion

                #region Prereq

                if (!bAccount && !bVoucher && !bAccountBalance)
                {
                    ic.AvabErrorMessageGeneral = "NO_IMPORTTYPE_SELECTED";
                    return false;
                }

                //Get AccountYear                
                if (bVoucher || bAccountBalance)
                {
                    ic.AccountYear = AccountManager.GetAccountYear(ic.AccountYearId);
                    if (this.ic.AccountYear == null)
                    {
                        ic.AvabErrorMessageGeneral = "ACCOUNTYEAR_MANDATORY";
                        return false;
                    }

                }

                #endregion

                #region Save

                bool success = true;
                bool previewed = preview != null;
                var dimNrsToImport = previewed ? preview.AccountDims.Where(x => x.IsImport ).Select(s => s.DimNr).ToList() : new List<int>();
                switch (ic.ImportType)
                {
                    case SieImportType.Account:
                        #region Account

                        if (!Parse(ref ic, true, false, false)) return false;

                        if (ic.ImportAccountInternal)
                        {
                            success = SaveAccountDims(dimNrsToImport).Success;
                            if (success)
                                success = SaveAccountInternals(dimNrsToImport).Success;
                        }
                        if (ic.ImportAccountStd)
                        {
                            if (success)
                                success = SaveAccountStds().Success;
                        }
                        return success;

                    #endregion
                    case SieImportType.Voucher:
                        #region Voucher

                        if (!Parse(ref ic, false, true, false)) return false;

                        PrereqVouchers();

                        //Dont save vouchers if there are conflicts
                        success = ic.NoOfVoucherConflicts() == 0;
                        if (success)
                        {
                            if (ic.OverrideVoucherDeletes || (ic.VoucherSeriesDeleteDict != null && ic.VoucherSeriesDeleteDict.Count > 0))
                                success = DeleteExistingVouchers().Success;
                            if (success)
                                success = SaveVouchers(useAccountDistribution, voucherBatchId.Value).Success;
                        }
                        return success;

                    #endregion
                    case SieImportType.AccountBalance:
                        #region AccountBalance

                        if (!Parse(ref ic, false, false, true)) return false;

                        success = SaveAccountStdBalance().Success;
                        if (success)
                            success = SaveAccountInternalBalance().Success;
                        return success;

                    #endregion
                    case SieImportType.Account_Voucher_AccountBalance:
                        #region Account_Voucher_AccountBalance

                        bool accountImportConflicts = false;

                        //Account
                        if (bAccount)
                        {
                            if (!Parse_Account_Voucher_AccountBalance(ref ic, true, false, false))
                            {
                                ic.AvabErrorMessageGeneral += "ACCOUNT_NOT_IMPORTED_PARSE_CONFLICTS";
                            }
                            else
                            {
                                if (ic.ImportAccountInternal)
                                {
                                    success = SaveAccountDims(dimNrsToImport).Success;
                                    if (success)
                                        success = SaveAccountInternals(dimNrsToImport).Success;
                                }
                                if (success && ic.ImportAccountStd)
                                    success = SaveAccountStds().Success;
                                perfHelper.Checkpoint("SaveAccounts done");
                            }

                            if (success)
                                ic.AvabSuccessImportAccount = true;
                            else
                                ic.AvabFailedImportAccount = true;

                            if (ic.NoOfConflicts() > 0)
                            {
                                accountImportConflicts = true;
                            }

                        }

                        //Voucher
                        if (bVoucher)
                        {
                            if (accountImportConflicts)
                            {
                                ic.AvabErrorMessageGeneral += "VOUCHER_NOT_IMPORTED_ACCOUNT_CONFLICTS";
                                success = false;
                            }
                            else
                            {
                                if (!Parse_Account_Voucher_AccountBalance(ref ic, false, true, false))
                                {
                                    ic.AvabErrorMessageGeneral += "VOUCHER_NOT_IMPORTED_PARSE_CONFLICTS";
                                    success = false;
                                }
                                else
                                {
                                    int numberOfConflictsBeforePrereq = ic.NoOfVoucherConflicts();

                                    PrereqVouchers();
                                    perfHelper.Checkpoint("PrereqVouchers done");


                                    //Dont save vouchers if there are conflicts
                                    success = ic.NoOfVoucherConflicts() == numberOfConflictsBeforePrereq;
                                    if (success)
                                    {
                                        if (ic.OverrideVoucherDeletes || (ic.VoucherSeriesDeleteDict != null && ic.VoucherSeriesDeleteDict.Count > 0))
                                            success = DeleteExistingVouchers().Success;
                                        if (success)
                                            success = SaveVouchers(useAccountDistribution, voucherBatchId.Value).Success;
                                    }

                                    if (success)
                                        ic.AvabSuccessImportVoucher = true;
                                    else
                                        ic.AvabFiledImportVoucher = true;
                                }
                            }
                        }

                        //AccountBalance
                        if (bAccountBalance)
                        {
                            if (!Parse_Account_Voucher_AccountBalance(ref ic, false, false, true))
                            {
                                ic.AvabErrorMessageGeneral += "ACCOUNT_BALANCE_NOT_IMPORTED_PARSE_CONFLICTS";
                                success = false;
                            }
                            else
                            {
                                success = SaveAccountStdBalance().Success;
                                if (success)
                                    success = SaveAccountInternalBalance().Success;
                                perfHelper.Checkpoint("SaveAccountBalances done");

                                if (success)
                                    ic.AvabSuccessImportAccountBalance = true;
                                else
                                    ic.AvabFailedImportAccountBalance = true;

                            }
                        }

                        return success;

                        #endregion

                }

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return false;
            }

            return true;
        }

        private string GetImportMessage(bool importSuccess, SieImportType importType, SieImportContainer ic)
        {
            // Use a list to gather all parts of the message
            string errorCodes = string.Empty;

            // Determine the base message based on success/failure
            if (importSuccess && importType != SieImportType.Account_Voucher_AccountBalance)
            {
                errorCodes = ic.HasConflicts() ? "SUCCESS_WITHCONFLICTS" : "SUCCESS";
            }
            else
            {
                switch (importType)
                {
                    case SieImportType.Account:
                        errorCodes = ic.HasConflicts() ? "FAILED_ACCOUNT_WITHCONFLICTS" : "FAILED_ACCOUNT";
                        break;
                    case SieImportType.Voucher:
                        errorCodes = ic.HasConflicts() ? "FAILED_VOUCHER_WITHCONFLICTS" : "FAILED_VOUCHER";
                        break;
                    case SieImportType.AccountBalance:
                        errorCodes = ic.HasConflicts() ? "FAILED_ACCOUNTBALANCE_WITHCONFLICTS" : "FAILED_ACCOUNTBALANCE";
                        break;
                    case SieImportType.Account_Voucher_AccountBalance:
                        //SUCCESS
                        if (ic.AvabSuccessImportAccount)
                            errorCodes += "SUCCESS_AVAB_ACCOUNT_IMPORT";
                        if (ic.AvabSuccessImportVoucher)
                            errorCodes += "SUCCESS_AVAB_VOUCHER_IMPORT";
                        if (ic.AvabSuccessImportAccountBalance)
                            errorCodes += "SUCCESS_AVAB_BALANCE_IMPORT";

                        //FAILED
                        if (ic.AvabFailedImportAccount)
                            errorCodes += "FAILED_AVAB_ACCOUNT_IMPORT";
                        if (ic.AvabFiledImportVoucher)
                            errorCodes += "FAILED_AVAB_VOUCHER_IMPORT";
                        if (ic.AvabFailedImportAccountBalance)
                            errorCodes += "FAILED_AVAB_BALANCE_IMPORT";
                        //ERRORMESSAGE GENERAL
                        if (ic.AvabErrorMessageGeneral != null)
                        {
                            if (ic.AvabErrorMessageGeneral.Contains("NO_IMPORTTYPE_SELECTED"))
                                errorCodes += "FAILED_AVAB_NO_IMPORTTYPE_SELECTED";
                            if (ic.AvabErrorMessageGeneral.Contains("ACCOUNTYEAR_MANDATORY"))
                                errorCodes += "FAILED_AVAB_ACCOUNTYEAR_MANDATORY";
                            if (ic.AvabErrorMessageGeneral.Contains("VOUCHER_NOT_IMPORTED_ACCOUNT_CONFLICTS"))
                                errorCodes += "FAILED_AVAB_VOUCHER_NOT_IMPORTED_ACCOUNT_CONFLICTS";
                            if (ic.AvabErrorMessageGeneral.Contains("VOUCHER_NOT_IMPORTED_PARSE_CONFLICTS"))
                                errorCodes += "FAILED_AVAB_VOUCHER_NOT_IMPORTED_PARSE_CONFLICTS";
                            if (ic.AvabErrorMessageGeneral.Contains("ACCOUNT_NOT_IMPORTED_PARSE_CONFLICTS"))
                                errorCodes += "FAILED_AVAB_ACCOUNT_NOT_IMPORTED_PARSE_CONFLICTS";
                            if (ic.AvabErrorMessageGeneral.Contains("ACCOUNT_BALANCE_NOT_IMPORTED_PARSE_CONFLICTS"))
                                errorCodes += "FAILED_AVAB_ACCOUNT_BALANCE_NOT_IMPORTED_PARSE_CONFLICTS";
                        }
                        //DEFAULT
                        if (errorCodes == "")
                        {
                            if (importSuccess)
                                errorCodes = ic.HasConflicts() ? "SUCCESS_WITHCONFLICTS" : "SUCCESS";
                            else
                                errorCodes = ic.HasConflicts() ? "FAILED_WITHCONFLICTS" : "FAILED";
                        }

                        break;
                    default:
                        errorCodes = ic.HasConflicts() ? "FAILED_WITHCONFLICTS" : "FAILED";
                        break;
                }
            }

            // Combine all the message parts into one string.
            return ImportErrorCodesToMessage(errorCodes);
        }

        private string ImportErrorCodesToMessage(string errorCodes)
        {
            string message = string.Empty;
            if (!String.IsNullOrEmpty(errorCodes))
            {
                if (errorCodes == "SUCCESS")
                    message = GetText(1168, "SIE import klar");
                if (errorCodes == "SUCCESS_WITHCONFLICTS")
                    message = GetText(1168, "SIE import klar") + ". " + GetText(1169, "Konflikter uppstod");
                if (errorCodes == "FAILED")
                    message = GetText(1170, "SIE import misslyckades");
                if (errorCodes == "FAILED_WITHCONFLICTS")
                    message = GetText(1170, "SIE import misslyckades") + ". " + GetText(1169, "Konflikter uppstod");
                if (errorCodes == "FAILED_ACCOUNT")
                    message = GetText(1170, "SIE import misslyckades") + ". " + GetText(1643, "Konton kunde inte läsas");
                if (errorCodes == "FAILED_ACCOUNT_WITHCONFLICTS")
                    message = GetText(1170, "SIE import misslyckades") + ". " + GetText(1169, "Konflikter uppstod") + ". " + GetText(1643, "Konton kunde inte läsas");
                if (errorCodes == "FAILED_VOUCHER")
                    message = GetText(1170, "SIE import misslyckades") + ". " + GetText(1206, "Verifikat kunde inte läsas in");
                if (errorCodes == "FAILED_VOUCHER_WITHCONFLICTS")
                    message = GetText(1170, "SIE import misslyckades") + ". " + GetText(1169, "Konflikter uppstod") + ". " + GetText(1206, "Verifikat kunde inte läsas in");
                if (errorCodes == "FAILED_ACCOUNTBALANCE")
                    message = GetText(1170, "SIE import misslyckades") + ". " + GetText(1644, "Ingående balanser kunde inte läsas in");
                if (errorCodes == "FAILED_ACCOUNTBALANCE_WITHCONFLICTS")
                    message = GetText(1170, "SIE import misslyckades") + ". " + GetText(1169, "Konflikter uppstod") + ". " + GetText(1644, "Ingående balanser kunde inte läsas in");
                if (errorCodes == "FILENOTFOUND")
                    message = GetText(1179, "Filen hittades inte");
                if (errorCodes == "ACCOUNTYEAR_MANDATORY")
                    message = GetText(1639, "Du måste ange redovisningsår");

                string avab_message = "";
                if (errorCodes.Contains("FAILED_AVAB_NO_IMPORTTYPE_SELECTED"))
                    avab_message += " " + GetText(9250, "Ingen importtyp vald") + ".";
                if (errorCodes.Contains("FAILED_AVAB_ACCOUNTYEAR_MANDATORY"))
                    avab_message += " " + GetText(1639, "Du måste ange redovisningsår.");

                if (errorCodes.Contains("SUCCESS_AVAB_ACCOUNT_IMPORT"))
                    avab_message += " " + GetText(9251, "Konto import klar") + ".";
                if (errorCodes.Contains("FAILED_AVAB_ACCOUNT_IMPORT"))
                    avab_message += " " + GetText(9252, "Konto import misslyckades") + ".";
                if (errorCodes.Contains("FAILED_AVAB_ACCOUNT_NOT_IMPORTED_PARSE_CONFLICTS"))
                    avab_message += " " + GetText(9253, "Konto inte importerat, kunde inte läsas in") + ".";
                if (errorCodes.Contains("SUCCESS_AVAB_VOUCHER_IMPORT"))
                    avab_message += " " + GetText(9254, "Verifikat import klar") + ".";
                if (errorCodes.Contains("FAILED_AVAB_VOUCHER_IMPORT"))
                    avab_message += " " + GetText(9255, "Verifikat import misslyckades") + ".";
                if (errorCodes.Contains("FAILED_AVAB_VOUCHER_NOT_IMPORTED_ACCOUNT_CONFLICTS"))
                    avab_message += " " + GetText(9256, "Verifikat inte importerade, konflikter i Konto importen") + ".";
                if (errorCodes.Contains("FAILED_AVAB_VOUCHER_NOT_IMPORTED_PARSE_CONFLICTS"))
                    avab_message += " " + GetText(9257, "Verifikat inte importerade, kunde inte läsas in") + ".";
                if (errorCodes.Contains("SUCCESS_AVAB_BALANCE_IMPORT"))
                    avab_message += " " + GetText(9258, "Ingående balans import klar") + ".";
                if (errorCodes.Contains("FAILED_AVAB_BALANCE_IMPORT"))
                    avab_message += " " + GetText(9259, "Ingående balans import misslyckades") + ".";
                if (errorCodes.Contains("FAILED_AVAB_ACCOUNT_BALANCE_NOT_IMPORTED_PARSE_CONFLICTS"))
                    avab_message += " " + GetText(9260, "Ingående balans inte importerad, kunde inte läsas in") + ".";

                if (avab_message != "")
                {
                    message = avab_message;
                }
            }
            return message;
        }

        private List<SieImportConflictDTO> GetConflicts(SieImportContainer ic)
        {
            var conflictDTOs = new List<SieImportConflictDTO>();
            var conflicts = ic.GetAllConflicts();

            foreach (var item in conflicts)
            {
                conflictDTOs.Add(new SieImportConflictDTO
                {
                    RowNr = item.LineNr,
                    Label = item.Label,
                    Conflict = SetImportConflictText(item),
                    Value = SetImportConflictValueText(item)
                });

            }

            return conflictDTOs;
        }

        private string SetImportConflictValueText(SieImportItemBase sieImportItem)
        {
            string text = string.Empty;

            switch (sieImportItem.Label)
            {
                case Constants.SIE_LABEL_DIM:
                    SieAccountDimItem accountDimItem = sieImportItem as SieAccountDimItem;
                    if (accountDimItem != null)
                    {
                        text = GetText(1173, "Dimension") + ":" + accountDimItem.AccountDimNr + ", " +
                                    GetText(1190, "Namn") + ":" + accountDimItem.Name;
                    }
                    break;
                case Constants.SIE_LABEL_KONTO:
                    SieAccountStdItem accountStdItem = sieImportItem as SieAccountStdItem;
                    if (accountStdItem != null)
                    {
                        text = GetText(1166, "Kontonr") + ":" + accountStdItem.AccountNr + ", " +
                                    GetText(1167, "Namn") + ":" + accountStdItem.Name + ", " +
                                    GetText(1165, "Kontotyp") + ":" +
                                    (accountStdItem.AccountType != null ? GetAccountTypeText(Convert.ToInt32(accountStdItem.AccountType)) : String.Empty);
                    }
                    break;
                case Constants.SIE_LABEL_OBJEKT:
                    SieAccountInternalItem accountInternalItem = sieImportItem as SieAccountInternalItem;
                    if (accountInternalItem != null)
                    {
                        text = GetText(1173, "Dimension") + ":" + accountInternalItem.AccountDimNr + ", " +
                                    GetText(1188, "Objektkod") + ":" + accountInternalItem.ObjectCode + ", " +
                                    GetText(1189, "Objektnamn") + ":" + accountInternalItem.Name;
                    }
                    break;
                case Constants.SIE_LABEL_IB:
                    SieAccountStdInBalanceItem accountStdInBalanceItem = sieImportItem as SieAccountStdInBalanceItem;
                    if (accountStdInBalanceItem != null)
                    {
                        text = GetText(1394, "År") + ":" + accountStdInBalanceItem.AccountYear + ", " +
                                    GetText(1395, "Kontonr") + ":" + accountStdInBalanceItem.AccountNr + ", " +
                                    GetText(1396, "Saldo") + ":" + accountStdInBalanceItem.Balance + ", " +
                                    GetText(1397, "Kvantitet") + ":" + accountStdInBalanceItem.Quantity != null ? Convert.ToDecimal(accountStdInBalanceItem.Quantity).ToString() : String.Empty;
                    }
                    break;
                case Constants.SIE_LABEL_VER:
                    SieVoucherItem voucherItem = sieImportItem as SieVoucherItem;
                    if (voucherItem != null)
                    {
                        text = GetText(1198, "Serie") + ":" + voucherItem.VoucherSeriesTypeNr + ", " +
                                    GetText(1199, "Nr") + ":" + voucherItem.VoucherNr + ", " +
                                    GetText(1200, "Datum") + ":" + voucherItem.VoucherDate + ", " +
                                    GetText(1201, "Text") + ":" + voucherItem.Text + ", " +
                                    GetText(1202, "Balans") + ":" + voucherItem.Balance;
                    }
                    break;
            }
            return text;
        }

        private string SetImportConflictText(SieImportItemBase sieImportItem)
        {
            string text = string.Empty;
            foreach (SieConflictItem sieConflictItem in sieImportItem.Conflicts)
            {
                string conflict = "";
                switch (sieConflictItem.Conflict)
                {
                    //General
                    case SieConflict.Exception:
                        conflict = sieConflictItem.StrData;
                        break;

                    //Import
                    case SieConflict.Import_NameConflict:
                        INameItem nameItem = sieImportItem as INameItem;
                        if (nameItem != null)
                            conflict = GetText(1176, "Namnkonflikt");
                        conflict = conflict + " [" + nameItem.OriginalName + "-->" + nameItem.Name + "]";
                        break;
                    case SieConflict.Import_AddFailed:
                        conflict = GetText(1184, "Kunde inte lägga till");
                        break;
                    case SieConflict.Import_UpdateFailed:
                        conflict = GetText(1185, "Kunde inte uppdatera");
                        break;

                    //Syntax
                    case SieConflict.Import_InvalidLine:
                        conflict = GetText(1196, "Felaktig syntax") + " [" + sieConflictItem.Line + "]";
                        break;

                    //MandatoryFieldMissing
                    case SieConflict.Import_MandatoryFieldMissing_Dim_dimensionsnr:
                        conflict = GetText(1208, "Obligatoriska fältet dimensionsnr saknas på dimension");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_Dim_namn:
                        conflict = GetText(1209, "Obligatoriska fältet namn saknas på dimension");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_Objekt_dimensionsnr:
                        conflict = GetText(1210, "Obligatoriska fältet dimensionsnr saknas på dimension");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_Objekt_objektkod:
                        conflict = GetText(1211, "Obligatoriska fältet objektkod saknas på objekt");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_Objekt_objektnamn:
                        conflict = GetText(1212, "Obligatoriska fältet objektnamn saknas på objekt");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_Konto_kontonr:
                        conflict = GetText(1213, "Obligatoriska fältet kontonr saknas på konto");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_Konto_kontonamn:
                        conflict = GetText(1214, "Obligatoriska fältet namn saknas på konto");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_Ktyp_kontotyp:
                        conflict = GetText(1215, "Obligatoriska fältet kontotyp saknas");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_Ver_verdatum:
                        conflict = GetText(1197, "Obligatoriska fältet verifikatdatum saknas på verifikat");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_Trans_kontonr:
                        conflict = GetText(1203, "Obligatoriska fältet kontonr saknas på transaktion");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_Trans_belopp:
                        conflict = GetText(1204, "Obligatoriska fältet belopp saknas på transaktion");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_AccountBalance_accountyear:
                        conflict = GetText(1484, "Obligatoriska fältet redovisningsår saknas på ingående balans");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_AccountBalance_accountnr:
                        conflict = GetText(1485, "Obligatoriska fältet kontonr saknas på ingående balans");
                        break;
                    case SieConflict.Import_MandatoryFieldMissing_AccountBalance_balance:
                        conflict = GetText(1486, "Obligatoriska fältet saldo saknas på ingående balans");
                        break;

                    case SieConflict.Import_DimNotFound:
                        conflict = GetText(1177, "Kontodimension hittades inte");
                        conflict = sieConflictItem.IntData.HasValue ? conflict + " [" + sieConflictItem.IntData + "]" : conflict;
                        break;

                    //AccountInternal
                    case SieConflict.Import_ObjectRuleFailed:
                        conflict = GetText(1221, "Objekt uppfyller inte regler för sin dimension");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;

                    //AccountStd
                    case SieConflict.Import_AccountRuleFailed:
                        conflict = GetText(1222, "Konto uppfyller inte regler för sin dimension");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;
                    case SieConflict.Import_AccountHasNoAccountType:
                        conflict = GetText(1183, "Konto har ingen kontotyp");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;

                    //AccountBalance
                    case SieConflict.Import_AccountBalanceHasUnknownAccountYear:
                        conflict = GetText(1392, "Ingående balans har ogiltigt redovisningsår");
                        conflict = sieConflictItem.IntData.HasValue ? conflict + " [" + sieConflictItem.IntData + "]" : conflict;
                        break;
                    case SieConflict.Import_AccountBalanceExistInAccountYear:
                        conflict = GetText(1393, "Ingånde balans finns redan för angivet konto och redovisningsår");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;
                    case SieConflict.Import_AccountBalanceUseUBInsteadOfIB_PreviousYearNotInFile:
                        conflict = GetText(5980, "Kan ej använda UB istället för IB. UB för föregående år finns ej i filen");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;

                    //Voucher and Transactions
                    case SieConflict.Import_VoucherHasNoStartLabel:
                        conflict = GetText(1192, "Verifikat saknar startklammer för transaktioner");
                        conflict += " [ { ]";
                        break;
                    case SieConflict.Import_VoucherHasNoEndLabel:
                        conflict = GetText(1193, "Verifikat saknar slutklammer för transaktioner");
                        conflict += " [ } ]";
                        break;
                    //case SieConflict.Import_VoucherHasNoTransactions:
                    //    conflict = GetText(1194, "Verifikat saknar transaktioner");
                    //    break;
                    //case SieConflict.Import_VoucherHasInvalidBalance:
                    //    conflict = GetText(1195, "Verifikat har felaktig balans");
                    //    conflict = (sieConflictItem.DataDec != null) ? conflict + " [" + sieConflictItem.DataDec + "]" : conflict;
                    //    break;
                    case SieConflict.Import_VoucherSeriesTypeDoesNotExist:
                        conflict = GetText(1205, "Angiven serie i verifikat har ingen matchande serietyp, måste läggas upp manuellt");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;
                    case SieConflict.Import_VoucherHasNoVoucherNr:
                        conflict = GetText(1175, "Angivet verifikat har inget verifikatnr");
                        break;
                    case SieConflict.Import_VoucherAlreadyExist:
                        conflict = GetText(1073, "Angivet verifikat finns redan i aktuell verifikatserie");
                        conflict = sieConflictItem.IntData.HasValue ? conflict + " [" + sieConflictItem.IntData + "]" : conflict;
                        break;
                    case SieConflict.Import_VouchersAccountYearDoesNotMatchAccountYearDefault:
                        conflict = GetText(1220, "Angivet verifikats redovisningsår matchar inte valt redovisningsår");
                        conflict = sieConflictItem.DateData.HasValue ? conflict + " [" + sieConflictItem.DateData + "]" : conflict;
                        break;
                    case SieConflict.Import_VouchersAccountYearDoesNotExist:
                        conflict = GetText(1216, "Matchande redovisningsår finns inte, måste läggas upp manuellt");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;
                    case SieConflict.Import_VouchersAccountPeriodDoesNotExist:
                        conflict = GetText(1217, "Matchande period finns inte, måste läggas upp manuellt");
                        conflict = sieConflictItem.DateData.HasValue ? conflict + " [" + sieConflictItem.DateData + "]" : conflict;
                        break;
                    case SieConflict.Import_VouchersAccountPeriodIsNotOpen:
                        conflict = GetText(11969, "Angiven period är ej öppen, måste öppnas först");
                        conflict = sieConflictItem.DateData.HasValue ? conflict + " [" + sieConflictItem.DateData + "]" : conflict;
                        break;
                    case SieConflict.Import_VouchersAccountDoesNotExist:
                        conflict = GetText(1218, "Angivet konto i transaktion finns inte");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;
                    case SieConflict.Import_VouchersObjectDoesNotExist:
                        conflict = GetText(1219, "Angivet internkonto i transaktion finns inte");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;

                    //AccountStdBalance
                    case SieConflict.Import_AccountDoesNotExist:
                        conflict = GetText(1637, "Angivet konto finns inte");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;

                    //AccountYear
                    case SieConflict.Import_AccountYearIsNotOpen:
                        conflict = GetText(1434, "Angivet redovisningsår är inte öppet");
                        conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                        break;
                }
                if (String.IsNullOrEmpty(text))
                    text = conflict;
                else
                    text += conflict;
            }
            return text;
        }

        private string GetAccountTypeText(int accountType)
        {
            if (accountTypes == null)
                accountTypes = GetTermGroupDict(TermGroup.AccountType, this.GetLangId());
            return accountTypes[accountType];
        }

        private void SetSieContainer(SieImportDTO dto, ref SieImportContainer ic)
        {
            SetSieContainerForAccount(dto, ref ic);
            SetSieContainerForVoucher(dto, ref ic);
            SetSieContainerForAccountBalance(dto, ref ic);
        }

        private void SetSieContainerForAccount(SieImportDTO dto, ref SieImportContainer ic)
        {
            ic.OverwriteNameConflicts = dto.OverrideNameConflicts;
            ic.ApproveEmptyAccountNames = dto.ApproveEmptyAccountNames;
            ic.ImportAccountStd = dto.ImportAccountStd;
            ic.ImportAccountInternal = dto.ImportAccountInternal;
            ic.EmptyAccountName = "[" + GetText(1675, "Kontonamn saknas") + "]";
        }

        private void SetSieContainerForVoucher(SieImportDTO dto, ref SieImportContainer ic)
        {
            int voucherSeriesId = dto.DefaultVoucherSeriesId.GetValueOrDefault();
            if (voucherSeriesId > 0)
            {
                ic.DefaultVoucherSeriesId = voucherSeriesId;
                ic.OverrideVoucherSeries = false;
            }

            var voucherSeriesMappingItems = dto.VoucherSeriesTypesMappingDict;
            if (voucherSeriesMappingItems != null && voucherSeriesMappingItems.Count > 0)
            {
                ic.VoucherSeriesTypesMappingDict = new Dictionary<string, int>();
                foreach (var item in voucherSeriesMappingItems)
                {
                    ic.VoucherSeriesTypesMappingDict.Add(item.Key.ToLower(), item.Value);
                }
            }
            ic.SkipAlreadyExistingVouchers = dto.SkipAlreadyExistingVouchers;
            ic.OverrideVoucherDeletes = dto.OverrideVoucherSeriesDelete;
            ic.TakeVoucherNrFromSeries = dto.TakeVoucherNrFromSeries;
            var voucherSeriesDeleteItems = dto.VoucherSeriesDelete;
            if (voucherSeriesDeleteItems != null && voucherSeriesDeleteItems.Count > 0)
            {
                ic.VoucherSeriesDeleteDict = new Dictionary<int, bool>();
                foreach (var item in voucherSeriesDeleteItems)
                {
                    ic.VoucherSeriesDeleteDict.Add(item, true);
                }
            }
        }

        private void SetSieContainerForAccountBalance(SieImportDTO dto, ref SieImportContainer ic)
        {
            ic.AccountYearId = dto.AccountYearId;
            ic.OverrideAccountBalance = dto.OverrideAccountBalance;
            ic.UseUBInsteadOfIB = dto.UseUBInsteadOfIB;
        }


        #region Parse (string validation)

        public SieImportPreviewDTO SieImportPreview(int actorCompanyId, FileDTO file)
        {
            if (!FeatureManager.HasRolePermission(Feature.Economy_Import_Sie, Permission.Readonly, RoleId, actorCompanyId))
                return null;

            var stream = new MemoryStream(file.Bytes);
            var encoding = SieEncodingDetector.DetectTextFileEncoding(stream);

            this.ic = new SieImportContainer();
            ic.StreamReader = new StreamReader(stream, encoding);
            ic.ActorCompanyId = actorCompanyId;
            ic.ImportAccountStd = true;
            ic.ImportAccountInternal = true;

            if (Parse(ref ic, true, true, true))
            {
                var dto = new SieImportPreviewDTO();
                SetAccountingYear(actorCompanyId, dto);
                SetVoucherMapping(actorCompanyId, dto);
                SetAccounts(actorCompanyId, dto);
                SetIngoingBalance(actorCompanyId, dto);
                SetConflicts(dto, GetConflicts(ic));
                return dto;
            }

            return null;
        }

        private void SetAccountingYear(int actorCompanyId, SieImportPreviewDTO dto)
        {
            var accountYear = ic.AccountYearItems.FirstOrDefault(y => y.IsCurrentYear);
            if (accountYear == null)
                return;

            dto.AccountingYearFrom = accountYear.FromDate;
            dto.AccountingYearTo = accountYear.ToDate;

            var year = AccountManager.GetAccountYear(accountYear.FromDate, actorCompanyId);
            if (year != null)
            {
                dto.AccountingYearId = year.AccountYearId;
                dto.AccountingYearIsClosed = (year.Status == (int)TermGroup_AccountYearStatus.Closed || year.Status == (int)TermGroup_AccountYearStatus.Locked);
            }
        }
        private void SetVoucherMapping(int actorCompanyId, SieImportPreviewDTO dto)
        {
            var voucherSerieTypes = VoucherManager.GetVoucherSeriesTypes(actorCompanyId, false);
            var voucherSeriesDict = new Dictionary<string, SieVoucherSeriesMappingDTO>();
            this.ic.VoucherItems.ForEach(v =>
            {
                if (!voucherSeriesDict.ContainsKey(v.VoucherSeriesTypeNr))
                {
                    voucherSeriesDict.Add(v.VoucherSeriesTypeNr, new SieVoucherSeriesMappingDTO
                    {
                        Number = v.VoucherSeriesTypeNr,
                        VoucherNrFrom = v.VoucherNr.GetValueOrDefault(),
                        VoucherNrTo = v.VoucherNr.GetValueOrDefault(),
                        VoucherSeriesTypeId = voucherSerieTypes
                                .FirstOrDefault(t => t.VoucherSeriesTypeNr.ToString() == v.VoucherSeriesTypeNr)?.VoucherSeriesTypeId ?? 0
                    });
                }
                else
                {
                    var voucherSeries = voucherSeriesDict[v.VoucherSeriesTypeNr];
                    if (v.VoucherNr < voucherSeries.VoucherNrFrom)
                    {
                        voucherSeries.VoucherNrFrom = v.VoucherNr.GetValueOrDefault();
                    }
                    if (v.VoucherNr > voucherSeries.VoucherNrTo)
                    {
                        voucherSeries.VoucherNrTo = v.VoucherNr.GetValueOrDefault();
                    }
                }
            });

            dto.VoucherSeriesMappings = voucherSeriesDict.Values.ToList();
            dto.FileContainsVouchers = this.ic.VoucherItems.Count() > 0;
        }
        private void SetAccounts(int actorCompanyId, SieImportPreviewDTO dto)
        {
            var accountDims = AccountManager.GetAccountDimsByCompany(actorCompanyId,
                onlyStandard: false,
                onlyInternal: false,
                active: null,
                loadAccounts: true,
                loadInternalAccounts: true,
                loadParentOrCalculateLevels: false);

            dto.FileContainsAccountStd = this.ic.AccountStdItems.Count() > 0;


            this.ic.AccountDimItems.ForEach(d =>
            {
                var accountDim = accountDims.FirstOrDefault(a => a.AccountDimNr == d.AccountDimNr);

                var lookup = new Dictionary<string, Account>();
                if (accountDim != null)
                {
                    foreach (var account in accountDim.Account.OrderBy(a => a.State))
                    {
                        if (!lookup.ContainsKey(account.AccountNr))
                            lookup.Add(account.AccountNr, account);
                    }
                }

                var accountMappings = new List<SieAccountMappingDTO>();

                accountMappings = ic.AccountInternalItems.Where(i => i.AccountDimNr == d.AccountDimNr)
                    .Select(a =>
                    {
                        int? accountId = null;
                        if (lookup.ContainsKey(a.ObjectCode))
                            accountId = lookup[a.ObjectCode].AccountId;

                        return new SieAccountMappingDTO
                        {
                            Name = a.Name,
                            Number = a.ObjectCode,
                            AccountId = accountId
                        };
                    })
                    .ToList();



                dto.AccountDims.Add(new SieAccountDimMappingDTO
                {
                    DimNr = d.AccountDimNr.Value,
                    Name = d.Name,
                    AccountDimId = accountDim?.AccountDimId,
                    IsAccountStd = false,
                    AccountMappings = accountMappings
                });
            });

            var accountStdMappings = ic.AccountStdItems
                     .Select(a =>
                     {
                         return new SieAccountMappingDTO
                         {
                             Name = a.Name,
                             Number = a.AccountNr,
                             AccountId = null
                         };
                     })
                     .ToList();
                       
            var accountDimStd = AccountManager.GetAccountDimStd(actorCompanyId);
            if (accountDimStd != null)
            {
                dto.AccountStd = new SieAccountDimMappingDTO
                {
                    DimNr = accountDimStd.AccountDimNr,
                    Name = accountDimStd.Name,
                    AccountDimId = null,
                    IsAccountStd = true,
                    AccountMappings = accountStdMappings
                };
            }

        }
        private void SetIngoingBalance(int actorCompanyId, SieImportPreviewDTO dto)
        {
            dto.FileContainsAccountBalances = this.ic.AccountStdOutBalanceItems.Count() > 0 || this.ic.AccountStdInBalanceItems.Count() > 0;
        }
        private void SetConflicts(SieImportPreviewDTO dto, List<SieImportConflictDTO> conflicts)
        {
            dto.Conflicts = conflicts;
        }

        private bool Parse(ref SieImportContainer container, bool bAccount, bool bVoucher, bool bAccountBalance)
        {
            int numberOfConflictsBeforeParse = container.NoOfConflicts();

            container.ResetReadLine();
            string line = container.ReadLine();
            while (line != null)
            {
                try
                {
                    if (String.IsNullOrEmpty(line))
                        goto Next;

                    if (container.LineNr == 1 && !line.StartsWith("#"))
                    {
                        return false;
                    }

                    this.ParseLine(line, bAccount, bVoucher, bAccountBalance);
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                }

            Next:
                line = container.ReadLine();
            }

            //Dont save if there is syntax conflicts
            if (container.NoOfConflicts() > numberOfConflictsBeforeParse)
            {
                return false;
            }

            return true;

        }

        private bool Parse_Account_Voucher_AccountBalance(ref SieImportContainer container, bool bAccount, bool bVoucher, bool bAccountBalance)
        {
            int numberOfConflictsBeforeParse = container.NoOfConflicts();

            string line = string.Empty;
            container.ResetReadLine();

            if (bAccount)
                line = container.ReadLineAccount();
            else if (bVoucher)
                line = container.ReadLineVoucher();
            else if (bAccountBalance)
                line = container.ReadLineAccountBalance();

            while (line != null)
            {
                try
                {
                    if (String.IsNullOrEmpty(line))
                        goto Next;

                    if (container.LineNr == 1 && !line.StartsWith("#"))
                    {
                        return false;
                    }

                    this.ParseLine_Account_Voucher_AccountBalance(line, bAccount, bVoucher, bAccountBalance);
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                }

            Next:
                if (bAccount)
                    line = container.ReadLineAccount();
                else if (bVoucher)
                    line = container.ReadLineVoucher();
                else if (bAccountBalance)
                    line = container.ReadLineAccountBalance();
            }

            perfHelper.Checkpoint("Parse_Account_Voucher_AccountBalance");

            //Dont save if there is syntax conflicts
            if (container.NoOfConflicts() > numberOfConflictsBeforeParse)
            {
                return false;
            }

            return true;

        }


        private void ParseLine(string line, bool bAccount, bool bVoucher, bool bAccountBalance)
        {
            bool success = false;

            try
            {
                if (LineStartsWith(line, Constants.SIE_LABEL_DIM) && bAccount && ic.ImportAccountInternal)
                    success = this.ParseAccountDim(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_OBJEKT) && bAccount && ic.ImportAccountInternal)
                    success = this.ParseAccountInternal(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_KONTO) && bAccount && ic.ImportAccountStd)
                    success = this.ParseAccountStd(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_KTYP) && bAccount && ic.ImportAccountStd)
                    success = this.ParseAccountStdType(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_KPTYP) && bAccount && ic.ImportAccountStd)
                    success = this.ParseChartOfAccountsType(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_SRU) && bAccount && ic.ImportAccountStd)
                    success = this.ParseAccountSRU(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_VER) && bVoucher)
                    success = this.ParseVoucher(line);
                else if (ic.UseUBInsteadOfIB ? (LineStartsWith(line, Constants.SIE_LABEL_UB) && bAccountBalance) : (LineStartsWith(line, Constants.SIE_LABEL_IB) && bAccountBalance))
                    success = this.ParseAccountStdBalance(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_OIB) && bAccountBalance)
                    success = this.ParseAccountInternalBalance(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_RAR))
                    success = this.ParseAccountYear(line);
                else
                    success = true;

            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                success = false;
            }
            finally
            {
                if (!success)
                {
                    //Syntax conflict: Invalid Line
                    SieSyntaxItem syntaxItem = new SieSyntaxItem(line, ic.LineNr);
                    syntaxItem.AddConflict(SieConflict.Import_InvalidLine);
                    ic.SyntaxItems.Add(syntaxItem);
                }
            }
        }

        private void ParseLine_Account_Voucher_AccountBalance(string line, bool bAccount, bool bVoucher, bool bAccountBalance)
        {
            bool success = false;

            try
            {
                if (LineStartsWith(line, Constants.SIE_LABEL_DIM) && bAccount && ic.ImportAccountInternal)
                    success = this.ParseAccountDim(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_OBJEKT) && bAccount && ic.ImportAccountInternal)
                    success = this.ParseAccountInternal(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_KONTO) && bAccount && ic.ImportAccountStd)
                    success = this.ParseAccountStd(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_KTYP) && bAccount && ic.ImportAccountStd)
                    success = this.ParseAccountStdType(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_KPTYP) && bAccount && ic.ImportAccountStd)
                    success = this.ParseChartOfAccountsType(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_VER) && bVoucher)
                    success = this.ParseVoucher_Account_Voucher_AccountBalance(line);
                else if (ic.UseUBInsteadOfIB ? (LineStartsWith(line, Constants.SIE_LABEL_UB) && bAccountBalance) : (LineStartsWith(line, Constants.SIE_LABEL_IB) && bAccountBalance))
                    success = this.ParseAccountStdBalance(line);
                else if (LineStartsWith(line, Constants.SIE_LABEL_OIB) && bAccountBalance)
                    success = this.ParseAccountInternalBalance(line);
                else
                    success = true;

            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                success = false;
            }
            finally
            {
                if (!success)
                {
                    //Syntax conflict: Invalid Line
                    SieSyntaxItem syntaxItem = new SieSyntaxItem(line, ic.LineNr);
                    syntaxItem.AddConflict(SieConflict.Import_InvalidLine);
                    ic.SyntaxItems.Add(syntaxItem);
                }
            }
        }

        private bool LineIs(string line, string flag)
        {
            return line.Trim().Equals(flag);
        }

        private bool LineStartsWith(string line, params string[] flags)
        {
            line = line.Trim();

            foreach (string flag in flags)
            {
                if (line.StartsWith(flag + " ") || line.StartsWith(flag + StringUtility.GetAsciiTab()))
                    return true;
            }

            return false;
        }

        #region General parse

        /// <summary>
        /// Parse a #KPTYP line
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>True if line was syntax correct</returns>
        private bool ParseChartOfAccountsType(string line)
        {
            if (!LineStartsWith(line, Constants.SIE_LABEL_KPTYP))
                return false;

            string[] parts = Split(line);
            if (parts.Length < 2)
                return false;

            for (int i = 1; i < parts.Length; i++)
                ic.SieKpTyp += " " + parts[i];

            return true;
        }

        private bool ParseAccountYear(string line)
        {
            if (!LineStartsWith(line, Constants.SIE_LABEL_RAR))
                return false;

            string[] parts = Split(line);
            if (parts.Length < 4)
                return false;

            var accountYearItem = new SieAccountYearItem(line, ic.LineNr);
            accountYearItem.AccountYear = int.Parse(parts[1].Trim(' ', '"'));

            string startDate = parts[2].Trim(' ', '"');
            accountYearItem.FromDate = DateTime.ParseExact(startDate, "yyyyMMdd", CultureInfo.InvariantCulture);

            string endDate = parts[3].Trim(' ', '"');
            accountYearItem.ToDate = DateTime.ParseExact(endDate, "yyyyMMdd", CultureInfo.InvariantCulture);

            this.ic.AccountYearItems.Add(accountYearItem);

            return true;
        }

        #endregion

        #region Account parse

        /// <summary>
        /// Parse a #DIM line into a AccountDimItem
        /// Validates that the mandatory fields are not empty
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>True if line was syntax correct</returns>
        private bool ParseAccountDim(string line)
        {
            if (!LineStartsWith(line, Constants.SIE_LABEL_DIM))
                return false;

            string[] parts = Split(line);
            if (parts.Length < 3)
                return false;

            SieAccountDimItem accountDimItem = new SieAccountDimItem(line, ic.LineNr);

            //dimensionsnr
            string dimensionsnr = parts[1].Trim(' ', '"');
            if (!String.IsNullOrEmpty(dimensionsnr))
            {
                accountDimItem.AccountDimNr = Convert.ToInt32(dimensionsnr);
            }
            else
            {
                //Conflict: AccountDim has no AccountDimNr
                accountDimItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Dim_dimensionsnr);
            }

            //namn
            for (int i = 2; i < parts.Length; i++)
                accountDimItem.Name += " " + parts[i];
            accountDimItem.Name = accountDimItem.Name.Trim(' ', '"');
            if (String.IsNullOrEmpty(accountDimItem.Name))
            {
                //Conflict: AccountDim has no Name
                accountDimItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Dim_namn);
            }

            ic.AccountDimItems.Add(accountDimItem);

            return true;
        }

        /// <summary>
        /// Parse a #SRU line 
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>True if line was syntax correct</returns>
        private bool ParseAccountSRU(string line)
        {
            if (!LineStartsWith(line, Constants.SIE_LABEL_SRU))
                return false;

            string[] parts = Split(line);
            if (parts.Length < 3)
                return false;

            string accountNr = parts[1].Trim(' ', '"');
            if (String.IsNullOrEmpty(accountNr))
                return false;

            SieAccountStdItem accountStdItem = (from asi in ic.AccountStdItems
                                                where asi.AccountNr == accountNr
                                                select asi).FirstOrDefault<SieAccountStdItem>();

            if (accountStdItem != null)
            {
                string accountSRU = parts[2].Trim(' ', '"');
                if (!String.IsNullOrEmpty(accountSRU))
                {
                    accountStdItem.SruCode = accountSRU;
                }
            }

            return true;
        }

        /// <summary>
        /// Parse a #OBJECT line into a AccountInternalItem
        /// Validates that the mandatory fields are not empty
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>True if line was syntax correct</returns>
        private bool ParseAccountInternal(string line)
        {
            if (!LineStartsWith(line, Constants.SIE_LABEL_OBJEKT))
                return false;

            string[] parts = Split(line);
            if (parts.Length < 4)
                return false;

            SieAccountInternalItem accountInternalItem = new SieAccountInternalItem(line, ic.LineNr);

            //dimensionsnr
            string dimensionsnr = parts[1].Trim(' ', '"');
            if (!String.IsNullOrEmpty(dimensionsnr))
            {
                accountInternalItem.AccountDimNr = Convert.ToInt32(dimensionsnr);
            }
            else
            {
                //Conflict: AccountInternal has no AccountDimNr
                accountInternalItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Objekt_dimensionsnr);
            }

            //objektkod
            accountInternalItem.ObjectCode = parts[2].Trim(' ', '"');
            if (String.IsNullOrEmpty(accountInternalItem.ObjectCode))
            {
                //Conflict: AccountInternal has no ObjectCode
                accountInternalItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Objekt_objektkod);
            }

            //objektnamn
            for (int i = 3; i < parts.Length; i++)
                accountInternalItem.Name += " " + parts[i];
            accountInternalItem.Name = accountInternalItem.Name.Trim(' ', '"');
            if (String.IsNullOrEmpty(accountInternalItem.Name))
            {
                if (ic.ApproveEmptyAccountNames)
                {
                    accountInternalItem.Name = ic.EmptyAccountName;
                }
                else
                {
                    //Conflict: AccountInternal has no Name
                    accountInternalItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Objekt_objektnamn);
                }
            }

            ic.AccountInternalItems.Add(accountInternalItem);

            return true;
        }

        /// <summary>
        /// Parse a #ACCOUNT line into a AccountStdItem
        /// Validates that the mandatory fields are not empty
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>True if line was syntax correct</returns>
        private bool ParseAccountStd(string line)
        {
            if (!LineStartsWith(line, Constants.SIE_LABEL_KONTO))
                return false;

            string[] parts = Split(line);
            if (parts.Length < 3)
                return false;

            SieAccountStdItem accountStdItem = new SieAccountStdItem(line, ic.LineNr);
            accountStdItem.AccountDimNr = Constants.ACCOUNTDIM_STANDARD;

            //kontonr
            accountStdItem.AccountNr = parts[1].Trim(' ', '"');
            if (String.IsNullOrEmpty(accountStdItem.AccountNr))
            {
                //Conflict: AccountStd has no AccountNr
                accountStdItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Konto_kontonr);
            }

            //kontonamn
            for (int i = 2; i < parts.Length; i++)
                accountStdItem.Name += " " + parts[i];
            accountStdItem.Name = accountStdItem.Name.Trim(' ', '"');
            if (String.IsNullOrEmpty(accountStdItem.Name))
            {
                if (ic.ApproveEmptyAccountNames)
                {
                    accountStdItem.Name = ic.EmptyAccountName;
                }
                else
                {
                    //Conflict: AccountStd has no Name
                    accountStdItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Konto_kontonamn);
                }
            }

            ic.AccountStdItems.Add(accountStdItem);

            return true;
        }

        /// <summary>
        /// Parse a #KTYP line and completes a AccountStdItem with AccountType
        /// Validates that the mandatory fields are not empty
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>True if line was syntax correct</returns>
        private bool ParseAccountStdType(string line)
        {
            if (!LineStartsWith(line, Constants.SIE_LABEL_KTYP))
                return false;

            string[] parts = Split(line);
            if (parts.Length < 3)
                return false;

            string accountNr = parts[1].Trim(' ', '"');
            if (String.IsNullOrEmpty(accountNr))
                return false;

            SieAccountStdItem accountStdItem = (from asi in ic.AccountStdItems
                                                where asi.AccountNr == accountNr
                                                select asi).FirstOrDefault<SieAccountStdItem>();

            if (accountStdItem != null)
            {
                string accountStdType = String.Join(String.Empty, parts, 2, parts.Length - 2).Trim(' ', '"');
                accountStdType = parts[2].Trim(' ', '"');
                if (!String.IsNullOrEmpty(accountStdType))
                {
                    switch (accountStdType)
                    {
                        case "T":
                            accountStdItem.AccountType = (int)TermGroup_AccountType.Asset;
                            break;
                        case "S":
                            accountStdItem.AccountType = (int)TermGroup_AccountType.Debt;
                            break;
                        case "K":
                            accountStdItem.AccountType = (int)TermGroup_AccountType.Cost;
                            break;
                        case "I":
                            accountStdItem.AccountType = (int)TermGroup_AccountType.Income;
                            break;
                    }
                }
                else
                {
                    //Conflict: AccountType has no Type
                    accountStdItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Ktyp_kontotyp);
                }
            }
            return true;
        }

        #endregion

        #region Voucher parse

        /// <summary>
        /// Parse #VER and subsequent #TRANS lines into a VoucherItem with a collection of TransactionItems.
        /// Each TransactionItem have a collection of ObjectItems.
        /// Validates that the mandatory fields are not empty
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>True if line was syntax correct</returns>
        private bool ParseVoucher(string line)
        {
            if (!LineStartsWith(line, Constants.SIE_LABEL_VER))
                return false;

            SieVoucherItem voucherItem = this.CreateVoucherItem(line);
            if (voucherItem == null)
                return false;

            line = ic.ReadLine();

            if (LineIs(line, Constants.SIE_LABEL_START))
            {
                line = ic.ReadLine();
                while (!String.IsNullOrEmpty(line) && LineStartsWith(line, Constants.SIE_LABEL_TRANS, Constants.SIE_LABEL_BTRANS, Constants.SIE_LABEL_RTRANS))
                {
                    this.AddTransactionItem(line, voucherItem);

                    line = ic.ReadLine();
                    while (LineStartsWith(line, Constants.SIE_LABEL_TRANSEXT))
                        line = ic.ReadLine();
                }

                if (LineIs(line, Constants.SIE_LABEL_END))
                {
                    if (voucherItem.GetTransactionItems().Any())
                    {
                        //Check balance of the Vouchers all Transactions
                        voucherItem.Balance = Decimal.Zero;
                        foreach (SieTransactionItem transactionItem in voucherItem.GetTransactionItems())
                        {
                            voucherItem.Balance += transactionItem.Amount;
                        }
                    }
                }
                else
                {
                    //Conflict: Voucher has no end label (})
                    voucherItem.AddConflict(SieConflict.Import_VoucherHasNoEndLabel);

                    this.ParseLine(line, false, true, false);
                }
            }
            else
            {
                //Conflict: Voucher has no start label ({)
                voucherItem.AddConflict(SieConflict.Import_VoucherHasNoStartLabel);
                this.ParseLine(line, false, true, false);
            }

            ic.VoucherItems.Add(voucherItem);
            return true;
        }

        private bool ParseVoucher_Account_Voucher_AccountBalance(string line)
        {
            if (!LineStartsWith(line, Constants.SIE_LABEL_VER))
                return false;

            SieVoucherItem voucherItem = this.CreateVoucherItem(line);
            if (voucherItem == null)
                return false;

            line = ic.ReadLineVoucher();

            if (LineIs(line, Constants.SIE_LABEL_START))
            {
                line = ic.ReadLineVoucher();
                while (!String.IsNullOrEmpty(line) && LineStartsWith(line, Constants.SIE_LABEL_TRANS, Constants.SIE_LABEL_BTRANS, Constants.SIE_LABEL_RTRANS))
                {
                    this.AddTransactionItem(line, voucherItem);

                    line = ic.ReadLineVoucher();
                    while (LineStartsWith(line, Constants.SIE_LABEL_TRANSEXT))
                        line = ic.ReadLineVoucher();
                }

                if (LineIs(line, Constants.SIE_LABEL_END))
                {
                    if (voucherItem.GetTransactionItems().Any())
                    {
                        //Check balance of the Vouchers all Transactions
                        voucherItem.Balance = Decimal.Zero;
                        foreach (SieTransactionItem transactionItem in voucherItem.GetTransactionItems())
                        {
                            voucherItem.Balance += transactionItem.Amount;
                        }
                    }
                }
                else
                {
                    //Conflict: Voucher has no end label (})
                    voucherItem.AddConflict(SieConflict.Import_VoucherHasNoEndLabel);

                    this.ParseLine_Account_Voucher_AccountBalance(line, false, true, false);
                }
            }
            else
            {
                //Conflict: Voucher has no start label ({)
                voucherItem.AddConflict(SieConflict.Import_VoucherHasNoStartLabel);
                this.ParseLine_Account_Voucher_AccountBalance(line, false, true, false);
            }

            ic.VoucherItems.Add(voucherItem);
            return true;
        }

        /// <summary>
        /// Creates a VoucherItem from a #VER line
        /// Validates that the mandatory fields are not empty
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>The VoucherItem, null if failed</returns>
        private SieVoucherItem CreateVoucherItem(string line)
        {
            #region Prereq

            if (!LineStartsWith(line, Constants.SIE_LABEL_VER))
                return null;
            line = line.Replace("\" \" ", "\"\" ");
            string[] parts = Split(line);
            if (parts.Length < 4)
                return null;

            #endregion

            #region Parse

            SieVoucherItem voucherItem = new SieVoucherItem(line, ic.LineNr);

            #region serie

            voucherItem.VoucherSeriesTypeNr = ConcatArraySeparatedString(parts, 1, out int partDiff);

            partDiff--;

            #endregion

            #region vernr

            string vernr = voucherItem.Text = parts[2 + partDiff].Trim(' ', '"');

            if (!String.IsNullOrEmpty(vernr))
                voucherItem.VoucherNr = Convert.ToInt64(vernr);

            #endregion

            #region verdatum (mandatory)

            string verdatum = parts[3 + partDiff].Trim(' ', '"');
            if (!String.IsNullOrEmpty(verdatum))
            {
                voucherItem.VoucherDate = DateTime.ParseExact(verdatum, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            else
            {
                //Conflict: No VoucherDate
                voucherItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Ver_verdatum);
            }

            #endregion

            if (parts.Length >= 5)
            {
                #region vertext

                voucherItem.Text = ConcatArraySeparatedString(parts, 4 + partDiff, out int _);

                #endregion

                #region regdatum

                //    if (vertextEndIndex < (parts.Length - 1))
                //        voucherItem.RegDate = DateTime.ParseExact(parts[parts.Length - 1].Trim(' ', '"'), "yyyyMMdd", CultureInfo.InvariantCulture);

                #endregion
            }

            #endregion

            return voucherItem;
        }

        /// <summary>
        /// Creates a TransactionItem from a #TRANS line
        /// Validates that the mandatory fields are not empty
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <param name="voucherItem">The VoucherItem to add the TransactionItem to</param>
        private void AddTransactionItem(string line, SieVoucherItem voucherItem)
        {
            #region Prereq

            if (!LineStartsWith(line, Constants.SIE_LABEL_TRANS, Constants.SIE_LABEL_BTRANS, Constants.SIE_LABEL_RTRANS))
                return;

            string[] parts = Split(line);
            if (parts.Length < 4)
            {
                //Conflict: Not enough fields
                voucherItem.AddConflict(new SieConflictItem()
                {
                    Conflict = SieConflict.Import_InvalidLine,
                    Line = line,
                    LineNr = ic.LineNr,
                });
                return;
            }

            #endregion

            #region Parse

            SieTransactionItem transactionItem = new SieTransactionItem(line, ic.LineNr);
            if (LineStartsWith(line, Constants.SIE_LABEL_BTRANS))
                transactionItem.IsRemoved = true;
            else if (LineStartsWith(line, Constants.SIE_LABEL_RTRANS))
                transactionItem.IsAdded = true;
            else
                transactionItem.RelatedAddedTransaction = voucherItem.GetAddedTransaction(ic.LineNr - 1);

            #region kontonr (mandatory)

            string kontonr = parts[1].Trim(' ', '"');
            if (!String.IsNullOrEmpty(kontonr))
            {
                transactionItem.AccountNr = kontonr;
            }
            else
            {
                //Conflict: Transaction has no AccountNr
                transactionItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Trans_kontonr);
            }

            this.AddObjectItems(line, out int objListEndIndex, transactionItem);

            parts = Split(line.Substring(objListEndIndex + 1));
            if (parts.Length < 1)
            {
                //Conflict: Transaction has no Amount
                transactionItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Trans_belopp);
            }

            #endregion

            #region belopp (mandatory)

            string belopp = parts[0].Trim(' ', '"');
            if (!String.IsNullOrEmpty(belopp))
            {
                transactionItem.Amount = NumberUtility.ToDecimalWithComma(belopp, 2);
            }
            else
            {
                //Conflict: Transaction has no Amount
                transactionItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_Trans_belopp);
            }

            #endregion

            if (parts.Length >= 2)
            {
                #region transdat

                string transdat = parts[1].Trim(' ', '"');
                if (!String.IsNullOrEmpty(transdat))
                {
                    if (transdat.Length == 6)
                        transactionItem.TransactionDate = DateTime.ParseExact(transdat, "yyMMdd", CultureInfo.InvariantCulture);
                    else
                        transactionItem.TransactionDate = DateTime.ParseExact(transdat, "yyyyMMdd", CultureInfo.InvariantCulture);
                }

                #endregion

                if (parts.Length >= 3)
                {
                    #region transtext

                    string transtext = ConcatArraySeparatedString(parts, 2, out int transtextEndIndex);
                    if (!String.IsNullOrEmpty(transtext))
                        transactionItem.Text = transtext;

                    #endregion

                    #region kvantitet

                    int kvantitetIndex = transtextEndIndex + 1;
                    if (kvantitetIndex < parts.Length)
                    {
                        string kvantitet = parts[kvantitetIndex].Trim(' ', '"');
                        if (!String.IsNullOrEmpty(kvantitet))
                            transactionItem.Quantity = NumberUtility.ToNullableDecimalWithComma(kvantitet, 6);
                    }

                    #endregion

                    #region sign

                    //TODO: Not impletemented

                    #endregion
                }
            }

            #region Fill missing values

            //transdat
            if (!transactionItem.TransactionDate.HasValue)
            {
                //From added transaction or Voucher
                if (transactionItem.RelatedAddedTransaction != null)
                    transactionItem.TransactionDate = transactionItem.RelatedAddedTransaction.TransactionDate;
                else
                    transactionItem.TransactionDate = voucherItem.VoucherDate;
            }

            //transtext
            if (String.IsNullOrEmpty(transactionItem.Text))
            {
                //From Voucher
                transactionItem.Text = voucherItem.Text;
            }

            #endregion

            #region Conflicts

            if (transactionItem.Conflicts.Count > 0)
            {
                foreach (SieConflictItem sieConflictItem in transactionItem.Conflicts)
                {
                    if (sieConflictItem.Conflict == SieConflict.Import_InvalidLine)
                    {
                        //Overload with then transactions Line and LineNr
                        sieConflictItem.Line = transactionItem.Line;
                        sieConflictItem.LineNr = transactionItem.LineNr;
                        voucherItem.AddConflict(sieConflictItem);
                    }
                    else
                    {
                        //Use the default voucher Line and LineNr
                        voucherItem.AddConflict(sieConflictItem);
                    }
                }
            }

            #endregion

            voucherItem.AddTransactionItem(transactionItem);

            #endregion
        }

        /// <summary>
        /// Creates a Objectlist from a sequence of int(accountDimrNr) and string(objectcode) pairs
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <param name="objListEndIndex">The index the ObjectList ends at</param>
        /// <param name="transactionItem">The TransactionItem to create ObjectList for</param>
        private void AddObjectItems(string line, out int objListEndIndex, SieTransactionItem transactionItem)
        {
            #region Prereq

            objListEndIndex = line.IndexOf(Constants.SIE_LABEL_END);
            int objListStartIndex = line.IndexOf(Constants.SIE_LABEL_START) + 1;
            if (objListEndIndex == objListStartIndex)
                return;

            string objList = line.Substring(objListStartIndex, objListEndIndex - objListStartIndex).Trim();
            if (String.IsNullOrEmpty(objList))
                return;

            #endregion

            #region Parse

            #region objektlista

            int? sieDimension = null;
            string objectCode = null;

            string[] parts = Split(objList);
            for (int i = 0; i < parts.Length; i++)
            {
                string obj = parts[i].Trim(' ', '"');

                //even = AccountDimNr
                if (i % 2 == 0)
                {
                    if (!String.IsNullOrEmpty(obj))
                        sieDimension = Convert.ToInt32(obj);
                }
                //odd = Nr
                else
                {
                    objectCode = obj;

                    if (sieDimension.HasValue && objectCode != null)
                    {
                        transactionItem.AddObjectItem(new SieObjectItem()
                        {
                            SieDimension = sieDimension.Value,
                            ObjectCode = objectCode,
                        });
                    }
                    else
                    {
                        //Syntax conflict: Invalid Line
                        transactionItem.AddConflict(SieConflict.Import_InvalidLine);
                    }

                    sieDimension = null;
                    objectCode = null;
                }
            }

            if ((sieDimension.HasValue) && objectCode.IsNullOrEmpty())
                transactionItem.AddConflict(SieConflict.Import_InvalidLine);

            #endregion

            #endregion
        }

        #endregion

        #region Balance parse

        /// <summary>
        /// Parse a #IB line into a AccountYearBalanceHead
        /// Validates that the mandatory fields are not empty
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>True if line was syntax correct</returns>
        private bool ParseAccountStdBalance(string line)
        {
            if (ic.UseUBInsteadOfIB ? (!LineStartsWith(line, Constants.SIE_LABEL_UB)) : (!LineStartsWith(line, Constants.SIE_LABEL_IB)))
                return false;

            string[] parts = Split(line);
            if (parts.Length < 4)
                return false;

            SieAccountStdInBalanceItem accountStdBalanceItem = new SieAccountStdInBalanceItem(line, ic.LineNr);
            accountStdBalanceItem.UseUBInsteadOfIB = ic.UseUBInsteadOfIB;

            //accountYear
            string accountYear = parts[1].Trim(' ', '"');
            if (!String.IsNullOrEmpty(accountYear))
            {
                accountStdBalanceItem.AccountYear = Convert.ToInt32(accountYear);
            }
            else
            {
                //Conflict: AccountBalance has no AccountYear
                accountStdBalanceItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_AccountBalance_accountyear);
            }

            //accountNr
            string accountNr = parts[2].Trim(' ', '"');
            if (!String.IsNullOrEmpty(accountNr))
            {
                accountStdBalanceItem.AccountNr = accountNr;
            }
            else
            {
                //Conflict: AccountBalance has no AccountNr
                accountStdBalanceItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_AccountBalance_accountnr);
            }

            //balance
            string balance = parts[3].Trim(' ', '"');
            if (!String.IsNullOrEmpty(balance))
            {
                accountStdBalanceItem.Balance = NumberUtility.ToDecimalWithComma(balance, 2);
            }
            else
            {
                //Conflict: AccountBalance has no Balance
                accountStdBalanceItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_AccountBalance_accountnr);
            }

            if (parts.Length >= 5)
            {
                string quantity = parts[4].Trim(' ', '"');
                if (!String.IsNullOrEmpty(quantity))
                {
                    accountStdBalanceItem.Quantity = NumberUtility.ToNullableDecimalWithComma(quantity, 6);
                }
            }

            ic.AccountStdInBalanceItems.Add(accountStdBalanceItem);

            return true;
        }

        /// <summary>
        /// Parse a #OIB line into a AccountYearBalanceHead
        /// Validates that the mandatory fields are not empty
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>True if line was syntax correct</returns>
        private bool ParseAccountInternalBalance(string line)
        {
            if (!LineStartsWith(line, Constants.SIE_LABEL_OIB))
                return false;

            string[] parts = Split(line);
            if (parts.Length < 4)
                return false;

            SieAccountInternalInBalanceItem accountInternalInBalanceItem = new SieAccountInternalInBalanceItem(line, ic.LineNr);
            accountInternalInBalanceItem.UseUBInsteadOfIB = ic.UseUBInsteadOfIB;

            //accountYear
            string accountYear = parts[1].Trim(' ', '"');
            if (!String.IsNullOrEmpty(accountYear))
            {
                accountInternalInBalanceItem.AccountYear = Convert.ToInt32(accountYear);
            }
            else
            {
                //Conflict: AccountBalance has no AccountYear
                accountInternalInBalanceItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_AccountBalance_accountyear);
            }

            //accountNr
            string accountNr = parts[2].Trim(' ', '"');
            if (!String.IsNullOrEmpty(accountNr))
            {
                accountInternalInBalanceItem.AccountNr = accountNr;
            }
            else
            {
                //Conflict: AccountBalance has no AccountNr
                accountInternalInBalanceItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_AccountBalance_accountnr);
            }

            //balance
            string balance = parts[5].Trim(' ', '"');
            if (!String.IsNullOrEmpty(balance))
            {
                accountInternalInBalanceItem.Balance = NumberUtility.ToDecimalWithComma(balance, 2);
            }
            else
            {
                //Conflict: AccountBalance has no Balance
                accountInternalInBalanceItem.AddConflict(SieConflict.Import_MandatoryFieldMissing_AccountBalance_accountnr);
            }

            if (parts.Length >= 7)
            {
                string quantity = parts[6].Trim(' ', '"');
                if (!String.IsNullOrEmpty(quantity))
                {
                    accountInternalInBalanceItem.Quantity = NumberUtility.ToNullableDecimalWithComma(quantity, 6);
                }
            }

            this.AddObjectItems(line, out int _, accountInternalInBalanceItem);

            ic.AccountInternalInBalanceItems.Add(accountInternalInBalanceItem);
            return true;
        }
        ///// <summary>
        ///// Creates a Objectlist from a sequence of int(accountDimrNr) and string(objectcode) pairs
        ///// </summary>
        ///// <param name="line">The line to parse</param>
        ///// <param name="objListEndIndex">The index the ObjectList ends at</param>
        ///// <param name="transactionItem">The TransactionItem to create ObjectList for</param>
        private void AddObjectItems(string line, out int objListEndIndex, SieAccountInternalInBalanceItem accountInternalInBalanceItem)
        {
            #region Prereq

            objListEndIndex = line.IndexOf(Constants.SIE_LABEL_END);
            int objListStartIndex = line.IndexOf(Constants.SIE_LABEL_START) + 1;
            if (objListEndIndex == objListStartIndex)
                return;

            string objList = line.Substring(objListStartIndex, objListEndIndex - objListStartIndex).Trim();
            if (String.IsNullOrEmpty(objList))
                return;

            #endregion

            #region Parse

            #region objektlista

            int? sieDimension = null;
            string objectCode = null;

            string[] parts = Split(objList);
            for (int i = 0; i < parts.Length; i++)
            {
                string obj = parts[i].Trim(' ', '"');

                //even = AccountDimNr
                if (i % 2 == 0)
                {
                    if (!String.IsNullOrEmpty(obj))
                        sieDimension = Convert.ToInt32(obj);
                }
                //odd = Nr
                else
                {
                    objectCode = obj;

                    if (sieDimension.HasValue && objectCode != null)
                    {
                        accountInternalInBalanceItem.AddObjectItem(new SieObjectItem()
                        {
                            SieDimension = sieDimension.Value,
                            ObjectCode = objectCode,
                        });
                    }
                    else
                    {
                        //Syntax conflict: Invalid Line
                        accountInternalInBalanceItem.AddConflict(SieConflict.Import_InvalidLine);
                    }

                    sieDimension = null;
                    objectCode = null;
                }
            }

            if ((sieDimension.HasValue) && (String.IsNullOrEmpty(objectCode)))
                accountInternalInBalanceItem.AddConflict(SieConflict.Import_InvalidLine);
            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Prereq (data validation)

        /// <summary>
        /// Check prerequisites for Vouchers:
        /// - Default VoucherSerieType and AccountYear
        /// - VoucherSeriesType exist
        /// - AccountYear exist
        /// - AccountPeriod exist
        /// - AccountDim std exists
        /// - AccountDim for all AccountInternal exist
        /// - All AccountInternal exist
        /// - All Accounts fulfill the rules of the AccountDim
        /// </summary>
        private void PrereqVouchers()
        {
            #region Init

            if (ic.VoucherItems.Count == 0)
                return;

            #endregion

            #region Prereq

            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(actorCompanyId);
            var accountStds = AccountManager.GetAccountStdsByCompany(actorCompanyId, null)
                .DistinctBy(std => std.Account.AccountNr)
                .ToDictionary(std => std.Account.AccountNr);
            var accountInternals = AccountManager.GetAccountInternals(actorCompanyId, true)
                .DistinctBy(ai => new { ai.Account.AccountNr, ai.Account.AccountDimId })
                .ToDictionary(ai => new { ai.Account.AccountNr, ai.Account.AccountDimId });
            List<AccountYear> accountYears = AccountManager.GetAccountYears(actorCompanyId, false, false);
            List<AccountPeriod> accountPeriods = AccountManager.GetAccountPeriods(ic.AccountYearId, true);
            List<VoucherSeriesType> voucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(actorCompanyId, false);
            VoucherHeads = VoucherManager.GetVoucherHeadsByCompany(actorCompanyId);

            AccountDim accountDimStd = accountDims.FirstOrDefault(i => i.AccountDimNr == Constants.ACCOUNTDIM_STANDARD);

            Dictionary<int, List<VoucherSeries>> voucherSeriesForAccountYearDict = new Dictionary<int, List<VoucherSeries>>();

            //Check default VoucherSeriesType and AccountYear
            VoucherSeries voucherSeriesDefault = null;
            if (ic.DefaultVoucherSeriesId.HasValue)
                voucherSeriesDefault = VoucherManager.GetVoucherSerie(ic.DefaultVoucherSeriesId.Value, actorCompanyId, true);

            var uniqueVoucherNumberSeries = new HashSet<(long, int)>();

            #endregion

            foreach (SieVoucherItem voucherItem in ic.VoucherItems)
            {
                try
                {
                    #region AccountDim std

                    if (accountDimStd == null)
                    {
                        //Conflict: AccountDim std doesnt exist
                        voucherItem.AddConflict(SieConflict.Import_DimNotFound, Constants.ACCOUNTDIM_STANDARD);
                        return;
                    }

                    #endregion

                    #region AccountYear

                    AccountYear accountYear = accountYears.FirstOrDefault(ay => ay.From <= voucherItem.VoucherDate && ay.To >= voucherItem.VoucherDate);
                    if (accountYear != null)
                    {
                        if (accountYear.AccountYearId == ic.AccountYear.AccountYearId)
                        {
                            if (!ic.AllowNotOpenAccountYear)
                            {
                                //Check if AccountYear exist and is open (or locked/closed AccountYear is allowed)
                                if (accountYear.Status == (int)TermGroup_AccountStatus.Open)
                                {
                                    voucherItem.AccountYear = accountYear;

                                    //Check AccountPeriod
                                    voucherItem.AccountPeriod = accountPeriods.FirstOrDefault(ap => ap.From <= voucherItem.VoucherDate && ap.To >= voucherItem.VoucherDate);
                                    if (voucherItem.AccountPeriod == null)
                                    {
                                        //Conflict: AccountPeriod doesnt exist
                                        voucherItem.AddConflict(SieConflict.Import_VouchersAccountPeriodDoesNotExist, voucherItem.VoucherDate);
                                        return;
                                    }
                                    if (voucherItem.AccountPeriod.Status != (int)TermGroup_AccountStatus.Open)
                                    {
                                        //Conflict: AccountPeriod closed
                                        voucherItem.AddConflict(SieConflict.Import_VouchersAccountPeriodIsNotOpen, voucherItem.VoucherDate);
                                        return;
                                    }
                                }
                                else
                                {
                                    //Conflict: AccountYear isnt open
                                    voucherItem.AddConflict(SieConflict.Import_AccountYearIsNotOpen, accountYear.GetFromToShortString());
                                    return;
                                }
                            }
                            else
                            {
                                voucherItem.AccountYear = accountYear;

                                //Check AccountPeriod
                                voucherItem.AccountPeriod = accountPeriods.FirstOrDefault(ap => ap.From <= voucherItem.VoucherDate && ap.To >= voucherItem.VoucherDate);
                                if (voucherItem.AccountPeriod == null)
                                {
                                    //Conflict: AccountPeriod doesnt exist
                                    voucherItem.AddConflict(SieConflict.Import_VouchersAccountPeriodDoesNotExist, voucherItem.VoucherDate);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            //Conflict: AccountYear doesnt match choosed default AccountYear
                            voucherItem.AddConflict(SieConflict.Import_VouchersAccountYearDoesNotMatchAccountYearDefault, voucherItem.VoucherDate);
                            return;
                        }
                    }
                    else
                    {
                        //Conflict: AccountYear doesnt exist
                        voucherItem.AddConflict(SieConflict.Import_VouchersAccountYearDoesNotExist, voucherItem.VoucherDate.ToString("yyyyMMdd"));
                        return;
                    }

                    #endregion

                    #region VoucherSeries

                    if (ic.OverrideVoucherSeries)
                    {
                        #region Override with default

                        if (voucherSeriesDefault != null)
                        {
                            voucherItem.VoucherSeries = voucherSeriesDefault; //Override with default VoucherSerie
                            voucherItem.VoucherSeriesType = voucherSeriesDefault.VoucherSeriesType;
                            voucherItem.IsDefaultVoucherSeries = true;
                        }
                        else
                        {
                            //Conflict: Default VoucherSerie doesnt exist (shouldnt be allowed by gui)
                            voucherItem.AddConflict(SieConflict.Import_VoucherSeriesTypeDoesNotExist, ic.DefaultVoucherSeriesId.ToString());
                            return;
                        }

                        #endregion
                    }
                    else
                    {
                        #region VoucherSeries

                        //Check VouchSeries mapping
                        if (ic.HasVoucherSeriesTypesMapping && ic.VoucherSeriesTypesMappingDict.ContainsKey(voucherItem.VoucherSeriesTypeNr.ToLower()))
                        {
                            int voucherSeriesTypeId = ic.VoucherSeriesTypesMappingDict[voucherItem.VoucherSeriesTypeNr.ToLower()];
                            voucherItem.VoucherSeriesType = voucherSeriesTypes.FirstOrDefault(vst => vst.VoucherSeriesTypeId == voucherSeriesTypeId);
                        }
                        else
                        {
                            voucherItem.VoucherSeriesType = voucherSeriesTypes.FirstOrDefault(vst => vst.VoucherSeriesTypeNr.ToString() == voucherItem.VoucherSeriesTypeNr);
                        }

                        if (voucherItem.VoucherSeriesType != null)
                        {
                            #region VoucherSeries for AccountYear

                            List<VoucherSeries> voucherSeries = null;

                            if (voucherSeriesForAccountYearDict.ContainsKey(voucherItem.AccountYear.AccountYearId))
                            {
                                //Get from dict
                                voucherSeries = voucherSeriesForAccountYearDict[voucherItem.AccountYear.AccountYearId];
                            }
                            if (voucherSeries == null)
                            {
                                //Get from db
                                voucherSeries = VoucherManager.GetVoucherSeriesByYear(voucherItem.AccountYear.AccountYearId, actorCompanyId, false);

                                //Add to dict
                                if (voucherSeriesForAccountYearDict.ContainsKey(voucherItem.AccountYear.AccountYearId))
                                    voucherSeriesForAccountYearDict[voucherItem.AccountYear.AccountYearId] = voucherSeries;
                                else
                                    voucherSeriesForAccountYearDict.Add(voucherItem.AccountYear.AccountYearId, voucherSeries);
                            }

                            #endregion

                            //Check VoucherSeries
                            voucherItem.VoucherSeries = voucherSeries.FirstOrDefault(vst => vst.VoucherSeriesType.VoucherSeriesTypeId == voucherItem.VoucherSeriesType.VoucherSeriesTypeId);
                            if (voucherItem.VoucherSeries == null)
                            {
                                #region Add

                                voucherItem.VoucherSeries = new VoucherSeries();

                                if (!VoucherManager.AddVoucherSeries(voucherItem.VoucherSeries, actorCompanyId, ic.AccountYear.AccountYearId, voucherItem.VoucherSeriesType.VoucherSeriesTypeId).Success)
                                {
                                    //Conflict: Add failed
                                    voucherItem.AddConflict(SieConflict.Import_AddFailed);
                                    return;
                                }

                                #endregion
                            }
                        }
                        else
                        {
                            #region Default VoucherSeriesType

                            if (voucherSeriesDefault != null)
                            {
                                voucherItem.VoucherSeries = voucherSeriesDefault; //VoucherSerieType doesnt exist, use default VoucherSerie
                                voucherItem.VoucherSeriesType = voucherSeriesDefault.VoucherSeriesType;
                                voucherItem.IsDefaultVoucherSeries = true;
                            }
                            else
                            {
                                //Conflict: VoucherSeriesType doesnt exist, and no default VoucherSerie choosed
                                voucherItem.AddConflict(SieConflict.Import_VoucherSeriesTypeDoesNotExist, voucherItem.VoucherSeriesTypeNr);
                                return;
                            }

                            #endregion
                        }

                        #endregion
                    }

                    #endregion

                    #region VoucherNr

                    //Set VoucherNr from LatestVoucher from default VoucherSerie for VoucherItems without VoucherNr
                    if ((!voucherItem.VoucherNr.HasValue || voucherItem.IsDefaultVoucherSeries || ic.TakeVoucherNrFromSeries) && voucherItem.VoucherSeries != null && voucherItem.VoucherSeries.VoucherNrLatest.HasValue)
                    {
                        voucherItem.VoucherNr = voucherItem.VoucherSeries.VoucherNrLatest.Value + 1;
                        voucherItem.VoucherSeries.VoucherNrLatest = voucherItem.VoucherNr;
                        voucherItem.VoucherSeries.VoucherDateLatest = voucherItem.VoucherDate;
                    }

                    //Check VoucherNr
                    if (!voucherItem.VoucherNr.HasValue)
                    {
                        //Conflict: Voucher has no VoucherNr
                        voucherItem.AddConflict(SieConflict.Import_VoucherHasNoVoucherNr);
                        return;
                    }

                    //Check that Voucher doesnt already exist
                    bool voucherExists = VoucherHeads.Any(vh => vh.VoucherNr == voucherItem.VoucherNr.Value &&
                                                                vh.VoucherSeriesId == voucherItem.VoucherSeries.VoucherSeriesId &&
                                                                vh.AccountPeriod.AccountYearId == voucherItem.AccountPeriod.AccountYearId);

                    if (!ic.OverrideVoucherDeletes && !ic.SkipAlreadyExistingVouchers && (ic.VoucherSeriesDeleteDict == null || !ic.VoucherSeriesDeleteDict.ContainsKey(voucherItem.VoucherSeries.VoucherSeriesTypeId)) && voucherExists)
                    {
                        //Conflict: Voucher exist
                        voucherItem.AddConflict(SieConflict.Import_VoucherAlreadyExist, voucherItem.VoucherNr);
                        return;
                    }

                    if (uniqueVoucherNumberSeries.Contains((voucherItem.VoucherNr.Value, voucherItem.VoucherSeries.VoucherSeriesId)))
                    {
                        voucherItem.AddConflict(SieConflict.Import_VoucherAlreadyExist, voucherItem.VoucherNr);
                        return;
                    }
                    uniqueVoucherNumberSeries.Add((voucherItem.VoucherNr.Value, voucherItem.VoucherSeries.VoucherSeriesId));

                    #endregion

                    foreach (SieTransactionItem transactionItem in voucherItem.GetTransactionItems())
                    {
                        #region AccountStd

                        //Check that AccountStd exist
                        if (accountStds.TryGetValue(transactionItem.AccountNr, out AccountStd accountStd))
                        {
                            //Check AccountDim rules
                            if (!this.IsAccountDimRuleFulfilled(transactionItem, accountDimStd))
                            {
                                //Conflict: AccountStd not furfill rules of AccountDim
                                voucherItem.AddConflict(SieConflict.Import_AccountRuleFailed, transactionItem.AccountNr);
                                return;
                            }
                        }
                        else
                        {
                            //Conflict: AccountStd doesnt exist
                            voucherItem.AddConflict(SieConflict.Import_VouchersAccountDoesNotExist, transactionItem.AccountNr);
                            return;
                        }

                        #endregion

                        foreach (SieObjectItem objectItem in transactionItem.ObjectItems)
                        {
                            #region AccountDim

                            //Check AccountDim
                            AccountDim accountDim = accountDims.FirstOrDefault(ad => ad.SysSieDimNr == objectItem.SieDimension);
                            if (accountDim != null)
                            {
                                //Check that AccountInternal exist
                                accountInternals.TryGetValue(new { AccountNr = objectItem.ObjectCode, AccountDimId = accountDim.AccountDimId }, out AccountInternal accountInternal);
                                if (accountInternal != null)
                                {
                                    //Check AccountDim rules
                                    if (!AccountManager.IsAccountValidInAccountDim(accountDim.MinChar, accountDim.MaxChar, accountInternal.Account.AccountNr))
                                    {
                                        //Conflict: AccountInternal not furfill rules of AccountDim
                                        voucherItem.AddConflict(SieConflict.Import_AccountRuleFailed, objectItem.ObjectCode);
                                    }
                                }
                                else
                                {
                                    //Conflict: AccountInternal doesnt exist
                                    voucherItem.AddConflict(SieConflict.Import_VouchersObjectDoesNotExist, objectItem.ObjectCode);
                                    return;
                                }
                            }
                            else
                            {
                                //Conflict: AccountDim doesnt exist
                                voucherItem.AddConflict(SieConflict.Import_DimNotFound, objectItem.SieDimension);
                                return;
                            }

                            #endregion
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Conflict: Exception
                    voucherItem.AddConflict(SieConflict.Exception, ex.Message);
                    return;
                }
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves imported AccountDims.
        /// 
        /// - A AccountDim is updated if its SysSieDimNr is equals to dimnr from SIE file.
        /// - A AccountDim is inserted if no SysSieDimNr is equals to dimnr from SIE file is found. The dimnr is set to next available.
        /// 
        /// Name conflicts is overwrited if overwriteNameConflicts is true, otherwise a conflict is reported, but it
        /// does not fail and its other properties are updated.
        /// </summary>
        private ActionResult SaveAccountDims(List<int> dimNrsToImport)
        {
            #region Init

            if (ic.AccountDimItems == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            if (ic.AccountDimItems.Count == 0)
                return new ActionResult();
            if (dimNrsToImport == null)
                return new ActionResult();
            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                //Get Company
                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Get AccountDim
                List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId);

                //Get max AccountDimNr
                int maxDimrNr = (from ad in accountDims
                                 select ad.AccountDimNr).Max();

                #endregion

                foreach (SieAccountDimItem accountDimItem in ic.AccountDimItems.Where(x => dimNrsToImport.Contains(x.AccountDimNr.GetValueOrDefault())))
                {
                    try
                    {
                        #region Validate

                        if (accountDimItem.Invalid)
                            continue;

                        #endregion

                        #region AccountDim

                        int accountDimNr = Convert.ToInt32(accountDimItem.AccountDimNr);

                        //Check for SIE mappings
                        AccountDim accountDim = accountDims.FirstOrDefault(ad => ad.SysSieDimNr == accountDimNr);
                        if (accountDim == null)
                        {
                            #region Add

                            if (maxDimrNr >= Constants.ACCOUNTDIM_STANDARD)
                                maxDimrNr++;
                            else
                                maxDimrNr = Constants.ACCOUNTDIM_STANDARD + 1;

                            accountDim = new AccountDim()
                            {
                                Company = company,
                                AccountDimNr = maxDimrNr,
                                Name = accountDimItem.Name,
                                SysSieDimNr = accountDimNr,
                            };
                            SetCreatedProperties(accountDim);
                            entities.AccountDim.AddObject(accountDim);

                            if (accountDim.Name.Length <= 10)
                                accountDim.ShortName = accountDim.Name;
                            else
                                accountDim.ShortName = accountDimItem.Name.Substring(0, 10);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            //Reset state
                            accountDim.State = (int)SoeEntityState.Active;
                            SetModifiedProperties(accountDim);

                            accountDimItem.OriginalName = accountDim.Name;
                            if (accountDimItem.OriginalName != accountDimItem.Name)
                            {
                                if (ic.OverwriteNameConflicts)
                                {
                                    accountDim.Name = accountDimItem.Name;
                                }
                                else
                                {
                                    //Conflict: Name conflict
                                    accountDimItem.AddConflict(SieConflict.Import_NameConflict);
                                }
                            }

                            #endregion
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, this.log);

                        //Conflict: Exception
                        accountDimItem.AddConflict(SieConflict.Exception, ex.Message);
                        return new ActionResult((int)ActionResultSave.Unknown, ex.Message);
                    }
                }

                var result = SaveChangesWithTransaction(entities);
                perfHelper.Checkpoint("SaveAccountDims");
                return result;
            }
        }

        /// <summary>
        /// Saves imported AccountInternal
        /// 
        /// Name conflicts is overwrited if overwriteNameConflicts is true, otherwise a conflict is reported, but it
        /// does not fail and its other properties are updated.
        /// </summary>
        private ActionResult SaveAccountInternals(List<int> dimNrsToImport)
        {
            #region Init

            if (ic.AccountInternalItems == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            if (ic.AccountInternalItems.Count == 0)
                return new ActionResult();
            if (dimNrsToImport == null)
                return new ActionResult();
            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                User user = UserManager.GetUser(entities, ic.UserId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(entities, actorCompanyId);
                List<Account> accountInternals = AccountManager.GetAccountsInternalsByCompany(entities, actorCompanyId, true, true, true);

                #endregion

                foreach (SieAccountInternalItem accountInternalItem in ic.AccountInternalItems.Where(x => dimNrsToImport.Contains(x.AccountDimNr.GetValueOrDefault())))
                {
                    try
                    {
                        #region Validate

                        if (accountInternalItem.Invalid || accountInternalItem.AccountDimNr == null)
                            continue;

                        //Check for SIE mappings
                        AccountDim accountDimInternal = accountDimInternals.FirstOrDefault(ad => ad.SysSieDimNr == accountInternalItem.AccountDimNr);
                        if (accountDimInternal == null)
                        {
                            //Conflict: No matching AccountDim found
                            accountInternalItem.AddConflict(SieConflict.Import_DimNotFound, accountInternalItem.AccountDimNr);
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");
                        }

                        if (!this.IsAccountDimRuleFulfilled(accountInternalItem, accountDimInternal))
                        {
                            //Conflict: Object not furfill rules of AccountDim
                            accountInternalItem.AddConflict(SieConflict.Import_ObjectRuleFailed, accountInternalItem.ObjectCode);
                            return new ActionResult((int)ActionResultSave.AccountDimRuleNotFulfilled);
                        }

                        #endregion

                        #region Account

                        Account account = accountInternals.FirstOrDefault(a => a.AccountDim.AccountDimNr == accountDimInternal.AccountDimNr && a.AccountNr == accountInternalItem.ObjectCode);
                        if (account == null)
                        {
                            #region Add

                            account = new Account()
                            {
                                Name = accountInternalItem.Name,
                                AccountNr = accountInternalItem.ObjectCode,

                                //Set FK
                                ActorCompanyId = actorCompanyId,

                                //Set references
                                AccountDim = accountDimInternal,
                            };
                            SetCreatedProperties(account);
                            entities.Account.AddObject(account);

                            #region AccountInternal

                            account.AccountInternal = new AccountInternal();

                            #endregion

                            #region AccountHistory

                            AccountHistory accountHistory = new AccountHistory()
                            {
                                User = user,
                                Account = account,
                                Name = account.Name,
                                AccountNr = account.AccountNr,
                                Date = DateTime.Now,
                                SysAccountStdTypeId = null,
                                SieKpTyp = null,
                            };
                            SetCreatedProperties(accountHistory);
                            entities.AccountHistory.AddObject(accountHistory);

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Update

                            //Reset state
                            account.State = (int)SoeEntityState.Active;
                            SetModifiedProperties(account);

                            accountInternalItem.OriginalName = account.Name;
                            if (accountInternalItem.OriginalName != accountInternalItem.Name)
                            {
                                if (ic.OverwriteNameConflicts)
                                {
                                    account.Name = accountInternalItem.Name;
                                }
                                else
                                {
                                    //Conflict: Name conflict
                                    accountInternalItem.AddConflict(SieConflict.Import_NameConflict);
                                }
                            }

                            #region AccountHistory

                            AccountHistory accountHistory = new AccountHistory()
                            {
                                User = user,
                                Account = account,
                                Name = account.Name,
                                AccountNr = account.AccountNr,
                                Date = DateTime.Now,
                                SysAccountStdTypeId = null,
                                SieKpTyp = null,
                            };
                            SetCreatedProperties(accountHistory);
                            entities.AccountHistory.AddObject(accountHistory);

                            #endregion

                            #endregion
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, this.log);

                        //Conflict: Exception
                        accountInternalItem.AddConflict(SieConflict.Exception, ex.Message);
                        return new ActionResult((int)ActionResultSave.Unknown, ex.Message);
                    }
                }

                var result = SaveChangesWithTransaction(entities);
                perfHelper.Checkpoint("SaveAccountInternals");
                return result;
            }
        }

        /// <summary>
        /// Saves imported Accounts
        /// 
        /// Name conflicts is overwrited if overwriteNameConflicts is true, otherwise a conflict is reported, but it
        /// does not fail and its other properties are updated.
        /// </summary>
        private ActionResult SaveAccountStds()
        {
            if (ic.AccountStdItems == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            if (ic.AccountStdItems.Count == 0)
                return new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                User user = UserManager.GetUser(entities, ic.UserId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                IEnumerable<SysAccountSruCode> sysAccountSruCodes = null;
                sysAccountSruCodes = AccountManager.GetSysAccountSruCodes();
                List<Account> accountStds = null;
                AccountDim accountDimStd = AccountManager.GetAccountDimStd(entities, actorCompanyId);
                if (accountDimStd != null)
                    accountStds = AccountManager.GetAccountsByDim(entities, accountDimStd.AccountDimId, actorCompanyId, null, true, true).ToList();

                #endregion

                foreach (SieAccountStdItem accountStdItem in ic.AccountStdItems)
                {
                    try
                    {
                        #region Validate

                        if (accountStdItem.Invalid)
                            continue;

                        if (accountDimStd == null)
                        {
                            //Conflict: No AccountDim std found
                            accountStdItem.AddConflict(SieConflict.Import_DimNotFound, Constants.ACCOUNTDIM_STANDARD);
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStdItem");
                        }

                        if (!this.IsAccountDimRuleFulfilled(accountStdItem, accountDimStd))
                        {
                            //Conflict: AccountStd not furfill rules of AccountDim
                            accountStdItem.AddConflict(SieConflict.Import_AccountRuleFailed, accountStdItem.AccountNr);
                            return new ActionResult((int)ActionResultSave.AccountDimRuleNotFulfilled);
                        }

                        #endregion

                        #region Account

                        Account account = accountStds.FirstOrDefault(a => a.AccountNr == accountStdItem.AccountNr);
                        if (account == null)
                        {
                            #region Add

                            account = new Account()
                            {
                                Name = accountStdItem.Name.Substring(0, Math.Min(accountStdItem.Name.Length, 100)),
                                AccountNr = accountStdItem.AccountNr,

                                //Set FK
                                ActorCompanyId = actorCompanyId,

                                //Set references
                                AccountDim = accountDimStd,
                            };
                            SetCreatedProperties(account);
                            entities.Account.AddObject(account);

                            #region AccountStd

                            //AccountStd
                            account.AccountStd = new AccountStd()
                            {
                                SieKpTyp = ic.SieKpTyp,
                            };

                            #endregion

                            #region AccountType

                            if (accountStdItem.AccountType.HasValue)
                            {
                                account.AccountStd.AccountTypeSysTermId = accountStdItem.AccountType.Value;
                            }
                            else
                            {
                                //Get default AccountType
                                int? accountType = AccountManager.GetAccountStdTypeFromAccountNr(accountStdItem.AccountNr);
                                if (accountType.HasValue)
                                {
                                    account.AccountStd.AccountTypeSysTermId = accountType.Value;
                                }
                                else
                                {
                                    //Conflict: No AccountType in SIE file and could not get AccountType from AccountNr
                                    accountStdItem.AddConflict(SieConflict.Import_AccountHasNoAccountType, accountStdItem.AccountNr);
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStdItem");
                                }
                            }

                            #endregion

                            #region AccountHistory

                            AccountHistory accountHistory = new AccountHistory()
                            {
                                User = user,
                                Account = account,
                                Name = account.Name,
                                AccountNr = account.AccountNr,
                                Date = DateTime.Now,
                                SysAccountStdTypeId = null,
                                SieKpTyp = account.AccountStd.SieKpTyp,
                            };
                            SetCreatedProperties(accountHistory);
                            entities.AccountHistory.AddObject(accountHistory);

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Update

                            //Do not update or reset Account that is set to inactive
                            if (account.State == (int)SoeEntityState.Inactive)
                                continue;

                            //Reset state (from deleted to active)
                            account.State = (int)SoeEntityState.Active;
                            SetModifiedProperties(account);

                            accountStdItem.OriginalName = account.Name;
                            if (accountStdItem.OriginalName != accountStdItem.Name)
                            {
                                if (ic.OverwriteNameConflicts)
                                {
                                    account.Name = accountStdItem.Name.Substring(0, Math.Min(accountStdItem.Name.Length, 100));
                                }
                                else
                                {
                                    //Conflict: Name conflict
                                    accountStdItem.AddConflict(SieConflict.Import_NameConflict);
                                }
                            }

                            #region AccountType

                            if (account.AccountStd == null)
                            {
                                account.AccountStd = new AccountStd()
                                {
                                    SieKpTyp = ic.SieKpTyp,
                                };
                            }
                            else
                            {
                                account.AccountStd.SieKpTyp = ic.SieKpTyp;
                                if (accountStdItem.AccountType.HasValue)
                                {
                                    account.AccountStd.AccountTypeSysTermId = accountStdItem.AccountType.Value;
                                }
                            }
                            #endregion

                            #region SRUcode                           


                            if (accountStdItem.SruCode != null)
                            {
                                if (!account.AccountStd.AccountSru.IsLoaded)
                                    account.AccountStd.AccountSru.Load();

                                SysAccountSruCode accountSruCode = sysAccountSruCodes.FirstOrDefault(a => a.SruCode == accountStdItem.SruCode);
                                List<AccountSru> accountSrus = account.AccountStd.AccountSru.OrderBy(i => i.AccountSruId).ToList();
                                AccountSru accountSru1 = account.AccountStd.AccountSru.Count >= 1 ? account.AccountStd.AccountSru.FirstOrDefault() : null;
                                AccountSru accountSru2 = account.AccountStd.AccountSru.Count >= 2 ? account.AccountStd.AccountSru.Skip(1).FirstOrDefault() : null;
                                if (accountSruCode != null)
                                    AccountManager.SetAccountSru(entities, account.AccountStd, accountSru1, accountSruCode.SysAccountSruCodeId);

                            }
                            #endregion

                            #region AccountHistory

                            AccountHistory accountHistory = new AccountHistory()
                            {
                                User = user,
                                Account = account,
                                Name = account.Name,
                                AccountNr = account.AccountNr,
                                Date = DateTime.Now,
                                SysAccountStdTypeId = null,
                                SieKpTyp = null,
                            };
                            SetCreatedProperties(accountHistory);
                            entities.AccountHistory.AddObject(accountHistory);

                            #endregion

                            #endregion
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, this.log);

                        //Conflict: Exception
                        accountStdItem.AddConflict(SieConflict.Exception, ex.Message);
                        return new ActionResult((int)ActionResultSave.Unknown, ex.Message);
                    }
                }

                var result = SaveChangesWithTransaction(entities);
                perfHelper.Checkpoint("SaveAccountStds");
                return result;
            }
        }

        /// <summary>
        /// Saves imported AccountBalance.
        /// 
        /// - A AccountBalance is inserted if Balance for the given Year and Account doesnt exist
        /// - Fails if the Balance for the given Year and Balance already exist
        /// </summary>
        private ActionResult SaveAccountStdBalance()
        {
            #region Init

            if (ic.OverrideAccountBalance)
            {
                //delete all balance
                AccountBalanceManager(actorCompanyId).DeleteAllAccountYearBalanceHead(ic.AccountYearId, actorCompanyId);
            }

            if (ic.AccountStdInBalanceItems == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            if (ic.AccountStdInBalanceItems.Count == 0)
                return new ActionResult();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prerq

                AccountDim accountDimStd = AccountManager.GetAccountDimStd(entities, actorCompanyId);
                List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, null, true, true);

                List<AccountStd> updatedAccountStds = new List<AccountStd>();
                AccountYear currentAccountYear = null;

                #endregion

                int accountStdInBalanceCounter = 0;
                foreach (SieAccountStdInBalanceItem accountStdBalanceItem in ic.AccountStdInBalanceItems)
                {
                    //Behandla inte ingående balans fg år
                    if (!accountStdBalanceItem.UseUBInsteadOfIB && accountStdBalanceItem.AccountYear == -1)
                        continue;
                    //Behandla inte utgående balans i år
                    if (accountStdBalanceItem.UseUBInsteadOfIB && accountStdBalanceItem.AccountYear == 0)
                    {
                        if (!ic.AccountStdInBalanceItems.Any(i => i.AccountNr == accountStdBalanceItem.AccountNr && i.AccountYear == -1))
                            accountStdBalanceItem.AddConflict(SieConflict.Import_AccountBalanceUseUBInsteadOfIB_PreviousYearNotInFile, accountStdBalanceItem.AccountNr);
                        continue;
                    }
                    //Utgående balans fg år -> Ingående balans i år
                    if (accountStdBalanceItem.UseUBInsteadOfIB && accountStdBalanceItem.AccountYear == -1)
                        accountStdBalanceItem.AccountYear = 0;

                    try
                    {
                        #region Validate

                        if (accountStdBalanceItem.Invalid)
                            continue;

                        #endregion

                        #region AccountStd

                        //Check that AccountStd exist
                        AccountStd accountStd = accountStds.FirstOrDefault(a => a.Account.AccountNr == accountStdBalanceItem.AccountNr);
                        if (accountStd != null)
                        {
                            //Check AccountDim rules
                            if (!this.IsAccountDimRuleFulfilled(accountStdBalanceItem, accountDimStd))
                            {
                                //Conflict: AccountStd not furfill rules of AccountDim
                                accountStdBalanceItem.AddConflict(SieConflict.Import_AccountRuleFailed, accountStdBalanceItem.AccountNr);
                                return new ActionResult((int)ActionResultSave.AccountDimRuleNotFulfilled);
                            }
                        }
                        else
                        {
                            //Conflict: AccountStd doesnt exist
                            accountStdBalanceItem.AddConflict(SieConflict.Import_AccountDoesNotExist, accountStdBalanceItem.AccountNr);
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStdBalanceItem");
                        }

                        #endregion

                        #region AccountYear

                        AccountYear accountYear = null;

                        //Get current AccountYear
                        if (currentAccountYear == null)
                            currentAccountYear = AccountManager.GetAccountYear(entities, ic.AccountYear.AccountYearId);

                        if (accountStdBalanceItem.AccountYear == 0)
                        {
                            #region Current AccountYear

                            accountYear = currentAccountYear;

                            #endregion
                        }
                        else
                        {
                            #region Previous AccountYear

                            accountYear = AccountManager.GetPreviousAccountYearFromNr(entities, currentAccountYear, accountStdBalanceItem.AccountYear);
                            if (accountYear != null)
                            {
                                //Check if AccountYear is open (or locked/closed AccountYear is allowed)
                                if (!ic.AllowNotOpenAccountYear && accountYear.Status != (int)TermGroup_AccountStatus.Open)
                                {
                                    //Conflict: AccountYear isnt open
                                    accountStdBalanceItem.AddConflict(SieConflict.Import_AccountYearIsNotOpen, accountYear.GetFromToShortString());
                                    return new ActionResult((int)ActionResultSave.AccountYearNotOpen);
                                }
                            }
                            else
                            {
                                //Conflict: Unknown AccountYear
                                accountStdBalanceItem.AddConflict(SieConflict.Import_AccountBalanceHasUnknownAccountYear, accountStdBalanceItem.AccountYear);
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStdBalanceItem");
                            }

                            #endregion
                        }

                        #endregion

                        #region AccountYearBalanceHead

                        decimal balance = Decimal.Round(accountStdBalanceItem.Balance, 2);

                        //Check for SIE mappings
                        AccountYearBalanceHead accountYearBalanceHead = AccountBalanceManager(actorCompanyId).GetAccountYearBalanceHeadByAccountNr(entities, accountYear.AccountYearId, accountStdBalanceItem.AccountNr);
                        if (accountYearBalanceHead == null)
                        {
                            #region Add

                            accountYearBalanceHead = new AccountYearBalanceHead()
                            {
                                AccountYear = accountYear,
                                AccountStd = accountStd,
                                Balance = balance,
                                Quantity = accountStdBalanceItem.Quantity.HasValue ? Decimal.Round(accountStdBalanceItem.Quantity.Value, 6) : accountStdBalanceItem.Quantity,
                            };
                            SetCreatedProperties(accountYearBalanceHead);
                            entities.AccountYearBalanceHead.AddObject(accountYearBalanceHead);

                            //Set currency amounts
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, accountYearBalanceHead);

                            updatedAccountStds.Add(accountStd);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            if (ic.OverrideAccountBalance)
                            {
                                accountYearBalanceHead.Balance = balance;
                                accountYearBalanceHead.Quantity = accountStdBalanceItem.Quantity.HasValue ? Decimal.Round(accountStdBalanceItem.Quantity.Value, 6) : accountStdBalanceItem.Quantity;
                                updatedAccountStds.Add(accountStd);
                            }
                            else
                            {
                                //Conflict: AccountBalance exist for Account in AccountYear
                                accountStdBalanceItem.AddConflict(SieConflict.Import_AccountBalanceExistInAccountYear, accountStdBalanceItem.ToStringYear(accountYear));
                                return new ActionResult((int)ActionResultSave.AccountBalanceExists);
                            }

                            #endregion
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, this.log);

                        //Conflict: Exception
                        accountStdBalanceItem.AddConflict(SieConflict.Exception, ex.Message);
                        return new ActionResult((int)ActionResultSave.Unknown, ex.Message);
                    }
                    finally
                    {
                        accountStdInBalanceCounter++;
                    }
                }

                ActionResult result = SaveChangesWithTransaction(entities);
                if (result.Success)
                {
                    //Update balance on all accounts that was updated
                    AccountBalanceManager(actorCompanyId).CalculateAccountBalanceForAccountsInAccountYear(entities, actorCompanyId, ic.AccountYearId, updatedAccountStds);
                }
                return result;
            }
        }

        private ActionResult SaveAccountInternalBalance()
        {
            /// <summary>
            /// Saves imported AccountBalance.
            /// 
            /// - A AccountInternalBalance is inserted if Balance for the given Year and Account doesnt exist
            /// - Fails if the Balance for the given Year and Balance already exist
            /// </summary>
            {
                #region Init

                if (ic.AccountInternalInBalanceItems == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull);

                if (ic.AccountInternalInBalanceItems.Count == 0)
                    return new ActionResult();

                #endregion


                using (var entities = new CompEntities())
                {
                    #region Prerq

                    List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId);
                    List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, null, true, true);
                    List<Account> accountInternals = AccountManager.GetAccountsInternalsByCompany(ActorCompanyId, loadAccountDim: true);
                    List<Account> updatedAccounts = new List<Account>();
                    AccountYear currentAccountYear = null;

                    #endregion
                    int accountInternalInBalanceCounter = 0;
                    foreach (SieAccountInternalInBalanceItem accountInternalBalanceItem in ic.AccountInternalInBalanceItems)
                    {
                        //Behandla inte ingående balans fg år
                        if (!accountInternalBalanceItem.UseUBInsteadOfIB && accountInternalBalanceItem.AccountYear == -1)
                            continue;
                        //Behandla inte utgående balans i år
                        if (accountInternalBalanceItem.UseUBInsteadOfIB && accountInternalBalanceItem.AccountYear == 0)
                        {
                            if (!ic.AccountInternalInBalanceItems.Any(i => i.AccountNr == accountInternalBalanceItem.AccountNr && i.AccountYear == -1))
                                accountInternalBalanceItem.AddConflict(SieConflict.Import_AccountBalanceUseUBInsteadOfIB_PreviousYearNotInFile, accountInternalBalanceItem.AccountNr);
                            continue;
                        }
                        //Utgående balans fg år -> Ingående balans i år
                        if (accountInternalBalanceItem.UseUBInsteadOfIB && accountInternalBalanceItem.AccountYear == -1)
                            accountInternalBalanceItem.AccountYear = 0;

                        try
                        {
                            #region Validate

                            if (accountInternalBalanceItem.Invalid)
                                continue;

                            #endregion

                            #region AccountInternal

                            //Check that AccountInternal exist
                            var x = accountInternalBalanceItem.ObjectItems.FirstOrDefault();
                            if (x.ObjectCode != null)
                            {
                                var accountInternal = accountInternals.FirstOrDefault(a => a.AccountNr == x.ObjectCode && a.AccountDim.SysSieDimNr == x.SieDimension);
                                if (accountInternal == null)
                                {
                                    //Conflict: AccountStd doesnt exist
                                    accountInternalBalanceItem.AddConflict(SieConflict.Import_AccountDoesNotExist, accountInternalBalanceItem.AccountNr);
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStdBalanceItem");
                                }
                            }

                            #endregion

                            #region AccountYear

                            AccountYear accountYear = null;

                            //Get current AccountYear
                            if (currentAccountYear == null)
                                currentAccountYear = AccountManager.GetAccountYear(entities, ic.AccountYear.AccountYearId);

                            if (accountInternalBalanceItem.AccountYear == 0)
                            {
                                #region Current AccountYear

                                accountYear = currentAccountYear;

                                #endregion
                            }
                            else
                            {
                                #region Previous AccountYear

                                accountYear = AccountManager.GetPreviousAccountYearFromNr(entities, currentAccountYear, accountInternalBalanceItem.AccountYear);
                                if (accountYear != null)
                                {
                                    //Check if AccountYear is open (or locked/closed AccountYear is allowed)
                                    if (!ic.AllowNotOpenAccountYear && accountYear.Status != (int)TermGroup_AccountStatus.Open)
                                    {
                                        //Conflict: AccountYear isnt open
                                        accountInternalBalanceItem.AddConflict(SieConflict.Import_AccountYearIsNotOpen, accountYear.GetFromToShortString());
                                        return new ActionResult((int)ActionResultSave.AccountYearNotOpen);
                                    }
                                }
                                else
                                {
                                    //Conflict: Unknown AccountYear
                                    accountInternalBalanceItem.AddConflict(SieConflict.Import_AccountBalanceHasUnknownAccountYear, accountInternalBalanceItem.AccountYear);
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStdBalanceItem");
                                }

                                #endregion
                            }

                            #endregion

                            #region AccountYearBalanceHead

                            decimal balance = Decimal.Round(accountInternalBalanceItem.Balance, 2);
                            //Check for SIE mappings
                            List<AccountYearBalanceHead> listaccountYearBalanceHeads = AccountBalanceManager(actorCompanyId).GetAccountYearBalanceHeadsByAccountNr(entities, accountYear.AccountYearId, accountInternalBalanceItem.AccountNr);
                            //Check that AccountInternal exist

                            var bh = listaccountYearBalanceHeads.FirstOrDefault(a => a.AccountInternal.Any(y => y.Account.AccountNr == x.ObjectCode && y.Account.AccountDim.SysSieDimNr == x.SieDimension));

                            if (bh == null)
                            {
                                #region Add

                                Account account = AccountManager.GetAccountByDimNr(accountInternalBalanceItem.AccountNr, Constants.ACCOUNTDIM_STANDARD, actorCompanyId, loadAccount: true);
                                AccountStd accountStd = accountStds.FirstOrDefault(a => a.Account.AccountNr == accountInternalBalanceItem.AccountNr);

                                AccountYearBalanceHead accountYearBalanceHead = new AccountYearBalanceHead();
                                {
                                    accountYearBalanceHead.AccountYear = accountYear;
                                    accountYearBalanceHead.AccountStd = accountStd;
                                    accountYearBalanceHead.Balance = balance;
                                    accountYearBalanceHead.Quantity = accountInternalBalanceItem.Quantity.HasValue ? Decimal.Round(accountInternalBalanceItem.Quantity.Value, 6) : accountInternalBalanceItem.Quantity;
                                }
                                SetCreatedProperties(accountYearBalanceHead);

                                //lägg till internkonto
                                if (x.ObjectCode != null)
                                {
                                    AccountDim accountDim = accountDims.FirstOrDefault(ad => ad.SysSieDimNr == x.SieDimension);
                                    if (accountDim != null)
                                    {
                                        account = AccountManager.GetAccountByDimNr(entities, x.ObjectCode, accountDim.AccountDimNr, actorCompanyId, loadAccount: true);
                                        if (account?.AccountInternal != null)
                                            accountYearBalanceHead.AccountInternal.Add(account.AccountInternal);
                                    }
                                }
                                entities.AccountYearBalanceHead.AddObject(accountYearBalanceHead);

                                //Set currency amounts
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, accountYearBalanceHead);

                                updatedAccounts.Add(account);

                                #endregion
                            }
                            else
                            {
                                #region Update
                                var accountYearBalanceHead = bh;
                                Account account = AccountManager.GetAccountByDimNr(accountInternalBalanceItem.AccountNr, Constants.ACCOUNTDIM_STANDARD, actorCompanyId, loadAccount: true);
                                if (ic.OverrideAccountBalance)
                                {
                                    accountYearBalanceHead.Balance = balance;
                                    accountYearBalanceHead.Quantity = accountInternalBalanceItem.Quantity.HasValue ? Decimal.Round(accountInternalBalanceItem.Quantity.Value, 6) : accountInternalBalanceItem.Quantity;
                                    updatedAccounts.Add(account);
                                }
                                else
                                {
                                    //Conflict: AccountBalance exist for Account in AccountYear
                                    accountInternalBalanceItem.AddConflict(SieConflict.Import_AccountBalanceExistInAccountYear, accountInternalBalanceItem.ToStringYear(accountYear));
                                    return new ActionResult((int)ActionResultSave.AccountBalanceExists);
                                }

                                #endregion
                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            base.LogError(ex, this.log);

                            //Conflict: Exception
                            accountInternalBalanceItem.AddConflict(SieConflict.Exception, ex.Message);
                            return new ActionResult((int)ActionResultSave.Unknown, ex.Message);
                        }
                        accountInternalInBalanceCounter++;
                    }
                    ActionResult result = SaveChangesWithTransaction(entities);
                    if (result.Success)
                    {
                        //Update balance on all accounts that was updated
                        AccountBalanceManager(actorCompanyId).CalculateAccountBalanceForAccountsInAccountYear(entities, actorCompanyId, ic.AccountYearId, updatedAccounts);
                    }
                    return result;
                }

            }
        }
        /// <summary>
        /// Saves imported Vouchers
        /// </summary>
        private ActionResult SaveVouchers(bool useAccountDistribution = false, Guid batchId = default)
        {
            #region Init

            var result = new ActionResult();

            if (ic.VoucherItems == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            if (ic.VoucherItems.Count == 0)
                return new ActionResult();

            #endregion

            List<AccountStd> accountStdsUsed = new List<AccountStd>();
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId);
                List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, null, true, false);
                var accountStdsLookup = accountStds
                    .DistinctBy(a => a.Account.AccountNr)
                    .ToDictionary(a => a.Account.AccountNr);
                var accountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, true);
                var accountInternalsLookup = accountInternals
                    .DistinctBy(ai => new { ai.Account.AccountNr, ai.Account.AccountDimId })
                    .ToDictionary(ai => new { ai.Account.AccountNr, ai.Account.AccountDimId });
                List<VoucherSeries> voucherSeries = VoucherManager.GetVoucherSeriesByYear(entities, ic.AccountYearId, actorCompanyId, false);
                List<AccountPeriod> accountPeriods = AccountManager.GetAccountPeriods(entities, ic.AccountYearId, false);

                var accountStdIdUsed = new HashSet<int>();

                #endregion

                var vouchers = new List<VoucherHead>();
                foreach (SieVoucherItem voucherItem in ic.VoucherItems)
                {
                    try
                    {
                        #region Validate

                        //Should been taken care of in PrereqVouchers
                        if (!voucherItem.VoucherNr.HasValue)
                            continue;

                        #endregion

                        #region VoucherHead

                        if (ic.SkipAlreadyExistingVouchers)
                        {
                            int noOfVouchers = (from vh in VoucherHeads
                                                where ((vh.VoucherNr == voucherItem.VoucherNr.Value) &&
                                                (vh.VoucherSeriesId == voucherItem.VoucherSeries.VoucherSeriesId) &&
                                                (vh.AccountPeriod.AccountYearId == voucherItem.AccountPeriod.AccountYearId))
                                                select vh).Count();

                            if (noOfVouchers > 0)
                                continue;
                        }

                        VoucherHead voucherHead = new VoucherHead()
                        {
                            VoucherNr = voucherItem.VoucherNr.Value,
                            Date = voucherItem.VoucherDate.Date,
                            Text = voucherItem.Text,
                            Status = voucherItem.AccountPeriod.Status,

                            //Set FK
                            ActorCompanyId = actorCompanyId,

                            //Link to FileImportHead
                            BatchId = batchId,
                        };
                        vouchers.Add(voucherHead);
                        SetCreatedProperties(voucherHead);
                        #region VoucherSeries

                        voucherHead.VoucherSeries = (from vs in voucherSeries
                                                     where vs.VoucherSeriesId == voucherItem.VoucherSeries.VoucherSeriesId
                                                     select vs).FirstOrDefault();

                        voucherHead.VoucherSeries.VoucherNrLatest = voucherHead.VoucherNr;
                        voucherHead.VoucherSeries.VoucherDateLatest = voucherHead.Date;
                        SetModifiedProperties(voucherHead.VoucherSeries);

                        #endregion

                        #region AccountPeriod

                        voucherHead.AccountPeriod = (from ap in accountPeriods
                                                     where ap.AccountYearId == voucherItem.AccountYear.AccountYearId &&
                                                     ap.From <= voucherHead.Date &&
                                                     ap.To >= voucherHead.Date
                                                     select ap).FirstOrDefault();

                        #endregion

                        #endregion

                        var voucherRows = new List<VoucherRow>();
                        int voucherRowCounter = 1;
                        foreach (SieTransactionItem transactionItem in voucherItem.GetTransactionItems())
                        {
                            #region VoucherRow

                            VoucherRow voucherRow = new VoucherRow()
                            {
                                Text = transactionItem.Text,
                                Date = transactionItem.TransactionDate,
                                Amount = Decimal.Round(transactionItem.Amount, 2),
                                Quantity = transactionItem.Quantity.HasValue ? Decimal.Round(transactionItem.Quantity.Value, 6) : transactionItem.Quantity,
                                RowNr = voucherRowCounter++,
                            };

                            //Set currency amounts
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);

                            #region AccountStd

                            accountStdsLookup.TryGetValue(transactionItem.AccountNr, out AccountStd accountStd);
                            //check if AccountStd is enabled

                            voucherRow.AccountStd = accountStd;
                            if (voucherRow.AccountStd != null && !accountStdIdUsed.Contains(voucherRow.AccountStd.AccountId))
                            {
                                accountStdIdUsed.Add(voucherRow.AccountStd.AccountId);
                                accountStdsUsed.Add(voucherRow.AccountStd);
                            }


                            #endregion

                            #region AccountInternals

                            foreach (SieObjectItem objectItem in transactionItem.ObjectItems)
                            {

                                //Get AccountDim
                                AccountDim accountDim = accountDims.FirstOrDefault(ad => ad.SysSieDimNr == objectItem.SieDimension);

                                //Get AccountInternal
                                accountInternalsLookup.TryGetValue(new { AccountNr = objectItem.ObjectCode, accountDim.AccountDimId }, out AccountInternal accountInternal);

                                if (accountInternal != null)
                                {
                                    //Add VoucherRowAccount to VoucherRow
                                    voucherRow.AccountInternal.Add(accountInternal);
                                }
                            }

                            #endregion

                            #endregion

                            /*#region VoucherRowHistory

                            VoucherManager.AddVoucherRowHistory(entities, voucherRow, NumberUtility.GetFormattedDecimalValue(voucherRow.Amount, 2), NumberUtility.GetFormattedDecimalValue(voucherRow.Quantity, 6), false, false, actorCompanyId);

                            #endregion*/

                            voucherRows.Add(voucherRow);
                        }

                        #region Account distribution

                        if (useAccountDistribution)
                        {
                            var distributedRows = VoucherManager.ApplyAutomaticAccountDistribution(entities, voucherRows, accountStds, accountDims, accountInternals, actorCompanyId, true, null);
                            voucherHead.VoucherRow.Clear();
                            voucherHead.VoucherRow.AddRange(distributedRows);
                        }
                        else
                        {
                            voucherHead.VoucherRow.AddRange(voucherRows);
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, this.log);

                        //Conflict: Exception
                        voucherItem.AddConflict(SieConflict.Exception, ex.Message);
                        return new ActionResult((int)ActionResultSave.Unknown, ex.Message);
                    }
                }

                result = this.UseOptimizedImport ? SaveVouchersWithBulkInsert(entities, vouchers) : SaveChangesWithTransaction(entities);
                perfHelper.Checkpoint("SaveVouchers done");
            }
            if (result.Success)
            {
                using (var entities = new CompEntities())
                {
                    //Update balance on all accounts
                    AccountBalanceManager(actorCompanyId).CalculateAccountBalanceForAccountsInAccountYearOptimized(entities, actorCompanyId, ic.AccountYearId, accountStdsUsed);
                    perfHelper.Checkpoint("CalculateAccountBalanceForAccountsInAccountYearOptimized done");
                }
            }
            return result;
        }

        private ActionResult SaveVouchersWithBulkInsert(CompEntities entities, List<VoucherHead> vouchers)
        {
            var uniqueVoucherHeadIds = this.VoucherHeads.Select(vh => vh.VoucherHeadId).ToHashSet();
            return VoucherManager.BulkInsertVouchers(entities, actorCompanyId, ic.AccountYearId, vouchers, uniqueVoucherHeadIds);
        }

        /// <summary>
        /// Delete existing Vouchers
        /// </summary>
        private ActionResult DeleteExistingVouchers()
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {

                try
                {
                    if (entities.Connection.State != System.Data.ConnectionState.Open)
                        entities.Connection.Open();

                    var transactionOptions = new TransactionOptions
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                        Timeout = TransactionManager.MaximumTimeout
                    };
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_SUPPRESS, transactionOptions))
                    {
                        var voucherHeads = (from vh in entities.VoucherHead
                                            .Include("VoucherSeries")
                                            .Include("VoucherSeries.VoucherSeriesType")
                                            .Include("VoucherSeries.AccountYear")
                                            .Include("VoucherRow")
                                            .Include("VoucherRow.AccountInternal")
                                            .Include("VoucherRow.VoucherRowHistory")
                                            .Include("Invoice")
                                            .Include("Invoice1")
                                            .Include("AccountDistributionEntry")
                                            .Include("AccountDistributionLog")
                                            .Include("InventoryLog")
                                            .Include("PaymentRow")
                                            .Include("HouseholdTaxDeductionRow")
                                            .Include("InvoicePaymentMatchingRecord")
                                            where ((vh.VoucherSeries.AccountYear.ActorCompanyId == ic.ActorCompanyId) &&
                                            (vh.VoucherSeries.AccountYear.AccountYearId == ic.AccountYear.AccountYearId) &&
                                            (vh.Invoice.Count == 0) && (vh.Invoice1.Count == 0) &&
                                            (vh.AccountDistributionEntry.Count == 0) && (vh.AccountDistributionLog.Count == 0) &&
                                            (vh.InventoryLog.Count == 0) && (vh.PaymentRow.Count == 0) &&
                                            (vh.HouseholdTaxDeductionRow.Count == 0) && (vh.InvoicePaymentMatchingRecord.Count == 0))
                                            select vh).ToList();

                        for (int vh = voucherHeads.Count - 1; vh >= 0; vh--)
                        {
                            var voucherHead = voucherHeads[vh];
                            if (ic.OverrideVoucherDeletes || (ic.VoucherSeriesDeleteDict != null && ic.VoucherSeriesDeleteDict.ContainsKey(voucherHead.VoucherSeriesReference.Value.VoucherSeriesTypeId)))
                            {
                                for (int vr = voucherHead.VoucherRow.Count - 1; vr >= 0; vr--)
                                {
                                    var voucherRow = voucherHead.VoucherRow.ToList()[vr];
                                    for (int vha = voucherRow.AccountInternal.Count - 1; vha >= 0; vha--)
                                    {
                                        var accountInternal = voucherRow.AccountInternal.ToList()[vha];
                                        entities.DeleteObject(accountInternal);
                                    }

                                    for (int vrh = voucherRow.VoucherRowHistory.Count - 1; vrh >= 0; vrh--)
                                    {
                                        var voucherRowHistory = voucherRow.VoucherRowHistory.ToList()[vrh];
                                        entities.DeleteObject(voucherRowHistory);
                                    }
                                    entities.DeleteObject(voucherRow);
                                }
                                entities.DeleteObject(voucherHead);
                            }
                        }



                        if (result.Success)
                            result = SaveChanges(entities, transaction);


                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        perfHelper.Checkpoint("DeleteExistingVouchers done");
                    }

                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
                perfHelper.Checkpoint("DeleteExistingVouchers done");
                return result;
            }
        }

        #endregion

        public ActionResult ReverseSieImport(int actorCompanyId, SieReverseImportDTO importReverseRequest)
        {
            ActionResult result = new ActionResult();
            try
            {
                if (string.IsNullOrWhiteSpace(importReverseRequest.Comment))
                    return new ActionResult(GetText(12553, "Ange orsak till att ångra ändringen."));

                using (CompEntities entities = new CompEntities())
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    try
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_READUNCOMMITED))
                        {
                            var fileImportHead = entities
                                                   .FileImportHead
                                                   .FirstOrDefault(f =>
                                                       f.FileImportHeadId == importReverseRequest.FileImportHeadId &&
                                                       f.ActorCompanyId == actorCompanyId);

                            if (fileImportHead is null)
                                return new ActionResult(GetText(12549));

                            #region Validate & Delete Vouchers

                            #region Validate Vouchers

                            var vouchers = (from vh in entities.VoucherHead
                                           .Include("VoucherSeries")
                                            where vh.BatchId == fileImportHead.BatchId
                                            select vh).ToList();

                            var voucherSeriesMaxNrs = vouchers
                                                        .GroupBy(v => new
                                                        {
                                                            v.VoucherSeriesId,
                                                            v.VoucherSeries.VoucherNrLatest,
                                                        })
                                                        .Select(x => new
                                                        {
                                                            x.Key.VoucherSeriesId,
                                                            VoucherSeriesLatestNr = x.Key.VoucherNrLatest,
                                                            VocherHeadMaxVoucherNr = x.Max(v => v.VoucherNr),
                                                        }).ToList();

                            if (voucherSeriesMaxNrs.Count(y => y.VoucherSeriesLatestNr < y.VocherHeadMaxVoucherNr) > 0)
                                return new ActionResult(GetText(12550));
                            #endregion

                            #region Delete Vouchers
                            int[] voucherHeadIds = vouchers
                                                    .Select(v => v.VoucherHeadId)
                                                    .ToArray();

                            var voucherHeads = (from vh in entities.VoucherHead
                                                .Include("VoucherRow")
                                                .Include("VoucherRow.AccountInternal")
                                                .Include("VoucherRow.VoucherRowHistory")
                                                where voucherHeadIds.Contains(vh.VoucherHeadId)
                                                orderby vh.VoucherNr descending
                                                select vh).ToList();

                            for (int vh = voucherHeads.Count - 1; vh >= 0; vh--)
                            {
                                var voucherHead = voucherHeads[vh];
                                for (int vr = voucherHead.VoucherRow.Count - 1; vr >= 0; vr--)
                                {
                                    var voucherRow = voucherHead.VoucherRow.ToList()[vr];
                                    for (int vha = voucherRow.AccountInternal.Count - 1; vha >= 0; vha--)
                                    {
                                        var accountInternal = voucherRow.AccountInternal.ToList()[vha];
                                        entities.DeleteObject(accountInternal);
                                    }
                                    for (int vrh = voucherRow.VoucherRowHistory.Count - 1; vrh >= 0; vrh--)
                                    {
                                        var voucherRowHistory = voucherRow.VoucherRowHistory.ToList()[vrh];
                                        entities.DeleteObject(voucherRowHistory);
                                    }
                                    entities.DeleteObject(voucherRow);
                                }
                                entities.DeleteObject(voucherHead);
                            }

                            result = SaveChanges(entities);

                            #region Update VoucherSeries - VoucherNrLatest
                            int[] voucherSeriesIds = voucherSeriesMaxNrs.Select(x => x.VoucherSeriesId).ToArray();
                            var maxVoucherNrs = entities
                                                    .VoucherHead
                                                    .Where(vh => voucherSeriesIds.Contains(vh.VoucherSeriesId))
                                                    .GroupBy(v => v.VoucherSeriesId)
                                                    .Select(g => new
                                                    {
                                                        VoucherSeriesId = g.Key,
                                                        MaxVoucherNr = g.Select(v => v.VoucherNr).DefaultIfEmpty(0).Max()
                                                    }).ToList();

                            var voucherSeries = entities
                                                    .VoucherSeries
                                                    .Where(vs =>
                                                        voucherSeriesIds.Contains(vs.VoucherSeriesId))
                                                    .ToList();

                            foreach (var voucherSeriesItem in voucherSeries)
                            {
                                var maxVoucherNr = maxVoucherNrs.FirstOrDefault(x => x.VoucherSeriesId == voucherSeriesItem.VoucherSeriesId);

                                if (maxVoucherNr != null)
                                {
                                    voucherSeriesItem.VoucherNrLatest = maxVoucherNr.MaxVoucherNr;
                                    voucherSeriesItem.VoucherDateLatest = DateTime.Now;
                                }
                            }
                            #endregion

                            #endregion
                            #endregion


                            #region Updare FileImportHead

                            fileImportHead.Status = (int)TermGroup_FileImportStatus.Reversed;
                            fileImportHead.SystemMessage = GetText(12552, "Backad");
                            fileImportHead.Comment = importReverseRequest.Comment;
                            SetModifiedProperties(fileImportHead);

                            #endregion              
                            result = SaveChanges(entities);
                            transaction.Complete();
                        }
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, this.log);
                        result.Exception = ex;
                        result.IntegerValue = 0;
                    }
                    finally
                    {
                        if (!result.Success)
                            base.LogTransactionFailed(this.ToString(), this.log);

                        entities.Connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }
            return result;
        }

        #endregion

        #region Export

        public ActionResult Export(SieExportDTO dto)
        {
            ActionResult result = new ActionResult();
            SieExportContainer container = new SieExportContainer();

            string filePath = ConfigSettings.SOE_SERVER_DIR_TEMP_SIE_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_SIE_SUFFIX;

            container.ActorCompanyId = dto.ActorCompanyId;
            container.LoginName = dto.LoginName;
            container.Program = dto.Program;
            container.Version = dto.Version;

            container.ExportType = dto.ExportType;
            container.Comment = dto.Comment;
            container.ExportPreviousYear = dto.ExportPreviousYear;
            container.ExportAccount = dto.ExportAccount;
            container.ExportAccountType = dto.ExportAccountType;
            container.ExportObject = dto.ExportObject;
            container.ExportSruCodes = dto.ExportSruCodes;
            container.VoucherSortBy = dto.sortVoucherBy;

            container.Es = new EvaluatedSelection();
            container.Es.ActorCompanyId = dto.ActorCompanyId;
            container.Es.SSTD_AccountYearId = dto.AccountingYearId;
            container.Es.HasDateInterval = true;
            container.Es.DateFrom = dto.DateFrom;
            container.Es.DateTo = dto.DateTo;

            container.Es.OnlyActiveAccounts = true;

            container.Es.SSTD_BudgetId = dto.BudgetHeadId;

            container.Es.SA_HasAccountInterval = false;
            dto.AccountSelection = dto.AccountSelection.Where(x => x.AccountDimId > 0).ToList();
            if (dto.AccountSelection.Count > 0)
            {
                container.Es.SA_HasAccountInterval = true;
                container.Es.SA_AccountIntervals = new List<AccountIntervalDTO>();
                container.Es.SA_AccountIntervals.AddRange(dto.AccountSelection.ToAccountIntervalDTOs());
            }

            #region Old Voucher seleciton handling
            // This region can be removed when old page is no longer in use
            container.Es.SV_HasVoucherSeriesTypeNrInterval = false;
            if (dto.VoucherSeriesId > 0)
            {
                container.Es.SV_HasVoucherSeriesTypeNrInterval = true;
                container.Es.SV_VoucherSeriesTypeNrFrom = dto.VoucherSeriesId;
                container.Es.SV_VoucherSeriesTypeNrTo = dto.VoucherSeriesId;
            }
            else
            {
                container.Es.SV_VoucherSeriesTypeNrFrom = container.Es.SV_VoucherSeriesTypeNrTo = -1;
            }

            container.Es.SV_HasVoucherNrInterval = false;
            if (dto.VoucherNoFrom > 0 || dto.VoucherNoTo > 0)
            {
                container.Es.SV_HasVoucherNrInterval = true;
                container.Es.SV_VoucherNrFrom = dto.VoucherNoFrom;
                container.Es.SV_VoucherNrTo = dto.VoucherNoTo;
            }
            else
            {
                container.Es.SV_VoucherNrFrom = container.Es.SV_VoucherNrTo = -1;
            }
            #endregion

            #region Vouchern selection handling
            dto.VoucherSelection = dto.VoucherSelection.Where(x => x.VoucherSeriesId > 0).ToList();
            if (dto.VoucherSelection.Count > 0)
            {
                container.Es.SV_HasVoucherInterval = true;
                container.Es.SV_VoucherIntervals = new List<VoucherIntervalDTO>();
                container.Es.SV_VoucherIntervals.AddRange(dto.VoucherSelection.ToVoucherIntervalDTOs());
            }
            #endregion

            using (TextWriter writer = new StreamWriter(filePath, false, Constants.ENCODING_IBM437))
			{
                container.StreamReader = writer;

                try
                {
                    result = this.Export(container);
                }
                finally
                {
                    if (!(container.StreamReader is null))
                        container.CloseWriter();
                    if (!result.Success)
                        File.Delete(filePath);
                }
            }

            if (result.Success)
            {
                result.BooleanValue = ec.HasConflicts();
                if (ec.HasConflicts())
                {
                    result.ErrorMessage = GetText(1432, "SIE export klar") + ". " + GetText(1169, "Konflikter uppstod");
                    result.Value2 = this.GetConflictsDTO(ec.GetConflicts());
                }
                else
                    result.InfoMessage = GetText(1432, "SIE export klar");

                int exportType = (int)dto.ExportType > (int)SieExportType.Type4 ? (int)SieExportType.Type4 : (int)dto.ExportType;
                string fileName = Constants.SOE_SERVER_FILENAME_PREFIX + Constants.SOE_SERVER_SIE_PREFIX + exportType + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Constants.SOE_SERVER_FILE_SIE_SUFFIX;

                result.Value = new SieExportResultDTO
                {
                    FileName = fileName,
                    Content = Convert.ToBase64String(File.ReadAllBytes(filePath)),
                    FileType = "text/plain"
                };
            }
            else
            {
                result.ErrorMessage = GetText(1431, "SIE export misslyckades: ") + result.ErrorMessage;
                if (ec.HasConflicts())
                {
                    result.ErrorMessage = $"{result.ErrorMessage}. {GetText(1169, "Konflikter uppstod")}";
                    result.Value2 = this.GetConflictsDTO(ec.GetConflicts());
                }
            }

            return result;
        }

        private List<SieExportConflictDTO> GetConflictsDTO(List<SieImportItemBase> conflicts)
        {
            List<SieExportConflictDTO> conflictDTOs = new List<SieExportConflictDTO>();

            foreach (SieImportItemBase conflict in conflicts)
            {
                SieExportConflictDTO conflictDto = new SieExportConflictDTO();
                conflictDto.Label = conflict.Label;
                conflictDto.Message = "";

                foreach (SieConflictItem conflictItem in conflict.Conflicts)
                {
                    string conflictMsg = "";
                    switch (conflictItem.Conflict)
                    {
                        //General
                        case SieConflict.Exception:
                            conflictMsg = conflictItem.StrData;
                            break;

                        //Export
                        case SieConflict.Export_WriteFailed:
                            conflictMsg = GetText(1487, "Kunde inte skriva rad");
                            conflictMsg = (conflictItem.StrData != null) ? conflictMsg + " [" + conflictItem.StrData + "]" : conflictMsg;
                            break;
                        case SieConflict.Export_AccountStdIsNotNumeric:
                            conflictMsg = GetText(1488, "Kontonr måste vara numeriskt");
                            conflictMsg = (conflictItem.StrData != null) ? conflictMsg + " [" + conflictItem.StrData + "]" : conflictMsg;
                            break;
                    }
                    if (String.IsNullOrEmpty(conflictDto.Message))
                        conflictDto.Message = conflictMsg;
                    else
                        conflictDto.Message += "\r\n" + conflictMsg;
                }

                conflictDTOs.Add(conflictDto);
            }

            return conflictDTOs;
        }

        /// <summary>
        /// SIE export for Type1 (bokslutssaldon), Type2 (periodsaldon), Type3 (objektsaldon) and Type4 (transaktioner)
        /// </summary>
        /// <param name="container">The SieExportContainer with all parameters</param>
        /// <returns>True if the Export was successfull, otherwise false</returns>
        public ActionResult Export(SieExportContainer container)
        {
            try
            {
                this.ec = container;
                this.actorCompanyId = ec.ActorCompanyId;

                #region Prereq

                //Get AccountYear
                ec.AccountYear = AccountManager.GetAccountYear(ec.Es.SSTD_AccountYearId);

                if (ec.AccountYear == null)
                    return new ActionResult(GetText(10141, "Räkenskapsår kunde inte hittas"));


                if (ec.AccountYear != null)
                {
                    // AccountYearNr is always 0 for this year and -1 for prevoius year
                    //int? accountYearNr = am.GetAccountYearNr(ec.AccountYear.AccountYearId, actorCompanyId);
                    //if (accountYearNr != null)
                    //    ec.AccountYearNr = Convert.ToInt32(accountYearNr);
                    ec.AccountYearNr = 0;
                }

                //Get previous AccountYear
                if (ec.ExportPreviousYear && ec.AccountYear != null)
                    ec.PreviousAccountYear = AccountManager.GetPreviousAccountYear(ec.AccountYear);

                //Get Company and Contact
                var company = CompanyManager.GetCompanyDTO(actorCompanyId);
                if (company == null)
                    return new ActionResult(false);

                //Get AccountDim std
                AccountDim accountDimStd = AccountManager.GetAccountDimStd(actorCompanyId);
                if (accountDimStd == null)
                    return new ActionResult(false);

                //Get AccountDim internals
                List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(actorCompanyId);

                ec.OrgNr = company.OrgNr;
                ec.CompanyName = company.Name;

                Contact contact = ContactManager.GetContactFromActor(company.ActorCompanyId);
                if (contact != null)
                {
                    var contactAddressRows = ContactManager.GetContactAddressRows(contact.ContactId, (int)TermGroup_SysContactAddressType.Distribution);
                    foreach (ContactAddressRow contactAddressRow in contactAddressRows)
                    {
                        switch (contactAddressRow.SysContactAddressRowTypeId)
                        {
                            case (int)TermGroup_SysContactAddressRowType.Address:
                                ec.Address = contactAddressRow.Text;
                                break;
                            case (int)TermGroup_SysContactAddressRowType.PostalAddress:
                                ec.PostalAddress = contactAddressRow.Text;
                                break;
                            case (int)TermGroup_SysContactAddressRowType.PostalCode:
                                ec.PostalCode = contactAddressRow.Text;
                                break;
                        }
                    }

                    ContactECom contactECom = ContactManager.GetContactECom(contact.ContactId, (int)TermGroup_SysContactEComType.PhoneJob, false);
                    if (contactECom != null)
                    {
                        ec.Phone = contactECom.Text;
                    }
                }

                #endregion

                WriteCommonLines();

                using (var entities = new CompEntities())
                {
                    #region Prereq

                    //Get AccountDims
                    AccountDim accountDimStds = AccountManager.GetAccountDimStd(ec.ActorCompanyId);
                    EvaluatedSelection es = this.ec.Es;
                    es.ActorCompanyId = ec.ActorCompanyId;
                    List<AccountStd> accountStdsInInterval = new List<AccountStd>();
                    List<AccountInternal> accountInternalsInInterval = new List<AccountInternal>();

                    bool validSelection = AccountManager.GetAccountsInInterval(entities, es, accountDimStds, true, ref accountStdsInInterval, ref accountInternalsInInterval);

                    #endregion

                    switch (ec.ExportType)
                    {
                        case SieExportType.Type1:
                            WriteAccountStd(entities, accountStdsInInterval);
                            if (ec.Es.SA_AccountIntervals != null)
                            {
                                WriteAccountStdInAndOutBalances(entities, accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval));
                                WriteAccountStdYearBalance(entities, accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval));
                            }
                            else
                            {
                                WriteAccountStdInAndOutBalances(entities, accountStdsInInterval);
                                WriteAccountStdYearBalance(entities, accountStdsInInterval);
                            }

                            break;
                        case SieExportType.Type2:
                            WriteAccountStd(entities, accountStdsInInterval);
                            WriteAccountInternal(accountDimInternals, accountInternalsInInterval);

                            WriteAccountStdInAndOutBalances(entities, accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval));
                            WriteAccountStdYearBalance(entities, accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval));

                            WriteAccountPeriodBalance(accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval), true, true, !(ec.Es.SSTD_BudgetId is null) && ec.Es.SSTD_BudgetId.Value > 0);
                            break;
                        case SieExportType.Type3:
                            WriteAccountStd(entities, accountStdsInInterval);
                            WriteAccountInternal(accountDimInternals, accountInternalsInInterval);
                            if (ec.Es.SA_AccountIntervals != null)
                            {
                                WriteAccountStdInAndOutBalances(entities, accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval));
                                WriteAccountStdYearBalance(entities, accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval));
                            }
                            else
                            {
                                WriteAccountStdInAndOutBalances(entities, accountStdsInInterval);
                                WriteAccountStdYearBalance(entities, accountStdsInInterval);
                            }

                            WriteAccountPeriodBalance(accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval), true, true, !(ec.Es.SSTD_BudgetId is null) && ec.Es.SSTD_BudgetId.Value > 0);
                            break;
                        case SieExportType.Type4:
                            WriteAccountStd(entities, accountStdsInInterval);
                            WriteAccountInternal(accountDimInternals, accountInternalsInInterval);
                            if (ec.Es.SA_AccountIntervals != null)
                            {
                                WriteAccountStdInAndOutBalances(entities, accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval));
                                WriteAccountStdYearBalance(entities, accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval));
                            }
                            else
                            {
                                WriteAccountStdInAndOutBalances(entities, accountStdsInInterval);
                                WriteAccountStdYearBalance(entities, accountStdsInInterval);
                            }

                            WriteAccountPeriodBalance(accountStdsInInterval, accountDimInternals, (es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval), true, true, !(ec.Es.SSTD_BudgetId is null) && ec.Es.SSTD_BudgetId.Value > 0);
                            WriteVouchers((es.SA_AccountIntervals.IsNullOrEmpty() ? new List<AccountInternal>() : accountInternalsInInterval));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult(ex.Message);
            }
            return new ActionResult();
        }

        #region Write

        /// <summary>
        /// Write all AccounInternals and their AccountDims
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="accountInternals">The AccounInternals to write</param>
        private void WriteAccountInternal(List<AccountDim> accountDimInternals, List<AccountInternal> accountInternals)
        {
            if (accountDimInternals == null || accountInternals == null)
                return;

            foreach (AccountDim accountDimInternal in accountDimInternals)
            {
                if (accountDimInternal.State != (int)SoeEntityState.Active)
                    continue;

                int counter = (from a in accountInternals
                               where a.Account.AccountDimId == accountDimInternal.AccountDimId
                               select a).Count();

                if (counter == 0)
                    continue;

                SieAccountDimItem accountDimItem = new SieAccountDimItem(ec.LineNr);
                accountDimItem.AccountDimNr = accountDimInternal.SysSieDimNrOrAccountDimNr;
                accountDimItem.Name = accountDimInternal.Name;

                string line = GetLine(Constants.SIE_LABEL_DIM, accountDimItem);
                if (!String.IsNullOrEmpty(line))
                {
                    ec.WriteLine(line);
                }
                else
                {
                    //Conflict: Write failed
                    accountDimItem.AddConflict(SieConflict.Export_WriteFailed, accountDimItem.ToString());
                }

                ec.AccountDimItems.Add(accountDimItem);
            }

            //AccountInternal
            foreach (AccountInternal accountInternal in accountInternals)
            {
                if (accountInternal.Account.State != (int)SoeEntityState.Active)
                    continue;

                SieAccountInternalItem accountInternalItem = new SieAccountInternalItem(ec.LineNr);
                accountInternalItem.AccountDimNr = accountInternal.Account.AccountDim.SysSieDimNrOrAccountDimNr;
                accountInternalItem.ObjectCode = accountInternal.Account.AccountNr;
                accountInternalItem.Name = accountInternal.Account.Name;

                string line = GetLine(Constants.SIE_LABEL_OBJEKT, accountInternalItem);
                if (!String.IsNullOrEmpty(line))
                {
                    ec.WriteLine(line);
                }
                else
                {
                    //Conflict: Write failed
                    accountInternalItem.AddConflict(SieConflict.Export_WriteFailed, accountInternalItem.ToString());
                }

                ec.AccountInternalItems.Add(accountInternalItem);
            }
        }

        /// <summary>
        /// Write all AccountStds
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="accountStds">The AccountStds to write</param>
        private void WriteAccountStd(CompEntities entities, List<AccountStd> accountStds)
        {
            if (!ec.ExportAccount || accountStds == null)
                return;

            IEnumerable<SysAccountSruCode> sysAccountSruCodes = null;
            if (ec.ExportSruCodes)
                sysAccountSruCodes = AccountManager.GetSysAccountSruCodes();

            var duplicateAccountNr = accountStds.Where(x => x.Account.State != (int)SoeEntityState.Deleted).GroupBy(x => x.Account.AccountNr)
              .Where(g => g.Count() > 1)
              //.Select(y => y.Key)
              .ToList();

            /*if (duplicateAccountNr.Any())
            {
                throw new Exception(GetText(7658, "Kontonumret är upplagt flera gånger") +": " + duplicateAccountNr.First() );
            }*/

            List<int> accountsToIgnore = new List<int>();
            foreach (AccountStd accountStd in accountStds)
            {
                if (accountStd.Account.State == (int)SoeEntityState.Deleted || accountsToIgnore.Contains(accountStd.AccountId))
                    continue;


                if (accountStd.Account.State == (int)SoeEntityState.Inactive)
                {
                    if (!(from r in entities.VoucherRow select r).Any(r => r.AccountStd.AccountId == accountStd.AccountId && r.State == (int)SoeEntityState.Active))
                    {
                        accountsToIgnore.Add(accountStd.Account.AccountId);
                        continue;
                    }
                }

                int accountsWithSaldo = 0;
                var accGroup = duplicateAccountNr.FirstOrDefault(g => g.Key == accountStd.Account.AccountNr);
                if (accGroup != null)
                {
                    var accountsToCheck = accGroup.Where(a => !accountsToIgnore.Contains(a.AccountId));
                    foreach (var account in accountsToCheck)
                    {
                        if (account.Account.State == (int)SoeEntityState.Inactive)
                        {
                            if (!(from r in entities.VoucherRow select r).Any(r => r.AccountStd.AccountId == account.AccountId && r.State == (int)SoeEntityState.Active))
                            {
                                accountsToIgnore.Add(accountStd.Account.AccountId);
                                continue;
                            }
                        }
                        accountsWithSaldo++;
                    }

                    if (accountsWithSaldo > 1)
                        throw new SoeGeneralException(GetText(7658, "Kontonumret är upplagt flera gånger") + ": " + accountStd.Account.AccountNr, this.ToString());
                }

                SieAccountStdItem accountStdItem = new SieAccountStdItem(ec.LineNr);
                accountStdItem.AccountDimNr = Constants.ACCOUNTDIM_STANDARD;
                accountStdItem.AccountNr = accountStd.Account.AccountNr?.Trim();
                accountStdItem.Name = accountStd.Account.Name?.Trim();
                accountStdItem.AccountType = accountStd.AccountTypeSysTermId;

                if (Int32.TryParse(accountStdItem.AccountNr, out _))
                {
                    //Account
                    string line = GetLine(Constants.SIE_LABEL_KONTO, accountStdItem);
                    if (!String.IsNullOrEmpty(line))
                    {
                        ec.WriteLine(line);

                        if (ec.ExportAccountType)
                        {
                            //AccountType
                            line = GetLine(Constants.SIE_LABEL_KTYP, accountStdItem);
                            if (!String.IsNullOrEmpty(line))
                            {
                                ec.WriteLine(line);
                            }
                            else
                            {
                                accountStdItem.AddConflict(SieConflict.Export_WriteFailed, accountStdItem.ToString());
                            }
                        }

                        if (ec.ExportSruCodes)
                        {
                            if (!accountStd.AccountSru.IsLoaded)
                                accountStd.AccountSru.Load();

                            foreach (AccountSru accountSru in accountStd.AccountSru)
                            {
                                SysAccountSruCode sysAccountSruCode = sysAccountSruCodes.FirstOrDefault<SysAccountSruCode>(sru => sru.SysAccountSruCodeId == accountSru.SysAccountSruCodeId);
                                if (sysAccountSruCode != null)
                                {
                                    accountStdItem.SruCode = sysAccountSruCode.SruCode;

                                    //SRU
                                    line = GetLine(Constants.SIE_LABEL_SRU, accountStdItem);
                                    if (!String.IsNullOrEmpty(line))
                                    {
                                        ec.WriteLine(line);
                                    }
                                    else
                                    {
                                        //Conflict: Write failed
                                        accountStdItem.AddConflict(SieConflict.Export_WriteFailed, accountStdItem.ToStringSru());
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Conflict: Write failed
                        accountStdItem.AddConflict(SieConflict.Export_WriteFailed, accountStdItem.ToString());
                    }
                }
                else
                {
                    //Conflict: Write failed
                    accountStdItem.AddConflict(SieConflict.Export_AccountStdIsNotNumeric, accountStdItem.AccountNr);
                }

                ec.AccountStdItems.Add(accountStdItem);
            }
        }

        /// <summary>
        /// Write in and out balance for all given AccountStds
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="accountStds">The AccountStds to write in balance for</param>
        private void WriteAccountStdInAndOutBalances(CompEntities entities, List<AccountStd> accountStds, List<AccountDim> accountDimInternals = null, List<AccountInternal> accountInternals = null)
        {
            if (accountStds == null)
                return;

            #region Prereq

            //Year in
            Dictionary<int, BalanceItemDTO> biYearInBalanceDict = AccountBalanceManager(actorCompanyId).GetYearInBalance(entities, ec.AccountYear, accountStds, accountInternals, actorCompanyId);

            //Balance change
            Dictionary<int, BalanceItemDTO> biPeriodBalanceDict = AccountBalanceManager(actorCompanyId).GetPeriodOutBalance(entities, ec.AccountYear, ec.Es.DateTo, accountStds, accountDimInternals, accountInternals, biYearInBalanceDict, actorCompanyId);

            //Previous year
            Dictionary<int, BalanceItemDTO> biOpeningYearBalancePrevYearDict = new Dictionary<int, BalanceItemDTO>();
            Dictionary<int, BalanceItemDTO> biPeriodBalancePrevYearDict = new Dictionary<int, BalanceItemDTO>();
            if (ec.ExportPreviousYear && ec.PreviousAccountYear != null)
            {
                //Year in
                biOpeningYearBalancePrevYearDict = AccountBalanceManager(actorCompanyId).GetYearInBalance(entities, ec.PreviousAccountYear, accountStds, accountInternals, actorCompanyId);

                //Balance change
                biPeriodBalancePrevYearDict = AccountBalanceManager(actorCompanyId).GetPeriodOutBalance(entities, ec.PreviousAccountYear, CalendarUtility.GetEndOfDay(ec.PreviousAccountYear.To), accountStds, accountDimInternals, accountInternals, biOpeningYearBalancePrevYearDict, actorCompanyId);
            }

            #endregion

            #region In

            foreach (AccountStd accountStd in accountStds)
            {
                #region AccountStd

                SieAccountStdInBalanceItem accountStdInBalanceItem = new SieAccountStdInBalanceItem(ec.LineNr);
                accountStdInBalanceItem.AccountNr = accountStd.Account.AccountNr;
                accountStdInBalanceItem.AccountYear = ec.AccountYearNr;

                BalanceItemDTO biOpeningYearBalance = biYearInBalanceDict[accountStd.AccountId];
                accountStdInBalanceItem.Balance = biOpeningYearBalance.Balance;
                accountStdInBalanceItem.Quantity = biOpeningYearBalance.Quantity;

                //Dont write zero rows
                if (accountStdInBalanceItem.Balance == Decimal.Zero && !ec.ExportPreviousYear)
                    continue;
                if (ec.ExportPreviousYear && ec.PreviousAccountYear != null)
                {
                    BalanceItemDTO biOpeningYearBalancePrevYear = biOpeningYearBalancePrevYearDict[accountStd.AccountId];
                    if (biOpeningYearBalancePrevYear.Balance == Decimal.Zero && accountStdInBalanceItem.Balance == Decimal.Zero)
                        continue;
                }

                if (Int32.TryParse(accountStdInBalanceItem.AccountNr, out _))
                {
                    //Account
                    string line = GetLine(Constants.SIE_LABEL_IB, accountStdInBalanceItem);
                    if (!String.IsNullOrEmpty(line))
                    {
                        ec.WriteLine(line);

                        //Write one line with previous year
                        if (ec.ExportPreviousYear && ec.PreviousAccountYear != null)
                        {
                            accountStdInBalanceItem.AccountYear = ec.AccountYearNr - 1;

                            BalanceItemDTO biOpeningYearBalancePrevYear = biOpeningYearBalancePrevYearDict[accountStd.AccountId];
                            accountStdInBalanceItem.Balance = biOpeningYearBalancePrevYear.Balance;
                            accountStdInBalanceItem.Quantity = biOpeningYearBalancePrevYear.Quantity;

                            line = GetLine(Constants.SIE_LABEL_IB, accountStdInBalanceItem);
                            if (!String.IsNullOrEmpty(line))
                            {
                                ec.WriteLine(line);
                            }
                            else
                            {
                                //Conflict: Write failed
                                accountStdInBalanceItem.AddConflict(SieConflict.Export_WriteFailed, accountStdInBalanceItem.ToStringYear(ec.PreviousAccountYear));
                            }
                        }
                    }
                    else
                    {
                        //Conflict: Write failed
                        accountStdInBalanceItem.AddConflict(SieConflict.Export_WriteFailed, accountStdInBalanceItem.ToStringYear(ec.PreviousAccountYear));
                    }
                }
                else
                {
                    //Conflict: Write failed
                    accountStdInBalanceItem.AddConflict(SieConflict.Export_WriteFailed, accountStdInBalanceItem.ToStringYear(ec.AccountYear));
                }

                ec.AccountStdInBalanceItems.Add(accountStdInBalanceItem);

                #endregion
            }

            #endregion

            #region Out

            foreach (AccountStd accountStd in accountStds)
            {
                #region AccountStd

                if (accountStd.Account.State != (int)SoeEntityState.Active)
                    continue;

                if (accountStd.AccountTypeSysTermId == 3 || accountStd.AccountTypeSysTermId == 4)
                    continue;

                SieAccountStdOutBalanceItem accountStdOutBalanceItem = new SieAccountStdOutBalanceItem(ec.LineNr);
                accountStdOutBalanceItem.AccountNr = accountStd.Account.AccountNr;
                accountStdOutBalanceItem.AccountYear = ec.AccountYearNr;

                BalanceItemDTO biPeriodBalance = biPeriodBalanceDict[accountStd.AccountId]; //abm.GetPeriodBalance(entities, actorCompanyId, ec.AccountYear, ec.AccountYear.To, accountStd, null)
                accountStdOutBalanceItem.Balance = biPeriodBalance.Balance;
                accountStdOutBalanceItem.Quantity = biPeriodBalance.Quantity;

                //Dont write zero rows
                if (accountStdOutBalanceItem.Balance == Decimal.Zero)
                    continue;

                if (Int32.TryParse(accountStdOutBalanceItem.AccountNr, out _))
                {
                    //Account
                    string line = GetLine(Constants.SIE_LABEL_UB, accountStdOutBalanceItem);
                    if (!String.IsNullOrEmpty(line))
                    {
                        ec.WriteLine(line);

                        //Write one line with previous year
                        if (ec.ExportPreviousYear && ec.PreviousAccountYear != null)
                        {
                            accountStdOutBalanceItem.AccountYear = ec.AccountYearNr - 1;

                            BalanceItemDTO biPeriodBalancePrevYear = biPeriodBalancePrevYearDict[accountStd.AccountId];
                            accountStdOutBalanceItem.Balance = biPeriodBalancePrevYear.Balance;
                            accountStdOutBalanceItem.Quantity = biPeriodBalancePrevYear.Quantity;

                            line = GetLine(Constants.SIE_LABEL_UB, accountStdOutBalanceItem);
                            if (!String.IsNullOrEmpty(line))
                            {
                                ec.WriteLine(line);
                            }
                            else
                            {
                                //Conflict: Write failed
                                accountStdOutBalanceItem.AddConflict(SieConflict.Export_WriteFailed, accountStdOutBalanceItem.ToStringYear(ec.PreviousAccountYear));
                            }
                        }
                    }
                    else
                    {
                        //Conflict: Write failed
                        accountStdOutBalanceItem.AddConflict(SieConflict.Export_WriteFailed, accountStdOutBalanceItem.ToStringYear(ec.AccountYear));
                    }
                }
                else
                {
                    //Conflict: Write failed
                    accountStdOutBalanceItem.AddConflict(SieConflict.Export_AccountStdIsNotNumeric, accountStdOutBalanceItem.AccountNr);
                }

                ec.AccountStdOutBalanceItems.Add(accountStdOutBalanceItem);

                #endregion
            }

            #endregion
        }

        /// <summary>
        /// Write year result for all given AccountStds
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="accountStds">The AccountStds to write result for</param>
        private void WriteAccountStdYearBalance(CompEntities entities, List<AccountStd> accountStds, List<AccountDim> accountDimInternals = null, List<AccountInternal> accountInternals = null)
        {
            if (accountStds == null)
                return;

            #region Prereq

            List<AccountStd> accountStdsValidAccountTypes = accountStds.Where(a => a.AccountTypeSysTermId == (int)TermGroup_AccountType.Cost || a.AccountTypeSysTermId == (int)TermGroup_AccountType.Income).ToList();

            //Balance change year
            Dictionary<int, BalanceItemDTO> biBalanceChangeDict = AccountBalanceManager(actorCompanyId).GetBalanceChange(entities, ec.AccountYear, ec.AccountYear.From, CalendarUtility.GetEndOfDay(ec.Es.DateTo), accountStdsValidAccountTypes, accountDimInternals, accountInternals, actorCompanyId);

            //Previous year
            Dictionary<int, BalanceItemDTO> biBalanceChangePrevYearDict = new Dictionary<int, BalanceItemDTO>();
            if (ec.ExportPreviousYear && ec.PreviousAccountYear != null)
            {
                //Balance change year
                biBalanceChangePrevYearDict = AccountBalanceManager(actorCompanyId).GetBalanceChange(entities, ec.PreviousAccountYear, ec.PreviousAccountYear.From, ec.PreviousAccountYear.To, accountStdsValidAccountTypes, accountDimInternals, accountInternals, actorCompanyId);
            }

            #endregion

            foreach (AccountStd accountStd in accountStdsValidAccountTypes)
            {
                #region AccountStd

                if (accountStd.Account.State != (int)SoeEntityState.Active)
                    continue;

                SieAccountStdYearBalanceItem accountStdYearBalanceItem = new SieAccountStdYearBalanceItem(ec.LineNr);
                accountStdYearBalanceItem.AccountNr = accountStd.Account.AccountNr;
                accountStdYearBalanceItem.AccountYear = ec.AccountYearNr;

                BalanceItemDTO biBalanceChangeItem = biBalanceChangeDict[accountStd.AccountId];
                accountStdYearBalanceItem.Balance = biBalanceChangeItem.Balance;
                accountStdYearBalanceItem.Quantity = biBalanceChangeItem.Quantity;

                //Dont write zero rows
                if (accountStdYearBalanceItem.Balance == Decimal.Zero && !ec.ExportPreviousYear)
                    continue;
                if (ec.ExportPreviousYear && ec.PreviousAccountYear != null)
                {
                    BalanceItemDTO biBalanceChangePrevYear = biBalanceChangePrevYearDict[accountStd.AccountId];
                    if (biBalanceChangePrevYear.Balance == Decimal.Zero && accountStdYearBalanceItem.Balance == Decimal.Zero)
                        continue;
                }

                if (Int32.TryParse(accountStdYearBalanceItem.AccountNr, out _))
                {
                    //Account
                    string line = GetLine(Constants.SIE_LABEL_RES, accountStdYearBalanceItem);
                    if (!String.IsNullOrEmpty(line))
                    {
                        ec.WriteLine(line);

                        //Write one line with previous year
                        if (ec.ExportPreviousYear && ec.PreviousAccountYear != null)
                        {
                            accountStdYearBalanceItem.AccountYear = ec.AccountYearNr - 1;

                            BalanceItemDTO biBalanceChangePrevYear = biBalanceChangePrevYearDict[accountStd.AccountId];
                            accountStdYearBalanceItem.Balance = biBalanceChangePrevYear.Balance;
                            accountStdYearBalanceItem.Quantity = biBalanceChangePrevYear.Quantity;

                            line = GetLine(Constants.SIE_LABEL_RES, accountStdYearBalanceItem);
                            if (!String.IsNullOrEmpty(line))
                            {
                                ec.WriteLine(line);
                            }
                            else
                            {
                                //Conflict: Write failed
                                accountStdYearBalanceItem.AddConflict(SieConflict.Export_WriteFailed, accountStdYearBalanceItem.ToStringYear(ec.PreviousAccountYear));
                            }
                        }
                    }
                    else
                    {
                        //Conflict: Write failed
                        accountStdYearBalanceItem.AddConflict(SieConflict.Export_WriteFailed, accountStdYearBalanceItem.ToStringYear(ec.AccountYear));
                    }
                }
                else
                {
                    //Conflict: Write failed
                    accountStdYearBalanceItem.AddConflict(SieConflict.Export_WriteFailed, accountStdYearBalanceItem.ToStringYear(ec.AccountYear));
                }

                ec.AccountStdYearBalanceItems.Add(accountStdYearBalanceItem);

                #endregion
            }
        }

        /// <summary>
        /// Write period result for all given AccountStds
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="accountStds">The AccountStds to write result for</param>
        /// <param name="accountInternals">The AccountInternals to write result for</param>
        /// <param name="writeAccountStds">True if AccountStds should be included</param>
        /// <param name="writeAccountInternals">True if AccountInternals should be included</param>
        /// <param name="writeBudget">True if BudgetHeadId is greater than 0</param>
        private void WriteAccountPeriodBalance(List<AccountStd> accountStds, List<AccountDim> accountDimInternals, List<AccountInternal> accountInternals, bool writeAccountStds, bool writeAccountInternals, bool writeBudget)
        {
            if (accountStds == null)
                return;
            if (!writeAccountStds && !writeAccountInternals)
                return;

            #region Prereq

            AccountYearDTO accountYearDTO = ec.AccountYear.ToDTO();
            List<AccountDTO> accountDTOs = accountStds.ToDTOs().ToList();
            List<AccountInternalDTO> accountInternalDTOs = accountInternals.ToDTOs();
            List<AccountDimDTO> accountDimDTOs = accountDimInternals.ToDTOs();

            Dictionary<int, Dictionary<int, BalanceItemDTO>> biBalanceChangeDict = new Dictionary<int, Dictionary<int, BalanceItemDTO>>();
            Dictionary<int, Dictionary<int, BalanceItemDTO>> biBudgetBalanceChangeDict = new Dictionary<int, Dictionary<int, BalanceItemDTO>>();
            List<VoucherHeadDTO> voucherHeads = new List<VoucherHeadDTO>();

            List<AccountPeriod> accountPeriodsInInterval = AccountManager.GetAccountPeriodsInDateInterval(ec.AccountYear.AccountYearId, ec.Es.DateFrom, ec.Es.DateTo);
            foreach (AccountPeriod accountPeriod in accountPeriodsInInterval)
            {
                biBalanceChangeDict.Add(accountPeriod.AccountPeriodId, AccountBalanceManager(actorCompanyId).GetBalanceChangeFromDTO(accountYearDTO, accountPeriod.From, accountPeriod.To, accountDTOs, accountDimDTOs, accountInternalDTOs, actorCompanyId));

                if (writeBudget)
                    biBudgetBalanceChangeDict.Add(accountPeriod.AccountPeriodId, BudgetManager.GetBudgetForPeriodFromDTO((int)ec.Es.SSTD_BudgetId, accountYearDTO, accountPeriod.From, accountPeriod.To, accountDTOs, accountInternalDTOs, actorCompanyId, true, out voucherHeads));
            }

            #endregion

            foreach (AccountStd accountStd in accountStds)
            {
                #region AccountStd

                if (accountStd.Account.State != (int)SoeEntityState.Active)
                    continue;

                foreach (AccountPeriod accountPeriod in accountPeriodsInInterval)
                {
                    #region AccountPeriod

                    Dictionary<int, BalanceItemDTO> accountPeriodBalanceChangeDict = biBalanceChangeDict[accountPeriod.AccountPeriodId];
                    BalanceItemDTO biBalanceChange = accountPeriodBalanceChangeDict[accountStd.AccountId];

                    if (writeAccountStds)
                    {
                        #region AccountStd

                        SieAccountPeriodBalanceItem periodBalanceItem = new SieAccountPeriodBalanceItem(ec.LineNr)
                        {
                            AccountNr = accountStd.Account.AccountNr,
                            AccountYear = ec.AccountYearNr,
                            Period = accountPeriod.From,
                            Balance = biBalanceChange.Balance,
                            Quantity = biBalanceChange.Quantity,
                        };

                        //Account
                        if (Int32.TryParse(periodBalanceItem.AccountNr, out _))
                        {
                            string line = GetLine(Constants.SIE_LABEL_PSALDO, periodBalanceItem);
                            if (!String.IsNullOrEmpty(line))
                            {
                                ec.WriteLine(line);

                                #region Budget
                                //Budget
                                if (writeBudget)
                                {
                                    Dictionary<int, BalanceItemDTO> accountPeriodBudgetChangeDict = biBudgetBalanceChangeDict[accountPeriod.AccountPeriodId];
                                    BalanceItemDTO biBudgetBalanceChange = accountPeriodBudgetChangeDict[accountStd.AccountId];
                                    this.WriteAccountPeriodBudegtBalance(accountStd.Account.AccountNr, accountPeriod.From, biBudgetBalanceChange);
                                }
                                #endregion
                            }
                            else
                            {
                                //Conflict: Write failed
                                periodBalanceItem.AddConflict(SieConflict.Export_WriteFailed, periodBalanceItem.AccountNr);
                            }
                        }
                        else
                        {
                            //Conflict: Write failed
                            periodBalanceItem.AddConflict(SieConflict.Export_WriteFailed, periodBalanceItem.AccountNr);
                        }

                        ec.AccountPeriodBalanceItems.Add(periodBalanceItem);


                        #endregion
                    }

                    if (writeAccountInternals)
                    {
                        #region AccountInternal

                        foreach (BalanceItemInternalDTO balanceItemInternal in biBalanceChange.BalanceItemInternals)
                        {
                            SieAccountPeriodBalanceItem periodBalanceItem = new SieAccountPeriodBalanceItem(ec.LineNr)
                            {
                                AccountNr = accountStd.Account.AccountNr,
                                AccountYear = ec.AccountYearNr,
                                Period = accountPeriod.From,
                                Balance = balanceItemInternal.Balance,
                                Quantity = balanceItemInternal.Quantity,
                            };

                            foreach (AccountInternalDTO accountInternal in balanceItemInternal.AccountInternals)
                            {
                                if (accountInternal.SysSieDimNr.HasValue)
                                {
                                    periodBalanceItem.AddObjectItem(new SieObjectItem()
                                    {
                                        SieDimension = accountInternal.SysSieDimNr.Value,
                                        ObjectCode = accountInternal.AccountNr
                                    });
                                }
                            }

                            string line = GetLine(Constants.SIE_LABEL_PSALDO, periodBalanceItem);
                            if (!String.IsNullOrEmpty(line))
                            {
                                ec.WriteLine(line);

                                #region Budget
                                //Budget
                                if (writeBudget)
                                {
                                    Dictionary<int, BalanceItemDTO> accountPeriodBudgetChangeDict = biBudgetBalanceChangeDict[accountPeriod.AccountPeriodId];
                                    BalanceItemDTO biBudgetBalanceChange = accountPeriodBudgetChangeDict[accountStd.AccountId];
                                    this.WriteAccountPeriodBudegtBalance(accountStd.Account.AccountNr, accountPeriod.From, biBudgetBalanceChange, periodBalanceItem.ObjectItems);
                                }
                                #endregion
                            }
                            else
                            {
                                //Conflict: Write failed
                                periodBalanceItem.AddConflict(SieConflict.Export_WriteFailed, periodBalanceItem.AccountNr);
                            }

                            ec.AccountPeriodBalanceItems.Add(periodBalanceItem);
                        }

                        #endregion
                    }

                    #endregion
                }

                #endregion
            }
        }

        /// <summary>
        /// Write budget balance result for given account and period
        /// </summary>
        /// <param name="accountNr">Related account number</param>
        /// <param name="accountPeriod">Related account period</param>
        /// <param name="budgetBalanceChange">Budget balance object</param>
        /// <param name="objectItems">Sie object items if available</param>
        private void WriteAccountPeriodBudegtBalance(string accountNr, DateTime accountPeriod, BalanceItemDTO budgetBalanceChange, List<SieObjectItem> objectItems = null)
        {
            if (string.IsNullOrWhiteSpace(accountNr) || accountPeriod == null || budgetBalanceChange == null)
                return;


            SieAccountPeriodBudgetBalanceItem periodBudgetItem = new SieAccountPeriodBudgetBalanceItem(ec.LineNr)
            {
                AccountNr = accountNr,
                AccountYear = ec.AccountYearNr,
                Period = accountPeriod,
                Balance = budgetBalanceChange.Balance,
                Quantity = budgetBalanceChange.Quantity,
            };

            if (objectItems != null && objectItems.Count > 0)
            {
                foreach (SieObjectItem objectItem in objectItems)
                {
                    periodBudgetItem.AddObjectItem(objectItem);
                }
            }

            string line = GetLine(Constants.SIE_LABEL_PBUDGET, periodBudgetItem);
            if (!String.IsNullOrEmpty(line))
            {
                ec.WriteLine(line);
            }
            else
            {
                //Conflict: Write failed
                periodBudgetItem.AddConflict(SieConflict.Export_WriteFailed, periodBudgetItem.AccountNr);
            }

            ec.AccountPeriodBudgetBalanceItems.Add(periodBudgetItem);
        }

        /// <summary>
        /// Write Voucher lines
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        private void WriteVouchers(List<AccountInternal> accountInternals)
        {
            List<AccountInternalDTO> accountInternalInIntervalDTOs = new List<AccountInternalDTO>();
            if (accountInternals.Count > 0)
                accountInternalInIntervalDTOs = accountInternals.ToDTOs();

            foreach (VoucherHeadDTO voucherHead in VoucherManager.GetVoucherHeadDTOsFromSelection(ec.Es, orderByVoucherNr: ec.VoucherSortBy == TermGroup_SieExportVoucherSort.ByVoucherNr))
            {
                #region VoucherHead

                if (voucherHead.Template)
                    continue;

                SieVoucherItem voucherItem = new SieVoucherItem(ec.LineNr);
                voucherItem.VoucherNr = voucherHead.VoucherNr;
                voucherItem.Text = StringUtility.Left(voucherHead.Text, 100).StripNewLineAndHyphen();
                voucherItem.VoucherSeriesTypeNr = voucherHead.VoucherSeriesTypeNr.ToString();
                voucherItem.VoucherDate = voucherHead.Date;
                voucherItem.RegDate = voucherHead.Created;

                //Voucher
                string line = GetLine(Constants.SIE_LABEL_VER, voucherItem);
                if (!String.IsNullOrEmpty(line))
                {
                    ec.WriteLine(line);

                    //Transaction Start
                    line = GetLine(Constants.SIE_LABEL_START, null);
                    ec.WriteLine(line);

                    foreach (VoucherRowDTO voucherRow in voucherHead.Rows)
                    {
                        if (voucherRow.State != (int)SoeEntityState.Active)
                            continue;
                        if (ec.Es.SA_AccountIntervals != null && !VoucherManager.VoucherRowDTOContainsAccountInternals(voucherRow, accountInternalInIntervalDTOs))
                            continue;
                        //VoucherRow
                        SieTransactionItem transactionItem = new SieTransactionItem(ec.LineNr);
                        transactionItem.AccountNr = voucherRow.Dim1Nr;
                        transactionItem.Amount = voucherRow.Amount;
                        transactionItem.TransactionDate = voucherRow.Date;
                        transactionItem.Text = StringUtility.Left(voucherRow.Text, 100).StripNewLineAndHyphen();
                        transactionItem.Quantity = voucherRow.Quantity;

                        foreach (AccountInternalDTO accountInternal in voucherRow.AccountInternalDTO_forReports)
                        {
                            if (accountInternal.SysSieDimNr.HasValue)
                            {
                                //Object
                                transactionItem.AddObjectItem(new SieObjectItem()
                                {
                                    SieDimension = accountInternal.SysSieDimNr.Value,
                                    ObjectCode = accountInternal.AccountNr
                                });
                            }
                        }

                        //Transaction
                        line = GetLine(Constants.SIE_LABEL_TRANS, transactionItem);
                        if (!String.IsNullOrEmpty(line))
                        {
                            ec.WriteLine(line);
                        }
                        else
                        {
                            //Conflict: Write failed
                            transactionItem.AddConflict(SieConflict.Export_WriteFailed, voucherItem.ToStringTransaction(transactionItem));
                        }

                        voucherItem.AddTransactionItem(transactionItem);
                    }

                    line = GetLine(Constants.SIE_LABEL_END, null);
                    ec.WriteLine(line);
                }
                else
                {
                    //Conflict: Write failed
                    voucherItem.AddConflict(SieConflict.Export_WriteFailed, voucherItem.VoucherNr.ToString());
                }

                ec.VoucherItems.Add(voucherItem);

                #endregion
            }
        }

        /// <summary>
        /// Write common lines for all types of SIE export
        /// </summary>
        private void WriteCommonLines()
        {
            ec.WriteLine(GetLine(Constants.SIE_LABEL_FLAGGA, null));
            ec.WriteLine(GetLine(Constants.SIE_LABEL_PROGRAM, null));
            ec.WriteLine(GetLine(Constants.SIE_LABEL_FORMAT, null));
            ec.WriteLine(GetLine(Constants.SIE_LABEL_GEN, null));
            ec.WriteLine(GetLine(Constants.SIE_LABEL_SIETYP, null));
            ec.WriteLine(GetLine(Constants.SIE_LABEL_PROSA, null));
            ec.WriteLine(GetLine(Constants.SIE_LABEL_FNR, null));
            ec.WriteLine(GetLine(Constants.SIE_LABEL_ORGNR, null));
            ec.WriteLine(GetLine(Constants.SIE_LABEL_ADRESS, null));
            ec.WriteLine(GetLine(Constants.SIE_LABEL_FNAMN, null));
            ec.WriteLine(GetLine(Constants.SIE_LABEL_RAR, null));
        }

        /// <summary>
        /// Builds a line with the given label to export
        /// </summary>
        /// <param name="label">The label</param>
        /// <param name="obj">The object to get information from</param>
        /// <returns>A line to write to the SIE export file</returns>
        private string GetLine(string label, object obj)
        {
            string line = label.Trim() + StringUtility.GetAsciiTab();

            SieAccountStdItem accountStdItem;
            SieAccountStdInBalanceItem accountStdInBalanceItem;
            SieAccountStdOutBalanceItem accountStdOutBalanceItem;
            SieAccountStdYearBalanceItem accountStdYearBalanceItem;
            SieAccountPeriodBalanceItem accountStdPeriodBalanceItem;
            SieAccountPeriodBudgetBalanceItem accountPeriodBudgetBalanceItem;
            //AccountInternalInBalanceItem accountInternalInBalanceItem = null; // TODO: Implement
            //AccountInternalOutBalanceItem accountInternalOutBalanceItem = null; // TODO: Implement
            SieVoucherItem voucherItem;
            SieTransactionItem transactionItem;

            switch (label)
            {
                case Constants.SIE_LABEL_START:
                    break;
                case Constants.SIE_LABEL_END:
                    break;
                case Constants.SIE_LABEL_FLAGGA:
                    line += 0;
                    break;
                case Constants.SIE_LABEL_PROGRAM:
                    line += GetValidString(ec.Program, true);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(ec.Version, true);
                    break;
                case Constants.SIE_LABEL_FORMAT:
                    line += "PC8";
                    break;
                case Constants.SIE_LABEL_GEN:
                    line += GetValidDate(DateTime.Now);
                    line += StringUtility.GetAsciiSpace();
                    line += ec.LoginName;
                    break;
                case Constants.SIE_LABEL_SIETYP:
                    line += (int)ec.ExportType;
                    break;
                case Constants.SIE_LABEL_PROSA:
                    line += GetValidString(ec.Comment, true);
                    break;
                case Constants.SIE_LABEL_FNR:
                    line += actorCompanyId;
                    break;
                case Constants.SIE_LABEL_ORGNR:
                    line += GetValidString(ec.OrgNr);
                    break;
                case Constants.SIE_LABEL_ADRESS:
                    line += GetValidString(ec.ContactName, true);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(ec.Address, true);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidStrings(false, true, ec.PostalCode.RemoveWhiteSpace(), ec.PostalAddress);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(ec.Phone, true);
                    break;
                case Constants.SIE_LABEL_FNAMN:
                    line += GetValidString(ec.CompanyName, true);
                    break;
                case Constants.SIE_LABEL_RAR:
                    if (ec.AccountYear != null)
                    {
                        line += ec.AccountYearNr;
                        line += StringUtility.GetAsciiSpace();
                        line += ec.AccountYear.From.ToString("yyyyMMdd");
                        line += StringUtility.GetAsciiSpace();
                        line += ec.AccountYear.To.ToString("yyyyMMdd");
                    }
                    if (ec.PreviousAccountYear != null)
                    {
                        // Add prevoius year as -1
                        line += StringUtility.GetAsciiCarriageReturn();
                        line += StringUtility.GetAsciiNewLine();
                        line += label.Trim() + StringUtility.GetAsciiTab();
                        line += -1;
                        line += StringUtility.GetAsciiSpace();
                        line += ec.PreviousAccountYear.From.ToString("yyyyMMdd");
                        line += StringUtility.GetAsciiSpace();
                        line += ec.PreviousAccountYear.To.ToString("yyyyMMdd");
                    }
                    break;
                case Constants.SIE_LABEL_DIM:
                    var accountDimItem = obj as SieAccountDimItem;
                    if (accountDimItem == null)
                        return String.Empty;

                    line += accountDimItem.AccountDimNr;
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(accountDimItem.Name, true);
                    break;
                case Constants.SIE_LABEL_OBJEKT:
                    var accountInternalItem = obj as SieAccountInternalItem;
                    if (accountInternalItem == null)
                        return String.Empty;

                    line += accountInternalItem.AccountDimNr;
                    line += StringUtility.GetAsciiSpace();
                    line += StringUtility.SurroundString(accountInternalItem.ObjectCode, StringUtility.GetAsciiQuote());
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(accountInternalItem.Name, true);
                    break;
                case Constants.SIE_LABEL_KONTO:
                    accountStdItem = obj as SieAccountStdItem;
                    if (accountStdItem == null)
                        return String.Empty;

                    line += GetValidString(accountStdItem.AccountNr, true);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(accountStdItem.Name, true);
                    break;
                case Constants.SIE_LABEL_KTYP:
                    accountStdItem = obj as SieAccountStdItem;
                    if (accountStdItem == null)
                        return String.Empty;

                    line += accountStdItem.AccountNr;
                    line += StringUtility.GetAsciiSpace();
                    line += accountStdItem.AccountTypeString;
                    break;
                case Constants.SIE_LABEL_SRU:
                    accountStdItem = obj as SieAccountStdItem;
                    if (accountStdItem == null)
                        return String.Empty;

                    line += GetValidString(accountStdItem.AccountNr, true);
                    line += StringUtility.GetAsciiSpace();
                    line += accountStdItem.SruCode;
                    break;
                case Constants.SIE_LABEL_IB:
                    accountStdInBalanceItem = obj as SieAccountStdInBalanceItem;
                    if (accountStdInBalanceItem == null)
                        return String.Empty;

                    line += accountStdInBalanceItem.AccountYear;
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(accountStdInBalanceItem.AccountNr, true);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidDecimal(accountStdInBalanceItem.Balance);
                    if (!String.IsNullOrEmpty(accountStdInBalanceItem.QuantityString))
                    {
                        line += StringUtility.GetAsciiSpace();
                        line += StringUtility.SurroundString(accountStdInBalanceItem.QuantityString, StringUtility.GetAsciiQuote());
                    }
                    break;
                case Constants.SIE_LABEL_UB:
                    accountStdOutBalanceItem = obj as SieAccountStdOutBalanceItem;
                    if (accountStdOutBalanceItem == null)
                        return String.Empty;

                    line += accountStdOutBalanceItem.AccountYear;
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(accountStdOutBalanceItem.AccountNr, true);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidDecimal(accountStdOutBalanceItem.Balance);
                    if (!String.IsNullOrEmpty(accountStdOutBalanceItem.QuantityString))
                    {
                        line += StringUtility.GetAsciiSpace();
                        line += StringUtility.SurroundString(accountStdOutBalanceItem.QuantityString, StringUtility.GetAsciiQuote());
                    }
                    break;
                case Constants.SIE_LABEL_RES:
                    accountStdYearBalanceItem = obj as SieAccountStdYearBalanceItem;
                    if (accountStdYearBalanceItem == null)
                        return String.Empty;

                    line += accountStdYearBalanceItem.AccountYear;
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(accountStdYearBalanceItem.AccountNr, true);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidDecimal(accountStdYearBalanceItem.Balance);
                    if (!string.IsNullOrEmpty(accountStdYearBalanceItem.QuantityString))
                    {
                        line += StringUtility.GetAsciiSpace();
                        line += StringUtility.SurroundString(accountStdYearBalanceItem.QuantityString, StringUtility.GetAsciiQuote());
                    }
                    break;
                case Constants.SIE_LABEL_PSALDO:
                    accountStdPeriodBalanceItem = obj as SieAccountPeriodBalanceItem;
                    if (accountStdPeriodBalanceItem == null)
                        return String.Empty;

                    line += accountStdPeriodBalanceItem.AccountYear;
                    line += StringUtility.GetAsciiSpace();
                    line += accountStdPeriodBalanceItem.PeriodString;
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(accountStdPeriodBalanceItem.AccountNr, true);
                    line += StringUtility.GetAsciiSpace();
                    line += Constants.SIE_LABEL_START;
                    foreach (var item in accountStdPeriodBalanceItem.ObjectItems)
                    {
                        line += item.SieDimension;
                        line += StringUtility.GetAsciiSpace();
                        line += StringUtility.SurroundString(item.ObjectCode, StringUtility.GetAsciiQuote());
                        line += StringUtility.GetAsciiSpace();
                    }
                    line += Constants.SIE_LABEL_END;
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidDecimal(accountStdPeriodBalanceItem.Balance);
                    line += StringUtility.GetAsciiSpace();
                    if (!string.IsNullOrEmpty(accountStdPeriodBalanceItem.QuantityString))
                    {
                        line += StringUtility.SurroundString(accountStdPeriodBalanceItem.QuantityString, StringUtility.GetAsciiQuote());
                    }

                    break;
                case Constants.SIE_LABEL_PBUDGET:
                    accountPeriodBudgetBalanceItem = obj as SieAccountPeriodBudgetBalanceItem;
                    if (accountPeriodBudgetBalanceItem == null)
                        return String.Empty;

                    line += accountPeriodBudgetBalanceItem.AccountYear;
                    line += StringUtility.GetAsciiSpace();
                    line += accountPeriodBudgetBalanceItem.PeriodString;
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(accountPeriodBudgetBalanceItem.AccountNr, true);
                    line += StringUtility.GetAsciiSpace();
                    line += Constants.SIE_LABEL_START;
                    foreach (var item in accountPeriodBudgetBalanceItem.ObjectItems)
                    {
                        line += item.SieDimension;
                        line += StringUtility.GetAsciiSpace();
                        line += StringUtility.SurroundString(item.ObjectCode, StringUtility.GetAsciiQuote());
                        line += StringUtility.GetAsciiSpace();
                    }
                    line += Constants.SIE_LABEL_END;
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidDecimal(accountPeriodBudgetBalanceItem.Balance);
                    line += StringUtility.GetAsciiSpace();
                    if (!string.IsNullOrEmpty(accountPeriodBudgetBalanceItem.QuantityString))
                    {
                        line += StringUtility.SurroundString(accountPeriodBudgetBalanceItem.QuantityString, StringUtility.GetAsciiQuote());
                    }

                    break;
                case Constants.SIE_LABEL_VER:
                    voucherItem = obj as SieVoucherItem;
                    if (voucherItem == null)
                        return String.Empty;

                    line += GetValidString(voucherItem.VoucherSeriesTypeNr, true);
                    line += StringUtility.GetAsciiSpace();
                    line += voucherItem.VoucherNr;
                    line += StringUtility.GetAsciiSpace();
                    line += voucherItem.VoucherDateString;
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(voucherItem.Text, true);
                    line += StringUtility.GetAsciiSpace();
                    line += voucherItem.RegDateString;
                    break;
                case Constants.SIE_LABEL_TRANS:
                    transactionItem = obj as SieTransactionItem;
                    if (transactionItem == null)
                        return String.Empty;

                    line = StringUtility.GetAsciiTab() + line;
                    line += GetValidString(transactionItem.AccountNr, true);
                    line += StringUtility.GetAsciiSpace();
                    line += Constants.SIE_LABEL_START;
                    foreach (var item in transactionItem.ObjectItems)
                    {
                        line += item.SieDimension;
                        line += StringUtility.GetAsciiSpace();
                        line += StringUtility.SurroundString(item.ObjectCode, StringUtility.GetAsciiQuote());
                        line += StringUtility.GetAsciiSpace();
                    }

                    line += Constants.SIE_LABEL_END;
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidDecimal(transactionItem.Amount);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidDate(transactionItem.TransactionDate);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidString(transactionItem.Text, true);
                    line += StringUtility.GetAsciiSpace();
                    line += GetValidQuantity(transactionItem.Quantity);
                    break;
            }

            line += StringUtility.GetAsciiCarriageReturn();
            line += StringUtility.GetAsciiNewLine();

            return line;
        }

        #endregion

        #endregion

        #region Help methods

        /// <summary>
        /// Concats a string from a string array.
        /// If the string is quote separated it concats until a end quote is found.
        /// 
        /// </summary>
        /// <param name="arr">The array with the separated string</param>
        /// <param name="startIndex">The index to start from in the array</param>
        /// <param name="value">The index in which the separated string ends in (out parameter)</param>
        /// <returns>The string to concatenate</returns>
        private string ConcatArraySeparatedString(string[] arr, int startIndex, out int endIndex)
        {
            string value = arr[startIndex];
            endIndex = startIndex;

            if (value != "\"\"")
            {
                if ((value.StartsWith("\"") && value.Length == 1) ||
                   (value.StartsWith("\"") && !value.EndsWith("\"") && arr.Length >= (startIndex + 2)))
                {
                    for (int i = (startIndex + 1); i < arr.Length; i++)
                    {
                        string temp = arr[i].Trim();
                        value += " " + temp;
                        if (temp.EndsWith("\"") || (i + 1 == arr.Length))
                        {
                            endIndex = i;
                            break;
                        }
                    }
                }
            }

            return value.Trim(' ', '"');
        }

        private string GetValidString(string source, bool surroundBlanksWithQuote = false)
        {
            if (String.IsNullOrEmpty(source))
                return surroundBlanksWithQuote ? StringUtility.SurroundString(String.Empty, StringUtility.GetAsciiQuote()) : String.Empty;

            //Preced quote with backslash
            source = source.Replace(StringUtility.GetAsciiQuote().ToString(), StringUtility.GetAsciiBackslash().ToString() + StringUtility.GetAsciiQuote().ToString());

            if (surroundBlanksWithQuote && StringUtility.ContainsBlank(source))
                source = StringUtility.SurroundString(source, StringUtility.GetAsciiQuote());

            return source;
        }

        private string GetValidStrings(bool surroundBlanksWithQuote, bool surroundResultWithQuote, params string[] sources)
        {
            string result = "";
            bool valid = false;

            foreach (string str in sources)
            {
                //Check that all strings are not empty
                if (!String.IsNullOrEmpty(str))
                    valid = true;

                if (!String.IsNullOrEmpty(str))
                {
                    if (!String.IsNullOrEmpty(result))
                        result += " ";

                    //Preced quote with backslash
                    result += str.Replace(StringUtility.GetAsciiQuote().ToString(), StringUtility.GetAsciiBackslash().ToString() + StringUtility.GetAsciiQuote().ToString());

                    if (surroundBlanksWithQuote && StringUtility.ContainsBlank(result))
                        result = StringUtility.SurroundString(result, StringUtility.GetAsciiQuote());
                }
            }

            if (valid)
            {
                if (surroundResultWithQuote && StringUtility.ContainsBlank(result))
                    result = StringUtility.SurroundString(result, StringUtility.GetAsciiQuote());
            }
            else
            {
                result = String.Empty;
                if (surroundBlanksWithQuote)
                    result = StringUtility.SurroundString(result, StringUtility.GetAsciiQuote());
            }

            return result;
        }

        private string GetValidQuantity(decimal? source)
        {
            if (!source.HasValue)
                return StringUtility.GetAsciiDoubleQoute();

            return Decimal.Round(source.Value, 6).ToString().Replace(',', '.');
        }

        private string GetValidDecimal(decimal source)
        {
            return source.ToString("0.00##", CultureInfo.InvariantCulture);
        }

        private string GetValidDate(DateTime? source)
        {
            if (!source.HasValue)
                return StringUtility.GetAsciiDoubleQoute();

            return Convert.ToDateTime(source.Value).ToString("yyyyMMdd");
        }

        private string[] Split(string line)
        {
            //Constants.SIE_DELIMETER.ToCharArray()
            char[] separator = new char[2];
            separator[0] = StringUtility.GetAsciiSpace();
            separator[1] = StringUtility.GetAsciiTab();
            return line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }

        #region AccountDim

        private bool IsAccountDimRuleFulfilled(SieImportItemBase item, AccountDim accountDim)
        {
            bool success = false;
            if (item is SieAccountInternalItem internalItem)
                success = AccountManager.IsAccountValidInAccountDim(accountDim.MinChar, accountDim.MaxChar, internalItem.ObjectCode);
            if (item is SieAccountStdItem stdItem)
                success = AccountManager.IsAccountValidInAccountDim(accountDim.MinChar, accountDim.MaxChar, stdItem.AccountNr);
            if (item is SieAccountStdInBalanceItem balanceItem)
                success = AccountManager.IsAccountValidInAccountDim(accountDim.MinChar, accountDim.MaxChar, balanceItem.AccountNr);
            if (item is SieTransactionItem transactionItem)
                success = AccountManager.IsAccountValidInAccountDim(accountDim.MinChar, accountDim.MaxChar, transactionItem.AccountNr);
            return success;
        }

        #endregion

        #endregion
    }
}
