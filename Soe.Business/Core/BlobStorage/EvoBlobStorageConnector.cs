using SO.Internal.Shared.Api.Blob.ExternalProduct;
using SO.Internal.Shared.Api.Blob.ExternalProducts;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Util;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.BlobStorage
{
    public static class EvoBlobStorageConnector
    {
        private static string url
        {
            get
            {
                var uri = ConfigurationSetupUtil.GetEvoUrl().RemoveTrailingSlash();
                // uri = new Uri("https://localhost:7257/");
                return uri.ToString();
            }
        }

        public static string token
        {
            get { return ConnectorBase.GetAccessToken(); }
        }

        public static ExternalProductUpsertResult UpsertExternalProduct(byte[] file, int sysProductId, FileType fileType = FileType.Jpeg, SizeType sizeType = SizeType.Full)
        {

            ExternalProductUpsertRequest request = ExternalProductUpsertRequest.Create(sysProductId, sizeType, fileType, file);
            return UpsertExternalProduct(request);
        }

        public static ActionResult ExternalProductUpsertResults(List<ExternalProductUpsertRequest> externalProductUpsertRequests)
        {
            var response = ExternalProductClient.UpsertExternalProducts(url, token, externalProductUpsertRequests);

            if (response.Count == externalProductUpsertRequests.Count)
            {
                return new ActionResult(true);
            }
            else
            {
                return new ActionResult() { ErrorMessage = response.FirstOrDefault()?.Message ?? "", Success = false};
            }
        }

        public static ExternalProductUpsertResult UpsertExternalProduct(ExternalProductUpsertRequest externalProductUpsertRequest)
        {
            var response = ExternalProductClient.UpsertExternalProduct(url, token, externalProductUpsertRequest);
            return response;
        }

        public static byte[] GetFile(int sysProductId, FileType fileType = FileType.Jpeg, SizeType sizeType = SizeType.Full)
        {
            var externalProduct = GetExternalProduct(sysProductId, fileType, sizeType);
            return externalProduct?.GetImage();
        }

        public static ExternalProductResult GetExternalProduct(int sysProductId, FileType fileType = FileType.Jpeg, SizeType sizeType = SizeType.Full)
        {
            ExternalProductGetRequest request = new ExternalProductGetRequest()
            {
                SizeType = sizeType,
                FileType = fileType,
                SysProductId = sysProductId
            };

            var response = ExternalProductClient.GetExternalProduct(url, token, request);

            return response?.ExternalProductResults?.FirstOrDefault();
        }

        public static List<byte[]> GetFiles(List<int> sysProductIds, FileType fileType = FileType.Jpeg, SizeType sizeType = SizeType.Full)
        {
            List<byte[]> files = new List<byte[]>();
            var externalProducts = GetExternalProducts(sysProductIds, fileType, sizeType);
            if (externalProducts != null)
            {
                foreach (var externalProduct in externalProducts)
                {
                    files.Add(externalProduct.GetImage());
                }
            }
            return files;
        }

        public static List<ExternalProductResult> GetExternalProducts(List<int> sysProductIds, FileType fileType = FileType.Jpeg, SizeType sizeType = SizeType.Full)
        {
            ExternalProductsGetRequest request = new ExternalProductsGetRequest()
            {
                SizeType = sizeType,
                FileType = fileType,
                SysProductIds = sysProductIds
            };

            var response = ExternalProductClient.GetExternalProducts(url, token, request);
            return response?.ExternalProductResults;
        }
    }
}
