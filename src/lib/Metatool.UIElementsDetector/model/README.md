# Model Download & Conversion

Scripts to download a YOLO `.pt` model and convert it to `.onnx` for use with [YoloSharp](https://github.com/dme-compunet/YoloSharp) in the `Metatool.UIElementsDetector` C# project.

## Supported Models

| Model | Source | Use Case |
|-------|--------|----------|
| `omniparser` | [microsoft/OmniParser-v2.0](https://huggingface.co/microsoft/OmniParser-v2.0) | UI element detection (buttons, icons, inputs) |
| Stock YOLO models (`yolo26n`, `yolo11n`, `yolov8n`, etc.) | [Ultralytics](https://github.com/ultralytics/ultralytics) | General object detection (COCO) |

> **Note:** The OmniParser model is fine-tuned for detecting UI elements on screenshots.
> Stock YOLO models are trained on the COCO dataset (people, cars, animals, etc.) and
> would need fine-tuning to detect UI elements.

The script automatically queries the latest available models from the [Ultralytics GitHub releases](https://github.com/ultralytics/assets/releases/latest), so you always see the most up-to-date model list including Detection, Segmentation, Classification, Pose Estimation, and OBB variants.

## Prerequisites

- Python 3.10+

## Quick Start (PowerShell)

The `.ps1` script handles venv creation and dependency installation automatically:

```powershell
# Interactive mode - browse and select from latest available models
.\download_and_convert_model.ps1

# List available models without converting
.\download_and_convert_model.ps1 -List

# OmniParser icon_detect (UI element detection)
.\download_and_convert_model.ps1 -Model omniparser

# Stock YOLO26 nano
.\download_and_convert_model.ps1 -Model yolo26n

# Custom image size
.\download_and_convert_model.ps1 -Model yolo26s -ImgSize 640
```

## Manual Usage (Python)

```bash
# Install dependencies
pip install ultralytics huggingface_hub onnxslim

# Interactive mode - queries latest models and lets you pick
python download_and_convert_model.py

# List available models and exit (no conversion)
python download_and_convert_model.py --list

# OmniParser icon_detect
python download_and_convert_model.py --model omniparser

# Stock YOLO26n (auto-downloads from ultralytics)
python download_and_convert_model.py --model yolo26n

# Stock YOLO26s with custom image size
python download_and_convert_model.py --model yolo26s --imgsz 640

# Convert a local .pt file
python download_and_convert_model.py --model /path/to/custom_model.pt

# Custom output path
python download_and_convert_model.py --output /path/to/output.onnx
```

## Parameters

### PowerShell (`download_and_convert_model.ps1`)

| Parameter | Default | Description |
|-----------|---------|-------------|
| `-Model` | *(interactive)* | Model to convert: `omniparser`, a stock model name (e.g. `yolo26n`), or omit for interactive selection |
| `-ImgSize` | `640` | Input image size for ONNX export |
| `-ModelRepo` | `microsoft/OmniParser-v2.0` | Hugging Face repository (only used with `omniparser`) |
| `-List` | | List available models and exit (no conversion) |

### Python (`download_and_convert_model.py`)

| Parameter | Default | Description |
|-----------|---------|-------------|
| `--model` | *(interactive)* | Model to convert: `omniparser`, a stock model name, a local `.pt` path, or omit for interactive selection |
| `--model-repo` | `microsoft/OmniParser-v2.0` | Hugging Face repository (only used with `omniparser`) |
| `--imgsz` | `640` | Input image size for ONNX export |
| `--output` | `../icon_detect.onnx` | Output ONNX file path |
| `--download-dir` | `weights/` | Directory for downloaded `.pt` files |
| `--list` | | List available models and exit (no conversion) |

## Output

The converted `.onnx` file is placed in the parent directory (`Metatool.UIElementsDetector/icon_detect.onnx`) by default, where the C# project expects it.

## Directory Structure After Running

```
model/
  download_and_convert_model.py
  download_and_convert_model.ps1
  README.md
  weights/          # downloaded .pt files (gitignored)
    icon_detect/
      model.pt
      model.yaml
      train_args.yaml
```

## References

- [YoloSharp](https://github.com/dme-compunet/YoloSharp) - C# YOLO inference library (supports YOLOv8, YOLO11, YOLO26)
- [Ultralytics](https://github.com/ultralytics/ultralytics) - Python YOLO framework and ONNX export
- [OmniParser-v2.0](https://huggingface.co/microsoft/OmniParser-v2.0) - Microsoft's UI element detection model
