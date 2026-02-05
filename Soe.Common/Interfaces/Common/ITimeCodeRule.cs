using SoftOne.Soe.Common.Attributes;
using System;

namespace SoftOne.Soe.Common.Interfaces.Common
{
    [TSInclude]
    public interface ITimeCodeRule
    {
        int Type { get; }
        int Value { get; }
        DateTime? Time { get; }
    }
}
