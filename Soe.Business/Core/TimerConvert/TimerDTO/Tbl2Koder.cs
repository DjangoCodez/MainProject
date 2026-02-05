using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimerConvert.TimerDTO
{
    public class Tbl2KoderDTO
    {
        public Tbl2KoderDTO() { }

        public int KodId { get; set; }
        public string Kod { get; set; }
        public string Beskr { get; set; }
        public string Typ { get; set; }
        public string Alias { get; set; }
        public string Flagga { get; set; }
        public int GOPayrollProductId { get; internal set; }
    }


    public class Tbl2KoderFactory : TimerFactoryBase
    {
        private readonly string timerConnectionString;
        private readonly CompEntities entities;
        private readonly int actorCompanyId;

        public Tbl2KoderFactory(string timerConnectionString, CompEntities entities, int actorCompanyId)
        {
            this.timerConnectionString = timerConnectionString;
            this.entities = entities;
            this.actorCompanyId = actorCompanyId;
        }

        public Dictionary<string, Tbl2KoderDTO> GetDictionary()
        {
            return GetAll().GroupBy(x => x.Kod).ToDictionary(k => k.Key, w => w.First());
        }

        public List<Tbl2KoderDTO> GetAll() 
        {
            //var converts = entities.WtConvertMapping.Where(w => w.ActorCompanyId == actorCompanyId && w.Type == (int)TimerGOConvertType.PayrollProduct);
            List<Tbl2KoderDTO> dtos = new List<Tbl2KoderDTO>();

            using (SqlConnection connection = new SqlConnection(timerConnectionString))
            {
                string sqlQuery = $@"SELECT * FROM tbl2Avtal";
                var queryResult = FrownedUponSQLClient.ExcuteQueryNew(connection, sqlQuery);
                var reader = queryResult.SqlDataReader;

                while (reader.Read())
                {
                    var dto = new Tbl2KoderDTO();

                    dto.KodId = (int)reader["kodId"];
                    dto.Kod = (string)reader["kod"];
                    dto.Beskr = (string)reader["beskr"];
                    dto.Typ = (string)reader["typ"];
                    dto.Alias = (string)reader["alias"];
                    dto.Flagga = (string)reader["flagga"];

                    #region Mapping

                    // connect GOs payroll products (loaded from setup conversion table)

                    // dto.GOPayrollProductId = matchingConvert.PayrollProductId;
                    
                    #endregion

                    dtos.Add(dto);
                }
            }
            return dtos;
        }
    }
}
