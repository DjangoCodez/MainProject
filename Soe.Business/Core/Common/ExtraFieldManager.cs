using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class ExtraFieldManager : ManagerBase
    {
        #region Ctor

        public ExtraFieldManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region ExtraField

        public List<ExtraField> GetExtraFields(int actorCompanyId, bool loadRecords = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExtraField.NoTracking();
            return GetExtraFields(entities, actorCompanyId, loadRecords);
        }

        public List<ExtraField> GetExtraFields(int entity, int actorCompanyId, bool loadRecords = false, int connectedEntity = 0, int connectedRecordId = 0, bool loadExternalCodes = false, bool loadValues = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExtraField.NoTracking();
            return GetExtraFields(entities, entity, actorCompanyId, loadRecords, connectedEntity, connectedRecordId, loadExternalCodes, loadValues);
        }

        public List<ExtraFieldGridDTO> GetExtraFieldGridDTOs(int entity, int actorCompanyId, bool loadRecords = false, int connectedEntity = 0, int connectedRecordId = 0, int? extraFieldId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExtraField.NoTracking();
            return GetExtraFieldGridDTOs(entities, entity, actorCompanyId, loadRecords, connectedEntity, connectedRecordId, extraFieldId);
        }

        public List<ExtraField> GetExtraFields(CompEntities entities, int actorCompanyId, bool loadRecords = false, bool loadValues = false)
        {
            var query = (from ef in entities.ExtraField
                         where ef.ActorCompanyId == actorCompanyId &&
                         ef.State == (int)SoeEntityState.Active
                         select ef);

            if (loadRecords)
                query = query.Include("ExtraFieldRecord");
            if (loadValues)
                query = query.Include("ExtraFieldValue");

            return query.ToList();
        }

        public List<ExtraField> GetExtraFields(CompEntities entities, int entity, int actorCompanyId, bool loadRecords = false, int connectedEntity = 0, int connectedRecordId = 0, bool loadExternalCodes = false, bool loadValues = false)
        {
            IQueryable<ExtraField> query = (from ef in entities.ExtraField
                                            where ef.ActorCompanyId == actorCompanyId &&
                                            ef.Entity == entity &&
                                            ef.State == (int)SoeEntityState.Active
                                            orderby ef.Text
                                            select ef);

            if (connectedEntity > 0 && connectedRecordId > 0)
                query = (from ef in query
                         where ef.ConnectedEntity == connectedEntity &&
                         ef.ConnectedRecordId == connectedRecordId
                         select ef);

            if (loadRecords)
                query = query.Include("ExtraFieldRecord");
            if (loadValues)
                query = query.Include("ExtraFieldValue");

            List<ExtraField> result = query.OrderBy(ef => ef.Text).ToList();
            if (!result.IsNullOrEmpty() && loadExternalCodes)
            {
                List<CompanyExternalCode> externalCodes = ActorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.ExtraField, actorCompanyId);
                if (!externalCodes.IsNullOrEmpty())
                {
                    foreach (ExtraField extraField in result)
                    {
                        List<CompanyExternalCode> matching = externalCodes.Where(w => w.RecordId == extraField.ExtraFieldId).ToList();
                        if (matching.IsNullOrEmpty())
                            continue;

                        extraField.ExternalCodes = matching.Select(s => s.ExternalCode).ToList();
                    }
                }
            }

            return result;
        }

        public Dictionary<int, string> GetExtraFieldsDict(int entity, int actorCompanyId, int connectedEntity = 0, int connectedRecordId = 0, bool addEmptyRow = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, "");

            List<ExtraField> fields = GetExtraFields(entity, actorCompanyId, false, connectedEntity, connectedRecordId);
            foreach (ExtraField field in fields)
            {
                dict.Add(field.ExtraFieldId, field.Text);
            }

            return dict;
        }

        public List<ExtraFieldGridDTO> GetExtraFieldGridDTOs(CompEntities entities, int entity, int actorCompanyId, bool loadRecords = false, int connectedEntity = 0, int connectedRecordId = 0, int? extraFieldId = null)
        {
            IQueryable<ExtraField> query = (from ef in entities.ExtraField select ef);
            if (loadRecords)
                query = query.Include("ExtraFieldRecord");

            if (connectedEntity > 0 && connectedRecordId > 0)
                query = query.Where(ef => ef.ConnectedEntity == connectedEntity && ef.ConnectedRecordId == connectedRecordId);

            if (extraFieldId.HasValue)
                query = query.Where(ef => ef.ExtraFieldId == extraFieldId.Value);

            return (from ef in query
                    join ad in entities.AccountDim on ef.ConnectedRecordId equals ad.AccountDimId into efAds
                    from efAd in efAds.DefaultIfEmpty()
                    where ef.ActorCompanyId == actorCompanyId &&
                    ef.Entity == entity &&
                    ef.State == (int)SoeEntityState.Active
                    orderby ef.Text
                    select new ExtraFieldGridDTO
                    {
                        ExtraFieldId = ef.ExtraFieldId,
                        Text = ef.Text,
                        Type = ef.Type,
                        AccountDimId = efAd == null ? 0 : efAd.AccountDimId,
                        AccountDimName = efAd == null ? string.Empty : efAd.Name,
                        SysExtraFieldId = ef.SysExtraFieldId,
                    }).ToList();
        }

        public ExtraField GetExtraField(int extraFieldId, bool loadRecords = false, bool loadValues = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExtraField.NoTracking();
            return GetExtraField(entities, extraFieldId, loadRecords, loadValues);
        }

        public ExtraField GetExtraField(CompEntities entities, int extraFieldId, bool loadRecords = false, bool loadValues = false)
        {
            if (extraFieldId <= 0)
                return null;

            IQueryable<ExtraField> query = entities.ExtraField;
            if (loadRecords)
                query = query.Include("ExtraFieldRecord");
            if (loadValues)
                query = query.Include("ExtraFieldValue");

            ExtraField field = (from t in query
                                where t.ExtraFieldId == extraFieldId
                                select t).FirstOrDefault();

            // Get translations
            if (field != null)
                field.Translations = TermManager.GetCompTerms(entities, CompTermsRecordType.ExtraField, extraFieldId);

            if (field != null)
            {
                List<CompanyExternalCode> externalCodes = ActorManager.GetCompanyExternalCodes(entities, TermGroup_CompanyExternalCodeEntity.ExtraField, field.ExtraFieldId, field.ActorCompanyId);
                if (!externalCodes.IsNullOrEmpty())
                    field.ExternalCodes = externalCodes.Select(s => s.ExternalCode).ToList();
            }

            return field;
        }

        public List<ExtraFieldValue> GetExtraFieldValues(int extraFieldId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExtraFieldValue.NoTracking();
            return GetExtraFieldValues(entities, extraFieldId);
        }

        public List<ExtraFieldValue> GetExtraFieldValues(CompEntities entities, int extraFieldId)
        {
            return (from v in entities.ExtraFieldValue
                    where v.ExtraFieldId == extraFieldId &&
                    v.State == (int)SoeEntityState.Active
                    select v).ToList();
        }

        private bool ExtraFieldExists(string text, int entity, int? connectedEntity, int? connectedRecordId, int actorCompanyId, int? extraFielId = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ExtraField.NoTracking();
            var query = from ef in entitiesReadOnly.ExtraField
                        where ef.Company.ActorCompanyId == actorCompanyId
                                && ef.Text == text && ef.Entity == entity
                                && ef.State != (int)SoeEntityState.Deleted
                        select ef;

            if (connectedEntity.HasValue)
                query = query.Where(ef => ef.ConnectedEntity == connectedEntity.Value);

            if (connectedRecordId.HasValue)
                query = query.Where(ef => ef.ConnectedRecordId == connectedRecordId.Value);

            if (extraFielId.HasValue)
                query = query.Where(ef => ef.ExtraFieldId != extraFielId.Value);

            return query.Any();
        }

        public ActionResult SaveExtraField(ExtraFieldDTO extraFieldInput, int actorCompanyId)
        {
            if (extraFieldInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92032, "Extra fält kan inte hittas."));

            ActionResult result;

            using (CompEntities entities = new CompEntities())
            {
                int extraFieldId = 0;

                ExtraField existingExtraField;

                if (extraFieldInput.ExtraFieldId <= 0)
                {
                    if (ExtraFieldExists(extraFieldInput.Text, (int)extraFieldInput.Entity, extraFieldInput.ConnectedEntity, extraFieldInput.ConnectedRecordId, actorCompanyId))
                        return new ActionResult((int)ActionResultSave.EntityExists, string.Format(
                                GetText(92030, (int)TermGroup.General, "{0}: {1} finns redan."),
                                GetText(4078, (int)TermGroup.General, "Titel"),
                                extraFieldInput.Text
                                ));

                    existingExtraField = new ExtraField()
                    {
                        ActorCompanyId = actorCompanyId,
                        SysExtraFieldId = extraFieldInput.SysExtraFieldId,
                        Entity = (int)extraFieldInput.Entity,
                        Text = extraFieldInput.Text,
                        Type = (int)extraFieldInput.Type,
                        ConnectedEntity = extraFieldInput.ConnectedEntity,
                        ConnectedRecordId = extraFieldInput.ConnectedRecordId
                    };

                    SetCreatedProperties(existingExtraField);
                    entities.ExtraField.AddObject(existingExtraField);

                    result = AddEntityItem(entities, existingExtraField, "ExtraField");

                    extraFieldId = existingExtraField.ExtraFieldId;
                }
                else
                {
                    existingExtraField = GetExtraField(entities, extraFieldInput.ExtraFieldId, false, true);

                    if (ExtraFieldExists(extraFieldInput.Text, (int)extraFieldInput.Entity, extraFieldInput.ConnectedEntity, extraFieldInput.ConnectedRecordId, actorCompanyId, existingExtraField.ExtraFieldId))
                    {
                        return new ActionResult((int)ActionResultSave.EntityExists, string.Format(
                                GetText(92030, (int)TermGroup.General, "{0}: {1} finns redan."),
                                GetText(9177, (int)TermGroup.General, "Titel"),
                                extraFieldInput.Text
                                ));
                    }

                    existingExtraField.Text = extraFieldInput.Text;
                    existingExtraField.Type = (int)extraFieldInput.Type;
                    existingExtraField.ConnectedEntity = extraFieldInput.ConnectedEntity;
                    existingExtraField.ConnectedRecordId = extraFieldInput.ConnectedRecordId;

                    SetModifiedProperties(existingExtraField);

                    result = SaveChanges(entities);

                    extraFieldId = existingExtraField.ExtraFieldId;
                }

                #region ExtraFieldValues

                if (extraFieldInput.ExtraFieldValues != null)
                {
                    #region Delete rows that exists in database but not in input

                    foreach (ExtraFieldValue existingValue in existingExtraField.ExtraFieldValue.Where(e => e.State != (int)SoeEntityState.Deleted).ToList())
                    {
                        if (!extraFieldInput.ExtraFieldValues.Any(x => x.ExtraFieldValueId == existingValue.ExtraFieldValueId))
                            ChangeEntityState(existingValue, SoeEntityState.Deleted);
                    }

                    #endregion

                    #region Add/update values

                    foreach (ExtraFieldValueDTO inputValue in extraFieldInput.ExtraFieldValues)
                    {
                        ExtraFieldValue existingValue = null;
                        if (inputValue.ExtraFieldValueId != 0)
                            existingValue = existingExtraField.ExtraFieldValue.Where(e => e.State != (int)SoeEntityState.Deleted).FirstOrDefault(e => e.ExtraFieldValueId == inputValue.ExtraFieldValueId);

                        if (existingValue == null)
                        {
                            existingValue = new ExtraFieldValue()
                            {
                                ExtraField = existingExtraField,
                                Type = (int)inputValue.Type,
                            };
                            SetCreatedProperties(existingValue);
                            entities.ExtraFieldValue.AddObject(existingValue);
                        }
                        else
                        {
                            SetModifiedProperties(existingValue);
                        }

                        existingValue.Value = inputValue.Value;
                        existingValue.Sort = inputValue.Sort;
                    }

                    #endregion
                }

                #endregion

                #region Translations

                if (extraFieldInput.Translations != null)
                {
                    List<int> langIdsToSave = extraFieldInput.Translations.Select(i => (int)i.Lang).Distinct().ToList();
                    List<CompTerm> existingTranslations = TermManager.GetCompTerms(entities, CompTermsRecordType.ExtraField, extraFieldId);

                    #region Delete existing translations for other languages

                    foreach (var existingTranslation in existingTranslations)
                    {
                        if (langIdsToSave.Contains(existingTranslation.LangId))
                            continue;

                        existingTranslation.State = (int)SoeEntityState.Deleted;
                    }

                    #endregion

                    #region Add or update translations for languages

                    foreach (int langId in langIdsToSave)
                    {
                        CompTerm translation = null;
                        var inputTranslation = extraFieldInput.Translations.FirstOrDefault(i => (int)i.Lang == langId);

                        var existingTranslationsForLang = existingTranslations.Where(i => i.LangId == langId).ToList();
                        if (existingTranslationsForLang.Count == 0)
                        {
                            #region Add

                            translation = new CompTerm { ActorCompanyId = ActorCompanyId };
                            entities.CompTerm.AddObject(translation);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            for (int i = 0; i < existingTranslationsForLang.Count; i++)
                            {
                                if (i > 0)
                                {
                                    //Remove duplicates
                                    existingTranslationsForLang[i].State = (int)SoeEntityState.Deleted;
                                    continue;
                                }

                                translation = existingTranslationsForLang[i];
                            }

                            #endregion
                        }

                        #region Set values

                        if (translation != null)
                        {
                            translation.RecordType = (int)CompTermsRecordType.ExtraField;
                            translation.RecordId = extraFieldId;
                            translation.LangId = (int)inputTranslation.Lang;
                            translation.Name = inputTranslation.Name;
                            translation.State = (int)SoeEntityState.Active;
                        }

                        #endregion
                    }

                    #endregion

                    result = SaveChanges(entities);
                    if (!result.Success)
                    {
                        result.ErrorNumber = (int)ActionResultSave.TranslationsSaveFailed;
                        return result;
                    }
                }

                #endregion

                result.IntegerValue = extraFieldId;

                if (result.Success)
                    ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.ExtraField, extraFieldId, extraFieldInput.ExternalCodesString, actorCompanyId);

                return result;
            }
        }

        public ActionResult DeleteExtraField(int extraFieldId)
        {
            using (CompEntities entities = new CompEntities())
            {
                var originalExtraField = GetExtraField(entities, extraFieldId, true);
                return DeleteExtraField(entities, originalExtraField);
            }
        }

        public ActionResult DeleteExtraField(CompEntities entities, ExtraField originalExtraField)
        {
            if (originalExtraField == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(92032, "Extra fält kan inte hittas."));

            if (originalExtraField.ExtraFieldRecord.Any(x => x.BoolData.HasValue || x.DateData.HasValue || x.DecimalData.HasValue || x.IntData.HasValue || !string.IsNullOrEmpty(x.StrData)))
                return new ActionResult(string.Format(
                    GetText(92031, "Borttagning misslyckades. {0}: {1} används."),
                    GetText(9177, (int)TermGroup.General, "Titel"),
                    originalExtraField.Text
                    )
                );

            if (originalExtraField.ExtraFieldRecord.Count > 0)
            {
                // If there are records, we need to delete them first
                foreach (var record in originalExtraField.ExtraFieldRecord.ToList())
                {
                    ChangeEntityState(entities, record, SoeEntityState.Deleted, true);
                }
            }
            //Remove Textblock
            return ChangeEntityState(entities, originalExtraField, SoeEntityState.Deleted, true);
        }

        public List<ExtraField> GetExtraFieldsAndRecordsForSysType(CompEntities entities, SoeEntityType entityType, SysExtraFieldType? sysExtraFieldType = null, List<SysExtraField> sysExtraFields = null)
        {
            List<ExtraField> extraFields = new List<ExtraField>();
            if (sysExtraFields == null)
                sysExtraFields = GetSysExtraFields(entityType);

            if (sysExtraFieldType != null)
                sysExtraFields = sysExtraFields.Where(sef => sef.SysType == (int)sysExtraFieldType.Value).ToList();

            if (sysExtraFields.Any())
            {
                IQueryable<ExtraField> query = entities.ExtraField.Include("ExtraFieldRecord");

                query = from ef in query
                        where ef.SysExtraFieldId.HasValue && ef.SysExtraFieldId.Value != 0
                        && ef.State == (int)SoeEntityState.Active
                        select ef;

                extraFields = query.ToList();

                // Filter only those matching the sys extra fields
                extraFields = extraFields.Where(ef => sysExtraFields.Any(sef => sef.SysExtraFieldId == ef.SysExtraFieldId)).ToList();
            }

            return extraFields;
        }

        #endregion

        #region ExtraFieldRecord

        public List<ExtraFieldRecord> GetExtraFieldRecords(int recordId, int entity, int actorCompanyId, bool loadExtraField = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExtraFieldRecord.NoTracking();
            return GetExtraFieldRecords(entities, recordId, entity, actorCompanyId, loadExtraField);
        }

        public List<ExtraFieldRecord> GetExtraFieldRecords(CompEntities entities, int recordId, int entity, int actorCompanyId, bool loadExtraField = false)
        {
            IQueryable<ExtraFieldRecord> query = entities.ExtraFieldRecord;
            if (loadExtraField)
                query = query.Include("ExtraField");

            return query.Where(efr => efr.RecordId == recordId &&
                    efr.Entity == entity &&
                    efr.State == (int)SoeEntityState.Active
                    ).Select(s => s).ToList();
        }

        public List<ExtraFieldRecord> GetExtraFieldRecords(List<int> recordIds, int entity, int actorCompanyId, bool loadRecords = false, bool loadValues = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExtraFieldRecord.NoTracking();
            return GetExtraFieldRecords(entities, recordIds, entity, actorCompanyId, loadRecords, loadValues);
        }

        public List<ExtraFieldRecord> GetExtraFieldRecords(CompEntities entities, List<int> recordIds, int entity, int actorCompanyId, bool loadRecords = false, bool loadValues = false)
        {
            IQueryable<ExtraFieldRecord> query = entities.ExtraFieldRecord;
            if (loadRecords)
                query = query.Include("ExtraField");
            if (loadValues)
                query = query.Include("ExtraField.ExtraFieldValue");

            return query.Where(efr => efr.RecordId.HasValue && recordIds.Contains(efr.RecordId.Value) &&
                   efr.Entity == entity &&
                   efr.State == (int)SoeEntityState.Active
                   ).Select(s => s).ToList();
        }

        public ExtraFieldRecord GetExtraFieldRecord(int extraFieldId, int recordId, int entity, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExtraFieldRecord.NoTracking();
            return (from efr in entities.ExtraFieldRecord
                    where efr.ExtraFieldId == extraFieldId &&
                    efr.RecordId == recordId &&
                    efr.Entity == entity &&
                    efr.State == (int)SoeEntityState.Active
                    select efr).FirstOrDefault();
        }

        public List<ExtraFieldRecordDTO> GetExtraFieldWithRecords(int recordId, int entity, int actorCompanyId, int langId, int connectedEntity = 0, int connectedRecordId = 0)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExtraFieldRecord.NoTracking();
            return GetExtraFieldWithRecords(entities, recordId, entity, actorCompanyId, langId, connectedEntity, connectedRecordId);
        }

        public List<ExtraFieldRecordDTO> GetExtraFieldWithRecords(CompEntities entities, int recordId, int entity, int actorCompanyId, int langId, int connectedEntity = 0, int connectedRecordId = 0)
        {
            List<ExtraFieldRecordDTO> dtos = new List<ExtraFieldRecordDTO>();

            List<ExtraField> extraFields = this.GetExtraFields(entities, entity, actorCompanyId, connectedEntity: connectedEntity, connectedRecordId: connectedRecordId, loadValues: true);
            List<ExtraFieldRecord> extraFieldRecords = this.GetExtraFieldRecords(entities, recordId, entity, actorCompanyId);

            foreach (ExtraField extraField in extraFields)
            {
                ExtraFieldRecordDTO dto = new ExtraFieldRecordDTO()
                {
                    ExtraFieldId = extraField.ExtraFieldId,
                    ExtraFieldType = extraField.Type,
                    ExtraFieldText = extraField.Text,
                };

                if (extraField.ExtraFieldValue != null)
                    dto.ExtraFieldValues = extraField.ExtraFieldValue.Where(e => e.State == (int)SoeEntityState.Active).ToDTOs().ToList();

                if (langId != 0)
                {
                    CompTerm translation = TermManager.GetCompTerm(CompTermsRecordType.ExtraField, extraField.ExtraFieldId, langId);
                    if (translation != null && !string.IsNullOrEmpty(translation.Name))
                        dto.ExtraFieldText = translation.Name;
                }

                dto.DataTypeId = SetExtraFieldRecordDataType(dto.ExtraFieldType);

                ExtraFieldRecord record = extraFieldRecords.FirstOrDefault(r => r.ExtraFieldId == extraField.ExtraFieldId);
                if (record != null)
                {
                    dto.ExtraFieldRecordId = record.ExtraFieldRecordId;
                    dto.StrData = record.StrData;
                    dto.IntData = record.IntData;
                    dto.BoolData = record.BoolData;
                    dto.DecimalData = record.DecimalData;
                    dto.DateData = record.DateData;
                }

                dtos.Add(dto);
            }

            return dtos;
        }

        public int SetExtraFieldRecordDataType(int extraFieldType)
        {
            var datatypeId = 0;

            switch (extraFieldType)
            {
                case (int)TermGroup_ExtraFieldType.Checkbox:
                    datatypeId = (int)MatrixDataType.Boolean;
                    break;
                case (int)TermGroup_ExtraFieldType.YesNo:
                    datatypeId = (int)MatrixDataType.Integer;
                    break;
                case (int)TermGroup_ExtraFieldType.Integer:
                    datatypeId = (int)MatrixDataType.Integer;
                    break;
                case (int)TermGroup_ExtraFieldType.FreeText:
                    datatypeId = (int)MatrixDataType.String;
                    break;
                case (int)TermGroup_ExtraFieldType.Decimal:
                    datatypeId = (int)MatrixDataType.Decimal;
                    break;
                case (int)TermGroup_ExtraFieldType.Date:
                    datatypeId = (int)MatrixDataType.Date;
                    break;
                default:
                    break;
            }

            return datatypeId;
        }

        public List<ExtraFieldRecordDTO> GetExtraFieldWithRecords(List<int> recordIds, int entity, int actorCompanyId, int langId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ExtraFieldRecord.NoTracking();
            return GetExtraFieldWithRecords(entities, recordIds, entity, actorCompanyId, langId);
        }

        public List<ExtraFieldRecordDTO> GetExtraFieldWithRecords(CompEntities entities, List<int> recordIds, int entity, int actorCompanyId, int langId)
        {
            List<ExtraFieldRecordDTO> dtos = new List<ExtraFieldRecordDTO>();

            List<ExtraField> extraFields = this.GetExtraFields(entities, entity, actorCompanyId);
            List<ExtraFieldRecord> extraFieldRecords = this.GetExtraFieldRecords(entities, recordIds, entity, actorCompanyId);

            foreach (ExtraField extraField in extraFields)
            {
                ExtraFieldRecordDTO dto = new ExtraFieldRecordDTO()
                {
                    ExtraFieldId = extraField.ExtraFieldId,
                    ExtraFieldType = extraField.Type,
                    ExtraFieldText = extraField.Text,
                };

                if (langId != 0)
                {
                    CompTerm translation = TermManager.GetCompTerm(CompTermsRecordType.ExtraField, extraField.ExtraFieldId, langId);
                    if (translation != null && !string.IsNullOrEmpty(translation.Name))
                        dto.ExtraFieldText = translation.Name;
                }

                ExtraFieldRecord record = extraFieldRecords.FirstOrDefault(r => r.ExtraFieldId == extraField.ExtraFieldId);
                if (record != null)
                {
                    dto.ExtraFieldRecordId = record.ExtraFieldRecordId;
                    dto.RecordId = record.RecordId ?? 0;
                    dto.DataTypeId = record.DataTypeId;
                    dto.StrData = record.StrData;
                    dto.IntData = record.IntData;
                    dto.BoolData = record.BoolData;
                    dto.DecimalData = record.DecimalData;
                    dto.DateData = record.DateData;
                }

                dtos.Add(dto);
            }

            return dtos;
        }

        public ActionResult SaveExtraFieldRecords(List<ExtraFieldRecordDTO> extraFieldRecords, int entity, int recordId, int actorCompanyId)
        {
            if (extraFieldRecords == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ExtraFieldRecordDTO");

            using (CompEntities entities = new CompEntities())
            {
                return SaveExtraFieldRecords(entities, extraFieldRecords, entity, recordId, actorCompanyId);
            }
        }

        public ActionResult SaveExtraFieldRecords(CompEntities entities, List<ExtraFieldRecordDTO> extraFieldRecords, int entity, int recordId, int actorCompanyId)
        {
            var existingRecords = GetExtraFieldRecords(entities, recordId, entity, actorCompanyId);

            foreach (var record in extraFieldRecords.Where(r => r.ExtraFieldRecordId > 0 || r.BoolData.HasValue || r.DecimalData.HasValue || r.IntData.HasValue || r.DateData.HasValue || r.StrData != String.Empty))
            {
                ExtraFieldRecord extraFieldRecord = null;
                if (record.ExtraFieldRecordId != 0)
                {
                    // Get existing by id
                    extraFieldRecord = existingRecords.FirstOrDefault(r => r.ExtraFieldRecordId == record.ExtraFieldRecordId);
                }
                else
                {
                    // Get existing by extra field type
                    // This will happen if registering a new employment from employee template
                    extraFieldRecord = existingRecords.FirstOrDefault(r => r.ExtraFieldId == record.ExtraFieldId);
                }

                if (extraFieldRecord == null)
                {
                    // New
                    extraFieldRecord = new ExtraFieldRecord();
                    extraFieldRecord.ExtraFieldId = record.ExtraFieldId;
                    extraFieldRecord.RecordId = recordId;
                    extraFieldRecord.Entity = entity;
                    extraFieldRecord.DataTypeId = GetDataTypeId((TermGroup_ExtraFieldType)record.ExtraFieldType);

                    SetCreatedProperties(extraFieldRecord);
                    entities.ExtraFieldRecord.AddObject(extraFieldRecord);
                }
                else
                {
                    // Existing
                    SetModifiedProperties(extraFieldRecord);
                }

                extraFieldRecord.DataTypeId = extraFieldRecord.DataTypeId == 0 ? record.DataTypeId : extraFieldRecord.DataTypeId;
                extraFieldRecord.StrData = record.StrData;
                extraFieldRecord.IntData = record.IntData;
                extraFieldRecord.BoolData = record.BoolData;
                extraFieldRecord.DecimalData = record.DecimalData;
                extraFieldRecord.DateData = record.DateData;
                extraFieldRecord.Comment = record.Comment;
            }

            return SaveChanges(entities);
        }

        #endregion

        #region SysExtraField

        public List<SysExtraField> GetSysExtraFields(SoeEntityType entityType)
        {

            int countryId = CompanyManager.GetCompanySysCountryId(ActorCompanyId);

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var query = from ef in sysEntitiesReadOnly.SysExtraField
                        where ef.Entity == (int)entityType
                           && ef.SysCountryId == countryId
                           && ef.State != (int)SoeEntityState.Deleted
                        select ef;

            List<SysExtraField> result = query.ToList();

            // add terms
            foreach (var sysExtraField in result)
            {
                sysExtraField.Name = GetText(sysExtraField.SysTermId, sysExtraField.SysTermGroupId);
            }
            return result;
        }

        #endregion

        #region Helpers

        private int GetDataTypeId(TermGroup_ExtraFieldType extraFieldType)
        {
            switch (extraFieldType)
            {
                case TermGroup_ExtraFieldType.FreeText:
                    return (int)SettingDataType.String;
                case TermGroup_ExtraFieldType.Integer:
                    return (int)SettingDataType.Integer;
                case TermGroup_ExtraFieldType.Decimal:
                    return (int)SettingDataType.Decimal;
                case TermGroup_ExtraFieldType.YesNo:
                    return (int)SettingDataType.Integer;
                case TermGroup_ExtraFieldType.Checkbox:
                    return (int)SettingDataType.Boolean;
                case TermGroup_ExtraFieldType.Date:
                    return (int)SettingDataType.Date;
                case TermGroup_ExtraFieldType.SingleChoice:
                    return (int)SettingDataType.Integer;
                case TermGroup_ExtraFieldType.MultiChoice:
                    return (int)SettingDataType.String;
                default:
                    return (int)SettingDataType.String;
            }
        }

        public string GetExtraFieldRecordValueAsString(ExtraFieldRecord record)
        {
            if (record == null)
                return string.Empty;
            switch (record.DataTypeId)
            {
                case (int)SettingDataType.String:
                    return record.StrData ?? string.Empty;
                case (int)SettingDataType.Integer:
                    return record.IntData.HasValue ? record.IntData.Value.ToString() : string.Empty;
                case (int)SettingDataType.Decimal:
                    return record.DecimalData.HasValue ? record.DecimalData.Value.ToString() : string.Empty;
                case (int)SettingDataType.Boolean:
                    if (record.BoolData.HasValue)
                    {
                        return record.BoolData.Value ? "True" : "False";
                    }
                    return string.Empty;
                case (int)SettingDataType.Date:
                    return record.DateData.HasValue ? record.DateData.Value.ToShortDateString() : string.Empty;
                default:
                    return string.Empty;
            }
        }

        #endregion
    }
}
