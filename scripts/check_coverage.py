"""check_coverage.py — exige cobertura de producto >=45% fusionando reportes.

Fusiona (union de lineas cubiertas) todos los `coverage.cobertura.xml` encontrados,
excluyendo ensamblados de test. Sin reporte informa y pasa (advisory, base esqueleto).
Uso: python3 scripts/check_coverage.py
"""
import glob
import sys
import xml.etree.ElementTree as ET

MIN = 45
TEST_TOKENS = ("unittest", "integrationtest", "securitytest", "test", "tests")


def is_test_package(name):
    parts = (name or "").lower().replace("-", ".").replace("_", ".").split(".")
    return any(p in TEST_TOKENS for p in parts)


def merged_lines(reports):
    hits = {}
    for r in reports:
        try:
            root = ET.parse(r).getroot()
        except Exception as e:  # noqa: BLE001 - script de gate, reporta y sigue
            print(f"WARN {r}: {e}")
            continue
        packages = root.find("packages")
        if packages is None:
            continue
        for pkg in packages.findall("package"):
            if is_test_package(pkg.get("name")):
                continue
            for cls in pkg.findall("classes/class"):
                filename = cls.get("filename", "")
                lines = cls.find("lines")
                if lines is None:
                    continue
                for line in lines.findall("line"):
                    try:
                        n = int(line.get("number", 0))
                        h = int(line.get("hits", 0))
                    except (TypeError, ValueError):
                        continue
                    key = (filename, n)
                    hits[key] = max(hits.get(key, 0), h)
    return hits


reports = glob.glob("coverage*.xml") + glob.glob("**/coverage.cobertura.xml", recursive=True)
reports = sorted(set(reports))
if not reports:
    print("ADVISORY check_coverage: sin reporte, base esqueleto. Umbral >=45% aplica desde primer slice con codigo.")
    sys.exit(0)
hits = merged_lines(reports)
if not hits:
    print("ADVISORY check_coverage: reportes sin lineas de producto (solo tests o vacios).")
    sys.exit(0)
covered = sum(1 for h in hits.values() if h > 0)
rate = covered / len(hits) * 100
detail = f"{covered}/{len(hits)} lineas en {len(reports)} reporte(s)"
if rate < MIN:
    print(f"FAIL check_coverage: {rate:.1f}% < {MIN}% ({detail})")
    sys.exit(1)
print(f"PASS check_coverage: {rate:.1f}% >= {MIN}% ({detail})")
