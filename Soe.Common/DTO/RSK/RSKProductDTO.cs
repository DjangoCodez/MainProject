using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.DTO.RSK
{
    /*"searchParams": {
    "outputSettings": {
      "include": [
        "baseInfo",
        "productGroup",
        "nameInfo",
        "manufacturer"
      ],
      "etimVersion": 1
    },
    "searchCriteria": {
	"activeStatus": "Active",
      "productGroupIdentifier": "10"
    }
  }*/
    public class RSKBatchRequestParams
    {
        public string Type { get; set; }

        public RSKBatchRequestOutputParams OutputSettings { get; set; }
        public RSKBatchRequestSearchParams SearchCriteria { get; set; }
    }

    public class RSKBatchRequestOutputParams
    {
        public string[] Include { get; set; }
        public int EtimVersion { get; set; }

    }

    public class RSKBatchRequestSearchParams
    {
        public string ActiveStatus { get; set; }
        public string ProductGroupIdentidifier { get; set; }

    }

    public class RSKBatchResponse
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public decimal ProcessedPercentage { get; set; }
        public int RecommendedRecheckAfterSeconds { get; set; }
        public RSKBatchResponseResult SearchResult { get; set; }
    }

    public class RSKBatchResponseMetaData
    {
        public int PageSize { get; set; }   
        public int PageNumber { get; set; } 
        public int TotalCount { get; set; }     
    }

    public class RSKBatchResponseResult
    {
        public RSKBatchResponseMetaData PaginationMetadata { get; set; }
        public List<RSKProductDTO> ProductItems { get; set; }
    }

    public class RSKProductDTO
    {
        public int Id { get; set; }
        public string Number { get; set; } 
        public Manufacturer Manufacturer { get; set; }
        public BaseInfo BaseInfo { get; set; }
        public NameInfo NameInfo { get; set; }
        public ProductGroup ProductGroup { get; set; }
        public List<Uri> Uris { get; set; }
        public List<ExtNumber> ExtNumbers { get; set; }

        public string GetExtendedInfo()
        {
            return (String.IsNullOrEmpty(ProductGroup.Name) ? "" : ProductGroup.Name) +
                (String.IsNullOrEmpty(NameInfo.ProductTypeName) ? "" : " | " + NameInfo.ProductTypeName) +
                (String.IsNullOrEmpty(NameInfo.NameSv) ? "" : " | " + NameInfo.NameSv) +
                (String.IsNullOrEmpty(NameInfo.Dimension) ? "" : " | " + NameInfo.Dimension) +
                (String.IsNullOrEmpty(NameInfo.Size) ? "" : " | " + NameInfo.Size) +
                (String.IsNullOrEmpty(NameInfo.Design) ? "" : " | " + NameInfo.Design);
        }
    }

    public class Manufacturer
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
    }

    public class BaseInfo
    {
        public DateTime? EndAt { get; set; }    
        public DateTime? ModifiedAt { get; set; }
        public string Unit { get; set; }
    }
 
    public class NameInfo
    {
        public string Design { get; set; } 
        public string NameSv { get; set; }
        public string Dimension { get; set; }
        public string Size { get; set; }
        public string ProductTypeName { get; set; }

    }

    public class ProductGroup
    {
        public string FullIdentifier { get; set; }
        public string Name { get; set; }
    }

    public class Uri
    {
        public string fullUrl { get; set; }
        public string shortUrl { get; set; }
        public string type { get; set; }
    }

    public class ExtNumber
    {
        public string TypeIdentifier { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
    }

    public class RSKProductGroupDTO
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string FullIdentifier { get; set; }
        public string Name { get; set; }
    }
}
