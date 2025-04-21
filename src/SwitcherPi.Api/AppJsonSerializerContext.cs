using System.Text.Json.Serialization;

namespace MrCapitalQ.SwitcherPi.Api;

[JsonSerializable(typeof(DevicesStateResponse))]
[JsonSerializable(typeof(DeviceStateRequest))]
[JsonSerializable(typeof(DeviceStateResponse))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class AppJsonSerializerContext : JsonSerializerContext;
