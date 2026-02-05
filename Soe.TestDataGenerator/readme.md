# Soe.TestDataGenerator

A command-line tool for generating anonymized test data files based on real data.

## Motivation

Unit tests require realistic test data that matches actual production scenarios. Manually creating this data is:

- **Error-prone**: Complex data structures with interdependencies are easy to get wrong
- **Time-consuming**: Crafting realistic XML, JSON, and database state takes significant effort
- **Hard to maintain**: When business logic changes, test data must be updated consistently

This project automates test data generation by:

1. Using real production data as input
2. Running actual business logic to capture expected outputs
3. Anonymizing sensitive data for safe inclusion in the test suite
4. Producing complete, consistent test cases ready for use

## Usage

### Interactive Mode

```
Soe.TestDataGenerator.exe
```

Shows a menu of available scripts and prompts for required inputs.

### CLI Mode

```
Soe.TestDataGenerator.exe <script-name> [script-arguments...]
```

Runs a specific script with provided arguments.

### Examples

```bash
# Interactive - select script and provide inputs via prompts
Soe.TestDataGenerator.exe

# CLI - run ConvertSupplierStreamToEntity with arguments
Soe.TestDataGenerator.exe ConvertSupplierStreamToEntity "C:\data\payment.xml" soecompv_18 100257786
```

## Available Scripts

See each script's `readme.md` in `Scripts\<ScriptName>\` for detailed documentation.

## Project Structure

```
Soe.TestDataGenerator\
  Program.cs                    # Entry point, script discovery and selection
  ITestDataScript.cs            # Interface all scripts must implement
  Scripts\
    <ScriptName>\
      <ScriptName>.cs           # Script implementation
      readme.md                 # Script documentation
      InputData\                # Input files for interactive mode
```

## Adding a New Script

### 1. Create the folder structure

```
Scripts\
  MyNewScript\
    MyNewScript.cs
    readme.md
    InputData\
      (placeholder or sample files)
```

### 2. Implement ITestDataScript

```csharp
namespace Soe.TestDataGenerator.Scripts
{
    public class MyNewScript : ITestDataScript
    {
        public string Name => "MyNewScript";

        public string Description => "Brief description shown in the menu";

        public int Run(string[] args)
        {
            // Parse args for CLI mode or prompt for interactive mode
            // Generate test data files
            // Return 0 for success, non-zero for failure
        }
    }
}
```

### 3. Register in Program.cs

```csharp
private static readonly List<ITestDataScript> AvailableScripts = new List<ITestDataScript>
{
    new Scripts.ConvertSupplierStreamToEntity(),
    new Scripts.MyNewScript()  // Add your script here
};
```

### 4. Write documentation

Create `Scripts\MyNewScript\readme.md` with:

- **Motivation**: Why this script exists, what problem it solves
- **Output Files**: What files are generated and their purpose
- **Usage**: Interactive and CLI modes with examples
- **Prerequisites**: Required setup, database state, input files

## Configuration

The `App.config` file contains connection string templates. Scripts use `{DATABASE}` as a placeholder that gets replaced with the actual database name at runtime:

```xml
<connectionStrings>
  <add name="SOECompEntities"
       connectionString="metadata=...;data source=YOURSERVER;initial catalog={DATABASE};..." />
</connectionStrings>
```

## Output

Generated test data is saved to:

```
bin\Debug\Output\<ScriptName>\<TestCaseName>\
```

Copy the generated folder to the appropriate test project's `TestData\` directory.
