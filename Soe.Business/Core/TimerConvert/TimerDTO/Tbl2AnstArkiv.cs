using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimerConvert.TimerDTO
{
    public class Tbl2AnstArkivDTO
    {
        public Tbl2AnstArkivDTO() { }

        public int AnstArkivId { get; set; }
        public int PersonalId { get; set; }
        public int ButikId { get; set; }
        //public int RefButikId { get; set; }
        public string Avtal { get; set; } // to be matched with Tbl2AvtalDTO.AvtGrupp
        public string AnstTyp { get; set; }
        public float ArbProcent { get; set; }
        public float ArbTid { get; set; }
        public string Text { get; set; }
        public int TjansteBenamning { get; set; } // Contains id to be matched with 'Befattning'
        public int Placering { get; set; } // Contains id to be matched with ?
        public float Lon { get; set; } // Salary according to work rate
        public float Lon100 { get; set; } // Salary at 100% work rate
        public string LonTyp { get; set; } // 'tlon'/'mlon'
        public string BranchvanaBeskr { get; set; }
        public int BranchvanaAr { get; set; }
        public int BranchvanaManad { get; set; }
        public string ArbetsOmf { get; set; } // maybe not needed ('heltid'/'deltid')
        public DateTime? Tilltradesdag { get; set; } // maybe not needed
        public int Nyanställning { get; set; } // maybe not needed '1'/'2'
        public string VikPga { get; set; }
        public string AnstalldFor { get; set; } // ex substitute for whom
        public bool SemErsLon { get; set; } // holiday pay included in salary
        public bool Tjm { get; set; } // officer/white collar '1'/'0'
        public DateTime DatumFrom { get; set; } // agreement date from
        public DateTime DatumTom { get; set; } // agreement date to
        public DateTime? DatumAndrad { get; set; }
        public DateTime AvtalFrom { get; set; }
        public DateTime AvtalTom { get; set; }
        public bool Avvikelserap { get; set; }
        public int AvtalTyp { get; set; } // 0 = basavtal, 1 = plusavtal, 2 = tillfälligt avtal
        public float LonTillagg { get; set; }
        public string LonTillaggTyp { get; set; }
        public bool ReseTillagg { get; set; }
        public int SemDagar { get; set; }
        public string LoneavtalAr { get; set; }
        public DateTime? ProvDatum { get; set; }
        public bool Fyllt67 { get; set; }
        public int LonenivaId { get; set; } // id to match with paylevel registry (for min wage etc)
        public bool DagsAvtal { get; set; } // Agreement is written for only 1 day at a time
        public int YrkeskodId { get; set; } // id to match with SSYK registry
        public bool Inlanad { get; set; } // lended in, 1 = yes, 0/null = no
        public int? RefBId { get; set; } // from which Butik its lended
        public int? RefPId { get; set; }
        public int BelastnKontoId { get; set; } // id to match with Belastningskonto registry
        public bool InlanBelastning { get; set; }
        public string AvtalNr { get; set; }
        public bool KortaAvtal { get; set; } // if agreement is for less than 7 days. Agreed weekly worktime is distributed on agreement days instead of 7 days.
        public bool Avslutad { get; set; }
        public DateTime? AvslutDatum { get; set; } // end date if agreement is ended
        public int AvslutId { get; set; } // id to match with Avslut registry

        // Extensions
        public Tbl2AvtalDTO GOAvtal { get; set; }
    }


    public class Tbl2AnstArkivFactory : TimerFactoryBase
    {
        private readonly string timerConnectionString;
        private readonly CompEntities entities;
        private readonly int actorCompanyId;

        public Tbl2AnstArkivFactory(string timerConnectionString, CompEntities entities, int actorCompanyId)
        {
            this.timerConnectionString = timerConnectionString;
            this.entities = entities;
            this.actorCompanyId = actorCompanyId;
        }

        private DateTime TryParseYYYYMMDD(string value)
        {
            string valueWithDashes = value.Substring(0, 4) + "-" + value.Substring(4, 2) + "-" + value.Substring(6, 2);
            return DateTime.TryParse(valueWithDashes, out DateTime valueOut) ? valueOut : CalendarUtility.DATETIME_DEFAULT;
        }

        public List<Tbl2AnstArkivDTO> GetForPersonalAndButik(int timerPersonalId, int timerButikId, DateTime timerDateFrom, DateTime timerDateTo, List<Tbl2AvtalDTO> dictAvtal)
        {
            //var converts = entities.WtConvertMapping.Where(w => w.ActorCompanyId == actorCompanyId && w.Type == (int)TimerGOConvertType.PayrollProduct);
            List<Tbl2AnstArkivDTO> dtos = new List<Tbl2AnstArkivDTO>();

            using (SqlConnection connection = new SqlConnection(timerConnectionString))
            {
                //Open sql connection
                connection.Open();

                SqlCommand cmd = GetCommandAgreements(connection, timerButikId, timerPersonalId, timerDateFrom, timerDateTo);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        #region Parse data

                        //int placeringId = int.TryParse((string)reader["placering"], out int placeringIdOut) ? placeringIdOut : 0;
                        DateTime tilltradesdag = TryParseYYYYMMDD(reader.GetString(25)); // DateTime.TryParse((string)reader["tilltradesdag"], out DateTime tilltradesdagOut) ? tilltradesdagOut : CalendarUtility.DATETIME_DEFAULT;

                        DateTime datumFrom;
                        if (reader.GetString(9) == "00000000") // (string)reader["datumFrom"]
                            datumFrom = CalendarUtility.DATETIME_MINVALUE;
                        else
                            datumFrom = TryParseYYYYMMDD(reader.GetString(9)); // DateTime.TryParse(reader.GetString(9), out DateTime datumFromOut) ? datumFromOut : CalendarUtility.DATETIME_DEFAULT;

                        DateTime datumTom;
                        if (reader.GetString(10) == "99999999") // (string)reader["datumTom"]
                            datumTom = CalendarUtility.DATETIME_MAXVALUE;
                        else
                            datumTom = TryParseYYYYMMDD(reader.GetString(10)); // DateTime.TryParse(reader.GetString(10), out DateTime datumTomOut) ? datumTomOut : CalendarUtility.DATETIME_DEFAULT;

                        DateTime? datumAndrad = null;
                        if (reader["datumAndrad"] != DBNull.Value && reader.GetString(29) != null) // reader["datumAndrad"]
                            datumAndrad = TryParseYYYYMMDD(reader.GetString(29)); // DateTime.TryParse(reader.GetString(29), out DateTime datumAndradOut) ? datumAndradOut : CalendarUtility.DATETIME_DEFAULT;

                        DateTime? provDatum = null;
                        if (reader["provDatum"] != DBNull.Value && reader.GetString(5) != null) // reader["provDatum"]
                            provDatum = TryParseYYYYMMDD(reader.GetString(5)); // DateTime.TryParse(reader.GetString(5), out DateTime provDatumOut) ? provDatumOut : CalendarUtility.DATETIME_DEFAULT;

                        DateTime? avslutDatum = null;
                        if (reader["avslutDatum"] != DBNull.Value && reader.GetString(51) != null) // reader["avslutDatum"]
                            avslutDatum = TryParseYYYYMMDD(reader.GetString(51)); // DateTime.TryParse(reader.GetString(51), out DateTime avslutDatumOut) ? avslutDatumOut : CalendarUtility.DATETIME_DEFAULT;

                        DateTime avtalFrom;
                        if (reader.GetString(46) == "00000000") // (string)reader["avtalFrom"]
                            avtalFrom = CalendarUtility.DATETIME_MINVALUE;
                        else
                            avtalFrom = TryParseYYYYMMDD(reader.GetString(46)); // DateTime.TryParse(reader.GetString(46), out DateTime avtalFromOut) ? avtalFromOut : CalendarUtility.DATETIME_DEFAULT;

                        DateTime avtalTom;
                        if (reader.GetString(47) == "99999999") // (string)reader["avtalTom"]
                            avtalTom = CalendarUtility.DATETIME_MAXVALUE;
                        else
                            avtalTom = TryParseYYYYMMDD(reader.GetString(47)); // DateTime.TryParse(reader.GetString(47), out DateTime avtalTomOut) ? avtalTomOut : CalendarUtility.DATETIME_DEFAULT;

                        #endregion

                        var dto = new Tbl2AnstArkivDTO();

                        dto.AnstArkivId = reader.GetInt32(4);  // (int)reader["aid"];
                        dto.PersonalId = reader.GetInt32(3); // (int)reader["personalID"];
                        dto.ButikId = timerButikId;
                        dto.RefBId = reader["refBID"] == DBNull.Value ? 0 : reader.GetInt32(27);
                        dto.Avtal = reader["avtal"] != DBNull.Value ? (string)reader["avtal"] : string.Empty; // to be matched with Tbl2AvtalDTO.AvtGrupp
                        dto.AnstTyp = reader["anstTyp"] != DBNull.Value ? (string)reader["anstTyp"] : string.Empty;
                        dto.ArbProcent = reader["arbProcent"] != DBNull.Value ? reader.GetFloat(13) : 0;
                        dto.ArbTid = reader["arbTid"] != DBNull.Value ? reader.GetFloat(14) : 0;
                        //dto.Text = (string)reader["text"];
                        dto.TjansteBenamning = reader["tjanstebenamning"] != DBNull.Value ? (int)reader["tjanstebenamning"] : 0; // Contains id to be matched with 'Befattning'
                                                                                                                                 //dto.Placering = placeringId; // Contains id to be matched with ?
                        dto.Lon = (float)reader["lon"]; // Salary according to work rate
                                                        //dto.Lon100 = (float)reader["lon100"]; // Salary at 100% work rate
                        dto.LonTyp = (string)reader["lonTyp"]; // 'tlon'/'mlon'
                                                               //dto.BranchvanaBeskr = (string)reader["branchvana_beskr"];
                        dto.BranchvanaAr = (int)reader["branchvana_ar"];
                        dto.BranchvanaManad = (int)reader["branchvana_manad"];
                        dto.ArbetsOmf = reader["arbetsOmf"] != DBNull.Value ? (string)reader["arbetsOmf"] : string.Empty; // maybe not needed ('heltid'/'deltid')
                        dto.Tilltradesdag = tilltradesdag; // maybe not needed
                                                           //dto.Nyanställning = (int)reader["nyanstallning"]; // maybe not needed '1'/'2'
                                                           //dto.VikPga = (string)reader["vikpga"];
                                                           //dto.AnstalldFor = (string)reader["anstalld_for"]; // ex substitute for whom
                        dto.SemErsLon = reader["semers_lon"] != DBNull.Value ? (int)reader["semers_lon"] == 1 : false; // holiday pay included in salary
                        dto.Tjm = (int)reader["tjm"] == 1; // officer/white collar '1'/'0'
                        dto.DatumFrom = datumFrom; // agreement date from
                        dto.DatumTom = datumTom; // agreement date to
                        dto.DatumAndrad = datumAndrad;
                        dto.AvtalFrom = avtalFrom; // base agreement valid from
                        dto.AvtalTom = avtalTom; // base agreement valid to
                        dto.Avvikelserap = reader["avtal_avrap"] != DBNull.Value ? (int)reader["avtal_avrap"] == 1 : false;
                        dto.AvtalTyp = (int)reader["avtalTyp"]; // 0 = basavtal, 1 = plusavtal, 2 = tillfälligt avtal
                        dto.LonTillagg = (float)reader["lonTillagg"];
                        dto.LonTillaggTyp = (string)reader["lonTillaggTyp"];
                        //dto.ReseTillagg = (int)reader["reseTillagg"] == 1;
                        dto.SemDagar = (int)reader["semDagar"]; //
                                                                //dto.LoneavtalAr = (string)reader["loneAvtalAr"];
                        dto.ProvDatum = provDatum;
                        //dto.Fyllt67 = (int)reader["fyllt67"] == 1;
                        //dto.LonenivaId = (int)reader["loneniva"]; // id to match with paylevel registry (for min wage etc)
                        //dto.DagsAvtal = (int)reader["dagsAvtal"] == 1; // Agreement is written for only 1 day at a time
                        dto.YrkeskodId = (int)reader["yrkeskodID"]; // id to match with SSYK registry
                        dto.Inlanad = (int)reader["inlanad"] == 1; // lended in, 1 = yes, 0/null = no
                        dto.RefBId = reader["refBid"] != DBNull.Value ? (int)reader["refBid"] : 0; // from which Butik its lended
                        dto.RefPId = reader["refPid"] != DBNull.Value ? (int)reader["refPid"] : 0;
                        //dto.BelastnKontoId = (int)reader["belastnKonto"]; // id to match with Belastningskonto registry
                        dto.InlanBelastning = (int)reader["inlanBelastning"] == 1;
                        //dto.AvtalNr = (string)reader["avtalNr"];
                        dto.KortaAvtal = reader["kortaAvtal"] != DBNull.Value ? (int)reader["kortaAvtal"] == 1 : false; // if agreement is for less than 7 days. Agreed weekly worktime is distributed on agreement days instead of 7 days.
                        dto.Avslutad = reader["avslutad"] != DBNull.Value ? (int)reader["avslutad"] == 1 : false;
                        dto.AvslutDatum = avslutDatum; // end date if agreement is ended
                                                       //dto.AvslutId = (int)reader["avslutID"];

                        #region Mapping

                        // connect GOs payroll products (loaded from setup conversion table)
                        dto.GOAvtal = dictAvtal.FirstOrDefault(a => a.AvtGrupp == dto.Avtal);

                        // dto.GOPayrollProductId = matchingConvert.PayrollProductId;

                        #endregion

                        dtos.Add(dto);
                    }
                }

                // Close connection
                connection.Close();
            }
            return dtos;
        }
    }
}
