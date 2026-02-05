using System;
using System.IO;
using System.Web;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Util;
using System.Linq;

namespace SoftOne.Soe.Web.soe.billing.import.pricelist
{
    public partial class _default : PageBase
    {
        #region Variables

        private SysPriceListManager splm;

        public string Legend
        {
            get
            {
                var text = string.Empty;
                text = GetText(4163, "Importera prislista");
                return text;
            }
        }

        #endregion

        private static List<string> RemovedPriceLists()
        {
            return new List<string>()
            {
                "RexelFINetto",
                "StorelNetto",
                "AhlsellFINetto",
                "AhlsellFIPLNetto"
            };
        }

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Import_Pricelist;
            Form1.OnSubmit = "formSubmitted()";
            base.Page_Init(sender, e);
            Scripts.Add("/soe/billing/import/pricelist/pricelist.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            splm = new SysPriceListManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            string header = GetText(4163, "Importera prislista");
            Form1.SetTabHeaderText(1, header);
            PostOptionalParameterCheck(Form1, null, true);

            #endregion

            #region Actions

            if (Request.Form["action"] == "upload")
            {
                if (F["Provider"] == null)
                    Form1.MessageError = GetText(9041, "Du måste ange en grossist");
                else
                {
                    SoeCompPriceListProvider providerType = (SoeCompPriceListProvider)Enum.Parse(typeof(SoeCompPriceListProvider), F["Provider"]);
                    HttpPostedFile file = Request.Files["FileInput"];
                    if (file != null && file.ContentLength > 0)
                    {
                        HttpPostedFile file2 = Request.Files["FileInput2"];

                        ActionResult result = Import(file,file2, providerType);
                        if (result.Success)
                            Form1.MessageSuccess = result.ErrorMessage;
                        else
                            Form1.MessageError = result.ErrorMessage;
                    }
                    else
                        Form1.MessageWarning = GetText(1179, "Filen hittades inte");
                }
            }

            #region Populate
            var providers = Enum.GetNames(typeof(SoeCompPriceListProvider));
            providers = providers.Where(p => !RemovedPriceLists().Contains(p)).ToArray();
            Provider.DataSource = providers;
            Provider.DataBind();

            #endregion

            #endregion
        }

        #region Action-methods

        private ActionResult Import(HttpPostedFile file, HttpPostedFile file2, SoeCompPriceListProvider providerType)
        {
            //this.log.Info
            var result = new ActionResult();
            string fileName = file.FileName;
            Stream stream = null;
            try
            {
                if (file.FileName.Contains(@"\"))
                {
                    fileName = file.FileName.Substring(file.FileName.LastIndexOf(@"\"));
                    fileName = fileName.Replace("\\", "");
                }
                if (file.FileName.Contains(@"/"))
                {
                    fileName = file.FileName.Substring(file.FileName.LastIndexOf(@"/"));
                    fileName = fileName.Replace("//", "");
                }
            }
            catch (Exception ex)
            {
                SysLogManager.LogError<_default>(new SoeGeneralException("Prislisteimport: parse filename(" + file.FileName + ")", ex, this.ToString()));
                return new ActionResult(false, 0, "Kunde inte läsa sökväg till filen");
            }
            try
            {
                if (fileName.ToLower().Contains(".zip"))
                {
                    var path = ConfigSettings.SOE_SERVER_DIR_TEMP_PRICELIST_PHYSICAL;
                    if (ZipUtility.Unzip(file.InputStream, path))
                    {
                        path += fileName.Replace(".zip", GetFileExtension(providerType));
                        stream = File.Open(path, FileMode.Open);
                    }
                    else
                        return new ActionResult(false, 0, GetText(1952, "Kunde inte packa upp zipfil"));
                }
                else
                    stream = file.InputStream;
            }
            catch (Exception ex)
            {
                SysLogManager.LogError<_default>(new SoeGeneralException("Prislisteimport: zipfilsläsning", ex, this.ToString()));
                return new ActionResult(false, 0, GetText(1951, "Kunde inte öppna filen"));
            }

            //Lunda import, with files
            if ( (providerType == SoeCompPriceListProvider.LundaStyckNetto) || (providerType == SoeCompPriceListProvider.LundaBrutto))
            {
                return splm.ImportLunda(file.InputStream, file.FileName, file2.InputStream, file2.FileName, SoeCompany.ActorCompanyId);
            }
            else if (providerType == SoeCompPriceListProvider.RexelFINetto || 
                     providerType == SoeCompPriceListProvider.AhlsellFINetto || 
                     providerType == SoeCompPriceListProvider.AhlsellFIPLNetto ||
                     providerType == SoeCompPriceListProvider.SoneparFINetto ||
                     providerType == SoeCompPriceListProvider.OnninenFINettoS ||
                     providerType == SoeCompPriceListProvider.DahlFINetto ||
                     providerType == SoeCompPriceListProvider.OnninenFINettoLVI
                     )
            {                                
                return splm.ImportFINetto(file.InputStream, file2.InputStream, providerType, SoeCompany.ActorCompanyId);
            }
            else
            {
                return splm.Import(stream, providerType, SoeCompany.ActorCompanyId, fileName);
            }
            
        }

        #endregion

        #region Help methods
        private string GetFileExtension(SoeCompPriceListProvider providerType)
        {
            var ext = string.Empty;
            //switch (providerType)
            //{
                //case SoeCompPriceListProvider.SvanborgData:
                //    return ".csv";
                //Add comp pricelists here when they exist
            //}
            return ext;
        }
        #endregion
    }
}
