using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SoftOne.Soe.Business.Util
{
    public static class SqlCommandUtil
    {
        public static ActionResult ExecuteSqlUpsertCommand(ObjectContext entities, string sql, bool failIfZeroAffected = true)
        {
            return ExecuteSqlUpsertCommand(entities, sql, new List<SqlParameter>());
        }

        public static ActionResult ExecuteSqlUpsertCommand(ObjectContext entities, string sql, List<SqlParameter> parameters, bool failIfZeroAffected = true, bool failifNoTransaction = true)
        {
            ActionResult result = new ActionResult();

            try
            {
                if (failifNoTransaction && entities.TransactionHandler == null)
                    return new ActionResult("No Transaction present");

                result.ObjectsAffected = entities.ExecuteStoreCommand(TransactionalBehavior.DoNotEnsureTransaction, sql, parameters.ToArray());

                if (result.ObjectsAffected < 0 || (failIfZeroAffected && result.ObjectsAffected == 0))
                {
                    string error = "ExecuteSqlUpsertCommand returned zero or less rows ";
                    LogCollector.LogCollector.LogError(error + sql);
                    result = new ActionResult(error);
                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogCollector.LogCollector.LogError(ex.ToString() + sql);
            }

            return result;
        }

        public static ActionResult ExecuteStoredProcedure(ObjectContext entities, string storedProcedureName, List<SqlParameter> parameters, bool failIfNoTransaction = true)
        {
            ActionResult result = new ActionResult();

            try
            {
                if (failIfNoTransaction && entities.TransactionHandler == null)
                {
                    return new ActionResult("No Transaction present");
                }

                // Build the SQL command string
                StringBuilder sb = new StringBuilder();
                sb.Append("EXEC ");
                sb.Append(storedProcedureName);
                sb.Append(" ");

                // Add the parameters to the command string
                for (int i = 0; i < parameters.Count; i++)
                {
                    sb.Append(!parameters[i].ParameterName.StartsWith("@") ? "@" : "" + parameters[i].ParameterName);
                    if (i < parameters.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }

                // Execute the stored procedure and get the number of affected rows
                result.ObjectsAffected = entities.ExecuteStoreCommand(TransactionalBehavior.DoNotEnsureTransaction, sb.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogCollector.LogCollector.LogError("ExecuteStoredProcedure " + ex.ToString() + storedProcedureName);
            }

            return result;
        }


        public static void test()
        {
            TimeCodeTransaction tt = new TimeCodeTransaction();
            var sqlParameter = GetSaslaösdk<TimeCodeTransaction>(tt.State.GetProperties().First(), 2);
        }

        public static SqlParameter GetSaslaösdk<T>(PropertyInfo prop, object value)
        {
            var type = typeof(T);
            var p = type.GetProperties().FirstOrDefault(f => f.Name == prop.Name);

            if (p == null)
                throw new Exception("wtf");

            return new SqlParameter("@" + p.Name, value);
        }

        public static List<SqlParameter> GetUpdatedSqlParameters(ObjectContext entities, object entity)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            var myObjectState = entities.ObjectStateManager.GetObjectStateEntry(entity);
            var modifiedProperties = myObjectState.GetModifiedProperties();
            foreach (var propName in modifiedProperties)
            {
                sqlParameters.Add(new SqlParameter("@" + propName, myObjectState.CurrentValues[propName]));
            }

            return sqlParameters;
        }

        public static string CreateAttestTransitionLogInsertStatement(int actorCompanyId, int attestTransitionId, List<int> transactionIds, TermGroup_AttestEntity attestEntity, int userId, DateTime logDate)
        {
            var sqlsb = new StringBuilder();
            string timeStamp = CalendarUtility.ToSqlFriendlyDateTime(logDate);
            string into = "INSERT INTO [dbo].[AttestTransitionLog] ([ActorCompanyId], [AttestTransitionId], [Entity], [UserId], [Date], [RecordId])";
            foreach (var transactionId in transactionIds)
            {
                sqlsb.Append($"{into} Values({actorCompanyId}, {attestTransitionId}, {(int)attestEntity}, {userId}, '{timeStamp}', {transactionId})");
                sqlsb.Append(Environment.NewLine);
            }
            return sqlsb.ToString();
        }
    }
}
