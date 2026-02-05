using SoftOne.Soe.Common.Attributes;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO.ClientManagement
{
    public class MultiCompanyResponseDTO<TResult> where TResult : new()
    {
        public TResult Value { get; set; } = new TResult();
        public List<MultiCompanyErrorDTO> Errors { get; set; } = new List<MultiCompanyErrorDTO>();
	}

	[TSInclude]
	public class MultiCompanyErrorDTO
    {
        public int TargetActorCompanyId { get; set; }
        public string TargetCompanyName { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
    }
}
