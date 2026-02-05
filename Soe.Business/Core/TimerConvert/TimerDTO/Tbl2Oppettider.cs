using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimerConvert.TimerDTO
{
    public class Tbl2OppettiderDTO
    {
        public Tbl2OppettiderDTO() { }

        public int OppettiderId { get; set; }
        public int ButikId { get; set; }
        public int DagTyp { get; set; }
        public int Dag { get; set; }
        public string Fran { get; set; }
        public string Till { get; set; }
        public string Datum { get; set; }
        public int Stangt { get; set; }
        public string DatumFrom { get; set; }
        public int GruppId { get; set; }
    }


    public class Tbl2OppettiderFactory : TimerFactoryBase
    {
        private readonly string timerConnectionString;
        private readonly CompEntities entities;
        private readonly int actorCompanyId;

        public Tbl2OppettiderFactory(string timerConnectionString, CompEntities entities, int actorCompanyId)
        {
            this.timerConnectionString = timerConnectionString;
            this.entities = entities;
            this.actorCompanyId = actorCompanyId;
        }

        public Dictionary<int, Tbl2OppettiderDTO> GetDictionaryForButik(int butikID)
        {
            return GetAll().GroupBy(x => x.ButikId).ToDictionary(k => k.Key, w => w.First());
        }

        public List<Tbl2OppettiderDTO> GetAll() 
        {
            //var converts = entities.WtConvertMapping.Where(w => w.ActorCompanyId == actorCompanyId && w.Type == (int)TimerGOConvertType.);
            List<Tbl2OppettiderDTO> dtos = new List<Tbl2OppettiderDTO>();

            using (SqlConnection connection = new SqlConnection(timerConnectionString))
            {
                string sqlQuery = $@"SELECT * FROM tbl2Oppettider";
                var queryResult = FrownedUponSQLClient.ExcuteQueryNew(connection, sqlQuery);
                var reader = queryResult.SqlDataReader;

                while (reader.Read())
                {
                    var dto = new Tbl2OppettiderDTO();

                    dto.OppettiderId = (int)reader["oppettiderID"];
                    dto.ButikId = (int)reader["butikID"];
                    dto.DagTyp = (int)reader["dagTyp"];
                    dto.Dag = (int)reader["dag"];
                    dto.Fran = (string)reader["fran"];
                    dto.Till = (string)reader["till"];
                    dto.Datum = (string)reader["datum"];
                    dto.Stangt = (int)reader["stangt"];
                    dto.DatumFrom = (string)reader["datumFrom"];
                    dto.GruppId = (int)reader["gruppID"];

                    dtos.Add(dto);
                }
            }
            return dtos;
        }
    }
}
