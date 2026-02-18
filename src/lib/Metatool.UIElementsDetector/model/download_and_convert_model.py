"""
Download a YOLO model and convert it to ONNX format for use with YoloSharp in C#.

Supports two model sources:
  1. OmniParser icon_detect - fine-tuned for UI element detection
     https://huggingface.co/microsoft/OmniParser-v2.0
  2. Stock Ultralytics YOLO models - general object detection
     https://github.com/ultralytics/ultralytics

YoloSharp: https://github.com/dme-compunet/YoloSharp
Export ref: from ultralytics import YOLO; YOLO('model.pt').export(format='onnx')

Prerequisites:
    pip install ultralytics huggingface_hub

Usage:
    # Interactive mode - queries latest models and lets you pick
    python download_and_convert_model.py

    # Non-interactive - specify model directly
    python download_and_convert_model.py --model yolo26n
    python download_and_convert_model.py --model omniparser
"""

import argparse
import shutil
import urllib.request
import json
from pathlib import Path

# Use the OS certificate store (trusts corporate proxy certs on Windows)
try:
    import truststore
    truststore.inject_into_ssl()
except ImportError:
    pass


def fetch_ultralytics_models() -> list[dict]:
    """Fetch available .pt models from the latest ultralytics GitHub release."""
    url = "https://api.github.com/repos/ultralytics/assets/releases/latest"
    req = urllib.request.Request(url, headers={"Accept": "application/vnd.github.v3+json"})
    try:
        with urllib.request.urlopen(req, timeout=10) as resp:
            data = json.loads(resp.read().decode())
    except Exception as e:
        print(f"Warning: could not fetch models from GitHub ({e})")
        return []

    models = []
    for asset in data.get("assets", []):
        name = asset["name"]
        if name.endswith(".pt"):
            models.append({
                "name": name.removesuffix(".pt"),
                "file": name,
                "size_mb": round(asset["size"] / 1024 / 1024, 1),
                "url": asset["browser_download_url"],
            })

    # Sort: detection models first, then by family and size
    size_order = {"n": 0, "s": 1, "m": 2, "l": 3, "x": 4}

    def sort_key(m):
        n = m["name"]
        # task suffix: no suffix = detect (first), then -seg, -cls, -pose, -obb
        task_order = {"": 0, "-seg": 1, "-cls": 2, "-pose": 3, "-obb": 4, "-world": 5, "-worldv2": 6}
        task = ""
        for t in sorted(task_order.keys(), key=len, reverse=True):
            if t and n.endswith(t):
                task = t
                break
        # extract size letter (last char before task suffix)
        base = n.removesuffix(task)
        sz = base[-1] if base and base[-1] in size_order else "z"
        return (task_order.get(task, 9), base, size_order.get(sz, 9))

    models.sort(key=sort_key)
    return models


def group_models(models: list[dict]) -> dict[str, list[dict]]:
    """Group models by task type for display."""
    groups = {}
    for m in models:
        name = m["name"]
        if "-seg" in name:
            task = "Segmentation"
        elif "-cls" in name:
            task = "Classification"
        elif "-pose" in name:
            task = "Pose Estimation"
        elif "-obb" in name:
            task = "Oriented Bounding Box"
        elif "-world" in name:
            task = "Open-Vocabulary (YOLO-World)"
        else:
            task = "Detection"
        groups.setdefault(task, []).append(m)
    return groups


def interactive_select(models: list[dict]) -> str:
    """Display available models and let the user pick one."""
    print("\n=== Available Models ===\n")

    # OmniParser as option 0
    print("  [0]  omniparser (OmniParser icon_detect - UI element detection)")
    print("       Source: huggingface.co/microsoft/OmniParser-v2.0\n")

    # Group ultralytics models by task
    groups = group_models(models)
    idx = 1
    index_map = {0: "omniparser"}

    for task, task_models in groups.items():
        print(f"  --- {task} ---")
        for m in task_models:
            print(f"  [{idx:>2}]  {m['name']:<28} {m['size_mb']:>6.1f} MB")
            index_map[idx] = m["name"]
            idx += 1
        print()

    # Prompt
    while True:
        try:
            choice = input(f"Select model [0-{idx - 1}] (default: 0): ").strip()
            if choice == "":
                return "omniparser"
            n = int(choice)
            if n in index_map:
                selected = index_map[n]
                print(f"\nSelected: {selected}")
                return selected
        except (ValueError, EOFError):
            pass
        print(f"  Please enter a number between 0 and {idx - 1}")


def download_omniparser(repo_id: str, download_dir: Path) -> Path:
    """Download OmniParser icon_detect model.pt from Hugging Face."""
    from huggingface_hub import hf_hub_download

    print(f"Downloading icon_detect/model.pt from {repo_id}...")
    pt_path = hf_hub_download(
        repo_id=repo_id,
        filename="icon_detect/model.pt",
        local_dir=str(download_dir),
    )
    pt_path = Path(pt_path)
    print(f"Downloaded to: {pt_path}")

    for extra in ["icon_detect/model.yaml", "icon_detect/train_args.yaml"]:
        try:
            hf_hub_download(repo_id=repo_id, filename=extra, local_dir=str(download_dir))
            print(f"Downloaded: {extra}")
        except Exception:
            pass

    return pt_path


def load_stock_model(model_name: str) -> "YOLO":
    """Load a stock ultralytics model (auto-downloads .pt on first use)."""
    from ultralytics import YOLO

    pt_name = model_name if model_name.endswith(".pt") else f"{model_name}.pt"
    print(f"Loading stock model {pt_name} (auto-downloads if needed)...")
    return YOLO(pt_name)


def convert_to_onnx(pt_path_or_model, output_path: Path, imgsz: int) -> Path:
    """Convert a .pt model to .onnx using ultralytics."""
    from ultralytics import YOLO

    if isinstance(pt_path_or_model, (str, Path)):
        print(f"Loading model from {pt_path_or_model}...")
        model = YOLO(str(pt_path_or_model))
    else:
        model = pt_path_or_model

    print(f"Exporting to ONNX (imgsz={imgsz})...")
    onnx_path = Path(model.export(
        format="onnx",
        imgsz=imgsz,
        simplify=True,
    ))
    print(f"Exported to: {onnx_path}")

    if onnx_path.resolve() != output_path.resolve():
        shutil.copy2(onnx_path, output_path)
        print(f"Copied to: {output_path}")

    return output_path


def main():
    parser = argparse.ArgumentParser(
        description="Download a YOLO model and convert to ONNX for YoloSharp"
    )
    parser.add_argument(
        "--model",
        default=None,
        help=(
            "Model to use. 'omniparser' for OmniParser icon_detect, or a stock "
            "ultralytics model name like 'yolo26n', or a local .pt file path. "
            "If omitted, shows an interactive model picker."
        ),
    )
    parser.add_argument(
        "--model-repo",
        default="microsoft/OmniParser-v2.0",
        help="Hugging Face repo for OmniParser (default: microsoft/OmniParser-v2.0)",
    )
    parser.add_argument(
        "--imgsz",
        type=int,
        default=640,
        help="Input image size for ONNX export (default: 640)",
    )
    parser.add_argument(
        "--output",
        default=None,
        help="Output ONNX file path (default: ../icon_detect.onnx, i.e. project root)",
    )
    parser.add_argument(
        "--download-dir",
        default=None,
        help="Directory to download .pt model to (default: weights/ in this folder)",
    )
    parser.add_argument(
        "--list",
        action="store_true",
        help="List available models and exit (no conversion)",
    )
    args = parser.parse_args()

    script_dir = Path(__file__).parent
    project_dir = script_dir.parent
    output_path = Path(args.output) if args.output else project_dir / "icon_detect.onnx"
    download_dir = Path(args.download_dir) if args.download_dir else script_dir / "weights"
    download_dir.mkdir(parents=True, exist_ok=True)

    # Interactive mode: no --model specified, or --list
    if args.model is None or args.list:
        print("Fetching available models from GitHub (ultralytics/assets)...")
        models = fetch_ultralytics_models()
        if args.list:
            interactive_select(models)  # just display, then exit
            return
        model_choice = interactive_select(models)
    else:
        model_choice = args.model

    # Execute based on selection
    if model_choice.lower() == "omniparser":
        print("\n=== OmniParser icon_detect (UI element detection) ===")
        pt_path = download_omniparser(args.model_repo, download_dir)
        convert_to_onnx(pt_path, output_path, args.imgsz)
    elif Path(model_choice).exists():
        print(f"\n=== Local model: {model_choice} ===")
        convert_to_onnx(Path(model_choice), output_path, args.imgsz)
    else:
        print(f"\n=== Ultralytics model: {model_choice} ===")
        model = load_stock_model(model_choice)
        convert_to_onnx(model, output_path, args.imgsz)

    print(f"\nDone! ONNX model ready at: {output_path}")


if __name__ == "__main__":
    main()
