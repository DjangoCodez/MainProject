namespace SoftOne.Soe.Common.DTO
{
    

    /// <summary>
    /// Name is same as column name in AttestEmployeeDirective. Thats why first letter is lowercase
    /// </summary>
    public class UserCompanySettingDTO
    {
        public int ActorCompanyId { get; set; }
        public int SettingTypeId { get; set; }
        public int DataTypeId { get; set; }
        public string StrData { get; set;}
        public int? IntData { get; set; }
        public bool? BoolData { get; set; }
    }
}
