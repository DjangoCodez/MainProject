using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimerConvert.TimerDTO
{
    public class Tbl2AvtalDTO
    {
        public Tbl2AvtalDTO() { }

        public string AvtGrupp { get; internal set; }
        public string Namn { get; internal set; }
        public int GOVacationGroupId { get; set; }
        public int GOEmployeeGroupId { get; set; }
        public int GOPayrollGroupId { get; set; }
    }

    public class GOGroups
    {
        public GOGroups() { }

        public int GOEmployeeGroupId { get; set; }
        public int GOVacationGroupId { get; set; }
        public int GOPayrollGroupId { get; set; }
    }


    public class Tbl2AvtalFactory : TimerFactoryBase
    {
        private readonly string timerConnectionString;
        private readonly CompEntities entities;
        private readonly int actorCompanyId;

        public Tbl2AvtalFactory(string timerConnectionString, CompEntities entities, int actorCompanyId)
        {
            this.timerConnectionString = timerConnectionString;
            this.entities = entities;
            this.actorCompanyId = actorCompanyId;
        }

        public Dictionary<string, Tbl2AvtalDTO> GetDictionary()
        {
            return GetAll().GroupBy(x => x.AvtGrupp).ToDictionary(k => k.Key, w => w.First());
        }

        public List<Tbl2AvtalDTO> GetAll()
        {
            //var converts = entities.WtConvertMapping.Where(w => w.ActorCompanyId == actorCompanyId && w.Type == (int)TimerGOConvertType.Agreement);
            List<Tbl2AvtalDTO> dtos = new List<Tbl2AvtalDTO>();

            using (SqlConnection connection = new SqlConnection(timerConnectionString))
            {
                string sqlQuery = $@"SELECT * FROM tbl2Avtal";
                var queryResult = FrownedUponSQLClient.ExcuteQueryNew(connection, sqlQuery);
                var reader = queryResult.SqlDataReader;

                while (reader.Read())
                {
                    var dto = new Tbl2AvtalDTO();
                    dto.AvtGrupp = (string)reader["AvtGrupp"];
                    dto.Namn = (string)reader["Namn"];

                    #region Mapping

                    // connect GOs different agreement groups (loaded from setup conversion table)

                    GOGroups groups = GetGOGroups(dto.AvtGrupp);

                    dto.GOEmployeeGroupId = groups.GOEmployeeGroupId;
                    dto.GOPayrollGroupId = groups.GOPayrollGroupId;
                    dto.GOVacationGroupId = groups.GOVacationGroupId;

                    // dto.GOEmployeeGroupId = matchingConvert.EmployeeGroupId;
                    // dto.GOPayrollGroupId = matchingConvert.PayrollGroupId;
                    // dto.GOVacationGroupId = matchingConvert.VacationGroupId;

                    #endregion

                    dtos.Add(dto);
                }
            }
            return dtos;
        }

        public GOGroups GetGOGroups(string timerAvtGrupp)
        {
            // These group ids should be loaded from a more suitable setup setting (or columns in tblAvtal in Timer)
            Dictionary<string, GOGroups> groupsDictionary = new Dictionary<string, GOGroups>();
            groupsDictionary["asd"] = new GOGroups { GOEmployeeGroupId = 1, GOPayrollGroupId = 1, GOVacationGroupId = 1 };

            return groupsDictionary[timerAvtGrupp] ?? new GOGroups();
        }
    }
}
