using Npgsql;
using NpgsqlTypes;

namespace PlataformaSoat.Infrastructure.Helpers
{
    public static class PgHelper
    {
        /// <summary>
        /// Creates an INOUT NpgsqlParameter.
        /// </summary>
        public static NpgsqlParameter InOut(string name, NpgsqlDbType dbType, object? value = null)
        {
            return new NpgsqlParameter(name, dbType)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = value ?? DBNull.Value
            };
        }

        /// <summary>
        /// Safely retrieves the value from a parameter, handling DBNull and nulls.
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
    }
}
