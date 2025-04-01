using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace MrCapitalQ.SwitcherPi.Api;

internal class DeviceSelectorService(IOptions<DeviceScanCodeOptions> options, ILogger<DeviceSelectorService> logger)
{
    private readonly DeviceScanCodeOptions _options = options.Value;
    private readonly ILogger<DeviceSelectorService> _logger = logger;

    public async Task SelectDeviceAsync(int deviceId)
    {
        _logger.LogInformation("Selecting device {deviceId}", deviceId);

        if (_options.DeviceScanCodes?.TryGetValue(deviceId, out var scanCodes) == true)
        {
            foreach (var scanCode in scanCodes)
            {
                _logger.LogInformation("Sending scan code {ScanCode}", scanCode);
                var process = Process.Start("ir-ctl", $"-S {scanCode}");
                await process.WaitForExitAsync();
            }
        }
    }
}
