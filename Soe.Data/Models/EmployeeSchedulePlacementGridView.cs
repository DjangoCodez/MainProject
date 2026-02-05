using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeSchedulePlacementGridView
    {
        public List<Employment> Employments { get; set; }
        public DateTime? EmploymentFromDate { get; set; }
        public DateTime? EmploymentToDate { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region EmployeeSchedulePlacement

        public static EmployeeSchedulePlacementGridViewDTO ToDTO(this EmployeeSchedulePlacementGridView e)
        {
            if (e == null)
                return null;

            EmployeeSchedulePlacementGridViewDTO dto = new EmployeeSchedulePlacementGridViewDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.EmployeeNr,
                EmployeeEndDate = e.EmployeeEndDate,
                EmployeeFirstName = e.EmployeeFirstName,
                EmployeeLastName = e.EmployeeLastName,
                EmployeePosition = e.EmployeePosition,
                EmployeeScheduleId = e.EmployeeScheduleId,
                EmployeeScheduleStartDate = e.EmployeeScheduleStartDate,
                EmployeeScheduleStopDate = e.EmployeeScheduleStopDate,
                EmployeeScheduleStartDayNumber = e.EmployeeScheduleStartDayNumber,
                IsPlaced = e.EmployeeScheduleId > 0,
                IsPreliminary = e.IsPreliminary,
                TimeScheduleTemplateHeadId = e.TimeScheduleTemplateHeadId,
                TimeScheduleTemplateHeadName = e.TimeScheduleTemplateHeadName,
                TimeScheduleTemplateHeadNoOfDays = e.TimeScheduleTemplateHeadNoOfDays,
                TemplateEmployeeId = e.TemplateEmployeeId,
                TemplateStartDate = e.TemplateStartDate,
            };

            //Extensions
            dto.Employments = e.Employments.ToDTOs(includeEmployeeGroup: true, includePayrollGroup: true).ToList();
            if (e.EmployeeScheduleStartDate.HasValue && e.EmployeeScheduleStopDate.HasValue)
                dto.SetCurrentEmploymentProperties(e.EmployeeScheduleStartDate.Value, e.EmployeeScheduleStopDate.Value, forward: false);
            else
                dto.SetCurrentEmploymentProperties(e.EmployeeScheduleStartDate);

            return dto;
        }

        public static IEnumerable<EmployeeSchedulePlacementGridViewDTO> ToDTOs(this IEnumerable<EmployeeSchedulePlacementGridView> l)
        {
            var dtos = new List<EmployeeSchedulePlacementGridViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static void SetEmployments(this List<EmployeeSchedulePlacementGridView> placementItems, List<Employment> employments)
        {
            if (placementItems.IsNullOrEmpty() || employments == null)
                return;

            placementItems.ForEach(placementItem => placementItem.SetEmployments(employments));
        }

        public static void SetEmployments(this EmployeeSchedulePlacementGridView placementItem, List<Employment> employments)
        {
            if (employments == null || placementItem == null)
                return;

            placementItem.Employments = employments.Where(i => i.EmployeeId == placementItem.EmployeeId).ToList();
            placementItem.EmploymentFromDate = placementItem.EmployeeScheduleStartDate;
            placementItem.EmploymentToDate = placementItem.EmployeeScheduleStopDate;
        }

        #endregion
    }
}
