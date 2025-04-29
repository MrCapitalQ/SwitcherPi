using HomeAssistantDiscoveryNet;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using System.Text.Json;
using ToMqttNet;

namespace MrCapitalQ.SwitcherPi.Api;

internal class MqttBackgroundService : BackgroundService
{
    private readonly string _stateTopic;
    private readonly string _setSelectedDeviceTopic;
    private readonly MqttConnectionService _mqtt;
    private readonly DeviceSelectorService _service;
    private readonly ILogger<MqttBackgroundService> _logger;

    public MqttBackgroundService(MqttConnectionService mqtt,
        DeviceSelectorService service,
        ILogger<MqttBackgroundService> logger)
    {
        _mqtt = mqtt;
        _service = service;
        _logger = logger;
        _stateTopic = $"{_mqtt.MqttOptions.NodeId}/state";
        _setSelectedDeviceTopic = $"{_mqtt.MqttOptions.NodeId}/command/set_selected_device";
        _mqtt.OnConnectAsync += Mqtt_OnConnectAsync;
        _mqtt.OnApplicationMessageReceivedAsync += Mqtt_OnApplicationMessageReceivedAsync;

        _service.SelectedDeviceChanged += Service_SelectedDeviceChanged;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _mqtt.SubscribeAsync(new MqttTopicFilter() { Topic = _setSelectedDeviceTopic });

        var device = new MqttDiscoveryDevice
        {
            Name = "SwitcherPi",
            Identifiers = [_mqtt.MqttOptions.NodeId]
        };

        var availability = new List<MqttDiscoveryAvailablilty>
        {
            new()
            {
                Topic = $"{_mqtt.MqttOptions.NodeId}/connected",
                PayloadAvailable = "2",
                PayloadNotAvailable = "0"
            }
        };

        await _mqtt.PublishDiscoveryDocument(new MqttSelectDiscoveryConfig()
        {
            Name = $"Selected Device",
            UniqueId = $"switcherpi_selected_device",
            Availability = availability,
            Device = device,
            CommandTopic = _setSelectedDeviceTopic,
            StateTopic = _stateTopic,
            ValueTemplate = "{{ value_json.selectedDevice }}",
            Options =
            [
                DeviceSelectOptions.NoDevice,
                DeviceSelectOptions.Device1,
                DeviceSelectOptions.Device2,
                DeviceSelectOptions.Device3
            ],
            Retain = true
        });

        // Wait indefinitely until the service is stopped so it's not cleaned up.
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private async Task PublishStateUpdateAsync()
    {
        _logger.LogInformation("Publishing current state to MQTT broker.");

        var selectedDeviceId = await _service.GetSelectedDeviceIdAsync();
        var payload = JsonSerializer.Serialize(new DevicesStateResponse(selectedDeviceId),
            AppJsonSerializerContext.Default.DevicesStateResponse);

        await _mqtt.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic(_stateTopic)
            .WithPayload(payload)
            .WithRetainFlag()
            .Build());
    }

    private async Task Mqtt_OnConnectAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogInformation("MQTT client connected to broker.");
        await PublishStateUpdateAsync();
    }

    private async Task Mqtt_OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        _logger.LogInformation("Received MQTT message with topic {MqttTopic}", arg.ApplicationMessage.Topic);

        if (arg.ApplicationMessage.Topic != _setSelectedDeviceTopic)
            return;

        var payload = arg.ApplicationMessage.ConvertPayloadToString();
        var deviceId = payload switch
        {
            DeviceSelectOptions.Device1 => 1,
            DeviceSelectOptions.Device2 => 2,
            DeviceSelectOptions.Device3 => 3,
            _ => 0
        };

        await _service.SelectDeviceAsync(deviceId);
    }

    private async void Service_SelectedDeviceChanged(object? sender, EventArgs e) => await PublishStateUpdateAsync();
}
