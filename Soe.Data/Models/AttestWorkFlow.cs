using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class AttestWorkFlowHead : ICreatedModified, IState
    {
        public string TypeName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region AttestWorkFlowHead

        public static AttestWorkFlowHeadDTO ToDTO(this AttestWorkFlowHead e, bool setTypeName, bool loadTemplate)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (loadTemplate && !e.IsAdded() && !e.AttestWorkFlowTemplateHeadReference.IsLoaded)
                {
                    e.AttestWorkFlowTemplateHeadReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("AttestWorkFlow.cs e.AttestWorkFlowTemplateHeadReference");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new AttestGroupDTO()
            {
                AttestWorkFlowHeadId = e.AttestWorkFlowHeadId,
                AttestWorkFlowTemplateHeadId = e.AttestWorkFlowTemplateHeadId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TermGroup_AttestWorkFlowType)e.Type,
                Entity = (SoeEntityType)e.Entity,
                RecordId = e.RecordId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Name = e.Name.NullToEmpty(),
                SendMessage = e.SendMessage ?? true,
                AdminInformation = e.AdminInformation,
                IsDeleted = (e.State == (int)SoeEntityState.Deleted),
                Rows = new List<AttestWorkFlowRowDTO>()
            };

            if (e.AttestWorkFlowRow != null)
            {
                foreach (var row in e.AttestWorkFlowRow)
                {
                    if (row.OriginateFromRowId.HasValue)
                    {
                        var index = dto.Rows.FindIndex(r => r.AttestWorkFlowRowId == row.OriginateFromRowId);
                        dto.Rows.Insert(index + 1, row.ToDTO(false));
                    }
                    else
                    {
                        dto.Rows.Add(row.ToDTO(false));
                    }
                }
            }

            if (e is AttestWorkFlowGroup attestWorkFlowGroup)
            {
                dto.AttestGroupCode = attestWorkFlowGroup.AttestGroupCode;
                dto.AttestGroupName = attestWorkFlowGroup.AttestGroupName;
            }

            // Extensions
            if (setTypeName)
                dto.TypeName = e.TypeName;
            if (loadTemplate && e.AttestWorkFlowTemplateHead != null)
                dto.TemplateName = e.AttestWorkFlowTemplateHead.Name;

            return dto;
        }

        public static List<AttestWorkFlowHeadDTO> ToGroupDTOs(this IEnumerable<AttestWorkFlowHead> l, bool setTypeName, bool loadTemplate)
        {
            var dtos = new List<AttestWorkFlowHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(setTypeName, loadTemplate));
                }
            }
            return dtos;
        }

        public static AttestGroupDTO ToAttestGroupDTO(this AttestWorkFlowGroup e, bool setTypeName, bool loadTemplate)
        {
            return e?.ToDTO(setTypeName, loadTemplate) as AttestGroupDTO;
        }

        public static List<AttestGroupDTO> ToAttestGroupDTOs(this IEnumerable<AttestWorkFlowGroup> l, bool setTypeName, bool loadTemplate)
        {
            return l.Select(s => s.ToAttestGroupDTO(setTypeName, loadTemplate)).ToList();
        }

        #endregion

        #region AttestWorkFlowRow

        public static AttestWorkFlowRowDTO ToDTO(this AttestWorkFlowRow e, bool includeExtensions)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeExtensions && !e.IsAdded())
                {
                    if (!e.AttestTransitionReference.IsLoaded)
                    {
                        e.AttestTransitionReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("AttestWorkFlow.cs e.AttestTransitionReference");
                    }
                    if (e.AttestTransition != null)
                    {
                        if (!e.AttestTransition.AttestStateFromReference.IsLoaded)
                        {
                            e.AttestTransition.AttestStateFromReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("AttestWorkFlow.cs e.AttestTransition.AttestStateFromReference");
                        }
                        if (!e.AttestTransition.AttestStateToReference.IsLoaded)
                        {
                            e.AttestTransition.AttestStateToReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("AttestWorkFlow.cs e.AttestTransition.AttestStateToReference");
                        }
                    }
                    if (!e.AttestRoleReference.IsLoaded)
                    {
                        e.AttestRoleReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("AttestWorkFlow.cs e.AttestRoleReference");
                    }
                    if (!e.UserReference.IsLoaded)
                    {
                        e.UserReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("AttestWorkFlow.cs e.UserReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            AttestWorkFlowRowDTO dto = new AttestWorkFlowRowDTO()
            {
                AttestWorkFlowRowId = e.AttestWorkFlowRowId,
                AttestWorkFlowHeadId = e.AttestWorkFlowHeadId,
                AttestTransitionId = e.AttestTransitionId,
                AttestRoleId = e.AttestRoleId,
                UserId = e.UserId,
                OriginateFromRowId = e.OriginateFromRowId,
                ProcessType = (TermGroup_AttestWorkFlowRowProcessType)e.ProcessType,
                Answer = e.Answer,
                Comment = e.Comment,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (TermGroup_AttestFlowRowState)e.State,
                Type = e.Type.HasValue ? (TermGroup_AttestWorkFlowType)e.Type : (TermGroup_AttestWorkFlowType?)null,
                AnswerText = e.AnswerText,
                AnswerDate = e.AnswerDate,
                CommentUser = e.CommentUser,
                CommentDate = e.CommentDate,
            };

            // Extensions
            if (includeExtensions)
            {
                dto.AttestStateFromId = e.AttestTransition?.AttestStateFromId ?? 0;
                dto.AttestStateFromName = e.AttestTransition?.AttestStateFrom?.Name ?? string.Empty;
                dto.AttestStateToName = e.AttestTransition?.AttestStateTo?.Name ?? string.Empty;
                dto.AttestStateSort = e.AttestTransition?.AttestStateFrom?.Sort ?? 0;
                dto.AttestTransitionName = e.AttestTransition?.Name ?? string.Empty;
                dto.AttestRoleName = e.AttestRole?.Name ?? string.Empty;
                dto.LoginName = e.User?.LoginName ?? string.Empty;
                dto.Name = e.User?.Name ?? string.Empty;
                dto.IsDeleted = e.State == (int)TermGroup_AttestFlowRowState.Deleted;
            }

            return dto;
        }

        public static IEnumerable<AttestWorkFlowRowDTO> ToDTOs(this IEnumerable<AttestWorkFlowRow> l, bool includeExtensions)
        {
            var dtos = new List<AttestWorkFlowRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeExtensions));
                }
            }
            return dtos;
        }

        public static bool OkToMoveAttestFlowToNextLevel(this List<AttestWorkFlowRow> allRows, int attestTransitionId)
        {
            // Check if all the rows at current level are answered
            // Skip the first row with process type 'Registered' and deleted rows
            // If they are, move to next level    
            return !allRows.Any(r => r.AttestTransitionId == attestTransitionId &&
                r.ProcessType != (int)TermGroup_AttestWorkFlowRowProcessType.Registered &&
                r.State != (int)TermGroup_AttestFlowRowState.Deleted &&
                (r.State == (int)TermGroup_AttestFlowRowState.Unhandled || r.ProcessType == (int)TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess));
        }

        #endregion

        #region AttestWorkFlowTemplateHead

        public static AttestWorkFlowTemplateHeadGridDTO ToGridDTO(this AttestWorkFlowTemplateHead e)
        {
            if (e == null)
                return null;

            return new AttestWorkFlowTemplateHeadGridDTO()
            {
                AttestWorkFlowTemplateHeadId = e.AttestWorkFlowTemplateHeadId,
                Type = (TermGroup_AttestWorkFlowType)e.Type,
                Name = e.Name,
                Description = e.Description,
            };
        }

        public static AttestWorkFlowTemplateHeadDTO ToDTO(this AttestWorkFlowTemplateHead e)
        {
            if (e == null)
                return null;

            return new AttestWorkFlowTemplateHeadDTO()
            {
                AttestWorkFlowTemplateHeadId = e.AttestWorkFlowTemplateHeadId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TermGroup_AttestWorkFlowType)e.Type,
                AttestEntity = (TermGroup_AttestEntity)e.AttestEntity,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<AttestWorkFlowTemplateHeadGridDTO> ToGridDTOs(this IEnumerable<AttestWorkFlowTemplateHead> l)
        {
            var dtos = new List<AttestWorkFlowTemplateHeadGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<AttestWorkFlowTemplateHeadDTO> ToDTOs(this IEnumerable<AttestWorkFlowTemplateHead> l)
        {
            var dtos = new List<AttestWorkFlowTemplateHeadDTO>();
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

        #region AttestWorkFlowTemplateRow

        public static AttestWorkFlowTemplateRowDTO ToDTO(this AttestWorkFlowTemplateRow e, bool includeExtensions)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeExtensions && !e.IsAdded())
                {
                    if (!e.AttestTransitionReference.IsLoaded)
                    {
                        e.AttestTransitionReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("AttestWorkFlow.cs e.AttestTransitionReference");
                    }
                    if (e.AttestTransition != null)
                    {
                        if (!e.AttestTransition.AttestStateFromReference.IsLoaded)
                        {
                            e.AttestTransition.AttestStateFromReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("AttestWorkFlow.cs e.AttestTransition.AttestStateFromReference");
                        }
                        if (!e.AttestTransition.AttestStateToReference.IsLoaded)
                        {
                            e.AttestTransition.AttestStateToReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("AttestWorkFlow.cs e.AttestTransition.AttestStateToReference");
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            AttestWorkFlowTemplateRowDTO dto = new AttestWorkFlowTemplateRowDTO()
            {
                AttestWorkFlowTemplateHeadId = e.AttestWorkFlowTemplateHeadId,
                AttestWorkFlowTemplateRowId = e.AttestWorkFlowTemplateRowId,
                AttestTransitionId = e.AttestTransitionId,
                Sort = e.Sort,
                Type = e.Type
            };

            if (includeExtensions && e.AttestTransition != null)
            {
                dto.AttestTransitionName = e.AttestTransition.Name;
                if (e.AttestTransition.AttestStateFrom != null)
                {
                    dto.AttestStateFromName = e.AttestTransition.AttestStateFrom.Name;
                    if (e.AttestTransition.AttestStateFrom.Initial)
                        dto.Initial = true;
                }
                if (e.AttestTransition.AttestStateTo != null)
                {
                    dto.AttestStateToName = e.AttestTransition.AttestStateTo.Name;
                    dto.AttestStateToColor = e.AttestTransition.AttestStateTo.Color;
                    if (e.AttestTransition.AttestStateTo.Closed)
                        dto.Closed = true;
                }
            }

            return dto;
        }

        public static List<AttestWorkFlowTemplateRowDTO> ToDTOs(this IEnumerable<AttestWorkFlowTemplateRow> l, bool includeExtensions)
        {
            var dtos = new List<AttestWorkFlowTemplateRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeExtensions));
                }
            }
            return dtos;
        }

        #endregion
    }
}
