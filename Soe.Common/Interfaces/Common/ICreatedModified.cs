using System;

namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface ICreatedModified : ICreated, IModified
    {

    }
    public interface ICreatedModifiedNotNull : ICreatedNotNull, IModified
    {

    }
    public interface ICreated
    {
        DateTime? Created { get; set; }
        string CreatedBy { get; set; }
    }
    public interface ICreatedNotNull
    {
        DateTime Created { get; set; }
        string CreatedBy { get; set; }
    }
    public interface IModified
    {
        DateTime? Modified { get; set; }
        string ModifiedBy { get; set; }
    }
}
