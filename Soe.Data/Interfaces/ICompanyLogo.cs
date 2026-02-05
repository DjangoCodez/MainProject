namespace SoftOne.Soe.Data
{
    public interface ICompanyLogo
    {
        string Extension { get; set; }
        string FileName { get; }
        byte[] Logo { get; set; }
    }
}
