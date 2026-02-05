using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        #region AnnualLeaveTransaction

        public static AnnualLeaveTransactionEditDTO ToEditDTO(this AnnualLeaveTransaction e)
        {
            if (e == null)
                return null;

            AnnualLeaveTransactionEditDTO dto = new AnnualLeaveTransactionEditDTO()
            {
                AnnualLeaveTransactionId = e.AnnualLeaveTransactionId,
                EmployeeId = e.EmployeeId,
                DateEarned = e.DateEarned,
                MinutesEarned = e.MinutesEarned,
                DateSpent = e.DateSpent,
                MinutesSpent = e.MinutesSpent,
                AccumulatedMinutes = e.AccumulatedMinutes,
                LevelEarned = e.LevelEarned,
                ManuallyAdded = e.ManuallyAdded,
                Type = (TermGroup_AnnualLeaveTransactionType)e.Type,
                DayBalance = e.DayBalance,
                MinuteBalance = e.MinuteBalance,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                EmployeeNrAndName = e.Employee != null ? e.Employee.EmployeeNrAndName : null,
                TypeColor = GetTypeColor(e.Type),
                ManuallyEarned = e.ManuallyEarned,
                ManuallySpent = e.ManuallySpent
            };

            return dto;
        }

        public static AnnualLeaveTransactionGridDTO ToGridDTO(this AnnualLeaveTransaction e)
        {
            if (e == null)
                return null;

            AnnualLeaveTransactionGridDTO dto = new AnnualLeaveTransactionGridDTO()
            {
                AnnualLeaveTransactionId = e.AnnualLeaveTransactionId,
                EmployeeId = e.EmployeeId,
                DateEarned = e.DateEarned,
                MinutesEarned = e.MinutesEarned,
                DateSpent = e.DateSpent,
                MinutesSpent = e.MinutesSpent,
                AccumulatedMinutes = e.AccumulatedMinutes,
                LevelEarned = e.LevelEarned,
                ManuallyAdded = e.ManuallyAdded,
                Type = (TermGroup_AnnualLeaveTransactionType)e.Type,
                TypeName = e.TypeName,
                DayBalance = e.DayBalance,
                MinuteBalance = e.MinuteBalance,
                EmployeeNrAndName = e.Employee != null ? e.Employee.EmployeeNrAndName : null,
                TypeColor = GetTypeColor(e.Type),
                ManuallyEarned = e.ManuallyEarned,
                ManuallySpent = e.ManuallySpent
            };

            return dto;
        }

        public static List<AnnualLeaveTransactionEditDTO> ToEditDTOs(this IEnumerable<AnnualLeaveTransaction> l)
        {
            var dtos = new List<AnnualLeaveTransactionEditDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToEditDTO());
                }
            }
            return dtos;
        }

        public static List<AnnualLeaveTransactionGridDTO> ToGridDTOs(this IEnumerable<AnnualLeaveTransaction> l, bool sort = false)
        {
            var dtos = new List<AnnualLeaveTransactionGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            if (sort)
                dtos = dtos.OrderBy(x => x.EmployeeNrAndName).ThenBy(x => x.DateEarned).ThenBy(x => x.DateSpent).ToList();

            return dtos;
        }

        #endregion

        #region Helpers

        private static string GetTypeColor(int type)
        {
            // TODO: Remove this temporary set of colors later
            string typeColor = "lightgrey";
            switch (type)
            {
                case 1:
                    // yearly balance
                    typeColor = "orchid";
                    break;
                case 2:
                    // manually earned
                    typeColor = "lightgreen";
                    break;
                case 3:
                    // manually spent
                    typeColor = "coral";
                    break;
                default:
                    // single calculated transaction
                    typeColor = "lightgrey";
                    break;
            }
            return typeColor;
        }

        #endregion
    }
}
