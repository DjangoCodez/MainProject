using SoftOne.Soe.Business.Util.API.AvionData;
using PRH = SoftOne.Soe.Business.Util.API.AvionData.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Core
{
    internal class PRHCompanySearchManager : ManagerBase
    {
        #region Variables

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AvionConnector _avionConnector;

        #endregion
        public PRHCompanySearchManager(ParameterObject parameterObject) : base(parameterObject)
        {
            this._avionConnector = new AvionConnector();
        }

        public List<ExternalCompanyResultDTO> GetExternalComanyResultDTOsFromPrh(ExternalCompanyFilterDTO filter)
        {
            var result = new List<ExternalCompanyResultDTO>();
            try
            {
                var prhResult = this._avionConnector.SearchCompanies(new PRH.CompanySearchFilter
                {
                    Name = filter.Name,
                    BusinessId = filter.RegistrationNr
                });

                if (prhResult.IsSuccess && prhResult.Result != null && prhResult.Result.Count > 0)
                {
                    result = prhResult.Result.Select(c => new ExternalCompanyResultDTO
                    {
                        RegistrationNr = c.BusinessId.Value,
                        Name = BuildName(c.Names),
                        StreetAddress = c.Addresses != null && c.Addresses.Any(a => a.Type == 1) ?
                            BuildPRHAddress(c.Addresses.FirstOrDefault(a => a.Type == 1)) : null,
                        PostalAddress = c.Addresses != null && c.Addresses.Any(a => a.Type == 2) ?
                            BuildPRHAddress(c.Addresses.FirstOrDefault(a => a.Type == 2)) : null,
                        WebUrl = c.Website?.Url ?? string.Empty,
                    }).ToList();
                }
                else if (prhResult.Error != null)
                {
                    this.LogError($"Error occurred while fetching data from PRH. - ${prhResult.Error.Message}");
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, this.log);
            }
            return result;
        }

        private string BuildName(List<PRH.RegisterName> names)
        {
            return names.Count() == 1
                                ? names[0].Name
                                : names.FirstOrDefault(x => x.Type == "1" && string.IsNullOrWhiteSpace(x.EndDate))?.Name
                                  ?? names.FirstOrDefault(x => x.Type == "1")?.Name
                                  ?? names.OrderByDescending(x => Convert.ToDateTime(x.EndDate, System.Globalization.CultureInfo.InvariantCulture))
                                      .FirstOrDefault()?.Name
                                  ?? string.Empty;
        }

        private ExternalCompanyAddressDTO BuildPRHAddress(PRH.Address address)
        {
            if (address == null) return null;

            ExternalCompanyAddressDTO addressDto = new ExternalCompanyAddressDTO();

            addressDto.CO = address.Co ?? "";
            addressDto.Street = address.Street ?? "";

            if (address.BuildingNumber.HasValue())
                addressDto.Street = $"{addressDto.Street} {address.BuildingNumber}";

            if (address.Entrance.HasValue())
                addressDto.Street = $"{addressDto.Street} {address.Entrance}";

            if (address.ApartmentNumber.HasValue())
                addressDto.Street = $"{addressDto.Street} {address.ApartmentNumber}";

            if (address.ApartmentIdSuffix.HasValue())
                addressDto.Street = $"{addressDto.Street} {address.ApartmentIdSuffix}";

            if(!string.IsNullOrWhiteSpace(address.PostOfficeBox))
                addressDto.PostOfficeBox = $"PL {address.PostOfficeBox}";

            addressDto.ZipCode = address.PostCode ?? "";
            addressDto.City = address.PostOffices
                        .FirstOrDefault(p => p.LanguageCode == this.GetPRHLangCode())?.City
                        ?? (address.PostOffices
                        .FirstOrDefault(p => p.LanguageCode == this.GetPRHLangCode(true))?.City ?? "");

            return addressDto;
        }

        private string GetPRHLangCode(bool getDefault = false)
        {
            TermGroup_Languages sysLang = (TermGroup_Languages)this.GetLangId();
            string langCode = "1";
            if (getDefault)
                return langCode;

            if (sysLang == TermGroup_Languages.Finnish)
                langCode = "1";
            else if (sysLang == TermGroup_Languages.Swedish)
                langCode = "2";
            else if (sysLang == TermGroup_Languages.English)
                langCode = "3";

            return langCode;
        }
    }
}
