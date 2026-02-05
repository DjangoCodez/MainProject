namespace Soe.TestDataGenerator
{
    /// <summary>
    /// Interface for test data generation scripts.
    /// Each script generates test data files for a specific scenario.
    /// </summary>
    public interface ITestDataScript
    {
        /// <summary>
        /// Short name used for CLI argument (e.g., "ConvertSupplierStreamToEntity").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Human-readable description shown in the selection menu.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Runs the test data generation script.
        /// </summary>
        /// <param name="args">Command line arguments (excluding script name).</param>
        /// <returns>Exit code (0 = success, non-zero = failure).</returns>
        int Run(string[] args);
    }
}
