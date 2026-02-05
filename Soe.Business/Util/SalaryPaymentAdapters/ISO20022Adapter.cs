using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryPaymentAdapters
{
    //https://www.handelsbanken.com/en/our-services/digital-services/global-gateway/iso-20022-xml
    //Country specific
    //https://www.handelsbanken.com/tron/xgpu/info/contents/v1/document/72-111386
    //Credit transfer
    //https://www.handelsbanken.com/tron/xgpu/info/contents/v1/document/72-111387
    class ISO20022Adapter : SalaryPaymentBaseAdapter
    {
        #region Variables

        private decimal totalNetAmount = 0;
        private decimal paymentEntryTransactionsCount = 0;
        private readonly string currency; 
        readonly private Company company;
        readonly private string senderCustomerId;
        readonly private string divisionName;
        readonly private string senderCountryCode;
        readonly private DateTime debitDate;
        readonly string agreementNumber;
        readonly private PaymentInformationRowDTO paymentInformation;
        readonly private TermGroup_TimeSalaryPaymentExportBank paymentExportBank;
        readonly private bool useIBAN = false;
        readonly XNamespace ns = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";
        readonly XNamespace ns_xsi = "http://www.w3.org/2001/XMLSchema-instance";

        #endregion

        public ISO20022Adapter(List<Employee> employees, List<TimePayrollTransaction> transactions, TimePeriod timePeriod, Company company, PaymentInformationRowDTO paymentInformation, string senderCustomerId, string senderCountryCode, DateTime debitDate, string agreementNumber, decimal? currencyRate, string currency, bool useIBAN, string divisionName)
            : base(employees, transactions, timePeriod)
        {
            base.ExportFormat = SoeTimeSalaryPaymentExportFormat.XML;
            base.ExportType = TermGroup_TimeSalaryPaymentExportType.ISO20022;
            base.Extension = "xml";
            base.UniqueMsgKey = base.CreateUniqueId();
            base.UniquePaymentKey = base.CreateUniqueId();
            base.CurrencyRate = currencyRate;

            this.company = company;
            this.paymentInformation = paymentInformation;
            this.senderCustomerId = senderCustomerId.Trim().Replace("-", "");            
            this.divisionName = divisionName;
            this.senderCountryCode = senderCountryCode;                        
            this.debitDate = debitDate;
            this.agreementNumber = agreementNumber;
            this.currency = currency;
            this.useIBAN = useIBAN;

            paymentExportBank = this.paymentInformation.GetPaymentExportBank();
        }
        
        public override byte[] CreateFile()
        {
            XElement pmtInfElement = this.CreatePmtInfNode();
            //Must be called after pmtInfElement (includes sums)
            XElement grpHdrElement = this.CreateGrpHdrNode();

            XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "true"));

            //Document
            XElement rootElement = new XElement(ns + "Document", new XAttribute(XNamespace.Xmlns + "xsi", ns_xsi));            
            XElement cstmrCdtTrfInitnElement = new XElement(ns + "CstmrCdtTrfInitn");
            cstmrCdtTrfInitnElement.Add(grpHdrElement);            
            cstmrCdtTrfInitnElement.Add(pmtInfElement);            

            rootElement.Add(cstmrCdtTrfInitnElement);            
            doc.Add(rootElement);            

            MemoryStream stream = new MemoryStream();
            doc.Save(stream);
            return stream.ToArray();
        }

        private XElement CreateGrpHdrNode()
        {
            if (paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.Nordea || paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.BNP)
            {
                return new XElement(ns + "GrpHdr",
                        new XElement(ns + "MsgId", base.UniqueMsgKey),
                        new XElement(ns + "CreDtTm", this.GetISO20222ExportDate()),
                        new XElement(ns + "NbOfTxs", this.paymentEntryTransactionsCount),
                        new XElement(ns + "CtrlSum", this.FormatAmount(this.totalNetAmount)),
                        this.CreateInitgPtyNodeNordea());
            }
            else if (paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.DNB)
            {
                return new XElement(ns + "GrpHdr",
                        new XElement(ns + "MsgId", base.UniqueMsgKey),
                        new XElement(ns + "CreDtTm", this.GetISO20222ExportDate()),
                        new XElement(ns + "NbOfTxs", this.paymentEntryTransactionsCount),
                        new XElement(ns + "CtrlSum", this.FormatAmount(this.totalNetAmount)),
                        this.CreateInitgPtyNodeDNB());
            }
            else
            {
                return new XElement(ns + "GrpHdr",
                        new XElement(ns + "MsgId", base.UniqueMsgKey),
                        new XElement(ns + "CreDtTm", this.GetISO20222ExportDate()),
                        new XElement(ns + "NbOfTxs", this.paymentEntryTransactionsCount),
                        new XElement(ns + "CtrlSum", this.FormatAmount(this.totalNetAmount)),
                        this.CreateInitgPtyNode());
            }
        }

        private XElement CreateInitgPtyNode()
        { 
            return new XElement(ns + "InitgPty",
                        new XElement(ns + "Id",
                            new XElement(ns + "OrgId",
                                new XElement(ns + "Othr",
                                    new XElement(ns + "Id", this.senderCustomerId)))));          //orgnr eller kundnummer?  
        }
        private XElement CreateInitgPtyNodeNordea()
        {
            return new XElement(ns + "InitgPty",
                        new XElement(ns + "Id",
                            new XElement(ns + "OrgId",
                                new XElement(ns + "Othr",
                                    new XElement(ns + "Id", this.senderCustomerId),      //orgnr eller kundnummer?  
                                        new XElement(ns + "SchmeNm",
                                            new XElement(ns+ "Cd","CUST"))))));         
        }
        private XElement CreateInitgPtyNodeDNB()
        {
            return new XElement(ns + "InitgPty",
                        new XElement(ns + "Id",
                            new XElement(ns + "OrgId",
                                new XElement(ns + "Othr",
                                    new XElement(ns + "Id", senderCustomerId),      //orgnr eller kundnummer?  
                                        new XElement(ns + "SchmeNm",
                                            new XElement(ns + "Cd", "CUST"))),
                                new XElement(ns + "Othr",
                                    new XElement(ns + "Id", divisionName),    
                                        new XElement(ns + "SchmeNm",
                                            new XElement(ns + "Cd", "BANK"))))));
        }
        private XElement CreatePmtInfNode()
        {
            if (paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.Nordea || paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.BNP)
            {
                return new XElement(ns + "PmtInf",
                           new XElement(ns + "PmtInfId", base.UniquePaymentKey),
                           new XElement(ns + "PmtMtd", "TRF"),
                           this.CreatePmtTpInfNodeNordea(),
                           new XElement(ns + "ReqdExctnDt", this.GetExecutionDate()),
                           this.CreateDbtrNodeNordea(),
                           this.CreateDbtrAcctNodeNordea(),
                           this.CreateDbtrAgtNodeNordea(),
                           (paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.BNP && useIBAN) ? new XElement(ns + "ChrgBr", "SHAR") : null, 
                           this.CreateCdtTrfTxInfNode());
            }
            else if (paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.DNB)
            {
                return new XElement(ns + "PmtInf",
                    new XElement(ns + "PmtInfId", base.UniquePaymentKey),
                    new XElement(ns + "PmtMtd", "TRF"),
                    this.CreatePmtTpInfNodeNordea(),
                    new XElement(ns + "ReqdExctnDt", this.GetExecutionDate()),
                    this.CreateDbtrNodeDNB(),
                    this.CreateDbtrAcctNodeNordea(),
                    this.CreateDbtrAgtNodeNordea(),
                    this.CreateCdtTrfTxInfNode());
            }
            else
            {
                return new XElement(ns + "PmtInf",
                            new XElement(ns + "PmtInfId", base.UniquePaymentKey),
                            new XElement(ns + "PmtMtd", "TRF"),
                            this.CreatePmtTpInfNode(),
                            new XElement(ns + "ReqdExctnDt", this.GetExecutionDate()),
                            this.CreateDbtrNode(),
                            this.CreateDbtrAcctNode(),
                            this.CreateDbtrAgtNode(),
                            this.CreateCdtTrfTxInfNode());
            }
        }

        private XElement CreatePmtTpInfNode()
        {
            return new XElement(ns + "PmtTpInf",
                        new XElement(ns + "SvcLvl",
                            new XElement(ns + "Cd", "NURG")), //NURG = non urgent
                        new XElement(ns + "CtgyPurp",
                            new XElement(ns + "Cd", "SALA"))); //SALA = Salary Payment
        }
        private XElement CreatePmtTpInfNodeNordea()
        {
            return new XElement(ns + "PmtTpInf",
                        new XElement(ns+ "InstrPrty","NORM"), // NORM = Default Nordea
                        new XElement(ns + "SvcLvl",
                            new XElement(ns + "Cd", "NURG")), //NURG = non urgent
                        new XElement(ns + "CtgyPurp",
                            new XElement(ns + "Cd", "SALA"))); //SALA = Salary Payment, SUPP FOR NORDEA??
        }
        private XElement CreateDbtrNode()
        {
            return new XElement(ns + "Dbtr",
                    new XElement(ns + "Nm", this.company.Name),
                      new XElement(ns + "Id",
                          new XElement(ns + "OrgId",
                              new XElement(ns + "Othr",
                                  new XElement(ns + "Id", this.senderCustomerId)))), //orgnr eller kundnummer?  
                      new XElement(ns + "CtryOfRes", this.senderCountryCode));  //OBS, defaultar till "SE"
        }
        private XElement CreateDbtrNodeNordea()
        {
            return new XElement(ns + "Dbtr",
                    new XElement(ns + "Nm", this.company.Name),
                    new XElement(ns + "PstlAdr",
                        new XElement(ns + "Ctry", (TermGroup_Country)this.company.SysCountryId)),
                      new XElement(ns + "Id",
                          new XElement(ns + "OrgId",
                              new XElement(ns + "Othr",
                                  new XElement(ns + "Id", this.agreementNumber),  //orgnr eller kundnummer?  
                                  new XElement(ns + "SchmeNm",
                                            new XElement(ns + "Cd", "BANK"))))));     
                     
        }
        private XElement CreateDbtrNodeDNB()
        {
            return new XElement(ns + "Dbtr",
                    new XElement(ns + "Nm", this.company.Name),
                    new XElement(ns + "PstlAdr",
                        new XElement(ns + "Ctry", (TermGroup_Country)this.company.SysCountryId)));

        }

        private XElement CreateDbtrAcctNode()
        {
            //we only support TermGroup_SysPaymentType.BIC and TermGroup_SysPaymentType.Bank for the moment

            if (this.paymentInformation.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC) {
                return new XElement(ns + "DbtrAcct",
                           new XElement(ns + "Id",
                            new XElement(ns + "IBAN", this.GetIBAN_OR_BBAN_AccountNr())));
            }
            else 
            {
                //assume TermGroup_SysPaymentType.Bank -> BBAN
                return new XElement(ns + "DbtrAcct",
                                    new XElement(ns + "Id",
                                        new XElement(ns + "Othr",
                                            new XElement(ns + "Id", this.GetIBAN_OR_BBAN_AccountNr()),
                                            new XElement(ns + "SchmeNm",
                                                new XElement(ns + "Cd", "BBAN")))));
            }
        }
        private XElement CreateDbtrAcctNodeNordea()
        {
            //we only support TermGroup_SysPaymentType.BIC and TermGroup_SysPaymentType.Bank for the moment

            if (this.paymentInformation.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
            {
                return new XElement(ns + "DbtrAcct",
                           new XElement(ns + "Id",
                            new XElement(ns + "IBAN", this.GetIBAN_OR_BBAN_AccountNr())),
                           new XElement(ns + "Ccy", this.currency));
            }
            else
            {
                //assume TermGroup_SysPaymentType.Bank -> BBAN
                return new XElement(ns + "DbtrAcct",
                                    new XElement(ns + "Id",
                                        new XElement(ns + "Othr",
                                            new XElement(ns + "Id", this.GetIBAN_OR_BBAN_AccountNr()),
                                            new XElement(ns + "SchmeNm",
                                                new XElement(ns + "Cd", "BBAN")))));
            }
        }
        private XElement CreateDbtrAgtNode()
        {
            //we only support TermGroup_SysPaymentType.BIC and TermGroup_SysPaymentType.Bank for the moment
            
            if (this.paymentInformation.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
            {
                return new XElement(ns + "DbtrAgt",
                        new XElement(ns + "FinInstnId",
                            new XElement(ns + "BIC", this.GetBIC_OR_ClearingNr())));
            }
            else
            {
                //assume TermGroup_SysPaymentType.Bank -> MmbId is clearingnr

                return new XElement(ns + "DbtrAgt",
                        new XElement(ns + "FinInstnId",
                            new XElement(ns + "ClrSysMmbId",
                                new XElement(ns + "ClrSysId",
                                    new XElement(ns + "Cd", "SESBA")),  //Swedish bank, should be different if not a swedish bank
                                new XElement(ns + "MmbId", this.GetBIC_OR_ClearingNr()))));
            }
        }
        private XElement CreateDbtrAgtNodeNordea()
        {
            //we only support TermGroup_SysPaymentType.BIC and TermGroup_SysPaymentType.Bank for the moment

            if (this.paymentInformation.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
            {
                return new XElement(ns + "DbtrAgt",
                        new XElement(ns + "FinInstnId",
                            new XElement(ns + "BIC", this.GetBIC_OR_ClearingNr()),
                        new XElement(ns + "PstlAdr",
                            new XElement(ns + "Ctry", (TermGroup_Country)this.company.SysCountryId))));
            }
            else
            {
                //assume TermGroup_SysPaymentType.Bank -> MmbId is clearingnr

                return new XElement(ns + "DbtrAgt",
                        new XElement(ns + "FinInstnId",
                            new XElement(ns + "ClrSysMmbId",
                                new XElement(ns + "ClrSysId",
                                    new XElement(ns + "Cd", "SESBA")),  //Swedish bank, should be different if not a swedish bank
                                new XElement(ns + "MmbId", this.GetBIC_OR_ClearingNr()))));
            }
        }
        private List<XElement> CreateCdtTrfTxInfNode()
        {
            List<XElement> transactions = new List<XElement>();
            foreach (var employeeItem in base.EmployeeItems)
            {
                if (employeeItem.IsSE_CashDeposit)
                    continue;

                if (employeeItem.IsZeroNetAmount)
                    continue;

                paymentEntryTransactionsCount++;

                employeeItem.UniquePaymentRowKey = base.CreateUniqueId();
                var instrId = base.CreateUniqueId();

                #region NetAmount

                decimal netAmount = decimal.Round(employeeItem.GetNetAmount(base.CurrencyRate), 2, MidpointRounding.AwayFromZero); //Amount is already rounded to 2 decimals, but to bee sure...                
                totalNetAmount += netAmount;

                #endregion

                #region ClearingNr, AccountNr and IBAN

                string clearingNr = employeeItem.GetClearingNr();
                string accountNr = employeeItem.GetAccountNr();
                string bic = employeeItem.GetBIC()?.Trim();
                string iban = employeeItem.GetIBAN()?.Trim();
                string countryCode = employeeItem.GetCountryCode()?.Trim();

                #endregion

                XElement cdtTrfTxInfElement = new XElement(ns + "CdtTrfTxInf");
                XElement pmtIdElement = null;

                if (paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.DNB)
                {
                    pmtIdElement = new XElement(ns + "PmtId",
                        new XElement(ns + "InstrId", instrId),
                        new XElement(ns + "EndToEndId", employeeItem.UniquePaymentRowKey));
                }
                else
                {
                    pmtIdElement = new XElement(ns + "PmtId",
                                                new XElement(ns + "EndToEndId", employeeItem.UniquePaymentRowKey));
                }

                XElement amtElement =  new XElement(ns + "Amt",
                                            new XElement(ns + "InstdAmt", new XAttribute("Ccy", this.currency), this.FormatAmount(netAmount)));

                XElement cdtrElement;

                if (paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.Nordea || paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.DNB || paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.BNP)
                {
                    cdtrElement = new XElement(ns + "Cdtr",
                                                new XElement(ns + "Nm", employeeItem.Name.Trim()),
                                                new XElement(ns + "PstlAdr",
                                                    new XElement(ns + "Ctry", (TermGroup_Country)this.company.SysCountryId)));
                }
                else
                {

                    cdtrElement = new XElement(ns + "Cdtr",
                                                new XElement(ns + "Nm", employeeItem.Name.Trim()));

                }
                XElement cdtrAcctElement = new XElement(ns + "CdtrAcct");
                XElement cdtrAgtElement = new XElement(ns + "CdtrAgt");
                if (paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.BNP)
                {
                    cdtrAgtElement = new XElement(ns + "CdtrAgt",
                                            new XElement(ns + "FinInstnId",
                                                new XElement(ns + "BIC", bic),
                                                new XElement(ns + "PstlAdr",
                                                new XElement(ns + "Ctry", !countryCode.IsNullOrEmpty() ? countryCode : ((TermGroup_Country)company.SysCountryId).ToString()))));
                }

                if (useIBAN)
                {
                    if (paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.Nordea || paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.DNB || paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.BNP)
                    {
                        cdtrAcctElement.Add(new XElement(ns + "Id",
                                           new XElement(ns + "IBAN",iban)));
                    }
                    else
                    {

                        employeeItem.FormattedRecieverAccountNr = bic + "/" + iban;

                        cdtrAgtElement.Add(new XElement(ns + "FinInstnId",
                                             new XElement(ns + "BIC", bic)));

                        cdtrAcctElement.Add(new XElement(ns + "Id",
                                            new XElement(ns + "IBAN", employeeItem.FormattedRecieverAccountNr)));
                    }
                }
                else
                {

                    employeeItem.FormattedRecieverAccountNr = clearingNr + accountNr;
                    if (paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.Nordea || paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.DNB || paymentExportBank == TermGroup_TimeSalaryPaymentExportBank.BNP)
                    {
                        /*  LEAVE EMPTY IF DOMESTIC payments */
                        

                        cdtrAcctElement.Add(new XElement(ns + "Id",
                                                    new XElement(ns + "Othr",
                                                        new XElement(ns + "Id", employeeItem.FormattedRecieverAccountNr),
                                                        new XElement(ns + "SchmeNm",
                                                            new XElement(ns + "Cd", "BBAN")))));
                        
                    }
                    else
                    {
                        cdtrAgtElement.Add(new XElement(ns + "FinInstnId",
                                                   new XElement(ns + "ClrSysMmbId",
                                                       new XElement(ns + "ClrSysId",
                                                           new XElement(ns + "Cd", "SESBA")), //Swedish bank, should be different if not a swedish bank
                                                       new XElement(ns + "MmbId", clearingNr))));


                        cdtrAcctElement.Add(new XElement(ns + "Id",
                                                    new XElement(ns + "Othr",
                                                        new XElement(ns + "Id", employeeItem.FormattedRecieverAccountNr),
                                                        new XElement(ns + "SchmeNm",
                                                            new XElement(ns + "Cd", "BBAN")))));
                    }
                }

                cdtTrfTxInfElement.Add(pmtIdElement);                
                cdtTrfTxInfElement.Add(amtElement);
                if (paymentExportBank != TermGroup_TimeSalaryPaymentExportBank.Nordea && paymentExportBank != TermGroup_TimeSalaryPaymentExportBank.DNB)
                    cdtTrfTxInfElement.Add(cdtrAgtElement);     //NORDEA DOMESTIC LEAVE EMPTY

                cdtTrfTxInfElement.Add(cdtrElement);
                cdtTrfTxInfElement.Add(cdtrAcctElement);
                transactions.Add(cdtTrfTxInfElement);
            }
            
            return transactions;
        }


        #region Help methods

        private string GetExecutionDate()
        {
            return this.GetPaymentDateYYYY_MM_DD(debitDate);       
        }

        public string FormatAmount(decimal amount)
        {
            CultureInfo enUsCulture = CultureInfo.GetCultureInfo("en-US");
            return amount.ToString("##.00", enUsCulture);
        }

        private string GetIBAN_OR_BBAN_AccountNr()
        {
            return this.paymentInformation.GetIBANOrBBANForSalaryPayment();           
        }

        private string GetBIC_OR_ClearingNr()
        {
            return this.paymentInformation.GetBICOrClearingNrForSalaryPayment();            
        }

        #endregion

    }
}
