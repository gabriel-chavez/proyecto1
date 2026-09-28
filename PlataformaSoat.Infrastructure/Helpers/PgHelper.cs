using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace PlataformaSoat.Infrastructure.Helpers
{
    public static class PgHelper
    {
        /// <summary>
        /// Crea un NpgsqlParameter de entrada (IN) asegurando el prefijo '@'.
        /// </summary>
        public static NpgsqlParameter In(string name, NpgsqlDbType dbType, object? value = null)
        {
            var cleanName = name.StartsWith('@') ? name : "@" + name;
            return new NpgsqlParameter(cleanName, dbType)
            {
                Value = value ?? DBNull.Value
            };
        }

        /// <summary>
        /// Crea un NpgsqlParameter de entrada/salida (INOUT) asegurando el prefijo '@'.
        /// </summary>
        public static NpgsqlParameter InOut(string name, NpgsqlDbType dbType, object? value = null)
        {
            var cleanName = name.StartsWith('@') ? name : "@" + name;
            return new NpgsqlParameter(cleanName, dbType)
            {
                Direction = ParameterDirection.InputOutput,
                Value = value ?? DBNull.Value
            };
        }

        /// <summary>
        /// Obtiene de forma segura el valor de un parámetro, manejando DBNull y nulls.
        /// </summary>
        public static T Get<T>(NpgsqlParameter parameter)
        {
            if (parameter.Value == DBNull.Value || parameter.Value == null)
            {
                return default!;
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)parameter.Value.ToString()!;
            }

            return (T)parameter.Value;
        }

        /// <summary>
        /// Construye el texto SQL "CALL schema.sp(@p1, @p2, ...)" garantizando siempre el prefijo '@' en los parámetros.
        /// </summary>
        public static string BuildCall(string procedureName, IReadOnlyList<NpgsqlParameter> parameters)
        {
            var sb = new System.Text.StringBuilder(procedureName.Length + parameters.Count * 14);
            sb.Append("CALL ").Append(procedureName).Append('(');

            for (int i = 0; i < parameters.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var name = parameters[i].ParameterName;
                if (!name.StartsWith('@'))
                    sb.Append('@');
                sb.Append(name);
            }

            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Convierte una lista de parámetros en un diccionario nombre → valor
        /// (con DBNull convertido a null) insensible a mayúsculas/minúsculas y accesible con o sin '@'.
        /// </summary>
        public static Dictionary<string, object?> ToDictionary(IReadOnlyList<NpgsqlParameter> parameters)
        {
            var dict = new Dictionary<string, object?>(parameters.Count * 2, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                var trimmed = p.ParameterName.TrimStart('@');
                var val = p.Value == DBNull.Value ? null : p.Value;
                dict[trimmed] = val;
                dict["@" + trimmed] = val;
            }
            return dict;
        }
    }
}