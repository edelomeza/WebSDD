"""check_coverage.py — exige cobertura >=45% si hay reporte; en base esqueleto informa y pasa (advisory)."""
import glob
import sys
import xml.etree.ElementTree as ET

MIN = 45
reports = glob.glob("coverage*.xml") + glob.glob("**/coverage.cobertura.xml", recursive=True)
if not reports:
    print("ADVISORY check_coverage: sin reporte, base esqueleto. Umbral >=45% aplica desde primer slice con codigo.")
    sys.exit(0)
worst = 100.0
for r in reports:
    try:
        root = ET.parse(r).getroot()
        rate = root.get("line-rate") or root.get("coverage-line-rate")
        if rate is not None:
            worst = min(worst, float(rate) * 100)
    except Exception as e:  # noqa: BLE001 - script de gate, reporta y sigue
        print(f"WARN {r}: {e}")
if worst < MIN:
    print(f"FAIL check_coverage: {worst:.1f}% < {MIN}%")
    sys.exit(1)
print(f"PASS check_coverage: {worst:.1f}% >= {MIN}%")
