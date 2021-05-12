param([string]$rid = 'linux-musl-x64')
$ErrorActionPreference = 'Stop'

Write-Host 'dotnet SDK info'
dotnet --info

$net_tfm = 'net5.0'
$configuration = 'Release'
$output_dir = "$PSScriptRoot\Dawdler\bin\$configuration"
$proj_path = "$PSScriptRoot\Dawdler\Dawdler.csproj"

function New-App
{
    param([string]$rid)
    Write-Host 'Building'

    $outdir = "$output_dir\$net_tfm"
    $publishDir = "$outdir\publish"

    Remove-Item $publishDir -Recurse -Force -Confirm:$false -ErrorAction Ignore

    dotnet publish "$proj_path" -c $configuration -f $net_tfm -p:PublishSingleFile=true --self-contained true -p:PublishTrimmed=True -p:TrimMode=Link -r $rid
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

New-App $rid
