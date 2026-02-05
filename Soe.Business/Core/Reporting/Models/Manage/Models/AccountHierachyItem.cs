using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models
{
    public class AccountHierarchyItem
    {
        public AccountHierarchyItem()
        {
            ChildrenAccountHierarchyItems = new List<AccountHierarchyItem>();
        }

        public AccountAnalysisField AccountField { get; set; }
        public List<AccountHierarchyItem> ChildrenAccountHierarchyItems { get; set; }
        public Guid Guid { get; set; }
        public int RowNumber { get; set; }

        public List<AccountHierarchyItem> GetHierarchyItems(ref int rowNumber, List<int> validAccountDimIds = null)
        {
            rowNumber++;
            List<AccountHierarchyItem> items = new List<AccountHierarchyItem>();
            this.Guid = Guid.NewGuid();
            var itemsToAdd = GetHierarchyChildItems(this, ref rowNumber, validAccountDimIds);

            if (validAccountDimIds == null)
                items.AddRange(itemsToAdd);
            else
            {
                items.AddRange(itemsToAdd.Where(w => validAccountDimIds.Contains(w.AccountField.AccountDimId)));
            }
            return items;
        }

        private List<AccountHierarchyItem> GetHierarchyChildItems(AccountHierarchyItem item, ref int rowNumber, List<int> validAccountDimIds = null)
        {
            item.RowNumber = rowNumber;
            List<AccountHierarchyItem> items = new List<AccountHierarchyItem>() { item };
            if (item.ChildrenAccountHierarchyItems.Any())
            {
                int row = 1;
                foreach (var childItem in item.ChildrenAccountHierarchyItems)
                {
                    childItem.Guid = item.Guid;
                    if (row > 1)
                        rowNumber++;

                    childItem.RowNumber = rowNumber;
                    var clone = childItem.CloneDTO();
                    if (item.RowNumber != rowNumber)
                    {
                        var parentClone = item.CloneDTO();
                        parentClone.RowNumber = rowNumber;
                        if (validAccountDimIds == null || validAccountDimIds.Contains(parentClone.AccountField.AccountDimId))
                            items.Add(parentClone);
                    }
                    int inGoingRowNumber = rowNumber;
                    items.AddRange(childItem.GetHierarchyChildItems(clone, ref rowNumber, validAccountDimIds));

                    while (inGoingRowNumber <= rowNumber)
                    {
                        if (!items.Any(a => a.RowNumber == inGoingRowNumber && a.AccountField.AccountId == item.AccountField.AccountId))
                        {
                            var parentClone = item.CloneDTO();
                            parentClone.RowNumber = inGoingRowNumber;
                            if (validAccountDimIds == null || validAccountDimIds.Contains(parentClone.AccountField.AccountDimId))
                                items.Add(parentClone);
                        }

                        inGoingRowNumber++;
                    }

                    row++;
                }
                return items;
            }
            else
                return items;
        }
    }
}
