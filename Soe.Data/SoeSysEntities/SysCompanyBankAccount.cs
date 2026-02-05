namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysCompanyBankAccount")]
    public partial class SysCompanyBankAccount : SysEntity
    {
        public int SysCompanyBankAccountId { get; set; }
        public int SysCompanyId { get; set; }
        public int SysBankId { get; set; }
        public int AccountType { get; set; }

        [StringLength(128)]
        [Required(AllowEmptyStrings = false)]
        public string PaymentNr { get;set; }
        public DateTime Created { get; set; }
        
        [StringLength(50)]
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }
    
        public virtual SysBank SysBank { get; set; }
        public virtual SysCompany SysCompany { get; set; }
    }
}
