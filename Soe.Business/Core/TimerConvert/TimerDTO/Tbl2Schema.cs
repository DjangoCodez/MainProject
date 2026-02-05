using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimerConvert.TimerDTO
{
    public class Tbl2SchemaDTO
    {
        public Tbl2SchemaDTO() { }

        public int ButikID { get; set; }
        public int PersonalID { get; set; }
        public string Datum { get; set; }
        public string Start { get; set; }
        public string Stop { get; set; }
        public string Rast { get; set; }
        public float ArbTot { get; set; }

        // Extensions
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public float GrossAmount { get; set; }
        public float NetAmount { get; set; }
        public DateTime BreakStartTime { get; set; }
        public DateTime BreakStopTime { get; set; }
        public float BreakAmount { get; set; }
    }


    public class Tbl2SchemaFactory : TimerFactoryBase
    {
        private readonly string timerConnectionString;
        private readonly CompEntities entities;
        private readonly int actorCompanyId;

        public Tbl2SchemaFactory(string timerConnectionString, CompEntities entities, int actorCompanyId)
        {
            this.timerConnectionString = timerConnectionString;
            this.entities = entities;
            this.actorCompanyId = actorCompanyId;
        }

        public List<Tbl2SchemaDTO> GetAll(DateTime dateFrom, DateTime dateTo)
        {
            //var converts = entities.WtConvertMapping.Where(w => w.ActorCompanyId == actorCompanyId && w.Type == (int)TimerGOConvertType.);
            List<Tbl2SchemaDTO> dtos = new List<Tbl2SchemaDTO>();

            using (SqlConnection connection = new SqlConnection(timerConnectionString))
            {
                string dateFromString = dateFrom.ToString("yyyyMMdd");
                string dateToString = dateTo.ToString("yyyyMMdd");

                string sqlQuery = $@"SELECT * FROM tbl2Schema WHERE datum BETWEEN '" + dateFromString + "' AND datum '" + dateToString + "'";
                var queryResult = FrownedUponSQLClient.ExcuteQueryNew(connection, sqlQuery);
                var reader = queryResult.SqlDataReader;

                while (reader.Read())
                {
                    var dto = new Tbl2SchemaDTO();

                    // CONTINUE HERE...

                    dtos.Add(dto);
                }
            }
            return dtos;
        }
    }
}
