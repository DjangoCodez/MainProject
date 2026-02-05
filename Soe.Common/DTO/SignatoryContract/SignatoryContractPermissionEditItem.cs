namespace SoftOne.Soe.Common.DTO.SignatoryContract
{
    using SoftOne.Soe.Common.Attributes;

    [TSInclude]
    public class SignatoryContractPermissionEditItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsSelected { get; set; }
    }
}
