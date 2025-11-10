import openpyxl
wb=openpyxl.load_workbook('docs/Comparativa combustibles1.xlsb.xlsx', data_only=True)
ws=wb['TABLAS ']
for row in ws.iter_rows(min_row=1, max_row=40, min_col=1, max_col=6, values_only=True):
    if any(row):
        print(row)
