namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface ITask
    {
        long? CreatedByTask { get; set; }
        long? ModifiedByTask { get; set; }
    }
}
