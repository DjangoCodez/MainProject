using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class AngularSpaHost : ControlBase
    {
        private static Dictionary<string, string> Hashes = null;
        private static object SyncRoot = new object();


        protected void Page_Init(object sender, EventArgs e)
        {
            PageBase.HasAngularSpaHost = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Hashes == null || Hashes.Count == 0)
            {
                lock (SyncRoot)
                {
                    try
                    {
                        if (Hashes == null || Hashes.Count == 0)
                        {
                            Hashes = new Dictionary<string, string>(); //NOSONAR
                            var spaFiles = Directory.GetFiles(Server.MapPath("~/angular/dist/spa/browser")).Select(x => x.Split('\\').Last()).ToArray();
                            Hashes.Add("polyfills", GetHash(spaFiles, "polyfills"));
                            Hashes.Add("main", GetHash(spaFiles, "main"));
                            Hashes.Add("styles", GetHash(spaFiles, "styles", "css"));
                        }
                    }
                    catch
                    {
                        // Intentionally ignored, safe to continue
                        // NOSONAR
                    }
                }
            }
        }

        private string GetHash(string[] files, string fileName, string extension = "js")
        {
            return files
                .First(x => x.StartsWith(fileName))
                .Replace(fileName, "")
                .Replace(extension, "");
        }

        public string PolyfillsHash { get { return Hashes["polyfills"]; } }
        public string MainHash { get { return Hashes["main"]; } }
        public string StylesHash { get { return Hashes["styles"]; } }
    }
}