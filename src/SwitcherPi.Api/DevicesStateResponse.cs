namespace MrCapitalQ.SwitcherPi.Api;

internal record DevicesStateResponse(int SelectedDeviceId)
{
    public string SelectedDevice => SelectedDeviceId switch
    {
        1 => DeviceSelectOptions.Device1,
        2 => DeviceSelectOptions.Device2,
        3 => DeviceSelectOptions.Device3,
        _ => DeviceSelectOptions.NoDevice
    };
}

internal class DeviceSelectOptions
{
    public const string NoDevice = "No device";
    public const string Device1 = "Device 1";
    public const string Device2 = "Device 2";
    public const string Device3 = "Device 3";
}
