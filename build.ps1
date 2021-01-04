$ErrorActionPreference = 'Stop'

Write-Host 'dotnet SDK version'
dotnet --version

$net_tfm = 'net5.0'
$configuration = 'Release'
$output_dir = "$PSScriptRoot\Dawdler\bin\$configuration"
$proj_path = "$PSScriptRoot\Dawdler\Dawdler.csproj"

function Build-App
{
    Write-Host 'Building'

    $outdir = "$output_dir\$net_tfm"
    $publishDir = "$outdir\publish"

    Remove-Item $publishDir -Recurse -Force -Confirm:$false -ErrorAction Ignore

    dotnet publish -c $configuration -f $net_tfm $proj_path
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

Build-App
