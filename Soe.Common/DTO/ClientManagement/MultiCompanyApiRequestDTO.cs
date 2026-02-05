using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO.ClientManagement
{
	[TSInclude]
	public class MultiCompanyApiRequestDTO
    {
        public ClientManagementResourceType Feature { get; set; }
        public object Inputs { get; set; }
        public List<object> TargetCompanies { get; set; }
    }

	[TSInclude]
	public class MultiCompanyApiResponseDTO
    {
        public List<object> Value { get; set; }
        public List<MultiCompanyErrorDTO> Errors { get; set; }
		public static MultiCompanyApiResponseDTO CreateError(string errorMessage)
		{
			return new MultiCompanyApiResponseDTO
			{
				Errors = new List<MultiCompanyErrorDTO>
				{
					new MultiCompanyErrorDTO
					{
						ErrorMessage = errorMessage
					}
				}
			};
		}

	}

    public class MCSupplierInvoicesFilterDTO
    {
        public TermGroup_ChangeStatusGridAllItemsSelection AllItemsSelection { get; set; }
        public bool LoadOpen { get; set; }
        public bool LoadClosed { get; set; }
    }
}
