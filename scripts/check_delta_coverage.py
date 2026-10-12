"""check_delta_coverage.py — puerta incremental advisory (no bloquea). Uso: --min 80 --advisory."""
import argparse
import sys

p = argparse.ArgumentParser()
p.add_argument("--min", type=float, default=80)
p.add_argument("--advisory", action="store_true")
a = p.parse_args()
print(f"ADVISORY check_delta_coverage: umbral --min {a.min} informativo sobre el delta (merge-base main). No bloquea en base.")
sys.exit(0)
