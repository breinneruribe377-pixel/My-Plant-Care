<p align="center">
  <img src="docs/assets/logo.png" alt="MyPlantCare" width="420">
</p>

# MyPlantCare API

API REST para **organizar el cuidado de las plantas del hogar**: registrar plantas, definir sus cuidados
periódicos (riego, fertilización, poda, rotación, trasplante, revisión de plagas), marcarlos como realizados
y consultar qué tareas están pendientes.

> Caso de estudio – Programación de Dispositivos Móviles · Institución Universitaria Pascual Bravo
> Autor: Breinner Uribe Mazo · Docente: Omar Andrés Cardona Zapata
> Documento de definición: [`docs/Definicion_documental_Caso_de_Estudio.docx`](docs/Definicion_documental_Caso_de_Estudio.docx)

## Tecnologías

| Capa | Tecnología |
|---|---|
| API | ASP.NET Core 9 (Web API con controladores) |
| ORM / acceso a datos | Entity Framework Core 9 (SQL Server) — mapeo a un esquema creado por script DDL |
| Base de datos | SQL Server 2019+ / Express / contenedor Docker |
| Autenticación | JWT Bearer (contraseñas con hash `PasswordHasher`) |
| Documentación | OpenAPI + Swagger UI (`/swagger`) y [`docs/openapi.yaml`](docs/openapi.yaml) |

## Estructura del repositorio

```
├── MyPlantCare/                 Proyecto de la API
│   ├── Controllers/             Auth, Plantas, Cuidados, Pendientes
│   ├── Models/                  Entidades (Usuario, Planta, Cuidado)
│   ├── Dtos/                    Contratos de entrada/salida
│   ├── Data/                    DbContext (mapeo a las tablas)
│   ├── Services/                TokenService, CalculoFechas (RF-03)
│   ├── Program.cs               Configuración (JWT, EF Core, Swagger)
│   └── MyPlantCare.http         Peticiones de ejemplo
├── database/01_schema.sql       Script DDL (BD + tablas + restricciones)
├── docs/                        Documento del caso de estudio, OpenAPI y logo
├── Dockerfile
├── docker-compose.yml           SQL Server + creación del esquema + API
└── .env.example                 Variables de entorno de ejemplo
```

## Modelo de datos

```
usuario (1) ──< planta (1) ──< cuidado
```

| Tabla | Columnas principales |
|---|---|
| `usuario` | `id` PK, `nombre`, `email` (único), `password_hash`, `fecha_registro` |
| `planta` | `id` PK, `usuario_id` FK→usuario, `nombre`, `especie`, `ubicacion`, `foto`, `fecha_creacion` |
| `cuidado` | `id` PK, `planta_id` FK→planta, `tipo`, `frecuencia_dias`, `ultima_vez_realizado`, `proxima_fecha` |

- `cuidado.tipo` ∈ `riego`, `fertilizacion`, `poda`, `rotacion`, `trasplante`, `revision_plagas` (CHECK).
- `cuidado.frecuencia_dias > 0` (CHECK).
- Borrado en cascada: usuario → plantas → cuidados.

El esquema **no lo genera EF Core**: se crea con el script [`database/01_schema.sql`](database/01_schema.sql)
(idempotente), y EF solo lo mapea.

## Configuración (variables de entorno)

| Variable | Descripción | Ejemplo |
|---|---|---|
| `ConnectionStrings__Default` | Cadena de conexión a SQL Server | `Server=localhost,1433;Database=db_usuario;User Id=sa;Password=...;TrustServerCertificate=True;` |
| `Jwt__Key` | Clave secreta para firmar el JWT (mín. 32 caracteres) | `una-clave-larga-y-aleatoria-...` |
| `Jwt__Issuer` / `Jwt__Audience` | Emisor y audiencia (opcionales) | `PlantCare.Api` / `PlantCare.Clientes` |
| `Jwt__ExpiresMinutes` | Vigencia del token en minutos (opcional) | `60` |

Nunca subas contraseñas ni claves reales al repositorio. En desarrollo puedes usar
`dotnet user-secrets` o variables de entorno; en `appsettings.Development.json` solo hay una clave de desarrollo local.

## Cómo ejecutarlo

### Opción A — Docker (recomendada, todo en un comando)

Requisitos: Docker Desktop.

```bash
git clone <URL-DEL-REPOSITORIO>
cd <carpeta-del-repo>
cp .env.example .env        # opcional: cambia las claves
docker compose up --build
```

Esto levanta SQL Server, ejecuta `database/01_schema.sql` y arranca la API.
Abre **http://localhost:5216/swagger**.

### Opción B — Local con .NET y SQL Server / Express

Requisitos: [.NET SDK 9](https://dotnet.microsoft.com/download) y SQL Server (Express sirve).

1. **Crear la base de datos y las tablas** ejecutando `database/01_schema.sql`
   (en SSMS / Azure Data Studio, o por consola):
   ```bash
   sqlcmd -S localhost\SQLEXPRESS -E -i database/01_schema.sql
   ```
2. **Configurar la conexión** (ajusta el servidor a tu instancia):
   ```bash
   # PowerShell
   $env:ConnectionStrings__Default = "Server=localhost\SQLEXPRESS;Database=db_usuario;Trusted_Connection=True;TrustServerCertificate=True;"
   # Linux / macOS
   export ConnectionStrings__Default="Server=localhost,1433;Database=db_usuario;User Id=sa;Password=TU_CLAVE;TrustServerCertificate=True;"
   ```
3. **Ejecutar la API** (en modo Development ya incluye una clave JWT de desarrollo):
   ```bash
   dotnet run --project MyPlantCare --launch-profile https
   ```
4. Abre **https://localhost:7051/swagger** (o http://localhost:5216/swagger).

## Documentación de la API

- **Swagger UI** en vivo: `/swagger` (documento JSON en `/openapi/v1.json`).
- Especificación estática: [`docs/openapi.yaml`](docs/openapi.yaml).

### Endpoints

| Método | Ruta | Descripción | Req. |
|---|---|---|---|
| POST | `/auth/register` | Registrar usuario y obtener JWT | — |
| POST | `/auth/login` | Iniciar sesión y obtener JWT | — |
| GET | `/plantas` | Listar plantas del usuario (con sus cuidados) | RF-01 |
| POST | `/plantas` | Crear planta | RF-01 |
| GET | `/plantas/{plantaId}` | Detalle de una planta | RF-01 |
| PUT | `/plantas/{plantaId}` | Editar planta | RF-01 |
| DELETE | `/plantas/{plantaId}` | Eliminar planta (y sus cuidados) | RF-01 |
| POST | `/plantas/{plantaId}/cuidados` | Agregar cuidado a una planta | RF-02, RF-03 |
| PUT | `/cuidados/{cuidadoId}/completar` | Marcar cuidado como realizado y recalcular la próxima fecha | RF-04, RF-03 |
| GET | `/pendientes` | Cuidados cuya próxima fecha ya llegó | RF-05 |

Todos los endpoints, salvo `/auth/*`, requieren el header `Authorization: Bearer <accessToken>`.
Cada usuario solo ve y modifica **sus** plantas y cuidados.

### Ejemplo rápido

```bash
# 1. Registrarse (devuelve accessToken)
curl -X POST http://localhost:5216/auth/register -H "Content-Type: application/json" \
  -d '{"nombre":"Ana","email":"ana@example.com","password":"Clave12345"}'

# 2. Crear una planta
curl -X POST http://localhost:5216/plantas -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"nombre":"Monstera","especie":"Monstera deliciosa","ubicacion":"Sala"}'

# 3. Agregarle un cuidado de riego cada 3 días
curl -X POST http://localhost:5216/plantas/1/cuidados -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"tipo":"riego","frecuenciaDias":3}'

# 4. Ver pendientes y marcar un cuidado como realizado
curl http://localhost:5216/pendientes -H "Authorization: Bearer $TOKEN"
curl -X PUT http://localhost:5216/cuidados/1/completar -H "Authorization: Bearer $TOKEN"
```

Más ejemplos en [`MyPlantCare/MyPlantCare.http`](MyPlantCare/MyPlantCare.http) (VS / VS Code con REST Client).

### Formato de errores

```json
{ "code": "RECURSO_NO_ENCONTRADO", "message": "La planta solicitada no existe o no te pertenece." }
```
Las validaciones de datos (400) devuelven el formato estándar `ProblemDetails` de ASP.NET Core.

## Requerimientos funcionales cubiertos

| ID | Requerimiento | Implementación |
|---|---|---|
| RF-01 | Gestión de plantas (CRUD) | `PlantasController` |
| RF-02 | Cuidados asociados a una planta | `POST /plantas/{id}/cuidados` |
| RF-03 | Cálculo automático de la próxima fecha | `Services/CalculoFechas.cs` |
| RF-04 | Registro de cuidado realizado | `PUT /cuidados/{id}/completar` |
| RF-05 | Vista de pendientes | `GET /pendientes` |
