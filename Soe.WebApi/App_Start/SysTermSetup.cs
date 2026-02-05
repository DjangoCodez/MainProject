using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using System;
using System.Text;

namespace Soe.WebApi
{
    public class SysTermSetup
    {
        public static void CreateSysTermJson()
        {
            try
            {
                TermManager tm = new TermManager(null);

                string defaultPhysical = ConfigSettings.SOE_SERVER_DIR_DEFAULT_PHYSICAL;
                string termsPath = defaultPhysical + "\\terms\\";

                string sv_SE = tm.GetSysTermJson((int)TermGroup_Languages.Swedish);
                string fi_FI = tm.GetSysTermJson((int)TermGroup_Languages.Finnish);
                string en_US = tm.GetSysTermJson((int)TermGroup_Languages.English);
                string nb_NO = tm.GetSysTermJson((int)TermGroup_Languages.Norwegian);
                string da_DK = tm.GetSysTermJson((int)TermGroup_Languages.Danish);       
         
                System.IO.File.WriteAllText(termsPath + Constants.SYSLANGUAGE_LANGCODE_SWEDISH + ".json", sv_SE, Encoding.UTF8);
                System.IO.File.WriteAllText(termsPath + Constants.SYSLANGUAGE_LANGCODE_FINISH + ".json", fi_FI, Encoding.UTF8);
                System.IO.File.WriteAllText(termsPath + Constants.SYSLANGUAGE_LANGCODE_ENGLISH + ".json", en_US, Encoding.UTF8);
                System.IO.File.WriteAllText(termsPath + Constants.SYSLANGUAGE_LANGCODE_NORWEGIAN + ".json", nb_NO, Encoding.UTF8);
                System.IO.File.WriteAllText(termsPath + Constants.SYSLANGUAGE_LANGCODE_DANISH + ".json", da_DK, Encoding.UTF8);
            }
            catch (Exception)
            {
                //SysLogManager slm = new SysLogManager(null);
                //slm.AddSysLog(ex, log4net.Core.Level.Error, HttpContext.Current);
            }
        }
    }
}