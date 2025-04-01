using MrCapitalQ.SwitcherPi.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<DeviceSelectorService>();
builder.Services.AddOptions<DeviceScanCodeOptions>()
    .Bind(builder.Configuration.GetSection("DeviceScanCodes"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapPut("/api/Devices", static async (int id, DeviceSelectorService service) =>
{
    await service.SelectDeviceAsync(id);
    return TypedResults.NoContent();
});

app.Run();
