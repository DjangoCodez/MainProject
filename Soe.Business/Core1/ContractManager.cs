using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class ContractManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ContractManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion


        #region Contract period


        public static DateTime GetNextContactPeriodDate(ContractGroup contractGroup, DateTime contractPeriodDate, out int nextContractPeriodYear, out int nextContractPeriodValue)
        {
            TermGroup_ContractGroupPeriod period = (TermGroup_ContractGroupPeriod)contractGroup.Period;
            Tuple<int, int> nextPeriod = CalendarUtility.CalculateNextPeriod(period, contractGroup.Interval, contractPeriodDate.Year, contractPeriodDate.Month);
            nextContractPeriodYear = nextPeriod.Item1;
            nextContractPeriodValue = nextPeriod.Item2;
            return CalendarUtility.ConvertContractPeriodToDate(period, contractPeriodDate, nextContractPeriodYear, nextContractPeriodValue, contractGroup.DayInMonth);
        }

        #endregion

        #region ContractGroup

        public IEnumerable<ContractGroup> GetContractGroups(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ContractGroup.NoTracking();
            return (from c in entities.ContractGroup
                    where c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    orderby c.Name
                    select c);
        }

        public IEnumerable<ContractGroupExtendedGridDTO> GetContractGroupsExtended(int actorCompanyId, int? contractGroupId = null)
        {
            List<ContractGroupExtendedGridDTO> contractGroups = new List<ContractGroupExtendedGridDTO>();
            var periods = base.GetTermGroupContent(TermGroup.ContractGroupPeriod);
            var managementTypes = base.GetTermGroupContent(TermGroup.ContractGroupPriceManagement);

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ContractGroup.NoTracking();
            var items = (from c in entities.ContractGroup
                    where c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    orderby c.Name
                    select c);

            foreach(var item in items)
            {
                ContractGroupExtendedGridDTO group = new ContractGroupExtendedGridDTO()
                {
                    ContractGroupId = item.ContractGroupId,
                    Name = item.Name,
                    Description = item.Description,
                    Interval = item.Interval,
                    DayInMonth = item.DayInMonth,
                };

                var period = periods.FirstOrDefault(p => p.Id == item.Period);
                if (period != null)
                {
                    group.PeriodId = period.Id;
                    group.PeriodText = period.Name;
                }

                var managementType = managementTypes.FirstOrDefault(p => p.Id == item.PriceManagement);
                if(managementType != null)
                    group.PriceManagementText = managementType.Name;

                contractGroups.Add(group);
            }

            if(contractGroupId.HasValue)
            {
                contractGroups = contractGroups.Where(cg => cg.ContractGroupId == contractGroupId.Value).ToList();
            }

            return contractGroups;
        }

        public IEnumerable<ContractGroup> GetContractGroups(CompEntities entities, int actorCompanyId)
        {
            return (from c in entities.ContractGroup
                    where c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    orderby c.Name
                    select c);
        }

        public ContractGroup GetContractGroup(CompEntities entities, int contractGroupId)
        {
            return (from c in entities.ContractGroup
                    where c.ContractGroupId == contractGroupId
                    select c).FirstOrDefault();
        }

        public ContractGroup GetContractGroup(int contractGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ContractGroup.NoTracking();
            return GetContractGroup(entities, contractGroupId);
        }

        /// <summary>
        /// Insert or update a contract group
        /// </summary>
        /// <param name="contractGroupInput">ContractGroup entity</param>
        /// <param name="actorCompanyId">Company Id</param>
        /// <returns>ActionResult</returns>
        /// 
        public ActionResult SaveContractGroup(ContractGroupDTO contractGroupInput, int actorCompanyId)
        {
            if (contractGroupInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ContractGroup");

            // Default result is successful
            ActionResult result = new ActionResult();

            int contractGroupId = contractGroupInput.ContractGroupId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get Company
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        #endregion

                        #region ContractGroup

                        // Get existing group
                        ContractGroup contractGroup = GetContractGroup(entities, contractGroupId);
                        if (contractGroup == null)
                        {
                            #region ContractGroup Add

                            contractGroup = new ContractGroup()
                            {
                                Company = company
                            };
                            SetCreatedProperties(contractGroup);
                            entities.ContractGroup.AddObject(contractGroup);
                            #endregion
                        }
                        else
                        {
                            #region ContractGroup Update

                            SetModifiedProperties(contractGroup);

                            #endregion
                        }

                        contractGroup.Name = contractGroupInput.Name;
                        contractGroup.Description = contractGroupInput.Description;
                        contractGroup.Period = (int)contractGroupInput.Period;
                        contractGroup.DayInMonth = contractGroupInput.DayInMonth;
                        contractGroup.Interval = contractGroupInput.Interval;
                        contractGroup.PriceManagement = (int)contractGroupInput.PriceManagement;
                        contractGroup.InvoiceText = contractGroupInput.InvoiceText;
                        contractGroup.InvoiceTextRow = contractGroupInput.InvoiceTextRow;
                        contractGroup.OrderTemplate = contractGroupInput.OrderTemplate;
                        contractGroup.InvoiceTemplate = contractGroupInput.InvoiceTemplate;
                        contractGroup.State = (int)contractGroupInput.State;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            contractGroupId = contractGroup.ContractGroupId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = contractGroupId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }


        public ActionResult SaveContractGroup(CompEntities entities, ContractGroupDTO contractGroupInput, int actorCompanyId)
        {
            if (contractGroupInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ContractGroup");

            // Default result is successful
            ActionResult result = new ActionResult();

            int contractGroupId = contractGroupInput.ContractGroupId;

            using (entities)
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get Company
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        #endregion

                        #region ContractGroup

                        // Get existing group
                        ContractGroup contractGroup = GetContractGroup(entities, contractGroupId);
                        if (contractGroup == null)
                        {
                            #region ContractGroup Add

                            contractGroup = new ContractGroup()
                            {
                                Company = company
                            };
                            SetCreatedProperties(contractGroup);
                            entities.ContractGroup.AddObject(contractGroup);
                            #endregion
                        }
                        else
                        {
                            #region ContractGroup Update

                            SetModifiedProperties(contractGroup);

                            #endregion
                        }

                        contractGroup.Name = contractGroupInput.Name;
                        contractGroup.Description = contractGroupInput.Description;
                        contractGroup.Period = (int)contractGroupInput.Period;
                        contractGroup.DayInMonth = contractGroupInput.DayInMonth;
                        contractGroup.Interval = contractGroupInput.Interval;
                        contractGroup.PriceManagement = (int)contractGroupInput.PriceManagement;
                        contractGroup.InvoiceText = contractGroupInput.InvoiceText;
                        contractGroup.InvoiceTextRow = contractGroupInput.InvoiceTextRow;
                        contractGroup.OrderTemplate = contractGroupInput.OrderTemplate;
                        contractGroup.InvoiceTemplate = contractGroupInput.InvoiceTemplate;
                        contractGroup.State = (int)contractGroupInput.State;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            contractGroupId = contractGroup.ContractGroupId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = contractGroupId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Sets a contract groups state to Deleted
        /// </summary>
        /// <param name="contractGroupId">Contract group to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteContractGroup(int contractGroupId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ContractGroup contractGroup = GetContractGroup(entities, contractGroupId);
                if (contractGroup == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ContractGroup");

                return ChangeEntityState(entities, contractGroup, SoeEntityState.Deleted, true);
            }
        }

        #endregion
    }
}
