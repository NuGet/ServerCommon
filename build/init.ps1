[CmdletBinding()]
param(
    [string]$Branch
)

# This file is downloaded to "build/init.ps1" so use the parent folder as the root
$NuGetClientRoot = Split-Path -Path $PSScriptRoot -Parent

# Download common.ps1 and other tools used by this build script
$RootGitHubApiUri = "https://api.github.com/repos/NuGet/ServerCommon/contents"

if ($Branch) {
    $Ref = '?ref=' + $Branch
} else {
    $Ref = ''
}

Function Download-Folder {
	[CmdletBinding()]
	param(
		[string]$Path
	)
	
	if (-not (Test-Path $Path)) {
		New-Item -Path $Path -ItemType "directory"
	}
	
    $FolderUri = Join-Path $RootGitHubApiUri "$Path$Ref"
	Write-Host $FolderUri
	wget $FolderUri | ConvertFrom-Json | foreach {
		$FilePath = $_.path
		if ($_.type -eq "file") {
			$DownloadUrl = $_.download_url
			Write-Host $DownloadUrl
			wget -Uri $DownloadUrl -OutFile (Join-Path $NuGetClientRoot $FilePath)
		} elseif ($_.type -eq "dir") {
			Download-Folder -Path $FilePath
		}
	}
}

$FoldersToDownload = "build", "tools"
foreach ($Folder in $FoldersToDownload) {
	Download-Folder -Path $Folder
}

# Run common.ps1
. "$NuGetClientRoot\build\common.ps1"