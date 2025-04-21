using MrCapitalQ.SwitcherPi.Api;
using ToMqttNet;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// Add services to the container.
builder.Services.AddMqttConnection()
    .Bind(builder.Configuration.GetSection(MqttConnectionOptions.Section));
builder.Services.AddHostedService<MqttBackgroundService>();

builder.Services.AddSingleton<DeviceSelectorService>();
builder.Services.AddOptions<DeviceScanCodeOptions>()
    .Bind(builder.Configuration.GetSection("DeviceScanCodes"));

var app = builder.Build();

// Configure the HTTP request pipeline.
var devicesGroup = app.MapGroup("/api/Devices");
devicesGroup.MapPut("/", static async (int id, DeviceSelectorService service) =>
{
    await service.SelectDeviceAsync(id);
    return TypedResults.NoContent();
});

devicesGroup.MapGet("/", static async (DeviceSelectorService service) =>
{
    var selectedDeviceId = await service.GetSelectedDeviceIdAsync();
    return TypedResults.Ok(new DevicesStateResponse(selectedDeviceId));
});

devicesGroup.MapGet("/{id:int}", static async (int id, DeviceSelectorService service) =>
{
    var selectedDeviceId = await service.GetSelectedDeviceIdAsync();
    return TypedResults.Ok(new DeviceStateResponse(id, selectedDeviceId == id));
});

devicesGroup.MapPost("/{id:int}", static async (int id, DeviceStateRequest state, DeviceSelectorService service) =>
{
    var selectedDeviceId = await service.GetSelectedDeviceIdAsync();
    if (state.IsSelected && selectedDeviceId != id)
        await service.SelectDeviceAsync(id);
    else if (!state.IsSelected && selectedDeviceId == id)
        await service.SelectDeviceAsync(0);

    selectedDeviceId = await service.GetSelectedDeviceIdAsync();
    return TypedResults.Ok(new DeviceStateResponse(id, selectedDeviceId == id));
});

await app.Services.GetRequiredService<DeviceSelectorService>().SynchronizeAsync();

app.Run();
