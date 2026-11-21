ARG PYTHON_IMAGE=python:3.12-slim-bookworm
ARG HERMES_IMAGE=nousresearch/hermes-agent:0.23.2

FROM ${PYTHON_IMAGE} AS graphify
RUN apt-get update && apt-get install -y --no-install-recommends git ripgrep \
    && rm -rf /var/lib/apt/lists/* \
    && git config --system --add safe.directory /workspace
COPY tools/knowledge/requirements.txt /tmp/graphify-requirements.txt
RUN python -m venv /opt/graphify \
    && /opt/graphify/bin/pip install --no-cache-dir -r /tmp/graphify-requirements.txt
ENV GRAPHIFY_ENV=/opt/graphify PYTHONUNBUFFERED=1 PYTHONDONTWRITEBYTECODE=1
WORKDIR /workspace
ENTRYPOINT ["/opt/graphify/bin/python", "tools/knowledge/knowledge.py"]
CMD ["status"]

FROM ${HERMES_IMAGE} AS hermes
USER root
RUN apt-get update && apt-get install -y --no-install-recommends ripgrep \
    && rm -rf /var/lib/apt/lists/* \
    && git config --system --add safe.directory /workspace
COPY tools/knowledge/requirements.txt /tmp/graphify-requirements.txt
RUN python3 -m venv /opt/graphify \
    && /opt/graphify/bin/pip install --no-cache-dir -r /tmp/graphify-requirements.txt
ENV GRAPHIFY_ENV=/opt/graphify PYTHONDONTWRITEBYTECODE=1
WORKDIR /workspace
# Keep the official /init entrypoint and its UID/data-directory setup.
CMD ["chat"]
