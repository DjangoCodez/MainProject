using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Voucher
{
    public class VoucherHeadDTOCache
    {
        private readonly Dictionary<string, List<VoucherHeadDTO>> CachedDtos;

        public VoucherHeadDTOCache() {
            CachedDtos = new Dictionary<string, List<VoucherHeadDTO>>();
        }

        public List<VoucherHeadDTO> Get(VoucherManager vm, int actorCompanyId, DateTime from, DateTime to)
        {
            var voucherHeads = Get(actorCompanyId, from, to);
            if (voucherHeads == null)
            {
                voucherHeads = vm.GetVoucherHeadDTOs(actorCompanyId, from, to);
                Add(voucherHeads, actorCompanyId, from,to);
            }
            
            return voucherHeads;
        }

        private void Add(List<VoucherHeadDTO> data, int actorCompany, DateTime from, DateTime to)
        {
            var key = GetKey(actorCompany, from, to);
            CachedDtos[key] = data;
        }

        private List<VoucherHeadDTO> Get(int actorCompany, DateTime from, DateTime to)
        {
            var key = GetKey(actorCompany, from, to);
            List<VoucherHeadDTO> result;
            CachedDtos.TryGetValue(key, out result);
            return result;
        }

        private string GetKey(int actorCompany, DateTime from, DateTime to)
        {
            return $"{actorCompany}#{from}#{to}";
        }
    }
}
