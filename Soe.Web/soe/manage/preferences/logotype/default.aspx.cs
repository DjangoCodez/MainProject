using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.Controls;
using SoftOne.Soe.Web.Util;

namespace SoftOne.Soe.Web.soe.manage.preferences.logotype
{
    public partial class Default : PageBase
    {
        #region Variables

        private SettingManager sm;
        private LogoManager lm;

        private HttpPostedFile file = null;
        private string fileExtension = string.Empty;
        private int defaultLogoId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Preferences_LogotypeSettings;
            base.Page_Init(sender, e);

            StyleSheets.Add("default.css");
        }

        private void SendImage()
        {
            var id = Request.QueryString["id"];
            int imageId;
            if (int.TryParse(id, out imageId))
            {
                var companyLogo = lm.GetCompanyLogo(imageId, SoeCompany.ActorCompanyId);
                if (companyLogo != null)
                {
                    Response.Clear();
                    Response.BinaryWrite(companyLogo.Logo);
                    Response.End();
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            sm = new SettingManager(ParameterObject);
            lm = new LogoManager(ParameterObject);
            
            List<CompanyLogo> companyLogos;

            if (!string.IsNullOrEmpty(F["defaultValue"]))
            {
                defaultLogoId = Convert.ToInt32(F["defaultValue"]);
                sm.UpdateInsertIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreCompanyLogo, defaultLogoId, UserId, SoeCompany.ActorCompanyId, 0);
            }
            else
                defaultLogoId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreCompanyLogo, UserId, SoeCompany.ActorCompanyId, 0);

            #endregion

            #region Actions

            if (Request.QueryString["action"] == "getimage")
            {
                SendImage();
                return;
            }

            if (Request.Form["action"] == "upload")
            {
                file = Request.Files["File"];
                if (file != null && file.ContentLength > 0)
                {
                    fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
                    if (IsValidType(fileExtension))
                        Save();
                    else
                        Form1.MessageError = GetText(4093, "Uppladdning misslyckades");
                }
                else
                {
                    companyLogos = lm.GetCompanyLogos(SoeCompany.ActorCompanyId);
                    foreach (CompanyLogo obj in companyLogos)
                    {
                        var remove = F["remove_" + obj.ImageId];
                        if (!string.IsNullOrEmpty(remove) && Convert.ToBoolean(remove))
                        {
                            //reset old value
                            if (defaultLogoId == obj.ImageId)
                                sm.UpdateInsertIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreCompanyLogo, 0, UserId, SoeCompany.ActorCompanyId, 0);

                            //remove image
                            lm.DeleteCompanyLogo(obj.ImageId, SoeCompany.ActorCompanyId);
                            Form1.MessageSuccess = GetText(4099, "Logotyp borttagen");
                        }
                    }
                }
            }

            #endregion

            #region Populate

            companyLogos = lm.GetCompanyLogos(SoeCompany.ActorCompanyId);
            if (companyLogos != null)
            {
                //Dummy CompanyLogo to be able to choose no default
                companyLogos.Insert(0, new CompanyLogo()
                {
                    ImageId = 0,
                });

                //var images = new List<byte[]>();

                HtmlTable table;
                HtmlTableRow tRow;
                HtmlTableCell tCell;
                RadioButton radio;
                CheckBoxEntry check;
                Text text;

                foreach (CompanyLogo companyLogo in companyLogos)
                {
                    table = new HtmlTable();

                    bool showLogo = true;
                    string selectionText = "";
                    
                    if (companyLogo.ImageId == 0)
                    {
                        selectionText = GetText(4081, "Använd ingen logotyp");
                        showLogo = false;
                    }
                    else
                    {
                        selectionText = GetText(4097, "Använd som standardlogotyp");
                    }

                    #region RadioButton default

                    tRow = new HtmlTableRow();
                    tCell = new HtmlTableCell();
                    radio = new RadioButton()
                    {
                        ID = companyLogo.ImageId.ToString(),
                        Text = selectionText,
                        GroupName = "defaultValue",
                        Checked = companyLogo.ImageId == defaultLogoId,
                    };
                    tCell.Controls.Add(radio);
                    tRow.Cells.Add(tCell);
                    table.Rows.Add(tRow);

                    #endregion

                    if (showLogo)
                    {
                        #region CheckBox remove

                        tRow = new HtmlTableRow();

                        tCell = new HtmlTableCell();
                        check = new CheckBoxEntry()
                        {
                            ID = "remove_" + companyLogo.ImageId,
                            Value = Boolean.FalseString,
                            DisableSettings = true,
                            HideLabel = true,
                            FitInTable = true,
                        };
                        tCell.Controls.Add(check);
                        tCell.Controls.Add(new LiteralControl(" "));
                        text = new Text()
                        {
                            TermID = 4098,
                            DefaultTerm = "Ta bort logotyp",
                            FitInTable = true,
                        };
                        tCell.Controls.Add(text);
                        tRow.Cells.Add(tCell);

                        table.Rows.Add(tRow);

                        #endregion

                        #region Image

                        tRow = new HtmlTableRow();

                        tCell = new HtmlTableCell();
                        var img = new HtmlImage();
                        //img.Attributes.Add("src", pathRelative);
                        string base64 = Convert.ToBase64String(companyLogo.Logo);
                        string src = $"data:image/jpeg;base64,{base64}";

                        img.Attributes.Add("src", src);
                        img.Style.Add("float", "left");
                        img.Attributes.Add("id", "logo_" + SoeCompany.ActorCompanyId);
                        img.Attributes.Add("alt", "logo");
                        tCell.Controls.Add(img);
                        tRow.Cells.Add(tCell);

                        table.Rows.Add(tRow);

                        #endregion
                    }

                    tCell = new HtmlTableCell();
                    tCell.Controls.Add(table);
                    tRow = new HtmlTableRow();
                    tRow.Cells.Add(tCell);
                    tableThumbnails.Rows.Add(tRow);

                    //images.Add(companyLogo.Logo);
                }
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(4094, "Logotyp sparad");
                else if (MessageFromSelf.StartsWith("NOTSAVED") )
                    Form1.MessageError = GetText(4095, "Logotyp kunde inte sparas:"+ MessageFromSelf);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            bool success = true;
            string errorMessage = "";

            if (file != null && !(file.ContentLength == 0))
            {
                try
                {
                    byte[] image = new byte[Convert.ToInt32(file.InputStream.Length)];
                    file.InputStream.Read(image, 0, Convert.ToInt32(file.InputStream.Length));
                    image = ConvertAndResizeImage(image, System.IO.Path.GetFileName(file.FileName));
                    if (image == null)
                    {
                        errorMessage = "ConvertAndResizeImage returned null";
                        SysLogManager.LogInfo("Save Logo: ConvertAndResizeImage returned null");
                        RedirectToSelf("NOTSAVED", true);
                    }

                    fileExtension = ".jpg"; //todo remove hardcoding
                    success = lm.SaveCompanyLogo(image, fileExtension, null, SoeCompany.ActorCompanyId).Success;
                    if (!success)
                    {
                        errorMessage = "SaveCompanyLogo failed";
                        SysLogManager.LogInfo("Save Logo: SaveCompanyLogo failed");
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = "ConvertAndResizeImage exception:" + ex.Message;
                    success = false;
                    SysLogManager.LogError<Default>(ex);
                }
            }
            else
            {
                errorMessage = "File is null or content length = 0";
                SysLogManager.LogInfo("Save Logo: file is null or content length = 0");
            }

            //save setting
            if (success)
            {
                var logos = lm.GetCompanyLogos(SoeCompany.ActorCompanyId);
                if (logos == null || logos.Count == 1)
                {
                    CompanyLogo defaultCompanyLogo = lm.GetDefaultCompanyLogo(SoeCompany.ActorCompanyId);
                    if (defaultCompanyLogo != null)
                        defaultLogoId = defaultCompanyLogo.ImageId;
                }

                if (!sm.UpdateInsertIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreCompanyLogo, defaultLogoId, UserId, SoeCompany.ActorCompanyId, 0).Success)
                {
                    success = false;
                    errorMessage = "UpdateInsertIntSetting failed";
                    SysLogManager.LogInfo("Save Logo: UpdateInsertIntSetting failed");
                }
                
            }

            if (success)
                RedirectToSelf("SAVED");
            else
                RedirectToSelf("NOTSAVED:"+ errorMessage, true);
        }

        private byte[] ConvertAndResizeImage(byte[] image, string fileName)
        {
            var newImage = ImageHandler.Resize(image, Constants.COMP_LOGO_MAX_HEIGHT_PX, Constants.COMP_LOGO_MAX_WIDTH_PX);
            return newImage;
        }

        #endregion

        #region Help-methods

        private bool IsValidType(string extension)
        {
            List<string> validExternsions = new List<string>() { ".bmp", ".jpg", ".tiff", ".png", ".wmf" };
            return validExternsions.Contains(extension);
        }
        
        #endregion
    }
}
