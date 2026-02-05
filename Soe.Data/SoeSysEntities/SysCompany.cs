namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysCompany")]
    public partial class SysCompany : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysCompany()
        {
            SysCompanySetting = new HashSet<SysCompanySetting>();
            SysEdiMessageHead = new HashSet<SysEdiMessageHead>();

            SysCompanyUniqueValues = new HashSet<SysCompanyUniqueValue>();
        }

        public int SysCompanyId { get; set; }

        public int SysCompDbId { get; set; }

        public Guid CompanyApiKey { get; set; }
        public Guid? CompanyGuid { get; set; } //Unique identifier and will never be changed in Comp tables.

        public int? ActorCompanyId { get; set; }

        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(128)]
        public string Number { get; set; }

        public int? LicenseId { get; set; }

        [StringLength(128)]
        public string LicenseNumber { get; set; }

        [StringLength(128)]
        public string LicenseName { get; set; }

        [StringLength(128)]
        public string VerifiedOrgNr { get; set; }


        public bool IsSOP { get; set; }
        public bool UsesBankIntegration { get; set; }

        public int State { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public virtual SysCompDb SysCompDb { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysCompanySetting> SysCompanySetting { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysEdiMessageHead> SysEdiMessageHead { get; set; }

        public virtual ICollection<SysCompanyBankAccount> SysCompanyBankAccounts { get; set; }

        public virtual ICollection<SysCompanyUniqueValue> SysCompanyUniqueValues { get; set; }
    }
}
