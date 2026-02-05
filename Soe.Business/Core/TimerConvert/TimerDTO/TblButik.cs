using SoftOne.Soe.Business.Core.TimerConvert.Enum;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimerConvert.TimerDTO
{
    public class TblButikDTO
    {
        public TblButikDTO() { }

        public string ButikNr { get; internal set; }
        public int ButikID { get; internal set; }
        public int GOAccountId { get; internal set; }
        public string Butik { get; internal set; }
    }


    public class TblButikFactory : TimerFactoryBase
    {
        private readonly string timerConnectionString;
        private readonly CompEntities entities;
        private readonly int actorCompanyId;

        public TblButikFactory(string timerConnectionString, CompEntities entities, int actorCompanyId)
        {
            this.timerConnectionString = timerConnectionString;
            this.entities = entities;
            this.actorCompanyId = actorCompanyId;
        }

        public Dictionary<int, TblButikDTO> GetDictionary()
        {
            return GetAll().GroupBy(x => x.ButikID).ToDictionary(k => k.Key, w => w.First());
        }

        public List<TblButikDTO> GetAll() 
        {
            var converts = entities.WtConvertMapping.Where(w => w.ActorCompanyId == actorCompanyId && w.Type == (int)TimerGOConvertType.Account);
            List<TblButikDTO> dtos = new List<TblButikDTO>();

            using (SqlConnection connection = new SqlConnection(timerConnectionString))
            {
                string sqlQuery = $@"SELECT * FROM tblButik";
                var queryResult = FrownedUponSQLClient.ExcuteQueryNew(connection, sqlQuery);
                var reader = queryResult.SqlDataReader;

                while (reader.Read())
                {
                    string butik = (string)reader["butik"];
                    if (!butik.ToLower().EndsWith("_old"))
                    {
                        var dto = new TblButikDTO();
                        dto.ButikID = (int)reader["butikID"];
                        dto.ButikNr = (string)reader["butikNr"];
                        dto.Butik = butik; // Name

                        #region Mapping

                        var matchingConvert = converts.FirstOrDefault(w => w.WTId == dto.ButikID);
                        if (matchingConvert != null)
                        {
                            dto.GOAccountId = matchingConvert.XEId;
                        }

                        #endregion

                        dtos.Add(dto);
                    }
                }
            }
            return dtos;
        }


        public ActionResult SaveGOAccounts(bool doSave, out List<TblButikDTO> tblButikDTOs)
        {
            var dtos = GetAll();
            var dim = entities.AccountDim.FirstOrDefault(w => w.ActorCompanyId == actorCompanyId && w.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre && w.State == (int)SoeEntityState.Active);

            // Add only new accounts
            foreach (var dto in dtos.Where(w => w.GOAccountId == 0))
            {
                var accountInternal = new AccountInternal() 
                { 
                    Account = new Account()
                    {
                        ActorCompanyId = actorCompanyId,
                        Name = dto.Butik,
                        AccountNr = dto.ButikNr,
                        Created = DateTime.Now,
                        CreatedBy = "Timer Migration",
                        AccountDimId = dim.AccountDimId

                        // TODO: Contact information
                    }
                };
                if (doSave)
                {
                    entities.AccountInternal.AddObject(accountInternal);
                    var result = Save(entities);

                    dto.GOAccountId = accountInternal.AccountId;

                    if (result.Success)
                    {
                        var convert = new WtConvertMapping()
                        {
                            XEId = accountInternal.AccountId,
                            ActorCompanyId = actorCompanyId,
                            WTId = dto.ButikID,
                            Type = (int)TimerGOConvertType.Account
                        };
                        entities.WtConvertMapping.AddObject(convert);

                        result = Save(entities);
                    }
                }    
            }

            tblButikDTOs = dtos;

            return Save(entities);
        }
    }
}
