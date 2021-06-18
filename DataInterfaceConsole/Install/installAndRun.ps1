$ErrorActionPreference = 'SilentlyContinue'
dotnet --list-runtimes | findstr /C:"Microsoft.NETCore.App 3.1"
$ErrorActionPreference = 'Continue'

If ($lastExitCode -eq "0") {
    Write-Host ".NETCore 3.1 is already installed."
}
else
{
    $confirmation = Read-Host ".NETCore 3.1 appears to be not installed yet. Install it now? [y/n]"
	if ($confirmation -eq 'y') {
		Write-Host "Running installer..."
		$dlUri = "https://download.visualstudio.microsoft.com/download/pr/7cea63ad-1e76-41f0-a54a-eacb48fec749/87c339835cd7647c0fee3f14820cd909/windowsdesktop-runtime-3.1.16-win-x64.exe"
		$outpath = "dotnetinstaller31.exe"
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