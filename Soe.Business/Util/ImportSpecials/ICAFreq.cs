using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class ICAFreq : ImportExportManager
    {
        public ICAFreq(ParameterObject parameterObject) : base(parameterObject) { }

        public List<dynamic> GetStaffingNeedsFrequencyIODTOs(string content)
        {
            List<dynamic> dtos = new List<dynamic>();
            var files = CreateFiles(content);

            if (files.Count > 1)
            {
                ConcurrentBag<StaffingNeedsFrequencyIODTO> bag = new ConcurrentBag<StaffingNeedsFrequencyIODTO>();

                Parallel.ForEach(files, file =>
                {
                    var parsed = ParseToStaffingNeedsFrequencyIODTO(file);

                    foreach (var item in parsed)
                        bag.Add(item);
                });

                dtos.AddRange(bag.ToList());
            }
            else if (files.Count == 1)
            {
                dtos = ParseToStaffingNeedsFrequencyIODTO(files.FirstOrDefault());
            }


            TryDeleteFiles(files);

            return dtos;

        }

        public void TryDeleteFiles(List<string> files)
        {
            foreach (var item in files)
            {
                try
                {
                    if (File.Exists(item))
                        File.Delete(item);
                }
                catch
                {
                    // Do nothing
                    // NOSONAR
                }
            }
        }

        /// <summary>
        /// Create temp File on Disk to avoid outOfMemoryException
        /// </summary>
        private List<string> CreateFiles(string content)
        {
            List<string> files = new List<string>();
            Guid guid = Guid.NewGuid();
            string path = ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + guid.ToString();

            File.WriteAllText(path, content);

            StreamWriter writer = null;
            try
            {
                using (var inputfile = new System.IO.StreamReader(path))
                {
                    string line;
                    int count = 0;
                    int fileCount = 1;
                    while ((line = inputfile.ReadLine()) != null)
                    {
                        if (writer == null || count > 100000)
                        {
                            if (writer != null)
                            {
                                writer.Close();
                                writer = null;
                            }

                            string splitPath = path + "_" + fileCount.ToString() + ".ica";
                            writer = new StreamWriter(splitPath, true);
                            files.Add(splitPath);
                            fileCount++;
                            count = 0;
                        }

                        writer.WriteLine(line.ToLower());

                        ++count;
                    }
                }
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            return files;
        }

        private List<dynamic> ParseToStaffingNeedsFrequencyIODTO(string file)
        {
            List<dynamic> dtos = new List<dynamic>();
            int interval = 15;

            using (FileStream fs = File.Open(file, FileMode.Open))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader sr = new StreamReader(bs))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.ToUpper().Contains("BU_CODE;REF_NAME;DATE;VALUE"))
                                continue;

                            if (line == "") continue;
                            string[] inputRow = line.Split(';');

                            StaffingNeedsFrequencyIODTO staffingNeedsFrequencyIODTO = new StaffingNeedsFrequencyIODTO();

                            staffingNeedsFrequencyIODTO.DateFrom = inputRow[2].ToString().Replace("t", " ");
                            staffingNeedsFrequencyIODTO.TimeFrom = inputRow[2].ToString().Replace("t", " ");
                            staffingNeedsFrequencyIODTO.NbrOfItems = Convert.ToDecimal(inputRow[3].ToString().Replace(".", ","));
                            staffingNeedsFrequencyIODTO.ExternalCode = inputRow[1].ToString();
                            staffingNeedsFrequencyIODTO.DateTo = CalendarUtility.GetDateTime(staffingNeedsFrequencyIODTO.DateFrom).AddMinutes(interval).ToString();
                            staffingNeedsFrequencyIODTO.TimeTo = CalendarUtility.GetDateTime(staffingNeedsFrequencyIODTO.TimeFrom).AddMinutes(interval).ToString();
                            staffingNeedsFrequencyIODTO.FrequencyType = FrequencyType.Actual;

                            if (staffingNeedsFrequencyIODTO.ExternalCode.ToLower().StartsWith("sales_department_"))
                            {
                                staffingNeedsFrequencyIODTO.NbrOfCustomers = 0;
                                staffingNeedsFrequencyIODTO.Amount = staffingNeedsFrequencyIODTO.NbrOfItems;
                                staffingNeedsFrequencyIODTO.NbrOfItems = 0;
                            }
                            else if (staffingNeedsFrequencyIODTO.ExternalCode.ToLower().Contains("items_department_"))
                            {
                                staffingNeedsFrequencyIODTO.NbrOfItems = 0;
                            }
                            else if (staffingNeedsFrequencyIODTO.ExternalCode.ToLower().Contains("transactions_cash_desk"))
                            {
                                staffingNeedsFrequencyIODTO.NbrOfCustomers = staffingNeedsFrequencyIODTO.NbrOfItems;
                                staffingNeedsFrequencyIODTO.NbrOfItems = 0;
                            }
                            else if (staffingNeedsFrequencyIODTO.ExternalCode.ToLower().Contains("sales_cash_desk"))
                            {
                                staffingNeedsFrequencyIODTO.Amount = staffingNeedsFrequencyIODTO.NbrOfItems;
                                staffingNeedsFrequencyIODTO.NbrOfItems = 0;
                            }

                            if (staffingNeedsFrequencyIODTO.DateFrom.Contains("2"))  //Simple check to filter out headers. DateFrom is Mandatory
                                dtos.Add(staffingNeedsFrequencyIODTO);
                        }
                    }
                }
            }

            return dtos;
        }
    }
}
