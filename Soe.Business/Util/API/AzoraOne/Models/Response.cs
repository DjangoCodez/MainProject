using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.API.AzoraOne.Models
{
    public class AOResponseWrapper<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsGetRequest { get; set; }
        public string RawResponse { get; set; }
        public AOResponse<T> Response { get; set; }
        public List<BulkData<T>> Responses { get; set; } 
        public AOResponse<List<AOErrorDetails>> Error { get; set; }
        public bool GetSucceeded()
        {
            return this.IsSuccess && 
                this.Response.Success && 
                (this.Response.Data != null);
        }
        public T GetValue()
        {
            return Response.Data;
        }
        public bool HasError(ConnectorError error)
        {
            return !IsSuccess &&
                Error.Data.Exists(e => e.Code == (int)error);
        }
        public bool HasOnlyError(ConnectorError error)
        {
            return !IsSuccess &&
                Error.Data.Count == 1 &&
                Error.Data.Exists(e => e.Code == (int)error);
        }
        public static AOResponseWrapper<T> Success(AOResponse<T> value, bool isGetRequest, string rawResponse) 
        {
            return new AOResponseWrapper<T>
            {
                IsSuccess = true,
                Response = value,
                RawResponse = rawResponse,
                IsGetRequest = isGetRequest
            };
        }
        public static AOResponseWrapper<T> Success(AOResponseSuccessBulk<T> value, bool isGetRequest, string rawResponse)
        {
            return new AOResponseWrapper<T>
            {
                IsSuccess = true,
                Responses = value.ObjectList,
                RawResponse = rawResponse,
                IsGetRequest = isGetRequest
            };
        }
        public static AOResponseWrapper<T> Failure(AOResponse<List<AOErrorDetails>> value, bool isGetRequest, string rawResponse)
        {
            return new AOResponseWrapper<T>
            {
                IsSuccess = false,
                Error = value,
                RawResponse = rawResponse,
                IsGetRequest = isGetRequest
            };
        }
    }
    public class AOResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public AOExtended Extended { get; set; }
        //public string Meta { get; set; }
        public string Origin { get; set; }
        public string Time { get; set; }
    }
    public class AOResponseSuccessBulk<T>
    {
        public List<BulkData<T>> ObjectList { get; set; }
        public string Time { get; set; }
    }
    public class BulkData<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string ObjectId { get; set; }
    }


    public class AOErrorDetails
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string Element { get; set; }
    }

    public static class AOResponseWrapperExtensions
    {
        public static ActionResult ToActionResult<T>(this AOResponseWrapper<T> response)
        {
            if (response.IsSuccess)
            {
                var result = new ActionResult();

                if (!response.IsGetRequest)
                {
                    result.ObjectsAffected = response?.Responses?.Count ?? 1;
                }

                return result;
            }
            else
            {
                var result = new ActionResult(false);
                result.StrDict = new Dictionary<int, string>();
                result.ObjectsAffected = 0;
                var errors = response.Error.Data;
                if (errors != null)
                {
                    foreach (var error in errors)
                    {
                        result.ErrorMessage += error.Message + "\n";
                        result.StrDict.Add(error.Code, error.Message);
                    }
                    result.ErrorMessage = result.ErrorMessage.Trim();
                } 
                else
                {
                    result.ErrorMessage = response.RawResponse;
                }
                return result;
            }
        }
    }
    public enum ConnectorError
    {
        // Company
        Company_IdIsMissing = 111401,
        Company_IdIsNotValid = 111402,
        Company_IdAlreadyExists = 111403,
        Company_NameIsNotValid = 111502,
        Company_OrgNrIsNotValid = 113102,
        Company_BGIsNotValid = 113202,
        Company_PGIsNotValid = 113212,
        Company_IbanIsNotValid = 113222,
        Company_VatNrIsNotValid = 113232,

        // File
        File_IdAlreadyExists = 211403,

        // Invoice
        Invoice_DescriptionInvalid = 311802,
        Invoice_OcrInvalid = 315312,

        // Supplier
        Supplier_CannotIdentifyMatchingSupplier = 611001,
        Supplier_OrgNrAndBGAndPGAndIbanAreMissing = 611101,
        Supplier_IdIsMissing = 611401,
        Supplier_IdIsNotValid = 611402,
        Supplier_IdAlreadyExists = 611403,
        Supplier_NameIsNotValid = 611502,
        Supplier_OrgNrIsNotValid = 613102,
        Supplier_BGNumberIsNotValid = 613202,
        Supplier_PGNumberIsNotValid = 613212,
        Supplier_IbanIsNotValid = 613222
    }
}
