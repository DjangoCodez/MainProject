using SoftOne.Soe.Common.Util;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util
{
    public class ImportCreateInput
    {
        private XEConnectDataBaseMethods dbMethods = new XEConnectDataBaseMethods();

        private DataRelation RelationImport;

        private DataSet dsCommon = new DataSet();                   //"SysImportHead", "SysImporttSelect", "SysImportRelation"
        private DataSet dsCompany = new DataSet();                  //"Company", "ImportDefinition", "ImportDefinitionLevel"

        private DataSet dsImportXmlData = new DataSet();            //Tabeller som skapas när importdata är i Xml-format
        private DataSet dsImportXmlRelations = new DataSet();       //Relationer som skapas när importdata är i Xml-format
        private DataSet dsImportTables = new DataSet();             //Tabeller som skapas med tabelldata från importfil
        private DataSet dsImportOutput = new DataSet();             //Tabeller som skapas med tabelldata för import i affärslager
        private DataSet dsImportDefinitionColumns = new DataSet();
        private DataSet dsImportSelectColumns = new DataSet();


        //Connection-sträng, Databasnamn, Företagsnr, Importnamn, Ström Importdata, Filnamn Importdata 
        public ActionResult ImportCreateImport(string compConnectionString, string sysConnectionString, int actorCompanyId, int importId, string importData, int sysImportDefinitionId, int sysImportHeadId, out string returnValue)
        {
            returnValue = string.Empty;
            bool compImport = importId == 0 ? false : true;

            ActionResult result = new ActionResult();

            if (string.IsNullOrEmpty(importData))
                return new ActionResult((int)ActionResultSave.NothingSaved, "Filen innehåller ingen importdata");


            result = dbMethods.GetTabelDataSet(compConnectionString, dsCompany, "Company", "Select ActorCompanyId, Name,  CAST(ActorCompanyId AS varchar(5))+' - '+Name AS FtgNamn From Company Where ActorCompanyId = " + actorCompanyId);
            if (!result.Success)
                return result;

            if (dsCompany.Tables["Company"].Rows.Count == 0)
                return new ActionResult((int)ActionResultSave.NothingSaved, "Företag kunde inte hittas");

            if (compImport)
            {
                //Hämta aktuell definition i tabell Import
                result = dbMethods.GetTabelDataSet(compConnectionString, dsCompany, "Import", "Select * From Import Where ActorCompanyId = " + actorCompanyId + " And ImportId = " + importId);
                if (!result.Success)
                    return result;

                if (dsCompany.Tables["Import"].Rows.Count == 0)
                    return new ActionResult((int)ActionResultSave.NothingSaved, "Det finns ingen definition upplagd med ImportId: " + importId);
            }

            if (compImport && Convert.ToBoolean(dsCompany.Tables["Import"].Rows[0]["Standard"]) == false)
            {
                //Hämta aktuell definition i tabell ImportDefinition
                result = dbMethods.GetTabelDataSet(compConnectionString, dsCompany, "ImportDefinition", "Select * From ImportDefinition Where ActorCompanyId = " + actorCompanyId + " And ImportDefinitionId = " + Convert.ToInt32(dsCompany.Tables["Import"].Rows[0]["ImportDefinitionId"]));
                if (!result.Success)
                    return result;

                if (dsCompany.Tables["ImportDefinition"].Rows.Count == 0)
                    return new ActionResult((int)ActionResultSave.NothingSaved, "Det finns ingen definition upplagd med ImportDefinitionId: " + Convert.ToInt32(dsCompany.Tables["Import"].Rows[0]["ImportDefinitionId"]));

                //Hämta kolumndefinitioner för aktuell definition i tabell ImportDefinitionLevel
                result = dbMethods.GetTabelDataSet(compConnectionString, dsCompany, "ImportDefinitionLevel", "Select * From ImportDefinitionLevel Where ImportDefinitionId = " + Convert.ToInt32(dsCompany.Tables["Import"].Rows[0]["ImportDefinitionId"]) + " Order By Level");
                if (!result.Success)
                    return result;

                if (dsCompany.Tables["ImportDefinitionLevel"].Rows.Count == 0)
                    return new ActionResult((int)ActionResultSave.NothingSaved, "Det finns inga kolumner upplagda för ImportDefinitionId: " + Convert.ToInt32(dsCompany.Tables["Import"].Rows[0]["ImportDefinitionId"]));
            }
            else
            {
                //Hämta aktuell definition i tabell SysImportDefinition
                try
                {
                    sysImportDefinitionId = Convert.ToInt32(dsCompany.Tables["Import"].Rows[0]["ImportDefinitionId"]);
                }
                catch
                {
                    // Intentionally ignored, safe to continue
                    // NOSONAR
                }

                result = dbMethods.GetTabelDataSet(sysConnectionString, dsCompany, "ImportDefinition", "Select * From SysImportDefinition Where SysImportDefinitionId = " + sysImportDefinitionId);
                if (!result.Success)
                    return result;

                if (dsCompany.Tables["ImportDefinition"].Rows.Count == 0)
                    return new ActionResult((int)ActionResultSave.NothingSaved, "Det finns ingen definition upplagd med SysImportDefinitionId: " + sysImportDefinitionId);

                //Hämta kolumndefinitioner för aktuell definition i tabell SysImportDefinitionLevel
                result = dbMethods.GetTabelDataSet(sysConnectionString, dsCompany, "ImportDefinitionLevel", "Select * From SysImportDefinitionLevel Where SysImportDefinitionId = " + sysImportDefinitionId + " Order By Level");
                if (!result.Success)
                    return result;

                if (dsCompany.Tables["ImportDefinitionLevel"].Rows.Count == 0)
                    return new ActionResult((int)ActionResultSave.NothingSaved, "Det finns inga kolumner upplagda för SysImportDefinitionId: " + sysImportDefinitionId);
            }

            //Hämta SoftOnes standard för aktuell definition i tabell SysImportHead
            try
            {
                sysImportHeadId = Convert.ToInt32(dsCompany.Tables["ImportDefinition"].Rows[0]["SysImportHeadId"]);
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }

            result = dbMethods.GetTabelDataSet(sysConnectionString, dsCommon, "SysImportHead", "Select * From SysImportHead Where SysImportHeadId = " + sysImportHeadId);
            if (!result.Success)
                return result;
            if (dsCommon.Tables["SysImportHead"].Rows.Count == 0)
                return new ActionResult((int)ActionResultSave.NothingSaved, "Det finns ingen SysImportHead upplagd för definition");

            //Hämta SoftOnes standard för aktuell definition i tabell SysImportSelect
            result = dbMethods.GetTabelDataSet(sysConnectionString, dsCommon, "SysImporttSelect", "Select * From SysImportSelect Where SysImportHeadId = " + sysImportHeadId + " Order By Level");
            if (!result.Success)
                return result;

            if (dsCommon.Tables["SysImporttSelect"].Rows.Count == 0)
                return new ActionResult((int)ActionResultSave.NothingSaved, "Det finns ingen SysImportSelect upplagd för definition");

            //Hämta SoftOnes standard för aktuell definition i tabell SysImportRelation
            result = dbMethods.GetTabelDataSet(sysConnectionString, dsCommon, "SysImportRelation", "Select * From SysImportRelation Where SysImportHeadId = " + sysImportHeadId);
            if (!result.Success)
                return result;

            //Skapa tabeller med xml-information om definierade kolumner för respektive rad i tabellerna ImportDefinitionLevel och SysImportSelect
            CreateTablesForColumns();

            //Skapar utdata för format = xml
            if (Convert.ToInt32(dsCompany.Tables["ImportDefinition"].Rows[0]["Type"]) == (int)TermGroup_SysImportDefinitionType.XML)
                CreateTablesTypeXml(importData);
            else
                //Skapar utdata för format = separator
                if (Convert.ToInt32(dsCompany.Tables["ImportDefinition"].Rows[0]["Type"]) == (int)TermGroup_SysImportDefinitionType.Separator)
                CreateTablesTypeSeparator(importData);
            else
                    //Skapar utdata för format = fasta kolumner
                    if (Convert.ToInt32(dsCompany.Tables["ImportDefinition"].Rows[0]["Type"]) == (int)TermGroup_SysImportDefinitionType.Fixed)
                CreateTablesTypeColumns(importData);

            CreateRelations();

            //foreach (DataTable tabImportTables in dsImportTables.Tables)
            //{
            //    foreach (DataRow rowSysImporttSelect in dsCommon.Tables["SysImporttSelect"].Rows)
            //    {
            //        if (rowSysImporttSelect["Level"].ToString() == tabImportTables.TableName)
            //        {
            //            tabImportTables.TableName = rowSysImporttSelect["Name"].ToString();
            //            break;
            //        }
            //    }
            //}


            //Skapa en minnesström av tabeller i dataset dsImportTables som flyttas till strängen som returnerar importdata
            MemoryStream memStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memStream, Encoding.Default);
            dsImportTables.DataSetName = "ImportData";
            dsImportTables.WriteXml(writer, XmlWriteMode.IgnoreSchema);
            returnValue = Encoding.Default.GetString(memStream.GetBuffer(), 0, Convert.ToInt32(memStream.Length)).ToString();

            //Ta bort rader i strängen som returnerar importdata som avser kolumner (taggar) som enbart skapats för relationer av tabeller
            byte[] byteArray = Encoding.Default.GetBytes(returnValue);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);
            string line;
            returnValue = "";

            var stringBuilder = new StringBuilder();

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("My_Id") == true || line.Contains("Parent_Id") == true) continue;
                stringBuilder.AppendLine(line);
                //returnValue += line;
            }
            returnValue = stringBuilder.ToString();
            return result;

        }

        //Skapar tabell/tabeller med information om respektive kolumn som skall Importeras enligt definitionen
        private void CreateTablesForColumns()
        {
            //Utförs för respektive definition av kolumner i tabell ImportDefinitionLevel
            foreach (DataRow rowImportDefinitionLevel in dsCompany.Tables["ImportDefinitionLevel"].Rows)
            {
                if (rowImportDefinitionLevel["Xml"].ToString() == "") continue;

                //Skapa tabell Columns i dataset dsImportDefinitionColumns från xml-ström i tabellen ImportDefinitionLevel
                string xml = "<?xml version='1.0' encoding='ISO-8859-1'?>" + rowImportDefinitionLevel["Xml"].ToString();
                System.IO.Stream stream = new System.IO.MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(xml));
                dsImportDefinitionColumns.ReadXml(stream, XmlReadMode.InferSchema);
                //Lägg till kolumner i tabell Columns

                if (dsImportDefinitionColumns.Tables["Columns"].Columns["Column"] == null)
                    dsImportDefinitionColumns.Tables["Columns"].Columns.Add("Column");

                if (dsImportDefinitionColumns.Tables["Columns"].Columns["Update"] == null)
                    dsImportDefinitionColumns.Tables["Columns"].Columns.Add("Update");

                if (dsImportDefinitionColumns.Tables["Columns"].Columns["From"] == null)
                    dsImportDefinitionColumns.Tables["Columns"].Columns.Add("From");

                if (dsImportDefinitionColumns.Tables["Columns"].Columns["Characters"] == null)
                    dsImportDefinitionColumns.Tables["Columns"].Columns.Add("Characters");

                if (dsImportDefinitionColumns.Tables["Columns"].Columns["XmlTag"] == null)
                    dsImportDefinitionColumns.Tables["Columns"].Columns.Add("XmlTag");

                if (dsImportDefinitionColumns.Tables["Columns"].Columns["XmlAttribute"] == null)
                    dsImportDefinitionColumns.Tables["Columns"].Columns.Add("XmlAttribute");

                if (dsImportDefinitionColumns.Tables["Columns"].Columns["Position"] == null)
                    dsImportDefinitionColumns.Tables["Columns"].Columns.Add("Position");

                if (dsImportDefinitionColumns.Tables["Columns"].Columns["Standard"] == null)
                    dsImportDefinitionColumns.Tables["Columns"].Columns.Add("Standard");

                if (dsImportDefinitionColumns.Tables["Columns"].Columns["Convert"] == null)
                    dsImportDefinitionColumns.Tables["Columns"].Columns.Add("Convert");

                dsImportDefinitionColumns.Tables["Columns"].Columns.Add("Level");
                dsImportDefinitionColumns.Tables["Columns"].Columns.Add("Field");
                dsImportDefinitionColumns.Tables["Columns"].Columns.Add("DataType");

                //Skapa tabell Columns i dataset dsImportSelectColumns från xml-ström i tabellen SysImportSelect
                foreach (DataRow rowImportSelect in dsCommon.Tables["SysImporttSelect"].Rows)
                {
                    if (Convert.ToInt32(rowImportDefinitionLevel["Level"]) != Convert.ToInt32(rowImportSelect["Level"])) continue;
                    xml = "<?xml version='1.0' encoding='ISO-8859-1'?>" + rowImportSelect["Settings"].ToString();
                    stream = new System.IO.MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(xml));
                    dsImportSelectColumns.ReadXml(stream);
                    dsImportOutput.Tables.Add(rowImportDefinitionLevel["Level"].ToString());
                }
                foreach (DataRow rowImportSelect in dsImportSelectColumns.Tables["Columns"].Rows)
                {
                    dsImportOutput.Tables[rowImportDefinitionLevel["Level"].ToString()].Columns.Add(rowImportSelect["Column"].ToString());
                }

                int Field = dsImportDefinitionColumns.Tables["Columns"].Rows.Count - 1;
                //Utförs för respektive kolumndefinition som skapats utifrån tabellen ImportDefinitionLevel
                for (int i = dsImportDefinitionColumns.Tables["Columns"].Rows.Count - 1; i > -1; i--)
                {
                    DataRow rowImportDefinition = dsImportDefinitionColumns.Tables["Columns"].Rows[i];
                    //Lägger till värden för kolumner som avser "Fri standard" eller "Xml-tagg"
                    if (rowImportDefinition["Column"].ToString() == "XmlTag" || rowImportDefinition["Column"].ToString() == "RecordType")
                    {
                        rowImportDefinition["Level"] = rowImportDefinitionLevel["Level"];
                        rowImportDefinition["Field"] = Field--;
                        rowImportDefinition["DataType"] = "Sträng";

                        continue;
                    }
                    bool Exist = false;
                    //Lägger till värden för kolumner som inte avser "Fri standard" eller "Xml-tagg"
                    foreach (DataRow rowImportSelect in dsImportSelectColumns.Tables["Columns"].Rows)
                    {
                        if (rowImportDefinition["Column"].ToString() != rowImportSelect["Column"].ToString()) continue;
                        rowImportDefinition["Level"] = rowImportDefinitionLevel["Level"];
                        rowImportDefinition["Field"] = Field--; //"Field" + Field++;
                        rowImportDefinition["DataType"] = rowImportSelect["DataType"].ToString();
                        Exist = true;
                        break;
                    }

                    //Kolumnen tas bort om den finns definierad i ImportDefinitionLevel men inte i SysImportSelect
                    if (Exist == false && rowImportDefinition["Column"].ToString() != "XmlTag")
                        rowImportDefinition.Delete();
                }

                dsImportDefinitionColumns.Tables["Columns"].AcceptChanges();

                //Ändra tabellnamn från "Columns" till "Columns" + Level
                dsImportDefinitionColumns.Tables["Columns"].TableName = rowImportDefinitionLevel["Level"].ToString();
                dsImportSelectColumns.Tables["Columns"].TableName = rowImportDefinitionLevel["Level"].ToString();
            }

        }


        private string ConvertField(DataRow rowImportDefinition, string Field)
        {
            char[] Delimiter1 = { '&' };
            char[] Delimiter2 = { '=' };
            string[] String1 = rowImportDefinition["Convert"].ToString().Split(Delimiter1);

            for (int i = 0; i < String1.Length; i++)
            {
                string[] String2 = String1[i].Split(Delimiter2);
                int convertFrom = -1;

                if (String2[0] == "+" || String2[0] == "-" || String2[0] == "*" || String2[0] == "/" || String2[0] == ">" || String2[0] == "<" || String2[0] == "r" || String2[0] == "R" || int.TryParse(String2[0], out convertFrom))
                {
                    if (rowImportDefinition["DataType"].ToString().Contains("Heltal") == true)
                    {
                        try
                        {
                            int ValueIntField = Convert.ToInt32(Convert.ToDecimal(Field));
                            int ValueIntString2 = Convert.ToInt32(String2[1]);
                            int ValueIntString3 = String2.Length > 2 ? Convert.ToInt32(String2[2]) : 0;
                            int ValueIntString4 = String2.Length > 3 ? Convert.ToInt32(String2[3]) : 0;

                            if (convertFrom > 0 && ValueIntField == convertFrom)
                            {
                                // int convertion found, so set Field and break for loop
                                Field = ValueIntString2.ToString();
                                break;
                            }
                            else
                            {
                                if (String2[0] == "+")
                                    ValueIntField = ValueIntField + ValueIntString2;
                                else if (String2[0] == "-")
                                    ValueIntField = ValueIntField - ValueIntString2;
                                else if (String2[0] == "*")
                                    ValueIntField = ValueIntField * ValueIntString2;
                                else if (String2[0] == "/")
                                    ValueIntField = ValueIntField / ValueIntString2;
                                else if (String2[0] == "<")
                                    if (ValueIntField <= ValueIntString2)
                                        ValueIntField = ValueIntString3;
                                    else
                                        ValueIntField = ValueIntString4;
                                else if (String2[0] == ">")
                                    if (ValueIntField >= ValueIntString2)
                                        ValueIntField = ValueIntString3;
                                    else
                                        ValueIntField = ValueIntString4;
                            }

                            Field = ValueIntField.ToString();
                        }
                        catch
                        {
                            // Intentionally ignored, safe to continue
                            // NOSONAR
                        }
                    }
                    else if (rowImportDefinition["DataType"].ToString().Contains("Datum") == true)
                    {
                        try
                        {
                            int ValueIntField = Convert.ToInt32(Convert.ToDecimal(Field));
                            int ValueIntString2 = Convert.ToInt32(String2[1]);
                            int ValueIntString3 = String2.Length > 2 ? Convert.ToInt32(String2[2]) : 0;
                            int ValueIntString4 = String2.Length > 3 ? Convert.ToInt32(String2[3]) : 0;

                            if (convertFrom > 0 && ValueIntField == convertFrom)
                            {
                                // int convertion found, so set Field and break for loop
                                Field = ValueIntString2.ToString();
                                break;
                            }
                            else
                            {
                                if (String2[0] == "+")
                                    ValueIntField = ValueIntField + ValueIntString2;
                                else if (String2[0] == "-")
                                    ValueIntField = ValueIntField - ValueIntString2;
                                else if (String2[0] == "*")
                                    ValueIntField = ValueIntField * ValueIntString2;
                                else if (String2[0] == "/")
                                    ValueIntField = ValueIntField / ValueIntString2;
                                else if (String2[0] == "<")
                                    if (ValueIntField <= ValueIntString2)
                                        ValueIntField = ValueIntString3;
                                    else
                                        ValueIntField = ValueIntString4;
                                else if (String2[0] == ">")
                                    if (ValueIntField >= ValueIntString2)
                                        ValueIntField = ValueIntString3;
                                    else
                                        ValueIntField = ValueIntString4;
                            }

                            Field = ValueIntField.ToString();
                        }
                        catch
                        {
                            // Intentionally ignored, safe to continue
                            // NOSONAR
                        }
                    }
                    else
                        if (rowImportDefinition["DataType"].ToString().Contains("Belopp") == true)
                    {
                        try
                        {
                            double ValueDoubleField = Convert.ToDouble(Field.Replace(".", ","));
                            double ValueDoubleString2 = Convert.ToDouble(String2[1]);
                            int ValueIntString2 = Convert.ToInt32(String2[1]);
                            if (String2[0] == "+")
                                ValueDoubleField = ValueDoubleField + ValueDoubleString2;
                            else
                                    if (String2[0] == "-")
                                ValueDoubleField = ValueDoubleField - ValueDoubleString2;
                            else
                                        if (String2[0] == "*")
                                ValueDoubleField = ValueDoubleField * ValueDoubleString2;
                            else
                                            if (String2[0] == "/")
                                ValueDoubleField = ValueDoubleField / ValueDoubleString2;
                            else
                                                if (String2[0] == "r" || String2[0] == "R")
                                ValueDoubleField = Math.Round(ValueDoubleField, ValueIntString2);
                            Field = ValueDoubleField.ToString().Replace(",", ".");
                        }
                        catch
                        {
                            // Intentionally ignored, safe to continue
                            // NOSONAR
                        }
                    }
                }
                else if (String2.Length > 1 && !string.IsNullOrEmpty(String2[1]))
                {
                    // Replace
                    Field = Field.Replace(String2[0].Replace("\"", string.Empty), String2[1].Replace("\"", string.Empty));
                }

                if (String2[0] == Field || String2[1] == Field)
                {
                    Field = String2[1];
                    break;
                }
                else if (String2.Length > 2)
                {

                    Field = String2[2];
                    break;
                }
            }
            return Field;
        }


        private void CreateRelations()
        {

            if (dsCommon.Tables["SysImportRelation"].Rows.Count > 0)
            {
                //DataRelation RelationImport;
                foreach (DataRow rowRelation in dsCommon.Tables["SysImportRelation"].Rows)
                {
                    if (dsImportTables.Tables[rowRelation["TableParent"].ToString()] == null || dsImportTables.Tables[rowRelation["TableChild"].ToString()] == null) continue;
                    if (dsImportTables.Tables[rowRelation["TableParent"].ToString()].Rows.Count == 0) continue;
                    if (dsImportTables.Tables[rowRelation["TableChild"].ToString()].Rows.Count == 0) continue;
                    if (dsImportTables.Tables[rowRelation["TableChild"].ToString()].Rows[0]["Parent_Id"].ToString() == "") continue;
                    RelationImport = new DataRelation(rowRelation["TableParent"].ToString() + "_" + rowRelation["TableChild"].ToString(),
                        dsImportTables.Tables[rowRelation["TableParent"].ToString()].Columns["My_Id"],
                        dsImportTables.Tables[rowRelation["TableChild"].ToString()].Columns["Parent_Id"], false);

                    if (RelationImport != null) RelationImport.Nested = true;

                    dsImportTables.Relations.Add(RelationImport);
                }
            }
            //måste byta namn även fast det inte finns några relationer
            foreach (DataTable tabImportTables in dsImportTables.Tables)
            {
                foreach (DataRow rowSysImporttSelect in dsCommon.Tables["SysImporttSelect"].Rows)
                {
                    if (rowSysImporttSelect["Level"].ToString() == tabImportTables.TableName)
                    {
                        tabImportTables.TableName = rowSysImporttSelect["Name"].ToString();
                        break;
                    }
                }
            }

            if (RelationImport != null) RelationImport.Nested = true;

        }

        //Skapar utdata för format = xml
        private void CreateTablesTypeXml(string ImportData)
        {

            DataSet dsTemp = new DataSet();
            DataRow drTemp;

            MemoryStream stream = new System.IO.MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(ImportData));
            try
            {
                dsImportXmlData.ReadXml(stream);
            }
            catch
            {
                try
                {
                    stream = new MemoryStream((new UTF8Encoding()).GetBytes(ImportData));
                    dsImportXmlData.ReadXml(stream);
                }
                catch
                {
                    var data = Encoding.UTF8.GetBytes(ImportData);
                    var result = Encoding.UTF8.GetPreamble().Concat(data).ToArray();

                    stream = new MemoryStream((new UTF8Encoding()).GetBytes(result.ToString()));

                    dsImportXmlData.ReadXml(stream);
                }

            }

            //Skapar tabeller i "dsImportXmlRelations" utifrån tabell-relationer i "dsImportXmlData" (ReadXml(stream))
            //Tabeller skapas för de tabeller i "dsImportXmlData" som har en parent-relation
            //För varje tabell som är Child-relation skapas en rad
            foreach (DataRelation relImportXmlData in dsImportXmlData.Relations)
            {
                int Char = relImportXmlData.RelationName.IndexOf("_");
                string ParentTable = relImportXmlData.RelationName.Substring(0, Char);
                if (dsImportXmlRelations.Tables[ParentTable] == null)
                {
                    dsImportXmlRelations.Tables.Add(relImportXmlData.RelationName.Substring(0, Char));
                    dsImportXmlRelations.Tables[relImportXmlData.RelationName.Substring(0, Char)].Columns.Add("Child");
                }
                DataRow drImportDataRelations = dsImportXmlRelations.Tables[ParentTable].NewRow();
                dsImportXmlRelations.Tables[ParentTable].Rows.Add(drImportDataRelations);
                drImportDataRelations["Child"] = relImportXmlData.RelationName.Replace(ParentTable + "_", "");
            }

            //Rader tas bort i dsImportXmlRelations för tabeller som har en rad child-relation till en tabell
            //som finns som egen tabell i dsImportXmlRelations 
            foreach (DataTable tabImportXmlRelations in dsImportXmlRelations.Tables)
                for (int i = tabImportXmlRelations.Rows.Count - 1; i > -1; i--)
                    if (dsImportXmlRelations.Tables[tabImportXmlRelations.Rows[i]["Child"].ToString()] != null)
                        tabImportXmlRelations.Rows[i].Delete();
            dsImportXmlRelations.AcceptChanges();


            foreach (DataTable tab in dsImportDefinitionColumns.Tables)
            {
                string TableName = "";
                foreach (DataRow row in tab.Rows)
                    if (row["Column"].ToString() == "XmlTag")
                        TableName = row["XmlTag"].ToString();
                dsTemp.Tables.Add(TableName);
                foreach (DataRow row in tab.Rows)
                {
                    if (row["Column"].ToString() == "XmlTag") continue;
                    dsTemp.Tables[TableName].Columns.Add(row["XmlTag"].ToString());
                }
                tab.TableName = TableName;
            }

            //Utförs för varje definition
            foreach (DataTable tab in dsTemp.Tables)
            {

                if (dsImportXmlData.Tables[tab.TableName] == null)
                    continue;

                //Lägg till kolumner för Id-begrepp
                foreach (DataColumn colImportData in dsImportXmlData.Tables[tab.TableName].Columns)
                {
                    if (colImportData.ColumnName.Contains("_Id") == true)
                    {
                        if (colImportData.ColumnName == tab.TableName + "_Id")
                        {
                            tab.Columns.Add("My_Id");
                            colImportData.ColumnName = "My_Id";
                        }
                        else
                        {
                            tab.Columns.Add("Parent_Id");
                            colImportData.ColumnName = "Parent_Id";
                        }
                    }
                }

                //Utförs för varje importerad rad per definition
                foreach (DataRow rowImportData in dsImportXmlData.Tables[tab.TableName].Rows)
                {
                    //Lägg till en tom rad för aktuell definition
                    drTemp = tab.NewRow();
                    tab.Rows.Add(drTemp);
                    //Flytta kolumndata från import till aktuell definition
                    foreach (DataColumn col in tab.Columns)
                    {
                        if (dsImportXmlData.Tables[tab.TableName].Columns[col.ColumnName] != null)
                            drTemp[col.ColumnName] = rowImportData[col.ColumnName];
                    }
                    if (dsImportXmlRelations.Tables[tab.TableName] == null) continue;
                    foreach (DataRow rowRelation_1 in dsImportXmlRelations.Tables[tab.TableName].Rows)
                    {
                        DataRow[] arrRowsLevel2 = rowImportData.GetChildRows(tab.TableName + "_" + rowRelation_1["Child"].ToString());
                        string arrRowsTable2 = rowRelation_1["Child"].ToString();
                        for (int idxRowsLevel2 = 0; idxRowsLevel2 < arrRowsLevel2.Length; idxRowsLevel2++)
                        {
                            foreach (DataColumn col in dsImportXmlData.Tables[rowRelation_1["Child"].ToString()].Columns)
                                if (dsTemp.Tables[tab.TableName].Columns[col.ColumnName] != null && drTemp[col.ColumnName].ToString() == string.Empty)
                                    drTemp[col.ColumnName] = arrRowsLevel2[idxRowsLevel2][col.ColumnName];
                        }
                        if (dsImportXmlRelations.Tables[rowRelation_1["Child"].ToString()] == null) continue;
                        foreach (DataRow rowRelation_2 in dsImportXmlRelations.Tables[rowRelation_1["Child"].ToString()].Rows)
                        {
                            DataRow[] arrRowsLevel3 = rowImportData.GetChildRows(rowRelation_1["Child"].ToString() + "_" + rowRelation_2["Child"].ToString());
                            string arrRowsTable3 = rowRelation_2["Child"].ToString();
                            for (int idxRowsLevel3 = 0; idxRowsLevel3 < arrRowsLevel3.Length; idxRowsLevel3++)
                            {
                                foreach (DataColumn col in dsImportXmlData.Tables[rowRelation_2["Child"].ToString()].Columns)
                                    if (dsTemp.Tables[tab.TableName].Columns[col.ColumnName] != null && drTemp[col.ColumnName].ToString() == string.Empty)
                                        drTemp[col.ColumnName] = arrRowsLevel3[idxRowsLevel3][col.ColumnName];
                            }
                        }
                    }
                }
            }

            foreach (DataTable tab in dsTemp.Tables)
            {
                foreach (DataRow rowImportDefinitionColumns in dsImportDefinitionColumns.Tables[tab.TableName].Rows)
                {
                    if (rowImportDefinitionColumns["Column"].ToString() == "XmlTag") continue;
                    if (rowImportDefinitionColumns["Column"].ToString() == "RecordType") continue;
                    if (!tab.Columns.Contains(rowImportDefinitionColumns["Column"].ToString()) && tab.Rows.Count > 0)
                    {
                        tab.Columns[rowImportDefinitionColumns["XmlTag"].ToString()].ColumnName = rowImportDefinitionColumns["Column"].ToString();
                    }
                    if (rowImportDefinitionColumns["Standard"].ToString() != "")
                    {
                        foreach (DataRow row in tab.Rows)
                        {
                            if (row[rowImportDefinitionColumns["Column"].ToString()].ToString() == "")
                                row[rowImportDefinitionColumns["Column"].ToString()] = rowImportDefinitionColumns["Standard"].ToString();
                        }
                    }
                    if (rowImportDefinitionColumns["Convert"].ToString() != "")
                    {
                        foreach (DataRow row in tab.Rows)
                        {
                            if (row[rowImportDefinitionColumns["Column"].ToString()].ToString() != "")
                                row[rowImportDefinitionColumns["Column"].ToString()] = ConvertField(rowImportDefinitionColumns, row[rowImportDefinitionColumns["Column"].ToString()].ToString());
                        }
                    }

                }
                dsImportTables.Tables.Add(tab.Copy());
                dsImportTables.Tables[tab.TableName].TableName = dsImportDefinitionColumns.Tables[tab.TableName].Rows[0]["Level"].ToString();
            }

        }

        //Skapar utdata för format = sparator
        private void CreateTablesTypeSeparator(string ImportData)
        {
            DataSet dsTemp = new DataSet();
            DataRow drTemp;

            int RecordType_Count = 0;
            int RecordType_ColumnField = -1;
            string[] RecordType_Level = new string[dsCompany.Tables["ImportDefinitionLevel"].Rows.Count + 1];
            int[] Parent_Level = new int[dsCompany.Tables["ImportDefinitionLevel"].Rows.Count + 1];
            int[] My_Id = new int[dsCompany.Tables["ImportDefinitionLevel"].Rows.Count + 1];
            int[] Position_Count = new int[dsCompany.Tables["ImportDefinitionLevel"].Rows.Count + 1];
            char[] BlankTkn = { ' ' };

            for (int i = 1; i <= dsCompany.Tables["ImportDefinitionLevel"].Rows.Count; i++)
            {
                if (dsImportDefinitionColumns.Tables[i.ToString()] == null)
                    continue;

                foreach (DataRow rowImportDefinitionColumns in dsImportDefinitionColumns.Tables[i.ToString()].Rows)
                {
                    if (rowImportDefinitionColumns["Position"].ToString() != "" && Convert.ToInt32(rowImportDefinitionColumns["Position"]) > Position_Count[i])
                        Position_Count[i] = Convert.ToInt32(rowImportDefinitionColumns["Position"]);
                    if (rowImportDefinitionColumns["Column"].ToString() == "RecordType")
                    {
                        RecordType_Count++;
                        if (rowImportDefinitionColumns["Standard"].ToString() != "")
                            RecordType_Level[i] = rowImportDefinitionColumns["Standard"].ToString();
                        foreach (DataRow SysImportRelation in dsCommon.Tables["SysImportRelation"].Rows)
                        {
                            if (SysImportRelation["TableChild"].ToString() == dsCompany.Tables["ImportDefinitionLevel"].Rows[i - 1]["Level"].ToString())
                                Parent_Level[i] = Convert.ToInt32(SysImportRelation["TableParent"]);
                        }

                    }
                }
            }

            foreach (DataRow row in dsCompany.Tables["ImportDefinitionLevel"].Rows)
            {
                if (dsImportDefinitionColumns.Tables[row["Level"].ToString()] == null)
                    continue;

                dsTemp.Tables.Add(row["Level"].ToString());
                foreach (DataRow rowColumns in dsImportDefinitionColumns.Tables[row["Level"].ToString()].Rows)
                {
                    if (rowColumns["Column"].ToString() == "RecordType")
                        RecordType_ColumnField = Convert.ToInt32(rowColumns["Field"]);
                    dsTemp.Tables[row["Level"].ToString()].Columns.Add(rowColumns["Field"].ToString());
                }
                dsTemp.Tables[row["Level"].ToString()].Columns.Add("My_Id");
                dsTemp.Tables[row["Level"].ToString()].Columns.Add("Parent_Id");
            }

            byte[] byteArray = Encoding.Default.GetBytes(ImportData);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);
            char[] Delimiter = { Convert.ToChar(dsCompany.Tables["ImportDefinition"].Rows[0]["Separator"]) };
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                string[] InputRow = line.Split(Delimiter);
                string Level = "1";
                if (RecordType_Count > 0)
                {
                    for (int i = 1; i < RecordType_Level.Length; i++)
                    {
                        if (InputRow[RecordType_ColumnField] == RecordType_Level[i])
                        {
                            Level = i.ToString();
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(InputRow[RecordType_ColumnField].ToString()) && RecordType_Level[int.Parse(Level)] != InputRow[RecordType_ColumnField])
                        continue;
                }

                drTemp = dsTemp.Tables[Level].NewRow();
                dsTemp.Tables[Level].Rows.Add(drTemp);
                foreach (DataRow rowImportDefinitionColumns in dsImportDefinitionColumns.Tables[Level].Rows)
                {
                    if (rowImportDefinitionColumns["Position"].ToString() != "" && Convert.ToInt32(rowImportDefinitionColumns["Position"]) - 1 < InputRow.Length)
                        drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])] = InputRow[Convert.ToInt32(rowImportDefinitionColumns["Position"]) - 1].TrimStart(BlankTkn);
                    if (drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])].ToString() == "" && rowImportDefinitionColumns["Standard"].ToString() != "")
                        drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])] = rowImportDefinitionColumns["Standard"].ToString();
                    drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])] = RemoveSpace(drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])].ToString());
                    if (rowImportDefinitionColumns["Convert"].ToString() != "" && drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])].ToString() != "")
                        drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])] = ConvertField(rowImportDefinitionColumns, drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])].ToString());
                }
                drTemp["My_Id"] = My_Id[Convert.ToInt32(Level)]++;
                if (RecordType_Count > 0 && Parent_Level[Convert.ToInt32(Level)] > 0)
                    drTemp["Parent_Id"] = My_Id[Parent_Level[Convert.ToInt32(Level)]] - 1;
            }

            if (RecordType_Count > 0)
                foreach (DataTable tab in dsTemp.Tables)
                    tab.Columns.Remove(RecordType_ColumnField.ToString());

            foreach (DataTable tab in dsTemp.Tables)
            {
                foreach (DataRow rowImportDefinitionColumns in dsImportDefinitionColumns.Tables[tab.TableName].Rows)
                {
                    if (rowImportDefinitionColumns["Column"].ToString() == "XmlTag") continue;
                    if (rowImportDefinitionColumns["Column"].ToString() == "RecordType") continue;
                    if (!tab.Columns.Contains(rowImportDefinitionColumns["Column"].ToString()))
                    {
                        tab.Columns[rowImportDefinitionColumns["Field"].ToString()].ColumnName = rowImportDefinitionColumns["Column"].ToString();
                    }
                    else if (rowImportDefinitionColumns["DataType"].ToString() == "Belopp")
                    {
                        // TODO if amount the sum the values
                        throw new Exception("Field already exists: " + tab.Columns[rowImportDefinitionColumns["Field"].ToString()].ColumnName);
                    }
                    else
                    {
                        throw new Exception("Field already exists: " + tab.Columns[rowImportDefinitionColumns["Field"].ToString()].ColumnName);
                    }
                }
                dsImportTables.Tables.Add(tab.Copy());
            }

        }

        //Skapar utdata för format = kolumner
        private void CreateTablesTypeColumns(string ImportData)
        {
            DataSet dsTemp = new DataSet();
            DataRow drTemp;

            int RecordType_Count = 0;
            int RecordType_ColumnField = -1;
            int RecordType_From = 0;
            int RecordType_Characters = 0;
            string[] RecordType_Level = new string[dsCompany.Tables["ImportDefinitionLevel"].Rows.Count + 1];
            int[] Parent_Level = new int[dsCompany.Tables["ImportDefinitionLevel"].Rows.Count + 1];
            int[] My_Id = new int[dsCompany.Tables["ImportDefinitionLevel"].Rows.Count + 1];

            for (int i = 1; i <= dsCompany.Tables["ImportDefinitionLevel"].Rows.Count; i++)
            {
                if (dsImportDefinitionColumns.Tables.Count < i)
                    continue;

                foreach (DataRow rowImportDefinitionColumns in dsImportDefinitionColumns.Tables[i.ToString()].Rows)
                {
                    if (rowImportDefinitionColumns["Column"].ToString() == "RecordType")
                    {
                        RecordType_Count++;
                        if (rowImportDefinitionColumns["Standard"].ToString() != "")
                            RecordType_Level[i] = rowImportDefinitionColumns["Standard"].ToString();
                        if (i == 1)
                        {
                            RecordType_From = Convert.ToInt32(rowImportDefinitionColumns["From"]);
                            RecordType_Characters = Convert.ToInt32(rowImportDefinitionColumns["Characters"]);
                        }
                        foreach (DataRow SysImportRelation in dsCommon.Tables["SysImportRelation"].Rows)
                        {
                            if (SysImportRelation["TableChild"].ToString() == dsCompany.Tables["ImportDefinitionLevel"].Rows[i - 1]["Level"].ToString())
                                Parent_Level[i] = Convert.ToInt32(SysImportRelation["TableParent"]);
                        }
                    }
                }
            }

            foreach (DataRow row in dsCompany.Tables["ImportDefinitionLevel"].Rows)
            {
                if (int.Parse(row["level"].ToString()) > dsImportDefinitionColumns.Tables.Count)
                    continue;

                dsTemp.Tables.Add(row["Level"].ToString());
                foreach (DataRow rowColumns in dsImportDefinitionColumns.Tables[row["Level"].ToString()].Rows)
                {
                    if (rowColumns["Column"].ToString() == "RecordType")
                        RecordType_ColumnField = Convert.ToInt32(rowColumns["Field"]);
                    dsTemp.Tables[row["Level"].ToString()].Columns.Add(rowColumns["Field"].ToString());
                }
                dsTemp.Tables[row["Level"].ToString()].Columns.Add("My_Id");
                dsTemp.Tables[row["Level"].ToString()].Columns.Add("Parent_Id");
            }

            byte[] byteArray = Encoding.Default.GetBytes(ImportData);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                string Level = "1";
                if (RecordType_Count > 0)
                {
                    for (int i = 1; i < RecordType_Level.Length; i++)
                    {
                        if (line.Substring(RecordType_From - 1, RecordType_Characters) == RecordType_Level[i])
                        {
                            Level = i.ToString();
                            break;
                        }
                    }
                }

                if (dsTemp.Tables[Level] == null)
                    continue;

                drTemp = dsTemp.Tables[Level].NewRow();
                dsTemp.Tables[Level].Rows.Add(drTemp);
                foreach (DataRow rowImportDefinitionColumns in dsImportDefinitionColumns.Tables[Level].Rows)
                {
                    if (rowImportDefinitionColumns["From"].ToString() != "" || rowImportDefinitionColumns["Characters"].ToString() != "")
                    {
                        if (line.Length < Convert.ToInt32(rowImportDefinitionColumns["From"])) continue;
                        int length = Convert.ToInt32(rowImportDefinitionColumns["From"]) + Convert.ToInt32(rowImportDefinitionColumns["Characters"]);
                        if (rowImportDefinitionColumns["From"].ToString() != "")
                            if (length <= line.Length)
                                drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])] = line.Substring(Convert.ToInt32(rowImportDefinitionColumns["From"]) - 1, Convert.ToInt32(rowImportDefinitionColumns["Characters"]));
                            else
                                drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])] = line.Substring(Convert.ToInt32(rowImportDefinitionColumns["From"]) - 1);
                    }
                    if (drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])].ToString() == "" && rowImportDefinitionColumns["Standard"].ToString() != "")
                        drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])] = rowImportDefinitionColumns["Standard"].ToString();
                    drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])] = RemoveSpace(drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])].ToString());
                    if (rowImportDefinitionColumns["Convert"].ToString() != "")
                        drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])] = ConvertField(rowImportDefinitionColumns, drTemp[Convert.ToInt32(rowImportDefinitionColumns["Field"])].ToString());
                }
                drTemp["My_Id"] = My_Id[Convert.ToInt32(Level)]++;
                if (RecordType_Count > 0 && Parent_Level[Convert.ToInt32(Level)] > 0)
                    drTemp["Parent_Id"] = My_Id[Parent_Level[Convert.ToInt32(Level)]] - 1;
            }

            if (RecordType_Count > 0)
                foreach (DataTable tab in dsTemp.Tables)
                    tab.Columns.Remove(RecordType_ColumnField.ToString());

            foreach (DataTable tab in dsTemp.Tables)
            {
                foreach (DataRow rowImportDefinitionColumns in dsImportDefinitionColumns.Tables[tab.TableName].Rows)
                {
                    if (rowImportDefinitionColumns["Column"].ToString() == "XmlTag") continue;
                    if (rowImportDefinitionColumns["Column"].ToString() == "RecordType") continue;
                    tab.Columns[rowImportDefinitionColumns["Field"].ToString()].ColumnName = rowImportDefinitionColumns["Column"].ToString();
                }
                dsImportTables.Tables.Add(tab.Copy());
            }

        }

        public string RemoveSpace(string refField)
        {
            int i = 0;
            for (i = refField.Length; i != 0 && refField.Substring(i - 1, 1) == " "; i--) { }
            string Field = null;
            if (i != 0) { return Field = refField.Substring(0, i); }
            else
            { return Field; }
        }
    }
}
