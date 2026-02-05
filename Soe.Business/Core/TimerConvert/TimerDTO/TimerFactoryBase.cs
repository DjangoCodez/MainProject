using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimerConvert.TimerDTO
{
    public class TimerFactoryBase
    {

        #region Help-methods

        #region Data


        protected SqlCommand GetCommand(SqlConnection connection, string commandText)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = commandText;
            return cmd;
        }

        /// <summary>
        /// Adds the companyId as a parameter
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="wtCompanyId"></param>
        /// <returns></returns>
        protected SqlCommand GetCommand(SqlConnection conn, string storedProcedureName, int wtCompanyId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            SqlCommand cmd = GetCommand(conn, storedProcedureName);
            SqlParameter parameterCompanyId = new SqlParameter("id", System.Data.SqlDbType.Int);
            parameterCompanyId.Value = wtCompanyId;
            cmd.Parameters.Add(parameterCompanyId);

            if (fromDate != null)
            {
                SqlParameter parameterFromDate = new SqlParameter("fromDate", System.Data.SqlDbType.DateTime);
                parameterFromDate.Value = fromDate;
                cmd.Parameters.Add(parameterFromDate);
            }

            if (toDate != null)
            {
                SqlParameter parameterToDate = new SqlParameter("toDate", System.Data.SqlDbType.DateTime);
                parameterToDate.Value = toDate;
                cmd.Parameters.Add(parameterToDate);
            }

            return cmd;
        }

        protected SqlCommand GetCommandAgreements(SqlConnection conn, int timerButikId, int timerPersonalId, DateTime fromDate, DateTime toDate)
        {
            SqlCommand cmd = GetCommand(conn, "spGetSumArkivListaFull");
            
            SqlParameter parameterButikId = new SqlParameter("bid", System.Data.SqlDbType.Int);
            parameterButikId.Value = timerButikId;
            cmd.Parameters.Add(parameterButikId);

            SqlParameter parameterPersonalId = new SqlParameter("pidIN", System.Data.SqlDbType.Int);
            parameterPersonalId.Value = timerPersonalId;
            cmd.Parameters.Add(parameterPersonalId);

            SqlParameter parameterFromDate = new SqlParameter("dfrom", System.Data.SqlDbType.VarChar);
            parameterFromDate.Value = fromDate.GetSwedishFormattedDate().Replace("-", "");
            cmd.Parameters.Add(parameterFromDate);

            SqlParameter parameterToDate = new SqlParameter("dtom", System.Data.SqlDbType.VarChar);
            parameterToDate.Value = toDate.GetSwedishFormattedDate().Replace("-","");
            cmd.Parameters.Add(parameterToDate);

            return cmd;
        }

        protected ActionResult Save(CompEntities entities)
        {
            entities.SaveChanges();

            return new ActionResult(true);
        }

        

        protected void PopulateWtConvertMapping(CompEntities entities, Company company)
        {
            List<WtConvertMapping> mappings = (from m in entities.WtConvertMapping
                                               where m.ActorCompanyId == company.ActorCompanyId
                                               select m).ToList();

            #region Company

            //Make sure AccountDim is loaded
            if (!company.AccountDim.IsLoaded)
                company.AccountDim.Load();

            foreach (AccountDim accountDim in company.AccountDim)
            {
                //Make sure Account is loaded
                if (!accountDim.Account.IsLoaded)
                    accountDim.Account.Load();

                foreach (Account account in accountDim.Account)
                {
                    //Make sure AccountInternal is loaded
                    if (!account.AccountInternalReference.IsLoaded)
                        account.AccountInternalReference.Load();
                }
            }

            #endregion

            
        }

        #endregion


        #endregion
    }
}
