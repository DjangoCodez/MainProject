using Soe.Sys.Common.DTO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public static class ExtensionsSys
    {
        #region Tables

        #region Dashboard

        #region SysGauge

        public static SysGaugeDTO ToDTO(this SysGauge e)
        {
            if (e == null)
                return null;

            SysGaugeDTO dto = new SysGaugeDTO()
            {
                SysGaugeId = e.SysGaugeId,
                SysFeatureId = e.SysFeatureId,
                SysTermId = e.SysTermId,
                GaugeName = e.GaugeName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            dto.Name = e.Name;

            return dto;
        }

        public static IEnumerable<SysGaugeDTO> ToDTOs(this IEnumerable<SysGauge> l)
        {
            var dtos = new List<SysGaugeDTO>();
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

        #region SysPerformanceMonitor

        public static SysPerformanceMonitor FromDTO(this SysPerformanceMonitorDTO dto)
        {
            if (dto == null)
                return null;

            SysPerformanceMonitor e = new SysPerformanceMonitor()
            {
                SysPerformanceMonitorId = dto.SysPerformanceMonitorId,
                DatabaseName = dto.DatabaseName,
                HostName = dto.HostName,
                Task = (int)dto.Task,
                ActorCompanyId = dto.ActorCompanyId,
                RecordId = dto.RecordId,
                Timestamp = dto.Timestamp,
                Duration = dto.Duration,
                Size = dto.Size,
                NbrOfRecords = dto.NbrOfRecords,
                NbrOfSubRecords = dto.NbrOfSubRecords,
            };

            return e;
        }

        #endregion

        #endregion

        #region Import/Export

        #region Export

        #region SysExportDefinition

        public static SysExportDefinitionDTO ToDTO(this SysExportDefinition e, bool includeLevels)
        {
            if (e == null)
                return null;

            SysExportDefinitionDTO dto = new SysExportDefinitionDTO()
            {
                SysExportDefinitionId = e.SysExportDefinitionId,
                SysExportHeadId = e.SysExportHeadId,
                Type = (TermGroup_SysExportDefinitionType)e.Type,
                Name = e.Name,
                Separator = e.Separator,
                XmlTagHead = e.XmlTagHead,
                SpecialFunctionality = e.SpecialFunctionality,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            if (includeLevels)
                dto.SysExportDefinitionLevels = (e.SysExportDefinitionLevel != null && e.SysExportDefinitionLevel.Count > 0) ? e.SysExportDefinitionLevel.ToDTOs().ToList() : new List<SysExportDefinitionLevelDTO>();

            return dto;
        }

        public static IEnumerable<SysExportDefinitionDTO> ToDTOs(this IEnumerable<SysExportDefinition> l, bool includeLevels)
        {
            var dtos = new List<SysExportDefinitionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeLevels));
                }
            }
            return dtos;
        }

        #endregion

        #region SysExportDefinitionLevel

        public static SysExportDefinitionLevelDTO ToDTO(this SysExportDefinitionLevel e)
        {
            if (e == null)
                return null;

            SysExportDefinitionLevelDTO dto = new SysExportDefinitionLevelDTO()
            {
                SysExportDefinitionLevelId = e.SysExportDefinitionLevelId,
                SysExportDefinitionId = e.SysExportDefinitionId,
                Level = e.Level,
                Xml = e.Xml
            };

            return dto;
        }

        public static IEnumerable<SysExportDefinitionLevelDTO> ToDTOs(this IEnumerable<SysExportDefinitionLevel> l)
        {
            var dtos = new List<SysExportDefinitionLevelDTO>();
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

        #region SysExportHead

        public static SysExportHeadDTO ToDTO(this SysExportHead e, bool includeRelations, bool includeSelects)
        {
            if (e == null)
                return null;

            SysExportHeadDTO dto = new SysExportHeadDTO()
            {
                SysExportHeadId = e.SysExportHeadId,
                Name = e.Name,
                Description = e.Description,
                Sortorder = e.Sortorder,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            if (includeRelations)
                dto.SysExportRelations = (e.SysExportRelation != null && e.SysExportRelation.Count > 0) ? e.SysExportRelation.ToDTOs().ToList() : new List<SysExportRelationDTO>();

            if (includeSelects)
                dto.SysExportSelects = (e.SysExportSelect != null && e.SysExportSelect.Count > 0) ? e.SysExportSelect.ToDTOs().ToList() : new List<SysExportSelectDTO>();

            return dto;
        }

        public static IEnumerable<SysExportHeadDTO> ToDTOs(this IEnumerable<SysExportHead> l, bool includeRelations, bool includeSelects)
        {
            var dtos = new List<SysExportHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRelations, includeSelects));
                }
            }
            return dtos;
        }

        public static SmallGenericType ToSmallGenericType(this SysExportHead e)
        {
            if (e == null)
                return null;

            SmallGenericType type = new SmallGenericType()
            {
                Id = e.SysExportHeadId,
                Name = e.Name
            };

            return type;
        }

        public static IEnumerable<SmallGenericType> ToSmallGenericTypes(this IEnumerable<SysExportHead> l)
        {
            var dtos = new List<SmallGenericType>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallGenericType());
                }
            }
            return dtos;
        }

        #endregion

        #region SysExportRelation

        public static SysExportRelationDTO ToDTO(this SysExportRelation e)
        {
            if (e == null)
                return null;

            SysExportRelationDTO dto = new SysExportRelationDTO()
            {
                SysExportRelationId = e.SysExportRelationId,
                SysExportHeadId = e.SysExportHeadId,
                LevelParent = e.LevelParent,
                LevelChild = e.LevelChild,
                FieldParent = e.FieldParent,
                FieldChild = e.FieldChild
            };

            return dto;
        }

        public static IEnumerable<SysExportRelationDTO> ToDTOs(this IEnumerable<SysExportRelation> l)
        {
            var dtos = new List<SysExportRelationDTO>();
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

        #region SysExportSelect

        public static SysExportSelectDTO ToDTO(this SysExportSelect e)
        {
            if (e == null)
                return null;

            SysExportSelectDTO dto = new SysExportSelectDTO()
            {
                SysExportSelectId = e.SysExportSelectId,
                SysExportHeadId = e.SysExportHeadId,
                Level = e.Level,
                Name = e.Name,
                Select = e.Select,
                Where = e.Where,
                GroupBy = e.GroupBy,
                OrderBy = e.OrderBy,
                Settings = e.Settings
            };

            return dto;
        }

        public static IEnumerable<SysExportSelectDTO> ToDTOs(this IEnumerable<SysExportSelect> l)
        {
            var dtos = new List<SysExportSelectDTO>();
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

        #region Import

        #region SysImportDefinition

        public static SysImportDefinitionDTO ToDTO(this SysImportDefinition e, bool includeLevels)
        {
            if (e == null)
                return null;

            SysImportDefinitionDTO dto = new SysImportDefinitionDTO()
            {
                SysImportDefinitionId = e.SysImportDefinitionId,
                SysImportHeadId = e.SysImportHeadId,
                Type = (TermGroup_SysImportDefinitionType)e.Type,
                Name = e.Name,
                Separator = e.Separator,
                XmlTagHead = e.XmlTagHead,
                SpecialFunctionality = e.SpecialFunctionality,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Module = (SoeModule)e.Module,
                Guid = e.Guid,
            };

            // Extensions            
            dto.SysImportDefinitionLevels = (includeLevels && e.SysImportDefinitionLevel != null && e.SysImportDefinitionLevel.Count > 0) ? e.SysImportDefinitionLevel.ToDTOs().ToList() : new List<SysImportDefinitionLevelDTO>();

            return dto;
        }

        public static IEnumerable<SysImportDefinitionDTO> ToDTOs(this IEnumerable<SysImportDefinition> l, bool includeLevels)
        {
            var dtos = new List<SysImportDefinitionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeLevels));
                }
            }
            return dtos;
        }

        #endregion

        #region SysImportDefinitionLevel

        public static SysImportDefinitionLevelDTO ToDTO(this SysImportDefinitionLevel e)
        {
            if (e == null)
                return null;

            SysImportDefinitionLevelDTO dto = new SysImportDefinitionLevelDTO()
            {
                SysImportDefinitionLevelId = e.SysImportDefinitionLevelId,
                SysImportDefinitionId = e.SysImportDefinitionId,
                Level = e.Level,
                Xml = e.Xml,
                Columns = e.Columns,
            };

            return dto;
        }

        public static IEnumerable<SysImportDefinitionLevelDTO> ToDTOs(this IEnumerable<SysImportDefinitionLevel> l)
        {
            var dtos = new List<SysImportDefinitionLevelDTO>();
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

        #region SysImportHead

        public static SysImportHeadDTO ToDTO(this SysImportHead e, bool includeRelations, bool includeSelects)
        {
            if (e == null)
                return null;

            SysImportHeadDTO dto = new SysImportHeadDTO()
            {
                SysImportHeadId = e.SysImportHeadId,
                Name = e.Name,
                Description = e.Description,
                Sortorder = e.Sortorder,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Module = (SoeModule)e.Module,
                SysImportHeadTypeId = e.SysImportHeadTypeId,
            };

            // Extensions            
            dto.SysImportRelations = (includeRelations && (e.SysImportRelation != null && e.SysImportRelation.Count > 0)) ? e.SysImportRelation.ToDTOs().ToList() : new List<SysImportRelationDTO>();
            dto.SysImportSelects = (includeSelects && (e.SysImportSelect != null && e.SysImportSelect.Count > 0)) ? e.SysImportSelect.ToDTOs().ToList() : new List<SysImportSelectDTO>();

            return dto;
        }

        public static IEnumerable<SysImportHeadDTO> ToDTOs(this IEnumerable<SysImportHead> l, bool includeRelations, bool includeSelects)
        {
            var dtos = new List<SysImportHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRelations, includeSelects));
                }
            }
            return dtos;
        }

        public static SmallGenericType ToSmallGenericType(this SysImportHead e)
        {
            if (e == null)
                return null;

            SmallGenericType type = new SmallGenericType()
            {
                Id = e.SysImportHeadId,
                Name = e.Name
            };

            return type;
        }

        public static IEnumerable<SmallGenericType> ToSmallGenericTypes(this IEnumerable<SysImportHead> l)
        {
            var dtos = new List<SmallGenericType>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallGenericType());
                }
            }
            return dtos;
        }

        #endregion

        #region SysImportRelation

        public static SysImportRelationDTO ToDTO(this SysImportRelation e)
        {
            if (e == null)
                return null;

            SysImportRelationDTO dto = new SysImportRelationDTO()
            {
                SysImportRelationId = e.SysImportRelationId,
                SysImportHeadId = e.SysImportHeadId,
                TableParent = e.TableParent,
                TableChild = e.TableChild
            };

            return dto;
        }

        public static IEnumerable<SysImportRelationDTO> ToDTOs(this IEnumerable<SysImportRelation> l)
        {
            var dtos = new List<SysImportRelationDTO>();
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

        #region SysImportSelect

        public static SysImportSelectDTO ToDTO(this SysImportSelect e)
        {
            if (e == null)
                return null;

            SysImportSelectDTO dto = new SysImportSelectDTO()
            {
                SysImportSelectId = e.SysImportSelectId,
                SysImportHeadId = e.SysImportHeadId,
                Level = e.Level,
                Name = e.Name,
                Select = e.Select,
                Where = e.Where,
                GroupBy = e.GroupBy,
                OrderBy = e.OrderBy,
                Settings = e.Settings,
                settingObjects = e.SettingsObject,
            };

            return dto;
        }

        public static IEnumerable<SysImportSelectDTO> ToDTOs(this IEnumerable<SysImportSelect> l)
        {
            var dtos = new List<SysImportSelectDTO>();
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

        #endregion

        #region Scheduled jobs

        #region SysJob

        public static SysJobDTO ToDTO(this SysJob e, bool includeSettings)
        {
            if (e == null)
                return null;

            SysJobDTO dto = new SysJobDTO()
            {
                SysJobId = e.SysJobId,
                Name = e.Name,
                Description = e.Description,
                AssemblyName = e.AssemblyName,
                ClassName = e.ClassName,
                AllowParallelExecution = e.AllowParallelExecution,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            if (includeSettings)
            {
                dto.SysJobSettings = new List<SysJobSettingDTO>();
                foreach (var f in e.SysJobSettingJob)
                {
                    if (f?.SysJobSetting != null)
                        dto.SysJobSettings.Add(f.SysJobSetting.ToDTO());
                }
            }


            return dto;
        }

        public static IEnumerable<SysJobDTO> ToDTOs(this IEnumerable<SysJob> l, bool includeSettings)
        {
            var dtos = new List<SysJobDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeSettings));
                }
            }
            return dtos;
        }

        #endregion

        #region SysJobSetting

        public static SysJobSettingDTO ToDTO(this SysJobSetting e)
        {
            if (e == null)
                return null;

            SysJobSettingDTO dto = new SysJobSettingDTO()
            {
                SysJobSettingId = e.SysJobSettingId,
                Type = (SysJobSettingType)e.Type,
                DataType = (SettingDataType)e.DataType,
                Name = e.Name,
                StrData = e.StrData,
                IntData = e.IntData,
                DecimalData = e.DecimalData,
                BoolData = e.BoolData,
                DateData = e.DateData,
                TimeData = e.TimeData
            };

            return dto;
        }

        public static IEnumerable<SysJobSettingDTO> ToDTOs(this IEnumerable<SysJobSetting> l)
        {
            var dtos = new List<SysJobSettingDTO>();
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

        #region SysScheduledJob

        public static SysScheduledJobDTO ToDTO(this SysScheduledJob e, bool includeSettings, bool includeSysJob, bool resolveType)
        {
            if (e == null)
                return null;

            SysScheduledJobDTO dto = new SysScheduledJobDTO()
            {
                SysScheduledJobId = e.SysScheduledJobId,
                SysJobId = e.SysJobId,
                Name = e.Name,
                Description = e.Description,
                DatabaseName = e.DatabaseName,
                ExecuteTime = e.ExecuteTime,
                ExecuteUserId = e.ExecuteUserId,
                AllowParallelExecution = e.AllowParallelExecution,
                RecurrenceType = (ScheduledJobRecurrenceType)e.RecurrenceType,
                RecurrenceCount = e.RecurrenceCount,
                RecurrenceDate = e.RecurrenceDate,
                RecurrenceInterval = e.RecurrenceInterval,
                RetryTypeForInternalError = (ScheduledJobRetryType)e.RetryTypeForInternalError,
                RetryTypeForExternalError = (ScheduledJobRetryType)e.RetryTypeForExternalError,
                RetryCount = e.RetryCount,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (ScheduledJobState)e.State,
                Type = Enum.IsDefined(typeof(ScheduledJobType), e.Type) ? (ScheduledJobType)e.Type : ScheduledJobType.Task,
            };

            // Extensions
            dto.StateName = e.StateName;

            if (includeSettings)
            {
                dto.SysJobSettings = new List<SysJobSettingDTO>();
                foreach (var f in e.SysJobSettingScheduledJob)
                {
                    if (f?.SysJobSetting != null)
                        dto.SysJobSettings.Add(f.SysJobSetting.ToDTO());
                }
            }

            if (includeSysJob && e.SysJob != null)
                dto.SysJob = e.SysJob.ToDTO(includeSettings);

            if (resolveType && e.SysJob != null)
            {
                var cls = ObjectFactory.Create(e.SysJob.AssemblyName, e.SysJob.ClassName);
                if (cls is IMessageClass message)
                    dto.JobStatusMessage = message.GetMessage();
            }
            return dto;
        }

        public static IEnumerable<SysScheduledJobDTO> ToDTOs(this IEnumerable<SysScheduledJob> l, bool includeSettings, bool includeSysJob, bool resolveType)
        {
            var dtos = new List<SysScheduledJobDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeSettings, includeSysJob, resolveType));
                }
            }
            return dtos;
        }

        #endregion

        #region SysScheduledJobLog

        public static SysScheduledJobLogDTO ToDTO(this SysScheduledJobLog e, bool setScheduledJobName)
        {
            if (e == null)
                return null;

            SysScheduledJobLogDTO dto = new SysScheduledJobLogDTO()
            {
                SysScheduledJobLogId = e.SysScheduledJobLogId,
                SysScheduledJobId = e.SysScheduledJobId,
                BatchNr = e.BatchNr,
                LogLevel = e.LogLevel,
                Time = e.Time,
                Message = e.Message
            };

            // Extensions
            if (setScheduledJobName && e.SysScheduledJob != null)
                dto.SysScheduledJobName = e.SysScheduledJob.Name;

            dto.LogLevelName = e.LogLevelName;

            return dto;
        }

        public static IEnumerable<SysScheduledJobLogDTO> ToDTOs(this IEnumerable<SysScheduledJobLog> l, bool setScheduledJobName)
        {
            var dtos = new List<SysScheduledJobLogDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(setScheduledJobName));
                }
            }
            return dtos;
        }

        #endregion

        #endregion

        #region SysAccountStd

        public static SysAccountStdDTO ToDTO(this SysAccountStd e, bool includeRelations)
        {
            if (e == null)
                return null;

            SysAccountStdDTO dto = new SysAccountStdDTO()
            {
                SysAccountStdId = e.SysAccountStdId,
                SysAccountStdTypeId = e.SysAccountStdType != null ? e.SysAccountStdType.SysAccountStdTypeId : 0,    // TODO: Add foreign key to model
                SysVatAccountId = e.SysVatAccount != null ? e.SysVatAccount.SysVatAccountId : 0,                    // TODO: Add foreign key to model
                AccountTypeSysTermId = e.AccountTypeSysTermId,
                AccountNr = e.AccountNr,
                Name = e.Name,
                AmountStop = e.AmountStop,
                UnitStop = e.UnitStop,
                Unit = e.Unit
            };

            // Extensions
            if (e.SysAccountSruCode.IsNullOrEmpty())
            {
                dto.SysAccountSruCodeIds = new List<int>();
                foreach (var sruCode in e.SysAccountSruCode)
                {
                    dto.SysAccountSruCodeIds.Add(sruCode.SysAccountSruCodeId);
                }
            }

            return dto;
        }

        public static SysAccountStdTypeDTO ToDTO(this SysAccountStdType e)
        {
            if (e == null)
                return null;

            SysAccountStdTypeDTO sysAccountStdTypeDTO = new SysAccountStdTypeDTO()
            {
                SysAccountStdTypeId = e.SysAccountStdTypeId,
                Name = e.Name,
                ShortName = e.ShortName,
                SysAccountStdTypeParentId = null,
            };

            return sysAccountStdTypeDTO;

        }

        public static List<SysAccountStdTypeDTO> ToDTOs(this List<SysAccountStdType> l)
        {
            var dtos = new List<SysAccountStdTypeDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;

        }

        public static SysAccountSruCodeDTO ToDTO(this SysAccountSruCode e)
        {
            if (e == null)
                return null;

            SysAccountSruCodeDTO sysAccountSruCodeDTO = new SysAccountSruCodeDTO()
            {
                SysAccountSruCodeId = e.SysAccountSruCodeId,
                SruCode = e.SruCode,
                Name = e.Name,
            };
            return sysAccountSruCodeDTO;

        }

        public static List<SysAccountSruCodeDTO> ToDTOs(this List<SysAccountSruCode> l)
        {
            var dtos = new List<SysAccountSruCodeDTO>();

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

        #region   SysContact

        public static SysContactTypeDTO ToDTO(this SysContactType e)
        {
            if (e == null)
                return null;

            SysContactTypeDTO sysContactTypeDTO = new SysContactTypeDTO()
            {
                SysContactTypeId = (TermGroup_SysContactType)e.SysContactTypeId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
            };
            return sysContactTypeDTO;

        }

        public static List<SysContactTypeDTO> ToDTOs(this List<SysContactType> l)
        {
            var dtos = new List<SysContactTypeDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;

        }

        public static SysContactAddressTypeDTO ToDTO(this SysContactAddressType e)
        {
            if (e == null)
                return null;

            SysContactAddressTypeDTO sysContactAddressTypeDTO = new SysContactAddressTypeDTO()
            {
                SysContactAddressTypeId = (TermGroup_SysContactAddressType)e.SysContactAddressTypeId,
                SysContactTypeId = (TermGroup_SysContactType)e.SysContactTypeId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
            };
            return sysContactAddressTypeDTO;

        }

        public static List<SysContactAddressTypeDTO> ToDTOs(this List<SysContactAddressType> l)
        {
            var dtos = new List<SysContactAddressTypeDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;

        }

        public static SysContactAddressRowTypeDTO ToDTO(this SysContactAddressRowType e)
        {
            if (e == null)
                return null;

            SysContactAddressRowTypeDTO sysContactAddressRowTypeDTO = new SysContactAddressRowTypeDTO()
            {
                SysContactAddressRowTypeId = (TermGroup_SysContactAddressRowType)e.SysContactAddressRowTypeId,
                SysContactAddressTypeId = (TermGroup_SysContactAddressType)e.SysContactAddressTypeId,
                SysContactTypeId = e.SysContactTypeId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
            };
            return sysContactAddressRowTypeDTO;

        }

        public static List<SysContactAddressRowTypeDTO> ToDTOs(this List<SysContactAddressRowType> l)
        {
            var dtos = new List<SysContactAddressRowTypeDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;

        }

        public static SysContactEComTypeDTO ToDTO(this SysContactEComType e)
        {
            if (e == null)
                return null;

            SysContactEComTypeDTO sysContactEComTypeDTO = new SysContactEComTypeDTO()
            {
                SysContactEComTypeId = (TermGroup_SysContactEComType)e.SysContactEComTypeId,
                SysContactTypeId = (TermGroup_SysContactType)e.SysContactTypeId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
            };
            return sysContactEComTypeDTO;

        }

        public static List<SysContactEComTypeDTO> ToDTOs(this List<SysContactEComType> l)
        {
            var dtos = new List<SysContactEComTypeDTO>();

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

        #region SysCountry

        public static void PopulateName(this IEnumerable<SysCountry> sysCountries)
        {
            foreach (var item in sysCountries)
            {

            }
        }



        public static SysCountryDTO ToDTO(this SysCountry e)
        {
            if (e == null)
                return null;

            var sysCountryDTO = new SysCountryDTO
            {
                SysCountryId = (TermGroup_Country)e.SysCountryId,
                Code = e.Code,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
                SysCurrencyId = e.SysCurrencyId,
                AreaCode = e.AreaCode,
                Name = e.Name,
                CultureCode = e.CultureCode
            };
            return sysCountryDTO;
        }

        #endregion

        #region SysCurrency

        public static SysCurrencyDTO ToDTO(this SysCurrency e)
        {
            if (e == null)
                return null;

            SysCurrencyDTO sysCurrencyDTO = new SysCurrencyDTO()
            {
                SysCurrencyId = (TermGroup_Currency)e.SysCurrencyId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
            return sysCurrencyDTO;
        }

        public static List<SysCurrencyDTO> ToDTOs(this List<SysCurrency> l)
        {
            var dtos = new List<SysCurrencyDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;

        }

        public static SmallGenericType ToSmallGenericType(this SysCurrency e, bool useCode = false)
        {
            if (e == null)
                return null;

            SmallGenericType type = new SmallGenericType()
            {
                Id = e.SysCurrencyId,
                Name = useCode ? e.Code : e.Name
            };

            return type;
        }

        public static IEnumerable<SmallGenericType> ToSmallGenericTypes(this IEnumerable<SysCurrency> l, bool addEmptyRow = false, bool useCode = false)
        {
            var dtos = new List<SmallGenericType>();

            if (addEmptyRow)
            {
                dtos.Add(new SmallGenericType()
                {
                    Id = 0,
                    Name = " "
                });
            }

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallGenericType(useCode));
                }
            }
            return dtos;
        }

        #endregion

        #region SysDayType

        public static SysDayTypeDTO ToDTO(this SysDayType e)
        {
            if (e == null)
                return null;

            SysDayTypeDTO sysDayTypeDTO = new SysDayTypeDTO()
            {
                SysDayTypeId = e.SysDayTypeId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
                StandardWeekdayFrom = e.StandardWeekdayFrom,
                StandardWeekdayTo = e.StandardWeekdayTo,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
            return sysDayTypeDTO;

        }

        public static List<SysDayTypeDTO> ToDTOs(this List<SysDayType> l)
        {
            var dtos = new List<SysDayTypeDTO>();

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

        #region SysExtraField

        public static SysExtraFieldDTO ToDTO(this SysExtraField e)
        {
            if (e == null)
                return null;
            SysExtraFieldDTO sysExtraFieldDTO = new SysExtraFieldDTO()
            {
                SysExtraFieldId = e.SysExtraFieldId,
                Entity = (SoeEntityType)e.Entity,
                SysType = (SysExtraFieldType)e.SysType,
                Type = (TermGroup_ExtraFieldType)e.Type,
                Name = e.Name,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
            return sysExtraFieldDTO;
        }

        public static List<SysExtraFieldDTO> ToDTOs(this List<SysExtraField> l)
        {
            var dtos = new List<SysExtraFieldDTO>();

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

        #region SysFeature

        public static SysFeatureDTO ToDTO(this SysFeature e)
        {
            if (e == null)
                return null;

            SysFeatureDTO sysFeatureDTO = new SysFeatureDTO()
            {
                SysFeatureId = e.SysFeatureId,
                ParentFeatureId = e.SysFeature2?.SysFeatureId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Order = e.Order,
                Inactive = e.Inactive,
            };
            return sysFeatureDTO;

        }

        public static List<SysFeatureDTO> ToDTOs(this List<SysFeature> l)
        {
            var dtos = new List<SysFeatureDTO>();

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

        #region SysHelp

        public static SysHelpDTO ToDTO(this SysHelp e)
        {
            if (e == null)
                return null;

            SysHelpDTO dto = new SysHelpDTO()
            {
                SysHelpId = e.SysHelpId,
                SysLanguageId = e.SysLanguageId,
                SysFeatureId = e.SysFeatureId,
                VersionNr = e.VersionNr,
                Title = e.Title,
                Text = e.Text,
                PlainText = e.PlainText,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
            return dto;

        }

        public static List<SysHelpDTO> ToDTOs(this List<SysHelp> l)
        {
            var dtos = new List<SysHelpDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;
        }

        public static SysHelpSmallDTO ToSmallDTO(this SysHelp e)
        {
            if (e == null)
                return null;

            SysHelpSmallDTO dto = new SysHelpSmallDTO()
            {
                SysHelpId = e.SysHelpId,
                Title = e.Title,
                Text = e.Text,
                PlainText = e.PlainText,
                SysFeatureId = e.SysFeatureId
            };
            return dto;

        }

        public static List<SysHelpSmallDTO> ToSmallDTOs(this List<SysHelp> l)
        {
            var dtos = new List<SysHelpSmallDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region SysHoliday

        public static SysHolidayDTO ToDTO(this SysHoliday e)
        {
            if (e == null)
                return null;

            SysHolidayDTO sysHolidayDTO = new SysHolidayDTO()
            {
                SysHolidayId = e.SysHolidayId,
                SysDayTypeId = e.SysDayType.SysDayTypeId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
                Date = e.Date,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
            return sysHolidayDTO;

        }

        public static List<SysHolidayDTO> ToDTOs(this List<SysHoliday> l)
        {
            var dtos = new List<SysHolidayDTO>();

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

        #region SysHouseholdType

        public static SysHouseholdTypeDTO ToDTO(this SysHouseholdType e)
        {
            if (e == null)
                return null;

            SysHouseholdTypeDTO sysHouseholdTypeDTO = new SysHouseholdTypeDTO()
            {
                SysHouseholdTypeId = e.SysHouseholdTypeId,
                SysHouseholdTypeClassification = e.SysHouseholdTypeClassification,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
                XMLTagName = e.XMLTagName,
            };
            return sysHouseholdTypeDTO;

        }

        public static List<SysHouseholdTypeDTO> ToDTOs(this List<SysHouseholdType> l)
        {
            var dtos = new List<SysHouseholdTypeDTO>();

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

        #region SysInformation

        public static InformationDTO ToDTO(this SysInformation e, bool includeText)
        {
            if (e == null)
                return null;

            InformationDTO dto = new InformationDTO()
            {
                InformationId = e.SysInformationId,
                SysLanguageId = e.SysLanguageId,
                SourceType = SoeInformationSourceType.Sys,
                Type = (SoeInformationType)e.Type,
                Severity = (TermGroup_InformationSeverity)e.Severity,
                Subject = e.Subject,
                ShortText = e.ShortText,
                Folder = e.Folder,
                ValidFrom = e.ValidFrom,
                ValidTo = e.ValidTo,
                StickyType = (TermGroup_InformationStickyType)e.StickyType,
                NeedsConfirmation = e.NeedsConfirmation,
                ShowInWeb = e.ShowInWeb,
                ShowInMobile = e.ShowInMobile,
                ShowInTerminal = e.ShowInTerminal,
                Notify = e.Notify,
                NotificationSent = e.NotificationSent,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };

            // Relations
            dto.SysFeatureIds = e.SysInformationFeature?.Where(f => f.State == (int)SoeEntityState.Active).Select(f => f.SysFeatureId).ToList() ?? new List<int>();
            dto.SysCompDbIds = e.SysInformationSysCompDb?.Select(c => c.SysCompDbId).ToList() ?? new List<int>();
            dto.SysInformationSysCompDbs = e.SysInformationSysCompDb?.ToDTOs() ?? new List<SysInformationSysCompDbDTO>();

            // Extensions
            dto.Text = includeText ? e.Text : null;
            dto.HasText = !String.IsNullOrEmpty((e.Text));


            return dto;
        }

        public static List<InformationDTO> ToDTOs(this IEnumerable<SysInformation> l, bool includeText)
        {
            List<InformationDTO> dtos = new List<InformationDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeText));
                }
            }
            return dtos;
        }

        public static InformationGridDTO ToGridDTO(this SysInformation e)
        {
            if (e == null)
                return null;

            InformationGridDTO dto = new InformationGridDTO()
            {
                InformationId = e.SysInformationId,
                Severity = (TermGroup_InformationSeverity)e.Severity,
                Subject = e.Subject,
                ShortText = e.ShortText,
                Folder = e.Folder,
                ValidFrom = e.ValidFrom,
                ValidTo = e.ValidTo,
                NeedsConfirmation = e.NeedsConfirmation,
                ShowInWeb = e.ShowInWeb,
                ShowInMobile = e.ShowInMobile,
                ShowInTerminal = e.ShowInTerminal,
                Notify = e.Notify,
                NotificationSent = e.NotificationSent
            };

            // Extensions
            dto.SeverityName = e.SeverityName;

            return dto;
        }

        public static List<InformationGridDTO> ToGridDTOs(this IEnumerable<SysInformation> l)
        {
            List<InformationGridDTO> dtos = new List<InformationGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static SysInformationSysCompDbDTO ToDTO(this SysInformationSysCompDb e)
        {
            if (e == null)
                return null;

            SysInformationSysCompDbDTO dto = new SysInformationSysCompDbDTO()
            {
                SysCompDbId = e.SysCompDbId,
                NotificationSent = e.NotificationSent
            };

            // Extensions
            dto.SiteName = e.SiteName;

            return dto;
        }

        public static List<SysInformationSysCompDbDTO> ToDTOs(this IEnumerable<SysInformationSysCompDb> l)
        {
            List<SysInformationSysCompDbDTO> dtos = new List<SysInformationSysCompDbDTO>();
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

        #region SysLanguage

        public static SysLanguageDTO ToDTO(this SysLanguage e)
        {
            if (e == null)
                return null;

            SysLanguageDTO sysLanguageDTO = new SysLanguageDTO()
            {
                SysLanguageId = e.SysLanguageId,
                LangCode = e.LangCode,
                Name = e.Name,
                ShortName = e.ShortName,
                Translated = e.Translated,
            };
            return sysLanguageDTO;

        }

        public static List<SysLanguageDTO> ToDTOs(this List<SysLanguage> l)
        {
            var dtos = new List<SysLanguageDTO>();

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

        #region SysLBError

        public static SysLbErrorDTO ToDTO(this SysLbError e)
        {
            if (e == null)
                return null;

            SysLbErrorDTO sysLbErrorDTO = new SysLbErrorDTO()
            {
                SysErrorId = e.SysErrorId,
                LbErrorCode = e.LbErrorCode,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
            };
            return sysLbErrorDTO;

        }

        public static List<SysLbErrorDTO> ToDTOs(this List<SysLbError> l)
        {
            var dtos = new List<SysLbErrorDTO>();

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

        #region SysLinkTable

        public static SysLinkDTO ToDTO(this SysLinkTable e, string valueName)
        {
            return new SysLinkDTO()
            {
                SysLinkTableKeyItemId = e.SysLinkTableKeyItemId,
                SysLinkTableIntegerValue = e.SysLinkTableIntegerValue,
                SysLinkTableIntegerValueName = valueName,
                SysLinkTableRecordType = (int)(SysLinkTableRecordType)e.SysLinkTableRecordType,
                SysLinkTableIntegerValueType = (int)(SysLinkTableIntegerValueType)e.SysLinkTableIntegerValueType,
            };
        }

        #endregion

        #region SysLog

        public static SysLogDTO ToDTO(this SysLog e)
        {
            if (e == null)
                return null;

            SysLogDTO dto = new SysLogDTO()
            {
                SysLogId = e.SysLogId,
                Date = e.Date,
                Level = e.Level,
                Message = e.Message,
                Exception = e.Exception,

                LicenseId = e.LicenseId.EmptyToNull(),
                LicenseNr = e.LicenseNr.EmptyToNull(),
                ActorCompanyId = e.ActorCompanyId.EmptyToNull(),
                CompanyName = e.CompanyName.EmptyToNull(),
                RoleId = e.RoleId.EmptyToNull(),
                RoleName = e.RoleName.EmptyToNull(),
                UserId = e.UserId.EmptyToNull(),
                LoginName = e.LoginName.EmptyToNull(),

                RecorId = e.RecordId,
                TaskWatchLogId = e.TaskWatchLogId,
                TaskWatchLogStart = e.TaskWatchLogStart.EmptyToNull(),
                TaskWatchLogStop = e.TaskWatchLogStop.EmptyToNull(),
                TaskWatchLogName = e.TaskWatchLogName.EmptyToNull(),
                TaskWatchLogParameters = e.TaskWatchLogParameters.EmptyToNull(),

                Application = e.Application.EmptyToNull(),
                From = e.Form.EmptyToNull(),
                HostName = e.HostName.EmptyToNull(),
                IpNr = e.IpNr.EmptyToNull(),
                LineNumber = e.LineNumber.EmptyToNull(),
                LogClass = e.LogClass.EmptyToNull(),
                Logger = e.Logger.EmptyToNull(),
                ReferUri = e.ReferUri.EmptyToNull(),
                RequestUri = e.RequestUri.EmptyToNull(),
                Session = e.Session.EmptyToNull(),
                Source = e.Source.EmptyToNull(),
                TargetSite = e.TargetSite.EmptyToNull(),
                Thread = e.Thread.EmptyToNull(),
            };

            return dto;
        }

        public static IEnumerable<SysLogDTO> ToDTOs(this IEnumerable<SysLog> l)
        {
            var dtos = new List<SysLogDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SysLogGridDTO ToGridDTO(this SysLog e)
        {
            if (e == null)
                return null;

            SysLogGridDTO dto = new SysLogGridDTO()
            {
                SysLogId = e.SysLogId,
                Date = e.Date,
                Level = e.Level,
                Message = e.Message,
                StackTrace = e.Exception,
                CompanyName = e.CompanyName,
                UniqueCounter = e.UniqueCounter > 0 ? e.UniqueCounter : 1,
            };

            return dto;
        }

        public static IEnumerable<SysLogGridDTO> ToGridDTOs(this IEnumerable<SysLog> l)
        {
            var dtos = new List<SysLogGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static List<SysLog> GetUnique(this List<SysLog> l)
        {
            if (l.IsNullOrEmpty())
                return new List<SysLog>();

            List<SysLog> sysLogs = new List<SysLog>();
            foreach (var sysLogsByUnqiue in l.GroupBy(i => i.UniqueError))
            {
                SysLog sysLog = sysLogsByUnqiue.First();
                sysLog.UniqueCounter = sysLogsByUnqiue.Count();
                sysLog.LicenseNr = sysLogsByUnqiue.Count(i => i.LicenseNr == sysLog.LicenseNr) == sysLog.UniqueCounter ? sysLog.LicenseNr : string.Empty;
                sysLog.ActorCompanyId = sysLogsByUnqiue.Count(i => i.ActorCompanyId == sysLog.ActorCompanyId) == sysLog.UniqueCounter ? sysLog.ActorCompanyId : null;
                sysLog.CompanyName = sysLogsByUnqiue.Count(i => i.CompanyName == sysLog.CompanyName) == sysLog.UniqueCounter ? sysLog.CompanyName : string.Empty;
                sysLog.UserId = sysLogsByUnqiue.Count(i => i.UserId == sysLog.UserId) == sysLog.UniqueCounter ? sysLog.UserId : null;
                sysLog.UserName = sysLogsByUnqiue.Count(i => i.UserName == sysLog.UserName) == sysLog.UniqueCounter ? sysLog.UserName : string.Empty;
                sysLog.RoleId = sysLogsByUnqiue.Count(i => i.RoleId == sysLog.RoleId) == sysLog.UniqueCounter ? sysLog.RoleId : null;
                sysLog.RoleTermId = sysLogsByUnqiue.Count(i => i.RoleTermId == sysLog.RoleTermId) == sysLog.UniqueCounter ? sysLog.RoleTermId : null;
                sysLog.RoleName = sysLogsByUnqiue.Count(i => i.RoleName == sysLog.RoleName) == sysLog.UniqueCounter ? sysLog.RoleName : string.Empty;
                sysLog.IpNr = sysLogsByUnqiue.Count(i => i.IpNr == sysLog.IpNr) == sysLog.UniqueCounter ? sysLog.IpNr : string.Empty;
                sysLogs.Add(sysLog);
            }
            return sysLogs.OrderByDescending(s => s.UniqueCounter).ToList();
        }

        #endregion

        #region SysMedia

        public static SysMediaDTO ToDTO(this SysMedia e)
        {
            if (e == null)
                return null;

            SysMediaDTO dto = new SysMediaDTO()
            {
                SysMediaId = e.SysMediaId,
                SysLanguageId = e.SysLanguageId,
                Type = (TermGroup_SysMediaType)e.Type,
                MediaType = (TermGroup_MediaType)e.MediaType,
                FormatType = (TermGroup_MediaFormat)e.FormatType,
                Name = e.Name,
                Description = e.Description,
                Filename = e.Filename,
                Path = e.Path,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            dto.TypeName = e.TypeName;

            return dto;
        }

        public static IEnumerable<SysMediaDTO> ToDTOs(this IEnumerable<SysMedia> l)
        {
            var dtos = new List<SysMediaDTO>();
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

        #region SysNews

        public static SysNewsSmallDTO ToSmallDTO(this SysNews e, bool includeDescription)
        {
            if (e == null)
                return null;

            SysNewsSmallDTO dto = new SysNewsSmallDTO()
            {
                SysNewsId = e.SysNewsId,
                PubDate = e.PubDate,
                Title = e.Title,
                Preview = e.Preview
            };

            if (includeDescription)
                dto.Description = e.Description;

            return dto;
        }

        public static IEnumerable<SysNewsSmallDTO> ToSmallDTOs(this IEnumerable<SysNews> l, bool includeDescription)
        {
            var dtos = new List<SysNewsSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO(includeDescription));
                }
            }
            return dtos;
        }

        public static SysNewsDTO ToDTO(this SysNews e)
        {
            if (e == null)
                return null;

            SysNewsDTO dto = new SysNewsDTO()
            {
                SysNewsId = e.SysNewsId,
                SysLanguageId = e.SysLanguageId,
                SysXEArticleId = e.SysXEArticleId,
                PubDate = e.PubDate,
                Title = e.Title,
                Description = e.Description,
                Preview = e.Preview,
                Link = e.Link,
                Author = e.Author,
                IsPublic = e.IsPublic,
                AttachmentFileName = e.AttachmentFileName,
                AttachmentImageSrc = e.AttachmentImageSrc,
                AttachmentExportType = e.AttachmentExportType,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<SysNewsDTO> ToDTOs(this IEnumerable<SysNews> l)
        {
            var dtos = new List<SysNewsDTO>();
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

        #region SysReportTemplate

        public static ReportTemplateDTO ToDTO(this SysReportTemplate e)
        {
            if (e == null)
                return null;

            ReportTemplateDTO dto = new ReportTemplateDTO()
            {
                ReportTemplateId = e.SysReportTemplateId,
                ActorCompanyId = null,
                SysReportTemplateTypeId = e.SysReportTemplateTypeId,
                SysReportTypeId = e.SysReportType != null ? e.SysReportType.SysReportTypeId : (int?)null,
                SysCountryIds = e.SysCountryIds,
                Module = e.SysReportTemplateType != null ? (SoeModule)e.SysReportTemplateType.Module : SoeModule.None,
                IsSystem = false,

                Name = e.Name,
                Description = e.Description,
                ReportNr = e.ReportNr,
                FileName = e.FileName,
                GroupByLevel1 = e.GroupByLevel1,
                GroupByLevel2 = e.GroupByLevel2,
                GroupByLevel3 = e.GroupByLevel3,
                GroupByLevel4 = e.GroupByLevel4,
                SortByLevel1 = e.SortByLevel1,
                SortByLevel2 = e.SortByLevel2,
                SortByLevel3 = e.SortByLevel3,
                SortByLevel4 = e.SortByLevel4,
                Special = e.Special,
                IsSortAscending = e.IsSortAscending,
                ShowGroupingAndSorting = e.ShowGroupingAndSorting,
                ShowOnlyTotals = e.ShowOnlyTotals,
                ValidExportTypes = e.GetValidExportTypes(),
                IsSystemReport = e.IsSystemReport,
                ReportTemplateSettings = e.SysReportTemplateSettings?.ToDTOs().ToList() ?? new List<ReportTemplateSettingDTO>(),

                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = SoeEntityState.Active,
            };

            return dto;
        }

        public static IEnumerable<ReportTemplateSettingDTO> ToDTOs(this IEnumerable<SysReportTemplateSetting> l)
        {
            var dtos = new List<ReportTemplateSettingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ReportTemplateSettingDTO ToDTO(this SysReportTemplateSetting e)
        {
            if (e == null)
                return null;
            ReportTemplateSettingDTO dto = new ReportTemplateSettingDTO()
            {
                ReportTemplateSettingId = e.SysReportTemplateSettingId,
                ReportTemplateId = e.SysReportTemplateId,
                SettingField = e.SettingField,
                SettingValue = e.SettingValue,
                SettingType = e.SettingType
            };
            return dto;
        }

        public static IEnumerable<ReportTemplateDTO> ToDTOs(this IEnumerable<SysReportTemplate> l)
        {
            var dtos = new List<ReportTemplateDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<int> GetValidExportTypes(this SysReportTemplate e)
        {
            return ReportTemplateDTO.GetValidExportTypes(e?.ValidExportTypes, e != null ? (SoeReportType)e?.SysReportTypeId : SoeReportType.CrystalReport);
        }

        public static bool IsValid(this SysReportTemplate e, TermGroup_ReportExportType exportType)
        {
            return e.GetValidExportTypes().Contains((int)exportType);
        }

        #endregion

        #region SysReportTemplateType

        public static SysReportTemplateTypeDTO ToDTO(this SysReportTemplateType e)
        {
            if (e == null)
                return null;

            SysReportTemplateTypeDTO sysReportTemplateTypeDTO = new SysReportTemplateTypeDTO()
            {
                SysReportTemplateTypeId = e.SysReportTemplateTypeId,
                SysReportTermId = e.SysReportTermId,
                SelectionType = e.SelectionType,
                GroupMapping = e.GroupMapping,
                Module = e.Module,
                Group = e.Group
            };
            return sysReportTemplateTypeDTO;

        }

        public static List<SysReportTemplateTypeDTO> ToDTOs(this List<SysReportTemplateType> l)
        {
            var dtos = new List<SysReportTemplateTypeDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;

        }

        public static string GetGroupName(this List<SysReportTemplateType> l, List<GenericType> sysReportTemplateGroups, int sysReportTemplateTypeId, bool addPostfix = true)
        {
            SysReportTemplateType sysReportTemplateType = l?.FirstOrDefault(i => i.SysReportTemplateTypeId == sysReportTemplateTypeId);
            if (sysReportTemplateType == null)
                return string.Empty;

            GenericType sysReportTemplateGroup = sysReportTemplateGroups?.FirstOrDefault(i => i.Id == sysReportTemplateType.Group);
            if (sysReportTemplateGroup == null)
                return string.Empty;

            return sysReportTemplateGroup.Name;
        }

        #endregion

        #region SysReportType

        public static SysReportTypeDTO ToDTO(this SysReportType e)
        {
            if (e == null)
                return null;

            SysReportTypeDTO sysReportTypeDTO = new SysReportTypeDTO()
            {
                SysReportTypeId = e.SysReportTypeId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
                FileExtension = e.FileExtension,
            };
            return sysReportTypeDTO;

        }

        public static List<SysReportTypeDTO> ToDTOs(this List<SysReportType> l)
        {
            var dtos = new List<SysReportTypeDTO>();

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

        #region SysSetting

        public static SysSettingDTO ToDTO(this SysSetting e)
        {
            if (e == null)
                return null;

            SysSettingDTO sysSettingDTO = new SysSettingDTO()
            {
                SysSettingId = e.SysSettingId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
                SysSettingTypeId = e.SysSettingType.SysSettingTypeId,
            };
            return sysSettingDTO;

        }

        public static List<SysSettingDTO> ToDTOs(this List<SysSetting> l)
        {
            var dtos = new List<SysSettingDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;

        }


        #region SysSettingType

        public static SysSettingTypeDTO ToDTO(this SysSettingType e)
        {
            if (e == null)
                return null;

            SysSettingTypeDTO sysSettingTypeDTO = new SysSettingTypeDTO()
            {
                SysSettingTypeId = e.SysSettingTypeId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
            };
            return sysSettingTypeDTO;

        }

        public static List<SysSettingTypeDTO> ToDTOs(this List<SysSettingType> l)
        {
            var dtos = new List<SysSettingTypeDTO>();

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

        #region SysPageStatus

        public static SysPageStatusDTO ToDTO(this SysPageStatus e)
        {
            if (e == null)
                return null;

            SysPageStatusDTO dto = new SysPageStatusDTO()
            {
                SysPageStatusId = e.SysPageStatusId,
                SysFeatureId = e.SysFeatureId,
                BetaStatus = (TermGroup_SysPageStatusStatusType)e.BetaStatus,
                LiveStatus = (TermGroup_SysPageStatusStatusType)e.LiveStatus,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            // Extensions
            dto.PageName = e.PageName;
            dto.BetaStatusName = e.BetaStatusName;
            dto.LiveStatusName = e.LiveStatusName;

            return dto;
        }

        public static IEnumerable<SysPageStatusDTO> ToDTOs(this IEnumerable<SysPageStatus> l)
        {
            var dtos = new List<SysPageStatusDTO>();
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

        #region SysPaymentMethod

        public static SysPaymentMethodDTO ToDTO(this SysPaymentMethod e)
        {
            if (e == null)
                return null;

            SysPaymentMethodDTO dto = new SysPaymentMethodDTO()
            {
                SysPaymentMethodId = e.SysPaymentMethodId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId
            };

            return dto;
        }

        public static IEnumerable<SysPaymentMethodDTO> ToDTOs(this IEnumerable<SysPaymentMethod> l)
        {
            var dtos = new List<SysPaymentMethodDTO>();
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

        #region SysPaymentType

        public static SysPaymentTypeDTO ToDTO(this SysPaymentType e)
        {
            if (e == null)
                return null;

            SysPaymentTypeDTO sysPaymentTypeDTO = new SysPaymentTypeDTO()
            {
                SysPaymentTypeId = e.SysPaymentTypeId,
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
            };
            return sysPaymentTypeDTO;

        }

        public static List<SysPaymentTypeDTO> ToDTOs(this List<SysPaymentType> l)
        {
            var dtos = new List<SysPaymentTypeDTO>();

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

        #region SysPayrollPrice

        public static SysPayrollPriceDTO ToDTO(this SysPayrollPrice e, bool includeIntervals)
        {
            if (e == null)
                return null;

            SysPayrollPriceDTO dto = new SysPayrollPriceDTO()
            {
                SysPayrollPriceId = e.SysPayrollPriceId,
                SysCountryId = e.SysCountryId,
                SysTermId = e.SysTermId,
                Type = (TermGroup_SysPayrollPriceType)e.Type,
                Code = e.Code,
                Amount = e.Amount,
                AmountType = (TermGroup_SysPayrollPriceAmountType)e.AmountType,
                FromDate = e.FromDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Relations
            if (includeIntervals)
            {
                dto.Intervals = new List<SysPayrollPriceIntervalDTO>();
                if (e.SysPayrollPriceInterval != null && e.SysPayrollPriceInterval.Any(i => i.State == (int)SoeEntityState.Active))
                    dto.Intervals = e.SysPayrollPriceInterval.Where(i => i.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }

            // Extensions
            dto.Name = e.Name;
            dto.TypeName = e.TypeName;
            dto.AmountTypeName = e.AmountTypeName;

            return dto;
        }

        public static IEnumerable<SysPayrollPriceDTO> ToDTOs(this IEnumerable<SysPayrollPrice> l, bool includeIntervals)
        {
            var dtos = new List<SysPayrollPriceDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeIntervals));
                }
            }
            return dtos;
        }

        public static SysPayrollPriceDTO ToDTO(this SysPayrollPriceViewDTO e)
        {
            if (e == null)
                return null;

            SysPayrollPriceDTO dto = new SysPayrollPriceDTO()
            {
                SysPayrollPriceId = e.SysPayrollPriceId,
                SysCountryId = e.SysCountryId,
                SysTermId = e.SysTermId,
                Type = (TermGroup_SysPayrollPriceType)e.Type,
                Code = e.Code,
                Amount = e.Amount,
                AmountType = (TermGroup_SysPayrollPriceAmountType)e.AmountType,
                FromDate = e.FromDate,
            };

            // Extensions
            dto.Name = e.Name;

            return dto;
        }

        public static IEnumerable<SysPayrollPriceDTO> ToDTOs(this IEnumerable<SysPayrollPriceViewDTO> l)
        {
            var dtos = new List<SysPayrollPriceDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SysPayrollPriceIntervalDTO ToDTO(this SysPayrollPriceInterval e)
        {
            if (e == null)
                return null;

            SysPayrollPriceIntervalDTO dto = new SysPayrollPriceIntervalDTO()
            {
                SysPayrollPriceIntervalId = e.SysPayrollPriceIntervalId,
                SysPayrollPriceId = e.SysPayrollPriceId,
                FromInterval = e.FromInterval,
                ToInterval = e.ToInterval,
                Amount = e.Amount,
                AmountType = (TermGroup_SysPayrollPriceAmountType)e.AmountType,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (e.SysPayrollPrice != null)
                dto.SysPayrollPrice = (TermGroup_SysPayrollPrice)e.SysPayrollPrice.SysTermId;

            // Extensions
            dto.AmountTypeName = e.AmountTypeName;

            return dto;
        }

        public static IEnumerable<SysPayrollPriceIntervalDTO> ToDTOs(this IEnumerable<SysPayrollPriceInterval> l)
        {
            var dtos = new List<SysPayrollPriceIntervalDTO>();
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

        #region SysPermission

        public static SysPermissionDTO ToDTO(this SysPermission e)
        {
            if (e == null)
                return null;

            SysPermissionDTO sysPermissionDTO = new SysPermissionDTO()
            {
                SysPermissionId = e.SysPermissionId,
                Name = e.Name,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
            return sysPermissionDTO;

        }

        public static List<SysPermissionDTO> ToDTOs(this List<SysPermission> l)
        {
            var dtos = new List<SysPermissionDTO>();

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

        #region SysPriceList

        public static SysPriceListDTO ToDTO(this SysPriceList e)
        {
            if (e == null)
                return null;

            SysPriceListDTO sysPriceListDTO = new SysPriceListDTO()
            {
                SysPriceListId = e.SysPriceListId,
                SysPriceListHeadId = e.SysPriceListHeadId,
                SysProductId = e.SysProductId,
                GNP = e.GNP,
                PurchaseUnit = e.PurchaseUnit,
                SalesUnit = e.SalesUnit,
                EnvironmentFee = e.EnvironmentFee,
                Storage = e.Storage,
                ReplacesProduct = e.ReplacesProduct,
                PackageSizeMin = e.PackageSizeMin,
                PackageSize = e.PackageSize,
                ProductLink = e.ProductLink,
                PriceChangeDate = e.PriceChangeDate,
                SysWholesellerId = e.SysWholesellerId,
                PriceStatus = e.PriceStatus,
                Code = e.Code,
            };
            return sysPriceListDTO;

        }

        public static List<SysPriceListDTO> ToDTOs(this List<SysPriceList> l)
        {
            var dtos = new List<SysPriceListDTO>();

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

        #region SysProductDTO
        public static List<SysProductDTO> ToDTOs(this List<SysProduct> l)
        {
            var dtos = new List<SysProductDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;
        }

        public static SysProductDTO ToDTO(this SysProduct e)
        {
            if (e == null)
                return null;

            var dto = new SysProductDTO()
            {
                SysProductId = e.SysProductId,
                Name = e.Name,
                EAN = e.EAN,
                ProductId = e.ProductId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Type = e.Type,
                ExternalId = e.ExternalId ?? 0,
                ExtendedInfo = e.ExtendedInfo,
                ImageFileName = e.ImageFileName,
                SysCountryId = (TermGroup_Country)e.SysCountryId,

            };

            return dto;
        }

        #endregion

        #region SysPosition

        public static SysPositionDTO ToDTO(this SysPosition e)
        {
            if (e == null)
                return null;

            SysPositionDTO dto = new SysPositionDTO()
            {
                SysPositionId = e.SysPositionId,
                SysCountryId = e.SysCountryId,
                SysLanguageId = e.SysLanguageId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                SysCountryCode = e.SysCountry.Code,
                SysLanguageCode = e.SysLanguage.LangCode,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<SysPositionDTO> ToDTOs(this IEnumerable<SysPosition> l)
        {
            var dtos = new List<SysPositionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SysPositionGridDTO ToGridDTO(this SysPosition e)
        {
            if (e == null)
                return null;

            SysPositionGridDTO dto = new SysPositionGridDTO()
            {
                SysPositionId = e.SysPositionId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
            };

            // Extensions
            if (e.SysCountry != null)
                dto.SysCountryCode = e.SysCountry.Code;
            if (e.SysLanguage != null)
                dto.SysLanguageCode = e.SysLanguage.LangCode;

            dto.IsLinked = e.IsLinked;

            return dto;
        }

        public static IEnumerable<SysPositionGridDTO> ToGridDTOs(this IEnumerable<SysPosition> l)
        {
            var dtos = new List<SysPositionGridDTO>();
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

        #region SysTerm

        public static SysTermDTO ToDTO(this SysTerm e)
        {
            if (e == null)
                return null;

            SysTermDTO dto = new SysTermDTO()
            {
                SysTermId = e.SysTermId,
                SysTermGroupId = e.SysTermGroupId,
                LangId = e.LangId,
                Name = e.Name.NullToEmpty(),
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                TranslationKey = e.TranslationKey.NullToEmpty(),
            };

            return dto;
        }

        public static IEnumerable<SysTermDTO> ToDTOs(this IEnumerable<SysTerm> l)
        {
            var dtos = new List<SysTermDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<SysTermDTO> ToDTOs(this List<SysTerm> l)
        {
            var dtos = new List<SysTermDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static IQueryable<SysTerm> Where(this IQueryable<SysTerm> query, WildCard dateWildcard, DateTime date)
        {
            if (dateWildcard == WildCard.Equals)
            {
                // equals is special since it linq to entities does not support DateTime.Date
                var greaterThan = date.Date;
                var smallerThan = date.Date.AddDays(1);
                query = query.Where(st => (st.Modified.HasValue ? st.Modified.Value > greaterThan && st.Modified.Value < smallerThan : st.Created.Value > greaterThan && st.Created.Value < smallerThan));
            }
            if (dateWildcard == WildCard.GreaterThan)
                query = query.Where(st => (st.Modified.HasValue ? st.Modified > date : st.Created > date));
            if (dateWildcard == WildCard.GreaterThanOrEquals)
                query = query.Where(st => (st.Modified.HasValue ? st.Modified >= date : st.Created >= date));
            if (dateWildcard == WildCard.LessThan)
                query = query.Where(st => (st.Modified.HasValue ? st.Modified < date : st.Created < date));
            if (dateWildcard == WildCard.LessThanOrEquals)
                query = query.Where(st => (st.Modified.HasValue ? st.Modified <= date : st.Created <= date));
            if (dateWildcard == WildCard.NotEquals)
                query = query.Where(st => (st.Modified.HasValue ? st.Modified == date : st.Created == date));

            return query;
        }

        #endregion

        #region SysTermGroup

        public static SysTermGroupDTO ToDTO(this SysTermGroup e)
        {
            if (e == null)
                return null;

            SysTermGroupDTO dto = new SysTermGroupDTO()
            {
                SysTermGroupId = e.SysTermGroupId,
                Name = e.Name,
                Description = e.Description
            };

            return dto;
        }

        public static IEnumerable<SysTermGroupDTO> ToDTOs(this IEnumerable<SysTermGroup> l)
        {
            var dtos = new List<SysTermGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<SysTermGroupDTO> ToDTOs(this List<SysTermGroup> l)
        {
            var dtos = new List<SysTermGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SmallGenericType ToSmallGenericType(this SysTermGroup e)
        {
            if (e == null)
                return null;

            return new SmallGenericType(e.SysTermGroupId, e.SysTermGroupId + ". " + e.Name);
        }

        public static IEnumerable<SmallGenericType> ToSmallGenericTypes(this IEnumerable<SysTermGroup> l)
        {
            var dtos = new List<SmallGenericType>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallGenericType());
                }
            }
            return dtos;
        }

        #endregion

        #region SysTimeInterval

        public static SysTimeIntervalDTO ToDTO(this SysTimeInterval e)
        {
            if (e == null)
                return null;

            SysTimeIntervalDTO dto = new SysTimeIntervalDTO()
            {
                SysTimeIntervalId = e.SysTimeIntervalId,
                SysTermId = e.SysTermId,
                Period = (TermGroup_TimeIntervalPeriod)e.Period,
                Start = (TermGroup_TimeIntervalStart)e.Start,
                StartOffset = e.StartOffset,
                Stop = (TermGroup_TimeIntervalStop)e.Stop,
                StopOffset = e.StopOffset,
                Sort = e.Sort,
                Name = e.Name,
            };

            return dto;
        }

        public static List<SysTimeIntervalDTO> ToDTOs(this List<SysTimeInterval> l)
        {
            List<SysTimeIntervalDTO> dtos = new List<SysTimeIntervalDTO>();

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

        #region SysVatCode

        public static SysVatAccountDTO ToDTO(this SysVatAccount e)
        {
            if (e == null)
                return null;

            SysVatAccountDTO sysVatAccountDTO = new SysVatAccountDTO()
            {
                SysVatAccountId = e.SysVatAccountId,
                AccountCode = e.AccountCode,
                VatCode = e.VatCode,
                LangId = e.LangId,
                VatNr1 = e.VatNr1,
                VatNr2 = e.VatNr2,
                Name = e.Name,
            };
            return sysVatAccountDTO;

        }

        public static List<SysVatAccountDTO> ToDTOs(this List<SysVatAccount> l)
        {
            var dtos = new List<SysVatAccountDTO>();

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

        #region SysVatRate

        public static SysVatRateDTO ToDTO(this SysVatRate e)
        {
            if (e == null)
                return null;

            SysVatRateDTO sysVatRateDTO = new SysVatRateDTO()
            {
                SysVatAccountId = e.SysVatAccountId,
                VatRate = e.VatRate,
                Date = e.Date,
                IsActive = e.IsActive,
            };
            return sysVatRateDTO;

        }

        public static List<SysVatRateDTO> ToDTOs(this List<SysVatRate> l)
        {
            var dtos = new List<SysVatRateDTO>();

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

        #region SysVehicleType

        public static SysVehicleTypeDTO ToDTO(this SysVehicleType e)
        {
            if (e == null)
                return null;

            SysVehicleTypeDTO dto = new SysVehicleTypeDTO()
            {
                SysVehicleTypeId = e.SysVehicleTypeId,
                Filename = e.Filename,
                ManufacturingYear = e.ManufacturingYear,
                XML = e.XML,
                DateFrom = e.DateFrom,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<SysVehicleTypeDTO> ToDTOs(this IEnumerable<SysVehicleType> l)
        {
            var dtos = new List<SysVehicleTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SysVehicleTypeGridDTO ToGridDTO(this SysVehicleType e)
        {
            if (e == null)
                return null;

            SysVehicleTypeGridDTO dto = new SysVehicleTypeGridDTO()
            {
                SysVehicleTypeId = e.SysVehicleTypeId,
                ManufacturingYear = e.ManufacturingYear,
                DateFrom = e.DateFrom,
                Created = e.Created,
                CreatedBy = e.CreatedBy
            };

            return dto;
        }

        public static IEnumerable<SysVehicleTypeGridDTO> ToGridDTOs(this IEnumerable<SysVehicleType> l)
        {
            var dtos = new List<SysVehicleTypeGridDTO>();
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

        #region SysWholeseller

        public static SysWholesellerDTO ToDTO(this SysWholeseller e)
        {
            if (e == null)
                return null;

            var dto = new SysWholesellerDTO
            {
                SysWholesellerId = e.SysWholesellerId,
                Name = e.Name,
                Type = e.Type,
                SysCountryId = e.SysCountryId,
                SysCurrencyId = e.SysCurrencyId,
                IsOnlyInComp = e.IsOnlyInComp,
                SysWholesellerEdiId = e.SysWholesellerEdiId,
                HasEdiFeature = e.SysWholesellerEdi != null && e.SysWholesellerEdi.SysEdiMsg.Count > 0,
            };

            if (e.SysWholesellerEdi != null && e.SysWholesellerEdi.SysEdiMsg != null)
                dto.MessageTypes = e.SysWholesellerEdi.SysEdiMsg.Select(m => m.SysEdiType.TypeName).Distinct().JoinToString(",");

            return dto;
        }

        public static IEnumerable<SysWholesellerDTO> ToDTOs(this IEnumerable<SysWholeseller> l)
        {
            var dtos = new List<SysWholesellerDTO>();
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

        #region SysXEArticle

        public static SysXEArticleDTO ToDTO(this SysXEArticle e)
        {
            if (e == null)
                return null;

            SysXEArticleDTO sysXEArticleDTO = new SysXEArticleDTO()
            {
                SysXEArticleId = e.SysXEArticleId,
                Name = e.Name,
                ArticleNr = e.ArticleNr,
                Description = e.Description,
                Inactive = e.Inactive,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                ModuleGroup = e.ModuleGroup,
                ArticleNrYear1 = e.ArticleNrYear1,
                ArticleNrYear2 = e.ArticleNrYear2,
                StartPrice = e.StartPrice,
                MonthlyPrice = e.MonthlyPrice,
            };

            if (e.SysXEArticleFeature != null)
                sysXEArticleDTO.SysXEArticleFeatures = e.SysXEArticleFeature.ToList().ToDTOs();

            return sysXEArticleDTO;
        }

        public static List<SysXEArticleDTO> ToDTOs(this List<SysXEArticle> l)
        {
            var dtos = new List<SysXEArticleDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;

        }

        #region SysXEArticleFeature

        public static SysXEArticleFeatureDTO ToDTO(this SysXEArticleFeature e)
        {
            if (e == null)
                return null;

            SysXEArticleFeatureDTO sysXEArticleFeatureDTO = new SysXEArticleFeatureDTO()
            {
                SysXEArticleId = e.SysXEArticleId,
                SysFeatureId = e.SysFeatureId,
                SysPermissionId = e.SysPermissionId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
            return sysXEArticleFeatureDTO;

        }

        public static List<SysXEArticleFeatureDTO> ToDTOs(this List<SysXEArticleFeature> l)
        {
            var dtos = new List<SysXEArticleFeatureDTO>();

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

        #endregion

        #region Views

        #region SysPayrollTypeView

        public static SysPayrollTypeViewDTO ToDTO(this SysPayrollTypeView e)
        {
            if (e == null)
                return null;

            SysPayrollTypeViewDTO dto = new SysPayrollTypeViewDTO()
            {
                SysTermId = e.SysTermId,
                ParentId = e.ParentId,
                Name = e.Name
            };

            return dto;
        }

        public static IEnumerable<SysPayrollTypeViewDTO> ToDTOs(this IEnumerable<SysPayrollTypeView> l)
        {
            var dtos = new List<SysPayrollTypeViewDTO>();
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

        #region SysReportTemplateView

        public static SysReportTemplateViewDTO ToDTO(this SysReportTemplateView e)
        {
            if (e == null)
                return null;

            SysReportTemplateViewDTO dto = new SysReportTemplateViewDTO()
            {
                SysReportTemplateId = e.SysReportTemplateId,
                SysReportTypeId = e.SysReportTypeId,
                SysTemplateTypeId = e.SysTemplateTypeId,
                Name = e.Name,
                Description = e.Description,
                SysReportTermId = e.SysReportTermId,
                SelectionType = e.SelectionType,
                GroupMapping = e.GroupMapping,
                Module = (SoeModule)e.Module


            };

            return dto;
        }

        public static IEnumerable<SysReportTemplateViewDTO> ToDTOs(this IEnumerable<SysReportTemplateView> l)
        {
            var dtos = new List<SysReportTemplateViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SysReportTemplateViewGridDTO ToGridDTO(this (SysReportTemplateView Template, List<SysLinkTable> CountryLinks) e)
        {
            if (e.Template == null)
                return null;

            SysReportTemplateViewGridDTO dto = new SysReportTemplateViewGridDTO()
            {
                SysReportTemplateId = e.Template.SysReportTemplateId,
                SysReportTemplateTypeName = e.Template.SysReportTemplateTypeName,
                ReportNr = e.Template.ReportNr,
                GroupName = e.Template.GroupName,
                Name = e.Template.Name,
                Description = e.Template.Description,
                SysCountryIds = e.CountryLinks.Select(x => x.SysLinkTableIntegerValue).ToList(),
                IsSystemReport = e.Template.IsSystemReport,
            };
            return dto;
        }

        public static IEnumerable<SysReportTemplateViewGridDTO> ToGridDTOs(this IEnumerable<(SysReportTemplateView Template, List<SysLinkTable> CountryLinks)> l)
        {
            var dtos = new List<SysReportTemplateViewGridDTO>();
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

        #endregion
    }
}
