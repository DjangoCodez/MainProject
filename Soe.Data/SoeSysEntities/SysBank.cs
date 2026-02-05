namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysBank")]
    public partial class SysBank : SysEntity
    {
        public int SysBankId { get; set; }

        public int SysCountryId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(128)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string BIC { get; set; }

        public bool HasIntegration { get; set; }

        public virtual SysCountry SysCountry { get; set; }
        public virtual ICollection<SysCompanyBankAccount> SysCompanyBankAccounts { get; set; }
    }
}
