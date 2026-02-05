using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class AccountHierarchyInput
    {
        public Dictionary<AccountHierarchyParamType, bool> ParamValues { get; }

        public static List<AccountHierarchyParamType> DefaultParams
        {
            get
            {
                return new List<AccountHierarchyParamType>()
                {
                    AccountHierarchyParamType.OnlyDefaultAccounts
                };
            }
        }

        private AccountHierarchyInput(Dictionary<AccountHierarchyParamType, bool> paramValues)
        {
            this.ParamValues = paramValues ?? new Dictionary<AccountHierarchyParamType, bool>();
        }

        public static AccountHierarchyInput GetInstance(params AccountHierarchyParamType[] trueTypes)
        {
            Dictionary<AccountHierarchyParamType, bool> paramValuesDict = new Dictionary<AccountHierarchyParamType, bool>();
            foreach (AccountHierarchyParamType paramType in trueTypes.Concat(DefaultParams))
            {
                if (paramType != AccountHierarchyParamType.None && !paramValuesDict.ContainsKey(paramType))
                    paramValuesDict.Add(paramType, true);
            }
            return new AccountHierarchyInput(paramValuesDict);
        }

        public void AddParamValue(AccountHierarchyParamType paramType, bool value)
        {
            if (paramType == AccountHierarchyParamType.None)
                return;

            if (this.ParamValues.ContainsKey(paramType))
                this.ParamValues[paramType] = value;
            else
                this.ParamValues.Add(paramType, value);
        }

        /// <summary>
        /// Compare if both ParamValues are identical. 
        /// Exclude false properties.
        /// IncludeVirtualParented used in methods against a pre-built hierarchy.
        /// </summary>
        /// <returns></returns>
        public bool IsIdentical(AccountHierarchyInput input)
        {
            if (this.ParamValues == null || input == null)
                return false;

            var thisTrueKeys = this.ParamValues
                .Where(kv => kv.Value && kv.Key != AccountHierarchyParamType.IncludeVirtualParented)
                .Select(kv => kv.Key)
                .ToList();

            var inputTrueKeys = input.ParamValues
                .Where(kv => kv.Value && kv.Key != AccountHierarchyParamType.IncludeVirtualParented)
                .Select(kv => kv.Key)
                .ToList();

            if (thisTrueKeys.Count != inputTrueKeys.Count)
                return false;

            foreach (var key in thisTrueKeys)
            {
                if (!input.ParamValues.TryGetValue(key, out bool inputValue) || !inputValue)
                    return false;
            }

            foreach (var key in inputTrueKeys)
            {
                if (!this.ParamValues.TryGetValue(key, out bool thisValue) || !thisValue)
                    return false;
            }

            return true;
        }
    }
}
