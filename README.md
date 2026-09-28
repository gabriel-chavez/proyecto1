# Plantilla Genérica Web API .NET 10 con Conexión WCF Externa

Esta es una plantilla genérica compilable estructurada con **Clean Architecture** en **.NET 10** y **C# 13**.
La persistencia de base de datos se maneja a través de un servicio externo **WCF (SOAP)** genérico para la ejecución de procedimientos almacenados (SPs), evitando la necesidad de Entity Framework Core o llamadas SQL directas en la API.

---

## 1. Arquitectura de la Solución

La solución consta de 4 proyectos, con un flujo de ejecución unidireccional y desacoplado mediante inversión de dependencias:

```text
PlataformaSoat.Api (Controllers, Middleware, Filters)
   │
   ▼
PlataformaSoat.Application (Services, DTOs, interfaces de repositorios, validadores)
   │
   ▼
PlataformaSoat.Domain (Entidades, Value Objects, Enums, Excepciones de negocio)
   ▲
   │ (implementa interfaces)
PlataformaSoat.Infrastructure (Clientes WCF, Repositorios, Caché)
```

### Proyectos y Responsabilidades:
1. **PlataformaSoat.Domain**: Contiene los conceptos fundamentales del negocio sin dependencias externas (entidades, enums de dominio, excepciones, etc.).
2. **PlataformaSoat.Application**: Contiene los casos de uso (servicios), los DTOs de entrada y salida, e interfaces de repositorios que serán implementadas por la infraestructura.
3. **PlataformaSoat.Infrastructure**: Se encarga del acceso a recursos externos. Implementa los contratos WCF dinámicamente usando `ChannelFactory` y mapea los resultados JSON a objetos de la aplicación.
4. **PlataformaSoat.Api**: Expone la capa REST externa (HTTP). Configura el pipeline de ASP.NET Core con control de errores globales, logging y correlation IDs.

---

## 2. Configuración (`appsettings.json`)

Toda la configuración técnica del servicio WCF y del sistema se especifica en el archivo `appsettings.json` del proyecto API:

```json
{
  "WcfSettings": {
    "UrlBase": "https://ws-desarrollo.univida.bo/ServiciosWebGenericasSqlServer/ServiciosWebGenericasManager.svc",
    "SendTimeoutMinutos": 5,
    "ReceiveTimeoutMinutos": 10,
    "MaxReceivedMessageSizeMB": 20,
    "NombreBD": "BD_CORE_SEGUROS"
  },
  "ApiSettings": {
    "NombreSistema": "MiAPI",
    "Version": "1.0.0",
    "Ambiente": "Development",
    "CodigoSistema": "031"
  }
}
```

---

## 3. Módulos Implementados en Plataforma SOAT

La solución cuenta con los siguientes módulos de negocio basados en Stored Procedures de PostgreSQL:

- **Autenticación (`Auth`)**: Control de accesos con tokens JWT Bearer, rotación de Refresh Tokens y revocación (`/api/auth`).
- **Parámetros SOAT (`SalesParams`)**: Consulta de parámetros generales, departamentos, tipos de vehículo, usos y tipos de placa (`/api/sales-params`).
- **Ventas SOAT (`Sales`)**: Emisión masiva de pólizas, solicitud y confirmación de anulaciones, y reporte de ventas (`/api/sales`).
- **Conciliación SOAT (`SalesRecon`)**: Conciliación de ventas por broker, anulación de conciliaciones y consulta de pólizas no conciliadas (`/api/sales-recon`).

---

## 4. Middleware y Utilidades Integradas

- **Correlation ID**: `CorrelationIdMiddleware.cs` agrega el encabezado `X-Correlation-ID` en logs y respuestas HTTP facilitando la trazabilidad.
- **Manejo de Errores**: `ExceptionMiddleware.cs` intercepta las excepciones del sistema y las formatea bajo una envoltura estándar `ApiResponse<T>`.
- **Health Checks**: Disponible en `/health/live` (estado del host) y `/health/ready` (estado de dependencias).
- **Swagger / OpenAPI**: Configurado y accesible en la ruta raíz `/` en ambiente de desarrollo.

---

## 5. Instrucciones de Compilación y Ejecución

### Prerrequisitos
- .NET 10 SDK

### Compilar la Solución
Desde la carpeta raíz del proyecto (`C:\Users\lchavez\.gemini\antigravity-ide\scratch\PlataformaSoat`), ejecuta:
```bash
dotnet build
```

### Ejecutar el Proyecto API
Para iniciar la API en modo de desarrollo, ejecuta:
```bash
dotnet run --project PlataformaSoat.Api
```

Una vez ejecutándose, la página de **Swagger UI** estará disponible automáticamente en tu navegador (típicamente `http://localhost:5000` o `https://localhost:5001` según la configuración de puertos locales).
