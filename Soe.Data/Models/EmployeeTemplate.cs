using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeTemplate : ICreatedModified, IState
    {

    }

    public partial class EmployeeTemplateGroup : ICreatedModified, IState
    {

    }

    public partial class EmployeeTemplateGroupRow : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region EmployeeTemplate

        public static EmployeeTemplateDTO ToDTO(this EmployeeTemplate e)
        {
            if (e == null)
                return null;

            EmployeeTemplateDTO dto = new EmployeeTemplateDTO()
            {
                EmployeeTemplateId = e.EmployeeTemplateId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeCollectiveAgreementId = e.EmployeeCollectiveAgreementId,
                Code = e.Code,
                ExternalCode = e.ExternalCode,
                Name = e.Name,
                Description = e.Description,
                Title = e.Title,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.EmployeeTemplateGroup != null)
                dto.EmployeeTemplateGroups = e.EmployeeTemplateGroup.Where(g => g.State == (int)SoeEntityState.Active).OrderBy(g => g.SortOrder).ToDTOs();

            return dto;
        }

        public static List<EmployeeTemplateDTO> ToDTOs(this IEnumerable<EmployeeTemplate> l)
        {
            var dtos = new List<EmployeeTemplateDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;
        }

        public static EmployeeTemplateGridDTO ToGridDTO(this EmployeeTemplate e)
        {
            if (e == null)
                return null;

            EmployeeTemplateGridDTO dto = new EmployeeTemplateGridDTO()
            {
                EmployeeTemplateId = e.EmployeeTemplateId,
                Code = e.Code,
                ExternalCode = e.ExternalCode,
                Name = e.Name,
                Description = e.Description,
                State = (SoeEntityState)e.State,
            };

            if (e.EmployeeCollectiveAgreement != null)
                dto.EmployeeCollectiveAgreementName = e.EmployeeCollectiveAgreement.Name;

            return dto;
        }

        public static List<EmployeeTemplateGridDTO> ToGridDTOs(this IEnumerable<EmployeeTemplate> l)
        {
            var dtos = new List<EmployeeTemplateGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region EmployeeTemplateGroup

        public static EmployeeTemplateGroupDTO ToDTO(this EmployeeTemplateGroup e)
        {
            if (e == null)
                return null;

            EmployeeTemplateGroupDTO dto = new EmployeeTemplateGroupDTO()
            {
                EmployeeTemplateGroupId = e.EmployeeTemplateGroupId,
                EmployeeTemplateId = e.EmployeeTemplateId,
                Type = (TermGroup_EmployeeTemplateGroupType)e.Type,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                SortOrder = e.SortOrder,
                NewPageBefore = e.NewPageBefore,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (e.EmployeeTemplateGroupRow != null)
                dto.EmployeeTemplateGroupRows = e.EmployeeTemplateGroupRow.Where(r => r.State == (int)SoeEntityState.Active).OrderBy(r => r.Row).ThenBy(r => r.StartColumn).ToDTOs();

            return dto;
        }

        public static List<EmployeeTemplateGroupDTO> ToDTOs(this IEnumerable<EmployeeTemplateGroup> l)
        {
            var dtos = new List<EmployeeTemplateGroupDTO>();
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

        #region EmployeeTemplateGroupRow

        public static EmployeeTemplateGroupRowDTO ToDTO(this EmployeeTemplateGroupRow e)
        {
            if (e == null)
                return null;

            EmployeeTemplateGroupRowDTO dto = new EmployeeTemplateGroupRowDTO()
            {
                EmployeeTemplateGroupRowId = e.EmployeeTemplateGroupRowId,
                EmployeeTemplateGroupId = e.EmployeeTemplateGroupId,
                Type = (TermGroup_EmployeeTemplateGroupRowType)e.Type,
                MandatoryLevel = e.MandatoryLevel,
                RegistrationLevel = e.RegistrationLevel,
                Title = e.Title,
                DefaultValue = e.DefaultValue,
                Comment = e.Comment,
                Row = e.Row,
                StartColumn = e.StartColumn,
                SpanColumns = e.SpanColumns,
                Format = e.Format,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                HideInReport = e.HideInReport,
                HideInReportIfEmpty = e.HideInReportIfEmpty,
                HideInRegistration = e.HideInRegistration,
                HideInEmploymentRegistration = e.HideInEmploymentRegistration,
                Entity = (SoeEntityType?)e.Entity,
                RecordId = e.RecordId
            };

            return dto;
        }

        public static List<EmployeeTemplateGroupRowDTO> ToDTOs(this IEnumerable<EmployeeTemplateGroupRow> l)
        {
            var dtos = new List<EmployeeTemplateGroupRowDTO>();
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

        #region EmployeeTemplateEdit

        public static XElement ToXElement(this EmployeeTemplateEditDTO employeeTemplateEdit, int xmlId)
        {
            XElement element = new XElement("EmployeeTemplate",
                                            new XAttribute("id", xmlId),
                                            new XElement("Title", CleanStringValue(employeeTemplateEdit.Title)),
                                            new XElement("EmployeeNr", employeeTemplateEdit.EmployeeNr),
                                            new XElement("SocialSec", employeeTemplateEdit.SocialSec),
                                            new XElement("EmployeeName", employeeTemplateEdit.EmployeeName));

            int groupXmlId = 0;
            foreach (var group in employeeTemplateEdit.EmployeeTemplateEditGroups)
            {
                groupXmlId++;
                element.Add(group.ToXElement(groupXmlId));
            }

            return element;
        }

        public static XElement ToXElement(this EmployeeTemplateEditGroupDTO employeeTemplateEditGroup, int xmlId)
        {
            XElement element = new XElement("EmployeeTemplateGroup",
                                            new XAttribute("id", xmlId),
                                            new XElement("Name", CleanStringValue(employeeTemplateEditGroup.EmployeeTemplateGroup.Name)),
                                            new XElement("Description", CleanStringValue(employeeTemplateEditGroup.EmployeeTemplateGroup.Description)),
                                            new XElement("Title", CleanStringValue(employeeTemplateEditGroup.EmployeeTemplateGroup.Name)),
                                            new XElement("SortOrder", employeeTemplateEditGroup.EmployeeTemplateGroup.SortOrder),
                                            new XElement("Type", (int)employeeTemplateEditGroup.EmployeeTemplateGroup.Type),
                                            new XElement("NewPageBefore", employeeTemplateEditGroup.EmployeeTemplateGroup.NewPageBefore.ToInt()));

            int rowXmlId = 1;
            if (!employeeTemplateEditGroup.EmployeeTemplateEditRows.Any() && employeeTemplateEditGroup.Type == TermGroup_EmployeeTemplateGroupType.SubstituteShifts)
            {
                var row = new EmployeeTemplateEditFieldDTO(new EmployeeTemplateGroupRowDTO(), "", "", "");
                element.Add(row.ToXElement(rowXmlId));
            }
            else
            {
                foreach (var row in employeeTemplateEditGroup.EmployeeTemplateEditRows.Where(w => !w.EmployeeTemplateGroupRowDTO.HideInReport))
                {
                    rowXmlId++;
                    element.Add(row.ToXElement(rowXmlId));
                }
            }
            int substituteShiftXmlId = 1;
            foreach (var groupedByDate in employeeTemplateEditGroup.SubstituteShiftsTuples.GroupBy(x => x.Item1))
            {
                foreach (var groupedByAbsence in groupedByDate.GroupBy(x => x.Item2))
                {
                    foreach (var groupedByText in groupedByAbsence.GroupBy(x => x.Item3))
                    {
                        List<SubstituteShiftDTO> shifts = groupedByText.Select(x => x.Item4).ToList();
                        DateTime scheduleIn = shifts.OrderBy(x => x.StartTime).FirstOrDefault().StartTime;
                        DateTime scheduleOut = shifts.OrderByDescending(x => x.StopTime).FirstOrDefault().StopTime;
                        int duration = shifts.Sum(s => s.Duration);

                        element.Add(new XElement("SubstituteShift",
                            new XAttribute("id", substituteShiftXmlId),
                            new XElement("IsAbsence", groupedByAbsence.Key.ToInt()),
                            new XElement("Date", groupedByDate.Key),
                            new XElement("Time", $"{scheduleIn.ToShortTimeString()}-{scheduleOut.ToShortTimeString()}"),
                            new XElement("NbrOfHours", CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(duration), false, false, false)),
                            new XElement("SubstituteText", groupedByText.Key)));
                        substituteShiftXmlId++;
                    }
                }
            }
            return element;
        }

        public static XElement ToXElement(this EmployeeTemplateEditFieldDTO employeeTemplateEditRow, int xmlId)
        {
            XElement element = new XElement("EmployeeTemplateField",
                                            new XAttribute("id", xmlId),
                                            new XElement("InitialValue", CleanStringValue(employeeTemplateEditRow.InitialValue)),
                                            new XElement("Type", (int)employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.Type),
                                            new XElement("RegistrationLevel", employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.RegistrationLevel),
                                            new XElement("MandatoryLevel", employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.MandatoryLevel),
                                            new XElement("Comment", employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.Comment),
                                            new XElement("Row", employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.Row),
                                            new XElement("StartColumn", employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.StartColumn),
                                            new XElement("SpanColumns", employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.SpanColumns),
                                            new XElement("NbrOfColumns", employeeTemplateEditRow.NumberOfColumns()),
                                            new XElement("Format", CleanStringValue(employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.Format)),
                                            new XElement("Title", employeeTemplateEditRow.Title),
                                            new XElement("Description", employeeTemplateEditRow.Description),
                                            new XElement("Bold", 0),
                                            new XElement("CanGrow", employeeTemplateEditRow.CanGrow.ToInt()),
                                            new XElement("Bool", employeeTemplateEditRow.IsValidBoolType && StringUtility.IsValidBool(employeeTemplateEditRow.InitialValue) ? true.ToInt() : false.ToInt()),
                                            new XElement("Checked", employeeTemplateEditRow.IsValidBoolType && StringUtility.IsValidBool(employeeTemplateEditRow.InitialValue) ? StringUtility.GetBool(employeeTemplateEditRow.InitialValue).ToInt() : false.ToInt()),
                                            new XElement("Border", 0));

            return element;
        }

        private static string CleanStringValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return Regex.Replace(value, @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]", "");
        }

        public static bool HasOverlappingColumns(this List<EmployeeTemplateEditFieldDTO> employeeTemplateEditRows)
        {
            foreach (var rowNumber in employeeTemplateEditRows.GroupBy(g => g.EmployeeTemplateGroupRowDTO.Row))
            {
                if (rowNumber.Any(a => a.NumberOfColumns() > 4))
                    return true;

                List<int> usedColumns = new List<int>();

                foreach (var row in rowNumber)
                {
                    foreach (var col in row.SpanningColumns())
                    {
                        if (!usedColumns.Contains(col))
                            usedColumns.Add(col);
                        else
                            return true;
                    }
                }
            }

            return false;
        }

        public static int NumberOfColumns(this EmployeeTemplateEditFieldDTO employeeTemplateEditRow)
        {
            return employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.StartColumn + employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.SpanColumns;
        }

        public static List<int> SpanningColumns(this EmployeeTemplateEditFieldDTO employeeTemplateEditRow)
        {
            List<int> ints = new List<int>();
            ints.Add(employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.StartColumn);
            int last = employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.StartColumn + employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.SpanColumns;
            int current = last;

            while (current > employeeTemplateEditRow.EmployeeTemplateGroupRowDTO.StartColumn)
            {
                if (!ints.Contains(current))
                    ints.Add(current);
                current--;
            }

            return ints;
        }

        #endregion
    }
}
