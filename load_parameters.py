import re
import unicodedata
from pathlib import Path

import openpyxl
import requests

API_BASE = "http://localhost:5001"
EXCEL_PATH = Path("docs/Comparativa combustibles1.xlsb.xlsx")
PARAM_ENDPOINT = f"{API_BASE}/api/admin/parameters"

def normalize_label(value):
    if value is None:
        return None
    text = str(value)
    ascii_text = ''.join(ch for ch in unicodedata.normalize('NFKD', text) if not unicodedata.combining(ch))
    ascii_text = ascii_text.upper()
    ascii_text = re.sub(r"[^A-Z0-9]+", " ", ascii_text)
    return ascii_text.strip()

class LabelTable:
    def __init__(self, worksheet):
        self._data = {}
        for row in worksheet.iter_rows(min_row=1, max_row=120, min_col=1, max_col=6, values_only=True):
            label_a = row[0]
            label_b = row[3]
            if label_a:
                self._data[normalize_label(label_a)] = [row[1], row[2]]
            if label_b:
                self._data[normalize_label(label_b)] = [row[4], row[5]]

    def numeric_value(self, label: str, index: int = 0) -> float:
        key = normalize_label(label)
        if key not in self._data:
            raise KeyError(f"Label '{label}' not found")
        values = [v for v in self._data[key] if isinstance(v, (int, float))]
        if len(values) <= index:
            raise ValueError(f"Numeric value index {index} not available for '{label}'")
        return float(values[index])

    def text_value(self, label: str):
        key = normalize_label(label)
        if key not in self._data:
            return None
        for value in self._data[key]:
            if isinstance(value, str):
                return value
        return None

def parse_second_driver_threshold(table: LabelTable) -> float:
    text = table.text_value("Conductores *")
    if not text:
        return 0.0
    match = re.search(r"(\d+(?:[.,]\d+)?)", text)
    if not match:
        return 0.0
    return float(match.group(1).replace(',', '.'))

def extract_parameters():
    workbook = openpyxl.load_workbook(str(EXCEL_PATH), data_only=True)
    sheet = workbook["CALCULADORA COSTES"]
    table = LabelTable(sheet)

    params = {}
    params["operation.kmsPerDayDefault"] = table.numeric_value("KMS VEHICULO /DIA")
    params["operation.daysPerMonthDefault"] = table.numeric_value("DIAS MES")
    params["operation.driverSalary"] = table.numeric_value("SALARIO CONDUCTOR (Tabla R)")
    params["pricing.margin"] = table.numeric_value("MARGEN")
    params["operation.secondDriverThreshold"] = parse_second_driver_threshold(table)
    params["assets.trailerPrice"] = table.numeric_value("PRECIO SEMIRREMOLQUE", 0)
    params["assets.dollyPrice"] = table.numeric_value("PRECIO SEMIRREMOLQUE", 1)
    params["operation.duoConsumptionSaving"] = table.numeric_value("% Ahorro consumo  Duo")
    params["corridor.yardCost"] = table.numeric_value("COSTE TRAILER YARD/")
    params["corridor.transportCost"] = table.numeric_value("COSTE TRANSPORTE PLAZA")
    params["corridor.deliveriesPerMonth"] = table.numeric_value("Nº DESCARGAS PLAZA")
    params["corridor.duoKm"] = table.numeric_value("KMS CORREDOR DUO")
    params["corridor.tripsPerMonth"] = table.numeric_value("Nº ACARREOS VIAJE")
    params["corridor.tollKmSimple"] = table.numeric_value("KMS AUTOPISTA", 0)
    params["corridor.tollKmDuo"] = table.numeric_value("KMS AUTOPISTA", 1)
    params["corridor.tollPricePerKmSimple"] = table.numeric_value("PRECIO AUTOPISTA", 0)
    params["corridor.tollPricePerKmDuo"] = table.numeric_value("PRECIO AUTOPISTA", 1)
    params["operation.extraDriverFactor"] = table.numeric_value("EXTRA CONDUCTOR")
    params["emissions.priceTonCo2"] = table.numeric_value("PRECIO t CO2")
    params["pricing.tariffCorrectionFactor"] = table.numeric_value("FACTOR CORRECTOR TARIFA")
    return params

def update_parameters(values):
    session = requests.Session()
    response = session.get(PARAM_ENDPOINT)
    response.raise_for_status()
    current = {item["key"]: item for item in response.json()}

    for key, value in values.items():
        dto = current.get(key)
        if not dto:
            print(f"[WARN] Parameter '{key}' does not exist in the API, skipping")
            continue
        dto["value"] = round(value, 6)
        put_response = session.put(f"{PARAM_ENDPOINT}/{key}", json=dto)
        put_response.raise_for_status()
        print(f"Updated {key} -> {dto['value']}")

if __name__ == "__main__":
    parameter_values = extract_parameters()
    update_parameters(parameter_values)
