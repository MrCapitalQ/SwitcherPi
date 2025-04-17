using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace MrCapitalQ.SwitcherPi.Api;

internal class DeviceSelectorService(IOptions<DeviceScanCodeOptions> options, ILogger<DeviceSelectorService> logger)
{
    private static int s_selectedDeviceId = 0;
    private readonly DeviceScanCodeOptions _options = options.Value;
    private readonly ILogger<DeviceSelectorService> _logger = logger;

    public int SelectedDeviceId => s_selectedDeviceId;

    public async Task SelectDeviceAsync(int deviceId)
    {
        _logger.LogInformation("Selecting device {deviceId}", deviceId);

        s_selectedDeviceId = deviceId;

        if (_options.DeviceScanCodes?.TryGetValue(deviceId, out var scanCodes) == true)
        {
            foreach (var scanCode in scanCodes)
            {
                _logger.LogInformation("Sending scan code {ScanCode}", scanCode);
                var process = new Process
                {
                    StartInfo = new()
                    {
                        FileName = "ir-ctl",
                        Arguments = $"-S {scanCode}",
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                await process.WaitForExitAsync();

                var error = await process.StandardError.ReadToEndAsync();
                if (!string.IsNullOrEmpty(error))
                    throw new DeviceSelectException(error);
            }
        }
    }
}

internal class DeviceSelectException(string message) : Exception(message) { }
