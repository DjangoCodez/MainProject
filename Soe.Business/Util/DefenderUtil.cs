using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SoftOne.Soe.Business.Util
{
    public class DefenderUtil
    {
        private static bool _isDefenderAvailable;
        private static string _defenderPath;
        private static SemaphoreSlim _lock = new SemaphoreSlim(5); //limit to 5 concurrent checks at a time

        //static ctor
        static DefenderUtil()
        {
            _defenderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender", "MpCmdRun.exe");
            _isDefenderAvailable = File.Exists(_defenderPath);
        }

        public static bool IsDefenderAvailable
        {
            get
            {
                return _isDefenderAvailable;
            }
            set
            {
                _isDefenderAvailable = value;
            }
        }

        public static bool IsVirusBase64(string base64)
        {
            if (base64 == null)
                return false;

            return IsVirus(Convert.FromBase64String(base64));
        }

        public static bool IsVirus(byte[] file)
        {
            if (!_isDefenderAvailable || file == null) return false;

            string path = GetTempFileName();
            File.WriteAllBytes(path, file); //save temp file

            try
            {
                return IsVirus(path);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path); //cleanup temp file
            }
        }

        private static string GetTempFileName()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".DefenderUtil");
        }

        public static bool IsVirus(string path)
        {
            bool result = false;

            if (!File.Exists(path))
                return false;

            _lock.Wait();
            try
            {
                using (var process = Process.Start(_defenderPath, $"-Scan -ScanType 3 -File \"{path}\" -DisableRemediation"))
                {
                    if (process == null)
                    {
                        _isDefenderAvailable = false; //disable future attempts
                        throw new InvalidOperationException("Failed to start MpCmdRun.exe");
                    }

                    try
                    {
                        process.WaitForExit(25000);
                    }
                    catch (TimeoutException ex) //timeout
                    {
                        throw new TimeoutException("Timeout waiting for MpCmdRun.exe to return", ex);
                    }
                    finally
                    {
                        try
                        {
                            if (process.ExitTime == DateTime.MinValue)
                                process.Kill(); //always try to kill the process, it's fine if it's already exited, but if we were timed out or cancelled via token - let's kill it }

                            result = process.ExitCode == 2;

                            if (File.Exists(path) && result)
                                File.Delete(path);
                        }
                        catch
                        {
                            // Intentionally ignored, safe to continue
                            // NOSONAR
                        }
                    }

                    if (Environment.MachineName.Contains("33"))
                        LogCollector.LogCollector.LogInfo("Scanned For Virus: " + path + " Result: " + result);

                    return result;
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}