[CmdletBinding()]
param(
  [string]$Version = (Get-Date -Format 'yyyy.MM.dd'),
  [switch]$NoZip
)

# The standalone variant doesn't require users to install .NET. The downside is it's very large (150MB-ish) :(
# I am not really sure how savvy users will be, so I've decided to publish both the standalone variant and
# the one that requires them to install .NET separately.

# Opting to include the console app as part of the output, rather than its own zip file.
# Not sure if anyone will use it but it's been very helpful for testing.

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$publish = Join-Path $root 'publish'
$projects = @(
  (Join-Path $root 'src\SkyblivionVoicesRemastered.App\SkyblivionVoicesRemastered.App.csproj'),
  (Join-Path $root 'src\SkyblivionVoicesRemastered.Console\SkyblivionVoicesRemastered.Console.csproj')
)
$flavors = @('Standalone', 'FrameworkDependent')

if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }

foreach ($flavor in $flavors) {
  # self-contained and framework-dependent publishes share obj\Release\net10.0\win-x64\ and contaminate each other
  # (a framework-dependent exe built on self-contained intermediates crashes at startup)\
  Get-ChildItem (Join-Path $root 'src') -Directory | ForEach-Object {
    $obj = Join-Path $_.FullName 'obj'
    if (Test-Path $obj) {
     Remove-Item $obj -Recurse -Force
    }
  }
  foreach ($project in $projects) {
    Write-Host "Publishing $(Split-Path $project -Leaf) [$flavor]..."
    dotnet publish $project -p:PublishProfile=$flavor -p:Version=$Version -nologo -v q
    if ($LASTEXITCODE -ne 0) {
      throw "dotnet publish failed for $project ($flavor)"
    }
  }
}

foreach ($dir in @('standalone', 'framework-dependent')) {
  Copy-Item (Join-Path $root 'README.md') (Join-Path $publish $dir) -Force
  Get-ChildItem (Join-Path $publish $dir) -Filter *.pdb | Remove-Item -Force
}

Write-Host ''
Write-Host 'Output:'
foreach ($dir in @('standalone', 'framework-dependent')) {
  Get-ChildItem (Join-Path $publish $dir) -File | ForEach-Object {
    '  {0,-22} {1,-45} {2,8:N1} MB' -f $dir, $_.Name, ($_.Length / 1MB)
  }
}

if (-not $NoZip) {
  Compress-Archive -Path (Join-Path $publish 'standalone\*') -DestinationPath (Join-Path $publish "SkyblivionVoicesRemastered-$Version-standalone.zip") -Force
  Compress-Archive -Path (Join-Path $publish 'framework-dependent\*') -DestinationPath (Join-Path $publish "SkyblivionVoicesRemastered-$Version-requires-dotnet10.zip") -Force
  Write-Host ''
  Get-ChildItem $publish -Filter *.zip | ForEach-Object { '  {0,-68} {1,8:N1} MB' -f $_.Name, ($_.Length / 1MB) }
}
