using SoftOne.Soe.Business.Interfaces;

namespace SoftOne.Soe.Business.Util
{
    public class RssItem : IRssItem
    {
        public int RssItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string PubDate { get; set; }
        public string Author { get; set; }

        //SysNews properties only, not RSS standardformat
        public int SysNewsId { get; set; }
        public byte[] Attachment { get; set; }
        public string AttachmentFileName { get; set; }
        public string AttachmentImage { get; set; }
    }
}
