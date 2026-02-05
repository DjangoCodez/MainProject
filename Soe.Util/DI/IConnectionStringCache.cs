namespace SoftOne.Soe.Util.DI
{
    public interface IConnectionStringCache
    {
        string GetConnectionString(string name);
    }
}
