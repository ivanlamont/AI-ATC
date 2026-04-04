FROM rhasspy/wyoming-piper AS base

# Install Flask for the built-in Piper HTTP server
RUN .venv/bin/pip install --no-cache-dir flask pathvalidate

# Copy our entrypoint that runs both Wyoming + HTTP servers
COPY entrypoint.sh /usr/src/entrypoint.sh
RUN chmod +x /usr/src/entrypoint.sh

EXPOSE 10200 5000

ENTRYPOINT ["/usr/src/entrypoint.sh"]
