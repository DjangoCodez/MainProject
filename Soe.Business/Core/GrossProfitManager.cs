using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Core
{
    public class GrossProfitManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public GrossProfitManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region GrossProfitCodes

        public List<GrossProfitCode> GetGrossProfitCodes(int actorCompanyId, int? accountYearId = null, int? grossProfitCodeId = null, bool includeDeleted = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.GrossProfitCode.NoTracking();
            return GetGrossProfitCodes(entities, actorCompanyId, accountYearId, grossProfitCodeId, includeDeleted);
        }

        public List<GrossProfitCode> GetGrossProfitCodes(CompEntities entities, int actorCompanyId, int? accountYearId = null, int? grossProfitCodeId = null, bool includeDeleted = false)
        {
            var codes = (from dc in entities.GrossProfitCode
                    .Include("AccountYear")
                         where dc.ActorCompanyId == actorCompanyId
                         select dc);

            if (includeDeleted)
                codes = codes.Where(x => x.State == (int)SoeEntityState.Deleted || x.State == (int)SoeEntityState.Active);
            else
                codes = codes.Where(x => x.State == (int)SoeEntityState.Active);


            if (grossProfitCodeId.HasValue) {
                codes = codes.Where(x=>x.GrossProfitCodeId == grossProfitCodeId);
            }

            return accountYearId.HasValue ? codes.Where(c => c.AccountYearId == accountYearId.Value).ToList() : codes.ToList();
        }

        public GrossProfitCode GetGrossProfitCode(int actorCompanyId, int grossProfitCodeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.GrossProfitCode.NoTracking();
            return GetGrossProfitCode(entities, actorCompanyId, grossProfitCodeId);
        }

        public GrossProfitCode GetGrossProfitCode(CompEntities entities, int actorCompanyId, int grossProfitCodeId)
        {
            return (from dc in entities.GrossProfitCode
                     .Include("AccountYear")
                    where dc.GrossProfitCodeId == grossProfitCodeId
                    select dc).FirstOrDefault();
        }
        
        public bool GrossProfitCodeExists(CompEntities entities, int actorCompanyId, int? accountId,int?accountDimId, int accountYearId, int code)
        {
            int counter = (from gpc in entities.GrossProfitCode
                           where gpc.ActorCompanyId == actorCompanyId && (accountId.HasValue ? gpc.AccountId == accountId : gpc.AccountId == null) && ( accountDimId.HasValue ? gpc.AccountDimId == accountDimId : gpc.AccountDimId == null ) && gpc.AccountYearId == accountYearId && gpc.Code == code &&
                           gpc.State != (int)SoeEntityState.Deleted
                           select gpc).Count();

            if (counter > 0)
                return true;
            return false;

        }

        public int GetExistingGrossProfitId(CompEntities entities, int actorCompanyId, int? accountId, int accountYearId, int code)
        {
            return (from gpc in entities.GrossProfitCode
                           where gpc.ActorCompanyId == actorCompanyId && 
                           (accountId == null || gpc.AccountId == accountId) && 
                           gpc.AccountYearId == accountYearId && 
                           gpc.Code == code &&
                           gpc.State != (int)SoeEntityState.Deleted
                           select gpc.GrossProfitCodeId).FirstOrDefault();
        }                

        public ActionResult SaveGrossProfitCode(int actorCompanyId, GrossProfitCodeDTO dto)
        {
            ActionResult result = new ActionResult(true);
            GrossProfitCode code = new GrossProfitCode();

            using (CompEntities entities = new CompEntities())
            {
                try
                {                    
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        if (dto == null)
                        {
                            result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));
                            return result;
                        }

                        if (dto.GrossProfitCodeId != 0)
                        {
                            code = GetGrossProfitCode(entities, actorCompanyId, dto.GrossProfitCodeId);

                            // Update
                            int existingGrossProfitId = GetExistingGrossProfitId(entities, actorCompanyId, dto.AccountId, dto.AccountYearId, dto.Code);

                            if (existingGrossProfitId > 0 && existingGrossProfitId != dto.GrossProfitCodeId)
                            {
                                if (GrossProfitCodeExists(entities, actorCompanyId, dto.AccountId,dto.AccountDimId, dto.AccountYearId, dto.Code))
                                {
                                    result = new ActionResult(false, (int)ActionResultSave.GrossProfitCodeExists, GetText(2156, "Bruttovinstkoden finns redan sparad"));
                                    return result;
                                }
                            }
                        }
                        else
                        {
                            //New 
                            if (GrossProfitCodeExists(entities, actorCompanyId, dto.AccountId,dto.AccountDimId, dto.AccountYearId, dto.Code))
                            {
                                result = new ActionResult(false, (int)ActionResultSave.GrossProfitCodeExists, GetText(2156, "Bruttovinstkoden finns redan sparad"));
                                return result;
                            }
                        }

                        #endregion

                        #region Perform
                        
                        code.AccountYearId = dto.AccountYearId;
                        code.Code = dto.Code;
                        code.Name = dto.Name;
                        code.Description = dto.Description;
                        code.AccountDimId = dto.AccountDimId;
                        code.AccountId = dto.AccountId;
                        code.OpeningBalance = dto.OpeningBalance;
                        code.Period1 = dto.Period1;
                        code.Period2 = dto.Period2;
                        code.Period3 = dto.Period3;
                        code.Period4 = dto.Period4;
                        code.Period5 = dto.Period5;
                        code.Period6 = dto.Period6;
                        code.Period7 = dto.Period7;
                        code.Period8 = dto.Period8;
                        code.Period9 = dto.Period9;
                        code.Period10 = dto.Period10;
                        code.Period11 = dto.Period11;
                        code.Period12 = dto.Period12;
                        code.Period13 = dto.Period13;
                        code.Period14 = dto.Period14;
                        code.Period15 = dto.Period15;
                        code.Period16 = dto.Period16;
                        code.Period17 = dto.Period17;
                        code.Period18 = dto.Period18;
                        SetModifiedProperties(code);
                        if (dto.GrossProfitCodeId == 0)
                        {
                            code.GrossProfitCodeId = 0;
                            code.ActorCompanyId = actorCompanyId;
                            entities.GrossProfitCode.AddObject(code);
                        }

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);
                    else if (code != null)
                        result.IntegerValue = code.GrossProfitCodeId;

                    entities.Connection.Close();
                }
            }
            return result;
        }

        public ActionResult DeleteGrossProfitCode(int actorCompanyId, int grossProfitCodeId)
        {
            using (CompEntities entities = new CompEntities())
            {
                var grossProfitCode = GetGrossProfitCode(entities, actorCompanyId, grossProfitCodeId);
                if (grossProfitCode == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "GrossProfitCode");

                return ChangeEntityState(entities, grossProfitCode, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult CopyGrossProfitCodes(int actorCompanyId, int previousAccountYearId, int accountYearId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.GrossProfitCode.NoTracking();
            return CopyGrossProfitCodes(entities, actorCompanyId, previousAccountYearId, accountYearId);
        }

        public ActionResult CopyGrossProfitCodes(CompEntities entities, int actorCompanyId, int previousAccountYearId, int accountYearId)
        {
            ActionResult result = new ActionResult(false);

            List<GrossProfitCode> grossProfitCodes = GetGrossProfitCodes(actorCompanyId).Where(p => p.AccountYearId == previousAccountYearId).ToList();
            if (grossProfitCodes.Count == 0)
                result.Success = true;

            if (grossProfitCodes != null)
            {
                foreach (var grossProfitCode in grossProfitCodes)
                {
                    if (!GrossProfitCodeExists(entities, actorCompanyId,null,null,accountYearId,grossProfitCode.Code))
                    {
                        GrossProfitCodeDTO newGrossProfitcode = new GrossProfitCodeDTO()
                        {
                            GrossProfitCodeId = 0,
                            ActorCompanyId = actorCompanyId,
                            AccountYearId = accountYearId,
                            AccountDimId = grossProfitCode.AccountDimId,
                            AccountId = grossProfitCode.AccountId,
                            Code = grossProfitCode.Code,
                            Name = grossProfitCode.Name,
                            Description = grossProfitCode.Description,
                            OpeningBalance = grossProfitCode.OpeningBalance,
                            Period1 = grossProfitCode.Period1,
                            Period2 = grossProfitCode.Period2,
                            Period3 = grossProfitCode.Period3,
                            Period4 = grossProfitCode.Period4,
                            Period5 = grossProfitCode.Period5,
                            Period6 = grossProfitCode.Period6,
                            Period7 = grossProfitCode.Period7,
                            Period8 = grossProfitCode.Period8,
                            Period9 = grossProfitCode.Period9,
                            Period10 = grossProfitCode.Period10,
                            Period11 = grossProfitCode.Period11,
                            Period12 = grossProfitCode.Period12,
                        };

                       result = SaveGrossProfitCode(actorCompanyId, newGrossProfitcode);
                    }
                }
            }

            return result;
        }

        public ActionResult CopyGrossProfitCodesCheckExisting(int actorCompanyId, int accountYearId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.GrossProfitCode.NoTracking();
            return CopyGrossProfitCodesCheckExisting(entities, actorCompanyId, accountYearId);
        }

        public ActionResult CopyGrossProfitCodesCheckExisting(CompEntities entities, int actorCompanyId, int accountYearId)
        {
            ActionResult result = new ActionResult(true);

            AccountYear currentAccountYear = AccountManager.GetAccountYear(entities, accountYearId, false);
            if (currentAccountYear == null)
                return new ActionResult((int)ActionResultDelete.EntityNotFound, "AccountYear");

            AccountYear previousAccountYear = AccountManager.GetPreviousAccountYear(entities, currentAccountYear.From, currentAccountYear.ActorCompanyId, false);
            if (previousAccountYear == null)
                result.Success = true;

            List<GrossProfitCode> grossProfitCodes = GetGrossProfitCodes(actorCompanyId).Where(p => p.AccountYearId == previousAccountYear.AccountYearId).ToList();
            if (grossProfitCodes.Count == 0)
                result.Success = true;

            foreach (var grossProfitCode in grossProfitCodes)
            {
                if (!GrossProfitCodeExists(entities, actorCompanyId, grossProfitCode.AccountId,grossProfitCode.AccountDimId, accountYearId, grossProfitCode.Code))
                {
                    GrossProfitCodeDTO newGrossProfitcode = new GrossProfitCodeDTO()
                    {
                        GrossProfitCodeId = 0,
                        ActorCompanyId = actorCompanyId,
                        AccountYearId = accountYearId,
                        AccountDimId = grossProfitCode.AccountDimId,
                        AccountId = grossProfitCode.AccountId,
                        Code = grossProfitCode.Code,
                        Name = grossProfitCode.Name,
                        Description = grossProfitCode.Description,
                        OpeningBalance = grossProfitCode.OpeningBalance,
                        Period1 = 0,
                        Period2 = 0,
                        Period3 = 0,
                        Period4 = 0,
                        Period5 = 0,
                        Period6 = 0,
                        Period7 = 0,
                        Period8 = 0,
                        Period9 = 0,
                        Period10 = 0, 
                        Period11 = 0,
                        Period12 = 0,
                    };

                    result = SaveGrossProfitCode(actorCompanyId, newGrossProfitcode);
                }
            }

            return result;
        }


        #endregion
    }
}
