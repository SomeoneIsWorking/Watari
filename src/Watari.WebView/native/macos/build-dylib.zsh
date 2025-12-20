#!/usr/bin/env zsh
set -euo pipefail

ROOT=$(cd "$(dirname "$0")" && pwd)

echo "Building macOS bridge dylibs in $ROOT"

pairs=(
    "application:-framework Cocoa"
    "window:-framework Cocoa"
    "webview:-framework Cocoa -framework WebKit"
)

for pair in $pairs; do
    library=${pair%%:*}
    flags=(${=pair#*:})
    echo "Building ${library} bridge dylib from: $library -> lib$library.dylib"
    clang -dynamiclib -o "$ROOT/lib$library.dylib" "$ROOT/${library}_bridge.m" "${flags[@]}" -fobjc-arc -Wall
    echo "Built: lib$library.dylib"
done
