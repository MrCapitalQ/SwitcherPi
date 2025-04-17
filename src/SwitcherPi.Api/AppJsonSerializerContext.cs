using System.Text.Json.Serialization;

namespace MrCapitalQ.SwitcherPi.Api;

[JsonSerializable(typeof(DeviceStateRequest))]
[JsonSerializable(typeof(DeviceStateResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;
