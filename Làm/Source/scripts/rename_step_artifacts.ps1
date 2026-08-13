# Rename Step### artifacts → domain names (153–167). Skips EF Migrations folder.
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")

# Order: longest / most specific first
$maps = @(
  @{ Old = "SysStep153"; New = "SysSsoFieldConfigPush"; OldKebab = "sys-step153"; NewKebab = "sys-sso-field-config-push"; OldRoute = $null; NewRoute = $null; OldPage = $null; NewPage = $null }
  @{ Old = "SysStep154"; New = "SysNotifScanExportIp"; OldKebab = "sys-step154"; NewKebab = "sys-notif-scan-export-ip"; OldRoute = $null; NewRoute = $null; OldPage = $null; NewPage = $null }
  @{ Old = "SysStep155"; New = "SysThemeRoleHomeMsg"; OldKebab = "sys-step155"; NewKebab = "sys-theme-role-home-msg"; OldRoute = $null; NewRoute = $null; OldPage = $null; NewPage = $null }
  @{ Old = "HrmStep156"; New = "HrmOrgDepartment"; OldKebab = "hrm-step156"; NewKebab = "hrm-org-department"; OldRoute = "api/hrm/step156"; NewRoute = "api/hrm/org-departments"; OldPage = "hrm/step156"; NewPage = "hrm/org-departments" }
  @{ Old = "HrmStep157"; New = "HrmSkillQualification"; OldKebab = "hrm-step157"; NewKebab = "hrm-skill-qualification"; OldRoute = "api/hrm/step157"; NewRoute = "api/hrm/skill-qualifications"; OldPage = "hrm/step157"; NewPage = "hrm/skill-qualifications" }
  @{ Old = "HrmStep158"; New = "HrmShiftImport"; OldKebab = "hrm-step158"; NewKebab = "hrm-shift-import"; OldRoute = "api/hrm/step158"; NewRoute = "api/hrm/shift-import"; OldPage = "hrm/step158"; NewPage = "hrm/shift-import" }
  @{ Old = "HrmStep159"; New = "HrmEvalTemplate"; OldKebab = "hrm-step159"; NewKebab = "hrm-eval-template"; OldRoute = "api/hrm/step159"; NewRoute = "api/hrm/eval-templates"; OldPage = "hrm/step159"; NewPage = "hrm/eval-templates" }
  @{ Old = "Step167"; New = "LmsAiAssist"; OldKebab = "step167"; NewKebab = "lms-ai-assist"; OldRoute = "api/step167"; NewRoute = "api/lms/ai-assist"; OldPage = "step167"; NewPage = "lms/ai-assist" }
  @{ Old = "Step166"; New = "LmsTrainingReports"; OldKebab = "step166"; NewKebab = "lms-training-reports"; OldRoute = "api/step166"; NewRoute = "api/lms/training-reports"; OldPage = "step166"; NewPage = "lms/training-reports" }
  @{ Old = "Step165"; New = "LmsPathTracking"; OldKebab = "step165"; NewKebab = "lms-path-tracking"; OldRoute = "api/step165"; NewRoute = "api/lms/path-tracking"; OldPage = "step165"; NewPage = "lms/path-tracking" }
  @{ Old = "Step164"; New = "LmsContentCompliance"; OldKebab = "step164"; NewKebab = "lms-content-compliance"; OldRoute = "api/step164"; NewRoute = "api/lms/content-compliance"; OldPage = "step164"; NewPage = "lms/content-compliance" }
  @{ Old = "Step163"; New = "LmsCertSyncOps"; OldKebab = "step163"; NewKebab = "lms-cert-sync"; OldRoute = "api/step163"; NewRoute = "api/lms/cert-sync"; OldPage = "step163"; NewPage = "lms/cert-sync" }
  @{ Old = "Step162"; New = "LmsEngageCert"; OldKebab = "step162"; NewKebab = "lms-engage-cert"; OldRoute = "api/step162"; NewRoute = "api/lms/engage-cert"; OldPage = "step162"; NewPage = "lms/engage-cert" }
  @{ Old = "Step161"; New = "LmsExamMentoring"; OldKebab = "step161"; NewKebab = "lms-exam-mentoring"; OldRoute = "api/step161"; NewRoute = "api/lms/exam-mentoring"; OldPage = "step161"; NewPage = "lms/exam-mentoring" }
  @{ Old = "Step160"; New = "HrmLmsEvalCatalog"; OldKebab = "step160"; NewKebab = "hrm-lms-eval-catalog"; OldRoute = "api/step160"; NewRoute = "api/hrm-lms/eval-catalog"; OldPage = "step160"; NewPage = "lms/eval-catalog" }
)

$ext = @("*.cs", "*.ts", "*.tsx", "*.mts", "*.json", "*.md")
$skipDir = [regex]'(?i)[\\/](bin|obj|node_modules|\.next|Migrations)[\\/]'

function Get-TextFiles {
  Get-ChildItem -Path $root -Recurse -File -Include $ext |
    Where-Object { $_.FullName -notmatch $skipDir }
}

Write-Host "Phase 1: content replace..."
$files = Get-TextFiles
foreach ($f in $files) {
  $text = [IO.File]::ReadAllText($f.FullName)
  $orig = $text
  foreach ($m in $maps) {
    # Kebab/class first so page path replace không phá import helper (step167-helpers → lms/ai-assist-helpers).
    if ($m.OldKebab) { $text = $text.Replace($m.OldKebab, $m.NewKebab) }
    $text = $text.Replace($m.Old, $m.New)
    if ($m.OldRoute) { $text = $text.Replace($m.OldRoute, $m.NewRoute) }
    if ($m.OldPage) {
      $text = $text.Replace("/$($m.OldPage)", "/$($m.NewPage)")
    }
  }
  # Fix double-prefix if any IHrmLms from I + HrmLms...
  if ($text -ne $orig) {
    [IO.File]::WriteAllText($f.FullName, $text)
    Write-Host "  updated $($f.FullName.Substring($root.Path.Length + 1))"
  }
}

Write-Host "Phase 2: rename files (longest first)..."
$allFiles = Get-ChildItem -Path $root -Recurse -File |
  Where-Object { $_.FullName -notmatch $skipDir }

foreach ($m in $maps) {
  $candidates = @($allFiles | Where-Object {
    $_.Name -like "*$($m.Old)*" -or ($m.OldKebab -and $_.Name -like "*$($m.OldKebab)*")
  })
  foreach ($f in $candidates) {
    $newName = $f.Name.Replace($m.Old, $m.New)
    if ($m.OldKebab) { $newName = $newName.Replace($m.OldKebab, $m.NewKebab) }
    if ($newName -eq $f.Name) { continue }
    $dest = Join-Path $f.DirectoryName $newName
    if (Test-Path $dest) { Write-Warning "skip exists: $dest"; continue }
    Rename-Item -LiteralPath $f.FullName -NewName $newName
    Write-Host "  file $($f.Name) -> $newName"
  }
}

Write-Host "Phase 3: rename directories (pages)..."
$pageDirs = @(
  @{ Old = "hrm\step156"; New = "hrm\org-departments" }
  @{ Old = "hrm\step157"; New = "hrm\skill-qualifications" }
  @{ Old = "hrm\step158"; New = "hrm\shift-import" }
  @{ Old = "hrm\step159"; New = "hrm\eval-templates" }
  @{ Old = "step160"; New = "lms\eval-catalog" }
  @{ Old = "step161"; New = "lms\exam-mentoring" }
  @{ Old = "step162"; New = "lms\engage-cert" }
  @{ Old = "step163"; New = "lms\cert-sync" }
  @{ Old = "step164"; New = "lms\content-compliance" }
  @{ Old = "step165"; New = "lms\path-tracking" }
  @{ Old = "step166"; New = "lms\training-reports" }
  @{ Old = "step167"; New = "lms\ai-assist" }
)

$dashRoot = Join-Path $root "frontend\src\app\(dashboard)"
foreach ($p in $pageDirs) {
  $src = Join-Path $dashRoot $p.Old
  $dst = Join-Path $dashRoot $p.New
  if (-not (Test-Path -LiteralPath $src)) { continue }
  $parent = Split-Path $dst -Parent
  if (-not (Test-Path -LiteralPath $parent)) {
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
  }
  if (Test-Path -LiteralPath $dst) {
    # move contents
    Get-ChildItem -LiteralPath $src | Move-Item -Destination $dst -Force
    Remove-Item -LiteralPath $src -Recurse -Force
  } else {
    Move-Item -LiteralPath $src -Destination $dst
  }
  Write-Host "  dir $($p.Old) -> $($p.New)"
}

Write-Host "Done."
