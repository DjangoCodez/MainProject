using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ImportDynamicDTO
    {
        public ImportOptionsDTO Options { get; set; }
        public List<ImportFieldDTO> Fields { get; set; }
    }

    [TSInclude]
    public class ImportOptionsDTO
    {
        public bool SkipFirstRow { get; set; } = true;
        public bool ImportNew { get; set; } = true;
        public bool UpdateExisting { get; set; } = true;
    }

    [TSInclude]
    public class ImportFieldDTO
    {
        public string Field { get; set; }
        public string Label { get; set; }
        public int Index { get; set; }
        public SettingDataType DataType { get; set; }
        public bool IsRequired { get; set; }
        public bool IsConfigured { get; set; }
        public bool HasDefaultValue;
        public string DefaultStringValue { get; set; }
        public bool? DefaultBoolValue { get; set; }
        public int? DefaultIntValue { get; set; }
        public decimal? DefaultDecimalValue { get; set; }
        public DateTime DefaultDateTimeValue { get; set; }
        public bool EnableValueMapping { get; set; }
        public List<SmallGenericType> AvailableValues { get; set; }
        public SmallGenericType DefaultGenericTypeValue { get; set; }
        public Dictionary<string, SmallGenericType> ValueMapping { get; set; }
    }

    [TSInclude]
    public class SupplierProductImportRawDTO
    {
        public string SupplierNumber { get; set; }
        public string SupplierProductNr { get; set; }
        public string SupplierProductName { get; set; }
        public string SupplierProductUnit { get; set; }
        public string SupplierProductCode { get; set; }
        public decimal SupplierProductPackSize { get; set; }
        public int SupplierProductLeadTime { get; set; }
        public string SalesProductNumber { get; set; }
        public decimal SupplierProductPricePrice { get; set; }
        public decimal SupplierProductPriceQuantity { get; set; }
        public DateTime SupplierProductPriceDate { get; set; }
        public DateTime SupplierProductPriceDateStop { get; set; }
        public string SupplierProductPriceCurrencyCode { get; set; }
    }

    [TSInclude]
    public class ImportDynamicResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ImportDynamicLogDTO> Logs { get; set; } = new List<ImportDynamicLogDTO>();
        public int TotalCount { get; set; }
        public int SkippedCount { get; set; }
        public int NewCount { get; set; }
        public int UpdateCount { get; set; }
        public void AddLog(int rowNr, LogType type, string message)
        {
            this.Logs.Add(new ImportDynamicLogDTO() { RowNr = rowNr, Type = (int)type, Message = message });
        }
    }

    public enum LogType
    {
        Info = 1,
        Warning = 2,
        Error = 3,
    }

    [TSInclude]
    public class ImportDynamicLogDTO
    {
        public int RowNr { get; set; }
        public int Type { get; set; }
        public string Message { get; set; }
    }

    [TSInclude]
    public class  ImportDynamicFileUploadDTO 
    {
        public string FileName { get; set; }
        public string FileContent { get; set; }

        [TSIgnore]
        public byte[] File 
        { 
            get 
            { 
                return Convert.FromBase64String(this.FileContent); 
            } 
        }
    }
}
