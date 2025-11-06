using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Json-конвертер, который обрезает входящее сообщение до допустимой длины.
/// </summary>
internal sealed class CallbackMessageJsonConverter : JsonConverter<string?>
{
    private const int MaxLength = 10_000;

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= MaxLength ? value : value[..MaxLength];
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(value))
        {
            writer.WriteNullValue();
            return;
        }

        var output = value.Length <= MaxLength ? value : value[..MaxLength];
        writer.WriteStringValue(output);
    }
}
