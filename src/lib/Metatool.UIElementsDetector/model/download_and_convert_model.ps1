<#
.SYNOPSIS
    Download a YOLO model and convert to ONNX for YoloSharp.

.DESCRIPTION
    Uses uv to manage Python environment and dependencies, then converts a YOLO
    model to ONNX format for use with YoloSharp in C#.

    Supports two model sources:
      - OmniParser icon_detect - fine-tuned for UI element detection
        https://huggingface.co/microsoft/OmniParser-v2.0
      - Stock Ultralytics YOLO models (e.g. yolo26n) - general object detection
        https://github.com/ultralytics/ultralytics

    When run without -Model, enters interactive mode: queries the latest
    available models from Ultralytics GitHub releases and lets you pick one.

    Prerequisites: uv (https://docs.astral.sh/uv/)
      Install: irm https://astral.sh/uv/install.ps1 | iex

    YoloSharp: https://github.com/dme-compunet/YoloSharp

.EXAMPLE
    # Interactive mode - browse and select from latest models
    .\download_and_convert_model.ps1

    # List available models without converting
    .\download_and_convert_model.ps1 -List

    # OmniParser (UI element detection)
    .\download_and_convert_model.ps1 -Model omniparser

    # Stock YOLO26n (general object detection)
    .\download_and_convert_model.ps1 -Model yolo26n

    # Custom image size
    .\download_and_convert_model.ps1 -Model yolo26s -ImgSize 640
#>

param(
    [string]$Model,
    [int]$ImgSize = 640,
    [string]$ModelRepo = "microsoft/OmniParser-v2.0",
    [switch]$List
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Ensure uv is available
if (-not (Get-Command uv -ErrorAction SilentlyContinue)) {
    Write-Error "uv is not installed. Install it with: irm https://astral.sh/uv/install.ps1 | iex"
    exit 1
}

# Build arguments for the Python script
$PyScript = Join-Path $ScriptDir "download_and_convert_model.py"
$PyArgs = @("--model-repo", $ModelRepo, "--imgsz", $ImgSize)

if ($List) {
    $PyArgs += "--list"
} elseif ($Model) {
    $PyArgs += @("--model", $Model)
}

# Run via uv - automatically manages venv and dependencies
Write-Host "Running model download and conversion (uv handles Python + deps)..."
uv run --project $ScriptDir $PyScript @PyArgs

if (-not $List) {
    Write-Host "`nDone! ONNX model is ready."
}
