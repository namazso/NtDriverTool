//  Copyright (c) 2025 namazso <admin@namazso.eu>
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System.ServiceProcess;
using NtCoreLib;
using NtCoreLib.Kernel.Process;

namespace NtDriverTool;

public class DriverDataAggregator
{
    // Cheat dictionary for fake drivers by Microsoft stuff
    private static readonly Dictionary<string, string> CheatDictionary = new()
    {
        { "\\Driver\\ACPI_HAL", "\\SystemRoot\\system32\\ntoskrnl.exe" },
        { "\\Driver\\RegistryHiveCacheDriver", "\\SystemRoot\\system32\\ntoskrnl.exe" },
        { "\\Driver\\VERIFIER_DDI", "\\SystemRoot\\system32\\ntoskrnl.exe" },
        { "\\Driver\\VERIFIER_FILTER", "\\SystemRoot\\system32\\ntoskrnl.exe" },
        { "\\Driver\\PnpManager", "\\SystemRoot\\system32\\ntoskrnl.exe" },
        { "\\Driver\\DeviceApi", "\\SystemRoot\\system32\\ntoskrnl.exe" },
        { "\\Driver\\SoftwareDevice", "\\SystemRoot\\system32\\ntoskrnl.exe" },
        { "\\Driver\\WMIxWDM", "\\SystemRoot\\system32\\ntoskrnl.exe" },
        { "\\Driver\\WscVReg", "\\SystemRoot\\system32\\ntoskrnl.exe" },
        { "\\FileSystem\\RAW", "\\SystemRoot\\system32\\ntoskrnl.exe" },
        { "\\Driver\\Win32k", "\\SystemRoot\\system32\\win32k.sys" }
    };

    // Property to expose the driver data - changed from List to SortableBindingList
    public SortableBindingList<DriverDataEntry> Drivers { get; } = [];

    public static string BuildFullDriverPath(string? imagePath, string keyName)
    {
        // Check if we have an image path from registry
        if (imagePath != null)
        {
            // If imagePath doesn't start with backslash, prepend SystemRoot
            if (!imagePath.StartsWith("\\"))
                return "\\SystemRoot\\" + imagePath;

            // Path already starts with backslash, use as-is
            return imagePath;
        }

        // No ImagePath, use default driver path with keyName and .SYS extension
        return "\\SystemRoot\\System32\\Drivers\\" + keyName + ".SYS";
    }

    public void Refresh()
    {
        var driverData = new List<DriverData>();

        var services = ServiceController.GetDevices()
            .ToDictionary(s => s.ServiceName, StringComparer.OrdinalIgnoreCase);

        foreach (var service in services)
            service.Value.Refresh();

        using (var servicesKey = NtKey.GetMachineKey().Open("SYSTEM\\CurrentControlSet\\Services"))
        {
            servicesKey.VisitAccessibleKeys(key =>
            {
                var objectName = key.QueryValue("ObjectName", false).GetResultOrDefault(null)?.Data.ToString() ??
                                 key.Name;
                if (services.TryGetValue(key.Name, out var svc))
                {
                    // Service keys known to SCM. Remove from the list so we can find any not in the registry.

                    services.Remove(key.Name);
                    if (svc.ServiceType.HasFlag(ServiceType.KernelDriver) ||
                        svc.ServiceType.HasFlag(ServiceType.FileSystemDriver) ||
                        svc.ServiceType.HasFlag(ServiceType.RecognizerDriver))
                    {
                        if (svc.ServiceType.HasFlag(ServiceType.FileSystemDriver) ||
                            svc.ServiceType.HasFlag(ServiceType.RecognizerDriver))
                            objectName = "\\FileSystem\\" + objectName;
                        else
                            objectName = "\\Driver\\" + objectName;

                        driverData.Add(new DriverData(key.Name)
                        {
                            ServiceKey = key.Duplicate(),
                            Service = svc,
                            ExpectedObjectName = objectName,
                            ModuleInformation = null
                        });
                    }
                }
                else
                {
                    // Service keys not known to SCM. Check the Type value to see if it's a driver.

                    uint? type = null;
                    try
                    {
                        var value = key.QueryValue("Type");
                        if (value is { Type: RegistryValueType.Dword })
                            type = BitConverter.ToUInt32(value.Data, 0);
                    }
                    catch (NtException)
                    {
                    }

                    // Type 1 = Kernel Driver, Type 2 = File System Driver, Type 8 = Recognizer Driver
                    if (type is 1 or 2 or 8)
                    {
                        if (type is 2 or 8)
                            objectName = "\\FileSystem\\" + key.Name;
                        else
                            objectName = "\\Driver\\" + key.Name;

                        driverData.Add(new DriverData(key.Name)
                        {
                            ServiceKey = key.Duplicate(),
                            Service = null,
                            ExpectedObjectName = objectName,
                            ModuleInformation = null
                        });
                    }
                }

                return true;
            });
        }

        // Services known to SCM but not in the registry.
        driverData.AddRange(from service in services.Values
            where service.ServiceType.HasFlag(ServiceType.KernelDriver) ||
                  service.ServiceType.HasFlag(ServiceType.FileSystemDriver) ||
                  service.ServiceType.HasFlag(ServiceType.RecognizerDriver)
            select new DriverData(service.ServiceName)
                { ServiceKey = null, Service = service, ModuleInformation = null });

        List<string> driverObjects = [];
        try
        {
            using var directory = NtDirectory.Open("\\Driver");
            driverObjects.AddRange(directory.Query()
                .Where(d => d.NtTypeName == "Driver")
                .Select(d => "\\Driver\\" + d.Name).ToList());
        }
        catch (NtException)
        {
        }

        try
        {
            using var directory = NtDirectory.Open("\\FileSystem");
            driverObjects.AddRange(directory.Query()
                .Where(d => d.NtTypeName == "Driver")
                .Select(d => "\\FileSystem\\" + d.Name).ToList());
        }
        catch (NtException)
        {
        }

        foreach (var driverObject in driverObjects)
        {
            var data = driverData.FirstOrDefault(d =>
                string.Equals(d.ExpectedObjectName, driverObject, StringComparison.OrdinalIgnoreCase));
            var identifier = driverObject.Substring(driverObject.LastIndexOf('\\') + 1);
            data ??= driverData.FirstOrDefault(d =>
                string.Equals(d.Identifier, identifier, StringComparison.OrdinalIgnoreCase));
            if (data is null)
                // Driver objects not known to SCM or the registry.
                driverData.Add(new DriverData(identifier)
                {
                    ServiceKey = null,
                    Service = null,
                    ModuleInformation = null,
                    HasDriverObject = true,
                    ExpectedObjectName = driverObject
                });
            else
                // Driver object known to SCM or the registry.
                data.HasDriverObject = true;
        }


        var modules = NtSystemInfo.GetKernelModules().ToList();

        foreach (var data in driverData)
            if (data.HasDriverObject || data.Service is { Status: ServiceControllerStatus.Running } or
                    { Status: ServiceControllerStatus.StopPending })
            {
                var imagePath = data.ServiceKey?.QueryValue("ImagePath", false).GetResultOrDefault(null)?.ToString();
                if (imagePath == null)
                    if (data.ExpectedObjectName != null && CheatDictionary.TryGetValue(data.ExpectedObjectName,
                            out var cheatPath))
                        imagePath = cheatPath;

                var fullPath = BuildFullDriverPath(imagePath, data.Identifier);
                data.ModuleInformation = modules.FirstOrDefault(m =>
                    string.Equals(m.FullPathName, fullPath, StringComparison.OrdinalIgnoreCase));
            }

        foreach (var data in driverData)
            if (data.ModuleInformation != null)
                modules.Remove(data.ModuleInformation);

        // Drivers with a module but not known to SCM or the registry.
        driverData.AddRange(from module in modules
            select new DriverData(module.Name)
                { ServiceKey = null, Service = null, ModuleInformation = module });

        // Update the Drivers list based on the newly collected driverData

        // Create a lookup dictionary for faster matching
        var newDriversDict = driverData.ToDictionary(d => d.Identifier, StringComparer.OrdinalIgnoreCase);

        // First pass: Update or mark for removal existing drivers
        for (var i = Drivers.Count - 1; i >= 0; i--)
        {
            var driver = Drivers[i];
            if (newDriversDict.TryGetValue(driver.Name, out var updatedData))
            {
                // Update existing driver with new data
                driver.SetOriginalData(updatedData);
                // Remove from dictionary to track what's been processed
                newDriversDict.Remove(driver.Name);
            }
            else
            {
                // Remove driver that no longer exists
                Drivers.RemoveAt(i);
            }
        }

        // Second pass: Add new drivers
        foreach (var newDriver in newDriversDict.Values)
            Drivers.Add(new DriverDataEntry(newDriver));
    }

    /// <summary>
    ///     Cache entry for driver data that stores computed properties to improve DataGridView performance
    /// </summary>
    public class DriverDataEntry
    {
        private DriverData _originalData;

        public DriverDataEntry(DriverData driverData)
        {
            _originalData = driverData;
        }

        // Cached properties
        public string Name => _originalData.Identifier;

        public string Status
        {
            get
            {
                if (_originalData.Service is not null)
                    return _originalData.Service.Status.ToString();

                if (_originalData.HasDriverObject || _originalData.ModuleInformation is not null)
                    return "Running (Unknown Service)";

                if (_originalData.ServiceKey is not null)
                    return "Stopped (Not known to SCM)";

                return "Unknown";
            }
        }

        public string ImagePath
        {
            get
            {
                if (_originalData.ModuleInformation?.FullPathName != null)
                    return _originalData.ModuleInformation.FullPathName;

                if (_originalData.ServiceKey != null)
                {
                    var regImagePath = _originalData.ServiceKey.QueryValue("ImagePath", false).GetResultOrDefault(null);
                    return BuildFullDriverPath(regImagePath?.ToString(), _originalData.ServiceKey.Name);
                }

                return string.Empty;
            }
        }

        public string ObjectName => _originalData.HasDriverObject
            ? _originalData.ExpectedObjectName ?? string.Empty
            : string.Empty;

        public string HasModule => _originalData.ModuleInformation != null ? "Yes" : "No";
        public string HasDriverObject => _originalData.HasDriverObject ? "Yes" : "No";
        public string HasService => _originalData.Service != null ? "Yes" : "No";
        public string HasRegistry => _originalData.ServiceKey != null ? "Yes" : "No";
        public string ModuleSection => _originalData.ModuleInformation?.Section.ToString("X") ?? string.Empty;
        public string ModuleMappedBase => _originalData.ModuleInformation?.MappedBase.ToString("X") ?? string.Empty;
        public string ModuleImageBase => _originalData.ModuleInformation?.ImageBase.ToString("X") ?? string.Empty;
        public string ModuleImageSize => _originalData.ModuleInformation?.ImageSize.ToString() ?? string.Empty;
        public string ModuleFlags => _originalData.ModuleInformation?.Flags.ToString("X") ?? string.Empty;

        public string ModuleLoadOrderIndex =>
            _originalData.ModuleInformation?.LoadOrderIndex.ToString() ?? string.Empty;

        // This doesn't seem to be provided even to admins
        //public string ModuleInitOrderIndex => _originalData.ModuleInformation?.InitOrderIndex.ToString() ?? string.Empty;
        public string ModuleLoadCount => _originalData.ModuleInformation?.LoadCount.ToString() ?? string.Empty;
        public string ModuleFullPathName => _originalData.ModuleInformation?.FullPathName ?? string.Empty;
        public string ModuleName => _originalData.ModuleInformation?.Name ?? string.Empty;

        public string ScmServiceName => _originalData.Service?.ServiceName ?? string.Empty;
        public string ScmDisplayName => _originalData.Service?.DisplayName ?? string.Empty;
        public string ScmServiceType => _originalData.Service?.ServiceType.ToString() ?? string.Empty;
        public string ScmStartType => _originalData.Service?.StartType.ToString() ?? string.Empty;
        public string ScmStatus => _originalData.Service?.Status.ToString() ?? string.Empty;

        public string RegImagePath =>
            _originalData.ServiceKey?.QueryValue("ImagePath", false).GetResultOrDefault(null)?.ToString() ??
            string.Empty;

        public string RegObjectName =>
            _originalData.ServiceKey?.QueryValue("ObjectName", false).GetResultOrDefault(null)?.ToString() ??
            string.Empty;

        public string RegType => _originalData.ServiceKey?.QueryValue("Type", false).GetResultOrDefault(null) is
            { Type: RegistryValueType.Dword } typeValue
            ? BitConverter.ToUInt32(typeValue.Data, 0).ToString()
            : string.Empty;

        public string RegErrorControl =>
            _originalData.ServiceKey?.QueryValue("ErrorControl", false).GetResultOrDefault(null) is
                { Type: RegistryValueType.Dword } errorValue
                ? BitConverter.ToUInt32(errorValue.Data, 0).ToString()
                : string.Empty;

        public DateTime? RegLastWriteTime => _originalData.ServiceKey?.LastWriteTime;

        // Reference to the original data
        public DriverData GetOriginalData()
        {
            return _originalData;
        }

        public void SetOriginalData(DriverData data)
        {
            _originalData = data;
        }
    }

    public class DriverData(string identifier)
    {
        public string Identifier { get; } = identifier;
        public ProcessModuleInformation? ModuleInformation { get; set; }
        public ServiceController? Service { get; set; }
        public NtKey? ServiceKey { get; set; }
        public string? ExpectedObjectName { get; set; }
        public bool HasDriverObject { get; set; }
    }
}