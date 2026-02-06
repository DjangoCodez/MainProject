using Soe.Edi.Common.DTO;
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using static Soe.Edi.Common.Enumerations;
using static Soe.Sys.Common.Enumerations;

namespace SoftOne.Soe.Business.Core
{
    public class SysServiceManager : ManagerBase
    {
        #region Variables

       
        #endregion

        #region Ctor      

        public SysServiceManager(ParameterObject parameterObject) : base(parameterObject)
        {
            SysConnectorBase.init();
        }


        #endregion

        #region Clients


        protected void LogError(Exception exception, string message)
        {

        }

        #endregion

        public static Dictionary<string, string> GetConnectApiKeys()
        {
            return SysMiscConnector.GetConnectApiKey();
        }

        #region SysCompany

        #region SysCompany

        public SysCompanyDTO CreateSysCompanyDTO(int actorCompanyId, int? sysCompDbId = null)
        {
            try
            {
                if (sysCompDbId == null)
                    sysCompDbId = SettingManager.GetIntSetting(SettingMainType.Application, (int)ApplicationSettingType.sysCompDbId, 0, 0, 0);

                var company = CompanyManager.GetCompany(actorCompanyId, loadLicense: true, loadEdiConnection: true);
                var companyApiKey = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, 0, actorCompanyId, 0);


                //Only fetch finvoice address if the company is Finnish
                var finvoiceAddress = company.SysCountryId == (int)TermGroup_Languages.Finnish ? 
                    SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceAddress, 0, actorCompanyId, 0) : 
                    null;

                if (string.IsNullOrEmpty(companyApiKey))
                {
                    SettingManager.UpdateInsertStringSetting(SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, Guid.NewGuid().ToString(), parameterObject != null && parameterObject.SoeUser != null ? parameterObject.UserId : 0, company.ActorCompanyId, 0);
                    companyApiKey = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, 0, actorCompanyId, 0);
                }

                Guid apiKey = new Guid();

                if (Guid.TryParse(companyApiKey, out apiKey))
                {
                    if (!string.IsNullOrEmpty(companyApiKey))
                    {
                        SoeSysEntityState state = SoeSysEntityState.Active;

                        if (company.License.State != (int)SoeEntityState.Active)
                            state = (SoeSysEntityState)company.License.State;

                        if (company.State != (int)SoeEntityState.Active)
                            state = (SoeSysEntityState)company.State;

                        SysCompanyDTO sysCompanyDTO = new SysCompanyDTO()
                        {
                            CompanyApiKey = apiKey,
                            CompanyGuid = company.CompanyGuid,
                            Name = company.Name,
                            SysCompDbId = sysCompDbId.Value,
                            IsSOP = false,
                            ActorCompanyId = actorCompanyId,
                            LicenseId = company.LicenseId,
                            LicenseNumber = company.License.LicenseNr,
                            LicenseName = company.License.Name,
                            Number = company.CompanyNr.HasValue ? company.CompanyNr.Value.ToString() : string.Empty,
                            State = state,
                        };

                        if (company.EdiConnection != null)
                        {
                            var EdiConnections = company.EdiConnection;

                            foreach (var connect in EdiConnections)
                            {
                                SysCompanySettingDTO sysCompanySettingDTO = new SysCompanySettingDTO();
                                sysCompanySettingDTO.SettingType = SysCompanySettingType.SysEdiMessageTypeAndNumber;

                                sysCompanySettingDTO.StringValue = connect.BuyerNr;
                                sysCompanySettingDTO.IntValue = connect.SysEdiMsgId;

                                if (sysCompanyDTO.SysCompanySettingDTOs == null)
                                    sysCompanyDTO.SysCompanySettingDTOs = new System.Collections.Generic.List<SysCompanySettingDTO>();

                                sysCompanyDTO.SysCompanySettingDTOs.Add(sysCompanySettingDTO);
                            }
                        }

                        if (!string.IsNullOrEmpty(company.OrgNr))
                        {
                            SysCompanySettingDTO sysCompanySettingDTO = new SysCompanySettingDTO();
                            sysCompanySettingDTO.SettingType = SysCompanySettingType.OrganisationNumber;

                            sysCompanySettingDTO.StringValue = company.OrgNr;

                            if (sysCompanyDTO.SysCompanySettingDTOs == null)
                                sysCompanyDTO.SysCompanySettingDTOs = new System.Collections.Generic.List<SysCompanySettingDTO>();

                            sysCompanyDTO.SysCompanySettingDTOs.Add(sysCompanySettingDTO);
                        }

                        var enumsToFind = (from entry in Enum.GetValues(typeof(TermGroup_CompanyEdiType)).OfType<TermGroup_CompanyEdiType>()
                                           where entry != TermGroup_CompanyEdiType.Unknown && entry != TermGroup_CompanyEdiType.Symbrio
                                           select entry).ToArray();

                        var companyEdis = EdiManager.GetCompanyEdis(actorCompanyId, enumsToFind);
                        if (!companyEdis.IsNullOrEmpty())
                        {
                            foreach (var edi in companyEdis)
                            {
                                SysCompanySettingDTO sysCompanySettingDTO = new SysCompanySettingDTO();
                                sysCompanySettingDTO.ChildSysCompanySettingDTOs = new List<SysCompanySettingDTO>();
                                sysCompanySettingDTO.SettingType = SysCompanySettingType.ExternalFtp;

                                sysCompanySettingDTO.StringValue = edi.Address;

                                if (sysCompanyDTO.SysCompanySettingDTOs == null)
                                    sysCompanyDTO.SysCompanySettingDTOs = new System.Collections.Generic.List<SysCompanySettingDTO>();

                                SysCompanySettingDTO sysCompanySettingDTO2 = new SysCompanySettingDTO();
                                sysCompanySettingDTO2.SettingType = SysCompanySettingType.UserName;
                                sysCompanySettingDTO2.StringValue = edi.Username;
                                sysCompanySettingDTO.ChildSysCompanySettingDTOs.Add(sysCompanySettingDTO2);

                                SysCompanySettingDTO sysCompanySettingDTO3 = new SysCompanySettingDTO();
                                sysCompanySettingDTO3.SettingType = SysCompanySettingType.Password;
                                sysCompanySettingDTO3.StringValue = edi.Password;
                                sysCompanySettingDTO.ChildSysCompanySettingDTOs.Add(sysCompanySettingDTO3);

                                sysCompanyDTO.SysCompanySettingDTOs.Add(sysCompanySettingDTO);
                            }
                        }

                        if (!string.IsNullOrEmpty(finvoiceAddress))
                        {
                            var settingDTO = new SysCompanySettingDTO()
                            {
                                SettingType = SysCompanySettingType.FinvoiceAddress,
                                StringValue = finvoiceAddress
                            };

                            if (sysCompanyDTO.SysCompanySettingDTOs == null)
                                sysCompanyDTO.SysCompanySettingDTOs = new System.Collections.Generic.List<SysCompanySettingDTO>();

                            sysCompanyDTO.SysCompanySettingDTOs.Add(settingDTO);
                        }

                        return sysCompanyDTO;
                    }
                }

                return new SysCompanyDTO();
            }
            catch
            {
                return null;
            }

        }

        public List<SysCompanyDTO> GetSysCompanies(int? sysCompanyId = null)
        {
            if (sysCompanyId.HasValue)
                return SysCompanyConnector.GetSysCompanyDTOs().Where(c => c.SysCompanyId == sysCompanyId.Value).ToList();

            return SysCompanyConnector.GetSysCompanyDTOs();
        }
        public List<Common.DTO.SmallGenericType> GetSysCompanyDict()
        {
            var list = SysCompanyConnector.GetSysCompanyDTOs();
            List<SoftOne.Soe.Common.DTO.SmallGenericType> smallList = new List<Common.DTO.SmallGenericType>();
            foreach (var item in list)
            {
                smallList.Add(new Common.DTO.SmallGenericType()
                {
                    Id = item.SysCompanyId,
                    Name = item.Name,
                });
            }
            return smallList;
        }

        public List<SysCompanyDTO> SearchSysCompanies(SearchSysCompanyDTO filter)
        {
            return SysCompanyConnector.SearchSysCompanies(filter, false);
        }

        public SysCompanyDTO GetSysCompany(string companyApiKey, int sysCompDBId)
        {
            return SysCompanyConnector.GetSysCompanyDTO(companyApiKey, sysCompDBId);
        }

        public SysCompanyDTO GetSysCompany(int sysCompanyId, bool includeSettings = false, bool includeBankAccounts = false, bool includeUniqueValues = false)
        {
            return SysCompanyConnector.GetSysCompanyDTO(sysCompanyId, includeSettings, includeBankAccounts, includeUniqueValues);
        }

        public ActionResult SaveSysCompany(SysCompanyDTO sysCompanyDTO, int sysCompDBId)
        {
            sysCompanyDTO.ModifiedBy = base.GetUserDetails();

            if (sysCompDBId == 0 && sysCompanyDTO.SysCompDbId == 0)
            {
                sysCompanyDTO.SysCompDbId = CompDbCache.Instance.SysCompDbId;
            }

            var result = SysCompanyConnector.SaveSysCompanyDTO(sysCompanyDTO);

            if (!result.Success)
            {
                if (result.ErrorMessage == "duplicate")
                    result.ErrorMessage = GetText(7756, 1, "Bankkonton innehåller dubletter");
            }

            return result;
        }

        public ActionResult SaveSysCompanies(List<SysCompanyDTO> sysCompanyDTOs)
        {
            return SysCompanyConnector.SaveSysCompanyDTOs(sysCompanyDTOs);
        }

        #endregion

        #region SysCompDB

        public static int? GetSysCompDBIdFromSetting()
        {
            return ConfigurationSetupUtil.GetCurrentSysCompDbId();
        }

        public int? GetSysCompDBId()
        {
            return GetSysCompDBIdFromSetting();
        }

        public List<SysCompDBDTO> GetSysCompDBs()
        {
            return SysCompanyConnector.GetSysCompDBDTOs();
        }

        public SysCompDBDTO GetSysCompDB(int sysCompDBId)
        {
            return SysCompanyConnector.GetSysCompDBDTO(sysCompDBId);
        }

        #endregion

        #region SysCompServer

        public List<SysCompServerDTO> GetSysCompServers()
        {
            return SysCompanyConnector.GetSysCompServerDTOs();
        }

        public SysCompServerDTO GetSysCompServer(int sysCompServerId)
        {
            return SysCompanyConnector.GetSysCompServerDTO(sysCompServerId);
        }

        #endregion

        #region SysWholeseller

        public List<Common.DTO.SysWholesellerDTO> GetSysWholesellers()
        {
            return SysWholesellerConnector.GetSysWholesellerDTOs();
        }

        public Common.DTO.SysWholesellerDTO GetSysWholeseller(int sysWholesellerId)
        {
            return SysWholesellerConnector.GetSysWholesellerDTO(sysWholesellerId);
        }

        public ActionResult SaveSysWholeseller(Common.DTO.SysWholesellerDTO SysWholesellerDTO)
        {
            return SysWholesellerConnector.SaveSysWholesellerDTO(SysWholesellerDTO);
        }

        #endregion

        #region SysEdiMessageRaw

        public List<SysEdiMessageRawDTO> GetSysEdiMessageRaws()
        {
            return SysEdiConnector.GetSysEdiMessageRawDTOs();
        }

        public SysEdiMessageRawDTO GetSysEdiMessageRaw(int sysEdiMessageRawId)
        {
            return SysEdiConnector.GetSysEdiMessageRawDTO(sysEdiMessageRawId);
        }

        #endregion

        #region SysEdiMessageHead

        public List<SysEdiMessageHeadDTO> GetSysEdiMessageHeads()
        {
            return SysEdiConnector.GetSysEdiMessageHeadDTOs();
        }

        public List<SysEdiMessageHeadGridDTO> GetSysEdiMessageGridHeads(SysEdiMessageHeadStatus status, int take, bool missingSysCompanyId, int? ediMessageHeadId = null)
        {
            List <SysEdiMessageHeadGridDTO> sysEdiMessages = SysEdiConnector.GetSysEdiMessageHeadGridDTOs(status, take, missingSysCompanyId);
            if (ediMessageHeadId.HasValue)
                sysEdiMessages = sysEdiMessages.Where(edi => edi.SysEdiMessageHeadId == ediMessageHeadId.Value).ToList();
            return sysEdiMessages;
        }

        public SysEdiMessageHeadDTO GetSysEdiMessageHead(int sysEdiMessageHeadId)
        {
            return SysEdiConnector.GetSysEdiMessageHeadDTO(sysEdiMessageHeadId);
        }

        public ActionResult GetSysEdiMessageHeadMessage(int sysEdiMessageHeadId)
        {
            return SysEdiConnector.GetSysEdiMessageHeadMessage(sysEdiMessageHeadId);
        }

        public ActionResult SaveSysEdiMessageHead(SysEdiMessageHeadDTO dto)
        {
            return SysEdiConnector.SaveSysEdiMessageHeadDTO(dto);
        }

        public List<SysEdiMessageHeadGridDTO> GetSysEdiMessagesGridHeads(bool open, bool closed, bool raw, bool missingSysCompanyId)
        {
            List<SysEdiMessageHeadGridDTO> sysEdiMessages = SysEdiConnector.GetSysEdiMessagesGridDTOs(new SysEdiMessageFilterDTO { Open = open,Sent = closed,IncludeRaw = raw, Take = 30000, OnlyMissingCompanyId = missingSysCompanyId });
            return sysEdiMessages;
        }

        #endregion

        #endregion

        #region SysServer

        public List<SysServerDTO> GetSysservers()
        {
            return SysLoginConnector.GetSysServers();
        }

        public SysServerDTO GetSysserver(int sysServerId)
        {
            List<SysServerDTO> servers = this.GetSysservers();
            if (servers == null)
                return null;

            return servers.FirstOrDefault(x => x.SysServerId == sysServerId);
        }
        public List<SysServerDTO> GetValidSysServers()
        {
            List<SysServerDTO> sysServers = GetSysservers();
            int? sysCompDBid = GetSysCompDBId();

            if (sysCompDBid.HasValue)
            {
                List<SysServerDTO> sysServersFromStatus = SoftOneStatusConnector.GetSysServersFromSysCompDb(sysCompDBid.Value);
                if (!sysServersFromStatus.IsNullOrEmpty())
                {
                    foreach (var item in sysServersFromStatus)
                    {
                        var sysServer = sysServers.FirstOrDefault(f => f.SysServerId == item.SysServerId);
                        if (sysServer != null)
                        {
                            item.Url = sysServer.Url;
                            item.UseLoadBalancer = sysServer.UseLoadBalancer;
                        }
                    }

                    return sysServersFromStatus;
                }
            }

            return sysServers;
        }

        #endregion

        #region EDI

        public ActionResult RunFlow()
        {
            return SysEdiConnector.RunFlow();
        }

        public ActionResult ImportEdiFromFtp()
        {
            return SysEdiConnector.ImportEdiFromFtp();
        }

        public ActionResult ImportEdiMessageHeads()
        {
            return SysEdiConnector.ImportEdiMessageHeads();
        }

        #endregion

        #region Holiday

        public List<Common.DTO.SysHolidayDTO> GetSysHolidayDTOs()
        {
            List<Common.DTO.SysHolidayDTO> dtos = SysHolidayConnector.GetSysHolidayDTOs();

            if (dtos != null)
            {
                return dtos;
            }
            else
                return new List<Common.DTO.SysHolidayDTO>();
        }

        public List<Common.DTO.SysHolidayTypeDTO> GetSysHolidayTypeDTOs()
        {
            List<Common.DTO.SysHolidayTypeDTO> dtos = SysHolidayConnector.GetSysHolidayTypeDTOs();

            if (dtos != null)
            {
                foreach (var item in dtos)
                {
                    item.Name = GetText(item.SysTermId, item.SysTermGroupId, "");
                }
            }
            else
                return new List<Common.DTO.SysHolidayTypeDTO>();

            return dtos;
        }

        #endregion

        #region Product

        public List<Common.DTO.ExternalProductSmallDTO> SysProductSearch(TermGroup_Country country, int fetchsize, string search, List<int> sysPriceListHeadIds = null)
        {
            return SysProductConnector.ProductSearch(country, fetchsize, search, sysPriceListHeadIds);

        }

        public List<Common.DTO.ExternalProductSmallDTO> SysProductAzureSearch(TermGroup_Country country, Common.Util.ExternalProductType externalProductType, int fetchsize, string number, string name, string groupIdentifier = "", string text = "", List<int> sysPriceListHeadIds = null)
        {
            return SysProductConnector.ProductAzureSearch(country, externalProductType, fetchsize, number, name, groupIdentifier, text, sysPriceListHeadIds);
        }

        public ActionResult PopulateAzureSearch()
        {
            return SysProductConnector.PopulateAzureSearch();

        }

        public List<Common.DTO.SysPriceListDTO> GetSysPriceListDTOsForProduct(TermGroup_Country country, Common.DTO.ExternalProductSmallDTO externalProductSmallDTO)
        {
            List<Common.DTO.ExternalProductSmallDTO> externalProductSmallDTOs = new List<Common.DTO.ExternalProductSmallDTO>();
            externalProductSmallDTOs.Add(externalProductSmallDTO);
            return SysProductConnector.GetPriceListDTOsForProducts(country, externalProductSmallDTOs);

        }

        public List<Common.DTO.SysPriceListDTO> GetSysPriceListDTOsForProducts(TermGroup_Country country, List<Common.DTO.ExternalProductSmallDTO> externalProductSmallDTOs)
        {
            return SysProductConnector.GetPriceListDTOsForProducts(country, externalProductSmallDTOs);

        }

        public List<Common.DTO.SysWholesellerDTO> GetSysWholesellerDTOs(TermGroup_Country country)
        {
            return SysProductConnector.GetSysWholesellerDTOs(country);

        }

        #endregion

        new public void LogError(string error, long? taskWatchLogId = null)
        {
            if (taskWatchLogId.HasValue)
                error += $"taskWatchLogId={taskWatchLogId.Value}";

            if (!error.IsNullOrEmpty())
                SysLogConnector.SaveErrorMessage(error);
        }

        new public void LogInfo(string information, long? taskWatchLogId = null)
        {
            base.LogInfo(information, taskWatchLogId: taskWatchLogId);
        }
    }
}
