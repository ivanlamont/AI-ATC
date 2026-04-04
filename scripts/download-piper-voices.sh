#!/usr/bin/env bash
# =============================================================================
# download-piper-voices.sh
#
# Pre-downloads all Piper TTS voice models used by AI-ATC into a target
# directory. Run this once before first use (or in a Kubernetes init container)
# so that voice synthesis doesn't trigger on-demand downloads at runtime.
#
# Usage:
#   ./scripts/download-piper-voices.sh [TARGET_DIR]
#
# TARGET_DIR defaults to ./piper-data (local dev) or /data (inside container).
# =============================================================================
set -euo pipefail

PIPER_BASE_URL="https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0"

# All voices used in AirlineVoiceMapper.cs — keep in sync!
VOICES=(
    # American English
    "en/en_US/ryan/high/en_US-ryan-high"
    "en/en_US/joe/medium/en_US-joe-medium"
    "en/en_US/bryce/medium/en_US-bryce-medium"
    "en/en_US/john/medium/en_US-john-medium"
    "en/en_US/lessac/high/en_US-lessac-high"
    "en/en_US/amy/medium/en_US-amy-medium"
    "en/en_US/kristin/medium/en_US-kristin-medium"
    "en/en_US/hfc_female/medium/en_US-hfc_female-medium"

    # British English
    "en/en_GB/alan/medium/en_GB-alan-medium"
    "en/en_GB/northern_english_male/medium/en_GB-northern_english_male-medium"
    "en/en_GB/cori/high/en_GB-cori-high"
    "en/en_GB/alba/medium/en_GB-alba-medium"

    # French
    "fr/fr_FR/tom/medium/fr_FR-tom-medium"
    "fr/fr_FR/siwis/medium/fr_FR-siwis-medium"

    # German
    "de/de_DE/thorsten/high/de_DE-thorsten-high"
    "de/de_DE/kerstin/low/de_DE-kerstin-low"

    # Italian
    "it/it_IT/riccardo/x_low/it_IT-riccardo-x_low"
    "it/it_IT/paola/medium/it_IT-paola-medium"

    # Spanish
    "es/es_ES/davefx/medium/es_ES-davefx-medium"
    "es/es_MX/claude/high/es_MX-claude-high"

    # Dutch
    "nl/nl_NL/pim/medium/nl_NL-pim-medium"
    "nl/nl_NL/mls/medium/nl_NL-mls-medium"

    # Russian
    "ru/ru_RU/denis/medium/ru_RU-denis-medium"
    "ru/ru_RU/irina/medium/ru_RU-irina-medium"

    # Polish
    "pl/pl_PL/darkman/medium/pl_PL-darkman-medium"
    "pl/pl_PL/gosia/medium/pl_PL-gosia-medium"

    # Norwegian / Scandinavian
    "no/no_NO/talesyntese/medium/no_NO-talesyntese-medium"
    "sv/sv_SE/lisa/medium/sv_SE-lisa-medium"
)

# Determine target directory
TARGET_DIR="${1:-./piper-data}"
mkdir -p "$TARGET_DIR"

echo "Downloading Piper TTS voices to: $TARGET_DIR"
echo "Total voices: ${#VOICES[@]}"
echo ""

DOWNLOADED=0
SKIPPED=0
FAILED=0

for voice_path in "${VOICES[@]}"; do
    voice_name=$(basename "$voice_path")
    onnx_file="${TARGET_DIR}/${voice_name}.onnx"
    json_file="${TARGET_DIR}/${voice_name}.onnx.json"

    # Skip if both files already exist
    if [[ -f "$onnx_file" && -f "$json_file" ]]; then
        echo "  [skip] ${voice_name} (already downloaded)"
        SKIPPED=$((SKIPPED + 1))
        continue
    fi

    echo "  [download] ${voice_name}..."

    onnx_url="${PIPER_BASE_URL}/${voice_path}.onnx"
    json_url="${PIPER_BASE_URL}/${voice_path}.onnx.json"

    if curl -sSfL -o "$onnx_file" "$onnx_url" && \
       curl -sSfL -o "$json_file" "$json_url"; then
        DOWNLOADED=$((DOWNLOADED + 1))
        size=$(du -sh "$onnx_file" | cut -f1)
        echo "           -> ${size}"
    else
        FAILED=$((FAILED + 1))
        echo "           -> FAILED"
        rm -f "$onnx_file" "$json_file"
    fi
done

echo ""
echo "Done: ${DOWNLOADED} downloaded, ${SKIPPED} skipped, ${FAILED} failed"
echo "Total size: $(du -sh "$TARGET_DIR" | cut -f1)"
