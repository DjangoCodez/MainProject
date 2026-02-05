
namespace SoftOne.Soe.Business.Interfaces
{
    interface IRssItem
    {
        string Title { get; set; }
        string Description { get; set; }
        string Link { get; set; }
        string PubDate { get; set; }
        string Author { get; set; }
    }
}
