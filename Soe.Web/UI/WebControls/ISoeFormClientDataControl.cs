
namespace SoftOne.Soe.Web.UI.WebControls
{
    public interface ISoeFormClientDataControl
    {
        object ClientData { get; set; }
        string ID { get; set; }
        void SetClientDataJson(string jsonString);
    }
}
