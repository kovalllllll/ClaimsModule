using System.Text.Json;
using System.Text.Json.Serialization;
using ClaimsModule.Application.Common.Validation;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.API.Serialization;

/// <summary>
/// Returns FRS 8 validation message for unknown reserve component values (HTTP 422).
/// </summary>
public sealed class StrictReserveComponentTypeJsonConverter : JsonConverter<ReserveComponentType>
{
    public override ReserveComponentType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw CreateInvalidComponentException();

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse<ReserveComponentType>(value, ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
        {
            throw CreateInvalidComponentException();
        }

        return parsed;
    }

    public override void Write(Utf8JsonWriter writer, ReserveComponentType value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());

    private static JsonException CreateInvalidComponentException()
        => new(ClaimValidationMessages.InvalidReserveComponentType);
}
