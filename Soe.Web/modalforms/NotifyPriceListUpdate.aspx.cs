using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class NotifyPriceListUpdate : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Parameters

            var wm = new WholeSellerManager(ParameterObject);

            int sysPriceListHeadId = 0;
            if(!string.IsNullOrEmpty(QS["sysPriceListHeadId"]))
                int.TryParse(QS["sysPriceListHeadId"], out sysPriceListHeadId);
            
            int sysWholeSellerId = 0;
            if(!string.IsNullOrEmpty(QS["sysWholeSellerId"]))
                int.TryParse(QS["sysWholeSellerId"], out sysWholeSellerId);
            
            //get actorcompanyid
            int actorCompanyId = SoeCompany.ActorCompanyId;
            if(!string.IsNullOrEmpty(QS["c"]))
                int.TryParse(QS["c"],out actorCompanyId);

            #endregion

            #region Content

            List<CompanyWholesellerPriceListViewDTO> priceListsToUpdate = wm.GetCompanyWholesellerPriceListsToUpdate(actorCompanyId);

            string action = Url;
            string headerText = string.Empty;
            string infoText = string.Empty;

            if (sysPriceListHeadId > 0 && sysWholeSellerId > 0)
            {
                action += "&upgrade=true";
                headerText = GetText(4233, "Uppgradera prislista");
                infoText = GetText(4230, "Detta kommer leda till att prissättningsregler tillhörande den gamla versionen används på den nya versionen och att pris på produkter kommer hämtas från den nya prislistan.");
            }
            else
            {
                action += "&upgradeAll=true";
                headerText = GetText(4227, "Nya prislistor finns tillgängliga");
                infoText = GetText(4228, "En eller flera av de prislistor som används finns tillgängliga i nyare version.");
                infoText += " " + GetText(4229, "Vill du använda de senaste prislistorna?");

                infoText += "</br></br>" + GetText(4230, "Detta kommer leda till att prissättningsregler tillhörande den gamla versionen används på den nya versionen och att pris på produkter kommer hämtas från den nya prislistan.");
                infoText += "</br></br>" + GetText(7543, "Glöm inte att uppdatera dina rabattbrev!");
                if (priceListsToUpdate.Count > 1)
                {
                    infoText += " ";
                    infoText += "</br>" + GetText(4586, "Välj grossist som du vill uppdatera.");
                }
            }

            ((ModalFormMaster)Master).HeaderText = headerText;
            ((ModalFormMaster)Master).InfoText = infoText;
            ((ModalFormMaster)Master).Action = action;

            #endregion

            #region Parse action

            bool upgrade = false;
            bool upgradeAll = false;

            if (!string.IsNullOrEmpty(QS["upgrade"]))
                upgrade = Convert.ToBoolean(QS["upgrade"]);

            if (!string.IsNullOrEmpty(QS["upgradeAll"]))
                upgradeAll = Convert.ToBoolean(QS["upgradeAll"]);

            if (upgrade)
            {
                ActionResult result = wm.UpgradeCompanyWholeSellerPriceLists(actorCompanyId, sysPriceListHeadId, sysWholeSellerId);
                Response.Redirect(Request.UrlReferrer.ToString());
            }
            else if (upgradeAll)
            {
                if (F.Count > 0)
                {
                    int wholeSellerId;
                    for (int i = 0; i < F.Count; i++)
                    {
                        Int32.TryParse(F[i], out wholeSellerId);
                        if (wholeSellerId > 0)
                        {
                            ActionResult result = wm.UpgradeCompanyWholeSellerPriceLists(actorCompanyId, null, wholeSellerId);
                        }
                    }

                }
                
                Response.Redirect(Request.UrlReferrer.ToString());
            }

            #endregion

            #region Render Table

            if (priceListsToUpdate.Count >= 1)
            {
                HtmlTableRow tRow;
                HtmlTableCell tCell;
                HtmlInputCheckBox checkBox;
                Text label;
                LiteralControl value;

                #region Header

                tRow = new HtmlTableRow();

                //Wholesellername
                tCell = new HtmlTableCell();
                label = new Text()
                {
                    //To do add term
                    TermID = 4585,
                    DefaultTerm = "Grossist",
                    FitInTable = true,
                };
                tCell.Controls.Add(label);
                tCell.Style["Padding"] = "5px";
                tRow.Cells.Add(tCell);

                TableUpdatePricelist.Rows.Add(tRow);

                #endregion

                foreach (var pricelist in priceListsToUpdate)
                {
                    #region Pricelist   

                    tRow = new HtmlTableRow();

                    //name
                    var text = pricelist.SysWholesellerId == (int)SoeWholeseller.Ahlsell ? pricelist.SysWholesellerName + " (" + ((SoeSysPriceListProvider)pricelist.Provider).ToString() + ")" : pricelist.SysWholesellerName;
                    value = new LiteralControl(text);
                    tCell = new HtmlTableCell();
                    tCell.Style["Padding"] = "5px";
                    tCell.Controls.Add(value);
                    tRow.Cells.Add(tCell);

                    checkBox = new HtmlInputCheckBox();
                    checkBox.ID = pricelist.SysWholesellerId.ToString();
                    checkBox.Value = pricelist.SysWholesellerId.ToString();
                    checkBox.Attributes.Add("Class", "Checkbox");
                    checkBox.Checked = true;
                    tCell = new HtmlTableCell();
                    tCell.Style["Padding"] = "5px";
                    tCell.Controls.Add(checkBox);
                    tRow.Cells.Add(tCell);

                    TableUpdatePricelist.Rows.Add(tRow);

                    #endregion
                }
            }
            else
            {
                TableUpdatePricelist.Visible = false;
            }

            #endregion
        }
    }
}