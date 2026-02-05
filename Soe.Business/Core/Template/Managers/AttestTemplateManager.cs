using SoftOne.Soe.Business.Core.Template.Models;
using SoftOne.Soe.Business.Core.Template.Models.Attest;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Template.Managers
{
    public class AttestTemplateManager : ManagerBase
    {
        readonly CompanyTemplateManager companyTemplateManager;
        public AttestTemplateManager(ParameterObject parameterObject) : base(parameterObject)
        {
            companyTemplateManager = new CompanyTemplateManager(parameterObject);
        }


        public TemplateCompanyAttestDataItem GetTemplateCompanyAttestDataItem(CopyFromTemplateCompanyInputDTO inputDTO)
        {
            TemplateCompanyAttestDataItem item = new TemplateCompanyAttestDataItem();
            item.CategoryCopyItems = GetCategoryCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestBilling) || inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestSupplier) || inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestTime) || inputDTO.DoCopy(TemplateCompanyCopy.SigningSettings))
            {
                item.AttestRoleCopyItems = GetAttestRoleCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.AttestStateCopyItems = GetAttestStateCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.AttestTransitionCopyItems = GetAttestTransitionCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.AttestWorkFlowTemplateHeadCopyItems = GetAttestWorkFlowTemplateHeadCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.CategoryCopyItems = GetCategoryCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
            }

            return item;
        }

        public List<TemplateResult> CopyTemplateCompanyAttestDataItem(CopyFromTemplateCompanyInputDTO inputDTO, TemplateCompanyDataItem templateCompanyDataItem)
        {
            List<TemplateResult> templateResults = new List<TemplateResult>();

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestTime))
                templateResults.Add(CopyAttestRolesFromTemplateCompany(templateCompanyDataItem, soeModule: SoeModule.Time));

            if (inputDTO.DoCopy(TemplateCompanyCopy.SigningSettings))
                templateResults.Add(CopyAttestRolesFromTemplateCompany(templateCompanyDataItem, soeModule: SoeModule.Manage));

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestBilling))
                templateResults.Add(CopyAttestRolesFromTemplateCompany(templateCompanyDataItem, soeModule: SoeModule.Billing));

            return templateResults;
        }

        public List<CategoryCopyItem> GetCategoryCopyItemsFromApi(int sysCompDbId, int templateCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<CategoryCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetCategoryCopyItems(templateCompanyId);

            return attestTemplateConnector.GetCategoryCopyItems(sysCompDbId, templateCompanyId);
        }

        public List<CategoryCopyItem> GetCategoryCopyItems(int templateCompanyId)
        {
            List<CategoryCopyItem> categoryCopyItems = new List<CategoryCopyItem>();

            try
            {
                List<Category> categories = CategoryManager.GetAllCategoriesWithRecords(templateCompanyId);

                foreach (var category in categories)
                {
                    CategoryCopyItem categoryCopyItem = new CategoryCopyItem()
                    {
                        // Assign properties based on category
                        CategoryId = category.CategoryId,
                        Name = category.Name,
                        Code = category.Code,
                        Type = (SoeCategoryType)category.Type
                    };

                    foreach (var companyCategoryRecord in category.CompanyCategoryRecord)
                    {
                        categoryCopyItem.CompanyCategoryRecordCopyItems.Add(new CompanyCategoryRecordCopyItem()
                        {
                            CategoryId = category.CategoryId,
                            Default = companyCategoryRecord.Default,
                            Entity = (SoeCategoryRecordEntity)companyCategoryRecord.Entity,
                            RecordId = companyCategoryRecord.RecordId,
                        });
                    }

                    categoryCopyItems.Add(categoryCopyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return categoryCopyItems;
        }


        public TemplateResult CopyAttestRolesFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem, SoeModule soeModule, bool copyAttest = true)
        {
            TemplateResult templateResult = new TemplateResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    #region Prereq

                    if (templateCompanyDataItem?.TemplateCompanyAttestDataItem?.AttestRoleCopyItems == null)
                        return templateResult;

                    Company newCompany = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    if (newCompany == null)
                    {
                        var result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                        templateResult.ActionResults.Add(result);
                        return templateResult;
                    }

                    List<AttestRole> existingAttestRoles = new List<AttestRole>();
                    List<AttestState> existingAttestStates = new List<AttestState>();
                    List<AttestTransition> existingAttestTransitions = new List<AttestTransition>();
                    List<AttestWorkFlowTemplateHead> existingAttestWorkFlowTemplates = new List<AttestWorkFlowTemplateHead>();

                    #endregion

                    #region Categories

                    if ((soeModule == SoeModule.Time || soeModule == SoeModule.Economy) && templateCompanyDataItem.TemplateCompanyAttestDataItem.CategoryCopyItems != null) // For now only employee categories are copied and only when copying attest for Time
                    {

                        List<Category> existingCategories = CategoryManager.GetCategories(entities, SoeCategoryType.Employee, templateCompanyDataItem.DestinationActorCompanyId, loadChildren: true);

                        foreach (var templateCategory in templateCompanyDataItem.TemplateCompanyAttestDataItem.CategoryCopyItems.Where(w => w.Type == SoeCategoryType.Employee))
                        {
                            #region Category

                            Category category = existingCategories?.FirstOrDefault(c => c.Name == templateCategory.Name);
                            if (category == null)
                            {
                                category = new Category();
                                category.Company = newCompany;
                                SetCreatedProperties(category);
                                existingCategories.Add(category);
                            }
                            else
                                SetModifiedProperties(category);

                            category.Name = templateCategory.Name;
                            category.Code = templateCategory.Code;
                            category.Type = (int)templateCategory.Type;
                            templateCompanyDataItem.TemplateCompanyAttestDataItem.AddCategoryMapping(templateCategory.CategoryId, category);
                            #endregion
                        }

                        var saveCategoriesResult = SaveChangesWithTransaction(entities);
                        templateResult.ActionResults.Add(saveCategoriesResult);


                        foreach (var templateCategory in templateCompanyDataItem.TemplateCompanyAttestDataItem.CategoryCopyItems.Where(w => w.Type == SoeCategoryType.Employee && w.ParentId.HasValue))
                        {

                            var parentMatch = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetCategory(templateCategory.ParentId ?? 0);
                            if (parentMatch != null)
                            {
                                var match = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetCategory(templateCategory.CategoryId);

                                if (match != null)
                                    match.ParentId = parentMatch.CategoryId;
                            }
                        }

                        var saveCategoriesParentResult = SaveChangesWithTransaction(entities);
                        templateResult.ActionResults.Add(saveCategoriesParentResult);
                    }

                    #endregion

                    if (copyAttest)
                    {
                        #region AttestRoles

                        existingAttestRoles = AttestManager.GetAttestRoles(entities, templateCompanyDataItem.DestinationActorCompanyId, soeModule, loadExternalCode: true);

                        foreach (var attestRoleCopyItem in templateCompanyDataItem.TemplateCompanyAttestDataItem.AttestRoleCopyItems.Where(w => w.Module == soeModule))
                        {
                            #region AttestRole

                            AttestRole attestRole = existingAttestRoles.FirstOrDefault(a => a.Name == attestRoleCopyItem.Name);
                            if (attestRole == null)
                            {
                                attestRole = new AttestRole();
                                attestRole.Company = newCompany;
                                SetCreatedProperties(attestRole);
                            }
                            else
                                SetModifiedProperties(attestRole);

                            attestRole.Name = attestRoleCopyItem.Name;
                            attestRole.Description = attestRoleCopyItem.Description;
                            attestRole.DefaultMaxAmount = attestRoleCopyItem.DefaultMaxAmount;
                            attestRole.Module = (int)attestRoleCopyItem.Module;
                            attestRole.ShowUncategorized = attestRoleCopyItem.ShowUncategorized;
                            attestRole.ShowAllCategories = attestRoleCopyItem.ShowAllCategories;
                            attestRole.ShowAllSecondaryCategories = attestRoleCopyItem.ShowAllSecondaryCategories;
                            attestRole.ShowTemplateSchedule = attestRoleCopyItem.ShowTemplateSchedule;
                            attestRole.ReminderNoOfDays = attestRoleCopyItem.ReminderNoOfDays;
                            attestRole.ReminderPeriodType = attestRoleCopyItem.ReminderPeriodType;
                            attestRole.AlsoAttestAdditionsFromTime = attestRoleCopyItem.AlsoAttestAdditionsFromTime;
                            attestRole.HumanResourcesPrivacy = attestRoleCopyItem.HumanResourcesPrivacy;
                            attestRole.Sort = attestRoleCopyItem.Sort;
                            templateCompanyDataItem.TemplateCompanyAttestDataItem.AddAttestRoleMapping(attestRoleCopyItem.AttestRoleId, attestRole);

                            #endregion
                        }

                        #endregion

                        #region AttestStates

                        existingAttestStates = AttestManager.GetAttestStates(entities, templateCompanyDataItem.DestinationActorCompanyId, TermGroup_AttestEntity.Unknown, soeModule);

                        foreach (var templateAttestState in templateCompanyDataItem.TemplateCompanyAttestDataItem.AttestStateCopyItems.Where(w => w.Module == soeModule))
                        {
                            #region AttestState

                            AttestState attestState = existingAttestStates?.FirstOrDefault(a => a.Name == templateAttestState.Name && a.Entity == (int)templateAttestState.Entity);
                            if (attestState == null)
                            {
                                attestState = new AttestState();
                                attestState.Company = newCompany;
                                SetCreatedProperties(attestState);
                            }
                            else
                                SetModifiedProperties(attestState);

                            attestState.Name = templateAttestState.Name;
                            attestState.Description = templateAttestState.Description;
                            attestState.Initial = templateAttestState.Initial;
                            attestState.Closed = templateAttestState.Closed;
                            attestState.Sort = templateAttestState.Sort;
                            attestState.Color = templateAttestState.Color;
                            attestState.Entity = (int)templateAttestState.Entity;
                            attestState.Module = (int)templateAttestState.Module;
                            attestState.Hidden = templateAttestState.Hidden;

                            templateCompanyDataItem.TemplateCompanyAttestDataItem.AddAttestStateMapping(templateAttestState.AttestStateId, attestState);

                            #endregion
                        }

                        SaveChanges(entities);

                        #region visableAttestStates

                        foreach (var attestRoleCopyItem in templateCompanyDataItem.TemplateCompanyAttestDataItem.AttestRoleCopyItems.Where(w => w.Module == soeModule))
                        {
                            if (attestRoleCopyItem.VisiableAttestStates.IsNullOrEmpty())
                                continue;

                            var newAttestRole = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetAttestRole(attestRoleCopyItem.AttestRoleId);

                            if (newAttestRole != null)
                            {
                                var fromDatabase = entities.AttestRole.First(f => f.AttestRoleId == newAttestRole.AttestRoleId);

                                foreach (var templateVisiableAttestState in attestRoleCopyItem.VisiableAttestStates)
                                {
                                    var matchingAttestState = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetAttestState(templateVisiableAttestState.AttestStateId);
                                    if (matchingAttestState != null)
                                    {
                                        if (!newAttestRole.VisibleAttestState.IsNullOrEmpty() && newAttestRole.VisibleAttestState.Any(i => i.AttestStateId == matchingAttestState.AttestStateId))
                                            continue;

                                        fromDatabase.VisibleAttestState.Add(new VisibleAttestState() { AttestStateId = matchingAttestState.AttestStateId });
                                    }
                                }
                            }

                        }

                        SaveChanges(entities);

                        #endregion
                        #endregion

                        #region AttestTransitions

                        existingAttestTransitions = AttestManager.GetAttestTransitions(entities, TermGroup_AttestEntity.Unknown, soeModule, true, templateCompanyDataItem.DestinationActorCompanyId);

                        foreach (var templateAttestTransition in templateCompanyDataItem.TemplateCompanyAttestDataItem.AttestTransitionCopyItems.Where(w => w.Module == soeModule))
                        {
                            #region AttestTransition

                            if (templateAttestTransition.AttestStateFromId == null || templateAttestTransition.AttestStateToId == null)
                                continue;

                            AttestTransition attestTransition = existingAttestTransitions.FirstOrDefault(a => a.Name == templateAttestTransition.Name && a.AttestStateFrom != null && a.AttestStateFrom.Entity == (int)templateAttestTransition.Entity);

                            if (attestTransition == null)
                            {
                                attestTransition = new AttestTransition();
                                SetCreatedProperties(attestTransition);
                            }
                            else
                                SetModifiedProperties(attestTransition);

                            var attestStateFromId = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetAttestState(templateAttestTransition.AttestStateFromId.Value)?.AttestStateId ?? 0;
                            var attestStateToId = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetAttestState(templateAttestTransition.AttestStateToId.Value)?.AttestStateId ?? 0;


                            attestTransition.Company = newCompany;
                            attestTransition.AttestStateFrom = entities.AttestState.FirstOrDefault(f => f.AttestStateId == attestStateFromId);
                            attestTransition.AttestStateTo = entities.AttestState.FirstOrDefault(f => f.AttestStateId == attestStateToId);
                            attestTransition.Name = templateAttestTransition.Name;
                            attestTransition.Module = (int)templateAttestTransition.Module;
                            attestTransition.NotifyChangeOfAttestState = templateAttestTransition.NotifyChangeOfAttestState;
                            templateCompanyDataItem.TemplateCompanyAttestDataItem.AddAttestTransitionMapping(templateAttestTransition.AttestTransitionId, attestTransition);
                            #region AttestRoleTransitionMapping

                            foreach (var templateAttestRoleId in templateAttestTransition.AttestRoleIds)
                            {
                                #region Update
                                var matchingAttestRole = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetAttestRole(templateAttestRoleId);
                                if (matchingAttestRole != null)
                                {
                                    if (!attestTransition.AttestRole.IsNullOrEmpty() && attestTransition.AttestRole.Any(i => i.AttestRoleId == matchingAttestRole.AttestRoleId))
                                        continue;

                                    attestTransition.AttestRole.Add(entities.AttestRole.First(f => f.AttestRoleId == matchingAttestRole.AttestRoleId));
                                }

                                #endregion
                            }

                            #endregion
                            #endregion
                        }

                        SaveChanges(entities);

                        #endregion

                        #region AttestWorkFlowTemplateHead

                        if (templateCompanyDataItem.TemplateCompanyAttestDataItem.AttestWorkFlowTemplateHeadCopyItems.Any())
                        {
                            existingAttestWorkFlowTemplates = AttestManager.GetAttestWorkFlowTemplateHeads(entities, templateCompanyDataItem.DestinationActorCompanyId, TermGroup_AttestEntity.Unknown);

                            foreach (var attestWorkFlowTemplateHeadCopyItem in templateCompanyDataItem.TemplateCompanyAttestDataItem.AttestWorkFlowTemplateHeadCopyItems)
                            {
                                AttestWorkFlowTemplateHead newAttestWorkFlowTemplateHead = existingAttestWorkFlowTemplates?.FirstOrDefault(a => a.Name == attestWorkFlowTemplateHeadCopyItem.Name && a.Type == (int)attestWorkFlowTemplateHeadCopyItem.Type && a.AttestEntity == (int)attestWorkFlowTemplateHeadCopyItem.AttestEntity && a.Description == attestWorkFlowTemplateHeadCopyItem.Description);
                                bool headExists = newAttestWorkFlowTemplateHead != null;
                                if (!headExists)
                                    newAttestWorkFlowTemplateHead = new AttestWorkFlowTemplateHead();

                                newAttestWorkFlowTemplateHead.Type = (int)attestWorkFlowTemplateHeadCopyItem.Type;
                                newAttestWorkFlowTemplateHead.AttestEntity = (int)attestWorkFlowTemplateHeadCopyItem.AttestEntity;
                                newAttestWorkFlowTemplateHead.Name = attestWorkFlowTemplateHeadCopyItem.Name;
                                newAttestWorkFlowTemplateHead.Description = attestWorkFlowTemplateHeadCopyItem.Description;
                                newAttestWorkFlowTemplateHead.Company = newCompany;

                                bool existingHasRows = false;
                                if (headExists)
                                {
                                    if (!newAttestWorkFlowTemplateHead.AttestWorkFlowTemplateRow.IsLoaded)
                                        newAttestWorkFlowTemplateHead.AttestWorkFlowTemplateRow.Load();

                                    if (!newAttestWorkFlowTemplateHead.AttestWorkFlowTemplateRow.IsNullOrEmpty())
                                        existingHasRows = true;
                                    SetModifiedProperties(newAttestWorkFlowTemplateHead);
                                }
                                else
                                {
                                    SetCreatedProperties(newAttestWorkFlowTemplateHead);
                                    entities.AttestWorkFlowTemplateHead.AddObject(newAttestWorkFlowTemplateHead);
                                }
                                SaveChanges(entities);

                                if (!headExists || !existingHasRows)
                                {
                                    foreach (var row in attestWorkFlowTemplateHeadCopyItem.AttestWorkFlowTemplateRowCopyItems)
                                    {
                                        AttestWorkFlowTemplateRow newAttestWorkFlowTemplateRow = new AttestWorkFlowTemplateRow()
                                        {
                                            AttestWorkFlowTemplateHeadId = newAttestWorkFlowTemplateHead.AttestWorkFlowTemplateHeadId,
                                            Type = row.Type,
                                            Sort = row.Sort,
                                        };

                                        var transition = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetAttestTransition(row.AttestTransitionId);

                                        if (transition == null)
                                        {
                                            var transitionFromTemplate = templateCompanyDataItem.TemplateCompanyAttestDataItem.AttestTransitionCopyItems.FirstOrDefault(t => t.AttestTransitionId == row.AttestTransitionId);

                                            if (transitionFromTemplate != null)
                                            {
                                                var existingTransition = entities.AttestTransition.FirstOrDefault(a => a.Name == transitionFromTemplate.Name && a.AttestStateFrom.Entity == (int)transitionFromTemplate.Entity && a.ActorCompanyId == newCompany.ActorCompanyId);

                                                if (existingTransition != null)
                                                    transition = existingTransition;
                                            }
                                        }
                                        if (transition != null)
                                        {
                                            newAttestWorkFlowTemplateRow.AttestTransitionId = transition.AttestTransitionId;
                                            entities.AttestWorkFlowTemplateRow.AddObject(newAttestWorkFlowTemplateRow);
                                        }
                                    }
                                }
                            }
                        }

                        #endregion
                    }

                    var saveAttestResult = SaveChangesWithTransaction(entities);
                    templateResult.ActionResults.Add(saveAttestResult);

                    if (saveAttestResult.Success)
                    {
                        #region ReminderAttestStateId
                        var attestRoleMappings = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetAttestRoleMappings();
                        foreach (var pair in attestRoleMappings)
                        {
                            var templateAttestRole = templateCompanyDataItem.TemplateCompanyAttestDataItem.AttestRoleCopyItems.FirstOrDefault(i => i.AttestRoleId == pair.Key);
                            AttestRole newAttestRole = pair.Value;
                            if (templateAttestRole == null || newAttestRole == null)
                                continue;
                            if (!templateAttestRole.ReminderAttestStateId.HasValue || attestRoleMappings.ContainsKey(templateAttestRole.ReminderAttestStateId.Value))
                                continue;

                            newAttestRole.ReminderAttestStateId = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetAttestState(templateAttestRole.ReminderAttestStateId.Value)?.AttestStateId;
                        }

                        #endregion

                        #region CompanyCategoryRecords

                        foreach (KeyValuePair<int, AttestRole> pair in attestRoleMappings)
                        {
                            #region AttestRole

                            var templateAttestRole = templateCompanyDataItem.TemplateCompanyAttestDataItem.AttestRoleCopyItems.FirstOrDefault(i => i.AttestRoleId == pair.Key);
                            AttestRole attestRole = pair.Value;
                            if (templateAttestRole == null || attestRole == null)
                                continue;

                            #region CompanyCategoryRecord

                            List<CompanyCategoryRecord> existingCompanyCategoryRecords = new List<CompanyCategoryRecord>();
                            if (templateCompanyDataItem.Update)
                            {
                                existingCompanyCategoryRecords.AddRange(CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRole, attestRole.AttestRoleId, templateCompanyDataItem.DestinationActorCompanyId));
                                existingCompanyCategoryRecords.AddRange(CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRoleSecondary, attestRole.AttestRoleId, templateCompanyDataItem.DestinationActorCompanyId));
                            }

                            var templateRecords = templateCompanyDataItem.TemplateCompanyAttestDataItem.CategoryCopyItems.Where(w => w.Type == SoeCategoryType.Employee)
                                .SelectMany(s => s.CompanyCategoryRecordCopyItems).Where(w => w.Entity == SoeCategoryRecordEntity.AttestRole || w.Entity == SoeCategoryRecordEntity.AttestRoleSecondary).ToList();

                            foreach (var categoryCopyItem in templateCompanyDataItem.TemplateCompanyAttestDataItem.CategoryCopyItems.Where(w => w.Type == SoeCategoryType.Employee))
                            {

                                foreach (var templateCategoryRecord in categoryCopyItem.CompanyCategoryRecordCopyItems.Where(w => w.Entity == SoeCategoryRecordEntity.AttestRole || w.Entity == SoeCategoryRecordEntity.AttestRoleSecondary))
                                {
                                    var cat = templateCompanyDataItem.TemplateCompanyAttestDataItem.GetCategory(templateCategoryRecord.CategoryId);
                                    CompanyCategoryRecord existingRecord = existingCompanyCategoryRecords.FirstOrDefault(c => c.Category.Name == cat.Name && c.Category.Type == cat.Type);
                                    if (existingRecord == null)
                                    {

                                        CompanyCategoryRecord record = new CompanyCategoryRecord()
                                        {
                                            RecordId = attestRole.AttestRoleId,
                                            Entity = (int)templateCategoryRecord.Entity,
                                            Default = templateCategoryRecord.Default,

                                            //Set references
                                            Company = newCompany,
                                            CategoryId = cat.CategoryId,
                                        };
                                        SetCreatedProperties(record);
                                    }
                                }
                            }

                            #endregion

                            #endregion
                        }

                        var saveAttestCategoryResult = SaveChangesWithTransaction(entities);

                        if (!saveAttestCategoryResult.Success)
                            saveAttestCategoryResult = companyTemplateManager.LogCopyError("attestRoleCopyItems", templateCompanyDataItem, saved: true);

                        templateResult.ActionResults.Add(saveAttestCategoryResult);

                        #endregion
                    }
                    else
                        templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("Attest", templateCompanyDataItem, saved: true));
                }
            }
            catch (Exception ex)
            {
                templateResult.ActionResults.Add(new ActionResult(ex));
                LogError(ex.ToString());
            }
            return templateResult;
        }



        public List<AttestRoleCopyItem> GetAttestRoleCopyItems(int actorCompanyId)
        {
            List<AttestRoleCopyItem> attestRoleCopyItems = new List<AttestRoleCopyItem>();

            try
            {
                List<AttestRole> attestRoles = AttestManager.GetAttestRoles(actorCompanyId);

                foreach (var attestRole in attestRoles)
                {
                    if (!attestRole.AttestTransition.IsLoaded)
                        attestRole.AttestTransition.Load();

                    if (!attestRole.VisibleAttestState.IsLoaded)
                        attestRole.VisibleAttestState.Load();

                    AttestRoleCopyItem attestRoleCopyItem = new AttestRoleCopyItem()
                    {
                        AttestRoleId = attestRole.AttestRoleId,
                        ActorCompanyId = actorCompanyId,
                        Name = attestRole.Name,
                        Description = attestRole.Description,
                        DefaultMaxAmount = attestRole.DefaultMaxAmount,
                        Module = (SoeModule)attestRole.Module,
                        ShowUncategorized = attestRole.ShowUncategorized,
                        ShowAllCategories = attestRole.ShowAllCategories,
                        ShowAllSecondaryCategories = attestRole.ShowAllSecondaryCategories,
                        ShowTemplateSchedule = attestRole.ShowTemplateSchedule,
                        ReminderNoOfDays = attestRole.ReminderNoOfDays,
                        ReminderPeriodType = attestRole.ReminderPeriodType,
                        AlsoAttestAdditionsFromTime = attestRole.AlsoAttestAdditionsFromTime,
                        HumanResourcesPrivacy = attestRole.HumanResourcesPrivacy,
                        Sort = attestRole.Sort,
                        AttestTransitions = attestRole.AttestTransition.Select(at => new AttestTransitionCopyItem()
                        {
                            AttestTransitionId = at.AttestTransitionId,
                        }).ToList(),
                        VisiableAttestStates = attestRole.VisibleAttestState.Select(vas => new AttestStateCopyItem()
                        {
                            AttestStateId = vas.AttestStateId,
                        }).ToList(),
                    };

                    attestRoleCopyItems.Add(attestRoleCopyItem);
                }

            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return attestRoleCopyItems;
        }

        public List<AttestRoleCopyItem> GetAttestRoleCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<AttestRoleCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetAttestRoleCopyItems(actorCompanyId);

            return attestTemplateConnector.GetAttestRoleCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<AttestStateCopyItem> GetAttestStateCopyItemsFromApi(int sysCompDbId, int templateCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<AttestStateCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetAttestStateCopyItems(templateCompanyId);

            return attestTemplateConnector.GetAttestStateCopyItems(sysCompDbId, templateCompanyId);
        }

        public List<AttestStateCopyItem> GetAttestStateCopyItems(int templateCompanyId)
        {
            List<AttestStateCopyItem> attestStateCopyItems = new List<AttestStateCopyItem>();

            try
            {
                List<AttestState> attestStates = AttestManager.GetAttestStates(templateCompanyId);

                foreach (var attestState in attestStates)
                {
                    AttestStateCopyItem attestStateCopyItem = new AttestStateCopyItem()
                    {
                        AttestStateId = attestState.AttestStateId,
                        Name = attestState.Name,
                        Description = attestState.Description,
                        Sort = attestState.Sort,
                        Closed = attestState.Closed,
                        Hidden = attestState.Hidden,
                        Color = attestState.Color,
                        Entity = (TermGroup_AttestEntity)attestState.Entity,
                        Initial = attestState.Initial,
                        Module = (SoeModule)attestState.Module,
                    };

                    attestStateCopyItems.Add(attestStateCopyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return attestStateCopyItems;
        }

        public List<AttestTransitionCopyItem> GetAttestTransitionCopyItemsFromApi(int sysCompDbId, int templateCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<AttestTransitionCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetAttestTransitionCopyItems(templateCompanyId);

            return attestTemplateConnector.GetAttestTransitionCopyItems(sysCompDbId, templateCompanyId);
        }

        public List<AttestTransitionCopyItem> GetAttestTransitionCopyItems(int templateCompanyId)
        {
            List<AttestTransitionCopyItem> attestTransitionCopyItems = new List<AttestTransitionCopyItem>();

            try
            {
                List<AttestTransition> attestTransitions = AttestManager.GetAllAttestTransitions(templateCompanyId);

                foreach (var attestTransition in attestTransitions)
                {
                    AttestTransitionCopyItem attestTransitionCopyItem = new AttestTransitionCopyItem()
                    {
                        AttestStateFromId = attestTransition.AttestStateFromId,
                        AttestStateToId = attestTransition.AttestStateToId,
                        AttestTransitionId = attestTransition.AttestTransitionId,
                        Entity = (TermGroup_AttestEntity)attestTransition.AttestStateFrom.Entity,
                        Module = (SoeModule)attestTransition.Module,
                        Name = attestTransition.Name,
                        NotifyChangeOfAttestState = attestTransition.NotifyChangeOfAttestState,
                    };

                    foreach (var attestRole in attestTransition.AttestRole)
                    {
                        attestTransitionCopyItem.AttestRoleIds.Add(attestRole.AttestRoleId);
                    }

                    attestTransitionCopyItems.Add(attestTransitionCopyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return attestTransitionCopyItems;
        }

        public List<AttestWorkFlowTemplateHeadCopyItem> GetAttestWorkFlowTemplateHeadCopyItemsFromApi(int sysCompDbId, int templateCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<AttestWorkFlowTemplateHeadCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetAttestWorkFlowTemplateHeadCopyItems(templateCompanyId);

            return attestTemplateConnector.GetAttestWorkFlowTemplateHeadCopyItems(sysCompDbId, templateCompanyId);
        }

        public List<AttestWorkFlowTemplateHeadCopyItem> GetAttestWorkFlowTemplateHeadCopyItems(int templateCompanyId)
        {
            List<AttestWorkFlowTemplateHeadCopyItem> attestWorkFlowTemplateHeadCopyItems = new List<AttestWorkFlowTemplateHeadCopyItem>();

            try
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                List<AttestWorkFlowTemplateHead> attestWorkFlowTemplateHeads = AttestManager.GetAttestWorkFlowTemplateHeadsIncludingRows(entitiesReadOnly, templateCompanyId, TermGroup_AttestEntity.Unknown);

                foreach (var head in attestWorkFlowTemplateHeads)
                {
                    AttestWorkFlowTemplateHeadCopyItem attestWorkFlowTemplateHeadCopyItem = new AttestWorkFlowTemplateHeadCopyItem()
                    {
                        AttestWorkFlowTemplateHeadId = head.AttestWorkFlowTemplateHeadId,
                        Description = head.Description,
                        Name = head.Name,
                        AttestEntity = (TermGroup_AttestEntity)head.AttestEntity,
                        ActorCompanyId = head.ActorCompanyId,
                        Type = (TermGroup_AttestWorkFlowType)head.Type,
                    };

                    foreach (var row in head.AttestWorkFlowTemplateRow)
                    {
                        attestWorkFlowTemplateHeadCopyItem.AttestWorkFlowTemplateRowCopyItems.Add(new AttestWorkFlowTemplateRowCopyItem()
                        {
                            AttestTransitionId = row.AttestTransitionId,
                            Sort = row.Sort,
                            Type = row.Type,
                        });
                    }

                    attestWorkFlowTemplateHeadCopyItems.Add(attestWorkFlowTemplateHeadCopyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return attestWorkFlowTemplateHeadCopyItems;
        }

    }
}
