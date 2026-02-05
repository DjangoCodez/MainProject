using System;
using System.Text;
using System.Data;
using System.Data.Entity.Core;
using System.IO;
using System.Data.SqlClient;

namespace SoftOne.EdiAdmin.Business.Util
{
    public class EdiDatabasMetoder
    {

        private SqlConnection con;
        private SqlDataAdapter da;
        private SqlCommandBuilder cb;
        private DataSet ds;

        public EdiDatabasMetoder()
        {
        }

        //Öppna Databas
        //Returvärde - Felmeddelande
        //Referenser - Connection-sträng, Databas
        private string ÖppnaDatabas(string refConnection, string refDatabas)
        {
            con = new SqlConnection(refConnection + "Database=" + refDatabas + ";Pooling=false");
            da = new SqlDataAdapter("", con);
            ds = new DataSet();

            StringBuilder errorMessages = new StringBuilder();
            try
            {
                con.Open();
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Felmeddelande: " + ex.Errors[i].Message + "\n");
                }
                errorMessages.Append(con.ConnectionString + "\n");
            }
            string ErrorMessage = errorMessages.ToString();
            return ErrorMessage;
        }

        //Hämta data till Dataset från Databas med Open och Close
        //Returvärde - Felmeddelande
        //Referenser - Connection-sträng, Databas, DataSet, Tabellnamn, Select-sats
        public string HämtaTabellDataSet(string refConnection, string refDatabas, DataSet refDataSet, string refDataSetNamn, string refSelect)
        {
            string ErrorMessage = ÖppnaDatabas(refConnection, refDatabas);
            if (ErrorMessage.ToString() != "") return ErrorMessage;

            StringBuilder errorMessages = new StringBuilder();

            da.SelectCommand.CommandText = refSelect;
            try
            {
                da.Fill(refDataSet, refDataSetNamn);
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    string fel = ex.Errors[i].Message.Replace("Invalid object name", "Tabell saknas");
                    errorMessages.Append("Fel: " + ex.Errors[i].LineNumber + "\n" + "Felmeddelande: " + fel + "\n");
                }
            }
            con.Close();

            ErrorMessage = errorMessages.ToString();
            return ErrorMessage;
        }

        //Uppdatera data från Dataset till Databas med Open och Close
        //Returvärde - Felmeddelande
        //Referenser - Connection-sträng, Databas, DataSet, Tabellnamn
        public string UppdateraTabellDataSet(string refConnection, string refDatabas, DataSet refDataSet, string refTabell)
        {
            string ErrorMessage = ÖppnaDatabas(refConnection, refDatabas);
            if (ErrorMessage.ToString() != "") return ErrorMessage;

            StringBuilder errorMessages = new StringBuilder();

            string fraga = "SELECT * FROM " + refTabell;
            da.SelectCommand.CommandText = "SELECT * FROM " + refTabell;
            cb = new SqlCommandBuilder(da);
            try
            {
                if (refTabell.ToString() != "")
                    da.Update(refDataSet, refTabell);
                else
                    da.Update(refDataSet);
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                    errorMessages.Append("Fel: " + ex.Errors[i].LineNumber + "\n" + "Felmeddelande: " + ex.Errors[i].Message + "\n");
            }

            con.Close();

            ErrorMessage = errorMessages.ToString();
            return ErrorMessage;

        }

        //Hämta aktuellt Identitets Counter för angiven tabell
        //Returvärde - Counter 
        //Referenser - Connection-sträng, Databas, DataSet, Tabellnamn
        public int HämtaCounter(string refConnection, string refDatabas, string refTabell)
        {
            SqlConnection connection = new SqlConnection(refConnection + "database=" + refDatabas + ";Pooling=false");
            SqlCommand conCommand = new SqlCommand();
            conCommand.CommandText = "SELECT IDENT_CURRENT('" + refTabell + "')";
            conCommand.Connection = connection;
            connection.Open();
            int Counter = Convert.ToInt32(conCommand.ExecuteScalar());
            if (Counter == 1)
            {
                ds = new DataSet();
                da = new SqlDataAdapter("SELECT * FROM " + refTabell, connection);
                da.SelectCommand.CommandText = "SELECT * FROM " + refTabell;
                da.Fill(ds, refTabell);
                if (ds.Tables[refTabell].Rows.Count == 0) Counter = 0;
            }
            connection.Close();
            return Counter;
        }

    }
}
