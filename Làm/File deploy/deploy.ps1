# Deploy BE / FE len MonsterASP (WebDeploy).
# Usage:
#   .\deploy.ps1              # ca BE + FE
#   .\deploy.ps1 -Target BE
#   .\deploy.ps1 -Target FE
#
# Can thiet: .NET SDK 8+, Node.js/npm, Web Deploy 3
# msdeploy: "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe"

param(
  [ValidateSet("All", "BE", "FE")]
  [string]$Target = "All"
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "..\..")
$LamDir = Resolve-Path (Join-Path $ScriptDir "..")
$MsDeploy = "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe"

function Read-PublishProfile([string]$path) {
  [xml]$xml = Get-Content -LiteralPath $path -Encoding UTF8
  $p = $xml.publishData.publishProfile
  [pscustomobject]@{
    PublishUrl = [string]$p.publishUrl
    Site       = [string]$p.msdeploySite
    UserName   = [string]$p.userName
    Password   = [string]$p.userPWD
    AppUrl     = [string]$p.destinationAppUrl
  }
}

function Invoke-MsDeploy([string]$source, [string]$destContentPath, $profile, [string[]]$extraArgs = @(), [int]$maxAttempts = 3) {
  if (-not (Test-Path $MsDeploy)) {
    throw "Khong tim thay msdeploy.exe. Cai Web Deploy 3 truoc."
  }
  $computerName = "https://$($profile.PublishUrl):8172/msdeploy.axd?site=$($profile.Site)"
  $dest = "contentPath=$destContentPath,ComputerName=$computerName,UserName=$($profile.UserName),Password=$($profile.Password),AuthType=Basic"
  $args = @(
    "-verb:sync",
    "-source:contentPath=$source",
    "-dest:$dest",
    "-allowUntrusted",
    "-enableRule:AppOffline"
  ) + $extraArgs

  for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
    & $MsDeploy @args
    if ($LASTEXITCODE -eq 0) { return }
    Write-Host "msdeploy attempt $attempt/$maxAttempts failed (exit $LASTEXITCODE) -> $destContentPath" -ForegroundColor Yellow
    if ($attempt -lt $maxAttempts) { Start-Sleep -Seconds (5 * $attempt) }
  }
  throw "msdeploy failed (exit $LASTEXITCODE) -> $destContentPath"
}

function Write-Utf8NoBom([string]$path, [string]$content) {
  $enc = New-Object System.Text.UTF8Encoding $false
  [System.IO.File]::WriteAllText($path, $content, $enc)
}

function Deploy-BE {
  Write-Host "`n=== DEPLOY BE ===" -ForegroundColor Cyan
  $profile = Read-PublishProfile (Join-Path $ScriptDir "BE.publishSettings")
  $proj = Join-Path $LamDir "Source\backend\src\Erp.Api\Erp.Api.csproj"
  $envFile = Join-Path $LamDir "Source\backend\src\Erp.Api\.env"
  $out = "C:\Temp\erp-api-publish"

  if (-not (Test-Path $proj)) { throw "Khong tim thay Erp.Api.csproj: $proj" }
  if (-not (Test-Path $envFile)) { throw "Thieu file .env: $envFile" }

  if (Test-Path $out) { Remove-Item $out -Recurse -Force }
  dotnet publish $proj -c Release -o $out --self-contained false
  if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

  Copy-Item $envFile (Join-Path $out ".env") -Force
  Remove-Item (Join-Path $out "Erp.Api.exe") -Force -ErrorAction SilentlyContinue

  $webConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\Erp.Api.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
      <httpErrors existingResponse="PassThrough" />
    </system.webServer>
  </location>
</configuration>
"@
  Write-Utf8NoBom (Join-Path $out "web.config") $webConfig
  New-Item -ItemType Directory -Path (Join-Path $out "logs") -Force | Out-Null
  Set-Content (Join-Path $out "logs\.gitkeep") "" -Encoding ASCII

  # wwwroot + site root (hosting MonsterASP)
  Invoke-MsDeploy $out "$($profile.Site)\wwwroot" $profile
  Invoke-MsDeploy $out $profile.Site $profile @("-skip:objectName=dirPath,absolutePath=wwwroot")

  Write-Host "BE OK -> https://pumerpapi.runasp.net" -ForegroundColor Green
}

function Deploy-FE {
  Write-Host "`n=== DEPLOY FE ===" -ForegroundColor Cyan
  $profile = Read-PublishProfile (Join-Path $ScriptDir "FE.publishSettings")
  $src = Join-Path $LamDir "Source\frontend"
  $buildRoot = "C:\Temp\erp-fe-build"
  $deploy = "C:\Temp\erp-fe-deploy"
  $apiUrl = "https://pumerpapi.runasp.net"

  if (-not (Test-Path $src)) { throw "Khong tim thay frontend: $src" }

  # Build o duong dan ASCII (tranh loi symlink/unicode)
  if (Test-Path $buildRoot) { Remove-Item $buildRoot -Recurse -Force }
  New-Item -ItemType Directory -Path $buildRoot -Force | Out-Null
  robocopy $src $buildRoot /E /XD node_modules .next out .git /NFL /NDL /NJH /NJS /nc /ns /np | Out-Null

  Push-Location $buildRoot
  try {
    if (-not (Test-Path "node_modules")) {
      npm install
      if ($LASTEXITCODE -ne 0) { throw "npm install failed" }
    }
    $env:NEXT_PUBLIC_API_URL = $apiUrl
    $env:NEXT_TELEMETRY_DISABLED = "1"
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "npm run build failed" }
  }
  finally {
    Pop-Location
  }

  if (Test-Path $deploy) { Remove-Item $deploy -Recurse -Force }
  New-Item -ItemType Directory -Path $deploy -Force | Out-Null
  Copy-Item "$buildRoot\.next\standalone\*" $deploy -Recurse -Force
  New-Item -ItemType Directory -Path "$deploy\.next\static" -Force | Out-Null
  Copy-Item "$buildRoot\.next\static\*" "$deploy\.next\static\" -Recurse -Force
  if (Test-Path "$buildRoot\public") {
    New-Item -ItemType Directory -Path "$deploy\public" -Force | Out-Null
    Copy-Item "$buildRoot\public\*" "$deploy\public\" -Recurse -Force
  }

  $webConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="httpPlatformHandler" path="*" verb="*" modules="httpPlatformHandler" resourceType="Unspecified" />
    </handlers>
    <httpPlatform processPath="node" arguments=".\server.js" startupTimeLimit="120" requestTimeout="00:10:00" stdoutLogEnabled="true" stdoutLogFile=".\logs\node" forwardWindowsAuthToken="false">
      <environmentVariables>
        <environmentVariable name="PORT" value="%HTTP_PLATFORM_PORT%" />
        <environmentVariable name="NODE_ENV" value="production" />
        <environmentVariable name="HOSTNAME" value="127.0.0.1" />
        <environmentVariable name="NEXT_TELEMETRY_DISABLED" value="1" />
        <environmentVariable name="NEXT_PUBLIC_API_URL" value="$apiUrl" />
      </environmentVariables>
    </httpPlatform>
    <httpErrors existingResponse="PassThrough" />
  </system.webServer>
</configuration>
"@
  Write-Utf8NoBom (Join-Path $deploy "web.config") $webConfig
  New-Item -ItemType Directory -Path (Join-Path $deploy "logs") -Force | Out-Null
  Set-Content (Join-Path $deploy "logs\.gitkeep") "" -Encoding ASCII

  # FE chay o site root (khong phai wwwroot)
  Invoke-MsDeploy $deploy $profile.Site $profile @("-skip:objectName=dirPath,absolutePath=wwwroot")

  Write-Host "FE OK -> https://pumerp.runasp.net" -ForegroundColor Green
  Write-Host "Nho Ctrl+F5 tren trinh duyet sau khi deploy FE." -ForegroundColor Yellow
}

Write-Host "Repo: $RepoRoot"
Write-Host "Target: $Target"

if ($Target -eq "All" -or $Target -eq "BE") { Deploy-BE }
if ($Target -eq "All" -or $Target -eq "FE") { Deploy-FE }

Write-Host "`nDone." -ForegroundColor Green
