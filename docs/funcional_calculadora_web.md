# ✅ Documento Funcional - Calculadora Comparativa de Combustibles

## 🔍 Objetivo

Desarrollar una aplicación web que replique fielmente la funcionalidad de la hoja Excel "CALCULADORA COSTES" del archivo `Comparativa combustibles1.xlsb.xlsx`. La app constará de:

1. Una **calculadora web interactiva** para el usuario final.
2. Un **panel de administración** para mantener y actualizar todos los datos que intervienen en los cálculos (precios, consumos, costes, coeficientes, etc.).

---

## 🎓 Fuentes de Datos

El Excel contiene distintas hojas y secciones que alimentan los cálculos. A partir del análisis se han identificado los siguientes conjuntos de datos clave:

### 1. Datos por Energía (una fila por tipo)
- Precio (por litro/kg/kWh)
- Consumo (l/100km, kg/100km, kWh/100km)
- Coste renting/mes
- Emisión CO2 por unidad de energía
- Reducción % (para energías renovables)
- Factor renovable (e.g. 90% del HVO es renovable)

### 2. Datos de Operación
- Km/día
- Días/mes
- Nº conductores según km
- Salario conductor
- Margen
- Tipo de semirremolque (Trailer/Dolly)
- Coste remolque

### 3. Costes por Operativa (corredor):
- Coste yard
- Coste transporte
- Nº entregas
- Km corredor
- Factor peaje y múltiples coeficientes de corrección

### 4. Constantes generales:
- Precio tonelada CO2
- Factor corrector tarifa DUO
- Umbral km para segundo conductor

---

## 💻 Interfaz Usuario - Calculadora

### Entradas ajustables por el usuario:
- Km por día (ej: 300)
- Días al mes (ej: 22)
- Tipo remolque

(Opcional: dejar otros campos accesibles también si se desea afinar el escenario)

### Resultados mostrados:
| Energía | Coste energía/km | Coste renting/km | Total coste/km | Coste día | Extracoste vs Diésel | Tarifa recomendada |
|---------|------------------|------------------|----------------|------------|-----------------------|--------------------|
| Diesel  | 0.28             | 0.33             | 0.61           | 201        | 0%                    | 3318 €           |
| GNL     | 0.39             | 0.36             | 0.75           | 243        | 12%                   | 3705 €           |

Se mostrarán también variantes DUO (costes diferentes).

---

## 📄 Panel Admin

### Módulos:
1. **Energías**:
   - CRUD de tipos de energía
   - Campos: nombre, precio, consumo, renting, emisiones, % renovable, % reducción

2. **Constantes y Parámetros**:
   - Salario conductor, margen, precio CO2, umbral km 2º conductor, etc.

3. **Peajes y operativa corredor**:
   - Costes logísticos adicionales, descargas, distancias y factores de corrección

### Validaciones:
- Valores positivos
- Porcentajes entre 0-1 o 0-100 según tipo

---

## ⚖️ Arquitectura Recomendada

- **Backend .NET 8** con acceso a DB (SQL Server o SQLite)
- **Frontend (Razor Pages / Blazor)** con componentes interactivos para cálculos
- Datos persistidos en base de datos, con posibilidad de export/import Excel si se desea como backup manual

---

## 🚀 Flujo

1. Admin actualiza datos desde su panel
2. Usuario usa la calculadora y obtiene tarifas por tipo de energía
3. Cálculo en tiempo real en base a la base de datos + fórmulas replicadas del Excel

---

## 🔧 Siguientes pasos

1. Crear estructuras de base de datos (tablas energías, parámetros, constantes...)
2. Implementar lógica de cálculo en backend
3. Montar formulario UI y renderizado de tabla resultados
4. Desplegar versión inicial de administración
5. Verificar con Excel la coherencia de resultados

---

> Este documento representa la traducción funcional del Excel a una aplicación profesional web multiusuario con control centralizado de datos.