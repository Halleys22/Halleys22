$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -eq 'Core') {
    & powershell.exe -NoProfile -Command {
        param($source, $target)
        Add-Type -AssemblyName System.Drawing
        Add-Type -Path $source -ReferencedAssemblies System.Drawing
        [BannerGenerator]::Build($target)
    } -Args (Join-Path $PSScriptRoot 'BannerGenerator.cs'), (Join-Path $PSScriptRoot 'assets')
    if ($LASTEXITCODE -ne 0) { throw 'Banner generation failed.' }
} else {
    Add-Type -AssemblyName System.Drawing
    Add-Type -Path (Join-Path $PSScriptRoot 'BannerGenerator.cs') -ReferencedAssemblies System.Drawing
    [BannerGenerator]::Build((Join-Path $PSScriptRoot 'assets'))
}

