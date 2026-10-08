using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using EtaInvoice.Models;

namespace EtaInvoice.Signing
{
    /// <summary>
    /// Produces a deterministic (canonical) JSON representation of a document,
    /// used as the input to <see cref="IDocumentSigner.SignAsync"/>.
    /// <para>
    /// The canonical form is: UTF-8 JSON, no whitespace, object properties sorted
    /// by name (recursively), arrays kept in order, and the <c>signatures</c>
    /// section excluded (per the ETA signature rules: signed content is the entire
    /// document structure except the signature section).
    /// </para>
    /// <para>
    /// <b>Important:</b> always verify the produced canonical string against the
    /// official ETA "Signature Creation" documentation (or the official
    /// EInvoicingSigner tracing tool) before submitting signed documents in
    /// production — canonicalization details are defined by ETA and may evolve.
    /// </para>
    /// </summary>
    public static class EtaCanonicalJson
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes <paramref name="document"/> to canonical JSON, excluding
        /// the <c>signatures</c> section.
        /// </summary>
        public static string ToCanonicalJson(EtaDocument document)
        {
            var json = JsonSerializer.Serialize(document, SerializerOptions);
            using (var doc = JsonDocument.Parse(json))
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream))
                    {
                        WriteCanonical(doc.RootElement, writer, excludeSignatures: true);
                    }
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }

        private static void WriteCanonical(JsonElement element, Utf8JsonWriter writer, bool excludeSignatures)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    var properties = element.EnumerateObject()
                        .Where(p => !(excludeSignatures && p.NameEquals("signatures")))
                        .OrderBy(p => p.Name, System.StringComparer.Ordinal);
                    foreach (var property in properties)
                    {
                        writer.WritePropertyName(property.Name);
                        WriteCanonical(property.Value, writer, excludeSignatures: false);
                    }
                    writer.WriteEndObject();
                    break;

                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                        WriteCanonical(item, writer, excludeSignatures: false);
                    writer.WriteEndArray();
                    break;

                default:
                    element.WriteTo(writer);
                    break;
            }
        }
    }
}
