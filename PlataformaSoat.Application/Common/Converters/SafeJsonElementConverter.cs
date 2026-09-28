using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Common.Converters;

/// <summary>
/// Convertidor seguro para JsonElement que serializa como null cuando el elemento no está inicializado
/// (ValueKind == JsonValueKind.Undefined), evitando excepciones en respuestas donde o_response es NULL.
/// </summary>
public class SafeJsonElementConverter : JsonConverter<JsonElement>
{
    public override JsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return doc.RootElement.Clone();
    }

    public override void Write(Utf8JsonWriter writer, JsonElement value, JsonSerializerOptions options)
    {
        if (value.ValueKind == JsonValueKind.Undefined)
        {
            writer.WriteNullValue();
        }
        else
        {
            value.WriteTo(writer);
        }
    }
}
