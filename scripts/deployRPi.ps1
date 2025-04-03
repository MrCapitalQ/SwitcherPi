$publishDirectory = ".\src\SwitcherPi.Api\bin\Release\net9.0\linux-arm64\publish"
$deployDirectory = "/var/www/SwitcherPi"
$sshUser = "q"
$sshHostName = "switcherpi"
$serviceName = "switcherpi.service"

$sshDeployTarget = "$sshUser@$sshHostName"
$currentDir = Get-Location

try {
    Set-Location $PSScriptRoot
    Set-Location ..

    # Clean publish directory
    Remove-Item -LiteralPath $publishDirectory -Force -Recurse -ErrorAction Ignore

    # Build and publish app for Raspberry Pi OS 64 bit.
    dotnet publish .\src\SwitcherPi.Api --configuration Release --runtime linux-arm64 --self-contained true
    if (!$?) { Exit $LASTEXITCODE }

    # Stop existing sevice
    ssh $sshDeployTarget "sudo systemctl stop $($serviceName)"
    ssh $sshDeployTarget "sudo systemctl disable $($serviceName)"

    # Ensure deploy destination exists
    ssh $sshDeployTarget "sudo mkdir -p $deployDirectory && sudo chown $sshUser $deployDirectory"
    if (!$?) { Exit $LASTEXITCODE }

    # Copy publish directory to Raspberry Pi deploy destination
    scp -r $publishDirectory\* "$($sshDeployTarget):$deployDirectory"
    if (!$?) { Exit $LASTEXITCODE }

    # Copy service definition to Raspberry Pi deploy destination
    scp -r ".\infra\linux\$($serviceName)" "$($sshDeployTarget):$deployDirectory"
    if (!$?) { Exit $LASTEXITCODE }

    # Give app execution permission.
    ssh $sshDeployTarget "chmod +x $($deployDirectory)/MrCapitalQ.SwitcherPi.Api"
    if (!$?) { Exit $LASTEXITCODE }

    # Copy latest service definition to systemd and reload services.
    ssh $sshDeployTarget "sudo cp $($deployDirectory)/$($serviceName) /etc/systemd/system"
    if (!$?) { Exit $LASTEXITCODE }

    ssh $sshDeployTarget "sudo systemctl daemon-reload"
    if (!$?) { Exit $LASTEXITCODE }

    # Enable auto startup and start the service.
    ssh $sshDeployTarget "sudo systemctl enable $($serviceName)"
    if (!$?) { Exit $LASTEXITCODE }

    ssh $sshDeployTarget "sudo systemctl start $($serviceName)"
    if (!$?) { Exit $LASTEXITCODE }

    # Check the status and logs of the service.
    ssh $sshDeployTarget "sudo systemctl status $($serviceName)"

    # To check logs, run the following.
    # journalctl -fu switcherpi.service --since today
}
finally {
    # Return to original directory
    Set-Location $currentDir
}
