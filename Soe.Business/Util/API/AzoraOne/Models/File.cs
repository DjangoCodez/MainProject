namespace SoftOne.Soe.Business.Util.API.AzoraOne.Models
{
    public class AOFile
    {
        public string FileID { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string VerificationSeries { get; set; }

        public bool IsNotWaiting()
        {
            return Status != "WAITING";
        }
    }
}
