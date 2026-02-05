using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public class EmployeeAccountNode
    {
        public int AccountId { get; private set; }
        public DateRangeDTO DateRange { get; private set; }
        public bool IsDefault { get; private set; }
        public bool IsMainAllocation { get; private set; }

        public List<EmployeeAccountNode> Children { get; private set; }

        public static EmployeeAccountNode Create(
            AccountDTO account,
            DateRangeDTO dateRange,
            bool isDefault,
            bool isMainAllocation,
            EmployeeAccountNode parent,
            params EmployeeAccountNode[] children
            )
        {
            if (account == null)
                throw new ArgumentException("Account cannot be null.", nameof(account));

            var employeeAccountNode = new EmployeeAccountNode
            {
                AccountId = account.AccountId,
                DateRange = dateRange,
                IsDefault = isDefault,
                IsMainAllocation = isMainAllocation,
                Children = children?.ToList(),
            };

            if (parent != null)
                employeeAccountNode.SetParent(parent);
            else if (!children.IsNullOrEmpty())
                employeeAccountNode.AddChildren(children);

            return employeeAccountNode;
        }

        public void SetParent(EmployeeAccountNode parent)
        {
            if (parent == null)
                return;
            if (parent.Children == null)
                parent.Children = new List<EmployeeAccountNode>();
            parent.Children.Add(this);
        }

        public void AddChildren(params EmployeeAccountNode[] children)
        {
            this.Children = children?.ToList() ?? new List<EmployeeAccountNode>();
        }
    }
}
