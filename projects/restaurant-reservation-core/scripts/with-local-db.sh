#!/usr/bin/env bash
set -euo pipefail

cd -- "$(dirname -- "$0")/.."

if [[ ! -f .env ]]; then
  echo "Configure the local Docker database in .env first. See docs/setup.md." >&2
  exit 1
fi

if [[ $# -eq 0 ]]; then
  echo "Usage: bash scripts/with-local-db.sh <command> [arguments...]" >&2
  exit 1
fi

set -a
source .env
set +a
exec "$@"
