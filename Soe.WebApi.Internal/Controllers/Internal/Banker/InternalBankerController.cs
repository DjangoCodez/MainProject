using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Banker.Shared.Types;
using SoftOne.Soe.Business.Core;
using System.Linq;
using System.IO;
using System.Text;
using System.Globalization;
using System.Threading;
using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;
using Soe.Sys.Common.DTO;
using System.Threading.Tasks;

namespace Soe.Api.Internal.Controllers.Internal.Banker
{
    [RoutePrefix("Internal/Banker/Banker")]
    public class InternalBankerController : ApiBase
    {
        #region Constructor

        const string LoginName = "SoftOne (Banking)";

        public InternalBankerController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("Status/")]
        [ResponseType(typeof(BankerActionResult))]
        public IHttpActionResult BankerStatus(BankerActionResult bankerAction)
        {
            ActionResult result = null;
            switch (bankerAction.MaterialType)
            {
                case MaterialType.Payment:
                    var pm = new PaymentIOManager(null);
                    result = pm.UpdateExportStatus(bankerAction.Success, bankerAction.MsgId, bankerAction.Status, bankerAction.Message);
                    break;
                case MaterialType.FinvoiceSend:
                case MaterialType.FinvoiceAttachmentSend:
                    var idm = new InvoiceDistributionManager(null);
                    result = idm.UpdateDistributionItem(bankerAction.MsgId, TermGroup_EDistributionType.Finvoice, bankerAction.Success ? TermGroup_EDistributionStatusType.Sent : TermGroup_EDistributionStatusType.Error, bankerAction.Message);
                    break;
                default:
                    result = new ActionResult("Unknown material type");
                    break;
            }

            return Content(HttpStatusCode.OK, new BankerActionResult {MsgId = bankerAction.MsgId, Success = result.Success, Message = string.IsNullOrEmpty(result.ErrorMessage) ? result.InfoMessage: result.ErrorMessage });
        }

        [HttpPost]
        [Route("FindActorCompany/")]
        [ResponseType(typeof(BankerSoftoneCompany))]
        public IHttpActionResult FindActorCompany(BankerFindSoftoneCompanyRequest request)
        {
            var ssm = new SysServiceManager(null);
            var response = new BankerSoftoneCompany { Success = false, SysCompDBId = ssm.GetSysCompDBId() ?? 0, };

            if (!string.IsNullOrEmpty(request.MsgId))
            {
                var pm = new PaymentManager(null);
                var company = pm.GetCompanyFromPaymentMsgId(request.MsgId);
                if (company != null)
                {
                    response = new BankerSoftoneCompany
                    {
                        ActorCompanyId = company.ActorCompanyId,
                        CompanyGuid = company.CompanyGuid,
                        SysCompDBId = ssm.GetSysCompDBId() ?? 0,
                        Success = true
                    };
                }
                else
                {
                    response.Message = "FindActorCompany: Company not found for MsgId:" + request.MsgId;
                }
            }
            else if (!string.IsNullOrEmpty(request.FinvoiceAddress))
            {
                var sm = new SettingManager(null);
                var settings = sm.GetCompanySettingsWithUniqueStringValue((int)CompanySettingType.BillingFinvoiceAddress, request.FinvoiceAddress);
                foreach (var settingActorCompanyId in settings.Select(x => x.ActorCompanyId))
                {
                    var useforbankintegration = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.FinvoiceUseBankIntegration, 0, settingActorCompanyId, 0);
                    if (useforbankintegration)
                    {
                        var cm = new CompanyManager(null);
                        var company = cm.GetCompany(settingActorCompanyId);
                        response = new BankerSoftoneCompany
                        {
                            ActorCompanyId = settingActorCompanyId,
                            CompanyGuid = company.CompanyGuid.ToString(),
                            SysCompDBId = ssm.GetSysCompDBId() ?? 0,
                            Success = true
                        };
                    }
                }
            }
            else
            {
                var cm = new CompanyManager(null);

                TermGroup_SysPaymentType sysPaymentType = TermGroup_SysPaymentType.Unknown;
                var account = request.Account;
                if (account != null && !string.IsNullOrEmpty(account.BIC) && !string.IsNullOrEmpty(account.AccountNr) && !string.IsNullOrEmpty(account.AccountType))
                {
                    if (account.AccountType == TermGroup_ISOPaymentAccountType.BGNR.ToString())
                    {
                        sysPaymentType = TermGroup_SysPaymentType.BG;
                    }
                    else if (account.AccountType == TermGroup_ISOPaymentAccountType.BBAN.ToString())
                    {
                        sysPaymentType = TermGroup_SysPaymentType.Bank;
                    }
                    else if (account.AccountType == TermGroup_ISOPaymentAccountType.IBAN.ToString())
                    {
                        sysPaymentType = TermGroup_SysPaymentType.BIC;
                    }

                    var company = cm.GetCompaniesBySearch(new CompanySearchFilterDTO { Demo = false, BankConnected = true, BankAccountBIC = account.BIC, BankAccountNr = account.AccountNr, BankAccountType = sysPaymentType })?.FirstOrDefault();
                    if (company != null)
                    {
                        response = new BankerSoftoneCompany
                        {
                            ActorCompanyId = company.ActorCompanyId,
                            CompanyGuid = company.CompanyGuid,
                            SysCompDBId = ssm.GetSysCompDBId() ?? 0,
                            Success = true
                        };
                    }
                }
                else
                {
                    response.Message = $"FindActorCompany got null account {account?.BIC}:{account?.AccountNr}:{account?.AccountType}";
                }
            }

            return Content(HttpStatusCode.OK, response);
        }

        [HttpPost]
        [Route("FindActorCompanySys/")]
        [ResponseType(typeof(BankerSoftoneCompany))]
        public IHttpActionResult FindActorCompanySys(BankerFindSoftoneCompanyRequest request)
        {
            var ssm = new SysServiceManager(null);
            var errorResponse = new BankerSoftoneCompany { Success = false, SysCompDBId = ssm.GetSysCompDBId() ?? 0, };

            if (!string.IsNullOrEmpty(request.MsgId))
            {
                //Prio 1: Search by MsgId which corresponds to a payment
                //          -> We can only do this in the CompDBs.
                //             The banker service iterates over all internal APIs to accomodate this.
                var pm = new PaymentManager(null);
                var company = pm.GetCompanyFromPaymentMsgId(request.MsgId);
                if (company != null)
                {
                    var response = new BankerSoftoneCompany
                    {
                        ActorCompanyId = company.ActorCompanyId,
                        CompanyGuid = company.CompanyGuid,
                        SysCompDBId = ssm.GetSysCompDBId() ?? 0,
                        Success = true
                    };
                    return Content(HttpStatusCode.OK, response);
                }
                else
                {
                    errorResponse.Message = "FindActorCompany: Company not found for MsgId:" + request.MsgId;
                    return Content(HttpStatusCode.OK, errorResponse);
                }
            }
            
            if (!string.IsNullOrEmpty(request.FinvoiceAddress))
            {
                //Prio 2: Search by FinvoiceAddress
                //          -> We use the SysService for this.
                var filter = new SearchSysCompanyDTO
                {
                    UsesBankIntegration = true,
                    FinvoiceAddress = request.FinvoiceAddress,
                };
                var companies = ssm.SearchSysCompanies(filter);

                if (companies.Count == 0)
                {
                    errorResponse.Message = "FindActorCompanySys: No companies found for FinvoiceAddress:" + request.FinvoiceAddress;
                    return Content(HttpStatusCode.OK, errorResponse);
                }
                if (companies.Count > 1)
                {
                    errorResponse.Message = "FindActorCompanySys: Multiple companies found for FinvoiceAddress:" + request.FinvoiceAddress;
                    return Content(HttpStatusCode.OK, errorResponse);
                }

                var company = companies.First();
                var response = new BankerSoftoneCompany
                {
                    Success = true,
                    ActorCompanyId = company.ActorCompanyId.GetValueOrDefault(),
                    CompanyGuid = company.CompanyGuid.ToString(),
                    SysCompDBId = company.SysCompDbId,
                };
                return Content(HttpStatusCode.OK, response);
            }


            var account = request.Account;
            if (account != null && !string.IsNullOrEmpty(account.BIC) && !string.IsNullOrEmpty(account.AccountNr) && !string.IsNullOrEmpty(account.AccountType))
            {
                //Prio 3: Search by BankAccount
                //          -> We use the SysService for this.
                TermGroup_SysPaymentType sysPaymentType = TermGroup_SysPaymentType.Unknown;
                if (account.AccountType == TermGroup_ISOPaymentAccountType.BGNR.ToString())
                    sysPaymentType = TermGroup_SysPaymentType.BG;
                else if (account.AccountType == TermGroup_ISOPaymentAccountType.BBAN.ToString())
                    sysPaymentType = TermGroup_SysPaymentType.Bank;
                else if (account.AccountType == TermGroup_ISOPaymentAccountType.IBAN.ToString())
                    sysPaymentType = TermGroup_SysPaymentType.BIC;

                var filter = new SearchSysCompanyDTO()
                {
                    UsesBankIntegration = true,
                    BankAccount = new SearchSysCompanyBankAccountDTO()
                    {
                        BIC = account.BIC,
                        PaymentNr = account.AccountNr,
                        PaymentType = sysPaymentType,
                    }
                };
                var companies = ssm.SearchSysCompanies(filter);

                if (companies.Count == 0)
                {
                    errorResponse.Message = "FindActorCompanySys: No companies found for account:" + account.BIC + ":" + account.AccountNr + ":" + account.AccountType;
                    return Content(HttpStatusCode.OK, errorResponse);
                }
                if (companies.Count > 1)
                {
                    errorResponse.Message = "FindActorCompanySys: Multiple companies found for account:" + account.BIC + ":" + account.AccountNr + ":" + account.AccountType;
                    return Content(HttpStatusCode.OK, errorResponse);
                }

                var company = companies.First();
                var response = new BankerSoftoneCompany
                {
                    Success = true,
                    ActorCompanyId = company.ActorCompanyId.GetValueOrDefault(),
                    CompanyGuid = company.CompanyGuid.ToString(),
                    SysCompDBId = company.SysCompDbId,
                };
                return Content(HttpStatusCode.OK, response);
            }

            errorResponse.Message = "No searching criteria provided";
            return Content(HttpStatusCode.OK, errorResponse);
        }

        [HttpPost]
        [Route("ImportFiles/")]
        [ResponseType(typeof(BankerActionResult))]
        public async Task<IHttpActionResult> ImportFiles(BankerFileMessages files)
        {
            var bankerResult = new BankerActionResult();
            bankerResult.StrDict = new Dictionary<int, string>();
            bankerResult.ItemResults = new Dictionary<int, BankerActionResult>();

            var culture = "";
            var companyManager = new CompanyManager(null);
                
            foreach (var companyMessages in files.Messages.GroupBy(x => x.CompanyGuid).OrderBy(x=> x.Key).ToList())
            {
                if (string.IsNullOrEmpty(companyMessages.Key))
                {
                    foreach (var message in companyMessages)
                    {
                        bankerResult.StrDict.Add(message.AvaloDownloadedFileId, $"Error: Missing CompanyGuid");
                    }
                    continue;
                }

                var actorCompanyId = companyManager.GetActorCompanyIdFromCompanyGuid(companyMessages.Key) ?? 0;
                if (actorCompanyId == 0)
                {
                    foreach (var message in companyMessages)
                    {
                        bankerResult.StrDict.Add(message.AvaloDownloadedFileId, $"Error: No actorCompanyId was found for CompanyGuid:" + companyMessages.Key);
                    }
                    continue;
                }

                var sysCountryId = companyManager.GetCompanySysCountryId(actorCompanyId);

                if (sysCountryId == (int)TermGroup_Country.FI)
                {
                    culture = Constants.SYSLANGUAGE_LANGCODE_FINISH;
                }
                else if (sysCountryId == (int)TermGroup_Country.SE)
                {
                    culture = Constants.SYSLANGUAGE_LANGCODE_SWEDISH;
                }
                else
                {
                    culture = Constants.SYSLANGUAGE_LANGCODE_ENGLISH; 
                }

                SetLanguage(culture);

                var parameterObject = GetParameterObject(actorCompanyId, 0, null, LoginName);
                var paymentManager = new PaymentIOManager(parameterObject);
                foreach (var message in companyMessages)
                {
                    var result = new ActionResult(false);
                    switch( message.Type)
                    {
                        case MaterialType.CreditNotification054:
                        case MaterialType.DebetNotification054:
                            {
                                ImportPaymentType importTyp = message.Type == MaterialType.DebetNotification054 ? ImportPaymentType.SupplierPayment : ImportPaymentType.CustomerPayment;
                                result = paymentManager.ImportCAMTFile(message.Account.BIC, message.Account.AccountNr, actorCompanyId, importTyp, message.File);
                                break;
                            }
                        case MaterialType.DebetCreditNotification053:
                            {
                                result = paymentManager.ImportCAMT53(message.Account.BIC, message.Account.AccountNr, actorCompanyId, message.File);
                                break;
                            }
                        case MaterialType.PaymentFeedback002:
                            {
                                result = paymentManager.ImportPain002(message.File);
                                break;
                            }
                        case MaterialType.OnboardingAcmt14:
                        case MaterialType.OnboardingSHB:
                            {
                                result = new ActionResult("OnBoardingFile Should not be imported"); //sepaMananger.ImportOnboardingFile(message.File, message.ActorCompanyId);
                                break;
                            }
                        case MaterialType.FinvoiceFeedback:
                            {
                                var im = new InvoiceDistributionManager(parameterObject);
                                result = im.UpdateFinvoiceFromFeedback(actorCompanyId, message.File);
                                break;
                            }
                        case MaterialType.FinvoiceDownload:
                            {
                                var edi = new EdiManager(parameterObject);
                                
                                result = await edi.AddFinvoiceFromFileImportAsync("", "Finvoice.xml", actorCompanyId, message.File, true).ConfigureAwait(false);
                                result.Success = result.IntegerValue > 0;
                                break;
                            }
                        case MaterialType.FinvoiceAttachmentDownload:
                            {
                                var edi = new EdiManager(GetParameterObject(actorCompanyId, 0, null, LoginName));
                                result = edi.AddFinvoiceAttachment("finvoiceattachment.xml", actorCompanyId, new MemoryStream(Encoding.UTF8.GetBytes(message.File)));
                                break;
                            }
                    }

                    bankerResult.ItemResults.Add(message.AvaloDownloadedFileId, new BankerActionResult { Success = result.Success, Message = result.Success? result.InfoMessage : result.ErrorMessage, MessageDetails = "InternalBankController" });
                    bankerResult.StrDict.Add(message.AvaloDownloadedFileId, result.Success? "" : $"Error: {result.ErrorMessage}");
                }
            }
            
            return Content(HttpStatusCode.OK, bankerResult);
        }

        #endregion
    }
}