namespace SoftOne.Soe.Shared.DTO.ThirdParty.AzoraOne
{
    public class AzoraOneFileDTO
    {
        public string FileID { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public bool IsReady()
        {
            return Status == "READY";
        }
        public bool IsError()
        {
            return Status == "ERROR";
        }
    }
}
