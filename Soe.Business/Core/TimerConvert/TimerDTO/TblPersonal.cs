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
    public class TblPersonalDTO
    {
        public TblPersonalDTO() { }

        public int PersonalID { get; internal set; }
        public string Fornamn { get; internal set; }
        public string Efternamn { get; set; }
        public string PersNr { get; set; }
        public string Kon { get; set; }
        public string AnstNr { get; set; }

        public string Tel1 { get; set; }
        public string Tel2 { get; set; }
        public string CoAdress { get; set; }
        public string Adress { get; set; }
        public string PostNr { get; set; }
        public string Ort { get; set; }
        public string Epost { get; set; }
        public string KommunNr { get; set; }

        public string KontoNr { get; set; }
        public string ClearingNr { get; set; }
        public string CheckNr { get; set; }
        public string Skattetabell { get; set; }
        public string SkattetabellTyp { get; set; }
        public float SkatteProc { get; set; }
        public float FriSkatteBelopp { get; set; }

        public string TaggID { get; set; }

        public int OwnedByRegion { get; set; }
        public int OwnedByUnit { get; set; }

        public List<int> ButikIDs { get; internal set; }
        public List<PersonalButikItem> PersonalButikRelations { get; set; }
        public List<Tbl2Anhorig> Anhoriga { get; set; }

        public List<int> GOAccountIds { get; set; }
        public int GOContactPersonId { get; internal set; }
        public int GOEmployeeId { get; internal set; }
    }

    public class PersonalButikItem
    {
        public PersonalButikItem() { }

        public int PersonalID { get; set; }
        public int ButikID { get; set; }
    }

    public class Tbl2Anhorig
    {
        public Tbl2Anhorig() { }

        public int Typ { get; set; }
        public string Fornamn { get; set; }
        public string Efternamn { get; set; }
        public string PersNr { get; set; }
        public string TelNr { get; set; }
        public string Mobil { get; set; }
    }



    public class TblPersonalFactory : TimerFactoryBase
    {
        private readonly string timerConnectionString;
        private readonly CompEntities entities;
        private readonly int actorCompanyId;

        public TblPersonalFactory(string timerConnectionString, CompEntities entities, int actorCompanyId)
        {
            this.timerConnectionString = timerConnectionString;
            this.entities = entities;
            this.actorCompanyId = actorCompanyId;
        }

        public Dictionary<int, TblPersonalDTO> GetDictionary()
        {
            return GetAll().GroupBy(x => x.PersonalID).ToDictionary(k => k.Key, w => w.First());
        }

        public List<TblPersonalDTO> GetAll()
        {
            var converts = entities.WtConvertMapping.Where(w => w.ActorCompanyId == actorCompanyId && w.Type == (int)TimerGOConvertType.Account);
            List<TblPersonalDTO> dtos = new List<TblPersonalDTO>();
            
            using (SqlConnection connection = new SqlConnection(timerConnectionString))
            {
                string sqlQuery = $@"SELECT * FROM tblPersonal WHERE (joker != 1 OR joker IS NULL) AND (persNr != '' OR persNr IS NOT NULL)";
                var queryResult = FrownedUponSQLClient.ExcuteQueryNew(connection, sqlQuery);
                var reader = queryResult.SqlDataReader;

                while (reader.Read())
                {
                    TblPersonalDTO existingDto = dtos.FirstOrDefault(x => x.PersNr == (string)reader["persNr"]);

                    if (existingDto != null)
                    {
                        // Prevent duplicate person relations
                        if (!existingDto.PersonalButikRelations.Any(x => x.PersonalID == (int)reader["personalID"]))
                        {
                            existingDto.PersonalButikRelations.Add(new PersonalButikItem { PersonalID = (int)reader["personalID"], ButikID = (int)reader["butikID"] });
                        }

                        // Prevent duplicate account relations
                        if (!existingDto.ButikIDs.Any(x => x == (int)reader["butikID"]))
                        {
                            existingDto.ButikIDs.Add((int)reader["butikID"]);

                            #region Mapping

                            var matchingConvert = converts.FirstOrDefault(w => w.WTId == (int)reader["butikID"]);
                            if (matchingConvert != null)
                            {
                                existingDto.GOAccountIds.Add(matchingConvert.XEId);
                            }

                            #endregion
                        }
                    }
                    else
                    {
                        var dto = new TblPersonalDTO();
                        dto.PersonalID = (int)reader["personalID"];
                        dto.Fornamn = (string)reader["fornamn"];
                        dto.Efternamn = (string)reader["efternamn"];
                        dto.PersNr = (string)reader["persNr"];
                        dto.Kon = (string)reader["kon"];
                        dto.AnstNr = (string)reader["anstNr"];

                        dto.Tel1 = (string)reader["tel1"];
                        dto.Tel2 = (string)reader["tel2"];
                        dto.CoAdress = (string)reader["co_adress"];
                        dto.Adress = (string)reader["adress"];
                        dto.PostNr = (string)reader["postNr"];
                        dto.Ort = (string)reader["ort"];
                        dto.KommunNr = (string)reader["kommunNr"];
                        dto.Epost = (string)reader["epost"];

                        dto.KontoNr = (string)reader["kontoNr"];
                        dto.ClearingNr = (string)reader["clearingNr"];
                        dto.CheckNr = (string)reader["checkNr"];
                        dto.Skattetabell = (string)reader["skattetabell"];
                        dto.SkattetabellTyp = (string)reader["skattetabellTyp"];
                        dto.SkatteProc = (float)reader["skatteProc"];
                        dto.FriSkatteBelopp = (float)reader["friSkatteBelopp"];

                        dto.TaggID = (string)reader["taggID"];

                        dto.OwnedByRegion = (int)reader["ownedByRegion"];
                        dto.OwnedByUnit = (int)reader["ownedByUnit"];

                        dto.ButikIDs.Add((int)reader["butikID"]);
                        existingDto.PersonalButikRelations.Add(new PersonalButikItem { PersonalID = (int)reader["personalID"], ButikID = (int)reader["butikID"] });

                        #region Mapping

                        var matchingConvert = converts.FirstOrDefault(w => w.WTId == (int)reader["butikID"]);
                        if (matchingConvert != null)
                        {
                            dto.GOAccountIds.Add(matchingConvert.XEId);
                        }

                        #endregion

                        dtos.Add(dto);
                    }
                }

                // Anhöriga

                sqlQuery = $@"SELECT * FROM tbl2Anhorig";
                queryResult = FrownedUponSQLClient.ExcuteQueryNew(connection, sqlQuery);
                reader = queryResult.SqlDataReader;

                while (reader.Read())
                {
                    int personalID = (int)reader["personalID"];
                    int typ = (int)reader["typ"];
                    string fornamn = (string)reader["fornamn"];
                    string efternamn = (string)reader["efternamn"];
                    string persNr = (string)reader["persNr"];
                    string telNr = (string)reader["telNr"];
                    string mobil = (string)reader["mobil"];

                    TblPersonalDTO dto = dtos.FirstOrDefault(p => p.PersonalButikRelations.Any(r => r.PersonalID == personalID));

                    if (dto != null)
                    {
                        /* Typ
                        
                        1: BARN
                        2: FÖRÄLDER
                        3: BARNBARN
                        4: MOR -/ FARFÖRÄLDER
                        5: SVÄRFÖRÄLDER
                        6: SYSKON
                        7: KUSIN
                        8: ANNAN
                        9: MAKE / MAKA
                        10: SAMBO */

                        if (!dto.Anhoriga.Any(a => a.Typ == typ && a.Fornamn == fornamn))
                        {
                            var anhorig = new Tbl2Anhorig()
                            {
                                Typ = typ,
                                Fornamn = fornamn,
                                Efternamn = efternamn,
                                PersNr = persNr,
                                TelNr = telNr,
                                Mobil = mobil,
                            };
                            dto.Anhoriga.Add(anhorig);
                        }
                    }
                }
            }
            return dtos;
        }


        public ActionResult SaveGOEmployees(bool doSave, out List<TblPersonalDTO> tblPersonalDTOs)
        {
            Dictionary<int, string> relativeRelationsDict = new Dictionary<int, string>()
            {
                { 1, "Barn" },
                { 2, "Förälder" },
                { 3, "Barnbarn" },
                { 4, "Mor- / Farförälder" },
                { 5, "Svärförälder" },
                { 6, "Syskon" },
                { 7, "Kusin" },
                { 8, "Annan" },
                { 9, "Make / Maka" },
                { 10, "Sambo" }
            };

            var dtos = GetAll();

            foreach (var dto in dtos)
            {
                int sex = (int)TermGroup_Sex.Unknown;

                switch (dto.Kon)
                {
                    case "M":
                        sex = (int)TermGroup_Sex.Male;
                        break;
                    case "F":
                        sex = (int)TermGroup_Sex.Female;
                        break;
                }

                #region Actor

                var actor = new Actor()
                {
                    ActorType = (int)SoeActorType.ContactPerson,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",
                };

                #endregion

                #region Contact

                var contact = new Contact()
                {
                    Actor = actor,
                    SysContactTypeId = (int)TermGroup_SysContactType.Employee,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",
                };

                #endregion

                #region ContactAddress

                var contactAddress = new ContactAddress()
                {
                    Contact = contact,
                    Name = "Utdelningsadress",
                    SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Distribution,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",
                };
                
                #region ContactAddressRow

                #region Adress
                var contactAddressRow1 = new ContactAddressRow()
                {
                    ContactAddress = contactAddress,
                    SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.Address,
                    Text = dto.Adress,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",
                };
                entities.ContactAddressRow.AddObject(contactAddressRow1);
                #endregion

                #region Postal code
                var contactAddressRow2 = new ContactAddressRow()
                {
                    ContactAddress = contactAddress,
                    SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.PostalCode,
                    Text = dto.PostNr,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",
                };
                entities.ContactAddressRow.AddObject(contactAddressRow2);
                #endregion

                #region City
                var contactAddressRow3 = new ContactAddressRow()
                {
                    ContactAddress = contactAddress,
                    SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.PostalAddress,
                    Text = dto.Ort,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",
                };
                entities.ContactAddressRow.AddObject(contactAddressRow3);
                #endregion

                #endregion

                #endregion

                #region ContactECom

                #region Phone
                var contactECom1 = new ContactECom()
                {
                    Contact = contact,
                    SysContactEComTypeId = (int)TermGroup_SysContactEComType.PhoneHome,
                    Text = dto.Tel1,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",
                };
                entities.ContactECom.AddObject(contactECom1);
                #endregion

                #region Mobilephone
                var contactECom2 = new ContactECom()
                {
                    Contact = contact,
                    SysContactEComTypeId = (int)TermGroup_SysContactEComType.PhoneMobile,
                    Text = dto.Tel2,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",
                };
                entities.ContactECom.AddObject(contactECom2);
                #endregion

                #region Email
                var contactECom3 = new ContactECom()
                {
                    Contact = contact,
                    SysContactEComTypeId = (int)TermGroup_SysContactEComType.Email,
                    Text = dto.Epost,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",
                };
                entities.ContactECom.AddObject(contactECom3);
                #endregion


                #region Closest relatives
                foreach (Tbl2Anhorig anhorig in dto.Anhoriga)
                {
                    var contactECom4 = new ContactECom()
                    {
                        Contact = contact,
                        SysContactEComTypeId = (int)TermGroup_SysContactEComType.ClosestRelative,
                        Text = anhorig.Mobil ?? anhorig.TelNr,
                        Description = anhorig.Fornamn + " " + anhorig.Efternamn + ";" + relativeRelationsDict[anhorig.Typ],
                        Created = DateTime.Now,
                        CreatedBy = "Timer Migration",
                    };
                    entities.ContactECom.AddObject(contactECom1);
                }
                #endregion

                #endregion

                #region ContactPerson

                var contactPerson = new ContactPerson()
                {
                    FirstName = dto.Fornamn,
                    LastName = dto.Efternamn,
                    SocialSec = dto.PersNr, //TODO: Formatting of SSN
                    Sex = sex,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",
                };

                #endregion

                var employee = new Employee()
                {
                    ActorCompanyId = actorCompanyId,
                    EmployeeNr = dto.AnstNr,
                    Created = DateTime.Now,
                    CreatedBy = "Timer Migration",

                    ContactPerson = contactPerson,

                    User = new User()
                    {
                        // TODO: Needed?
                        ContactPerson = contactPerson,
                    }
                };

                if (doSave)
                {
                    entities.Employee.AddObject(employee);
                    var result = Save(entities);

                    dto.GOEmployeeId = employee.EmployeeId;

                    if (result.Success)
                    {
                        var convert = new WtConvertMapping()
                        {
                            XEId = employee.EmployeeId,
                            ActorCompanyId = actorCompanyId,
                            WTId = dto.GOEmployeeId,
                            Type = (int)TimerGOConvertType.Employee
                        };
                        entities.WtConvertMapping.AddObject(convert);

                        result = Save(entities);
                    }
                }
            }
            tblPersonalDTOs = dtos;

            return Save(entities);
        }
    }
}
