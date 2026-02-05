using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class ImportSpecial : ImportExportManager
    {
        public ImportSpecial(ParameterObject parameterObject) : base(parameterObject) { }

        public List<dynamic> GetSpecialObjects(string importContent, Dictionary<string, string> special, byte[] content)
        {
            ICAFreq icaFreq = new ICAFreq(parameterObject);
            ScheduleTidTid tiltid = new ScheduleTidTid(parameterObject);
            ScheduleTimer scheduleTimer = new ScheduleTimer(parameterObject);
            IcaBudget icaBudget = new IcaBudget();

            if (special.Keys.Contains("icafreq"))
                return icaFreq.GetStaffingNeedsFrequencyIODTOs(importContent);

            if (special.Keys.Contains("tiltidschema"))
                return tiltid.GetTimeScheduleTemplateBlockIODTOs(importContent, isTemplate: true);

            if (special.Keys.Contains("tiltidschemap"))
                return tiltid.GetTimeScheduleTemplateBlockIODTOs(importContent, isTemplate: false);

            if (special.Keys.Contains("timerschedule"))
                return scheduleTimer.GetTimeScheduleTemplateBlockIODTOs(importContent, isTemplate: false);

            if (special.Keys.Contains("timertemplateschedule"))
                return scheduleTimer.GetTemplateTimeScheduleTemplateBlockIODTOs(importContent);

            if (special.Keys.Contains("icasalesbudget"))
                return icaBudget.GetBudgetHeadDTOs(content);

            return new List<dynamic>();

        }

        public string ApplySpecials(string importContent, byte[] content, SysImportHead importHead, Dictionary<string, string> special, int actorCompanyId, SysImportDefinition def, bool contentIsByteFromString, int importId, Encoding encoding, bool recursive, out bool returnSucess)
        {
            returnSucess = false;
            bool contentChanged = false;
            string returnstring = importContent;

            if (special.Keys.Contains("ansjö"))
            {
                var ansjo = new Ansjo();
                returnstring = ansjo.ApplyAnsjoSupplierInvoiceSpecialModification(importContent);
                contentChanged = true;
            }

            if (special.Keys.Contains("ICAkundfaktura"))
            {
                var icaCustomerInvoice = new IcaCustomerInvoice();
                returnstring = icaCustomerInvoice.ApplyIcaCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("ICAOnline"))
            {
                var icaonline = new IcaOnline();
                bool details = true;
                returnstring = icaonline.ApplyIcaOnlineModification(importContent, details);
                contentChanged = true;
            }

            if (special.Keys.Contains("ICAOnline Nodetails"))
            {
                var icaonline = new IcaOnline();
                bool details = false;
                returnstring = icaonline.ApplyIcaOnlineModification(importContent, details);
                contentChanged = true;
            }

            if (special.Keys.Contains("ICAOnline2"))
            {
                var ICAOnline2 = new ICAOnline2();
                bool details = true;
                returnstring = ICAOnline2.ApplyIcaOnline2(importContent, actorCompanyId, details);
                contentChanged = true;
            }
            if (special.Keys.Contains("ICAFakturan"))
            {
                var iCASupplierInvoice = new ICASupplierInvoice();
                returnstring = iCASupplierInvoice.ApplyICAFakturan(importContent);
                contentChanged = true;
            }

            if (special.Keys.Contains("ICABudget"))
            {
                var icaBudget = new IcaBudget();
                returnstring = icaBudget.ApplyBudget(importContent);
                contentChanged = true;
            }

            if (special.Keys.Contains("ICAFakturan2"))
            {
                var ICASupplierInvoice2 = new ICASupplierInvoice2();

                ImportExportManager iem = new ImportExportManager(parameterObject);
                AccountManager am = new AccountManager(parameterObject);
                var import = iem.GetImport(actorCompanyId, importId);
                Account account = null;
                if (import.Dim1AccountId.GetValueOrDefault() > 0)
                    account = am.GetAccount(actorCompanyId, import.Dim1AccountId.Value);
                if (account == null)
                {
                    SettingManager sm = new SettingManager(null);
                    int accSetting = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountInvoiceProductPurchase, base.UserId, actorCompanyId, 0);
                    account = am.GetAccount(actorCompanyId, accSetting);
                }
                returnstring = ICASupplierInvoice2.ApplyICAFakturan2(importContent, actorCompanyId, account);
                contentChanged = true;
            }

            if (special.Keys.Contains("ICAKunder2"))
            {
                var ICACustomer2 = new ICACustomer2();
                SettingManager sm = new SettingManager(parameterObject);
                AccountManager am = new AccountManager(parameterObject);
                int accSetting = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountInvoiceProductPurchase, base.UserId, actorCompanyId, 0);
                Account accountStdPurchase = am.GetAccount(actorCompanyId, accSetting);
                returnstring = ICACustomer2.ApplyICACustomer2(importContent, actorCompanyId, accountStdPurchase);
                contentChanged = true;
            }

            if (special.Keys.Contains("ICADepartmentMapping"))
            {
                returnstring = ICADepartmentMapping.ApplyICADepartmentMapping(importContent);
                contentChanged = true;
            }

            if (special.Keys.Contains("SOPvoucher"))
            {
                var sOPPP = new SOPPP(parameterObject);
                string voucher = string.Empty;
                voucher = sOPPP.ApplySOPVoucher(importContent, actorCompanyId);
                if (string.IsNullOrEmpty(voucher))
                {
                    returnstring = CheckXML(importContent, content, def, contentIsByteFromString, encoding);
                    returnstring = sOPPP.ApplySOPVoucher(returnstring, actorCompanyId);
                }
                else
                {
                    returnstring = voucher;
                }
                contentChanged = true;
            }

            if (special.Keys.Contains("CASKkundfakturor"))
            {
                var cask = new Cask();
                returnstring = cask.ApplyCaskCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

                       if (special.Keys.Contains("CASKkundfakturor"))
            {
                var cask = new Cask();
                returnstring = cask.ApplyCaskCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("PIRATkundfakturor"))
            {
                var pirateCustomerInvoice = new PirateCustomerInvoice();
                returnstring = pirateCustomerInvoice.ApplyPirateCustomerInvoiceSpecialModification(importContent, actorCompanyId, parameterObject);
                contentChanged = true;
            }

            if (special.Keys.Contains("PIRATartiklar"))
            {
                var pirateInvoiceProduct = new PirateInvoiceProduct();
                returnstring = pirateInvoiceProduct.ApplyPirateInvoiceProductSpecialModification(importContent, actorCompanyId, parameterObject);
                contentChanged = true;
            }

            if (special.Keys.Contains("PIRATkst"))
            {
                var PirateAccountCostPlace = new PirateAccountCostPlace();
                returnstring = PirateAccountCostPlace.ApplyPirateAccountCostPlaceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("PIRATprj"))
            {
                var PirateAccountProject = new PirateAccountProject();
                returnstring = PirateAccountProject.ApplyPirateAccountProjectSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("KsabKundfakturor"))
            {
                var ksab = new Ksab();
                returnstring = ksab.ApplyKsabCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("InvitePeopleKundfakturor"))
            {
                var invitePeople = new InvitePeople();
                returnstring = invitePeople.ApplyInvitePeopleCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("Spectrakundfakturor"))
            {
                var Spectra = new Spectra();
                returnstring = Spectra.ApplySpectraCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }
            if (special.Keys.Contains("LimeKundfakturor"))
            {
                var lime = new Lime();
                returnstring = lime.ApplyLimeCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }
            if (special.Keys.Contains("FastNet"))
            {
                var fastNet = new FastNet();
                returnstring = fastNet.ApplyFastNet(importContent);
                contentChanged = true;
            }

            if (special.Keys.Contains("Nalen"))
            {
                var nalen = new Nalen();
                returnstring = nalen.ApplyNalen(importContent);
                contentChanged = true;
            }

            if (special.Keys.Contains("KGMphOLD"))
            {
                var kGMphOLD = new KGMphOLD();
                returnstring = kGMphOLD.ApplyKGMphOLD(importContent);
                contentChanged = true;
            }

            if (special.Keys.Contains("KGMphNEW"))
            {
                var kGMphNEW = new KGMphNEW();
                returnstring = kGMphNEW.ApplyKGMphNEW(importContent);
                contentChanged = true;
            }

            if (special.Keys.Contains("HotSoft"))
            {
                var hotSoftVoucher = new HotSoftVoucher();
                returnstring = hotSoftVoucher.ApplyHotSoftVoucherSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("COOPcash"))
            {
                var coopCashVoucher = new CoopCashVoucher();
                returnstring = coopCashVoucher.ApplyCoopCashVoucherSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("COOPFakturan"))
            {
                var CoopSupplierInvoice = new CoopSupplierInvoice();
                returnstring = CoopSupplierInvoice.ApplyCoopFakturan(importContent);
                contentChanged = true;
            }

            if (special.Keys.Contains("COOPFakturan2442"))
            {
                var CoopSupplierInvoice = new CoopSupplierInvoice();
                string konto2442 = "2442";
                returnstring = CoopSupplierInvoice.ApplyCoopFakturan(importContent, konto2442);
                contentChanged = true;
            }

            if (special.Keys.Contains("COOPkundfaktura"))
            {
                var CoopCustomerInvoice = new CoopCustomerInvoice();
                returnstring = CoopCustomerInvoice.ApplyCoopCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("Momentum"))
            {
                var momentum = new Momentum();
                returnstring = momentum.ApplyMomentum(importContent);
                contentChanged = true;
            }

            if (special.Keys.Contains("SAPkundfakturor"))
            {
                var sAPCustomerinvoice = new SAPCustomerinvoice();
                returnstring = sAPCustomerinvoice.ApplySAPCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("Operakundfakturor"))
            {
                var operaCustomerinvoice = new OperaCustomerinvoice();
                returnstring = operaCustomerinvoice.ApplyOperaCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("ArosVoucher"))
            {
                var arosVoucher = new ArosVoucher();
                returnstring = arosVoucher.ArosVoucherSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("SOPkundreskontra"))
            {
                var sOPCustomerinvoice = new SOPCustomerinvoice();
                returnstring = sOPCustomerinvoice.ApplySOPCustomerInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("SAPlevfakturor"))
            {
                var sAPSupplierinvoice = new SAPSupplierinvoice();
                returnstring = sAPSupplierinvoice.ApplySAPSupplierInvoiceSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("SafiloVoucher"))
            {
                var safiloVoucher = new SafiloVoucher();
                returnstring = safiloVoucher.ApplySafiloVoucherSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("Hsb_orderrad"))
            {
                var hsbOrderRow = new HsbOrderRow();
                returnstring = hsbOrderRow.ApplyHsbOrderRowSpecialModification(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (special.Keys.Contains("SEBAkundfakturor"))
            {
                var SEBACustomerinvoice = new SEBACustomerinvoice();
                returnstring = SEBACustomerinvoice.ApplySEBACustomerInvoiceSpecialModification(importContent, actorCompanyId, parameterObject);
                contentChanged = true;
            }

            if (special.Keys.Contains("PeriodkonteringSOP")) //SOP pre process
            {
                var sOPPP = new SOPPP(parameterObject);
                returnstring = sOPPP.ApplyPeriodkonteringSOP(importContent, actorCompanyId);
                contentChanged = true;
            }

            if (contentChanged && !recursive)
            {
                bool checkOnContentIsByteFromString = true;

                if (special.Keys.Contains("Automaster"))
                    checkOnContentIsByteFromString = false;

                string checkedString = CheckXML(returnstring, content, def, contentIsByteFromString, encoding, checkOnContentIsByteFromString);

                if (!returnstring.Equals(checkedString))
                    returnstring = ApplySpecials(importContent, content, importHead, special, actorCompanyId, def, contentIsByteFromString, importId, encoding, true, out returnSucess);
            }

            if (special.Keys.Contains("BaseAccounts") && importHead.SysImportHeadType.SoeImportHeadTypeEnum == (int)TermGroup_IOImportHeadType.Settings)
            {
                var sOPPP = new SOPPP(parameterObject);
                using (CompEntities entities = new CompEntities())
                {
                    returnstring = CheckXML(importContent, content, def, contentIsByteFromString, encoding);
                    sOPPP.CreateBaseAccount(entities, returnstring, actorCompanyId);
                }

                returnSucess = true;

                return importContent;
            }

            #region //SOP pre process

            if (special.Keys.Contains("SOPPP")) //SOP pre process
            {
                var sOPPP = new SOPPP(parameterObject);
                returnstring = CheckXML(importContent, content, def, contentIsByteFromString, encoding);

                string elementName = string.Empty;
                string subElementName = string.Empty;
                if (importHead.SysImportHeadType.SoeImportHeadTypeEnum == (int)TermGroup_IOImportHeadType.Customer)
                {
                    elementName = "Kunder";
                    subElementName = "Kund";

                }
                if (importHead.SysImportHeadType.SoeImportHeadTypeEnum == (int)TermGroup_IOImportHeadType.Supplier)
                {
                    elementName = "Leverantörer";
                    subElementName = "Leverantör";
                }

                if (importHead.SysImportHeadType.SoeImportHeadTypeEnum == (int)TermGroup_IOImportHeadType.CustomerInvoice)
                {
                    if (importContent.Contains("Kundordrar"))
                    {
                        elementName = "Kundordrar";
                        subElementName = "Kundorder";
                    }
                    else
                    {
                        elementName = "Kundfakturor";
                        subElementName = "Kundfaktura";
                    }
                }

                if (elementName != string.Empty)
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        sOPPP.CreateCurrency(entities, returnstring, actorCompanyId, elementName, subElementName);
                        sOPPP.CreatePaymentConditions(entities, returnstring, actorCompanyId, elementName, subElementName);
                        sOPPP.CreateDeliveryConditions(entities, returnstring, actorCompanyId, elementName, subElementName);
                        sOPPP.CreateVatCodes(entities, returnstring, actorCompanyId, elementName, subElementName);
                        sOPPP.CreatePricelistTypes(entities, returnstring, actorCompanyId, elementName, subElementName);
                        sOPPP.CreateDeliveryTypes(entities, returnstring, actorCompanyId, elementName, subElementName);


                        if (importHead.SysImportHeadType.SoeImportHeadTypeEnum == (int)TermGroup_IOImportHeadType.Customer || importHead.SysImportHeadType.SoeImportHeadTypeEnum == (int)TermGroup_IOImportHeadType.CustomerInvoice)
                        {
                            sOPPP.CreateCustomerCategories(entities, returnstring, actorCompanyId, elementName, subElementName);
                        }

                        if (importHead.SysImportHeadType.SoeImportHeadTypeEnum == (int)TermGroup_IOImportHeadType.Supplier)
                        {
                            sOPPP.CreateSupplierCategories(entities, returnstring, actorCompanyId, elementName, subElementName);
                        }

                        entities.SaveChanges();

                    }
                }

                #endregion

            }

            return returnstring;
        }

        public string CheckXML(string importContent, byte[] content, SysImportDefinition def, bool contentIsByteFromString, Encoding encoding, bool checkOnContentIsByteFromString = true)
        {
            if (def.Type == (int)TermGroup_SysImportDefinitionType.XML)
            {
                DataSet dsImportXmlData = new DataSet();

                try
                {
                    MemoryStream stream = new System.IO.MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(importContent));
                    try
                    {
                        dsImportXmlData.ReadXml(stream);
                    }
                    catch
                    {
                        try
                        {
                            stream = new MemoryStream((new UTF8Encoding()).GetBytes(importContent));
                            dsImportXmlData.ReadXml(stream);
                        }
                        catch
                        {

                            if (contentIsByteFromString && checkOnContentIsByteFromString) // Not added, because encoding was overridden in Automaster files
                            {
                                MemoryStream newStream = new MemoryStream(content);
                                StreamReader sr = new StreamReader(newStream, encoding);
                                importContent = sr.ReadToEnd();
                                sr.Close();
                            }
                            else
                            {
                                importContent = System.Text.Encoding.UTF8.GetString(content);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex, "CheckXML");
                }
            }

            return importContent;
        }
    }
}
