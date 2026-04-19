/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Text.Json;

namespace BexioApiNet.Abstractions.Json;

/// <summary>
/// Reusable <see cref="JsonConverter{T}" /> base for discriminated-union style polymorphic
/// hierarchies. Derived converters only need to name the discriminator JSON property and map
/// the discriminator value to a concrete CLR type. On write, each value is serialized under
/// its runtime type so the concrete record's own property metadata drives the output.
/// </summary>
/// <remarks>
/// Registering the converter with <c>[JsonConverter(typeof(...))]</c> on the abstract base only
/// applies to that base (the attribute lookup uses <c>inherit: false</c>); concrete subtypes
/// therefore do not re-enter the converter on write and the <c>Read</c>/<c>Write</c> paths
/// stay recursion-free.
/// </remarks>
/// <typeparam name="TBase">The polymorphic base type handled by this converter.</typeparam>
public abstract class DiscriminatedJsonConverter<TBase> : JsonConverter<TBase>
    where TBase : class
{
    /// <summary>
    /// Name of the JSON property that carries the discriminator value (e.g. <c>"type"</c>).
    /// </summary>
    protected abstract string DiscriminatorPropertyName { get; }

    /// <summary>
    /// Map a discriminator value read from the JSON payload to the concrete CLR type that
    /// should be instantiated. Return <see langword="null" /> to signal an unknown value and
    /// have <see cref="Read" /> throw a <see cref="JsonException" />.
    /// </summary>
    /// <param name="discriminator">The discriminator string extracted from the payload.</param>
    /// <returns>The concrete subtype to deserialize into, or <see langword="null" /> if unknown.</returns>
    protected abstract Type? ResolveType(string discriminator);

    /// <inheritdoc />
    public override TBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null;

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (root.ValueKind is not JsonValueKind.Object)
            throw new JsonException($"Expected JSON object for {typeof(TBase).Name}, found {root.ValueKind}.");

        if (!root.TryGetProperty(DiscriminatorPropertyName, out var discriminatorElement)
            || discriminatorElement.ValueKind is not JsonValueKind.String)
        {
            throw new JsonException(
                $"Missing or non-string discriminator '{DiscriminatorPropertyName}' on {typeof(TBase).Name}.");
        }

        var discriminator = discriminatorElement.GetString()!;
        var concreteType = ResolveType(discriminator)
            ?? throw new JsonException(
                $"Unknown discriminator '{discriminator}' for {typeof(TBase).Name}.");

        return (TBase?)root.Deserialize(concreteType, options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);
}
