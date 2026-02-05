using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        #region ReportSelection

        #region ReportSelectionDate

        public static ReportSelectionDateDTO ToDTO(this ReportSelectionDate e)
        {
            if (e == null)
                return null;

            return new ReportSelectionDateDTO()
            {
                ReportSelectionDateId = e.ReportSelectionDateId,
                ReportSelectionId = e.ReportSelection?.ReportSelectionId ?? 0,// Add foreign key to model
                ReportSelectionType = (SoeSelectionData)e.ReportSelectionType,
                SelectFrom = e.SelectFrom,
                SelectTo = e.SelectTo,
                SelectGroup = e.SelectGroup,
                Order = e.Order
            };
        }

        public static IEnumerable<ReportSelectionDateDTO> ToDTOs(this IEnumerable<ReportSelectionDate> l)
        {
            var dtos = new List<ReportSelectionDateDTO>();
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

        #region ReportSelectionInt

        public static ReportSelectionIntDTO ToDTO(this ReportSelectionInt e)
        {
            if (e == null)
                return null;

            return new ReportSelectionIntDTO()
            {
                ReportSelectionIntId = e.ReportSelectionIntId,
                ReportSelectionId = e.ReportSelection?.ReportSelectionId ?? 0,// Add foreign key to model
                ReportSelectionType = (SoeSelectionData)e.ReportSelectionType,
                SelectFrom = e.SelectFrom,
                SelectTo = e.SelectTo,
                SelectGroup = e.SelectGroup,
                Order = e.Order
            };
        }

        public static IEnumerable<ReportSelectionIntDTO> ToDTOs(this IEnumerable<ReportSelectionInt> l)
        {
            var dtos = new List<ReportSelectionIntDTO>();
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

        #region ReportSelectionStr

        public static ReportSelectionStrDTO ToDTO(this ReportSelectionStr e)
        {
            if (e == null)
                return null;

            return new ReportSelectionStrDTO()
            {
                ReportSelectionStrId = e.ReportSelectionStrId,
                ReportSelectionId = e.ReportSelection?.ReportSelectionId ?? 0, // Add foreign key to model
                ReportSelectionType = (SoeSelectionData)e.ReportSelectionType,
                SelectFrom = e.SelectFrom,
                SelectTo = e.SelectTo,
                SelectGroup = e.SelectGroup,
                Order = e.Order
            };
        }

        public static IEnumerable<ReportSelectionStrDTO> ToDTOs(this IEnumerable<ReportSelectionStr> l)
        {
            var dtos = new List<ReportSelectionStrDTO>();
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

        #endregion
    }
}
