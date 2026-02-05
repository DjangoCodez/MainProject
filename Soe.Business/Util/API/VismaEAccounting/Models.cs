using Newtonsoft.Json;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.API.VismaEAccounting
{
    class VismaTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
    class VismaSetMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalNumberOfPages { get; set; }
        public int TotalNumberOfResults { get; set; }
        public DateTime ServerTimeUtc { get; set; }

    }
    class VismaSetResponse<T>
    {
        public VismaSetMetadata Meta { get; set; }
        public List<T> Data { get; set; }
    }
    class VismaErrorResponse
    {
        public int ErrorCode { get; set; }
        public string DeveloperErrorMessage { get; set; }
        public string ErrorId { get; set; }
        public string UserErrorMessage
        {
            get
            {
                return $"Error message: {DeveloperErrorMessage} ({ErrorCode})";
            }
        }
    }
    class VismaResponseWrapper<T>
    {
        public bool Success
        {
            get
            {
                return this.Error == null && this.Data != null;
            }
        }
        public T Data { get; set; }
        public VismaErrorResponse Error { get; set; }
    }

    class VismaCustomer
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        public string CustomerNumber { get; set; }
        public string CorporateIdentityNumber { get; set; }
        public string InvoiceAddress1 { get; set; }
        public string InvoiceAddress2 { get; set; }
        public string InvoiceCity { get; set; }
        public string InvoiceCountryCode { get; set; }
        public string InvoicePostalCode { get; set; }
        public string DeliveryCustomerName { get; set; }
        public string DeliveryAddress1 { get; set; }
        public string DeliveryAddress2 { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryCountryCode { get; set; }
        public string DeliveryPostalCode { get; set; }
        public string Name { get; set; }
        public bool ReverseChargeOnConstructionServices { get; set; }
        public string TermsOfPaymentId { get; set; }
        //public TermsOfPayment TermsOfPayment { get; set; }
        public string VatNumber { get; set; }
        public bool IsPrivatePerson { get; set; }
        public string TaxDeductionNumber { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public int? PropertyType { get; set; }
        public string PropertyReference { get; set; }
        public bool IsActive { get; set; } = true; //Not synced
        public string EmailAddress { get; set; }
        public string Telephone { get; set; }
        public VismaCustomer() { }
        public VismaCustomer(CustomerDistributionDTO customer, CustomerInvoiceDistributionDTO invoice, CustomerDTO customerInput)
        {
            CustomerNumber = invoice?.ActorNr;
            Name = invoice?.ActorName;
            EmailAddress = customer.Email;
            Telephone = customer.MobilePhone;
            CorporateIdentityNumber = invoice?.ActorOrgNr;
            VatNumber = invoice?.ActorVatNr;
            IsPrivatePerson = customer.IsPrivatePerson;
            InvoiceAddress1 = customer.BillingAddressStreet;
            InvoiceAddress2 = customer.BillingAddressCO;
            InvoicePostalCode = customer.BillingAddressPostalCode;
            InvoiceCity = customer.BillingAddressCity;
            InvoiceCountryCode = customer.CountryCode;
            DeliveryAddress1 = customer.DeliveryAddressStreet;
            DeliveryAddress2 = customer.DeliveryAddressCO;
            DeliveryPostalCode = customer.DeliveryAddressPostalCode;
            DeliveryCity = customer.DeliveryAddressCity;
            ReverseChargeOnConstructionServices = invoice?.VatType == (int)TermGroup_InvoiceVatType.Contractor;

            if (customerInput != null)
            {
                CustomerNumber = customerInput.CustomerNr;
                Name = customerInput.Name;
                CorporateIdentityNumber = customerInput.OrgNr;
                VatNumber = customerInput.VatNr;
                IsPrivatePerson = customerInput.IsPrivatePerson;
                TermsOfPaymentId = customerInput.PaymentConditionId.ToString();
                ReverseChargeOnConstructionServices = customerInput.VatType == TermGroup_InvoiceVatType.Contractor;
            }
        }

        public bool Equals(VismaCustomer other)
        {
            return this.CustomerNumber == other.CustomerNumber &&
                 this.Name == other.Name &&
                 this.CorporateIdentityNumber == other.CorporateIdentityNumber &&
                 this.VatNumber == other.VatNumber &&
                 this.IsPrivatePerson == other.IsPrivatePerson &&
                 this.InvoiceAddress1 == other.InvoiceAddress1 &&
                 this.InvoiceAddress2 == other.InvoiceAddress2 &&
                 this.InvoicePostalCode == other.InvoicePostalCode &&
                 this.InvoiceCity == other.InvoiceCity &&
                 this.InvoiceCountryCode == other.InvoiceCountryCode &&
                 this.DeliveryAddress1 == other.DeliveryAddress1 &&
                 this.DeliveryAddress2 == other.DeliveryAddress2 &&
                 this.DeliveryPostalCode == other.DeliveryPostalCode &&
                 this.DeliveryCity == other.DeliveryCity &&
                 this.DeliveryCountryCode == other.DeliveryCountryCode &&
                 this.IsActive == other.IsActive &&
                 this.Telephone == other.Telephone &&
                 this.EmailAddress == other.EmailAddress &&
                 this.ReverseChargeOnConstructionServices == other.ReverseChargeOnConstructionServices;
        }
        public void SetId(VismaCustomer customer)
        {
            this.Id = customer.Id;
        }
        public void SetTermsOfPayment(VismaTermsOfPayment termsOfPayment)
        {
            this.TermsOfPaymentId = termsOfPayment.Id;
        }
    }

    class VismaCustomerInvoice
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int InvoiceNumber { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CustomerId { get; set; }
        public List<VismaCustomerInvoiceRow> Rows { get; set; }


        //Household deduction
        public List<VismaHouseholdDeductionPerson> Persons { get; set; }
        public int RotReducedInvoicingType { get; set; } //0 = Normal, 1 = Rot, 2 = Rut, green == NORMAL
        public string RotReducedInvoicingPropertyName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RotReducedInvoicingOrgNumber { get; set; } = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? RotPropertyType { get; set; } //0 = None, 1 = Apartment, 2 = Property
        public bool UsesGreenTechnology { get; set; }
        public decimal WorkHouseOtherCosts { get; set; }
        public decimal RotReducedInvoicingAmount
        {
            get
            {
                return Persons.Any() ?
                    decimal.Round(Persons.Sum(p => p.Amount), 2) : 0;
            }
        }

        //Information fields
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string InvoiceDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DueDate { get; set; }

        public string OurReference { get; set; }
        public string YourReference { get; set; }
        public string TermsOfPaymentId { get; set; }


        //Invoice address
        public string InvoiceAddress1 { get; set; }
        public string InvoiceAddress2 { get; set; }
        public string InvoiceCity { get; set; }
        public string InvoiceCountryCode { get; set; } //Max 2 characters
        public string InvoicePostalCode { get; set; }

        //Delivery address
        public string DeliveryAddress1 { get; set; }
        public string DeliveryAddress2 { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryCountryCode { get; set; } //Max 2 characters
        public string DeliveryPostalCode { get; set; }

        public bool EuThirdParty { get; } = false; //Not synced
        public VismaCustomerInvoice() { }
        public VismaCustomerInvoice(CustomerInvoiceDistributionDTO invoiceDto, CustomerDistributionDTO customerDto)
        {
            InvoiceDate = invoiceDto.InvoiceDate != null ?
                invoiceDto.InvoiceDate.Value.ToString("yyyy-MM-dd") : "";
            DueDate = invoiceDto.DueDate != null ?
                invoiceDto.DueDate.Value.ToString("yyyy-MM-dd") : "";
            OurReference = invoiceDto.ReferenceOur;
            YourReference = invoiceDto.ReferenceYour;
            InvoiceAddress1 = customerDto.BillingAddressStreet;
            InvoiceAddress2 = customerDto.BillingAddressCO;
            InvoicePostalCode = customerDto.BillingAddressPostalCode;
            InvoiceCity = customerDto.BillingAddressCity;
            InvoiceCountryCode = customerDto.CountryCode;
            DeliveryAddress1 = customerDto.DeliveryAddressStreet;
            DeliveryAddress2 = customerDto.DeliveryAddressCO;
            DeliveryPostalCode = customerDto.DeliveryAddressPostalCode;
            DeliveryCity = customerDto.DeliveryAddressCity;
            this.RotReducedInvoicingType = (int)VismaHouseholdDeductionType.Normal;
        }
        public void SetInvoiceRows(List<VismaCustomerInvoiceRow> rows)
        {
            this.Rows = rows;
        }
        public void SetTermsOfPayment(VismaTermsOfPayment termsOfPayment)
        {
            this.TermsOfPaymentId = termsOfPayment.Id;
        }
        public void SetCustomer(VismaCustomer customer)
        {
            this.CustomerId = customer.Id;
        }
        //Housework setters
        public void SetPersons(List<VismaHouseholdDeductionPerson> persons)
        {
            this.Persons = persons;
        }
        public void SetRotRutValues(VismaHouseholdDeductionType type, string propertyName, string orgNr)
        {
            this.RotReducedInvoicingType = (int)type;
            this.SetProperty(propertyName, orgNr);
        }
        public void SetGreenWorkValues(string propertyName, string orgNr)
        {
            this.RotReducedInvoicingType = (int)VismaHouseholdDeductionType.Normal;
            this.UsesGreenTechnology = true;
            this.SetProperty(propertyName, orgNr);
        }
        public void SetProperty(string propertyName, string orgNr)
        {
            this.RotReducedInvoicingPropertyName = propertyName;
            if (!string.IsNullOrEmpty(orgNr))
                this.RotReducedInvoicingOrgNumber = orgNr;
            this.RotPropertyType = string.IsNullOrEmpty(orgNr) ?
                (int)VismaHouseholdPropertyType.Property :
                (int)VismaHouseholdPropertyType.Apartment;
        }
        public void SetOtherCosts(List<CustomerInvoiceRowDistributionDTO> rows)
        {
            this.WorkHouseOtherCosts = rows
                .Sum(r => VismaUtility.IsOtherCosts(r) ? r.PurchaseCost : 0);
        }
    }
    class VismaCustomerInvoiceRow
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ArticleId { get; set; } //Null if text row
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; } //4 decimals
        public decimal Quantity { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? WorkCostType { get; set; }
        public bool IsVatFree { get; set; }
        public decimal? WorkHours { get; set; }
        public decimal? MaterialCosts { get; set; }
        public int GreenTechnologyType { get; set; }
        public VismaContributionMargin ContributionMargin { get; set; }
        public VismaCustomerInvoiceRow() { }
        public VismaCustomerInvoiceRow(CustomerInvoiceRowDistributionDTO rowDto, bool hasDeduction, bool isPriceIncludingVat)
        {
            if (rowDto.IsTextRow)
            {
                this.ArticleId = null;
                this.Text = rowDto.Text;
                return;
            }
            //var amount = rowDto.SumAmountCurrency / (rowDto.Quantity > 0 ? rowDto.Quantity : 1);

            this.Text = rowDto.Text;
            this.DiscountPercentage = decimal.Round(rowDto.DiscountPercent / 100, 4);
            this.IsVatFree = rowDto.VatRate == 0;

            if (isPriceIncludingVat)
                this.UnitPrice = this.CalculatePriceInclVat(rowDto.AmountCurrency, rowDto.VATAmountCurrency, this.DiscountPercentage);
            else
                this.UnitPrice = decimal.Round(rowDto.AmountCurrency, 2);

            this.Quantity = decimal.Round(rowDto.Quantity, 2);
            if (hasDeduction) this.SetHouseholdDeductionValues(rowDto);
            this.ContributionMargin = new VismaContributionMargin(rowDto.MarginalIncome, rowDto.MarginalIncomeRatio * 100);
        }
        public VismaCustomerInvoiceRow(string text)
        {
            this.ArticleId = null;
            this.Text = text;
        }
        public void SetArticle(VismaArticle article)
        {
            this.ArticleId = article.Id;
        }
        public void SetHouseholdDeductionValues(CustomerInvoiceRowDistributionDTO row)
        {
            var houseHoldDeductionType = row.Product.HouseholdDeductionType;
            if (VismaUtility.IsGreenWork(houseHoldDeductionType))
            {
                this.GreenTechnologyType = (int)VismaUtility.GetVismaGreenTechnologyType(houseHoldDeductionType);
                if (row.Product.IsPayrollProduct)
                {
                    this.WorkHours = decimal.Round(row.Quantity, 2);
                }
            }
            if (VismaUtility.IsHouseWork(houseHoldDeductionType))
            {
                if (row.Product.IsPayrollProduct)
                {
                    this.WorkHours = decimal.Round(row.Quantity, 2);
                    this.WorkCostType = (int)VismaUtility.GetVismaHouseWorkType(houseHoldDeductionType);
                }
            }
        }
        public decimal CalculatePriceInclVat(decimal amount, decimal vatAmount, decimal discountRate)
        {
            //Recalculate what the VAT amount would have been if the discount was not applied
            var factor = discountRate == 1 ? 0 : 1 / (1 - discountRate);
            var recalculatedVat = vatAmount * factor;

            //The amount already includes the discount rate.
            return decimal.Round(amount + recalculatedVat, 2);
        }
    }
    class VismaContributionMargin
    {
        public decimal? Amount { get; set; }
        public decimal? Percentage { get; set; }
        public VismaContributionMargin() { }
        public VismaContributionMargin(decimal amount, decimal percentage)
        {
            this.Amount = decimal.Round(amount, 2);
            this.Percentage = percentage;
        }
    }

    class VismaHouseholdDeductionPerson
    {
        public string Ssn { get; set; }
        public decimal Amount { get; set; }
        public VismaHouseholdDeductionPerson() { }
        public VismaHouseholdDeductionPerson(HouseholdTaxDeductionRowDTO row)
        {
            Ssn = row.SocialSecNr;
            Amount = decimal.Round(row.Amount, 2);
        }
    }
    class VismaArticle
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string CodingId { get; set; }
        public string UnitId { get; set; }
        public bool IsActive { get; set; } = true;

        public VismaArticle() { }
        public VismaArticle(CustomerInvoiceRowDistributionDTO row)
        {
            Number = row.Product.Number;
            Name = row.Product.Name;
        }

        public bool Equals(VismaArticle other)
        {
            return this.Number == other.Number &&
             this.Name == other.Name &&
             this.CodingId == other.CodingId &&
             this.UnitId == other.UnitId;
        }
        public void SetId(VismaArticle article)
        {
            this.Id = article.Id;
        }
        public void SetUnit(VismaUnit unit)
        {
            this.UnitId = unit.Id;
        }
        public void SetAccountCoding(VismaArticleAccountCoding accountCoding)
        {
            this.CodingId = accountCoding.Id;
        }
    }

    class VismaUnit
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Abbreviation { get; set; }
        public string AbbreviationEnglish { get; set; }
    }

    class VismaArticleAccountCoding
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameEnglish { get; set; }
        public string Type { get; set; }
        public string VatRate { get; set; }
        public decimal VatRatePercent { get; set; }
        public int? DomesticSalesSubjectToReversedConstructionVatAccountNumber { get; set; }
        public int? DomesticSalesSubjectToVatAccountNumber { get; set; }
        public int? DomesticSalesVatExemptAccountNumber { get; set; }
        public int? ForeignSalesSubjectToMossAccountNumber { get; set; }
        public int? ForeignSalesSubjectToThirdPartySalesAccountNumber { get; set; }
        public int? ForeignSalesSubjectToVatWithinEuAccountNumber { get; set; }
        public int? ForeignSalesVatExemptOutsideEuAccountNumber { get; set; }
        public int? ForeignSalesVatExemptWithinEuAccountNumber { get; set; }
        public int? DomesticSalesVatCodeExemptAccountNumber { get; set; }
    }

    class VismaTermsOfPayment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameEnglish { get; set; }
        public int NumberOfDays { get; set; }
        public bool AvailableForSales { get; set; }
        public bool AvailableForPurchase { get; set; }
        public int TermsOfPaymentTypeId { get; set; }
    }

    class VismaCompanySettings
    {
        public bool ShowPricesExclVatPC { get; set; }
    }


    enum VismaHouseholdDeductionType
    {
        Normal = 0,
        Rot = 1,
        Rut = 2
    }
    enum VismaHouseholdPropertyType
    {
        None = 0,
        Apartment = 1,
        Property = 2
    }
    enum VismaGreenTechnologyType
    {
        None = 0,
        SolarCellInstallation = 1,
        ElectricEnergyStorageInstallation = 2,
        ElectricVehicleChargingPostStation = 3,
    }
    enum VismaHouseWorkType
    {
        None = 0,
        RotConstructionWork = 1,
        RotElectricalWork = 2,
        RotGlassSheetMetalWork = 3,
        RotGroundWork = 4,
        RotBrickWork = 5,
        RotPaintDecorateWork = 6,
        RotPlumbWork = 7,
        RutCleanJobWork = 9,
        RutCareClothTextile = 10,
        RutCook = 11,
        RutSnowRemove = 12,
        RutGarden = 13,
        RutBabySitt = 14,
        RutOtherCare = 15,
        RutHouseWorkHelp = 17,
        RutRemovalServices = 18,
        RutITServices = 19,
        RotHeatPump = 20,
        RotHeatPump2 = 21,
        RutHomeAppliances = 22,
        RotSolarHeatingSystem = 23,
        RotWoodBoiler = 24,
        RotFuelBoiler = 25,
        RutLaundry = 26,
        RutFurnishing = 27,
        RutGoodsTransport = 28,
        RutHomeSupervision = 29,
    }
}
