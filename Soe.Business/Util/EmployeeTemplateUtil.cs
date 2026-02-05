using Newtonsoft.Json;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    public static class EmployeeTemplateUtil
    {
        public static EmployeeChangeIODTO ToEmployeeChangeIODTO(this SaveEmployeeFromTemplateHeadDTO saveEmployeeFromTemplateHead, EmployeeChangeIODTO employeeChange, List<GenericType> terms, TermGroup_Country companyCountry)
        {
            if (saveEmployeeFromTemplateHead == null || employeeChange == null)
                return null;

            saveEmployeeFromTemplateHead.EnsureEmploymentStartDate();
            saveEmployeeFromTemplateHead.SplitHierarchicalAccounts();
            saveEmployeeFromTemplateHead.CreateNewEmployment(employeeChange, out string employmentExternalCode);
            saveEmployeeFromTemplateHead.MergeDisbursement();
            saveEmployeeFromTemplateHead.SplitEmploymentPriceTypes();

            foreach (SaveEmployeeFromTemplateRowDTO row in saveEmployeeFromTemplateHead.Rows)
            {
                var dto = row.ToEmployeeChangeRowIODTO(saveEmployeeFromTemplateHead);

                if (dto.EmployeeChangeType == EmployeeChangeType.EmployeePosition)
                {
                    string[] arrPosition = dto.Value.Split('#');
                    if (arrPosition.Length > 0)
                        dto.Value = arrPosition[0];

                    if (arrPosition.Length > 1 && arrPosition[1].ToLowerInvariant() == "true")
                    {
                        employeeChange.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO()
                        {
                            EmployeeChangeType = EmployeeChangeType.EmployeePositionDefault,
                            Value = arrPosition[0]  // This is not a typo, value should be same as positionId to make it default
                        });
                    }
                }

                if (dto.EmployeeChangeType != EmployeeChangeType.None)
                    employeeChange.EmployeeChangeRowIOs.Add(dto);
            }
            
            employeeChange.EmployeeChangeRowIOs.ForEach(s => s.Validate(terms, true, companyCountry));
            
            if (!employmentExternalCode.IsNullOrEmpty())
                employeeChange.EmployeeChangeRowIOs.Where(e => e.EmployeeChangeType != EmployeeChangeType.NewEmployments).ToList().ForEach(s => s.OptionalEmploymentExternalCode = employmentExternalCode);

            return employeeChange;
        }

        private static void EnsureEmploymentStartDate(this SaveEmployeeFromTemplateHeadDTO saveEmployeeFromTemplateHead)
        {
            if (saveEmployeeFromTemplateHead == null)
                return;

            if (!saveEmployeeFromTemplateHead.Rows.Any(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate))
            {
                saveEmployeeFromTemplateHead.Rows.Add(new SaveEmployeeFromTemplateRowDTO()
                {
                    Type = TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate,
                    Value = saveEmployeeFromTemplateHead.Date.ToShortDateString(),
                    StartDate = saveEmployeeFromTemplateHead.Date
                });
            }
            else
            {
                var row = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate);
                if (row != null)
                {
                    row.Value = saveEmployeeFromTemplateHead.Date.ToShortDateString();
                    row.StartDate = saveEmployeeFromTemplateHead.Date;
                }
            }
        }

        private static void SplitHierarchicalAccounts(this SaveEmployeeFromTemplateHeadDTO saveEmployeeFromTemplateHead)
        {
            var addedRows = new List<SaveEmployeeFromTemplateRowDTO>();

            foreach (var row in saveEmployeeFromTemplateHead.Rows)
            {
                if (row.Type != TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount)
                    continue;
                if (row.Value.IsNullOrEmpty())
                    continue;

                var accountDTO = JsonConvert.DeserializeObject<EmployeeTemplateEmployeeAccountDTO>(row.Value);
                if (accountDTO != null && !accountDTO.AccountId.IsNullOrEmpty())
                {
                    row.Value = null;

                    var accountRow = new SaveEmployeeFromTemplateRowDTO()
                    {
                        Entity = SoeEntityType.Employee,
                        Type = TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount,                        
                        Sort = 1,
                        Value = accountDTO.AccountId.ToString(),
                        ExtraValue = accountDTO.MainAllocation + "#" + accountDTO.Default,
                        StartDate = CalendarUtility.GetNullableDateTime(accountDTO.DateFromString),
                        StopDate = CalendarUtility.GetNullableDateTime(accountDTO.DateToString),
                    };

                    addedRows.Add(accountRow);

                    if (!accountDTO.ChildAccountId.IsNullOrEmpty())
                    {
                        var childAccountRow = new SaveEmployeeFromTemplateRowDTO()
                        {
                            Entity = SoeEntityType.Employee,
                            Type = TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount,
                            RecordId = accountDTO.AccountId,    // Parent
                            Sort = 2,
                            Value = accountDTO.ChildAccountId.ToString(),
                            ExtraValue = accountDTO.MainAllocation + "#" + accountDTO.Default,
                            StartDate = CalendarUtility.GetNullableDateTime(accountDTO.DateFromString),
                            StopDate = CalendarUtility.GetNullableDateTime(accountDTO.DateToString),
                        };

                        addedRows.Add(childAccountRow);

                        if (!accountDTO.SubChildAccountId.IsNullOrEmpty())
                        {
                            var subChildAccountRow = new SaveEmployeeFromTemplateRowDTO()
                            {
                                Entity = SoeEntityType.Employee,
                                Type = TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount,
                                RecordId = accountDTO.ChildAccountId,   // Parent
                                Sort = 3,
                                Value = accountDTO.SubChildAccountId.ToString(),
                                ExtraValue = accountDTO.MainAllocation + "#" + accountDTO.Default,
                                StartDate = CalendarUtility.GetNullableDateTime(accountDTO.DateFromString),
                                StopDate = CalendarUtility.GetNullableDateTime(accountDTO.DateToString),
                            };
                            addedRows.Add(subChildAccountRow);
                        }
                    }
                }
            }

            saveEmployeeFromTemplateHead.Rows = saveEmployeeFromTemplateHead.Rows.Where(w => w.Type != TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount).ToList();
            saveEmployeeFromTemplateHead.Rows.AddRange(addedRows);
        }

        private static void CreateNewEmployment(this SaveEmployeeFromTemplateHeadDTO saveEmployeeFromTemplateHead, EmployeeChangeIODTO employeeChange, out string employmentExternalCode)
        {
            employmentExternalCode = null;

            if (saveEmployeeFromTemplateHead.EmployeeId.IsNullOrEmpty())
                return;

            var employeeGroup = employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.EmployeeGroup)?.Value;
            if (employeeGroup != null)
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.EmployeeGroup));

            var payrollGroup = employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.PayrollGroup)?.Value;
            if (payrollGroup != null)
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.PayrollGroup));

            var vacationGroup = employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.VacationGroup)?.Value;
            if (vacationGroup != null)
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.VacationGroup));

            var annualLeaveGroup = employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.AnnualLeaveGroup)?.Value;
            if (annualLeaveGroup != null)
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.AnnualLeaveGroup));

            bool isSecondary = false;
            if (saveEmployeeFromTemplateHead.Rows.Any(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.IsSecondaryEmployment) && bool.TryParse(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.IsSecondaryEmployment)?.Value ?? "false", out isSecondary))
            {
                saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.IsSecondaryEmployment));
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.IsSecondaryEmployment));
                employmentExternalCode = Guid.NewGuid().ToString();
            }

            var dateFrom = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate)?.StartDate;
            if (!dateFrom.HasValue)
            {
                var row = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate);
                if (row != null && DateTime.TryParse(row.Value, out DateTime parsedDate))
                    dateFrom = parsedDate;
            }
            if (dateFrom.HasValue)
            {
                saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate));
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.EmploymentStartDateChange));
            }

            var dateTo = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentStopDate)?.StopDate;
            if (!dateTo.HasValue)
            {
                var row = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentStopDate);
                if (row != null && DateTime.TryParse(row.Value, out DateTime parsedDate))
                    dateTo = parsedDate;
            }
            if (dateTo.HasValue)
            {
                saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentStopDate));
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.EmploymentStopDateChange));
            }
            else if (isSecondary)
            {
                dateTo = dateFrom;
            }                

            var employmentWorkTimeWeek = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentWorkTimeWeek)?.Value;
            if (employmentWorkTimeWeek != null)
            {
                saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentWorkTimeWeek));
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.WorkTimeWeekMinutes));
            }
            int.TryParse(employmentWorkTimeWeek, out int employmentWorkTimeWeekMinutes);

            var employmentFullTimeWorkTimeWeek = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentFullTimeWorkWeek)?.Value;
            if (employmentFullTimeWorkTimeWeek != null)
            {
                saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentFullTimeWorkWeek));
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.FullTimeWorkTimeWeekMinutes));
            }

            var employmentType = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentType)?.Value;
            if (employmentType != null)
            {
                saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentType));
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.EmploymentType));
            }

            bool? excludeFromCalculation = null;
            string excludeFromCalculationValue = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(f => f.Type == TermGroup_EmployeeTemplateGroupRowType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment)?.Value;
            if (excludeFromCalculationValue != null)
            {
                saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment));
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment));

                int exclude = 0;
                Int32.TryParse(excludeFromCalculationValue, out exclude);
                if (exclude > 0)
                {
                    if (exclude == (int)TermGroup_ExcludeFromWorkTimeWeekCalculationItems.Yes)
                        excludeFromCalculation = true;
                    else if (exclude == (int)TermGroup_ExcludeFromWorkTimeWeekCalculationItems.No)
                        excludeFromCalculation = false;
                }
            }

            string substituteFor = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.SubstituteFor)?.Value;
            if (substituteFor != null)
            {
                saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.SubstituteFor));
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.SubstituteFor));
            }

            string substituteDueTo = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.SubstituteForDueTo)?.Value;
            if (substituteDueTo != null)
            {
                saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.SubstituteForDueTo));
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.SubstituteForDueTo));
            }

            // As of #112273 it should be possible to add a new position when creating a new employment
            //string position = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.Position)?.Value;
            //if (position != null)
            //{
            //    saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.Position));
            //    employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.EmployeePosition));
            //}

            string specialConditions = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.SpecialConditions)?.Value;
            if (specialConditions != null)
            {
                saveEmployeeFromTemplateHead.Rows.Remove(saveEmployeeFromTemplateHead.Rows.FirstOrDefault(a => a.Type == TermGroup_EmployeeTemplateGroupRowType.SpecialConditions));
                employeeChange.EmployeeChangeRowIOs.Remove(employeeChange.EmployeeChangeRowIOs.FirstOrDefault(f => f.EmployeeChangeType == EmployeeChangeType.SpecialConditions));
            }


            var employmentIODTO = new NewEmploymentRowIO()
            {
                DateFrom = dateFrom ?? saveEmployeeFromTemplateHead.Date,
                DateTo = dateTo,
                EmployeeGroupCode = employeeGroup,
                PayrollGroupCode = payrollGroup,
                VacationGroupCode = vacationGroup,
                WorkTimeWeek = employmentWorkTimeWeekMinutes,
                EmploymentTypeCode = employmentType,
                IsSecondaryEmployment = isSecondary,
                SubstituteFor = substituteFor,
                SubstituteForDueTo = substituteDueTo,
                SpecialConditions = specialConditions,
                ExternalCode = employmentExternalCode,
                ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = excludeFromCalculation
            };

            var newEmployments = employmentIODTO.ObjToList();

            employeeChange.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO()
            { 
                EmployeeChangeType = EmployeeChangeType.NewEmployments, 
                Value = JsonConvert.SerializeObject(newEmployments), 
                NewEmploymentRows = newEmployments
            });
        }

        private static void MergeDisbursement(this SaveEmployeeFromTemplateHeadDTO saveEmployeeFromTemplateHead)
        {
            var addedRow = new SaveEmployeeFromTemplateRowDTO()
            {
                Type = TermGroup_EmployeeTemplateGroupRowType.DisbursementMethod,
            };

            var method = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(w => w.Type == TermGroup_EmployeeTemplateGroupRowType.DisbursementAccount);
            if (method?.Value != null)
            {
                if (!string.IsNullOrEmpty(method.Value))
                {
                    var dto = JsonConvert.DeserializeObject<EmployeeTemplateDisbursementAccountDTO>(method.Value);
                    addedRow.Value = dto.Method.ToString();
                }
                saveEmployeeFromTemplateHead.Rows.Add(addedRow);
            }
        }

        private static void SplitEmploymentPriceTypes(this SaveEmployeeFromTemplateHeadDTO saveEmployeeFromTemplateHead)
        {
            var pricetypeRow = saveEmployeeFromTemplateHead.Rows.FirstOrDefault(w => w.Type == TermGroup_EmployeeTemplateGroupRowType.EmploymentPriceTypes);
            if (pricetypeRow == null)
                return;

            var addedRows = new List<SaveEmployeeFromTemplateRowDTO>();

            try
            {
                if (!string.IsNullOrEmpty(pricetypeRow.Value))
                {
                    var dtos = JsonConvert.DeserializeObject<List<EmployeeTemplateEmploymentPriceTypeDTO>>(pricetypeRow.Value);
                    if (!dtos.IsNullOrEmpty())
                    {
                        foreach (var dto in dtos.Where(d => d.Amount != 0).ToList())
                        {
                            string level = dto.PayrollLevelId.HasValue && dto.PayrollLevelId != 0 ? $"#{dto.PayrollLevelId}" : string.Empty;
                            var priceTypeRow = new SaveEmployeeFromTemplateRowDTO()
                            {
                                Entity = SoeEntityType.Employee,
                                Type = TermGroup_EmployeeTemplateGroupRowType.EmploymentPriceTypes,
                                Sort = 1,
                                Value = NumberUtility.ToDecimal(dto.Amount.ToString(), 2).ToString(),
                                StartDate = CalendarUtility.GetNullableDateTime(dto.FromDate),
                                ExtraValue = dto.PayrollPriceTypeId.ToString() + level
                            };

                            addedRows.Add(priceTypeRow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogCollector.LogCollector.LogError(ex, "SplitEmploymentPriceTypes");
            }

            saveEmployeeFromTemplateHead.Rows = saveEmployeeFromTemplateHead.Rows.Where(w => w.Type != TermGroup_EmployeeTemplateGroupRowType.EmploymentPriceTypes).ToList();
            saveEmployeeFromTemplateHead.Rows.AddRange(addedRows);
        }

        private static EmployeeChangeRowIODTO ToEmployeeChangeRowIODTO(this SaveEmployeeFromTemplateRowDTO saveEmployeeFromTemplateRowDTO, SaveEmployeeFromTemplateHeadDTO saveEmployeeFromTemplateHead)
        {
            var row = new EmployeeChangeRowIODTO();

            switch (saveEmployeeFromTemplateRowDTO.Type)
            {
                case TermGroup_EmployeeTemplateGroupRowType.FirstName:
                    row.EmployeeChangeType = EmployeeChangeType.FirstName;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.LastName:
                    row.EmployeeChangeType = EmployeeChangeType.LastName;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.SocialSec:
                    row.EmployeeChangeType = EmployeeChangeType.SocialSec;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate:
                    row.EmployeeChangeType = EmployeeChangeType.EmploymentStartDateChange;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.EmploymentStopDate:
                    row.EmployeeChangeType = EmployeeChangeType.EmploymentStopDateChange;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.OptionalEmploymentDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.EmploymentFullTimeWorkWeek:
                    row.EmployeeChangeType = EmployeeChangeType.FullTimeWorkTimeWeekMinutes;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.OptionalEmploymentDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.EmploymentType:
                    row.EmployeeChangeType = EmployeeChangeType.EmploymentType;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.EmploymentWorkTimeWeek:
                    row.EmployeeChangeType = EmployeeChangeType.WorkTimeWeekMinutes;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    int.TryParse(saveEmployeeFromTemplateRowDTO.Value, out int minutes);
                    row.Value = minutes.ToString();
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.EmploymentPercent:
                    row.EmployeeChangeType = EmployeeChangeType.EmploymentPercent;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.IsSecondaryEmployment:
                    row.EmployeeChangeType = EmployeeChangeType.IsSecondaryEmployment;
                    row.Value = StringUtility.GetBool(saveEmployeeFromTemplateRowDTO.Value).ToString();
                    row.OptionalEmploymentExternalCode = Guid.NewGuid().ToString();
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment:
                    row.EmployeeChangeType = EmployeeChangeType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.OptionalEmploymentDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.EmploymentPriceTypes:
                    row.EmployeeChangeType = EmployeeChangeType.EmploymentPriceType;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.OptionalExternalCode = saveEmployeeFromTemplateRowDTO.ExtraValue;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.AddressRow:
                    row.EmployeeChangeType = EmployeeChangeType.Address;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.AddressRow2:
                    row.EmployeeChangeType = EmployeeChangeType.AddressCO;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.ZipCode:
                    row.EmployeeChangeType = EmployeeChangeType.AddressPostCode;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.City:
                    row.EmployeeChangeType = EmployeeChangeType.AddressPostalAddress;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.Telephone:
                    row.EmployeeChangeType = EmployeeChangeType.PhoneMobile;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.Email:
                    row.EmployeeChangeType = EmployeeChangeType.Email;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.DisbursementMethod:
                    row.EmployeeChangeType = EmployeeChangeType.DisbursementMethod;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.DisbursementAccountNr:
                    row.EmployeeChangeType = EmployeeChangeType.DisbursementAccountNr;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.DisbursementAccount:
                    if (!string.IsNullOrEmpty(saveEmployeeFromTemplateRowDTO.Value))
                    {
                        var dto = JsonConvert.DeserializeObject<EmployeeTemplateDisbursementAccountDTO>(saveEmployeeFromTemplateRowDTO.Value);
                        string dontValidate = dto.DontValidateAccountNr || dto.Method == (int)TermGroup_EmployeeDisbursementMethod.SE_CashDeposit ? "#1" : "";
                        row.EmployeeChangeType = EmployeeChangeType.DisbursementAccountNr;
                        row.Value = $"{dto.ClearingNr}#{dto.AccountNr}" + dontValidate;
                    }
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.Position:
                    if (!string.IsNullOrEmpty(saveEmployeeFromTemplateRowDTO.Value))
                    {
                        var dto = JsonConvert.DeserializeObject<EmployeeTemplatePositionDTO>(saveEmployeeFromTemplateRowDTO.Value);
                        row.EmployeeChangeType = EmployeeChangeType.EmployeePosition;
                        row.Value = $"{dto.PositionId}#{dto.Default}";
                    }
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.Department:
                case TermGroup_EmployeeTemplateGroupRowType.WorkPlace:
                    row.EmployeeChangeType = EmployeeChangeType.WorkPlace;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.ExternalCode:
                    row.EmployeeChangeType = EmployeeChangeType.ExternalCode;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.ExperienceMonths:
                    row.EmployeeChangeType = EmployeeChangeType.ExperienceMonths;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.ExperienceAgreedOrEstablished:
                    row.EmployeeChangeType = EmployeeChangeType.ExperienceAgreedOrEstablished;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.WorkTasks:
                    row.EmployeeChangeType = EmployeeChangeType.WorkTasks;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.SubstituteFor:
                    row.EmployeeChangeType = EmployeeChangeType.SubstituteFor;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.SubstituteForDueTo:
                    row.EmployeeChangeType = EmployeeChangeType.SubstituteForDueTo;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.TaxRate:
                    row.EmployeeChangeType = EmployeeChangeType.TaxRate;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.TaxTinNumber:
                    row.EmployeeChangeType = EmployeeChangeType.TaxTinNumber;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCode:
                    row.EmployeeChangeType = EmployeeChangeType.TaxCountryCode;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.TaxBirthPlace:
                    row.EmployeeChangeType = EmployeeChangeType.TaxBirthPlace;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCodeBirthPlace:
                    row.EmployeeChangeType = EmployeeChangeType.TaxCountryCodeBirthPlace;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCodeCitizen:
                    row.EmployeeChangeType = EmployeeChangeType.TaxCountryCodeCitizen;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.VacationDaysPayed:
                    row.EmployeeChangeType = EmployeeChangeType.VacationDaysPaid;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.VacationDaysUnpayed:
                    row.EmployeeChangeType = EmployeeChangeType.VacationDaysUnPaid;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.VacationDaysAdvance:
                    row.EmployeeChangeType = EmployeeChangeType.VacationDaysAdvance;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee:
                    if (!string.IsNullOrEmpty(saveEmployeeFromTemplateRowDTO.Value))
                    {
                        row.EmployeeChangeType = EmployeeChangeType.ExtraFieldEmployee;
                        row.Value = saveEmployeeFromTemplateRowDTO.Value;
                        row.OptionalExternalCode = saveEmployeeFromTemplateRowDTO.RecordId.ToString();
                    }
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.SpecialConditions:
                    row.EmployeeChangeType = EmployeeChangeType.SpecialConditions;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.FromDate = saveEmployeeFromTemplateHead.Date;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.HierarchicalAccount:
                    row.EmployeeChangeType = EmployeeChangeType.HierarchicalAccount;
                    row.OptionalExternalCode = $"{saveEmployeeFromTemplateRowDTO.RecordId}|{saveEmployeeFromTemplateRowDTO.ExtraValue}";
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    row.Sort = saveEmployeeFromTemplateRowDTO.Sort;
                    row.FromDate = saveEmployeeFromTemplateRowDTO.StartDate ?? saveEmployeeFromTemplateHead.Date;
                    row.ToDate = saveEmployeeFromTemplateRowDTO.StopDate;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsPersonalCategory:
                    row.EmployeeChangeType = EmployeeChangeType.PayrollStatisticsPersonalCategory;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkTimeCategory:
                    row.EmployeeChangeType = EmployeeChangeType.PayrollStatisticsWorkTimeCategory;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsSalaryType:
                    row.EmployeeChangeType = EmployeeChangeType.PayrollStatisticsSalaryType;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkPlaceNumber:
                    row.EmployeeChangeType = EmployeeChangeType.PayrollStatisticsWorkPlaceNumber;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsCFARNumber:
                    row.EmployeeChangeType = EmployeeChangeType.PayrollStatisticsCFARNumber;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.ControlTaskWorkPlaceSCB:
                    row.EmployeeChangeType = EmployeeChangeType.ControlTaskWorkPlaceSCB;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.ControlTaskPartnerInCloseCompany:
                    row.EmployeeChangeType = EmployeeChangeType.ControlTaskPartnerInCloseCompany;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.ControlTaskBenefitAsPension:
                    row.EmployeeChangeType = EmployeeChangeType.ControlTaskBenefitAsPension;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.AFACategory:
                    row.EmployeeChangeType = EmployeeChangeType.AFACategory;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.AFASpecialAgreement:
                    row.EmployeeChangeType = EmployeeChangeType.AFASpecialAgreement;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.AFAWorkplaceNr:
                    row.EmployeeChangeType = EmployeeChangeType.AFAWorkplaceNr;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.AFAParttimePensionCode:
                    row.EmployeeChangeType = EmployeeChangeType.AFAParttimePensionCode;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.CollectumITPPlan:
                    row.EmployeeChangeType = EmployeeChangeType.CollectumITPPlan;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.CollectumAgreedOnProduct:
                    row.EmployeeChangeType = EmployeeChangeType.CollectumAgreedOnProduct;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.CollectumCostPlace:
                    row.EmployeeChangeType = EmployeeChangeType.CollectumCostPlace;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDate:
                    row.EmployeeChangeType = EmployeeChangeType.CollectumCancellationDate;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDateIsLeaveOfAbsence:
                    row.EmployeeChangeType = EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.KPARetirementAge:
                    row.EmployeeChangeType = EmployeeChangeType.KPARetirementAge;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.KPABelonging:
                    row.EmployeeChangeType = EmployeeChangeType.KPABelonging;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.KPAEndCode:
                    row.EmployeeChangeType = EmployeeChangeType.KPAEndCode;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.KPAAgreementType:
                    row.EmployeeChangeType = EmployeeChangeType.KPAAgreementType;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreementArea:
                    row.EmployeeChangeType = EmployeeChangeType.BygglosenAgreementArea;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.BygglosenAllocationNumber:
                    row.EmployeeChangeType = EmployeeChangeType.BygglosenAllocationNumber;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryFormula:
                    row.EmployeeChangeType = EmployeeChangeType.BygglosenSalaryFormula;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.BygglosenMunicipalCode:
                    row.EmployeeChangeType = EmployeeChangeType.BygglosenMunicipalCode;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.BygglosenProfessionCategory:
                    row.EmployeeChangeType = EmployeeChangeType.BygglosenProfessionCategory;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryType:
                    row.EmployeeChangeType = EmployeeChangeType.BygglosenSalaryType;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.BygglosenWorkPlaceNumber:
                    row.EmployeeChangeType = EmployeeChangeType.BygglosenWorkPlaceNumber;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.BygglosenLendedToOrgNr:
                    row.EmployeeChangeType = EmployeeChangeType.BygglosenLendedToOrgNr;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreedHourlyPayLevel:
                    row.EmployeeChangeType = EmployeeChangeType.BygglosenAgreedHourlyPayLevel;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.GTPAgreementNumber:
                    row.EmployeeChangeType = EmployeeChangeType.GTPAgreementNumber;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.GTPExcluded:
                    row.EmployeeChangeType = EmployeeChangeType.GTPExcluded;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentAddress:
                    row.EmployeeChangeType = EmployeeChangeType.AGIPlaceOfEmploymentAddress;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentCity:
                    row.EmployeeChangeType = EmployeeChangeType.AGIPlaceOfEmploymentCity;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentIgnore:
                    row.EmployeeChangeType = EmployeeChangeType.AGIPlaceOfEmploymentIgnore;
                    row.Value = saveEmployeeFromTemplateRowDTO.Value;
                    break;
                case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount:
                case TermGroup_EmployeeTemplateGroupRowType.PayrollFormula:
                case TermGroup_EmployeeTemplateGroupRowType.EmployeeNr:
                case TermGroup_EmployeeTemplateGroupRowType.Name:
                case TermGroup_EmployeeTemplateGroupRowType.Address:
                case TermGroup_EmployeeTemplateGroupRowType.ZipCity:
                case TermGroup_EmployeeTemplateGroupRowType.CompanyName:
                case TermGroup_EmployeeTemplateGroupRowType.CompanyOrgNr:
                case TermGroup_EmployeeTemplateGroupRowType.CompanyAddress:
                case TermGroup_EmployeeTemplateGroupRowType.CompanyAddressRow:
                case TermGroup_EmployeeTemplateGroupRowType.CompanyAddressRow2:
                case TermGroup_EmployeeTemplateGroupRowType.CompanyZipCode:
                case TermGroup_EmployeeTemplateGroupRowType.CompanyCity:
                case TermGroup_EmployeeTemplateGroupRowType.CompanyZipCity:
                case TermGroup_EmployeeTemplateGroupRowType.CompanyTelephone:
                case TermGroup_EmployeeTemplateGroupRowType.CompanyEmail:
                case TermGroup_EmployeeTemplateGroupRowType.CityAndDate:
                case TermGroup_EmployeeTemplateGroupRowType.SignatureEmployee:
                case TermGroup_EmployeeTemplateGroupRowType.SignatureEmployer:
                case TermGroup_EmployeeTemplateGroupRowType.GeneralText:
                    break;
                default:
                    break;
            }

            return row;
        }
    }
}
