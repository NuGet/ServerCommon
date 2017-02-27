### Constants ###
$HttpFileEtagCacheFilePath = "$PSScriptRoot\HttpFileEtagCache.json"

Function Get-HttpFile {
    Param (
        [string] $sourceUri,
        [string] $destinationFilePath,
        [string] $message
    )

    $sourceEtag = Get-SourceHttpFileEtag -sourceUri $sourceUri
    $localEtag = Get-LocalHttpFileEtag -filePath $destinationFilePath

    If ([string]::IsNullOrEmpty($sourceEtag) -Or $localEtag -Ne $sourceEtag) {
        If ([string]::IsNullOrEmpty($message)) {
            $message = "Downloading $sourceUri"
        }

        If (Get-Command -Name Trace-Log -CommandType Function -ErrorAction SilentlyContinue) {
            Trace-Log $message
        } Else {
            Write-Host $message
        }

        Invoke-WebRequest -UseBasicParsing -Uri $sourceUri -Method Get -OutFile $destinationFilePath

        If (-Not [string]::IsNullOrEmpty($sourceEtag)) {
            Set-LocalHttpFileEtag -filePath $destinationFilePath -etag $sourceEtag
        }
    }
}

Function Get-SourceHttpFileEtag {
    Param (
        [string] $sourceUri
    )

    $headers = Invoke-WebRequest -UseBasicParsing -Uri $sourceUri -Method Head | Select -ExpandProperty Headers

    If ($headers.ContainsKey("ETag")) {
        Return $headers.Item("ETag")
    }

    Return $null
}

Function Get-LocalHttpFileEtag {
    Param (
        [string] $filePath
    )

    $cache = Get-HttpFileEtagCache

    Return $cache.Get_Item($filePath)
}

Function Set-LocalHttpFileEtag {
    Param (
        [string] $filePath,
        [string] $etag
    )

    $cache = Get-HttpFileEtagCache

    $cache.Remove($filePath)
    $cache.Add($filePath, $etag)

    ConvertTo-Json -InputObject $cache | Set-Content -Path $HttpFileEtagCacheFilePath
}

Function Get-HttpFileEtagCache() {
    $cache = @{}

    If (Test-Path $HttpFileEtagCacheFilePath) {
        $json = Get-Content -Path $HttpFileEtagCacheFilePath -Raw
        $customObject = ConvertFrom-Json -InputObject $json
        $properties = $customObject | Get-Member -MemberType NoteProperty | Select -Property Name

        ForEach ($property In $properties) {
            $cache.Add($property.Name, $customObject."$($property.Name)")
        }
    }

    Return $cache
}