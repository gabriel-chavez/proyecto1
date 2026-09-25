# 📘 GUÍA OFICIAL DE CONVENCIONES DE BASE DE DATOS PostgreSQL

Estándar Corporativo de Desarrollo — Arquitectura Multicapa con Dapper

Motor: PostgreSQL 14+
Arquitectura: Multicapa (pp_ / ps_ / fn_ / tr_)
Auditoría: Obligatoria al 100% (dentro del ps_)
Acceso a datos: Dapper + Npgsql
Frontend: JSON / DTO / gRPC
ORM: Dapper (micro-ORM) — lectura y escritura vía SPs
Versión: 1.0
Estado: Aprobado para publicación

## 📋 ÍNDICE DE CONTENIDOS

Visión General y Filosofía

Arquitectura de Acceso a Datos con Dapper

Arquitectura de Base de Datos

Reglas Inviolables de la Arquitectura

Nomenclatura de Objetos

Parámetros de Entrada y Salida

Control de Flujo en Cascada

Contrato de Salida: JSON / DTO / gRPC

Auditoría Obligatoria al 100%

Convención de Errores

Flujos de un PP

Plantillas SQL Completas

Integración con .NET + Dapper + Npgsql

Ejemplos de Repositorios

Tabla Resumen de la Convención

Checklist de Cumplimiento

## 1. Visión General y Filosofía

El ecosistema de base de datos está construido sobre una arquitectura multicapa desacoplada en PostgreSQL, con acceso desde .NET mediante Dapper como micro-ORM y Npgsql como driver oficial.

Principios Básicos

Separación por responsabilidad: pp_ (principal) coordina, ps_ (secundario) ejecuta operaciones atómicas.

DML exclusivo en ps_: Solo los procedimientos secundarios realizan INSERT, UPDATE o DELETE.

Transacción controlada por pp_: Solo el procedimiento principal maneja COMMIT / ROLLBACK, y solo cuando corresponde.

Auditoría 100% obligatoria: Se registra dentro del ps_ que ejecuta DML, no vía triggers externos.

Control en cascada: Uso de s_procesado para propagar el estado de éxito/fallo.

Sin cursores: Uso de jsonb_array_elements, INSERT...SELECT, operaciones set-based.

Sin hardcoding: Los valores de negocio se pasan como parámetros explícitos.

Dapper como única capa de acceso: Toda operación (lectura o escritura) pasa por Dapper + Npgsql.

Objetivos del Estándar

Consistencia: Todos los desarrollos siguen la misma estructura.

Trazabilidad: 100% de las operaciones de escritura quedan auditadas.

Mantenibilidad: Código predecible, fácil de leer y modificar.

Rendimiento: Dapper evita la sobrecarga de un ORM completo.

Interoperabilidad: Contratos claros entre BD, backend y frontend.

## 2. Arquitectura de Acceso a Datos con Dapper

## 2.1 Stack Tecnológico

## 2.2 Modelo de Acceso

text

┌─────────────────────────────────────────────────────────────┐

│  API / BACKEND (.NET)                                       │

│                                                             │

│  ┌───────────────────────────────────────────────────────┐  │

│  │  Controllers / Minimal APIs                           │  │

│  └───────────────────────────────────────────────────────┘  │

│                          │                                  │

│                          ▼                                  │

│  ┌───────────────────────────────────────────────────────┐  │

│  │  Services (lógica de aplicación)                      │  │

│  └───────────────────────────────────────────────────────┘  │

│                          │                                  │

│                          ▼                                  │

│  ┌───────────────────────────────────────────────────────┐  │

│  │  Repositories (Dapper)                                │  │

│  │  - Lecturas: QueryAsync<T>                            │  │

│  │  - Escrituras: ExecuteAsync con CommandType.StoredProc│  │

│  └───────────────────────────────────────────────────────┘  │

│                          │                                  │

│                          ▼                                  │

│  ┌───────────────────────────────────────────────────────┐  │

│  │  IDbConnectionFactory (NpgsqlDataSource)              │  │

│  └───────────────────────────────────────────────────────┘  │

└─────────────────────────────────────────────────────────────┘

│

▼

┌─────────────────────────────────────────────────────────────┐

│  PostgreSQL                                                 │

│  pp_  → Punto de entrada, transacción, orquestación         │

│  ps_  → DML atómico, validaciones, cálculos, auditoría      │

│  fn_  → Funciones reutilizables en expresiones SQL          │

│  tr_  → Triggers (solo fecha de modificación)               │

└─────────────────────────────────────────────────────────────┘

## 2.3 Reglas de Uso de Dapper

Regla de oro:

"Dapper lee y ejecuta. PostgreSQL decide."

## 3. Arquitectura de Base de Datos

## 3.1 Esquemas

text

core     → Tablas y procedimientos de negocio

audit    → Auditoría transaccional de datos

err      → Registro de excepciones (opcional)

cat      → Catálogos y paramétricas

## 3.2 Bases de Datos

text

db_core      → Datos de negocio (core, cat, audit)

db_errors    → Registro de excepciones (opcional)

⚠️ Nota importante sobre BD separadas en PostgreSQL:
PostgreSQL no soporta transacciones entre bases de datos. Por atomicidad transaccional, la auditoría debe residir en el mismo db_core, esquema audit. Si por política se requiere una BD separada, usar FDW/dblink y aceptar las limitaciones de atomicidad (outbox pattern requerido).

## 4. Reglas Inviolables de la Arquitectura

Regla 1 — Excepciones (EXCEPTION WHEN OTHERS)

Solo el pp_ captura excepciones. Si un ps_ falla técnicamente, la excepción burbujea hacia el pp_, que la captura y llena s_codigo_error / s_detalle_error.

Regla 2 — Transacciones (COMMIT / ROLLBACK)

Solo el pp_ decide cuándo hacer COMMIT o ROLLBACK.
Los ps_ nunca ejecutan COMMIT, ROLLBACK, BEGIN ni SAVEPOINT.

Regla 3 — "Cuando corresponde"

No todos los pp_ manejan transacción explícita:

Regla 4 — DML Exclusivo en ps_

Ninguna sentencia INSERT, UPDATE o DELETE puede existir dentro de un pp_. Toda mutación vive dentro de su respectivo ps_.

Excepción: cuando la operación es trivial y única (1 sola tabla, sin proceso intermedio), el pp_ puede ejecutar DML directamente. Se recomienda evitarlo salvo casos justificados.

Regla 5 — Preservación del s_mensaje

Los ps_ pueden setear s_mensaje cuando detectan un error funcional (mensaje de negocio).

Los pp_ sobrescriben s_mensaje al final del flujo exitoso.

No se concatena s_mensaje en cascada.

Regla 6 — Eliminación Total de Cursores

Queda prohibido el uso de cursores (DECLARE ... CURSOR, FETCH).

Las inserciones masivas se realizan mediante:

sql

INSERT INTO ...

SELECT ...

FROM jsonb_array_elements(e_datos_json) AS item;

Regla 7 — Parámetros 100% Dinámicos

Los valores de negocio (tipo de cambio, porcentajes, impuestos, etc.) deben enviarse como parámetros explícitos. Prohibido hardcodear valores en consultas SQL.

Regla 8 — Validación Estándar del s_procesado

En los ps_, como primera instrucción:

sql

IF s_procesado IS DISTINCT FROM TRUE THEN

RETURN;

END IF;

Esto permite que los ps_ posteriores respeten el resultado de los anteriores.

Regla 9 — Llamadas Nombradas

Al invocar un procedimiento desde otro, usar siempre la sintaxis explícita con nombre de parámetro cuando aplique. En PostgreSQL, al usar CALL se pasa posicionalmente, por lo que el orden de los parámetros debe ser consistente entre pp_ y ps_.

## 5. Nomenclatura de Objetos

## 5.1 Prefijos de Objetos

Todo en minúsculas, siguiendo la convención de PostgreSQL.

## 5.2 Procedimiento Principal — pp_

Estructura: pp_<acción>_<entidad>

Responsabilidades:

Recibir parámetros de entrada

Inicializar salidas

Coordinar los ps_

Controlar el flujo general

Controlar la transacción (COMMIT / ROLLBACK) cuando corresponde

Manejar excepciones (EXCEPTION WHEN OTHERS)

Retornar el resultado

Ejemplos:

sql

core.pp_registrar_venta

core.pp_actualizar_venta

core.pp_anular_venta

core.pp_eliminar_venta

core.pp_obtener_venta

core.pp_listar_ventas

core.pp_aprobar_solicitud

core.pp_rechazar_solicitud

Importante: un pp_ sigue siendo procedimiento aunque solo realice consultas. pp_obtener_venta y pp_listar_ventas no deben convertirse en fn_.

## 5.3 Procedimiento Secundario — ps_

Estructura: ps_<acción>_<entidad>

Responsabilidades:

Validaciones

Consultas internas

Inserciones / actualizaciones / eliminaciones

Cálculos

Auditoría transaccional

Operaciones específicas de una funcionalidad

Ejemplos:

sql

core.ps_validar_venta

core.ps_registrar_venta

core.ps_actualizar_venta

core.ps_anular_venta

core.ps_obtener_venta

core.ps_listar_ventas

core.ps_registrar_venta_detalle

core.ps_calcular_total_venta

Diferencia clave entre pp_ y ps_: no depende de si consulta o modifica datos, sino de su rol arquitectónico:

text

Aplicación

│

▼

PP

│

├── PS

├── PS

└── PS

## 5.4 Funciones — fn_

Se utiliza únicamente cuando corresponde una función PostgreSQL real, es decir, operaciones reutilizables que puedan usarse dentro de expresiones SQL.

Ejemplos:

sql

core.fn_calcular_iva

core.fn_calcular_total

core.fn_obtener_edad

core.fn_formatear_documento

Uso:

sql

SELECT core.fn_calcular_iva(e_total);

Una operación funcional de negocio sigue siendo pp_, no fn_.

## 5.5 Tablas — tb_

Estructura: tb_<entidad> (singular)

Ejemplos:

sql

core.tb_venta

core.tb_venta_detalle

core.tb_cliente

core.tb_producto

core.tb_siniestro

core.tb_siniestro_documento

## 5.6 Vistas — vw_

Estructura: vw_<descripción>

Ejemplos:

sql

core.vw_venta

core.vw_venta_detalle

core.vw_ventas_pendientes

core.vw_siniestros_procesados

## 5.7 Triggers — tr_

Estructura: tr_<acción>_<entidad>

Ejemplos:

sql

core.tr_actualizar_fecha_modificacion

core.tr_validar_estado_siniestro

En esta convención, la auditoría transaccional se hace dentro del ps_, no vía trigger. Los triggers se reservan para fecha de modificación y validaciones simples.

## 5.8 Llaves Foráneas

Sufijo: _fk

En parámetros:

sql

e_cliente_fk

e_venta_fk

e_usuario_fk

e_siniestro_fk

En columnas:

sql

cliente_fk

venta_fk

usuario_fk

siniestro_fk

## 5.9 Identificadores

Clave primaria: id

sql

CREATE TABLE core.tb_venta (

id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

...

);

Referencias: <entidad>_fk

sql

CREATE TABLE core.tb_venta_detalle (

id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

venta_fk BIGINT NOT NULL,

producto_fk BIGINT NOT NULL,

cantidad NUMERIC(18,2) NOT NULL

);

## 5.10 Campos de Auditoría

sql

usuario_creacion

fecha_creacion

usuario_modificacion

fecha_modificacion

sistema_origen

## 5.11 Estados

sql

estado

-- o cuando requiera claridad:

estado_venta

estado_siniestro

estado_solicitud

## 5.12 Nombres de Acciones

## 6. Parámetros de Entrada y Salida

## 6.1 Prefijos

## 6.2 Parámetros de Entrada

Deben usar e_:

sql

e_usuario_ejecucion

e_sistema_origen

e_cliente_fk

e_venta_fk

e_fecha

e_estado

e_total

e_comentario

Equivalencias respecto a convenciones previas:

e_usuario_aut → e_usuario_ejecucion

e_codigo_sistema → e_sistema_origen

La intención es evitar abreviaturas poco claras y hacer que el nombre sea entendible sin conocer el sistema legado.

## 6.3 Parámetros Estándar de Salida

sql

s_procesado

s_resultado

s_mensaje

s_codigo_error

s_detalle_error

Declaración:

sql

INOUT s_procesado     BOOLEAN,

INOUT s_resultado     JSONB,

INOUT s_mensaje       TEXT,

INOUT s_codigo_error  TEXT,

INOUT s_detalle_error TEXT

## 7. Control de Flujo en Cascada

## 7.1 Validación estándar del s_procesado

En los ps_:

sql

IF s_procesado IS DISTINCT FROM TRUE THEN

RETURN;

END IF;

## 7.2 Comportamiento en cascada

text

PS 1

│

├── TRUE → continúa

▼

PS 2

│

├── TRUE → continúa

▼

PS 3

Si ocurre un error funcional:

text

PS 1

│

└── FALSE

│

▼

PS 2

│

└── RETURN (no ejecuta)

## 7.3 Reglas del flujo

No se usa RETURN en los ps_ salvo la validación de s_procesado y los errores funcionales.

No se anidan IF para verificar errores después de cada llamada en el pp_ (más allá del IS DISTINCT FROM TRUE).

No se modifica s_mensaje en los ps_ intermedios, salvo errores funcionales.

El flujo es 100% secuencial y limpio.

## 8. Contrato de Salida: JSON / DTO / gRPC

## 8.1 Comparativa de Opciones

## 8.2 Recomendación

Frontend / API externa → JSON + REST

Comunicación entre servicios → gRPC + Protobuf

Frontends complejos → GraphQL (opcional)

## 8.3 Estrategia del SP

El SP siempre devuelve JSONB en s_resultado. El backend decide si:

Retransmite el JSON tal cual al frontend (REST).

Lo mapea a un DTO (con System.Text.Json).

Lo mapea a un mensaje protobuf (gRPC).

Ventaja: el SP no conoce el protocolo de comunicación; solo produce datos.

Ejemplo de salida:

json

{

"venta_id": 12345,

"codigo": "V-2024-001",

"total": 1500.00,

"estado": "REGISTRADA"

}

## 9. Auditoría Obligatoria al 100%

## 9.1 Decisión de diseño

La auditoría se realiza dentro del ps_ que ejecuta DML, no vía trigger.

Razones:

Control explícito del desarrollador sobre qué se audita.

Posibilidad de incluir contexto de negocio (motivo, usuario_ejecucion, sistema_origen).

Evita triggers ocultos difíciles de debuggear.

Permite auditar el JSONB completo del registro afectado.

## 9.2 Estructura de la tabla de auditoría

sql

CREATE TABLE audit.tb_tabla_variacion (

id                    BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

usuario               VARCHAR(50)  NOT NULL,

sistema_origen        VARCHAR(20)  NOT NULL,

accion                VARCHAR(10)  NOT NULL,   -- INSERT / UPDATE / DELETE

procedimiento         VARCHAR(100) NOT NULL,

esquema_tabla         VARCHAR(50)  NOT NULL,

tabla                 VARCHAR(100) NOT NULL,

registro_id           BIGINT       NOT NULL,

datos_json            JSONB        NOT NULL,   -- registro completo afectado

motivo                VARCHAR(800) NULL,

fecha_ejecucion       TIMESTAMPTZ  NOT NULL DEFAULT NOW()

);

CREATE INDEX ix_tabla_variacion_tabla ON audit.tb_tabla_variacion (tabla, registro_id);

CREATE INDEX ix_tabla_variacion_fecha ON audit.tb_tabla_variacion (fecha_ejecucion DESC);

## 9.3 Función de auditoría

sql

CREATE OR REPLACE FUNCTION audit.fn_registrar_variacion(

e_usuario        VARCHAR(50),

e_sistema_origen VARCHAR(20),

e_accion         VARCHAR(10),

e_procedimiento  VARCHAR(100),

e_esquema_tabla  VARCHAR(50),

e_tabla          VARCHAR(100),

e_registro_id    BIGINT,

e_datos_json     JSONB,

e_motivo         VARCHAR(800) DEFAULT NULL

)

RETURNS VOID

LANGUAGE plpgsql

AS $$

BEGIN

INSERT INTO audit.tb_tabla_variacion (

usuario, sistema_origen, accion, procedimiento,

esquema_tabla, tabla, registro_id, datos_json, motivo

) VALUES (

e_usuario, e_sistema_origen, e_accion, e_procedimiento,

e_esquema_tabla, e_tabla, e_registro_id, e_datos_json, e_motivo

);

END;

$$;

## 9.4 Uso dentro de un ps_

sql

PERFORM audit.fn_registrar_variacion(

e_usuario_ejecucion,

e_sistema_origen,

'INSERT',

'core.ps_registrar_venta',

'core',

'tb_venta',

v_venta_id,

to_jsonb(v_registro_completo),

'Alta de venta'

);

## 9.5 Auditoría masiva (bulk)

Para operaciones INSERT...SELECT con múltiples filas:

sql

WITH ins AS (

INSERT INTO core.tb_venta_detalle (...)

SELECT ...

FROM jsonb_array_elements(e_detalles) AS item

RETURNING *

)

INSERT INTO audit.tb_tabla_variacion (

usuario, sistema_origen, accion, procedimiento,

esquema_tabla, tabla, registro_id, datos_json, motivo

)

SELECT

e_usuario_ejecucion,

e_sistema_origen,

'INSERT',

'core.ps_registrar_venta_detalle',

'core',

'tb_venta_detalle',

ins.id,

to_jsonb(ins),

'Alta masiva de detalle de venta'

FROM ins;

## 10. Convención de Errores

## 10.1 Procesamiento exitoso

sql

s_procesado     := TRUE;

s_codigo_error  := '00000';

s_detalle_error := '';

## 10.2 Error funcional esperado

sql

s_procesado     := FALSE;

s_codigo_error  := '00000';

s_mensaje       := 'La venta no existe.';

s_detalle_error := '';

RETURN;

## 10.3 Error técnico

sql

EXCEPTION

WHEN OTHERS THEN

s_procesado     := FALSE;

s_codigo_error  := SQLSTATE;

s_detalle_error := SQLERRM;

s_mensaje       := 'Ocurrió un error al procesar la operación.';

## 10.4 Diferencia clave

text

Error funcional

↓

s_procesado = FALSE

s_codigo_error = '00000'

s_mensaje = explicación funcional

s_detalle_error = ''

Error técnico

↓

s_procesado = FALSE

s_codigo_error = SQLSTATE

s_mensaje = mensaje genérico

s_detalle_error = SQLERRM

## 11. Flujos de un PP

## 11.1 PP transaccional (escritura)

text

PP

│

├── Inicializar salidas

│

├── PS Validación

│      │

│      ├── FALSE → ROLLBACK → RETURN

│      └── TRUE

│

├── PS Operación

│      │

│      ├── FALSE → ROLLBACK → RETURN

│      └── TRUE

│

├── PS adicional

│      │

│      ├── FALSE → ROLLBACK → RETURN

│      └── TRUE

│

└── COMMIT

El PP controla la transacción general. Los PS no deben hacer COMMIT ni ROLLBACK.

## 11.2 PP de consulta (lectura)

text

PP

│

├── Inicializar salidas

│

├── PS Obtener/Listar

│      │

│      ├── FALSE → RETURN

│      └── TRUE

│

└── Retornar resultado

No requiere COMMIT ni ROLLBACK explícito.

## 12. Plantillas SQL Completas

## 12.1 Cabecera estándar

sql

-- =============================================

-- Autor:         [NOMBRE_DESARROLLADOR]

-- Fecha:         [DD/MM/YYYY]

-- Descripción:   [DESCRIPCIÓN CLARA]

-- Ticket / Pase: [CÓDIGO]

-- =============================================

## 12.2 Procedimiento Principal — pp_ (escritura)

sql

CREATE OR REPLACE PROCEDURE core.pp_registrar_venta(

IN    e_usuario_ejecucion VARCHAR(50),

IN    e_sistema_origen    VARCHAR(20),

IN    e_cliente_fk        BIGINT,

IN    e_total             NUMERIC(18, 2),

INOUT s_procesado         BOOLEAN,

INOUT s_resultado         JSONB,

INOUT s_mensaje           TEXT,

INOUT s_codigo_error      TEXT,

INOUT s_detalle_error     TEXT

)

LANGUAGE plpgsql

AS $$

BEGIN

-- 1. Inicializar salidas

s_procesado     := TRUE;

s_resultado     := NULL;

s_mensaje       := '';

s_codigo_error  := '00000';

s_detalle_error := '';

-- 2. Validación

CALL core.ps_validar_venta(

e_usuario_ejecucion, e_sistema_origen, e_cliente_fk, e_total,

s_procesado, s_resultado, s_mensaje, s_codigo_error

);

IF s_procesado IS DISTINCT FROM TRUE THEN

ROLLBACK;

RETURN;

END IF;

-- 3. Registro (DML ocurre dentro del ps_)

CALL core.ps_registrar_venta(

e_usuario_ejecucion, e_sistema_origen, e_cliente_fk, e_total,

s_procesado, s_resultado, s_mensaje, s_codigo_error

);

IF s_procesado IS DISTINCT FROM TRUE THEN

ROLLBACK;

RETURN;

END IF;

-- 4. Éxito

COMMIT;

s_mensaje := 'La venta fue registrada correctamente.';

EXCEPTION

WHEN OTHERS THEN

ROLLBACK;

s_procesado     := FALSE;

s_resultado     := NULL;

s_codigo_error  := SQLSTATE;

s_detalle_error := SQLERRM;

s_mensaje       := 'Ocurrió un error al registrar la venta.';

END;

$$;

## 12.3 Procedimiento Secundario — ps_ (validación)

sql

CREATE OR REPLACE PROCEDURE core.ps_validar_venta(

IN    e_usuario_ejecucion VARCHAR(50),

IN    e_sistema_origen    VARCHAR(20),

IN    e_cliente_fk        BIGINT,

IN    e_total             NUMERIC(18, 2),

INOUT s_procesado         BOOLEAN,

INOUT s_resultado         JSONB,

INOUT s_mensaje           TEXT,

INOUT s_codigo_error      TEXT

)

LANGUAGE plpgsql

AS $$

DECLARE

v_existe_cliente BOOLEAN;

BEGIN

-- Cascada

IF s_procesado IS DISTINCT FROM TRUE THEN

RETURN;

END IF;

-- Validación: cliente existe

SELECT EXISTS (

SELECT 1 FROM core.tb_cliente WHERE id = e_cliente_fk

) INTO v_existe_cliente;

IF NOT v_existe_cliente THEN

s_procesado    := FALSE;

s_codigo_error := '00000';

s_mensaje      := 'El cliente especificado no existe.';

RETURN;

END IF;

-- Validación: total positivo

IF e_total IS NULL OR e_total <= 0 THEN

s_procesado    := FALSE;

s_codigo_error := '00000';

s_mensaje      := 'El total de la venta debe ser mayor a cero.';

RETURN;

END IF;

s_procesado := TRUE;

-- ❌ Sin EXCEPTION

-- ❌ Sin COMMIT / ROLLBACK

END;

$$;

## 12.4 Procedimiento Secundario — ps_ (registro con auditoría)

sql

CREATE OR REPLACE PROCEDURE core.ps_registrar_venta(

IN    e_usuario_ejecucion VARCHAR(50),

IN    e_sistema_origen    VARCHAR(20),

IN    e_cliente_fk        BIGINT,

IN    e_total             NUMERIC(18, 2),

INOUT s_procesado         BOOLEAN,

INOUT s_resultado         JSONB,

INOUT s_mensaje           TEXT,

INOUT s_codigo_error      TEXT

)

LANGUAGE plpgsql

AS $$

DECLARE

v_venta_id BIGINT;

v_registro JSONB;

BEGIN

-- Cascada

IF s_procesado IS DISTINCT FROM TRUE THEN

RETURN;

END IF;

-- DML

INSERT INTO core.tb_venta (

cliente_fk, total, estado,

usuario_creacion, fecha_creacion, sistema_origen

) VALUES (

e_cliente_fk, e_total, 'REGISTRADA',

e_usuario_ejecucion, NOW(), e_sistema_origen

)

RETURNING id INTO v_venta_id;

-- Auditoría

SELECT to_jsonb(v.*) INTO v_registro

FROM core.tb_venta v

WHERE v.id = v_venta_id;

PERFORM audit.fn_registrar_variacion(

e_usuario_ejecucion, e_sistema_origen,

'INSERT',

'core.ps_registrar_venta',

'core', 'tb_venta',

v_venta_id, v_registro,

'Alta de venta'

);

-- Resultado

s_resultado := jsonb_build_object(

'venta_id', v_venta_id,

'total',    e_total,

'estado',   'REGISTRADA'

);

s_procesado := TRUE;

-- ❌ Sin EXCEPTION

-- ❌ Sin COMMIT / ROLLBACK

END;

$$;

## 12.5 Procedimiento Secundario — ps_ (registro masivo con JSONB)

sql

CREATE OR REPLACE PROCEDURE core.ps_registrar_venta_detalle(

IN    e_usuario_ejecucion VARCHAR(50),

IN    e_sistema_origen    VARCHAR(20),

IN    e_venta_fk          BIGINT,

IN    e_detalles          JSONB,

INOUT s_procesado         BOOLEAN,

INOUT s_resultado         JSONB,

INOUT s_mensaje           TEXT,

INOUT s_codigo_error      TEXT

)

LANGUAGE plpgsql

AS $$

DECLARE

v_count INTEGER;

BEGIN

IF s_procesado IS DISTINCT FROM TRUE THEN

RETURN;

END IF;

IF e_detalles IS NULL OR jsonb_array_length(e_detalles) = 0 THEN

s_procesado := TRUE;

RETURN;

END IF;

WITH ins AS (

INSERT INTO core.tb_venta_detalle (

venta_fk, producto_fk, cantidad, precio_unitario,

usuario_creacion, fecha_creacion, sistema_origen

)

SELECT

e_venta_fk,

(item->>'producto_fk')::BIGINT,

(item->>'cantidad')::NUMERIC(18,2),

(item->>'precio_unitario')::NUMERIC(18,2),

e_usuario_ejecucion, NOW(), e_sistema_origen

FROM jsonb_array_elements(e_detalles) AS item

RETURNING *

)

INSERT INTO audit.tb_tabla_variacion (

usuario, sistema_origen, accion, procedimiento,

esquema_tabla, tabla, registro_id, datos_json, motivo

)

SELECT

e_usuario_ejecucion, e_sistema_origen, 'INSERT',

'core.ps_registrar_venta_detalle',

'core', 'tb_venta_detalle',

ins.id, to_jsonb(ins),

'Alta masiva de detalle de venta'

FROM ins;

GET DIAGNOSTICS v_count = ROW_COUNT;

s_resultado := jsonb_build_object(

'venta_fk', e_venta_fk,

'detalles_registrados', v_count

);

s_procesado := TRUE;

END;

$$;

## 12.6 Procedimiento Principal — pp_ (consulta)

sql

CREATE OR REPLACE PROCEDURE core.pp_obtener_venta(

IN    e_venta_fk          BIGINT,

IN    e_usuario_ejecucion VARCHAR(50),

IN    e_sistema_origen    VARCHAR(20),

INOUT s_procesado         BOOLEAN,

INOUT s_resultado         JSONB,

INOUT s_mensaje           TEXT,

INOUT s_codigo_error      TEXT,

INOUT s_detalle_error     TEXT

)

LANGUAGE plpgsql

AS $$

BEGIN

s_procesado     := TRUE;

s_resultado     := NULL;

s_mensaje       := '';

s_codigo_error  := '00000';

s_detalle_error := '';

CALL core.ps_obtener_venta(

e_venta_fk, e_usuario_ejecucion, e_sistema_origen,

s_procesado, s_resultado, s_mensaje, s_codigo_error

);

IF s_procesado IS DISTINCT FROM TRUE THEN

RETURN;                -- ⚠️ sin ROLLBACK (no hay escritura)

END IF;

-- ❌ Sin COMMIT (es consulta)

s_mensaje := 'Venta obtenida correctamente.';

EXCEPTION

WHEN OTHERS THEN

s_procesado     := FALSE;

s_resultado     := NULL;

s_codigo_error  := SQLSTATE;

s_detalle_error := SQLERRM;

s_mensaje       := 'Ocurrió un error al obtener la venta.';

END;

$$;

## 13. Integración con .NET + Dapper + Npgsql

## 13.1 Paquetes NuGet

bash

dotnet add package Dapper

dotnet add package Npgsql

dotnet add package Npgsql.DependencyInjection

## 13.2 Cadena de conexión

Desarrollo (appsettings.Development.json):

json

{

"ConnectionStrings": {

"PostgreSQL": "Host=localhost;Port=5432;Database=db_core;Username=dev_user;Password=dev_pass;Pooling=false;Include Error Detail=true;"

}

}

Producción (appsettings.Production.json):

json

{

"ConnectionStrings": {

"PostgreSQL": "Host=prod-db.ejemplo.com;Port=5432;Database=db_core;Username=app_user;Password=${DB_PASSWORD};Pooling=true;Minimum Pool Size=5;Maximum Pool Size=50;Timeout=30;Command Timeout=120;Ssl Mode=VerifyFull;Application Name=MiApp_v1.0;"

}

}

## 13.3 Registro en DI (Program.cs)

csharp

using Dapper;

using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Registrar NpgsqlDataSource como singleton

builder.Services.AddSingleton<NpgsqlDataSource>(sp =>

{

var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")

?? throw new InvalidOperationException("Falta la cadena de conexión 'PostgreSQL'.");

return NpgsqlDataSource.Create(connectionString);

});

// Registrar la fábrica de conexiones

builder.Services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();

// Registrar repositorios

builder.Services.AddScoped<IVentaRepository, VentaRepository>();

var app = builder.Build();

app.Run();

## 13.4 Fábrica de conexiones

csharp

using System.Data;

using Npgsql;

public interface IDbConnectionFactory

{

Task<IDbConnection> CreateAsync(CancellationToken ct = default);

}

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory

{

private readonly NpgsqlDataSource _dataSource;

public NpgsqlConnectionFactory(NpgsqlDataSource dataSource)

=> _dataSource = dataSource;

public async Task<IDbConnection> CreateAsync(CancellationToken ct = default)

=> await _dataSource.OpenConnectionAsync(ct);

}

## 13.5 DTOs

csharp

public sealed record VentaDto(

long VentaId,

string Codigo,

decimal Total,

string Estado);

public sealed record ResultadoSp<T>(

bool Procesado,

string Mensaje,

string CodigoError,

string DetalleError,

T? Resultado);

## 13.6 Manejo de errores con Dapper

csharp

using Npgsql;

try

{

// Llamada al SP

}

catch (PostgresException ex)

{

// Error técnico de PostgreSQL (SQLSTATE)

// ex.SqlState, ex.MessageText, ex.Detail

throw new DataAccessException($"Error PostgreSQL [{ex.SqlState}]: {ex.MessageText}", ex);

}

catch (NpgsqlException ex)

{

// Error de red o conexión

throw new DataAccessException("Error de conexión con PostgreSQL.", ex);

}

## 14. Ejemplos de Repositorios

## 14.1 Repositorio de Ventas (escritura + lectura)

csharp

using System.Data;

using System.Text.Json;

using Dapper;

using Npgsql;

public interface IVentaRepository

{

Task<ResultadoSp<VentaDto>> RegistrarVentaAsync(

string usuario, string sistema, long clienteId, decimal total,

CancellationToken ct = default);

Task<ResultadoSp<VentaDto>> ObtenerVentaAsync(

long ventaId, string usuario, string sistema,

CancellationToken ct = default);

Task<IReadOnlyList<VentaListadoDto>> ListarVentasAsync(

string? estado, CancellationToken ct = default);

}

public sealed class VentaRepository : IVentaRepository

{

private readonly IDbConnectionFactory _connectionFactory;

public VentaRepository(IDbConnectionFactory connectionFactory)

=> _connectionFactory = connectionFactory;

public async Task<ResultadoSp<VentaDto>> RegistrarVentaAsync(

string usuario, string sistema, long clienteId, decimal total,

CancellationToken ct = default)

{

await using var connection = await _connectionFactory.CreateAsync(ct);

var parametros = ConstruirParametrosEstandar(usuario, sistema);

parametros.Add("e_cliente_fk", clienteId);

parametros.Add("e_total",      total);

await connection.ExecuteAsync(

new CommandDefinition(

"core.pp_registrar_venta",

parametros,

commandType: CommandType.StoredProcedure,

cancellationToken: ct));

return ExtraerResultado<VentaDto>(parametros);

}

public async Task<ResultadoSp<VentaDto>> ObtenerVentaAsync(

long ventaId, string usuario, string sistema,

CancellationToken ct = default)

{

await using var connection = await _connectionFactory.CreateAsync(ct);

var parametros = ConstruirParametrosEstandar(usuario, sistema);

parametros.Add("e_venta_fk", ventaId);

await connection.ExecuteAsync(

new CommandDefinition(

"core.pp_obtener_venta",

parametros,

commandType: CommandType.StoredProcedure,

cancellationToken: ct));

return ExtraerResultado<VentaDto>(parametros);

}

public async Task<IReadOnlyList<VentaListadoDto>> ListarVentasAsync(

string? estado, CancellationToken ct = default)

{

await using var connection = await _connectionFactory.CreateAsync(ct);

const string sql = @"

SELECT id            AS VentaId,

codigo        AS Codigo,

total         AS Total,

estado        AS Estado,

fecha_creacion AS FechaCreacion

FROM core.vw_ventas

WHERE (@Estado IS NULL OR estado = @Estado)

ORDER BY fecha_creacion DESC";

var resultado = await connection.QueryAsync<VentaListadoDto>(

new CommandDefinition(sql, new { Estado = estado }, cancellationToken: ct));

return resultado.AsList();

}

// ---------------------------------------------------------------------

// Helpers

// ---------------------------------------------------------------------

private static DynamicParameters ConstruirParametrosEstandar(string usuario, string sistema)

{

var parametros = new DynamicParameters();

parametros.Add("e_usuario_ejecucion", usuario);

parametros.Add("e_sistema_origen",    sistema);

parametros.Add("s_procesado",         dbType: DbType.Boolean, direction: ParameterDirection.InputOutput, value: false);

parametros.Add("s_resultado",         dbType: DbType.String,  direction: ParameterDirection.InputOutput, value: null);

parametros.Add("s_mensaje",           dbType: DbType.String,  direction: ParameterDirection.InputOutput, value: "");

parametros.Add("s_codigo_error",      dbType: DbType.String,  direction: ParameterDirection.InputOutput, value: "00000");

parametros.Add("s_detalle_error",     dbType: DbType.String,  direction: ParameterDirection.InputOutput, value: "");

return parametros;

}

private static ResultadoSp<T> ExtraerResultado<T>(DynamicParameters parametros)

{

var procesado    = parametros.Get<bool>("s_procesado");

var mensaje      = parametros.Get<string?>("s_mensaje")       ?? "";

var codigoError  = parametros.Get<string?>("s_codigo_error")  ?? "";

var detalleError = parametros.Get<string?>("s_detalle_error") ?? "";

var json         = parametros.Get<string?>("s_resultado");

T? resultado = default;

if (procesado && !string.IsNullOrWhiteSpace(json))

{

resultado = JsonSerializer.Deserialize<T>(json, JsonOptions);

}

return new ResultadoSp<T>(procesado, mensaje, codigoError, detalleError, resultado);

}

private static readonly JsonSerializerOptions JsonOptions = new()

{

PropertyNameCaseInsensitive = true

};

}

## 14.2 DTOs adicionales

csharp

public sealed record VentaListadoDto(

long VentaId,

string Codigo,

decimal Total,

string Estado,

DateTime FechaCreacion);

## 14.3 Uso desde un servicio

csharp

public sealed class VentaService

{

private readonly IVentaRepository _ventaRepository;

public VentaService(IVentaRepository ventaRepository)

=> _ventaRepository = ventaRepository;

public async Task<VentaDto> RegistrarAsync(

string usuario, string sistema, long clienteId, decimal total,

CancellationToken ct)

{

var resultado = await _ventaRepository.RegistrarVentaAsync(

usuario, sistema, clienteId, total, ct);

if (!resultado.Procesado)

{

throw new BusinessException(resultado.Mensaje, resultado.CodigoError);

}

return resultado.Resultado!;

}

}

## 14.4 Uso desde un controlador (Minimal API)

csharp

app.MapPost("/api/ventas", async (

RegistrarVentaRequest request,

VentaService service,

CancellationToken ct) =>

{

var venta = await service.RegistrarAsync(

request.Usuario, request.Sistema, request.ClienteId, request.Total, ct);

return Results.Ok(venta);

});

## 15. Tabla Resumen de la Convención

Resumen de la Convención

text

PARÁMETROS

────────────────────────────────

e_  → Entrada

s_  → Salida

ENTRADAS ESTÁNDAR

────────────────────────────────

e_usuario_ejecucion

e_sistema_origen

SALIDAS ESTÁNDAR

────────────────────────────────

s_procesado

s_resultado

s_mensaje

s_codigo_error

s_detalle_error

OBJETOS

────────────────────────────────

pp_ → Procedimiento Principal

ps_ → Procedimiento Secundario

fn_ → Función

tr_ → Trigger

vw_ → Vista

tb_ → Tabla

STACK .NET

────────────────────────────────

Dapper + Npgsql

NpgsqlDataSource (singleton)

IDbConnectionFactory (scoped)

Repositorios (scoped)

## 16. Checklist de Cumplimiento

SQL — Nomenclatura

□

El nombre sigue el patrón pp_<acción>_<entidad> o ps_<acción>_<entidad>.

□

Está en el esquema correcto (core, audit, cat, etc.).

□

Los parámetros de entrada usan prefijo e_.

□

Los parámetros de salida usan prefijo s_.

SQL — Estructura

□

El pp_ inicializa s_procesado, s_resultado, s_mensaje, s_codigo_error, s_detalle_error.

□

Los ps_ tienen la validación de cascada IF s_procesado IS DISTINCT FROM TRUE THEN RETURN;.

□

El pp_ coordina todos los ps_ necesarios.

□

El pp_ retorna el resultado final.

SQL — Transacción

□

Solo el pp_ tiene COMMIT / ROLLBACK.

□

Los ps_ no tienen COMMIT / ROLLBACK / BEGIN / SAVEPOINT.

□

El pp_ de escritura hace COMMIT en éxito y ROLLBACK en error funcional o técnico.

□

El pp_ de consulta no hace COMMIT ni ROLLBACK.

SQL — Excepciones

□

Solo el pp_ tiene EXCEPTION WHEN OTHERS.

□

Los ps_ no tienen EXCEPTION.

□

El bloque EXCEPTION llena s_codigo_error := SQLSTATE y s_detalle_error := SQLERRM.

SQL — Auditoría

□

Cada ps_ que ejecuta DML invoca audit.fn_registrar_variacion.

□

El JSON auditado es el registro completo (to_jsonb(NEW)).

□

El campo motivo describe la operación.

□

Las operaciones masivas usan INSERT...SELECT ... RETURNING para auditar todas las filas.

SQL — DML y rendimiento

□

Ningún pp_ ejecuta DML directo (salvo casos triviales justificados).

□

Toda mutación vive en un ps_.

□

No hay cursores (CURSOR, FETCH).

□

No hay hardcoding de valores de negocio.

SQL — Errores

□

Errores funcionales: s_procesado := FALSE; s_codigo_error := '00000'; s_mensaje := '...'.

□

Errores técnicos: s_codigo_error := SQLSTATE; s_detalle_error := SQLERRM.

.NET — Dapper + Npgsql

□

NpgsqlDataSource registrado como singleton.

□

IDbConnectionFactory registrado como scoped.

□

Repositorios registrados como scoped.

□

Los SPs se invocan con CommandType.StoredProcedure y DynamicParameters.

□

Los parámetros INOUT se declaran con ParameterDirection.InputOutput.

□

El s_resultado (JSONB) se deserializa con System.Text.Json.

□

Se capturan PostgresException y NpgsqlException en la capa de datos.

□

No se usa EF Core para escritura.

□

No se usa CommandType.Text con CALL cuando se pueda usar StoredProcedure.

📌 Declaración Final

Este documento constituye el Estándar Oficial de Convenciones de Base de Datos PostgreSQL con Dapper para el desarrollo corporativo. Su cumplimiento es obligatorio para todo nuevo desarrollo y recomendado para refactorizaciones de código existente.

Cualquier excepción a estas reglas debe ser aprobada por el Arquitecto de Base de Datos y documentada con su justificación.



| Capa | Tecnología | Paquete NuGet |

| --- | --- | --- |

| Driver PostgreSQL | Npgsql | Npgsql |

| Gestión de conexiones | NpgsqlDataSource | Npgsql.DependencyInjection |

| Micro-ORM | Dapper | Dapper |

| Serialización JSON | System.Text.Json | Incluido en .NET |

| Inyección de dependencias | Microsoft.Extensions.DependencyInjection | Incluido en .NET |





| Operación | Herramienta | Método |

| --- | --- | --- |

| SELECT simple | Dapper | QueryAsync<T> |

| SELECT con parámetros | Dapper | QueryAsync<T> con parámetros anónimos |

| Reportes / agregaciones | Dapper | QueryAsync<T> con SQL explícito |

| Invocar pp_ / ps_ | Dapper | ExecuteAsync con CommandType.StoredProcedure |

| Operaciones masivas | SP con JSONB | Dapper + DynamicParameters |

| Auditoría | SP | Se ejecuta dentro del ps_ |





| Capa | ¿Lleva bloque EXCEPTION? |

| --- | --- |

| pp_ (Procedimiento Principal) | ✅ SÍ — obligatorio |

| ps_ (Procedimiento Secundario) | ❌ NO |

| fn_ (Función) | ⚠️ Solo si es estrictamente necesario |

| tr_ (Trigger) | ⚠️ Solo si es estrictamente necesario |





| Capa | ¿Maneja transacción? |

| --- | --- |

| pp_ (Procedimiento Principal) | ✅ SÍ — cuando corresponde |

| ps_ (Procedimiento Secundario) | ❌ NO — jamás |





| Tipo de pp_ | ¿Transacción explícita? | Ejemplo |

| --- | --- | --- |

| Escritura (registrar, actualizar, anular, eliminar) | ✅ Sí | pp_registrar_venta |

| Consulta (obtener, listar, buscar) | ❌ No | pp_obtener_venta, pp_listar_ventas |

| Procesamiento mixto | ✅ Sí | pp_procesar_cierre_mes |





| Prefijo | Objeto | Ejemplo |

| --- | --- | --- |

| pp_ | Procedimiento Principal | pp_registrar_venta |

| ps_ | Procedimiento Secundario | ps_validar_venta |

| fn_ | Función | fn_calcular_iva |

| tr_ | Trigger | tr_actualizar_fecha |

| vw_ | Vista | vw_ventas |

| tb_ | Tabla | tb_venta |





| Acción | Uso |

| --- | --- |

| registrar | Crear una operación |

| actualizar | Modificar información |

| eliminar | Eliminar |

| anular | Anular una operación |

| aprobar | Aprobar |

| rechazar | Rechazar |

| procesar | Ejecutar un procesamiento |

| obtener | Obtener un registro |

| listar | Obtener múltiples registros |

| buscar | Búsqueda con criterios |

| validar | Validación |

| calcular | Cálculo |





| Prefijo | Tipo | Ejemplo |

| --- | --- | --- |

| e_ | Entrada | e_cliente_fk |

| s_ | Salida | s_procesado |





| Parámetro | Tipo | Descripción |

| --- | --- | --- |

| s_procesado | BOOLEAN | Indica si el procesamiento fue exitoso |

| s_resultado | JSONB | Resultado de la operación |

| s_mensaje | TEXT | Mensaje funcional o informativo |

| s_codigo_error | TEXT | Código del error, cuando corresponda |

| s_detalle_error | TEXT | Detalle técnico de la excepción |





| Opción | Contrato | Payload | Curva | Ideal para |

| --- | --- | --- | --- | --- |

| JSON (REST) | Débil | Grande | Baja | MVPs, APIs simples |

| DTO + REST | Fuerte | Medio | Media | APIs empresariales |

| gRPC / Protobuf | Muy fuerte | Pequeño | Alta | Microservicios, alto rendimiento |

| GraphQL | Fuerte | Ajustable | Alta | Frontends complejos, mobile |





| Atributo | pp_ (Principal) | ps_ (Secundario) |

| --- | --- | --- |

| Punto de entrada | ✅ | ❌ |

| Inicializa s_procesado y salidas | ✅ | ❌ |

| Coordina llamadas | ✅ | ✅ (a otros ps_, si aplica) |

| Ejecuta DML (INSERT/UPDATE/DELETE) | ⚠️ Solo si es trivial y único | ✅ (responsabilidad principal) |

| COMMIT / ROLLBACK | ✅ Cuando corresponde | ❌ Nunca |

| BEGIN / SAVEPOINT | ✅ Cuando corresponde | ❌ Nunca |

| Bloque EXCEPTION WHEN OTHERS | ✅ Único lugar | ❌ Nunca |

| Maneja errores funcionales (s_procesado := FALSE) | ✅ | ✅ |

| Llena s_codigo_error / s_detalle_error técnico | ✅ (en EXCEPTION) | ❌ |

| Retorna resultado a la aplicación | ✅ | ❌ |

| Llamado directamente por Dapper | ✅ | ❌ |

| Llamado por otro pp_ | ❌ | ✅ |

| Llamado por otro ps_ | ❌ | ✅ |
