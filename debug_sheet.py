import unicodedata
import re
import openpyxl

WORKBOOK_PATH = "docs/Comparativa combustibles1.xlsb.xlsx"

sheet_to_energy = {
    "GASOIL": "DIESEL",
    "GAS NATURAL": "GAS NATURAL",
    "H2": "H2",
    "HVO": "HVO",
    "BIOMETANO": "BIOMETANO",
    "ELECTRICO": "ELECTRICO",
    "DUOTRAILER GASOIL": "DUO GASOIL",
    "DUOTRAILER HVO": "DUO HVO",
    "DUOTRAILER H2": "DUO H2",
    "DUOTRAILER ELECTRICO": "DUO ELECTRICO",
    "DUOTRAILER BIOMETANO": "DUO BIOMETANO"
}

def normalize_key(value: str) -> str:
    normalized = unicodedata.normalize("NFKD", value)
    ascii_only = ''.join(ch for ch in normalized if not unicodedata.combining(ch))
    ascii_only = ascii_only.upper().strip()
    ascii_only = re.sub(r"\s+", " ", ascii_only)
    return ascii_only

wb = openpyxl.load_workbook(WORKBOOK_PATH, data_only=True)
ws = wb['DUOTRAILER GASOIL']
print('monthly km row sample:')
for values in ws.iter_rows(min_row=1, max_row=10, min_col=1, max_col=8, values_only=True):
    print(values)
