namespace Volo.Abp.Internal.Telemetry.Constants;

static internal class DeviceManager
{
    public static string GetUniquePhysicalKey(bool shouldHash)
    {
        char platformId = '?';
        char osArchitecture = '?';
        string operatingSystem = "?";

        try
        {
            string osPrefix;
            string uniqueKey;

            platformId = GetPlatformIdOrDefault();
            osArchitecture = GetOsArchitectureOrDefault();

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform
                    .Windows))
            {
                operatingSystem = "Windows";
                uniqueKey = GetUniqueKeyForWindows();
                osPrefix = "W";
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                         .OSPlatform.Linux))
            {
                operatingSystem = "Linux";
                uniqueKey = GetHarddiskSerialForLinux();
                osPrefix = "L";
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                         .OSPlatform.OSX)) //MAC
            {
                operatingSystem = "OSX";
                uniqueKey = GetHarddiskSerialForOsX();
                osPrefix = "O";
            }
            else
            {
                operatingSystem = "Other";
                uniqueKey = GetNetworkAdapterSerial();
                osPrefix = "X";
            }

            if (shouldHash)
            {
                uniqueKey = ConvertToMd5(uniqueKey).ToUpperInvariant();
            }

            return osPrefix + platformId + osArchitecture + "-" + uniqueKey;
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine("WARNING ABP-LIC-0025! Contact to license@abp.io with the below information" +
                                     System.Environment.NewLine +
                                     "* Architecture: " +
                                     System.Runtime.InteropServices.RuntimeInformation.OSArchitecture +
                                     System.Environment.NewLine +
                                     "* Description: " +
                                     System.Runtime.InteropServices.RuntimeInformation.OSDescription +
                                     System.Environment.NewLine +
                                     "* Platform Id: " + platformId + System.Environment.NewLine +
                                     "* OS architecture: " + osArchitecture + System.Environment.NewLine +
                                     "* Operating system: " + operatingSystem + System.Environment.NewLine +
                                     "* Framework description: " +
                                     System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription +
                                     System.Environment.NewLine +
                                     "* Error: " + ex.ToString());

            return "95929008-b147-454a-8737-efed71fa2241";
        }
    }
    private static string GetNetworkAdapterSerial()
    {
        string macAddress = string.Empty;

        var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
        foreach (var networkInterface in networkInterfaces)
        {
            if (networkInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
            {
                continue;
            }

            var physicalAddress = networkInterface.GetPhysicalAddress().ToString();
            if (string.IsNullOrEmpty(physicalAddress))
            {
                continue;
            }

            macAddress = physicalAddress;
            break;
        }

        return macAddress!;
    }
    /// <returns>
    /// 0 - Win32S
    /// 1 - Win32Windows
    /// 2 - Win32NT
    /// 3 - WinCE
    /// 4 - Unix
    /// 5 - Xbox
    /// 6 - MacOSX
    /// </returns>
    private static char GetPlatformIdOrDefault(char defaultValue = '*')
    {
        try
        {
            return ((int)System.Environment.OSVersion.Platform).ToString()[0];
        }
        catch
        {
            return defaultValue;
        }
    }
    private static string ConvertToMd5(string text)
    {
        using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
        {
            return EncodeBase64(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text)));
        }
    }
    
    private static string EncodeBase64(byte[] ba)
    {
        var hex = new System.Text.StringBuilder(ba.Length * 2);

        foreach (var b in ba)
        {
            hex.AppendFormat("{0:x2}", b);
        }

        return hex.ToString();
    }

    /// <returns>
    /// 0 - X86 (An Intel-based 32-bit processor architecture)
    /// 1 - X64 (A 64-bit ARM processor architecture)
    /// 2 - Arm (A 32-bit ARM processor architecture)
    /// 3 - Arm64 (A 64-bit ARM processor architecture)
    /// </returns>
    private static char GetOsArchitectureOrDefault(char defaultValue = '*')
    {
        try
        {
            return ((int)System.Runtime.InteropServices.RuntimeInformation.OSArchitecture).ToString()[0];
        }
        catch
        {
            return defaultValue;
        }
    }

    private static string GetUniqueKeyForWindows()
    {
        try
        {
            return GetProcessorIdForWindows();
        }
        catch
        {
            //couldn't get processor id when logon user has no required permission. try other methods...
        }

        return GetWindowsMachineUniqueId();
    }

    private static string GetProcessorIdForWindows()
    {
        using (var managementObjectSearcher =
               new System.Management.ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
        {
            using (var searcherObj = managementObjectSearcher.Get())
            {
                if (searcherObj.Count == 0)
                {
                    throw new System.Exception("No unique computer ID found for this computer!");
                }

                var managementObjectEnumerator = searcherObj.GetEnumerator();
                managementObjectEnumerator.MoveNext();
                return managementObjectEnumerator.Current.GetPropertyValue("ProcessorId").ToString()!;
            }
        }
    }

    private static string GetWindowsMachineUniqueId()
    {
        return RunCommandAndGetOutput("powershell (Get-CimInstance -Class Win32_ComputerSystemProduct).UUID");
    }

  
    private static string GetHarddiskSerialForLinux()
    {
        return RunCommandAndGetOutput(
            "udevadm info --query=all --name=/dev/sda | grep ID_SERIAL_SHORT | tr -d \"ID_SERIAL_SHORT=:\"");
    }

    private static string GetHarddiskSerialForOsX()
    {
        var command =
            "ioreg -rd1 -c IOPlatformExpertDevice | awk '/IOPlatformUUID/ { split($0, line, \"\\\"\"); printf(\"%s\\n\", line[4]); }'";

        command = System.Text.RegularExpressions.Regex.Replace(command, @"(\\*)" + "\"", @"$1$1\" + "\"");

        return RunCommandAndGetOutput(command);
    }

    private static string RunCommandAndGetOutput(string command)
    {
        var output = "";

        using (var process = new System.Diagnostics.Process())
        {
            process.StartInfo = new System.Diagnostics.ProcessStartInfo(GetFileName())
            {
                Arguments = GetArguments(command),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            process.Start();
            process?.WaitForExit();

            using (var stdOut = process!.StandardOutput)
            {
                using (var stdErr = process.StandardError)
                {
                    output = stdOut.ReadToEnd();
                    output += stdErr.ReadToEnd();
                }
            }
        }

        return output.Trim();
    }

    private static string GetFileName()
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.OSX) ||
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform
                .Linux))
        {
            string[] fileNames = { "/bin/bash", "/usr/bin/bash", "/bin/sh", "/usr/bin/sh" };
            foreach (var fileName in fileNames)
            {
                try
                {
                    if (System.IO.File.Exists(fileName))
                    {
                        return fileName;
                    }
                }
                catch
                {
                    //ignore
                }
            }

            return "/bin/bash";
        }

        //Windows default.
        return "cmd.exe";
    }

    private static string GetArguments(string command)
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.OSX) ||
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform
                .Linux))
        {
            return "-c \"" + command + "\"";
        }

        //Windows default.
        return "/C \"" + command + "\"";
    }
}