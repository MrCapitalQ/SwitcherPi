$sshUser = "q"
$sshHostName = "switcherpi"
$sshDeployTarget = "$sshUser@$sshHostName"

# Update system.
ssh $sshDeployTarget "sudo apt update"
if (!$?) { Exit $LASTEXITCODE }

ssh $sshDeployTarget "sudo apt upgrade -y"
if (!$?) { Exit $LASTEXITCODE }

# Insteall ir-keytable.
ssh $sshDeployTarget "sudo apt-get install ir-keytable -y"
if (!$?) { Exit $LASTEXITCODE }

# Configure auto-login for headless setups.
ssh $sshDeployTarget "sudo raspi-config nonint do_boot_behaviour B2"
if (!$?) { Exit $LASTEXITCODE }

# Configure IR GPIO pins by following the readme.

