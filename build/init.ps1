[CmdletBinding()]
param(
    [string]$Branch = 'main'
)

# This file is downloaded to "build/init.ps1" so use the parent folder as the root
$NuGetClientRoot = Split-Path -Path $PSScriptRoot -Parent
$ServerCommonRoot = Join-Path $NuGetClientRoot "\ServerCommon";

Function Get-BuildTools {
    param(
        [string]$Branch
    )

    if (-not (Test-Path $ServerCommonRoot))
    {
        git clone -b $Branch https://github.com/NuGet/ServerCommon.git
    }
    Set-Location $ServerCommonRoot
    $BranchCommit = git rev-parse "origin/$Branch"

    Function Get-Folder {
        [CmdletBinding()]
        param(
            [string]$Path
        )
        # Create directory if not exists
        $DirectoryPath = (Join-Path $NuGetClientRoot $Path)
        if (-not (Test-Path $DirectoryPath)) {
            New-Item -Path $DirectoryPath -ItemType "directory"
        }

        # Verifies if marker file on the directory is up to date
        $MarkerFile = Join-Path $DirectoryPath ".marker"
        if (Test-Path $MarkerFile) {
            $content = Get-Content $MarkerFile
            if ($content -eq $BranchCommit) {
                Write-Host "Build tools directory '$Path' is already at '$Branch'."
                return;
            }
        }
        
        # Recursively creates the inner folders
        $FolderUri = Join-Path $ServerCommonRoot $Path
        $InnerDirectories = Get-ChildItem -Path $FolderUri -Directory
        foreach ($InnerDirectory in $InnerDirectories)
        {
            $InnerDirectoryPath = ($InnerDirectory.FullName).Replace("$ServerCommonRoot", "")
            Get-Folder -Path $InnerDirectoryPath
        }

        # Gets all files from current repository directory and moves them to root directory folder
        $FileDirectory = Join-Path $NuGetClientRoot $Path
        $FilesToMove = Get-ChildItem -Path $FolderUri -File
        Foreach ($File in $FilesToMove)
        {
            if (-not (Test-Path (Join-Path $FileDirectory $File)))
            {
                $File | Move-Item -Destination $FileDirectory
            }
            else
            {
                Write-Host "File $File Already created"
            }
        }

        # Creates the marker file for the current directory
        $BranchCommit | Out-File $MarkerFile
    }

    $FoldersToMove = "build", "tools"
    foreach ($Folder in $FoldersToMove) {
        Get-Folder -Path $Folder
    }
}

Get-BuildTools -Branch $Branch
Set-Location $NuGetClientRoot
Remove-Item -Path $ServerCommonRoot -Recurse -Force

# Run common.ps1
. "$NuGetClientRoot\build\common.ps1"