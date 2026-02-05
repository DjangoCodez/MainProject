using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class GrossProfitCodeCopyItem
    {
        public int GrossProfitCodeId { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal Period1 { get; set; }
        public decimal Period2 { get; set; }
        public decimal Period3 { get; set; }
        public decimal Period4 { get; set; }
        public decimal Period5 { get; set; }
        public decimal Period6 { get; set; }
        public decimal Period7 { get; set; }
        public decimal Period8 { get; set; }
        public decimal Period9 { get; set; }
        public decimal Period10 { get; set; }
        public decimal Period11 { get; set; }
        public decimal Period12 { get; set; }
        public int AccountDimId { get; set; }
        public int AccountId { get; set; }
        public int ActorCompanyId { get; set; }
        public int AccountYearId { get; set; }
    }
}
