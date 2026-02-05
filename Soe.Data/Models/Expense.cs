using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SoftOne.Soe.Data
{
    public partial class ExpenseHead : ICreatedModified, IState
    {

    }

    public partial class ExpenseRow : ICreatedModified, IState
    {

        public bool IsAccountingEmpty()
        {
            if (string.IsNullOrEmpty(this.Accounting))
                return true;

            // Split the string by the delimiter and check if all parts are empty
            var parts = this.Accounting.Split(';');
            return parts.All(string.IsNullOrEmpty);
        }
    }

    public static partial class EntityExtensions
    {
        #region ExpenseRow

        public static ExpenseRowGridDTO ToGridDTO(this ExpenseRow e)
        {
            if (e == null)
                return null;

            return new ExpenseRowGridDTO()
            {
                ExpenseRowId = e.ExpenseRowId,
                ExpenseHeadId = e.ExpenseHeadId,
                EmployeeId = e.Employee?.EmployeeId ?? 0,
                EmployeeName = e.Employee != null ? $"{e.Employee.EmployeeNr} - {e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                TimeCodeId = e.TimeCode?.TimeCodeId ?? 0,
                TimeCodeName = e.TimeCode != null ? $"{e.TimeCode.Code} - {e.TimeCode.Name}" : string.Empty,
                From = e.ExpenseHead?.Start ?? e.Start,
                Quantity = e.Quantity,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                InvoicedAmount = e.InvoicedAmount ?? 0,
                InvoicedAmountCurrency = e.InvoicedAmountCurrency ?? 0,
            };
        }

        public static IEnumerable<ExpenseRowGridDTO> ToGridDTOs(this IEnumerable<ExpenseRow> l)
        {
            var dtos = new List<ExpenseRowGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }

            }
            return dtos;
        }
        
        public static ExpenseRowDTO ToDTO(this ExpenseRow e)
        {
            if (e == null)
                return null;

            ExpenseRowDTO dto = new ExpenseRowDTO();
            var properties = dto.GetType().GetProperties();
            foreach (var property in properties)
            {
                PropertyInfo pi = e.GetType().GetProperty(property.Name);
                if (pi != null && pi.CanWrite)
                    property.SetValue(dto, pi.GetValue(e, null), null);
            }

            return dto;
        }

        public static IEnumerable<ExpenseRowDTO> ToDTOs(this IEnumerable<ExpenseRow> l)
        {
            var dtos = new List<ExpenseRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }

            }
            return dtos;
        }

        #endregion

        #region ExpenseRowTransactionView

        public static ExpenseRowDTO ToDTO(this ExpenseRowTransactionView e)
        {
            if (e == null)
                return null;

            ExpenseRowDTO dto = new ExpenseRowDTO()
            {
            };

            return dto;
        }

        public static IEnumerable<ExpenseRowDTO> ToDTOs(this IEnumerable<ExpenseRowTransactionView> l)
        {
            var dtos = new List<ExpenseRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }

            }
            return dtos;
        }

        #endregion
    }
}
