# 📄 Proyecto: Calculadora Comparativa de Combustibles

Este proyecto replica una hoja Excel avanzada para comparar los costes por tipo de energía (Diesel, GNL, H2, HVO, Biometano, Eléctrico, etc.) en operaciones logísticas con trailers o duotrailers. El objetivo es ofrecer una aplicación web profesional con:

- Una **calculadora interactiva** para usuarios operativos.
- Un **panel de administración** para mantener precios, consumos, factores y costes logísticos.

---

## ♻ Estructura

```
CALCULADORA/
├── docs/
│   ├── Comparativa combustibles1.xlsb.xlsx       # Excel fuente original
│   ├── funcional_calculadora_web.md              # Documento funcional de la app
│   └── agent_calculadora_costes.prompt           # Prompt para agente (opcional)
├── backend/                                      # Lógica de negocio y API
├── frontend/                                     # Interfaz web (Blazor, Razor Pages, etc.)
├── database/                                     # Scripts de tablas y datos
└── README.md                                     # Este archivo
```

---

## 🌐 Tecnologías Sugeridas

- **.NET 8** (Blazor Server / Razor Pages)
- **C#** para la lógica de cálculo
- **SQL Server / SQLite** para persistencia

---

## 🚀 Objetivos

- Replicar los cálculos de la hoja "CALCULADORA COSTES".
- Ofrecer una tabla comparativa de costes por energía según escenarios definidos por el usuario.
- Permitir que usuarios administradores gestionen datos clave (combustibles, consumos, costes, coeficientes...)
- Mantener los resultados alineados con el Excel.

---

## 📚 Documentación

Consulta `docs/funcional_calculadora_web.md` para:
- Parámetros implicados
- Datos necesarios
- Flujo de usuario
- Campos editables por admins

Consulta `docs/agent_calculadora_costes.prompt` si implementas una versión con agente conversacional local.

---

## 📅 Siguientes pasos

1. Diseñar modelo de datos
2. Replicar las fórmulas clave del Excel
3. Construir UI básica
4. Validar resultados contra Excel original

---

## Configuraci??n de base de datos

La API lee dos valores en `src/CalculadoraCostes.Api/appsettings.json`:

- `DatabaseProvider`: ahora est?? en `Sqlite` por defecto para facilitar despliegues personales (se crea `calculadora.db` en el directorio de trabajo). Cambia a `SqlServer` cuando necesites apuntar a una instancia de SQL Server.
- `ConnectionStrings:DefaultConnection`: cadena asociada al proveedor elegido (`Data Source=calculadora.db` para SQLite o la cadena de tu servidor SQL).
- `Swagger:Enabled`: controla si la UI de Swagger se expone tambi??n en entornos de producci??n (Render, Railway, etc.). Ponlo a `true` cuando quieras probar la API online.

El arranque ejecuta `context.Database.Migrate()` as?? que no necesitas comandos extra: basta con ajustar estos valores y la base se crear??/actualizar?? sola.

---

¡Vamos allá!

