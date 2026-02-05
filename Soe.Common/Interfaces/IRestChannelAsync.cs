using System;
using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract(Name = "IRestChannel")]
    public interface IRestChannelAsync
    {
        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetEniroCompanySearch(string searchWord, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.EniroDTO EndGetEniroCompanySearch(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetCustomers(int actorCompanyId, bool onlyActive, bool loadCategories, int roleId, int userId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.CustomerGridDTO[] EndGetCustomers(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetZippedFileAssemblies(int assemblyNamesEnum, string v, AsyncCallback callback, object asyncState);

        System.IO.Stream EndGetZippedFileAssemblies(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetHelpTextSearchResponse(int sysLangId, string searchString, SoftOne.Soe.Common.Util.SearchLevel level, AsyncCallback callback, object asyncState);

        System.Collections.Generic.List<SoftOne.Soe.Common.DTO.HelpTextDTOSearch> EndGetHelpTextSearchResponse(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginStatus(System.Guid guid, string key, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.SoftOneStatusDTO EndStatus(IAsyncResult result);
    }
}

