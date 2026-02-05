using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using SO.Internal.Shared.Api.Blob.ExternalProducts;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Business.Core.BlobStorage;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.RSK;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace SoftOne.Soe.Business.Core.RSK
{
    public class RSKManager : ManagerBase
    {
        SettingManager sm;

        //private static readonly string testUrlPart = "https://test.rskdatabasen.se/infodocs/BILD/";
        //private static readonly string liveUrlPart = "https://rskdatabasen.se/infodocs/BILD/";

        public RSKManager(ParameterObject parameterObject) : base(parameterObject)
        {
            sm = new SettingManager(parameterObject);
        }

        #region Update Product Name

        /*public ActionResult UpdateSysProductName(int sysProductId, string productId, string productName, List<GenericType<int, string>> mappings)
        {
            ActionResult result = new ActionResult();

            var rskProduct = RSKConnector.GetPlumbingProductFromRSK(sm, productId);
            if (rskProduct == null || rskProduct.NameInfo == null)
                return new ActionResult(false);

            var extendedInfo = rskProduct.GetExtendedInfo();
            var endProductAt = rskProduct.BaseInfo.EndAt;
            var manufacturer = rskProduct.Manufacturer != null ? rskProduct.Manufacturer.Name : String.Empty;
            var productGroupIdentifier = rskProduct.ProductGroup != null ? rskProduct.ProductGroup.FullIdentifier : String.Empty;

            if (!String.IsNullOrEmpty(extendedInfo) || !String.IsNullOrEmpty(manufacturer))
            {
                using (SOESysEntities entities = new SOESysEntities())
                {
                    var sysProduct = (from p in entities.SysProduct
                                      where p.SysProductId == sysProductId
                                      select p).FirstOrDefault();

                    // Get mapping
                    var productGroupMapping = mappings.FirstOrDefault(m => m.Field2 == productGroupIdentifier);

                    if (extendedInfo == sysProduct.ExtendedInfo && manufacturer == sysProduct.Manufacturer && endProductAt == sysProduct.EndAt && (productGroupMapping == null || sysProduct.SysProductGroupId == productGroupMapping.Field1))
                    {
                        result.BooleanValue = false;
                        return result;
                    }

                    if (sysProduct != null)
                    {
                        sysProduct.ExtendedInfo = extendedInfo;
                        sysProduct.Manufacturer = manufacturer;
                        sysProduct.EndAt = endProductAt;

                        sysProduct.ModifiedBy = "RSKJob";
                        sysProduct.Modified = DateTime.Now;

                        if (productGroupMapping != null)
                            sysProduct.SysProductGroupId = productGroupMapping.Field1;

                        result = SaveChanges(entities);
                        if (result.Success)
                            result.BooleanValue = true;
                    }
                    else
                    {
                        result.BooleanValue = false;
                    }
                }
            }
            else
            {
                result.BooleanValue = false;
            }

            return result;
        }*/

        public ActionResult UpdateSysProductNameFromProductGroup(GenericType<int, int?, string> productGroupSmall, KeyVaultSettings vaultsettings, int? monthsToGoBack = null)
        {
            ActionResult result = new ActionResult();

            int noOfUpdated = 0;
            var rskProducts = RSKConnector.CreateGetProductsByProductGroupRequest(sm, vaultsettings, productGroupSmall.Field3);

            //Filter for update
            if (monthsToGoBack.HasValue && monthsToGoBack.Value > 0)
            {
                var rerunDateToCompare = DateTime.Today.AddMonths(-monthsToGoBack.Value);
                rskProducts = rskProducts.Where(p => p.BaseInfo.ModifiedAt == null || p.BaseInfo.ModifiedAt.Value > rerunDateToCompare).ToList();
            }

            if (rskProducts != null && rskProducts.Count > 0)
            {
                using (SOESysEntities entities = new SOESysEntities())
                {
                    foreach (var rskProduct in rskProducts)
                    {
                        if (rskProduct == null || rskProduct.NameInfo == null)
                            return new ActionResult(false);

                        var extendedInfo = rskProduct.GetExtendedInfo();
                        var endProductAt = rskProduct.BaseInfo.EndAt;
                        var manufacturer = rskProduct.Manufacturer != null ? rskProduct.Manufacturer.Name : String.Empty;
                        var productGroupIdentifier = rskProduct.ProductGroup != null ? rskProduct.ProductGroup.FullIdentifier : String.Empty;

                        // Get image
                        var imageFileName = String.Empty;
                        if (rskProduct.Uris != null)
                        {
                            var imageItem = rskProduct.Uris.FirstOrDefault(i => i.type == "BILD");
                            if (imageItem != null)
                                imageFileName = imageItem.shortUrl;
                        }

                        if (!String.IsNullOrEmpty(extendedInfo) || !String.IsNullOrEmpty(manufacturer))
                        {
                            // Match to product
                            var sysProduct = (from p in entities.SysProduct
                                              where p.ProductId == rskProduct.Number &&
                                              p.Type == (int)ExternalProductType.Plumbing &&
                                              p.SysCountryId == (int)TermGroup_Country.SE
                                              select p).FirstOrDefault();

                            if (sysProduct == null || (extendedInfo == sysProduct.ExtendedInfo && manufacturer == sysProduct.Manufacturer && endProductAt == sysProduct.EndAt && imageFileName == sysProduct.ImageFileName))
                                continue;

                            sysProduct.ExtendedInfo = extendedInfo;
                            sysProduct.Manufacturer = manufacturer;
                            sysProduct.EndAt = endProductAt;
                            sysProduct.ExternalId = rskProduct.Id;

                            if (rskProduct.ExtNumbers != null && String.IsNullOrEmpty(sysProduct.EAN))
                            {
                                var gtinItem = rskProduct.ExtNumbers.FirstOrDefault(i => i.TypeIdentifier == "GTIN");
                                if (gtinItem != null)
                                    sysProduct.EAN = gtinItem.Value;
                            }

                            if (imageFileName != sysProduct.ImageFileName)
                                sysProduct.ImageFileName = imageFileName;

                            /*bool downloadImage = false;
                            if (imageFileName != sysProduct.ImageFileName)
                            {
                                sysProduct.ImageFileName = imageFileName;
                                downloadImage = true;
                            }*/

                            sysProduct.ModifiedBy = "RSKJob";
                            sysProduct.Modified = DateTime.Now;

                            // Get group
                            var sysProductGroup = GetSysProductGroup(entities, productGroupSmall.Field1);
                            sysProduct.SysProductGroup = sysProductGroup;

                            // Download image
                            /*if (downloadImage)
                            {
                                var url = (RSKConnector.GetTestMode(sm) ? testUrlPart : liveUrlPart) + sysProduct.ImageFileName;
                                var data = GetImages(url);

                                if (data != null && data.Count > 0)
                                {
                                    EvoBlobStorageConnector.UpsertExternalProduct(data[0].Image, sysProduct.SysProductId, data[0].FileType, data[0].SizeType);
                                    if(data.Count > 1)
                                        EvoBlobStorageConnector.UpsertExternalProduct(data[1].Image, sysProduct.SysProductId, data[1].FileType, data[1].SizeType);
                                }
                            }*/

                            noOfUpdated++;
                        }
                    }

                    result = SaveChanges(entities);

                    if (!result.Success)
                        return result;
                }
            }
            else
            {
                Thread.Sleep(2000);
            }

            result.IntegerValue = rskProducts?.Count ?? 0;
            result.IntegerValue2 = noOfUpdated;

            return result;
        }

        #endregion

        #region UpdateCreate RSK Product Groups

        public ActionResult UpdateCreateSysProductGroups(ref int created, ref int updated, ref int notUpdated)
        {
            ActionResult result = new ActionResult();

            var vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
            var rskProductGroups = RSKConnector.GetPlumbingProductGroupsFromRSK(sm, vaultsettings);
            if (rskProductGroups == null)
                return new ActionResult(false);

            using (SOESysEntities entities = new SOESysEntities())
            {
                var sysProductGroups = (from p in entities.SysProductGroup
                                        where p.SysCountryId == (int)TermGroup_Country.SE &&
                                         p.Type == (int)ExternalProductType.Plumbing &&
                                        p.State == (int)SoeEntityState.Active
                                        select p).ToList();

                List<int> handledProductGroups = new List<int>();
                foreach (var group in rskProductGroups.Where(g => g.ParentId == null))
                {
                    if (handledProductGroups.Contains(group.Id))
                        continue;

                    var existingGroup = sysProductGroups.FirstOrDefault(g => g.ExternalId == group.Id);
                    if (existingGroup == null)
                    {
                        existingGroup = new SysProductGroup()
                        {
                            Identifier = group.FullIdentifier,
                            Name = group.Name,
                            SysCountryId = (int)TermGroup_Country.SE,
                            Type = (int)ExternalProductType.Plumbing,
                            ExternalId = group.Id,
                        };

                        SetCreatedPropertiesOnEntity(existingGroup);
                        entities.SysProductGroup.Add(existingGroup);

                        created++;
                    }
                    else
                    {
                        if (existingGroup.Identifier != group.FullIdentifier || existingGroup.Name != group.Name)
                        {
                            existingGroup.Identifier = group.FullIdentifier;
                            existingGroup.Name = group.Name;

                            updated++;
                        }
                        else
                        {
                            notUpdated++;
                        }

                        SetModifiedPropertiesOnEntity(existingGroup);
                    }

                    var children = rskProductGroups.Where(g => g.ParentId == group.Id).ToList();
                    if (children.Any())
                        handledProductGroups.AddRange(HandleSysProductGroupChildren(entities, existingGroup, sysProductGroups, rskProductGroups, children, ref created, ref updated, ref notUpdated));

                    handledProductGroups.Add(group.Id);
                }
                result = SaveChanges(entities);
            }

            return result;
        }

        public List<int> HandleSysProductGroupChildren(SOESysEntities entities, SysProductGroup parent, List<SysProductGroup> sysProductGroups, List<RSKProductGroupDTO> all, List<RSKProductGroupDTO> groupsToHandle, ref int created, ref int updated, ref int notUpdated)
        {
            List<int> handledProductGroups = new List<int>();
            foreach (var group in groupsToHandle)
            {
                if (handledProductGroups.Contains(group.Id))
                    continue;

                var existingGroup = sysProductGroups.FirstOrDefault(g => g.ExternalId == group.Id);
                if (existingGroup == null)
                {
                    existingGroup = new SysProductGroup()
                    {
                        Identifier = group.FullIdentifier,
                        Name = group.Name,
                        SysCountryId = (int)TermGroup_Country.SE,
                        Type = (int)ExternalProductType.Plumbing,
                        ExternalId = group.Id,

                        ParentSysProductGroup = parent,
                    };

                    SetCreatedPropertiesOnEntity(existingGroup);
                    entities.SysProductGroup.Add(existingGroup);

                    created++;
                }
                else
                {
                    if (existingGroup.Identifier != group.FullIdentifier || existingGroup.Name != group.Name)
                    {
                        existingGroup.Identifier = group.FullIdentifier;
                        existingGroup.Name = group.Name;

                        updated++;
                    }
                    else
                    {
                        notUpdated++;
                    }

                    SetModifiedPropertiesOnEntity(existingGroup);
                }

                var children = all.Where(g => g.ParentId == group.Id).ToList();
                if (children.Any())
                    handledProductGroups.AddRange(HandleSysProductGroupChildren(entities, existingGroup, sysProductGroups, all, children, ref created, ref updated, ref notUpdated));

                handledProductGroups.Add(group.Id);
            }

            return handledProductGroups;
        }

        public List<GenericType<int, int?, string>> GetSysProductGroupsSmall(ExternalProductType type)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from p in sysEntitiesReadOnly.SysProductGroup
                    where p.SysCountryId == (int)TermGroup_Country.SE &&
                        p.Type == (int)type &&
                    p.State == (int)SoeEntityState.Active
                    select new GenericType<int, int?, string>()
                    {
                        Field1 = p.SysProductGroupId,
                        Field2 = p.ParentSysProductGroupId,
                        Field3 = p.Identifier,
                    }).ToList();
        }

        public SysProductGroup GetSysProductGroup(SOESysEntities entities, int sysProductGroupId)
        {
            return (from p in entities.SysProductGroup
                    where p.SysProductGroupId == sysProductGroupId &&
                    p.State == (int)SoeEntityState.Active
                    select p).FirstOrDefault();
        }

        #endregion

        #region Images

        public List<RskImageResult> GetImages(string url)
        {
            List<RskImageResult> images = new List<RskImageResult>();
            var fullImage = GetImage(url);

            if (fullImage?.Image != null)
            {
                var thumb = CreateThumbNail(fullImage.Image, fullImage.FileType);
                images.Add(fullImage);
                images.Add(thumb);
            }

            return images;

        }
        public RskImageResult GetImage(string url)
        {
            url = url.ToLower();
            var imageTypeBasedOnFileNameInUrl = url.Contains(".png") ? FileType.Png : url.Contains(".jpg") ? FileType.Jpeg : url.Contains(".bmp") ? FileType.Bmp : FileType.Jpeg;

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = httpClient.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                byte[] image = response.Content.ReadAsByteArrayAsync().Result;
                return new RskImageResult()
                {
                    Image = ConvertImage(image),
                    SizeType = SizeType.Full,
                    FileType = FileType.Jpeg,
                    Url = url
                };
            }
            else
            {
                return new RskImageResult()
                {
                    Image = null,
                    FileType = FileType.Jpeg,
                    SizeType = SizeType.Unknown,
                    Url = url
                };
            }
        }

        private byte[] ConvertImage(byte[] image)
        {
            var jpeg = new GraphicsManager(null).ConvertToJpeg(image, 80, 2000);
            return image.Length > jpeg.Length ? jpeg : image;
        }

        private RskImageResult CreateThumbNail(byte[] image, FileType fileType)
        {
            GraphicsManager gm = new GraphicsManager(null);
            var result = gm.CompressImage(image, ImageFormatType.JPG, 100, 80);
            return new RskImageResult()
            {
                Image = result,
                FileType = FileType.Jpeg,
                Url = string.Empty,
                SizeType = SizeType.Thumb
            };
        }

        public class RskImageResult
        {
            public string Url { get; set; }
            public byte[] Image { get; set; }
            public FileType FileType { get; set; }
            public SizeType SizeType { get; internal set; }
        }

        #endregion
    }
}
