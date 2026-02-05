using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Script.Serialization;

namespace SoftOne.Soe.Business.Core
{
    public class TermManager : ManagerBase
    {
        #region Ctor

        public TermManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region SysTerm

        public Dictionary<string, string> GetAngularSysTermPart(string cultureCode, string part)
        {
            Dictionary<string, string> sysTermParts = new Dictionary<string, string>();

            if (Enum.TryParse<TermGroup>("Angular" + part, true, out TermGroup group))
                sysTermParts = GetSysTermsFromDatabaseDict(GetLangId(cultureCode), (int)group);
            return sysTermParts;
        }

        public Dictionary<string, string> GetSysTermsFromDatabaseDict(int langId, int sysTermGroupId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<SysTerm> sysTerms = (from st in sysEntitiesReadOnly.SysTerm
                                      where st.SysTermGroupId == sysTermGroupId &&
                                      st.LangId == langId &&
                                      st.TranslationKey != null
                                      select st).ToList();

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (SysTerm sysTerm in sysTerms)
            {
                if (!dict.ContainsKey(sysTerm.TranslationKey))
                {
                    if (sysTerm.LangId != 1)
                        dict.Add(sysTerm.TranslationKey, StringUtility.ReplaceValue(sysTerm.Name, "\\n", Environment.NewLine));
                    else
                        dict.Add(sysTerm.TranslationKey, sysTerm.Name);
                }
            }

            return dict;
        }

        public List<SysTerm> GetSysTermsFromDatabase(int langId, int? sysTermGroupId, DateTime? date = null, WildCard dateWildcard = WildCard.Equals)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var query = from st in sysEntitiesReadOnly.SysTerm
                        where langId == (int)TermGroup_Languages.Unknown || st.LangId == langId
                        select st;

            if (date.HasValue)
                query = query.Where(dateWildcard, date.Value);

            if (!sysTermGroupId.HasValue || sysTermGroupId.Value == 0)
            {
                //Get for all sysTermGroupId
                query = from st in query
                        orderby st.SysTermGroupId, st.SysTermId
                        select st;
            }
            else
            {
                //Get for specific sysTermGroupId
                query = from st in query
                        where st.SysTermGroupId == sysTermGroupId
                        orderby st.SysTermId
                        select st;
            }

            return query.ToList();
        }

        public List<SysTerm> GetSysTermsWithGroupFromDatabase(int langId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from st in sysEntitiesReadOnly.SysTerm
                                    .Include("SysTermGroup")
                    where (st.LangId == langId || langId == (int)TermGroup_Languages.Unknown)
                    select st).ToList();
        }

        public List<SysTermDTO> GetSysTermDTOsFromDatabase(int? langId = null)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            if (!langId.HasValue || langId.Value == 0)

                return sysEntitiesReadOnly.SysTerm.ToList().ToDTOs();
            else
                return sysEntitiesReadOnly.SysTerm.Where(st => st.LangId == langId).ToList().ToDTOs();
        }

        public SysTerm GetSysTermFromDatabaseSafe(int sysTermId, int sysTermGroupId, int langId)
        {
            //Suppress any active transactions
            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_SUPPRESS, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
            {
                return GetSysTerm(sysTermId, sysTermGroupId, langId);
            }
        }

        public SysTerm GetSysTermByKeyFromDatabaseSafe(string translationKey, int langId)
        {
            //Suppress any active transactions
            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_SUPPRESS, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
            {
                return GetSysTermByKey(translationKey, langId);
            }
        }

        public string GetSysTermForFeature(Feature feature)
        {
            var sysFeature = SysDbCache.Instance.SysFeatures.FirstOrDefault(sf => sf.SysFeatureId == (int)feature);
            if (sysFeature != null)
                return GetText(sysFeature.SysTermId, (int)TermGroup.General);

            return String.Empty;
        }

        public string GetDaysTerm<T>(List<T> l)
        {
            return $"{l.Count} {(l.Count == 1 ? GetText(91907, "dag") : GetText(91908, "dagar"))}";
        }

        public void LoadSysTermsFromDatabase(Hashtable sysTermGroupCache, int sysTermGroupId)
        {
            //Suppress any active transactions
            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_SUPPRESS, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                var sysTerms = (from st in sysEntitiesReadOnly.SysTerm
                                where st.SysTermGroupId == sysTermGroupId
                                select new
                                {
                                    st.SysTermId,
                                    st.SysTermGroupId,
                                    st.Name,
                                    st.LangId,
                                }).ToList();

                foreach (var sysTerm in sysTerms)
                {
                    TermObject termObject = (TermObject)sysTermGroupCache[sysTerm.SysTermId.ToString()];
                    if (termObject != null)
                    {
                        //Update term
                        termObject.SetTerm(sysTerm.Name, sysTerm.LangId);
                    }
                    else
                    {
                        //Create term
                        termObject = new TermObject(sysTerm.SysTermId, sysTerm.SysTermGroupId);
                        termObject.SetTerm(sysTerm.Name, sysTerm.LangId);

                        //Add term to SysTermGroup Cache
                        sysTermGroupCache.Add(sysTerm.SysTermId.ToString(), termObject);
                    }
                }
            }
        }

        public void LoadSysTermsFromDatabase(Hashtable sysTermCache, string termGroupDictLoadedKey, string dictLoadedValue)
        {
            //Suppress any active transactions
            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_SUPPRESS, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                var sysTerms = (from st in sysEntitiesReadOnly.SysTerm
                                select new
                                {
                                    st.SysTermId,
                                    st.SysTermGroupId,
                                    st.Name,
                                    st.LangId,
                                }).ToList();

                foreach (var group in sysTerms.GroupBy(q => q.SysTermGroupId))
                {
                    Hashtable sysTermGroupCache = new Hashtable();
                    string sysTermGroupIdStr = group.Key.ToString();

                    foreach (var item in group)
                    {
                        TermObject termObject = (TermObject)sysTermGroupCache[item.SysTermId.ToString()];
                        if (termObject != null)
                        {
                            //Update term
                            termObject.SetTerm(item.Name, item.LangId);
                        }
                        else
                        {
                            //Create term
                            termObject = new TermObject(item.SysTermId, item.SysTermGroupId);
                            termObject.SetTerm(item.Name, item.LangId);

                            //Add term to SysTermGroup Cache
                            sysTermGroupCache.Add(item.SysTermId.ToString(), termObject);
                        }
                    }

                    // Mark that all SysTerms has been loaded into cache
                    sysTermGroupCache.Add(termGroupDictLoadedKey, dictLoadedValue);

                    if (sysTermCache.ContainsKey(sysTermGroupIdStr))
                        sysTermCache[sysTermGroupIdStr] = sysTermGroupCache;
                    else
                        sysTermCache.Add(sysTermGroupIdStr, sysTermGroupCache);
                }
            }
        }

        public ActionResult InsertSysTerm(int sysTermId, int sysTermGroupId, int langId, string name)
        {
            using (SOESysEntities entities = new SOESysEntities())
            {
                //Suppress any active transactions
                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_SUPPRESS, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    SysTerm newSysTerm = new SysTerm
                    {
                        SysTermId = sysTermId,
                        Name = name,
                        LangId = langId,
                        SysTermGroup = entities.SysTermGroup.FirstOrDefault<SysTermGroup>(i => i.SysTermGroupId == sysTermGroupId)
                    };
                    entities.SysTerm.Add(newSysTerm);
                    return SaveChanges(entities);
                }
            }
        }

        public ActionResult SaveSysTerms(List<SysTermDTO> terms)
        {
            ActionResult result = new ActionResult();

            using (SOESysEntities entities = new SOESysEntities())
            {
                foreach (SysTermDTO term in terms)
                {
                    SysTerm sysTerm = GetSysTerm(entities, term.SysTermId, term.SysTermGroupId, term.LangId);
                    if (sysTerm == null)
                    {
                        //Create
                        sysTerm = new SysTerm()
                        {
                            SysTermId = term.SysTermId,
                            Name = term.Name,
                            TranslationKey = term.TranslationKey,

                            //Set FK
                            SysTermGroupId = term.SysTermGroupId,
                            LangId = term.LangId,
                        };
                        SetCreatedPropertiesOnEntity(sysTerm);
                        entities.SysTerm.Add(sysTerm);
                    }
                    else
                    {
                        //Update
                        sysTerm.Name = term.Name;
                        sysTerm.TranslationKey = term.TranslationKey;
                        SetModifiedPropertiesOnEntity(sysTerm);
                    }
                }

                result = SaveChanges(entities);
            }

            return result;
        }

        #region Help-methods

        private SysTerm GetSysTerm(int sysTermId, int sysTermGroupId, int langId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysTerm(sysEntitiesReadOnly, sysTermId, sysTermGroupId, langId);
        }

        private SysTerm GetSysTerm(SOESysEntities entities, int sysTermId, int sysTermGroupId, int langId)
        {
            return (from st in entities.SysTerm
                    where st.SysTermId == sysTermId &&
                    st.SysTermGroup.SysTermGroupId == sysTermGroupId &&
                    st.LangId == langId
                    select st).FirstOrDefault();
        }

        private SysTerm GetSysTermByKey(string translationKey, int langId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysTermByKey(sysEntitiesReadOnly, translationKey, langId);
        }

        private SysTerm GetSysTermByKey(SOESysEntities entities, string translationKey, int langId)
        {
            return (from st in entities.SysTerm
                    where st.TranslationKey == translationKey &&
                    st.LangId == langId
                    select st).FirstOrDefault();
        }

        #endregion

        #endregion

        #region SysTermGroup

        public List<SysTermGroup> GetSysTermGroupsFromDatabase()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysTermGroup.OrderBy(stg => stg.SysTermGroupId).ToList();
        }

        public List<SysTermGroupDTO> GetSysTermGroupDTOs()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from stg in sysEntitiesReadOnly.SysTermGroup
                    orderby stg.SysTermGroupId
                    select stg).ToList().ToDTOs();
        }

        #region Help-methods

        private SysTermGroup GetSysTermGroup(SOESysEntities entities, int sysTermGroupId)
        {
            return (from stg in entities.SysTermGroup
                    where stg.SysTermGroupId == sysTermGroupId
                    orderby stg.SysTermGroupId
                    select stg).FirstOrDefault();
        }

        #endregion

        #endregion

        #region CompTerm

        #region Queries

        private IQueryable<CompTerm> GetCompTermsQuery(CompEntities entities, CompTermsRecordType recordType, int recordId)
        {
            return from entry in entities.CompTerm
                   where entry.RecordType == (int)recordType &&
                   entry.RecordId == recordId &&
                   entry.State == (int)SoeEntityState.Active
                   select entry;
        }

        private IQueryable<CompTerm> GetCompTermsByLangQuery(CompTermsRecordType recordType, int langId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompTerm.NoTracking();
            return GetCompTermsByLangQuery(entities, recordType, langId, actorCompanyId);
        }

        private IQueryable<CompTerm> GetCompTermsByLangQuery(CompEntities entities, CompTermsRecordType recordType, int langId, int actorCompanyId)
        {
            return from entry in entities.CompTerm
                   where entry.RecordType == (int)recordType &&
                   entry.LangId == langId &&
                   entry.ActorCompanyId == actorCompanyId &&
                   entry.State == (int)SoeEntityState.Active
                   select entry;
        }

        #endregion

        public List<CompTermDTO> GetCompTermDTOs(CompTermsRecordType recordType, int recordId, bool loadLangName = true)
        {
            var dtos = new List<CompTermDTO>();
            var compTerms = GetCompTerms(recordType, recordId);
            var countries = loadLangName ? CountryCurrencyManager.GetSysCountries(true) : new List<SysCountry>();

            foreach (var compTerm in compTerms)
            {
                var langName = loadLangName ? countries.Where(c => c.SysCountryId == compTerm.LangId).Select(c => c.Name).FirstOrDefault() : string.Empty;
                dtos.Add(compTerm.ToDTO(langName));
            }

            return dtos;
        }

        public List<CompTermDTO> GetCompTermDTOsByLanguage(CompTermsRecordType recordType, int langId, bool loadLangName = true)
        {
            var dtos = new List<CompTermDTO>();
            var compTerms = this.GetCompTermsByLangQuery(recordType, langId, base.ActorCompanyId).ToList();
            string langName = loadLangName ? CountryCurrencyManager.GetSysCountries(true).Where(c => c.SysCountryId == langId).Select(c => c.Name).FirstOrDefault() : null;

            foreach (var compTerm in compTerms)
            {
                dtos.Add(compTerm.ToDTO(langName));
            }
            return dtos;
        }

        public List<CompTerm> GetCompTerms(CompTermsRecordType recordType, int recordId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompTerms(entities, recordType, recordId);
        }

        public List<CompTerm> GetCompTerms(CompEntities entities, CompTermsRecordType recordType, int recordId)
        {
            return GetCompTermsQuery(entities, recordType, recordId).ToList();
        }

        public List<CompTerm> GetCompTermsByLang(CompTermsRecordType recordType, int langId)
        {
            using (var entities = new CompEntities())
            {
                entities.CompTerm.NoTracking();
                return GetCompTermsByLangQuery(entities, recordType, langId, base.ActorCompanyId).ToList();
            }
        }

        public CompTerm GetCompTerm(CompTermsRecordType recordType, int recordId, int langId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompTerm.NoTracking();
            return GetCompTerm(entities, recordType, recordId, langId);
        }

        public CompTerm GetCompTerm(CompEntities entities, CompTermsRecordType recordType, int recordId, int langId)
        {
            return this.GetCompTermsQuery(entities, recordType, recordId).FirstOrDefault(t => t.LangId == langId);
        }

        public CompTerm GetCompTerm(CompEntities entities, int compTermId)
        {
            return entities.CompTerm.FirstOrDefault(t => t.CompTermId == compTermId && t.State == (int)SoeEntityState.Active);
        }

        public ActionResult DeleteCompTerms(CompTermsRecordType recordType, int recordId)
        {
            using (var entities = new CompEntities())
            {
                List<CompTerm> compTerms = GetCompTerms(entities, recordType, recordId);
                foreach (var item in compTerms)
                {
                    entities.DeleteObject(item);
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult DeleteCompTerm(int compTermId)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                var compTerm = GetCompTerm(entities, compTermId);
                if (compTerm != null)
                {
                    result = ChangeEntityState(compTerm, SoeEntityState.Deleted);
                    if (result.Success)
                        result = SaveChanges(entities);
                }
            }

            return result;
        }

        public ActionResult SaveCompTerms(IEnumerable<CompTermDTO> terms, int actorCompanyId)
        {
            var result = new ActionResult();
            using (var entities = new CompEntities())
            {
                #region Delete

                foreach (var item in terms.Where(s => s.State != (int)SoeEntityState.Active))
                {
                    var compTerm = entities.CompTerm.FirstOrDefault(t => t.CompTermId == item.CompTermId);
                    if (compTerm != null)
                        entities.DeleteObject(compTerm);
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                #endregion

                #region Add/Update

                foreach (var item in terms.Where(t => t.State == (int)SoeEntityState.Active))
                {
                    // Overwrite existing terms if found in db for this recordid
                    var compTerm = entities.CompTerm.FirstOrDefault(t =>
                        t.RecordType == (int)item.RecordType &&
                        t.RecordId == item.RecordId &&
                        t.LangId == (int)item.Lang
                        );

                    if (compTerm == null)
                    {
                        //Create
                        compTerm = new CompTerm()
                        {
                            LangId = (int)item.Lang,
                            Name = item.Name,
                            RecordId = item.RecordId,
                            RecordType = (int)item.RecordType,
                            State = (int)item.State,
                            ActorCompanyId = actorCompanyId
                        };
                        entities.CompTerm.AddObject(compTerm);
                    }
                    else
                    {
                        //Update
                        compTerm.LangId = (int)item.Lang;
                        compTerm.Name = item.Name;
                        compTerm.RecordId = item.RecordId;
                        compTerm.RecordType = (int)item.RecordType;
                        compTerm.State = (int)item.State;
                    }
                }

                #endregion

                result = SaveChanges(entities);
            }

            return result;
        }

        public ActionResult SaveCompTerm(string name, CompTermsRecordType recordType, int recordId, int langId, int actorCompanyId)
        {
            if (langId == (int)TermGroup_Languages.Unknown || string.IsNullOrEmpty(name))
                return new ActionResult(false);

            using (var entities = new CompEntities())
            {
                CompTerm compTerm = GetCompTerm(entities, recordType, recordId, langId);
                if (compTerm == null)
                {
                    compTerm = new CompTerm();
                    entities.CompTerm.AddObject(compTerm);
                }
                compTerm.Name = name;
                compTerm.RecordType = (int)recordType;
                compTerm.RecordId = recordId;
                compTerm.LangId = langId;
                compTerm.ActorCompanyId = actorCompanyId;

                return SaveChanges(entities);
            }
        }

        #endregion

        #region Connect

        public List<SysTermDTO> CompareSystermDTOs(List<SysTermDTO> originalSysTerms, List<SysTermDTO> updatedSysTerms)
        {
            List<SysTermDTO> changedSysTermDTOs = new List<SysTermDTO>();

            foreach (SysTermDTO originalSysTerm in originalSysTerms)
            {
                if (updatedSysTerms.Any(updatedSysTerm => updatedSysTerm.IsEqualTo(originalSysTerm)))
                    continue;

                var updateItems = updatedSysTerms
                    .Where(newSysTermDTO =>
                        originalSysTerm.SysTermId == newSysTermDTO.SysTermId &&
                        originalSysTerm.SysTermGroupId == newSysTermDTO.SysTermGroupId &&
                        originalSysTerm.LangId == newSysTermDTO.LangId
                        )
                    .ToList();

                if (updateItems.Any())
                {
                    SysTermDTO sysTerm = originalSysTerm.CloneDTO();
                    sysTerm.PostChange = PostChange.Update;

                    changedSysTermDTOs.Add(sysTerm);
                }
                else
                {
                    SysTermDTO sysTerm = originalSysTerm.CloneDTO();
                    sysTerm.PostChange = PostChange.Insert;

                    changedSysTermDTOs.Add(sysTerm);
                }
            }

            foreach (SysTermDTO updatedSysTerm in updatedSysTerms)
            {
                var missingInUpdatedTerms = originalSysTerms
                    .Where(originalSysTerm =>
                        originalSysTerm.SysTermId == updatedSysTerm.SysTermId &&
                        originalSysTerm.SysTermGroupId == updatedSysTerm.SysTermGroupId &&
                        originalSysTerm.LangId == updatedSysTerm.LangId
                        )
                    .ToList();

                foreach (var missingItem in missingInUpdatedTerms)
                {
                    SysTermDTO deleteItem = missingItem.CloneDTO();
                    deleteItem.PostChange = PostChange.Delete;

                    changedSysTermDTOs.Add(deleteItem);
                }
            }

            return changedSysTermDTOs;
        }

        public List<SysTermDTO> CompareSysTermDTOs_NEW(List<SysTermDTO> originalSysTerms, List<SysTermDTO> updatedSysTerms)
        {
            var changedSysTermDTOs = new List<SysTermDTO>();

            var updatedLookup = updatedSysTerms.ToDictionary(
                x => (x.SysTermId, x.SysTermGroupId, x.LangId), x => x);

            var originalLookup = originalSysTerms.ToDictionary(
                x => (x.SysTermId, x.SysTermGroupId, x.LangId), x => x);

            foreach (var original in originalSysTerms)
            {
                if (updatedLookup.TryGetValue((original.SysTermId, original.SysTermGroupId, original.LangId), out var updated))
                {
                    if (!original.IsEqualTo(updated))
                    {
                        var sysTerm = updated.CloneDTO();
                        sysTerm.PostChange = PostChange.Update;
                        changedSysTermDTOs.Add(sysTerm);
                    }
                }
                else
                {
                    var sysTerm = original.CloneDTO();
                    sysTerm.PostChange = PostChange.Delete;
                    changedSysTermDTOs.Add(sysTerm);
                }
            }

            foreach (var updated in updatedSysTerms)
            {
                if (!originalLookup.ContainsKey((updated.SysTermId, updated.SysTermGroupId, updated.LangId)))
                {
                    var sysTerm = updated.CloneDTO();
                    sysTerm.PostChange = PostChange.Insert;
                    changedSysTermDTOs.Add(sysTerm);
                }
            }

            return changedSysTermDTOs;
        }

        public ActionResult UpdateSysTerms(List<SysTermDTO> sysTermDTOs)
        {
            ActionResult result = new ActionResult();

            var insertItems = sysTermDTOs.Where(s => s.PostChange == PostChange.Insert).ToList();
            var updateItems = sysTermDTOs.Where(s => s.PostChange == PostChange.Update).ToList();
            var deleteItems = sysTermDTOs.Where(s => s.PostChange == PostChange.Delete).ToList();

            try
            {
                using (SOESysEntities entities = new SOESysEntities())
                {
                    foreach (var newItem in insertItems)
                    {
                        SysTerm sysTerm = new SysTerm()
                        {
                            SysTermId = newItem.SysTermId,
                            SysTermGroupId = newItem.SysTermGroupId,
                            LangId = newItem.LangId,
                            Name = newItem.Name,
                            Created = newItem.Created,
                            CreatedBy = newItem.CreatedBy,
                            Modified = newItem.Modified,
                            ModifiedBy = newItem.ModifiedBy
                        };
                        entities.SysTerm.Add(sysTerm);
                    }

                    foreach (var updateItem in updateItems)
                    {
                        SysTerm sysTerm = GetSysTerm(entities, updateItem.SysTermId, updateItem.SysTermGroupId, updateItem.LangId);
                        if (sysTerm != null)
                        {
                            sysTerm.Name = updateItem.Name;
                            sysTerm.Created = updateItem.Created;
                            sysTerm.CreatedBy = updateItem.CreatedBy;
                            sysTerm.Modified = updateItem.Modified;
                            sysTerm.ModifiedBy = updateItem.ModifiedBy;
                        }
                    }

                    foreach (var deleteItem in deleteItems)
                    {
                        SysTerm sysTerm = GetSysTerm(entities, deleteItem.SysTermId, deleteItem.SysTermGroupId, deleteItem.LangId);
                        if (sysTerm != null)
                        {
                            entities.SysTerm.Remove(sysTerm);
                        }
                    }

                    return base.SaveChanges(entities);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.ToString();
            }

            return result;
        }

        public List<SysTermGroupDTO> CompareSysTermGroupDTOs(List<SysTermGroupDTO> originalSysTermGroups, List<SysTermGroupDTO> updatedSysTermGroups)
        {
            List<SysTermGroupDTO> changedSysTermGroupDTOs = new List<SysTermGroupDTO>();

            foreach (SysTermGroupDTO originalSysTermGroup in originalSysTermGroups)
            {
                if (updatedSysTermGroups.Any(updatedSysTermGroup => updatedSysTermGroup.IsEqualTo(originalSysTermGroup)))
                    continue;

                // Not matching items below

                var updateItems = updatedSysTermGroups.Where(updatedSysTermGroup => originalSysTermGroup.SysTermGroupId == updatedSysTermGroup.SysTermGroupId).ToList();
                if (updateItems.Any())
                {
                    SysTermGroupDTO updatedItem = originalSysTermGroup.CloneDTO();
                    updatedItem.PostChange = PostChange.Update;

                    changedSysTermGroupDTOs.Add(updatedItem);
                }
                else
                {
                    SysTermGroupDTO newItem = originalSysTermGroup.CloneDTO();
                    newItem.PostChange = PostChange.Insert;

                    changedSysTermGroupDTOs.Add(newItem);
                }
            }

            foreach (SysTermGroupDTO updatedSysTermGroup in updatedSysTermGroups)
            {
                var missingInUpdatedTerms = originalSysTermGroups.Where(oldSysTermGroupDTO => oldSysTermGroupDTO.SysTermGroupId == updatedSysTermGroup.SysTermGroupId).ToList();

                foreach (var missingItem in missingInUpdatedTerms)
                {
                    SysTermGroupDTO deleteItem = missingItem.CloneDTO();
                    deleteItem.PostChange = PostChange.Delete;

                    changedSysTermGroupDTOs.Add(deleteItem);
                }
            }

            return changedSysTermGroupDTOs;
        }

        public List<SysTermGroupDTO> CompareSysTermGroupDTOs_NEW(List<SysTermGroupDTO> originalSysTermGroups, List<SysTermGroupDTO> updatedSysTermGroups)
        {
            var changedSysTermGroupDTOs = new List<SysTermGroupDTO>();

            var updatedLookup = updatedSysTermGroups.ToDictionary(
                x => x.SysTermGroupId, x => x);

            var originalLookup = originalSysTermGroups.ToDictionary(
                x => x.SysTermGroupId, x => x);

            // Update och Delete
            foreach (var original in originalSysTermGroups)
            {
                if (updatedLookup.TryGetValue(original.SysTermGroupId, out var updated))
                {
                    if (!original.IsEqualTo(updated))
                    {
                        var sysTermGroup = updated.CloneDTO();
                        sysTermGroup.PostChange = PostChange.Update;
                        changedSysTermGroupDTOs.Add(sysTermGroup);
                    }
                }
                else
                {
                    var sysTermGroup = original.CloneDTO();
                    sysTermGroup.PostChange = PostChange.Delete;
                    changedSysTermGroupDTOs.Add(sysTermGroup);
                }
            }

            // Insert
            foreach (var updated in updatedSysTermGroups)
            {
                if (!originalLookup.ContainsKey(updated.SysTermGroupId))
                {
                    var sysTermGroup = updated.CloneDTO();
                    sysTermGroup.PostChange = PostChange.Insert;
                    changedSysTermGroupDTOs.Add(sysTermGroup);
                }
            }

            return changedSysTermGroupDTOs;
        }

        public ActionResult UpdateSysTermGroups(List<SysTermGroupDTO> sysTermGroupDTOs)
        {
            ActionResult result = new ActionResult();

            var insertItems = sysTermGroupDTOs.Where(s => s.PostChange == PostChange.Insert).ToList();
            var updateItems = sysTermGroupDTOs.Where(s => s.PostChange == PostChange.Update).ToList();
            var deleteItems = sysTermGroupDTOs.Where(s => s.PostChange == PostChange.Delete).ToList();

            try
            {
                using (SOESysEntities entities = new SOESysEntities())
                {
                    foreach (var newItem in insertItems)
                    {
                        SysTermGroup sysTermGroup = new SysTermGroup()
                        {
                            SysTermGroupId = newItem.SysTermGroupId,
                            Name = newItem.Name,
                            Description = newItem.Description
                        };
                        entities.SysTermGroup.Add(sysTermGroup);
                    }

                    foreach (var updateItem in updateItems)
                    {
                        SysTermGroup sysTermGroup = GetSysTermGroup(entities, updateItem.SysTermGroupId);
                        if (sysTermGroup != null)
                        {
                            sysTermGroup.SysTermGroupId = updateItem.SysTermGroupId;
                            sysTermGroup.Name = updateItem.Name;
                        }
                    }

                    foreach (var deleteItem in deleteItems)
                    {
                        SysTermGroup sysTermGroup = GetSysTermGroup(entities, deleteItem.SysTermGroupId);
                        if (sysTermGroup != null)
                        {
                            entities.SysTermGroup.Remove(sysTermGroup);
                        }
                    }

                    return base.SaveChanges(entities);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.ToString();
            }

            return result;
        }

        #endregion

        #region Json for Angular

        public string GetSysTermJson(int langId)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer
            {
                MaxJsonLength = int.MaxValue
            };

            List<SysTermJsonDTO> dtos = new List<SysTermJsonDTO>();

            List<SysTerm> terms = GetSysTermsWithGroupFromDatabase(langId);

            foreach (var term in terms)
            {
                SysTermJsonDTO dto = new SysTermJsonDTO
                {
                    Id = term.SysTermId,
                    LId = term.LangId,
                    GId = term.SysTermGroupId,
                    GName = term.SysTermGroup.Name,
                    Name = term.Name
                };
                dtos.Add(dto);
            }

            return serializer.Serialize(dtos);
        }

        #endregion
    }
}
