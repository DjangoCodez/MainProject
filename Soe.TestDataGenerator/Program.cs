using System;
using System.Collections.Generic;
using System.Linq;

namespace Soe.TestDataGenerator
{
    /// <summary>
    /// Test Data Generator - Entry point for various test data generation scripts.
    ///
    /// Usage:
    ///   Interactive: Soe.TestDataGenerator.exe
    ///   CLI: Soe.TestDataGenerator.exe &lt;script-name&gt; [script-arguments...]
    ///
    /// Available scripts are discovered automatically from ITestDataScript implementations.
    /// </summary>
    class Program
    {
        private static readonly List<ITestDataScript> AvailableScripts = new List<ITestDataScript>
        {
            new Scripts.ConvertSupplierStreamToEntity()
            // Add new scripts here
        };

        static int Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  SOE Test Data Generator");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            try
            {
                ITestDataScript selectedScript;
                string[] scriptArgs;

                if (args.Length >= 1)
                {
                    // CLI mode - first argument is script name
                    var scriptName = args[0];
                    selectedScript = FindScript(scriptName);

                    if (selectedScript == null)
                    {
                        Console.WriteLine($"Error: Unknown script '{scriptName}'");
                        Console.WriteLine();
                        ShowAvailableScripts();
                        return 1;
                    }

                    // Pass remaining arguments to the script
                    scriptArgs = args.Skip(1).ToArray();
                }
                else
                {
                    // Interactive mode - show script selection menu
                    selectedScript = SelectScript();
                    if (selectedScript == null)
                    {
                        Console.WriteLine("No script selected. Exiting.");
                        return 1;
                    }

                    scriptArgs = new string[0];
                }

                Console.WriteLine($"Running script: {selectedScript.Name}");
                Console.WriteLine();

                return selectedScript.Run(scriptArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static ITestDataScript FindScript(string name)
        {
            return AvailableScripts.FirstOrDefault(s =>
                s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static ITestDataScript SelectScript()
        {
            if (AvailableScripts.Count == 0)
            {
                Console.WriteLine("No scripts available.");
                return null;
            }

            if (AvailableScripts.Count == 1)
            {
                Console.WriteLine($"Only one script available: {AvailableScripts[0].Name}");
                Console.WriteLine();
                return AvailableScripts[0];
            }

            Console.WriteLine("Available scripts:");
            Console.WriteLine();
            for (int i = 0; i < AvailableScripts.Count; i++)
            {
                var script = AvailableScripts[i];
                Console.WriteLine($"  {i + 1}. {script.Name}");
                Console.WriteLine($"     {script.Description}");
                Console.WriteLine();
            }

            while (true)
            {
                Console.Write($"Select script (1-{AvailableScripts.Count}): ");
                var input = Console.ReadLine();
                if (int.TryParse(input, out int selection) && selection >= 1 && selection <= AvailableScripts.Count)
                {
                    Console.WriteLine();
                    return AvailableScripts[selection - 1];
                }
                Console.WriteLine("Invalid selection. Please try again.");
            }
        }

        private static void ShowAvailableScripts()
        {
            Console.WriteLine("Available scripts:");
            Console.WriteLine();
            foreach (var script in AvailableScripts)
            {
                Console.WriteLine($"  {script.Name}");
                Console.WriteLine($"     {script.Description}");
                Console.WriteLine();
            }

            Console.WriteLine("Usage:");
            Console.WriteLine("  Soe.TestDataGenerator.exe                     (interactive mode)");
            Console.WriteLine("  Soe.TestDataGenerator.exe <script-name> ...   (CLI mode)");
        }
    }
}
