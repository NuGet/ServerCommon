[CmdletBinding()]
param(
    [string]$BuildBranch
)

# This file is downloaded to "build/init.ps1" so use the parent folder as the root
$NuGetRepoRoot = Split-Path -Path $PSScriptRoot -Parent

. "$PSScriptRoot\HttpFileCaching.ps1"

Function Get-BuildTools {
    param(
        [string]$Branch
    )

    # Download common.ps1 and other tools used by this build script
    $RootGitHubApiUri = "https://api.github.com/repos/NuGet/ServerCommon/contents"

    if ($Branch) {
        $Ref = '?ref=' + $Branch
    } else {
        $Ref = ''
    }

    Function Get-Folder {
        [CmdletBinding()]
        param(
            [string]$Path
        )

        $DirectoryPath = (Join-Path $NuGetRepoRoot $FilePath)
        if (-not (Test-Path $DirectoryPath)) {
            New-Item -Path $DirectoryPath -ItemType "directory"
        }

        $FolderUri = "$RootGitHubApiUri/$Path$Ref"
        Write-Host "Downloading files from $FolderUri"
        $Files = wget -UseBasicParsing $FolderUri | ConvertFrom-Json
        Foreach ($File in $Files) {
            $FilePath = $File.path
            if ($File.type -eq "file") {
                $DownloadUrl = $File.download_url
                Get-HttpFile -sourceUri $DownloadUrl -destinationFilePath (Join-Path $NuGetRepoRoot $FilePath) -message "Downloading file at $DownloadUrl"
            } elseif ($File.type -eq "dir") {
                Get-Folder -Path $FilePath
            }
        }
    }

    $FoldersToDownload = "build", "tools"
    foreach ($Folder in $FoldersToDownload) {
        Get-Folder -Path $Folder
    }
}

Get-BuildTools -Branch $BuildBranch

# Run common.ps1
. "$NuGetRepoRoot\build\common.ps1"