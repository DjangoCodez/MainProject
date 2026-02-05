using Newtonsoft.Json;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.PaymentIO.SEPAV3;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Soe.TestDataGenerator.Scripts
{
    /// <summary>
    /// Generates test data files for SEPAV3 unit tests by running the actual ConvertSupplierStreamToEntity
    /// against a real database and capturing the results.
    ///
    /// Prerequisites:
    ///   - The payment data in the database must not already be imported (the payments should be in their original state).
    ///   - For interactive mode, place XML files in: bin\Debug\Scripts\ConvertSupplierStreamToEntity\InputData\
    ///
    /// Usage:
    ///   Interactive: Soe.TestDataGenerator.exe ConvertSupplierStreamToEntity
    ///   CLI: Soe.TestDataGenerator.exe ConvertSupplierStreamToEntity &lt;xml-file&gt; &lt;database-name&gt; &lt;actorCompanyId&gt;
    ///
    /// Output files (in bin\Debug\Output\ConvertSupplierStreamToEntity\{xml-filename}\):
    ///   - input.json: Test input parameters (actorCompanyId, importType, etc.)
    ///   - mockdata.json: PaymentRows from database with anonymized actor/invoice IDs
    ///   - expected.json: Expected PaymentImports result with anonymized IDs
    ///   - {original}.xml: XML file with supplier names and invoice numbers replaced with DB values
    /// </summary>
    public class ConvertSupplierStreamToEntity : ITestDataScript
    {
        public string Name => "ConvertSupplierStreamToEntity";

        public string Description => "Generate SEPAV3 test data from XML payment files using ConvertSupplierStreamToEntity";

        public int Run(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  SEPAV3 Test Data Generator");
            Console.WriteLine("  ConvertSupplierStreamToEntity");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            try
            {
                string xmlFilePath;
                string databaseName;
                int actorCompanyId;

                if (args.Length >= 3)
                {
                    xmlFilePath = args[0];
                    databaseName = args[1];
                    if (!int.TryParse(args[2], out actorCompanyId))
                    {
                        Console.WriteLine($"Error: Invalid actor company ID: {args[2]}");
                        return 1;
                    }
                }
                else if (args.Length > 0)
                {
                    Console.WriteLine("Usage: Soe.TestDataGenerator.exe ConvertSupplierStreamToEntity <xml-file> <database-name> <actorCompanyId>");
                    Console.WriteLine("   Or run without arguments for interactive mode.");
                    return 1;
                }
                else
                {
                    xmlFilePath = SelectXmlFile();
                    if (string.IsNullOrEmpty(xmlFilePath))
                    {
                        Console.WriteLine("No file selected. Exiting.");
                        return 1;
                    }

                    databaseName = PromptForDatabase();
                    if (string.IsNullOrEmpty(databaseName))
                    {
                        Console.WriteLine("Database name is required. Exiting.");
                        return 1;
                    }

                    actorCompanyId = PromptForActorCompanyId();
                }

                if (!File.Exists(xmlFilePath))
                {
                    Console.WriteLine($"Error: File not found: {xmlFilePath}");
                    return 1;
                }

                var outputFolder = GetOutputFolder(xmlFilePath);
                Console.WriteLine();
                Console.WriteLine($"XML File: {xmlFilePath}");
                Console.WriteLine($"Database: {databaseName}");
                Console.WriteLine($"Actor Company ID: {actorCompanyId}");
                Console.WriteLine($"Output Folder: {outputFolder}");
                Console.WriteLine();

                GenerateTestData(xmlFilePath, databaseName, actorCompanyId, outputFolder);
                Console.WriteLine();
                Console.WriteLine("Test data generation completed successfully!");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private string SelectXmlFile()
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var inputDataDir = Path.Combine(exeDir, "Scripts", Name, "InputData");

            if (!Directory.Exists(inputDataDir))
            {
                Console.WriteLine($"Input data directory not found: {inputDataDir}");
                Console.WriteLine();
                Console.WriteLine("Please create this directory and add XML files to it.");
                return null;
            }

            var xmlFiles = Directory.GetFiles(inputDataDir, "*.xml")
                .OrderBy(f => f)
                .ToList();

            if (xmlFiles.Count == 0)
            {
                Console.WriteLine($"No XML files found in: {inputDataDir}");
                return null;
            }

            Console.WriteLine("Available XML files:");
            for (int i = 0; i < xmlFiles.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {Path.GetFileName(xmlFiles[i])}");
                Console.WriteLine($"     ({xmlFiles[i]})");
            }
            Console.WriteLine();

            while (true)
            {
                Console.Write($"Select file (1-{xmlFiles.Count}): ");
                var input = Console.ReadLine();
                if (int.TryParse(input, out int selection) && selection >= 1 && selection <= xmlFiles.Count)
                {
                    return xmlFiles[selection - 1];
                }
                Console.WriteLine("Invalid selection. Please try again.");
            }
        }

        private string PromptForDatabase()
        {
            Console.Write("Database name (e.g., soeaborademo): ");
            return Console.ReadLine()?.Trim();
        }

        private int PromptForActorCompanyId()
        {
            while (true)
            {
                Console.Write("Actor Company ID: ");
                var input = Console.ReadLine()?.Trim();
                if (int.TryParse(input, out int value))
                {
                    return value;
                }
                Console.WriteLine("Invalid input. Please enter a valid integer.");
            }
        }

        private string SanitizeDirectoryName(string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var invalidChars = new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*', ' ' };
            foreach (var c in invalidChars)
            {
                name = name.Replace(c, '_');
            }
            return name.Trim().TrimEnd('.');
        }

        private string GetOutputFolder(string xmlFilePath)
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var folderName = SanitizeDirectoryName(Path.GetFileName(xmlFilePath));
            return Path.Combine(exeDir, "Output", Name, folderName);
        }

        private void GenerateTestData(string xmlFilePath, string databaseName, int actorCompanyId, string outputFolder)
        {
            Console.WriteLine("Reading XML file...");
            var xmlContent = File.ReadAllText(xmlFilePath);

            var connectionStringTemplate = ConfigurationManager.ConnectionStrings["SOECompEntities"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionStringTemplate))
            {
                throw new ConfigurationErrorsException("SOECompEntities connection string not found in App.config");
            }
            var connectionString = connectionStringTemplate.Replace("{DATABASE}", databaseName);

            Console.WriteLine("Connecting to database...");
            using (var entities = new CompEntities(connectionString))
            {
                Console.WriteLine("Parsing XML with SEPAV3Manager...");
                var sepaManager = new SEPAV3Manager(null);
                var files = sepaManager.ParseCAMTFile(xmlContent, ImportPaymentType.SupplierPayment);
                Console.WriteLine($"Found {files.Count} payment entries in XML");

                var paymentIds = ExtractPaymentIds(files);
                Console.WriteLine($"Found {paymentIds.Count} unique payment IDs");

                Console.WriteLine("Querying PaymentRows from database...");
                var paramObj = ParameterObject.Empty().Clone(actorCompanyId: actorCompanyId);
                var paymentManager = new PaymentManager(paramObj);
                var settingManager = new SettingManager(paramObj);

                var paymentRows = new List<PaymentRowInvoiceDTO>();
                foreach (var paymentId in paymentIds)
                {
                    var rows = paymentManager.GetPaymentRowsWithSupplierInvoice(entities, paymentId, actorCompanyId);
                    paymentRows.AddRange(rows);
                }
                Console.WriteLine($"Found {paymentRows.Count} PaymentRows in database");

                bool autoTransferAutogiro = settingManager.GetCompanyBoolSetting(CompanySettingType.SupplierInvoiceAutoTransferAutogiroPaymentsToVoucher);

                var dataAccess = new SupplierPaymentDataAccess(entities, paymentManager, settingManager);

                Console.WriteLine("Running ConvertSupplierStreamToEntity...");
                var logText = new List<string>();
                var conversionResult = sepaManager.ConvertSupplierStreamToEntity(
                    files: files,
                    actorCompanyId: actorCompanyId,
                    paymentOriginType: SoeOriginType.SupplierPayment,
                    logText: ref logText,
                    paymentImportId: 1,
                    batchId: 100,
                    importType: ImportPaymentType.SupplierPayment,
                    dataAccess: dataAccess,
                    warningLogger: msg => logText.Add($"[WARNING] {msg}")
                );

                Console.WriteLine($"Result: Success={conversionResult.Result.Success}");
                if (!string.IsNullOrEmpty(conversionResult.Result.ErrorMessage))
                {
                    Console.WriteLine($"Error: {conversionResult.Result.ErrorMessage}");
                }

                Directory.CreateDirectory(outputFolder);

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include
                };

                Console.WriteLine("Writing input.json...");
                var inputData = new
                {
                    ActorCompanyId = actorCompanyId,
                    PaymentOriginType = "SupplierPayment",
                    PaymentImportId = 1,
                    BatchId = 100,
                    ImportType = "SupplierPayment",
                    XmlFile = Path.GetFileName(xmlFilePath)
                };
                File.WriteAllText(
                    Path.Combine(outputFolder, "input.json"),
                    JsonConvert.SerializeObject(inputData, jsonSettings));

                Console.WriteLine("Writing mockdata.json...");
                var orderedPaymentRows = paymentRows.OrderBy(r => r.PaymentRowId).ToList();

                var xmlDataByPaymentRowId = new Dictionary<int, (string Name, string InvoiceNr)>();
                foreach (var file in files)
                {
                    if (!string.IsNullOrEmpty(file.EndToEndId))
                    {
                        var parts = file.EndToEndId.Split(',');
                        if (parts.Length >= 1 && int.TryParse(parts[0], out int paymentRowId))
                        {
                            xmlDataByPaymentRowId[paymentRowId] = (file.Name ?? "", file.InvoiceNr ?? "");
                        }
                    }
                }

                var actorIdMapping = new Dictionary<int, int>();
                var actorNameMapping = new Dictionary<int, string>();
                var xmlNameToDbName = new Dictionary<string, string>();
                var xmlInvoiceNrToDbInvoiceNr = new Dictionary<string, string>();
                var invoiceIdMapping = new Dictionary<int, int>();

                var actorIdCounter = 100;
                for (int i = 0; i < orderedPaymentRows.Count; i++)
                {
                    var row = orderedPaymentRows[i];
                    var actorId = row.InvoiceActorId ?? 0;

                    if (!actorIdMapping.ContainsKey(actorId))
                    {
                        actorIdMapping[actorId] = ++actorIdCounter;
                        actorNameMapping[actorId] = row.InvoiceActorName;
                    }

                    if (xmlDataByPaymentRowId.TryGetValue(row.PaymentRowId, out var xmlData))
                    {
                        if (!string.IsNullOrEmpty(xmlData.Name) && !xmlNameToDbName.ContainsKey(xmlData.Name))
                        {
                            xmlNameToDbName[xmlData.Name] = row.InvoiceActorName ?? "";
                        }
                        if (!string.IsNullOrEmpty(xmlData.InvoiceNr) && !xmlInvoiceNrToDbInvoiceNr.ContainsKey(xmlData.InvoiceNr))
                        {
                            xmlInvoiceNrToDbInvoiceNr[xmlData.InvoiceNr] = row.InvoiceNr ?? "";
                        }
                    }

                    var originalInvoiceId = row.InvoiceId;
                    if (!invoiceIdMapping.ContainsKey(originalInvoiceId))
                    {
                        invoiceIdMapping[originalInvoiceId] = 1001 + invoiceIdMapping.Count;
                    }
                }

                var mockData = new
                {
                    PaymentRows = orderedPaymentRows.Select(r => new
                    {
                        r.PaymentRowId,
                        r.PaymentId,
                        InvoiceId = invoiceIdMapping.TryGetValue(r.InvoiceId, out var anonInvId) ? anonInvId : 0,
                        r.InvoiceNr,
                        r.InvoiceTotalAmount,
                        r.InvoicePaidAmount,
                        r.FullyPayed,
                        r.Status,
                        InvoiceActorId = actorIdMapping[r.InvoiceActorId ?? 0],
                        InvoiceActorName = actorNameMapping[r.InvoiceActorId ?? 0],
                        r.BillingType,
                        r.Amount,
                        r.AmountCurrency,
                        r.InvoiceDate,
                        r.InvoiceDueDate,
                        PayDate = (DateTime?)r.PayDate,
                        r.PaymentNr,
                        r.InvoiceSeqNr,
                        SysPaymentTypeId = (int?)r.SysPaymentTypeId
                    }).ToList(),
                    Settings = new
                    {
                        AutoTransferAutogiro = autoTransferAutogiro
                    }
                };
                File.WriteAllText(Path.Combine(outputFolder, "mockdata.json"), JsonConvert.SerializeObject(mockData, jsonSettings));

                Console.WriteLine("Writing expected.json...");
                var capturedImports = conversionResult.PaymentImports ?? new List<PaymentImportIO>();
                var expectedData = new
                {
                    Success = conversionResult.Result.Success,
                    PaymentImports = capturedImports.Select(i => new
                    {
                        i.Status,
                        i.State,
                        InvoiceId = i.InvoiceId.HasValue && i.InvoiceId.Value > 0 && invoiceIdMapping.TryGetValue(i.InvoiceId.Value, out var anonInvId)
                            ? (int?)anonInvId
                            : i.InvoiceId,
                        i.InvoiceNr,
                        CustomerId = i.CustomerId.HasValue && actorIdMapping.TryGetValue(i.CustomerId.Value, out var anonCustId)
                            ? (int?)anonCustId
                            : i.CustomerId,
                        Customer = i.CustomerId.HasValue && actorNameMapping.TryGetValue(i.CustomerId.Value, out var custName) ? custName : i.Customer,
                        i.PaidAmount,
                        i.InvoiceAmount
                    }).ToList()
                };
                File.WriteAllText(Path.Combine(outputFolder, "expected.json"), JsonConvert.SerializeObject(expectedData, jsonSettings));

                Console.WriteLine("Creating modified XML file with DB values...");

                var xdoc = XDocument.Parse(xmlContent);
                var ns = xdoc.Root.GetDefaultNamespace();

                foreach (var nmElement in xdoc.Descendants(ns + "Nm"))
                {
                    var originalName = nmElement.Value;
                    if (xmlNameToDbName.TryGetValue(originalName, out var dbName))
                    {
                        nmElement.Value = dbName;
                    }
                }

                foreach (var kvp in xmlInvoiceNrToDbInvoiceNr)
                {
                    if (string.IsNullOrEmpty(kvp.Key)) continue;

                    foreach (var nbElement in xdoc.Descendants(ns + "Nb").Where(e => e.Value == kvp.Key))
                    {
                        nbElement.Value = kvp.Value;
                    }

                    foreach (var refElement in xdoc.Descendants(ns + "Ref").Where(e => e.Value == kvp.Key))
                    {
                        refElement.Value = kvp.Value;
                    }
                }

                var xmlDestPath = Path.Combine(outputFolder, Path.GetFileName(xmlFilePath));
                xdoc.Save(xmlDestPath);

                Console.WriteLine();
                Console.WriteLine($"Generated files in: {outputFolder}");
                Console.WriteLine($"  - input.json");
                Console.WriteLine($"  - mockdata.json ({paymentRows.Count} PaymentRows)");
                Console.WriteLine($"  - expected.json ({capturedImports.Count} PaymentImports)");
                Console.WriteLine($"  - {Path.GetFileName(xmlFilePath)}");
            }
        }

        private HashSet<int> ExtractPaymentIds(List<SEPAFile> files)
        {
            var paymentIds = new HashSet<int>();
            foreach (var file in files)
            {
                if (!string.IsNullOrEmpty(file.EndToEndId))
                {
                    var parts = file.EndToEndId.Split(',');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int paymentId))
                    {
                        paymentIds.Add(paymentId);
                    }
                }
            }
            return paymentIds;
        }
    }
}
