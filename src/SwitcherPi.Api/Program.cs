using MrCapitalQ.SwitcherPi.Api;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// Add services to the container.
builder.Services.AddTransient<DeviceSelectorService>();
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

devicesGroup.MapGet("/{id:int}", static (int id, DeviceSelectorService service) =>
{
    return TypedResults.Ok(new DeviceStateResponse(id, service.SelectedDeviceId == id));
});

devicesGroup.MapPost("/{id:int}", static async (int id, DeviceStateRequest state, DeviceSelectorService service) =>
{
    if (state.IsSelected && service.SelectedDeviceId != id)
        await service.SelectDeviceAsync(id);
    else if (!state.IsSelected && service.SelectedDeviceId == id)
        await service.SelectDeviceAsync(0);

    return TypedResults.Ok(new DeviceStateResponse(id, service.SelectedDeviceId == id));
});

app.Run();
