using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.RSK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SoftOne.Soe.Business.Core.BlobStorage;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Core.RSK.Tests
{
    [TestClass()]
    public class RSKManagerTests
    {
        [TestMethod()]
        public void GetImagesTest()
        {
            RSKManager rskManager = new RSKManager(null);
            var url = "https://test.rskdatabasen.se/infodocs/BILD/BILD_185_5510627.jpg";
            var data = rskManager.GetImages(url);

            File.WriteAllBytes("C:\\temp\\BILD_185_5510627_full.jpg", data[0].Image);
            File.WriteAllBytes("C:\\temp\\BILD_185_5510627_thumb.jpg", data[1].Image);

            var res1 =EvoBlobStorageConnector.UpsertExternalProduct(data[0].Image, 10, data[0].FileType, data[0].SizeType);
            var res2 = EvoBlobStorageConnector.UpsertExternalProduct(data[1].Image, 10, data[1].FileType, data[1].SizeType);

            var retrivedImageFull = EvoBlobStorageConnector.GetExternalProduct(10, data[0].FileType, data[0].SizeType);
            var retrivedImageThumb = EvoBlobStorageConnector.GetExternalProduct(10, data[1].FileType, data[1].SizeType);

            File.WriteAllBytes("C:\\temp\\BILD_185_5510627_full_retrived.jpg", retrivedImageFull.GetImage());
            File.WriteAllBytes("C:\\temp\\BILD_185_5510627_thumb_retrived.jpg", retrivedImageThumb.GetImage());

        }

        [TestMethod()]
        public void RunSysProductUpdatePerProductGroup()
        {
            var rskManager = new RSKManager(null);
            var vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
            rskManager.UpdateSysProductNameFromProductGroup(new Common.DTO.GenericType<int, int?, string>() { Field1 = 764, Field2 = 410, Field3 = "10141212" }, vaultsettings);
        }

        [TestMethod()]
        public void RunGetPlumbingProductGroupsFromRSK()
        {
            var rskManager = new RSKManager(null); 
            var sm = new SettingManager(null);
            var vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
            RSKConnector.GetPlumbingProductGroupsFromRSK(sm, vaultsettings);
        }
    }
}