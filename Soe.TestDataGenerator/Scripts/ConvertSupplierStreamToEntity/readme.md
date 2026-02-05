# ConvertSupplierStreamToEntity

## Motivation

Creating unit tests for `SEPAV3Manager.ConvertSupplierStreamToEntity` requires realistic test data that matches actual production scenarios. Manually crafting this test data is error-prone and time-consuming because:

- ISO 20022 payment files (CAMT.054) contain complex nested XML structures
- The expected output depends on database state (PaymentRows, invoice data, actor mappings)
- Payment matching logic involves multiple fields (EndToEndId, amounts, dates, invoice numbers)

This script automates test data generation by:
1. Parsing real XML payment files
2. Querying actual database state for payment rows
3. Running the real `ConvertSupplierStreamToEntity` method
4. Capturing the output and anonymizing sensitive data

The result is a complete, consistent test case that can be added to the test suite.

## Output Files

The script generates four files:

| File | Description |
|------|-------------|
| `input.json` | Test configuration (actorCompanyId, importType, etc.) |
| `mockdata.json` | Anonymized PaymentRows and settings from the database |
| `expected.json` | Expected PaymentImportIO results (anonymized) |
| `paymentdata.xml` | Copy of XML file with names/invoice numbers aligned with DB values |

### Anonymization

All sensitive IDs are replaced with sequential fake values to protect production data:

| Data | Anonymization |
|------|---------------|
| Actor/Supplier IDs | Replaced with sequential IDs starting at 101 |
| Invoice IDs | Replaced with sequential IDs starting at 1001 |
| Customer IDs | Mapped consistently with Actor IDs |

Preserved (non-sensitive): amounts, dates, invoice numbers, payment status, billing types.

## Usage

### Interactive Mode

```
Soe.TestDataGenerator.exe ConvertSupplierStreamToEntity
```

The script will prompt for:
1. XML file selection (from `Scripts\ConvertSupplierStreamToEntity\InputData\`)
2. Database name
3. Actor Company ID

### CLI Mode

```
Soe.TestDataGenerator.exe ConvertSupplierStreamToEntity <xml-file> <database-name> <actorCompanyId>
```

Example:
```
Soe.TestDataGenerator.exe ConvertSupplierStreamToEntity "C:\payments\camt054.xml" soeaborademo 100257786
```

### Prerequisites

1. Place XML payment files in `Scripts\ConvertSupplierStreamToEntity\InputData\` for interactive mode
2. Ensure App.config has a valid `SOECompEntities` connection string with `{DATABASE}` placeholder
3. The database must contain the payment data referenced in the XML file (not yet imported)

### Output Location

Generated files are saved to:
```
bin\Debug\Output\ConvertSupplierStreamToEntity\<sanitized-filename>\
```

## Adding Generated Tests to the Test Suite

1. Copy the generated folder to `Soe.Business.Test\TestData\SEPAV3\`
2. Rename the folder to a descriptive test case name (e.g., `Camt54_MultipleInvoices`)
3. Add a test method in `SEPAV3ManagerTests.cs`:

```csharp
[TestMethod]
public void ConvertSupplierStreamToEntity_YourTestCaseName() => RunTestCaseWithXml("YourTestCaseName");
```

See existing tests like `ConvertSupplierStreamToEntity_Camt54_1` for reference.
