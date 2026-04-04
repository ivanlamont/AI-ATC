#!/usr/bin/env bash
# Entrypoint for AIATC Piper TTS container.
# Runs both the Wyoming protocol server (port 10200) and the
# Flask HTTP API (port 5000) so the BFF can proxy TTS requests.
set -e

DATA_DIR="/data"
DEFAULT_VOICE="${PIPER_VOICE:-en_US-ryan-high}"

# Parse extra args passed via docker command/args
EXTRA_ARGS="$@"

echo "[entrypoint] Starting Wyoming server (port 10200) with default voice: ${DEFAULT_VOICE}"
cd /usr/src
.venv/bin/python3 -m wyoming_piper \
    --uri 'tcp://0.0.0.0:10200' \
    --data-dir "${DATA_DIR}" \
    --voice "${DEFAULT_VOICE}" \
    --update-voices \
    ${EXTRA_ARGS} &

WYOMING_PID=$!

# Wait for the default voice to be downloaded before starting the HTTP server
echo "[entrypoint] Waiting for default voice model..."
TRIES=0
while [ ! -f "${DATA_DIR}/${DEFAULT_VOICE}.onnx" ] && [ $TRIES -lt 120 ]; do
    sleep 1
    TRIES=$((TRIES + 1))
done

if [ ! -f "${DATA_DIR}/${DEFAULT_VOICE}.onnx" ]; then
    echo "[entrypoint] WARNING: Default voice model not found after 120s"
fi

echo "[entrypoint] Starting HTTP API server (port 5000)"
.venv/bin/python3 -m piper.http_server \
    --host 0.0.0.0 \
    --port 5000 \
    --model "${DEFAULT_VOICE}" \
    --data-dir "${DATA_DIR}" &

HTTP_PID=$!

echo "[entrypoint] Both servers started (Wyoming PID=${WYOMING_PID}, HTTP PID=${HTTP_PID})"

# Wait for either process to exit
wait -n $WYOMING_PID $HTTP_PID
EXIT_CODE=$?

echo "[entrypoint] A process exited with code ${EXIT_CODE}, shutting down..."
kill $WYOMING_PID $HTTP_PID 2>/dev/null || true
wait
exit $EXIT_CODE
