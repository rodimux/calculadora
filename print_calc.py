import openpyxl
wb=openpyxl.load_workbook('docs/Comparativa combustibles1.xlsb.xlsx', data_only=True)
ws=wb['CALCULADORA COSTES']
for row in ws.iter_rows(min_row=20, max_row=80, min_col=1, max_col=6, values_only=True):
    if any(row):
        print(row)
