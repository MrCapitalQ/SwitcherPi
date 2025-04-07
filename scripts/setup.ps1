$sshUser = "q"
$sshHostName = "switcherpi"
$sshDeployTarget = "$sshUser@$sshHostName"

# Update system.
ssh $sshDeployTarget "sudo apt update"
if (!$?) { Exit $LASTEXITCODE }

ssh $sshDeployTarget "sudo apt upgrade -y"
if (!$?) { Exit $LASTEXITCODE }

# Install ir-keytable.
ssh $sshDeployTarget "sudo apt-get install ir-keytable -y"
if (!$?) { Exit $LASTEXITCODE }

# Configure auto-login for headless setups.
ssh $sshDeployTarget "sudo raspi-config nonint do_boot_behaviour B2"
if (!$?) { Exit $LASTEXITCODE }

# Configure IR GPIO pins. Note that this pin configuration is specific for Anavi IR Phat.
ssh $sshDeployTarget "grep -qxF 'dtoverlay=gpio-ir,gpio_pin=18' /boot/firmware/config.txt || (echo 'dtoverlay=gpio-ir,gpio_pin=18' | sudo tee -a /boot/firmware/config.txt)"
if (!$?) { Exit $LASTEXITCODE }
ssh $sshDeployTarget "grep -qxF 'dtoverlay=gpio-ir-tx,gpio_pin=17' /boot/firmware/config.txt || (echo 'dtoverlay=gpio-ir-tx,gpio_pin=17' | sudo tee -a /boot/firmware/config.txt)"
if (!$?) { Exit $LASTEXITCODE }

ssh $sshDeployTarget "sudo reboot"
