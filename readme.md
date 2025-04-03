# Setup

## Run setup script
On a fresh Raspberry Pi OS Lite image, run the [setup.ps1](/scripts/setup.ps1) script to update and install required packages. Update the target hostname as needed before running.

## Configure GPIO Pins for IR
SSH into the Raspberry Pi and open `/boot/firmware/config.txt`.
```shell
sudo nano /boot/firmware/config.txt
```

Add following entries, save, and close the file.
```conf
dtoverlay=gpio-ir,gpio_pin=18
dtoverlay=gpio-ir-tx,gpio_pin=17
```
> Note that this pin configuration is specific for Anavi IR Phat.

Reboot to apply changes.
```shell
sudo reboot
```

## Deploy service
Run the [deployRpi.ps1](/scripts/deployRPi.ps1) script to build the application and deploy it. Update the target hostname as needed before running. Once deployed, the Raspberry Pi will be ready and listening for commands.

# Capturing IR Codes
To find IR codes of new remotes, the Raspberry Pi has to be reconfigured to receive. Edit `/boot/firmware/config.txt`, comment out `dtoverlay=gpio-ir-tx,gpio_pin=17`, and reboot.

Enable all IR protocols.

```shell
sudo ir-keytable -p all
```

> Old mappings might need to be cleared using `sudo ir-keytable -c`.

Start IR receive test.
```shell
ir-keytable -t
```

Press buttons on the IR remote and take note of the protocol and scancode. When finished, uncomment `dtoverlay=gpio-ir-tx,gpio_pin=17` again and reboot.

### Current scan codes
#### HDMI Switch
- 1 - `nec:0x8005`
- 2 - `nec:0x8009`
- 3 - `nec:0x8007`
- 4 - `nec:0x801b`
- 5 - `nec:0x8008`

#### USB Switch
- ON - `nec:0x8012`
- OFF - `nec:0x801e`
- 1 - `nec:0x8004`
- 2 - `nec:0x8006`
- 3 - `nec:0x800a`
- 4 - `nec:0x801f`
