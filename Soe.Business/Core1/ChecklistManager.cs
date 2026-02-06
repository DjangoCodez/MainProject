using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class ChecklistManager : ManagerBase
    {        
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ChecklistManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region ChecklistHead

        public List<int> GetChecklistHeadIds(int actorCompanyId, bool includeInactive, bool? onlyForOrder = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetChecklistHeadQuery(entities, actorCompanyId, includeInactive, onlyForOrder).Select(x => x.ChecklistHeadId).ToList();
        }

        public List<ChecklistHead> GetChecklistHeads(int actorCompanyId, bool includeInactive)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ChecklistHead.NoTracking();

            IQueryable<ChecklistHead> query = GetChecklistHeadQuery(entitiesReadOnly, actorCompanyId, includeInactive);

            List<ChecklistHead> heads = query.ToList();

            foreach (ChecklistHead head in heads)
            {
                head.TypeName = GetText(head.Type, (int)TermGroup.ChecklistHeadType);
            }

            return heads;
        }

        private static IQueryable<ChecklistHead> GetChecklistHeadQuery(CompEntities entities, int actorCompanyId, bool includeInactive, bool? onlyForOrder = null)
        {
            IQueryable<ChecklistHead> query = (from c in entities.ChecklistHead
                                               where c.ActorCompanyId == actorCompanyId
                                               select c);

            if (includeInactive)
            {
                query = query.Where(c => c.State == (int)SoeEntityState.Inactive || c.State == (int)SoeEntityState.Active);
            }
            else
            {
                query = query.Where(c => c.State == (int)SoeEntityState.Active);
            }

            if (onlyForOrder.HasValue)
            {
                query = query.Where(c => c.DefaultInOrder == onlyForOrder.Value);
            }

            return query;
        }

        public List<ChecklistHead> GetChecklistHeadsIncludingRows(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistHead.NoTracking();
            List<ChecklistHead> heads = (from c in entities.ChecklistHead.Include("ChecklistRow")
                                         where c.ActorCompanyId == actorCompanyId &&
                                         c.State == (int)SoeEntityState.Active
                                         select c).ToList();

            foreach (ChecklistHead head in heads)
            {
                head.TypeName = GetText(head.Type, (int)TermGroup.ChecklistHeadType);
            }

            return heads;
        }

        public List<ChecklistHead> GetChecklistHeadsForType(TermGroup_ChecklistHeadType type, int actorCompanyId, bool loadRows)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistHead.NoTracking();
            IQueryable<ChecklistHead> query = (from c in entities.ChecklistHead
                                               where c.ActorCompanyId == actorCompanyId &&
                                               c.Type == (int)type &&
                                               c.State == (int)SoeEntityState.Active
                                               orderby c.Name
                                               select c);

            if (loadRows)
            {
                query = query.Include("ChecklistRow.CheckListMultipleChoiceAnswerHead");
            }

            var heads = query.ToList();
            foreach (ChecklistHead head in heads)
            {
                head.TypeName = GetText(head.Type, (int)TermGroup.ChecklistHeadType);
            }

            return heads;
        }

        public List<SmallGenericType> GetChecklistHeadsDict(TermGroup_ChecklistHeadType type, int actorCompanyId, bool loadRows)
        {
            List<SmallGenericType> dict = new List<SmallGenericType>();
            var heads = GetChecklistHeadsForType(type, actorCompanyId, loadRows);

            foreach (ChecklistHead head in heads)
            {
                dict.Add(new SmallGenericType() { Id = head.ChecklistHeadId, Name = head.Name });
            }

            return dict;
        }

        public ChecklistHead GetChecklistHead(int checklistHeadId, int actorCompanyId, bool loadRows, bool loadTerms = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistHead.NoTracking();
            return GetChecklistHead(entities, checklistHeadId, actorCompanyId, loadRows, loadTerms);
        }

        public ChecklistHead GetChecklistHead(CompEntities entities, int checklistHeadId, int actorCompanyId, bool loadRows, bool loadTerms = false)
        {
            ChecklistHead head = (from c in entities.ChecklistHead
                                  where c.ChecklistHeadId == checklistHeadId &&
                                  c.ActorCompanyId == actorCompanyId &&
                                  c.State != (int)SoeEntityState.Deleted
                                  select c).FirstOrDefault();

            if (head != null)
            {
                if (loadTerms)
                    head.TypeName = GetText(head.Type, (int)TermGroup.ChecklistHeadType);

                if (loadRows)
                {
                    if (!head.ChecklistRow.IsLoaded)
                        head.ChecklistRow.Load();

                    if (loadTerms)
                    {
                        foreach (ChecklistRow row in head.ChecklistRow)
                        {

                            row.TypeName = GetText(row.Type, (int)TermGroup.ChecklistRowType);
                        }
                    }
                }
            }

            return head;
        }

        public ActionResult SaveChecklistHead(ChecklistHeadDTO checklistHeadInput, int actorCompanyId)
        {
            var result = new ActionResult(true);

            if (checklistHeadInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChecklistHead");

            // Name must be unique
            
            var existingChecklists = GetChecklistHeads(actorCompanyId, true);
            if (existingChecklists.Exists(p => p.Name == checklistHeadInput.Name && p.ChecklistHeadId  != checklistHeadInput.ChecklistHeadId))
            {
                return new ActionResult(4182, GetText(4182, "Det finns redan en checklista med valt namn") + ": " + checklistHeadInput.Name);
            }

            int checklistHeadId = 0;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Prereq

                    // Name mandatory
                    if (string.IsNullOrEmpty(checklistHeadInput.Name))
                    {
                        result = new ActionResult(7708, GetText(7708, "Checklistan saknar namn"));
                        return result;
                    }

                    // Type mandatory
                    if (checklistHeadInput.Type == (int)TermGroup_ChecklistHeadType.Unknown)
                    {
                        result = new ActionResult(7709, GetText(7709, "Checklistan saknar typ"));
                        return result;
                    }

                    #region rows
                    foreach (var checklistRowInput in checklistHeadInput.ChecklistRows)
                    {
                        // Type mandatory
                        if (checklistRowInput.Type == TermGroup_ChecklistRowType.Unknown)
                        {
                            result = new ActionResult(7710, GetText(7710, "En eller flera av checklistans rader saknar typ av svar"));
                            return result;
                        }

                        if (checklistRowInput.Text == null)
                        {
                            result = new ActionResult(7711, GetText(7711, "En eller flera av checklistans rader saknar fråga"));
                            return result;
                        }
                    }
                    #endregion

                    Report report = null;
                    if (checklistHeadInput.ReportId != null && checklistHeadInput.ReportId != 0)
                    {
                        report = ReportManager.GetReport(entities, (int)checklistHeadInput.ReportId, actorCompanyId);
                    }

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Perform

                        ChecklistHead checklistHead = null;

                        if (checklistHeadInput.ChecklistHeadId == 0)
                        {
                            #region Add

                            #region ChecklistHead

                            checklistHead = new ChecklistHead()
                            {
                                Name = checklistHeadInput.Name,
                                Description = checklistHeadInput.Description,
                                Type = (int)checklistHeadInput.Type,
                                AddAttachementsToEInvoice = checklistHeadInput.AddAttachementsToEInvoice,
                                DefaultInOrder = checklistHeadInput.DefaultInOrder,

                                //Set FK
                                ActorCompanyId = actorCompanyId,
                            };

                            if (report != null)
                            {
                                checklistHead.Report = report;
                            }

                            SetCreatedProperties(checklistHead);
                            entities.ChecklistHead.AddObject(checklistHead);

                            #endregion

                            #region ChecklistRow

                            foreach (var checklistRowInput in checklistHeadInput.ChecklistRows.OrderBy(i => i.RowNr))
                            {
                                ChecklistRow checklistRow = new ChecklistRow()
                                {
                                    RowNr = checklistRowInput.RowNr,
                                    Text = checklistRowInput.Text,
                                    Type = (int)checklistRowInput.Type,
                                    Mandatory = checklistRowInput.Mandatory,
                                    CheckListMultipleChoiceAnswerHeadId = checklistRowInput.CheckListMultipleChoiceAnswerHeadId > 0 ? checklistRowInput.CheckListMultipleChoiceAnswerHeadId : (int?)null,
                                };
                                SetCreatedProperties(checklistRow);

                                checklistHead.ChecklistRow.Add(checklistRow);
                            }

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Update

                            checklistHead = GetChecklistHead(entities, checklistHeadInput.ChecklistHeadId, actorCompanyId, true);
                            if (checklistHead == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "ChecklistHead");

                            #region ChecklistHead

                            checklistHead.Name = checklistHeadInput.Name;
                            checklistHead.Description = checklistHeadInput.Description;
                            checklistHead.Type = (int)checklistHeadInput.Type;
                            checklistHead.AddAttachementsToEInvoice = checklistHeadInput.AddAttachementsToEInvoice;
                            checklistHead.DefaultInOrder = checklistHeadInput.DefaultInOrder;

                            if (checklistHeadInput.State == SoeEntityState.Active || checklistHeadInput.State == SoeEntityState.Inactive)
                            {
                                checklistHead.State = (int)checklistHeadInput.State;
                            }

                            if (report != null)
                            {
                                checklistHead.Report = report;
                            }

                            SetModifiedProperties(checklistHead);

                            #endregion

                            #region ChecklistRow

                            var checklistRowsInput = checklistHeadInput.ChecklistRows.Where(i => i.State == SoeEntityState.Active).OrderBy(i => i.RowNr).ToList();
                            var checklistRows = checklistHead.ChecklistRow.Where(i => i.State == (int)SoeEntityState.Active).OrderBy(i => i.RowNr).ToList();

                            #region Add

                            foreach (ChecklistRowDTO checklistRowToAdd in checklistRowsInput.Where(i => i.ChecklistRowId == 0))
                            {
                                ChecklistRow checklistRow = new ChecklistRow()
                                {
                                    RowNr = checklistRowToAdd.RowNr,
                                    Text = checklistRowToAdd.Text,
                                    Type = (int)checklistRowToAdd.Type,
                                    Mandatory = checklistRowToAdd.Mandatory,
                                    CheckListMultipleChoiceAnswerHeadId = checklistRowToAdd.CheckListMultipleChoiceAnswerHeadId > 0 ? checklistRowToAdd.CheckListMultipleChoiceAnswerHeadId : (int?)null,
                                };
                                SetCreatedProperties(checklistRow);

                                checklistHead.ChecklistRow.Add(checklistRow);
                            }

                            #endregion

                            #region Update/Delete

                            //Check all existing ChecklistRows if they are updated or deleted in input collection
                            for (int idx = checklistRows.Count - 1; idx >= 0; idx--)
                            {
                                var checklistRow = checklistRows.ElementAt(idx);
                                var checklistRowInput = checklistRowsInput.FirstOrDefault(i => i.ChecklistRowId == checklistRow.ChecklistRowId);
                                if (checklistRowInput == null)
                                {
                                    #region Delete

                                    if (IsChecklistRowUsed(entities, checklistRow.ChecklistRowId))
                                        return new ActionResult(11826, GetText(11826, "Raden används och kan därför inte tas bort") + "\n\n- " + checklistRow.Text);

                                    ChangeEntityState(checklistRow, SoeEntityState.Deleted);

                                    #endregion
                                }
                                else
                                {
                                    #region Update

                                    checklistRow.RowNr = checklistRowInput.RowNr;
                                    checklistRow.Text = checklistRowInput.Text;
                                    checklistRow.Type = (int)checklistRowInput.Type;
                                    checklistRow.Mandatory = checklistRowInput.Mandatory;
                                    checklistRow.CheckListMultipleChoiceAnswerHeadId = checklistRowInput.CheckListMultipleChoiceAnswerHeadId > 0 ? checklistRowInput.CheckListMultipleChoiceAnswerHeadId : (int?)null;
                                    SetModifiedProperties(checklistRow);

                                    #endregion
                                }
                            }

                            #endregion

                            #endregion

                            #endregion
                        }

                        #endregion

                        #region Save

                        if (result.Success)
                        {
                            result = SaveChanges(entities, transaction);
                            checklistHeadId = checklistHead.ChecklistHeadId;
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    result.IntegerValue = checklistHeadId;

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult DeleteChecklistHead(int checklistHeadId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            if (checklistHeadId == 0)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "ChecklistHead");

            using (var entities = new CompEntities())
            {
                ChecklistHead originalChecklistHead = GetChecklistHead(entities, checklistHeadId, actorCompanyId, true);
                if (originalChecklistHead == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ChecklistHead");

                //Check if it is used already
                if (HasChecklistHeadRecords(entities, originalChecklistHead.ChecklistHeadId, actorCompanyId))
                    return new ActionResult(11825, GetText(11825, "Checklistan används och kan därför inte tas bort"));

                #region ChecklistRow

                List <ChecklistRow> checklistRows = originalChecklistHead.ChecklistRow.ToList();
                for (int i = 0; i < checklistRows.Count; i++)
                {
                    ChecklistRow checklistRow = checklistRows[i];
                    if (checklistRow != null)
                        ChangeEntityState(checklistRow, SoeEntityState.Deleted);
                }

                #endregion

                #region ChecklistHead

                result = ChangeEntityState(originalChecklistHead, SoeEntityState.Deleted);
                if (result.Success)
                    result = SaveChanges(entities);

                #endregion

                return result;
            }
        }

        #endregion

        #region ChecklistHeadRecord

        public List<int> GetChecklistHeadIdsFromRecord(SoeEntityType entity, int recordId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistHeadRecord.NoTracking();
            return GetChecklistHeadIdsFromRecord(entities, entity, recordId, actorCompanyId);
        }

        public List<int> GetChecklistHeadIdsFromRecord(CompEntities entities, SoeEntityType entity, int recordId, int actorCompanyId)
        {
            if (recordId == 0)
                return new List<int>();

            return GetChecklistHeadRecordsQuery(entities, entity, recordId, actorCompanyId, false).Select(x=> x.ChecklistHeadId).ToList();
        }

        public List<ChecklistHeadRecord> GetChecklistHeadRecords(SoeEntityType entity, int recordId, int actorCompanyId, bool includeCheckListHead = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistHeadRecord.NoTracking();
            return GetChecklistHeadRecords(entities, entity, recordId, actorCompanyId, includeCheckListHead);
        }

        public List<ChecklistHeadRecord> GetChecklistHeadRecords(CompEntities entities, SoeEntityType entity, int recordId, int actorCompanyId, bool includeCheckListHead = false)
        {
            if (recordId == 0)
                return new List<ChecklistHeadRecord>();

            return GetChecklistHeadRecordsQuery(entities,entity,recordId,actorCompanyId,includeCheckListHead).ToList();
        }

        private static IQueryable<ChecklistHeadRecord> GetChecklistHeadRecordsQuery(CompEntities entities, SoeEntityType entity, int recordId, int actorCompanyId, bool includeCheckListHead)
        {
            IQueryable<ChecklistHeadRecord> query = entities.ChecklistHeadRecord;
            if (includeCheckListHead)
            {
                query = query.Include("ChecklistHead");
            }

            return query.Where(c => c.ActorCompanyId == actorCompanyId &&
                                    c.Entity == (int)entity &&
                                    c.RecordId == recordId &&
                                    c.State == (int)SoeEntityState.Active);
        }

        public List<ChecklistHeadRecord> GetChecklistHeadRecordsWithRows(SoeEntityType entity, int recordId, int actorCompanyId, bool includeCheckListHead = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistHeadRecord.NoTracking();
            return GetChecklistHeadRecordsWithRows(entities, entity, recordId, actorCompanyId, includeCheckListHead);
        }

        public List<ChecklistHeadRecord> GetChecklistHeadRecordsWithRows(CompEntities entities, SoeEntityType entity, int recordId, int actorCompanyId, bool includeCheckListHead = false)
        {
            List<ChecklistHeadRecord> headRecords = new List<ChecklistHeadRecord>();

            if (recordId == 0)
                return headRecords;

            headRecords = (from c in entities.ChecklistHeadRecord
                            .Include("ChecklistHead")
                            .Include("ChecklistRowRecord.ChecklistRow")
                            where c.ActorCompanyId == actorCompanyId &&
                            c.Entity == (int)entity &&
                            c.RecordId == recordId &&
                            c.State == (int)SoeEntityState.Active
                            select c).ToList();

            return headRecords;
        }

        public List<ChecklistHeadRecordCompactDTO> GetChecklistsRecordsWithSignatures(SoeEntityType entity, int recordId, int actorCompanyId, bool includeCheckListHead = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistHeadRecord.NoTracking();
            return GetChecklistsRecordsWithSignatures(entities, entity, recordId, actorCompanyId, includeCheckListHead);
        }

        public List<ChecklistHeadRecordCompactDTO> GetChecklistsRecordsWithSignatures(CompEntities entities, SoeEntityType entity, int recordId, int actorCompanyId, bool includeCheckListHead = false)
        {
            List<ChecklistHeadRecordCompactDTO> headRecords = new List<ChecklistHeadRecordCompactDTO>();

            if (recordId == 0)
                return headRecords;

            headRecords = (from c in entities.ChecklistHeadRecord
                           where c.ActorCompanyId == actorCompanyId &&
                           c.Entity == (int)entity &&
                           c.RecordId == recordId &&
                           c.State == (int)SoeEntityState.Active
                           select new ChecklistHeadRecordCompactDTO
                           {
                               ChecklistHeadRecordId = c.ChecklistHeadRecordId,
                               ChecklistHeadId = c.ChecklistHeadId,
                               ChecklistHeadName = c.ChecklistHead != null ? c.ChecklistHead.Name : string.Empty,
                               AddAttachementsToEInvoice = c.AddAttachementsToEInvoice,
                               RecordId = c.RecordId,
                               State = (SoeEntityState)c.State,
                               Created = c.Created,
                               ChecklistRowRecords = c.ChecklistRowRecord.Select(r => new ChecklistExtendedRowDTO
                               {
                                   RowRecordId = r.ChecklistRowRecordId,
                                   HeadRecordId = r.ChecklistHeadRecordId,
                                   Comment = r.Comment,
                                   Date = r.Date,
                                   DataTypeId = r.DataTypeId,
                                   StrData = r.StrData,
                                   IntData = r.IntData,
                                   BoolData = r.BoolData,
                                   DecimalData = r.DecimalData,
                                   Created = r.Created,
                                   CreatedBy = r.CreatedBy,
                                   Modified = r.Modified,
                                   ModifiedBy = r.ModifiedBy,
                                   Text = r.Text,
                                   Type = (TermGroup_ChecklistRowType)r.Type,
                                   CheckListMultipleChoiceAnswerHeadId = r.ChecklistRow.CheckListMultipleChoiceAnswerHeadId,

                                   RowId = r.ChecklistRow != null ? r.ChecklistRow.ChecklistRowId : 0,
                                   HeadId = r.ChecklistRow != null ? r.ChecklistRow.ChecklistHeadId : 0,
                                   RowNr = r.ChecklistRow != null ? r.ChecklistRow.RowNr : 0,
                                   Mandatory = r.ChecklistRow != null ? r.ChecklistRow.Mandatory : false,

                                   Guid = Guid.NewGuid()
                               }).ToList()
                           }).ToList();

            foreach(var head in headRecords)
            {
                head.Signatures = new List<ImagesDTO>();
                head.Signatures.AddRange(ChecklistManager.GetEntityChecklistSignatures(actorCompanyId, SoeEntityType.ChecklistHeadRecord, head.ChecklistHeadRecordId));

                var imageRecords = head.ChecklistRowRecords.Where(x => x.Type == TermGroup_ChecklistRowType.Image).ToList();
                if (imageRecords.Any())
                {
                    var imgeRecordIds = imageRecords.Select(x => x.RowRecordId).ToList();
                    var hasDataStorages = GeneralManager.HasDataStorageRecords(entities, actorCompanyId, imgeRecordIds, SoeDataStorageRecordType.ChecklistHeadRecord, SoeEntityType.ChecklistHeadRecord);

                    foreach(var record in imageRecords)
                    {
                        record.BoolData = hasDataStorages.ContainsKey(record.RowRecordId);
                    }
                }
            }

            return headRecords;
        }

        public ChecklistHeadRecord GetChecklistHeadRecord(int checklistHeadRecordId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistHeadRecord.NoTracking();
            return GetChecklistHeadRecord(entities, checklistHeadRecordId, actorCompanyId);
        }

        public ChecklistHeadRecord GetChecklistHeadRecord(CompEntities entities, int checklistHeadRecordId, int actorCompanyId)
        {
            return (from c in entities.ChecklistHeadRecord
                    where c.ChecklistHeadRecordId == checklistHeadRecordId &&
                    c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    select c).FirstOrDefault();
        }

        public ChecklistHeadRecord GetChecklistHeadRecordWithChecklistRowRecords(CompEntities entities, int checklistHeadRecordId, int actorCompanyId)
        {
            return (from c in entities.ChecklistHeadRecord.Include("ChecklistRowRecord")
                    where c.ChecklistHeadRecordId == checklistHeadRecordId &&
                    c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    select c).FirstOrDefault();
        }

        public ChecklistHeadRecord GetChecklistHeadRecordWithCheckListHead(SoeEntityType entity, int recordId, int checklistHeadRecordId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistHeadRecord.NoTracking();
            return GetChecklistHeadRecordWithCheckListHead(entities, entity, recordId, checklistHeadRecordId, actorCompanyId);
        }

        public ChecklistHeadRecord GetChecklistHeadRecordWithCheckListHead(CompEntities entities, SoeEntityType entity, int recordId, int checklistHeadRecordId, int actorCompanyId)
        {
            return (from c in entities.ChecklistHeadRecord.Include("ChecklistHead")
                    where c.ActorCompanyId == actorCompanyId &&
                    c.ChecklistHeadRecordId == checklistHeadRecordId &&
                    c.Entity == (int)entity &&
                    c.RecordId == recordId &&
                    c.State == (int)SoeEntityState.Active
                    select c).FirstOrDefault();

        }

        public static int GetNrOfChecklistHeadRecords(CompEntities entities, SoeEntityType entity, int recordId, int actorCompanyId)
        {
            return (from c in entities.ChecklistHeadRecord
                    where c.ActorCompanyId == actorCompanyId &&
                    c.Entity == (int)entity &&
                    c.RecordId == recordId &&
                    c.State == (int)SoeEntityState.Active
                    select c).Count();
        }

        public static bool HasChecklistHeadRecords(CompEntities entities, int checklistHeadId, int actorCompanyId)
        {
            return (from c in entities.ChecklistHeadRecord
                    where c.ChecklistHeadId == checklistHeadId &&
                    c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    select c).Any();
        }

        public IEnumerable<ImagesDTO> GetChecklistRowImages(CompEntities entities, int checklistRowRecordId, int actorCompanyId)
        {
            return GeneralManager.GetDataStorageRecords(entities, actorCompanyId, (int?)null, checklistRowRecordId, SoeEntityType.ChecklistHeadRecord, SoeDataStorageRecordType.ChecklistHeadRecord, includeDataStorage:true, loadData:true).ToImagesDTOs(true);
        }

        #endregion

        #region ChecklistRowRecord

        public List<ChecklistRowRecord> GetChecklistRowRecords(CompEntities entities, SoeEntityType entity, int recordId, int actorCompanyId)
        {
            return (from c in entities.ChecklistRowRecord
                    where c.ChecklistHeadRecord.RecordId == recordId &&
                    c.ChecklistHeadRecord.ActorCompanyId == actorCompanyId &&
                    c.ChecklistHeadRecord.Entity == (int)entity &&
                    c.ChecklistHeadRecord.State == (int)SoeEntityState.Active &&
                    c.State == (int)SoeEntityState.Active
                    select c).ToList();
        }

        public static bool IsChecklistRowUsed(CompEntities entities, int checklistRowId)
        {
            return (from cr in entities.ChecklistRowRecord
                    where cr.ChecklistRowId == checklistRowId &&
                    cr.State == (int)SoeEntityState.Active
                    select cr).Any();
        }

        #endregion

        #region ChecklistRowDTO

        public List<ChecklistExtendedRowDTO> GetChecklistRows(SoeEntityType entity, int recordId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistRowRecord.NoTracking();
            return GetChecklistRows(entities, entity, recordId, actorCompanyId);
        }

        public List<ChecklistExtendedRowDTO> GetChecklistRows(CompEntities entities, SoeEntityType entity, int recordId, int actorCompanyId)
        {
            List<ChecklistExtendedRowDTO> dtos = new List<ChecklistExtendedRowDTO>();

            if (recordId == 0)
                return dtos;

            List<ChecklistHeadRecord> headRecords = GetChecklistHeadRecords(entity, recordId, actorCompanyId);
            foreach (ChecklistHeadRecord headRecord in headRecords)
            {
                dtos.AddRange(GetChecklistRows(entities, headRecord.ChecklistHeadId, entity, recordId, actorCompanyId, headRecord.ChecklistHeadRecordId));
            }

            return dtos;
        }

        public List<ChecklistExtendedRowDTO> GetChecklistRows(int checklistHeadId, SoeEntityType entity, int recordId, int actorCompanyId, int checklistHeadRecordId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ChecklistRow.NoTracking();
            entities.ChecklistRowRecord.NoTracking();
            return GetChecklistRows(entities, checklistHeadId, entity, recordId, actorCompanyId, checklistHeadRecordId);
        }

        public List<ChecklistExtendedRowDTO> GetChecklistRows(CompEntities entities, int checklistHeadId, SoeEntityType entity, int recordId, int actorCompanyId, int? checklistHeadRecordId = null)
        {
            List<ChecklistExtendedRowDTO> dtos = new List<ChecklistExtendedRowDTO>();

            List<ChecklistRow> rows = (from cr in entities.ChecklistRow
                                        .Include("ChecklistHead")
                                       where cr.ChecklistHeadId == checklistHeadId &&
                                       cr.State == (int)SoeEntityState.Active
                                       select cr).OrderBy(o => o.RowNr).ToList();

            List<ChecklistRowRecord> rowRecords = (from crr in entities.ChecklistRowRecord
                                                   where crr.ChecklistHeadRecord.Entity == (int)entity &&
                                                   crr.ChecklistHeadRecord.RecordId == recordId &&
                                                   crr.ChecklistHeadRecord.ActorCompanyId == actorCompanyId &&
                                                   crr.ChecklistHeadRecord.State == (int)SoeEntityState.Active &&
                                                   (!checklistHeadRecordId.HasValue || crr.ChecklistHeadRecordId == checklistHeadRecordId) &&
                                                   crr.State == (int)SoeEntityState.Active
                                                   select crr).ToList();

            foreach (ChecklistRow row in rows)
            {
                var dto = new ChecklistExtendedRowDTO();

                //ChecklistHead
                dto.Name = row.ChecklistHead.Name;
                dto.Guid = Guid.NewGuid();

                //ChecklistRow
                dto.RowId = row.ChecklistRowId;
                dto.HeadId = row.ChecklistHeadId;
                dto.RowNr = row.RowNr;
                dto.Mandatory = row.Mandatory;
                dto.Text = row.Text;
                dto.Type = (TermGroup_ChecklistRowType)row.Type;

                //ChecklistRowRecord
                ChecklistRowRecord rowRecord = rowRecords.FirstOrDefault(i => i.ChecklistRowId == row.ChecklistRowId);
                if (rowRecord != null)
                {
                    dto.RowRecordId = rowRecord.ChecklistRowRecordId;
                    dto.HeadRecordId = rowRecord.ChecklistHeadRecordId;
                    dto.Comment = rowRecord.Comment;
                    dto.Date = rowRecord.Date;
                    dto.DataTypeId = rowRecord.DataTypeId;
                    dto.StrData = rowRecord.StrData;
                    dto.IntData = rowRecord.IntData;
                    dto.BoolData = rowRecord.BoolData;
                    dto.DateData = rowRecord.DateData;
                    dto.DecimalData = rowRecord.DecimalData;
                    dto.Created = rowRecord.Created;
                    dto.CreatedBy = rowRecord.CreatedBy;
                    dto.Modified = rowRecord.Modified;
                    dto.ModifiedBy = rowRecord.ModifiedBy;

                    //Overwrite with values from record
                    dto.Text = rowRecord.Text;
                    dto.Type = (TermGroup_ChecklistRowType)rowRecord.Type;
                    if (dto.Type == TermGroup_ChecklistRowType.MultipleChoice)
                        dto.CheckListMultipleChoiceAnswerHeadId = row.CheckListMultipleChoiceAnswerHeadId;
                }
                else
                {
                    if (checklistHeadRecordId.HasValue)
                        dto.HeadRecordId = checklistHeadRecordId.Value;

                    switch (row.Type)
                    {
                        case (int)TermGroup_ChecklistRowType.String:
                            dto.DataTypeId = (int)SettingDataType.String;
                            break;
                        case (int)TermGroup_ChecklistRowType.YesNo:
                        case (int)TermGroup_ChecklistRowType.Checkbox:
                            dto.DataTypeId = (int)SettingDataType.Boolean;
                            break;
                        case (int)TermGroup_ChecklistRowType.Image:
                            dto.DataTypeId = (int)SettingDataType.Image;
                            break;
                        case (int)TermGroup_ChecklistRowType.MultipleChoice:
                            dto.CheckListMultipleChoiceAnswerHeadId = row.CheckListMultipleChoiceAnswerHeadId;
                            dto.DataTypeId = (int)SettingDataType.String;
                            break;
                    }
                }

                dtos.Add(dto);
            }

            return dtos;
        }

        public ActionResult SaveChecklistRecords(List<ChecklistExtendedRowDTO> dtos, SoeEntityType entity, int recordId, int actorCompanyId, bool callerIsMobile = false)
        {
            ActionResult result = new ActionResult(true);

            if (dtos == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChecklistRowDTO");
            if (recordId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "RecordId");

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        var headRecords = GetChecklistHeadRecords(entities, entity, recordId, actorCompanyId).ToDictionary(k => k.ChecklistHeadRecordId);
                        List<ChecklistRowRecord> rowRecords = GetChecklistRowRecords(entities, entity, recordId, actorCompanyId);

                        #endregion

                        foreach (var dto in dtos)
                        {
                            #region ChecklistHeadRecord
                            ChecklistHeadRecord headRecord = headRecords.Where(i => i.Key == dto.HeadRecordId).Select(i => i.Value).FirstOrDefault();

                            if (headRecord == null)
                            {
                                #region Add

                                headRecord = new ChecklistHeadRecord()
                                {
                                    Entity = (int)entity,
                                    RecordId = recordId,

                                    //Set FK
                                    ChecklistHeadId = dto.HeadId,
                                    ActorCompanyId = actorCompanyId,
                                };
                                SetCreatedProperties(headRecord);
                                entities.ChecklistHeadRecord.AddObject(headRecord);

                                headRecords.Add(dto.HeadRecordId, headRecord);

                                #endregion
                            }
                            else if (dto.HeadRecordId > 0)
                            {
                                #region Update

                                SetModifiedProperties(headRecord);

                                #endregion
                            }

                            #endregion

                            #region ChecklistRowRecord

                            ChecklistRowRecord rowRecord = rowRecords.FirstOrDefault(i => i.ChecklistRowRecordId == dto.RowRecordId && i.State == (int)SoeEntityState.Active);
                            if (rowRecord == null)
                            {
                                #region Add

                                rowRecord = new ChecklistRowRecord()
                                {
                                    Text = dto.Text,
                                    Type = (int)dto.Type,

                                    //Set FK
                                    ChecklistRowId = dto.RowId,
                                    ActorCompanyId = actorCompanyId,

                                    //Set references
                                    ChecklistHeadRecord = headRecord,
                                };

                                SetCreatedProperties(rowRecord);
                                entities.ChecklistRowRecord.AddObject(rowRecord);

                                #endregion
                            }
                            else
                            {
                                #region Update

                                SetModifiedProperties(rowRecord);

                                #endregion
                            }

                            #region Common

                            rowRecord.Comment = dto.Comment;
                            rowRecord.Date = dto.Date;
                            rowRecord.DataTypeId = dto.DataTypeId;
                            rowRecord.StrData = dto.StrData;
                            rowRecord.IntData = dto.IntData;
                            rowRecord.BoolData = dto.BoolData;
                            rowRecord.DateData = dto.DateData;
                            rowRecord.DecimalData = dto.DecimalData;

                            #endregion

                            #endregion
                        }

                        #region Delete

                        if (!callerIsMobile)
                        {
                            foreach (var headRecord in headRecords)
                            {
                                if (headRecord.Key <= 0)
                                    continue; //New

                                if (!dtos.Any(i => i.HeadRecordId == headRecord.Value.ChecklistHeadRecordId))
                                {
                                    ChangeEntityState(headRecord.Value, SoeEntityState.Deleted);

                                    foreach (ChecklistRowRecord rowRecord in rowRecords.Where(i => i.ChecklistHeadRecordId == headRecord.Value.ChecklistHeadRecordId))
                                    {
                                        ChangeEntityState(rowRecord, SoeEntityState.Deleted);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Save

                        if (result.Success)
                            result = SaveChanges(entities, transaction);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult SaveChecklistRecords(CompEntities entities, TransactionScope transaction, List<ChecklistExtendedRowDTO> dtos, SoeEntityType entity, int recordId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            #region Prereq

            List<ChecklistRowRecord> rowRecords = GetChecklistRowRecords(entities, entity, recordId, actorCompanyId);

            #endregion

            foreach (var dto in dtos)
            {
                #region Modify data

                if(dto.Type == TermGroup_ChecklistRowType.YesNo)
                {
                    if (dto.IntData > 0)
                    {
                        dto.BoolData = dto.IntData == 1 ? true : false;
                        dto.IntData = null;
                    }
                    else
                    {
                        dto.BoolData = null;
                        dto.IntData = null;
                    }
                }

                #endregion

                #region ChecklistRowRecord

                ChecklistRowRecord rowRecord = rowRecords.FirstOrDefault(i => i.ChecklistRowRecordId == dto.RowRecordId && i.State == (int)SoeEntityState.Active);
                if (rowRecord != null)
                {
                    #region Common

                    rowRecord.Comment = dto.Comment;
                    rowRecord.Date = dto.Date;
                    rowRecord.DataTypeId = dto.DataTypeId;
                    rowRecord.StrData = dto.StrData;
                    rowRecord.IntData = dto.IntData;
                    rowRecord.BoolData = dto.BoolData;
                    rowRecord.DateData = dto.DateData;
                    rowRecord.DecimalData = dto.DecimalData;

                    if (dto.Type == TermGroup_ChecklistRowType.Image && !dto.FileUploads.IsNullOrEmpty())
                    {
                        foreach (var file in dto.FileUploads)
                        {
                            if (!FileUtil.IsImageFile(file.FileName))
                                return new ActionResult(recordId, GetText(7773, "Bara bildfiler kan läggas till") + ": " + file.FileName);

                            if (file.IsDeleted)
                                result = GeneralManager.DeleteDataStorageRecord(entities, file.Id ?? 0, true);
                            else
                                result = GeneralManager.UpdateDataStorageRecord(entities, file.Id ?? 0, rowRecord.ChecklistRowRecordId, file.Description);

                            if (!result.Success)
                                return result;
                        }
                    }

                    SetModifiedProperties(rowRecord);

                    #endregion
                }
                #endregion
            }

            return result;
        }

        public ActionResult UpdateChecklistHeadRecords(CompEntities entities, TransactionScope transaction, List<ChecklistHeadRecordCompactDTO> dtos)
        {
            var result = new ActionResult(true);

            foreach (var dto in dtos)
            {
                var checkList = entities.ChecklistHeadRecord.FirstOrDefault(x => x.ChecklistHeadRecordId == dto.ChecklistHeadRecordId);
                if (checkList != null)
                {
                    checkList.AddAttachementsToEInvoice = dto.AddAttachementsToEInvoice;
                }
            }
            return result;
        }

        public ActionResult AddChecklistRecords(CompEntities entities, TransactionScope transaction, List<ChecklistHeadRecordCompactDTO> dtos, SoeEntityType entity, int recordId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);
   
            foreach (var dto in dtos)
            {
                #region ChecklistHeadRecord

                ChecklistHeadRecord headRecord = new ChecklistHeadRecord()
                {
                    Entity = (int)entity,
                    RecordId = recordId,
                    AddAttachementsToEInvoice = dto.AddAttachementsToEInvoice,

                    //Set FK
                    ChecklistHeadId = dto.ChecklistHeadId,
                    ActorCompanyId = actorCompanyId,
                };
                SetCreatedProperties(headRecord);
                entities.ChecklistHeadRecord.AddObject(headRecord);

                #endregion

                #region ChecklistRowRecord

                foreach (ChecklistExtendedRowDTO row in dto.ChecklistRowRecords)
                {
                    ChecklistRowRecord rowRecord = new ChecklistRowRecord()
                    {
                        Text = row.Text,
                        Type = (int)row.Type,

                        //Set FK
                        ChecklistRowId = row.RowId,
                        ActorCompanyId = actorCompanyId,

                        //Set references
                        ChecklistHeadRecord = headRecord,

                        Comment = row.Comment,
                        Date = row.Date,
                        DataTypeId = row.DataTypeId,
                        StrData = row.StrData,
                        IntData = row.IntData,
                        BoolData = row.BoolData,
                        DateData = row.DateData,
                        DecimalData = row.DecimalData
                    };

                    SetCreatedProperties(rowRecord);
                    entities.ChecklistRowRecord.AddObject(rowRecord);
                }

                #endregion
            }

            return result;
        }

        public ActionResult DeleteChecklistHeadRecords(SoeEntityType entity, int recordId, int actorCompanyId, List<int> checkListHeadRecordIds, int userId)
        {
            var result = new ActionResult(true);

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        result = DeleteChecklistHeadRecords(entities, transaction, actorCompanyId, checkListHeadRecordIds, userId);

                        #region Save

                        if (result.Success)
                        {
                            if (entity == SoeEntityType.Order)
                            {
                                var nofOfChecklists = GetNrOfChecklistHeadRecords(entities,SoeEntityType.Order, recordId, actorCompanyId);
                                if (nofOfChecklists < 2)
                                {
                                    var order = InvoiceManager.GetCustomerInvoice(entities, recordId);
                                    if (order != null)
                                    {
                                        order.StatusIcon = SetCheckListStatusIcon(order.StatusIcon, false);
                                    }
                                }
                            }
                            result = SaveChanges(entities, transaction);
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteChecklistHeadRecords(CompEntities entities, TransactionScope transaction, int actorCompanyId, List<int> checkListHeadRecordIds, int userId)
        {
            var result = new ActionResult(true);

            // Get user
            User user = UserManager.GetUser(entities, userId);
            if (user == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "User");


            foreach (var checkListHeadRecordId in checkListHeadRecordIds)
            {

                var headRecord = GetChecklistHeadRecordWithChecklistRowRecords(entities, checkListHeadRecordId, actorCompanyId);
                if (headRecord != null)
                {
                    result = ChangeEntityState(entities, headRecord, SoeEntityState.Deleted, false, user);
                    if (!result.Success)
                        break;

                    foreach (ChecklistRowRecord rowRecord in headRecord.ChecklistRowRecord)
                    {
                        result = ChangeEntityState(entities, rowRecord, SoeEntityState.Deleted, false, user);
                        if (!result.Success)
                            break;
                    }

                    if (!result.Success)
                        break;
                }
            }
            
            return result;
        }

        public static int SetCheckListStatusIcon(int currentStatusIcon, bool hasChecklists)
        {
            currentStatusIcon &= ~(int)SoeStatusIcon.Checklist;
            if (hasChecklists)
            {
                currentStatusIcon |= (int)SoeStatusIcon.Checklist;
            }

            return currentStatusIcon;
        }

        public ActionResult AddChecklistHeadRecords(SoeEntityType entity, int recordId, int actorCompanyId, List<int> checkListHeadIds, bool callerIsMobile)
        {
            var result = new ActionResult(true);
            var addedHeadRecords = new List<ChecklistHeadRecord>();
            if (recordId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "RecordId");

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        foreach (var checkListHeadId in checkListHeadIds)
                        {
                            ChecklistHeadRecord headRecord = null;
                            //(from c in entities.ChecklistHeadRecord
                            //                              where c.ChecklistHeadId == checkListHeadId &&
                            //                              c.ActorCompanyId == actorCompanyId &&
                            //                              c.RecordId == recordId &&
                            //                              c.Entity == (int)entity &&
                            //                              c.State == (int)SoeEntityState.Active
                            //                              select c).FirstOrDefault();

                            if (headRecord == null)
                            {
                                #region Add

                                headRecord = new ChecklistHeadRecord()
                                {
                                    Entity = (int)entity,
                                    RecordId = recordId,

                                    //Set FK
                                    ChecklistHeadId = checkListHeadId,
                                    ActorCompanyId = actorCompanyId,
                                };
                                SetCreatedProperties(headRecord);
                                entities.ChecklistHeadRecord.AddObject(headRecord);

                                addedHeadRecords.Add(headRecord);
                                #endregion
                            }
                        }

                        if (entity == SoeEntityType.Order)
                        {
                            var order = InvoiceManager.GetCustomerInvoice(entities, recordId);
                            if (order != null)
                            {
                                order.StatusIcon = SetCheckListStatusIcon(order.StatusIcon, true);
                            }
                        }

                        #region Save

                        if (result.Success)
                            result = SaveChanges(entities, transaction);

                        #endregion

                        #region Commit

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                #region Save empty rows

                List<ChecklistExtendedRowDTO> dtos = new List<ChecklistExtendedRowDTO>();

                foreach (var addedHeadRecord in addedHeadRecords)
                {
                    dtos.AddRange(GetChecklistRows(addedHeadRecord.ChecklistHeadId, entity, recordId, actorCompanyId, addedHeadRecord.ChecklistHeadRecordId));
                }

                result = SaveChecklistRecords(dtos, entity, recordId, actorCompanyId, callerIsMobile);

                #endregion

                return result;
            }
        }

        #endregion

        #region ChecklistSignatures

        public IEnumerable<ImagesDTO> GetEntityChecklistSignatures(int actorCompanyId, SoeEntityType entity, int orderChecklistId)
        {
            GraphicsManager grm = new GraphicsManager(null);
            GeneralManager gm = new GeneralManager(null);
            List<ImagesDTO> items = new List<ImagesDTO>();

            items.AddRange(grm.GetImages(actorCompanyId, SoeEntityType.ChecklistHeadRecord, orderChecklistId, SoeEntityImageType.ChecklistHeadRecordSignature, SoeEntityImageType.ChecklistHeadRecordSignatureExecutor).ToDTOs(true).ToList());
            items.AddRange(gm.GetDataStorageRecords(actorCompanyId, base.RoleId, orderChecklistId, SoeEntityType.ChecklistHeadRecord, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.ChecklistHeadRecordSignature, SoeDataStorageRecordType.ChecklistHeadRecordSignatureExecutor }, false, true).ToImagesDTOs(true));

            return items;
        }

        public IEnumerable<ImagesDTO> GetEntityChecklistsSignatures(int actorCompanyId, SoeEntityType entity, int invoiceId)
        {
            GraphicsManager grm = new GraphicsManager(null);
            GeneralManager gm = new GeneralManager(null);
            List<ImagesDTO> items = new List<ImagesDTO>();

            var orderChecklists = GetChecklistHeadRecords(entity, invoiceId, actorCompanyId);

            foreach (var orderChecklist in orderChecklists)
            {
                items.AddRange(grm.GetImages(actorCompanyId, SoeEntityType.ChecklistHeadRecord, orderChecklist.ChecklistHeadRecordId, SoeEntityImageType.ChecklistHeadRecordSignature, SoeEntityImageType.ChecklistHeadRecordSignatureExecutor).ToDTOs(true).ToList());
                items.AddRange(gm.GetDataStorageRecords(actorCompanyId, base.RoleId, orderChecklist.ChecklistHeadRecordId, SoeEntityType.ChecklistHeadRecord, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.ChecklistHeadRecordSignature, SoeDataStorageRecordType.ChecklistHeadRecordSignatureExecutor }, false, true).ToImagesDTOs(true));
            }

            return items;
        }

        public ActionResult SaveChecklistSignature(int actorCompanyId, int invoiceId, int checklistHeadRecordId, SoeDataStorageRecordType dataStorageRecordType, byte[] imageData, string description)
        {
            if (dataStorageRecordType != SoeDataStorageRecordType.ChecklistHeadRecordSignature && dataStorageRecordType != SoeDataStorageRecordType.ChecklistHeadRecordSignatureExecutor)
                return new ActionResult("Error: wrong type, type kan only be 6 or 8"); //this should never happen in production

            ChecklistHeadRecord orderChecklist = ChecklistManager.GetChecklistHeadRecordWithCheckListHead(SoeEntityType.Order, invoiceId, checklistHeadRecordId, actorCompanyId);

            if (orderChecklist == null)
                return new ActionResult( GetText(9074, "Checklista är ej kopplad till order"));

            var record = new DataStorageRecordExtendedDTO
            {
                Data = imageData,
                Type = dataStorageRecordType,
                Entity = SoeEntityType.ChecklistHeadRecord,
                FileName = dataStorageRecordType.ToString() + ".jpg",
                Description = description,
                RecordId = orderChecklist.ChecklistHeadRecordId
            };
            return GeneralManager.SaveDataStorageRecord(ActorCompanyId, record, false);
        }

        #endregion

        #region MultipleChoiceAnswerHead

        public List<CheckListMultipleChoiceAnswerHead> GetChecklistMultipleChoiceHeads(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CheckListMultipleChoiceAnswerHead.NoTracking();
            List<CheckListMultipleChoiceAnswerHead> multipleChoiceheads = (from c in entities.CheckListMultipleChoiceAnswerHead
                                                                           where c.ActorCompanyId == actorCompanyId
                                                                           select c).ToList();

            return multipleChoiceheads;
        }

        public CheckListMultipleChoiceAnswerHead GetChecklistMultipleChoiceAnswerHead(CompEntities entities, int checklistMultipleChoiceHeadId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.CheckListMultipleChoiceAnswerHead.NoTracking();

            CheckListMultipleChoiceAnswerHead head = (from c in entities.CheckListMultipleChoiceAnswerHead.Include("CheckListMultipleChoiceAnswerRow")
                                                      where c.CheckListMultipleChoiceAnswerHeadId == checklistMultipleChoiceHeadId &&
                                                      c.ActorCompanyId == actorCompanyId
                                                      select c).FirstOrDefault();

            return head;
        }

        public ActionResult SaveMultipleChoiceQuestion(CheckListMultipleChoiceAnswerHeadDTO checklistMultipleChoiceHeadInput, List<CheckListMultipleChoiceAnswerRowDTO> CheckListMultipleChoiceAnswerRowInput, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            if (checklistMultipleChoiceHeadInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChecklistHead");

            int checklistMultipleChoiceHeadId = 0;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Title
                        if (String.IsNullOrEmpty(checklistMultipleChoiceHeadInput.Title))
                        {
                            result = new ActionResult(false);
                            return result;
                        }

                        #endregion

                        #region Perform

                        CheckListMultipleChoiceAnswerHead checklistMultipleChoiceHead = null;
                        if (checklistMultipleChoiceHeadInput.CheckListMultipleChoiceAnswerHeadId == 0)
                        {
                            #region Add

                            #region checklistMultipleChoiceHead

                            checklistMultipleChoiceHead = new CheckListMultipleChoiceAnswerHead()
                            {
                                Title = checklistMultipleChoiceHeadInput.Title,
                                ActorCompanyId = actorCompanyId,
                                State = 0,
                            };
                            SetCreatedProperties(checklistMultipleChoiceHead);
                            entities.CheckListMultipleChoiceAnswerHead.AddObject(checklistMultipleChoiceHead);

                            #endregion

                            #region ChecklistMultipleChoiceRow

                            foreach (var checklistMultipleChoiceRowInput in CheckListMultipleChoiceAnswerRowInput)
                            {
                                CheckListMultipleChoiceAnswerRow checklistMultipleChoiceRow = new CheckListMultipleChoiceAnswerRow()
                                {
                                    Question = checklistMultipleChoiceRowInput.Question,
                                    CheckListMultipleChoiceAnswerHead = checklistMultipleChoiceHead,
                                };
                                SetCreatedProperties(checklistMultipleChoiceRow);
                                checklistMultipleChoiceHead.CheckListMultipleChoiceAnswerRow.Add(checklistMultipleChoiceRow);
                            }

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Update

                            checklistMultipleChoiceHead = GetChecklistMultipleChoiceAnswerHead(entities, checklistMultipleChoiceHeadInput.CheckListMultipleChoiceAnswerHeadId, actorCompanyId);
                            if (checklistMultipleChoiceHead == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "ChecklistMultipleChoiceAnswerHead");

                            #region Head

                            checklistMultipleChoiceHead.Title = checklistMultipleChoiceHeadInput.Title;
                            SetModifiedProperties(checklistMultipleChoiceHead);

                            #endregion

                            #region Row

                            var checklistMultipleChoiceRowsInput = CheckListMultipleChoiceAnswerRowInput.ToList();
                            var checklistMultipleChoiceRows = checklistMultipleChoiceHead.CheckListMultipleChoiceAnswerRow.OrderBy(i => i.CheckListMultipleChoiceAnswerRowId).ToList();

                            #region Add

                            foreach (CheckListMultipleChoiceAnswerRowDTO checklistMultipleChoiceRowToAdd in checklistMultipleChoiceRowsInput.Where(i => i.CheckListMultipleChoiceAnswerRowId == 0))
                            {
                                CheckListMultipleChoiceAnswerRow checklistMultipleChoiceRow = new CheckListMultipleChoiceAnswerRow()
                                {
                                    Question = checklistMultipleChoiceRowToAdd.Question,
                                };
                                SetCreatedProperties(checklistMultipleChoiceRow);

                                checklistMultipleChoiceHead.CheckListMultipleChoiceAnswerRow.Add(checklistMultipleChoiceRow);
                            }

                            #endregion

                            #region Update/Delete

                            //Check all existing ChecklistRows if they are updated or deleted in input collection
                            for (int idx = checklistMultipleChoiceRows.Count - 1; idx >= 0; idx--)
                            {
                                var checklistMultipleChoiceRow = checklistMultipleChoiceRows.ElementAt(idx);
                                var checklistMultipleChoiceRowInput = checklistMultipleChoiceRowsInput.FirstOrDefault(i => i.CheckListMultipleChoiceAnswerRowId == checklistMultipleChoiceRow.CheckListMultipleChoiceAnswerRowId);
                                if (checklistMultipleChoiceRowInput == null)
                                {
                                    #region Delete

                                    ChangeEntityState(checklistMultipleChoiceRow, SoeEntityState.Deleted);

                                    #endregion
                                }
                                else
                                {
                                    #region Update

                                    checklistMultipleChoiceRow.Question = checklistMultipleChoiceRowInput.Question;
                                    SetModifiedProperties(checklistMultipleChoiceRow);

                                    #endregion
                                }
                            }

                            #endregion

                            #endregion

                            #endregion
                        }

                        #endregion

                        #region Save

                        if (result.Success)
                        {
                            result = SaveChanges(entities, transaction);
                            checklistMultipleChoiceHeadId = checklistMultipleChoiceHead.CheckListMultipleChoiceAnswerHeadId;
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    result.IntegerValue = checklistMultipleChoiceHeadId;

                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion

        #region MultipleChoiceAnswerRow

        public CheckListMultipleChoiceAnswerRow GetChecklistMultipleChoiceAnswerRow(int multipleChoiceAnswerRowId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CheckListMultipleChoiceAnswerRow.NoTracking();
            var answer = (from c in entities.CheckListMultipleChoiceAnswerRow
                          where c.CheckListMultipleChoiceAnswerRowId == multipleChoiceAnswerRowId
                          select c).FirstOrDefault();

            return answer;
        }

        public List<CheckListMultipleChoiceAnswerRow> GetChecklistMultipleChoiceRows(int multipleChoiceAnswerHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CheckListMultipleChoiceAnswerRow.NoTracking();
            List<CheckListMultipleChoiceAnswerRow> multipleChoicerows = (from c in entities.CheckListMultipleChoiceAnswerRow
                                                                         where c.CheckListMultipleChoiceAnswerHeadId == multipleChoiceAnswerHeadId &&
                                                                         c.State == (int)SoeEntityState.Active
                                                                         select c).ToList();

            return multipleChoicerows;
        }

        public List<CheckListMultipleChoiceAnswerRowDTO> GetChecklistMultipleChoiceRows(List<int> multipleChoiceAnswerHeadIds)
        {
            List<CheckListMultipleChoiceAnswerRowDTO> rows = new List<CheckListMultipleChoiceAnswerRowDTO>();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CheckListMultipleChoiceAnswerRow.NoTracking();
            var multipleChoicerows = (from c in entities.CheckListMultipleChoiceAnswerRow
                                                                         where multipleChoiceAnswerHeadIds.Contains(c.CheckListMultipleChoiceAnswerHeadId)
                                                                         select c);
            
            foreach (int id in multipleChoiceAnswerHeadIds)
            {
                var mcrs = multipleChoicerows.Where(h => h.CheckListMultipleChoiceAnswerHeadId == id);
                if (mcrs.Any())
                {
                    rows.Add(new CheckListMultipleChoiceAnswerRowDTO() { CheckListMultipleChoiceAnswerHeadId = id, CheckListMultipleChoiceAnswerRowId = 0, Question = "" });
                    rows.AddRange(mcrs.ToDTOs());
                }
            }

            return rows;
        }

        #endregion

        #region ChecklistDocuments

        public Dictionary<string, byte[]> GetChecklistAsDocuments(ReportManager rm, ReportDataManager rdm, int invoiceId, List<int> checklistIds, int actorCompanyId)
        {
            var attachments = new Dictionary<string, byte[]>();
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

			var checklistReports = ReportManager.GetReportsByTemplateType(actorCompanyId, null, SoeReportTemplateType.OrderChecklistReport, onlyOriginal: true);
            if (checklistReports.IsNullOrEmpty())
                throw new Exception("GetChecklists failed finding reports");

            //koppling från checklistid -> report (ChecklistHeadRecord -> ChecklistHead där reportid ligger)
            var defaultReport = rm.GetReport(checklistReports[0].ReportId, actorCompanyId);
            var evaluatedSelections = new List<EvaluatedSelection>();

            List<ChecklistHeadRecord> recordHeads = (from c in entitiesReadOnly.ChecklistHeadRecord
                                               where c.ActorCompanyId == actorCompanyId &&
                                               c.State == (int)SoeEntityState.Active &&
                                               checklistIds.Contains(c.ChecklistHeadRecordId)
                                               select c).ToList();

            foreach (var recordHead in recordHeads)
            {
                var checklistId = recordHead.ChecklistHeadRecordId;
                recordHead.ChecklistHeadReference.Load();
                var report = checklistReports.FirstOrDefault(r => r.ReportId == recordHead.ChecklistHead.ReportId);
                report = report == null ? defaultReport : report;
                //GetReport med rätt report id
                evaluatedSelections.Clear();

                var reportItem = rm.GetChecklistPrintUrlDTO(invoiceId, checklistId, report.ReportId);

                var selection = new Selection(actorCompanyId, this.UserId, this.parameterObject.RoleId, this.LoginName, report: report.ToDTO(), isMainReport: true, exportType: (int)TermGroup_ReportExportType.Pdf, exportFileType: 0);
                selection.Evaluate(reportItem, 0);
                evaluatedSelections.Add(selection.Evaluated);

                ReportPrintoutDTO dto = rdm.PrintReportDTO(evaluatedSelections[0],true);
                if ((dto == null) || (dto.Status == (int)TermGroup_ReportPrintoutStatus.Error))
                {
                    throw new Exception("Running checklist report ended with error");
                }
                else
                {
                    attachments.Add($"{report.Name}_{checklistId}.pdf", dto.Data);
                }
            }

            return attachments;
        }

        #endregion
    }
}
