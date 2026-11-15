$base = Join-Path $PSScriptRoot "..\agents-base"
if (-not (Test-Path $base)) {
    Write-Warning "Sibling directory ../agents-base not found; using repository-embedded .agents and AGENTS.md."
    exit 0
}
if (-not (Test-Path "$PSScriptRoot\.agents")) {
    cmd /c mklink /J "$PSScriptRoot\.agents" "$base\.agents"
}
if (-not (Test-Path "$PSScriptRoot\AGENTS.md")) {
    cmd /c mklink /H "$PSScriptRoot\AGENTS.md" "$base\AGENTS.md"
}
Write-Host "Agent configuration verified."
