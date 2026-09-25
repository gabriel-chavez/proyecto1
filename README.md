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

## 3. Ejemplo Implementado: Módulo de Cobranza (`RevertirCobro`)

Se incluye una implementación completa del caso de uso de **Reversión de Cobro**, que llama al SP `EXTERNOS_V2.P_F_EX_COB_22_COBRO_REVERTIR`:

- **Controlador**: `CobranzaController.cs` expone el endpoint `POST api/Cobranza/cobros/reversion`.
- **Servicio**: `CobranzaService.cs` valida las reglas del caso de uso y orquesta el llamado.
- **Repositorio**: `CobranzaRepository.cs` estructura los parámetros de tipo String, Int, etc., en la secuencia y orden esperados.
- **Cliente WCF**: `ServiciosWebGenericasManagerClient.cs` invoca la operación `EjecutarSPJsonDocumentAsync` en el servicio SOAP externo enviando el payload serializado con las firmas de esquema correctas.

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
