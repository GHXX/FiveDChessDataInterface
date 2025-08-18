$ErrorActionPreference = 'SilentlyContinue'
dotnet --list-runtimes | findstr /C:"Microsoft.NETCore.App 8.0"
$ErrorActionPreference = 'Continue'

If ($lastExitCode -eq "0") {
    Write-Host ".NETCore 8.0 is already installed."
}
else
{
    $confirmation = Read-Host ".NETCore 3.1 appears to be not installed yet. Install it now? [y/n]"
	if ($confirmation -eq 'y') {
		Write-Host "Running installer..."
		$dlUri = "https://builds.dotnet.microsoft.com/dotnet/Runtime/8.0.19/dotnet-runtime-8.0.19-win-x64.exe"
		$outpath = "dotnetinstaller80.exe"
		Invoke-Webrequest -Uri $dlUri -OutFile $outpath
		&cmd /c "$outpath /install /quiet /norestart"
	}
	else
	{
		exit
	}
}

Start-Process "DataInterfaceConsole.exe"


Write-Host "Exiting in 5 seconds..."
Start-Sleep -Seconds 5