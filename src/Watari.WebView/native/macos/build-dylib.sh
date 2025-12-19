#!/usr/bin/env bash
set -euo pipefail

ROOT=$(cd "$(dirname "$0")/.." && pwd)
SRC_APP="$ROOT/macos/app_bridge.m"
SRC_WEB="$ROOT/macos/webview_bridge.m"
OUT_APP="$ROOT/macos/libwkapp.dylib"
OUT_WEB="$ROOT/macos/libwkwebview.dylib"

echo "Building app bridge dylib from: $SRC_APP -> $OUT_APP"
clang -dynamiclib -o "$OUT_APP" "$SRC_APP" -framework Cocoa -fobjc-arc -Wall
echo "Built: $OUT_APP"

echo "Building webview bridge dylib from: $SRC_WEB -> $OUT_WEB"
clang -dynamiclib -o "$OUT_WEB" "$SRC_WEB" -framework Cocoa -framework WebKit -fobjc-arc -Wall
echo "Built: $OUT_WEB"
