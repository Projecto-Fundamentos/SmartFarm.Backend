# SmartFarm: primeras operaciones del Cattle Service

Este paquete se aplica al proyecto que ya creaste en `C:\Users\Diego\SmartFarm.Backend`.
Requiere el SDK .NET 10, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3, PostgreSQL y la migración InitialCattleSchema ya aplicada.

## Instalación

1. Detén la API con Ctrl+C y cierra cualquier archivo Program.cs que tenga cambios sin guardar.
2. Descomprime este ZIP en una carpeta temporal.
3. Copia la carpeta `services` y los dos archivos de instrucciones y pruebas a `C:\Users\Diego\SmartFarm.Backend`.
4. Cuando Windows pregunte, reemplaza `Program.cs`. Los demás archivos son nuevos.
5. En VS Code abre la carpeta SmartFarm.Backend y ejecuta desde su raíz:

```powershell
dotnet build services/CattleService
dotnet run --project services/CattleService --launch-profile http -- --urls http://localhost:5001
```

La configuración de conexión guardada en user-secrets se conserva. Este paquete no incluye contraseñas, ni reemplaza el archivo del proyecto, las entidades, el contexto o las migraciones existentes. No hace falta crear otra migración: las tablas no cambian.

## Qué código agrega

| Capa | Archivos y función |
|---|---|
| Domain | Contrato ICattleCatalogRepository y excepciones del catálogo. Usa las entidades ya creadas. |
| Application | CattleCatalogService: registro y consulta, comprobación de estancia activa y validación del lote. |
| Infrastructure | CattleCatalogRepository: consultas y escritura con EF Core, traducción del conflicto por arete duplicado. |
| Interface | Controladores HTTP, DTO, respuestas y tratamiento de errores. |

La capa Application depende del contrato de Domain; Program.cs conecta su implementación de Infrastructure mediante inyección de dependencias.

## Operaciones

| Método | Ruta | Uso |
|---|---|---|
| POST | /api/v1/ranches | Crear estancia. JSON: `{ "name": "Fundo San José" }`. Devuelve 201. |
| GET | /api/v1/ranches | Consultar hasta 100 estancias, ordenadas por nombre e ID. |
| GET | /api/v1/ranches/{id} | Consultar una estancia. Devuelve 200 o 404. |
| POST | /api/v1/cattle | Registrar bovino. JSON: `{ "ranchId": "UUID", "earTag": "PE-APU-001", "lotId": null }`. Devuelve 201, 400, 404 o 409. |
| GET | /api/v1/cattle/{id}?ranchId={ranchId} | Consultar bovino dentro de la estancia indicada. Devuelve 200 o 404. |
| GET | /api/v1/cattle?ranchId={ranchId}&limit=100 | Consultar entre 1 y 100 bovinos por estancia. |
| GET | /health | Comprobar la conexión con PostgreSQL. |
| GET | /openapi/v1.json | Obtener la documentación OpenAPI en desarrollo. |

Para empezar, registra el bovino con `lotId: null`. El registro de lotes por API se añadirá en un paso posterior.

## Prueba manual con PowerShell

Deja la API funcionando y abre otra terminal en SmartFarm.Backend. Ejecuta:

```powershell
$ranch = Invoke-RestMethod -Uri 'http://localhost:5001/api/v1/ranches' -Method Post -ContentType 'application/json' -Body '{"name":"Fundo San José"}'
$ranch | ConvertTo-Json

$body = @{ ranchId = $ranch.id; earTag = 'PE-APU-001'; lotId = $null } | ConvertTo-Json
$animal = Invoke-RestMethod -Uri 'http://localhost:5001/api/v1/cattle' -Method Post -ContentType 'application/json' -Body $body
$animal | ConvertTo-Json

Invoke-RestMethod "http://localhost:5001/api/v1/cattle/$($animal.id)?ranchId=$($ranch.id)" | ConvertTo-Json
```

Para ejecutar las comprobaciones de integración incluidas:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Pruebas_Cattle.ps1
```

Ese comando permite ejecutar únicamente este proceso de PowerShell con la política indicada. El script crea una estancia y un bovino de prueba, los conserva como datos de demostración, y comprueba registro, consulta, duplicados normalizados, campos inválidos, estancia y bovino inexistentes, consulta desde otra estancia y ausencia de inserciones duplicadas. No comprueba autenticación.

## Comprobación directa en pgAdmin

En Query Tool de smartfarm_cattle:

```sql
SELECT ranch_id, name, status FROM cattle.ranches;
SELECT cattle_id, ranch_id, lot_id, ear_tag, status, version FROM cattle.cattle;
```

Después de registrar el bovino, puedes reiniciar la API y repetir GET con los IDs que guardaste para comprobar que persiste.

## Evidencias para el capítulo V

- Captura de `dotnet build` terminado correctamente.
- Capturas del registro de estancia y bovino, y consulta de sus datos.
- Captura de los datos en PostgreSQL.
- Salida real de Pruebas_Cattle.ps1 después de ejecutarlo correctamente.
- Documentación OpenAPI y commits reales cuando subas el código.

Este paquete contiene código para una primera implementación local. Su compilación y ejecución en tu equipo deben comprobarse con los comandos indicados; no representa pruebas ya aprobadas ni despliegue realizado. Todavía quedan autenticación y autorización mediante Identity, creación y asignación de lotes, actualización y baja lógica, paginación completa, biblioteca propia, pruebas unitarias y BDD, los demás microservicios, integración por eventos y despliegue en la nube.

## Fuentes técnicas

- Npgsql EF Core: https://www.npgsql.org/efcore/
- Manejo de errores ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-10.0
- Migraciones EF Core: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
