using SoftOne.Soe.Common.Util;
using System.Data;
using System.Data.SqlClient;

namespace SoftOne.Soe.Business.Util
{
    public class XEConnectDataBaseMethods
    {
        SqlConnection connection = null;
        SqlDataAdapter da = null;

        public XEConnectDataBaseMethods()
        {
        }

        private ActionResult OpenDatabase(string connectionString)
        {
            ActionResult result = new ActionResult();

            connection = new SqlConnection(connectionString);
            da = new SqlDataAdapter("", connection);

            try
            {
                connection.Open();
            }
            catch (SqlException ex)
            {
                result = new ActionResult(ex);
            }

            return result;
        }

        public ActionResult GetTabelDataSet(string connectionString, DataSet dataSet, string dataSetNamn, string selectStatement)
        {
            ActionResult result = new ActionResult();

            result = OpenDatabase(connectionString);
            if (!result.Success) return result;

            da.SelectCommand.CommandText = selectStatement;
            try
            {
                da.Fill(dataSet, dataSetNamn);
            }
            catch (SqlException ex)
            {
                result = new ActionResult(ex);
            }
            finally
            {
                connection.Close();
            }

            return result;
        }
    }
}
