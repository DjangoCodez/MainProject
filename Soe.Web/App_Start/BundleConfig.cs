using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Soe.Web
{
    public static class SoftOneScripts
    {
        private static readonly List<SoftOneScriptBundle> _bundles = new List<SoftOneScriptBundle>();

        public static void AddBundle(SoftOneScriptBundle bundle)
        {
            _bundles.Add(bundle);
        }



        public static string RenderBundle(string name)
        {
            SoftOneScriptBundle bundle = _bundles.FirstOrDefault(x => x.Name == name);
            if (bundle == null)
                return "";


            string versionNr = AngularConfig.TermVersionNr;
            string prefix = AngularConfig.Prefix;
            if (AngularConfig.UseMinified)
            {
                if (string.IsNullOrEmpty(bundle.MinifiedName))
                    return "";

                GeneralManager gm = new GeneralManager(null);
                DateTime modifiedDate = gm.GetAssemblyDate();

                return string.Format("<script src=\"{0}{1}?v={2}_{3}\" type=\"module\"></script>\r\n", prefix, bundle.MinifiedName, versionNr, modifiedDate.ToString("yyyyMMdd_HHmmss"));
            }

            if (AngularConfig.UseBundle)
            {
                if (string.IsNullOrEmpty(bundle.BundleName))
                    return "";

                return string.Format("<script src=\"{0}{1}?v={2}\" type=\"module\"></script>\r\n", prefix, bundle.BundleName, versionNr);
            }

            StringBuilder sb = new StringBuilder();
            foreach (string file in bundle.Files.Where(f => !f.EndsWith("Module.js")))
            {
                sb.AppendFormat("<script src=\"{0}?v={1}\" type=\"module\"></script>\r\n", file, versionNr);
            }
            foreach (string file in bundle.Files.Where(f => f.EndsWith("Module.js")))
            {
                sb.AppendFormat("<script src=\"{0}?v={1}\" type=\"module\"></script>\r\n", file, versionNr);
            }
            return sb.ToString();
        }
    }

    public class SoftOneScriptBundle
    {
        public SoftOneScriptBundle(string name, string bundleName, string minifiedName)
        {
            Name = name;
            BundleName = bundleName;
            MinifiedName = minifiedName;
            Files = new List<string>();
        }

        public void Include(string str)
        {
            Files.Add(str);
        }

        public string Name { get; private set; }
        public string BundleName { get; private set; }
        public string MinifiedName { get; private set; }
        public List<string> Files { get; set; }
    }

    public class BundleConfig
    {
        static BundleConfig()
        {
#if DEBUG
            Path = @"\..\Soe.Angular\TypeScript\";
#endif

        }

        static string Path = AngularConfig.Prefix;

        public static void RegisterBundles()
        {
            AddSoftOneBundles();
        }

        private static void AddSoftOneBundles()
        {
            SoftOneScriptBundle bundle = new SoftOneScriptBundle("~/SoftOne/Libs", "softone.libs.bundle.js", "softone.libs.min.js");
            bundle.Include("/angular/node_modules/jquery/dist/jquery.js");
            bundle.Include("/angular/node_modules/jquery-ui-dist/jquery-ui.js");
            bundle.Include("/angular/node_modules/angular/angular.js");
            bundle.Include("/angular/node_modules/angular-ui-router/release/angular-ui-router.js");
            bundle.Include("/angular/node_modules/angular-ui-bootstrap/dist/ui-bootstrap.js");
            bundle.Include("/angular/node_modules/angular-ui-bootstrap/dist/ui-bootstrap-tpls.js");
            bundle.Include("/angular/node_modules/angular-ui-sortable/dist/sortable.js");
            //bundle.Include("/angular/node_modules/angular-ui/build/angular-ui.js");
            bundle.Include("/angular/node_modules/pdfmake/build/pdfmake.js");
            bundle.Include("/angular/node_modules/pdfmake/build/vfs_fonts.js");
            bundle.Include("/angular/js/ui-grid.js");
            bundle.Include("/angular/node_modules/@ag-grid-community/all-modules/dist/ag-grid-community.js");
            bundle.Include("/angular/node_modules/@ag-grid-enterprise/all-modules/dist/ag-grid-enterprise.js");
            bundle.Include("/angular/node_modules/ag-charts-community/dist/ag-charts-community.js");
            //bundle.Include("/angular/node_modules/pdfjs-dist/lib/shared/compatibility.js");
            bundle.Include("/angular/js/pdfjs-dist/build/pdf.js");
            bundle.Include("/angular/node_modules/angular-translate/dist/angular-translate.js");
            bundle.Include("/angular/node_modules/angular-translate/dist/angular-translate-loader-partial/angular-translate-loader-partial.js");
            bundle.Include("/angular/node_modules/angular-sanitize/angular-sanitize.js");
            bundle.Include("/angular/node_modules/lodash/lodash.js");
            bundle.Include("/angular/node_modules/moment/moment.js");
            bundle.Include("/angular/node_modules/moment-range/dist/moment-range.js");
            bundle.Include("/angular/node_modules/moment-timezone/builds/moment-timezone-with-data.js");
            bundle.Include("/angular/node_modules/angular-moment/angular-moment.js");
            bundle.Include("/angular/node_modules/postal/lib/postal.js");
            bundle.Include("/angular/node_modules/angular-ui-indeterminate/dist/indeterminate.js");
            bundle.Include("/angular/js/select.js");
            bundle.Include("/angular/js/angularjs-dropdown-multiselect.js");
            bundle.Include("/angular/js/store.min.js");
            bundle.Include("/angular/node_modules/angular-animate/angular-animate.js");
            bundle.Include("/angular/node_modules/angular-file-upload/dist/angular-file-upload.js");
            bundle.Include("/angular/node_modules/angular-google-maps/dist/angular-google-maps.js");
            bundle.Include("/angular/node_modules/angular-simple-logger/dist/angular-simple-logger.js");
            bundle.Include("/angular/node_modules/oclazyload/dist/ocLazyLoad.js");
            bundle.Include("/angular/node_modules/tinymce/tinymce.js");
            bundle.Include("/angular/node_modules/angular-ui-tinymce/src/tinymce.js");
            bundle.Include("/angular/node_modules/angular-bootstrap-contextmenu/contextMenu.js");
            bundle.Include("/angular/js/HelpMenuPlugin.js");
            bundle.Include("/angular/js/tinymce/fi_FI.js");
            bundle.Include("/angular/js/tinymce/sv_SE.js");
            bundle.Include("/angular/js/tinymce/nb_NO.js");
            bundle.Include("/angular/js/tinymce/en_US.js");
            bundle.Include("/angular/js/tinymce/da_DK.js");
            bundle.Include("/angular/node_modules/d3/d3.js");
            bundle.Include("/angular/node_modules/nvd3/build/nv.d3.js");
            bundle.Include("/angular/node_modules/angular-nvd3/dist/angular-nvd3.js");
            bundle.Include("/angular/node_modules/angularjs-gauge/dist/angularjs-gauge.js");
            bundle.Include("/angular/node_modules/bootstrap-3-typeahead/bootstrap3-typeahead.js");
            bundle.Include("/angular/node_modules/angular-minicolors/angular-minicolors.js");
            bundle.Include("/angular/node_modules/jquery-minicolors/jquery.minicolors.js");
            bundle.Include("/angular/node_modules/file-saver/FileSaver.min.js");
            bundle.Include("/angular/node_modules/xlsx/dist/xlsx.core.min.js");

            //bundle.Include("/angular/node_modules/xlsx/dist/xlsx.full.min.js");

            bundle.Include("/angular/node_modules/angular-hotkeys/build/hotkeys.js");

            SoftOneScripts.AddBundle(bundle);
            DirectoryInfo dir = new DirectoryInfo(HttpRuntime.AppDomainAppPath + Path);
            foreach (DirectoryInfo subDir in dir.EnumerateDirectories())
            {
                AddBundle(subDir.Name);
            }
        }

        private static void AddBundle(string bundleName, SoftOneScriptBundle bundle = null)
        {
            if (bundle == null)
                bundle = new SoftOneScriptBundle("~/SoftOne/" + bundleName, "softone." + bundleName.ToLower() + ".bundle.js", "softone." + bundleName.ToLower() + ".min.js");

            DirectoryInfo dir = new DirectoryInfo(HttpRuntime.AppDomainAppPath + Path + bundleName);
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                AddBundle(bundleName + "/" + subDir.Name, bundle);
            }

            var files = dir.EnumerateFiles("*.js");
            var modules = files.Where(f => f.Name.EndsWith("Module.js", StringComparison.OrdinalIgnoreCase));
            var apps = files.Where(f => f.Name.EndsWith("App.js", StringComparison.OrdinalIgnoreCase));
            var javascript = files.Where(f => !(f.Name.EndsWith("App.js", StringComparison.OrdinalIgnoreCase) || f.Name.EndsWith("Module.js", StringComparison.OrdinalIgnoreCase)));

            foreach (var file in javascript)
            {
                bundle.Include("/angular/TypeScript/" + bundleName + file.FullName.Replace(dir.FullName, "").Replace(@"\", "/"));
            }

            foreach (var file in modules)
            {
                bundle.Include("/angular/TypeScript/" + bundleName + file.FullName.Replace(dir.FullName, "").Replace(@"\", "/"));
            }

            foreach (var file in apps)
            {
                bundle.Include("/angular/TypeScript/" + bundleName + file.FullName.Replace(dir.FullName, "").Replace(@"\", "/"));
            }


            SoftOneScripts.AddBundle(bundle);
        }
    }
}