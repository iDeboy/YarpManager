using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YarpManager.Common;

namespace YarpManager.Acme.Jws.Converters;
internal sealed partial class EnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum {

    private static readonly TypeCode s_enumTypeCode = Type.GetTypeCode(typeof(TEnum));

    private static readonly bool s_isSignedEnum = ((int)s_enumTypeCode % 2) == 1;
    private static readonly bool s_isFlagsEnum = typeof(TEnum).IsDefined(typeof(FlagsAttribute), inherit: false);

    public static readonly Regex IntegerRegex = CreateIntegerRegex();
    private const string IntegerRegexPattern = @"^\s*(?:\+|\-)?[0-9]+\s*$";

    [GeneratedRegex(IntegerRegexPattern, RegexOptions.None, matchTimeoutMilliseconds: 200)]
    private static partial Regex CreateIntegerRegex();

    private readonly bool _convertToStr;

    private readonly EnumFieldInfo[] _enumFieldInfo;

    private readonly Dictionary<string, EnumFieldInfo> _enumFieldInfoIndex;

    private readonly ConcurrentDictionary<ulong, JsonEncodedText> _nameCacheForWriting;
    private readonly ConcurrentDictionary<string, ulong> _nameCacheForReading;

    public EnumConverter(bool convertToStr) {
        _convertToStr = convertToStr;

        _enumFieldInfo = ResolveEnumFields(null);
        _enumFieldInfoIndex = new(StringComparer.OrdinalIgnoreCase);
        _nameCacheForWriting = new();
        _nameCacheForReading = new(StringComparer.Ordinal);
        JavaScriptEncoder? encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        foreach (EnumFieldInfo fieldInfo in _enumFieldInfo) {
            AddToEnumFieldIndex(fieldInfo);

            JsonEncodedText encodedName = JsonEncodedText.Encode(fieldInfo.JsonName, encoder);
            _nameCacheForWriting.TryAdd(fieldInfo.Key, encodedName);
            _nameCacheForReading.TryAdd(fieldInfo.JsonName, fieldInfo.Key);
        }

    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

        switch (reader.TokenType) {

            case JsonTokenType.String when _convertToStr:
                if (TryParseEnumFromString(ref reader, out TEnum result)) {
                    return result;
                }
                break;
            case JsonTokenType.Number when !_convertToStr:
                switch (s_enumTypeCode) {
                    case TypeCode.Int32 when reader.TryGetInt32(out int int32): return Unsafe.As<int, TEnum>(ref int32);
                    case TypeCode.UInt32 when reader.TryGetUInt32(out uint uint32): return Unsafe.As<uint, TEnum>(ref uint32);
                    case TypeCode.Int64 when reader.TryGetInt64(out long int64): return Unsafe.As<long, TEnum>(ref int64);
                    case TypeCode.UInt64 when reader.TryGetUInt64(out ulong uint64): return Unsafe.As<ulong, TEnum>(ref uint64);
                    case TypeCode.Byte when reader.TryGetByte(out byte ubyte8): return Unsafe.As<byte, TEnum>(ref ubyte8);
                    case TypeCode.SByte when reader.TryGetSByte(out sbyte byte8): return Unsafe.As<sbyte, TEnum>(ref byte8);
                    case TypeCode.Int16 when reader.TryGetInt16(out short int16): return Unsafe.As<short, TEnum>(ref int16);
                    case TypeCode.UInt16 when reader.TryGetUInt16(out ushort uint16): return Unsafe.As<ushort, TEnum>(ref uint16);
                }
                break;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) {

        if (_convertToStr) {

            ulong key = ConvertToUInt64(value);

            if (_nameCacheForWriting.TryGetValue(key, out JsonEncodedText formatted)) {
                writer.WriteStringValue(formatted);
                return;
            }

            if (IsDefinedValueOrCombinationOfValues(key)) {

                Debug.Assert(s_isFlagsEnum, "Should only be entered by flags enums.");

                string stringValue = FormatEnumAsString(key, value, dictionaryKeyPolicy: null);

                if (_nameCacheForWriting.Count < 64) {
                    formatted = JsonEncodedText.Encode(stringValue, options.Encoder);
                    writer.WriteStringValue(formatted);
                    _nameCacheForWriting.TryAdd(key, formatted);
                }
                else {
                    // We also do not create a JsonEncodedText instance here because passing the string
                    // directly to the writer is cheaper than creating one and not caching it for reuse.
                    writer.WriteStringValue(stringValue);
                }

                return;
            }

            throw new JsonException();
        }

        if (s_isSignedEnum) {
            writer.WriteNumberValue(ConvertToInt64(value));
        }
        else {
            writer.WriteNumberValue(ConvertToUInt64(value));
        }
    }

    private static long ConvertToInt64(TEnum value) {
        Debug.Assert(s_isSignedEnum);
        switch (s_enumTypeCode) {
            case TypeCode.Int32: return Unsafe.As<TEnum, int>(ref value);
            case TypeCode.Int64: return Unsafe.As<TEnum, long>(ref value);
            case TypeCode.Int16: return Unsafe.As<TEnum, short>(ref value);
            default:
                Debug.Assert(s_enumTypeCode is TypeCode.SByte);
                return Unsafe.As<TEnum, sbyte>(ref value);
        };
    }

    private string FormatEnumAsString(ulong key, TEnum value, JsonNamingPolicy? dictionaryKeyPolicy) {
        Debug.Assert(IsDefinedValueOrCombinationOfValues(key), "must only be invoked against valid enum values.");
        Debug.Assert(
            s_isFlagsEnum || (dictionaryKeyPolicy is not null && Enum.IsDefined(typeof(TEnum), value)),
            "must either be a flag type or computing a dictionary key policy.");

        if (s_isFlagsEnum) {
            StringBuilder sb = new();
            ulong remainingBits = key;

            foreach (EnumFieldInfo enumField in _enumFieldInfo) {
                ulong fieldKey = enumField.Key;
                if (fieldKey == 0 ? key == 0 : (remainingBits & fieldKey) == fieldKey) {
                    remainingBits &= ~fieldKey;
                    string name = dictionaryKeyPolicy is not null
                        ? ResolveAndValidateJsonName(enumField.OriginalName, dictionaryKeyPolicy, enumField.Kind)
                        : enumField.JsonName;

                    if (sb.Length > 0) {
                        sb.Append(", ");
                    }

                    sb.Append(name);

                    if (remainingBits == 0) {
                        break;
                    }
                }
            }

            Debug.Assert(remainingBits == 0 && sb.Length > 0, "unexpected remaining bits or empty string.");
            return sb.ToString();
        }
        else {
            Debug.Assert(dictionaryKeyPolicy != null);

            foreach (EnumFieldInfo enumField in _enumFieldInfo) {
                // Search for an exact match and apply the key policy.
                if (enumField.Key == key) {
                    return ResolveAndValidateJsonName(enumField.OriginalName, dictionaryKeyPolicy, enumField.Kind);
                }
            }

            Debug.Fail("should not have been reached.");
            return null;
        }
    }

    private bool IsDefinedValueOrCombinationOfValues(ulong key) {
        if (s_isFlagsEnum) {
            ulong remainingBits = key;

            foreach (EnumFieldInfo fieldInfo in _enumFieldInfo) {
                ulong fieldKey = fieldInfo.Key;
                if (fieldKey == 0 ? key == 0 : (remainingBits & fieldKey) == fieldKey) {
                    remainingBits &= ~fieldKey;

                    if (remainingBits == 0) {
                        return true;
                    }
                }
            }

            return false;
        }
        else {
            foreach (EnumFieldInfo fieldInfo in _enumFieldInfo) {
                if (fieldInfo.Key == key) {
                    return true;
                }
            }

            return false;
        }
    }

    private bool TryParseEnumFromString(ref Utf8JsonReader reader, out TEnum result) {

        Debug.Assert(reader.TokenType is JsonTokenType.String or JsonTokenType.PropertyName);

        int bufferLength = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;

        bool success;

        using var charBuffer = new PooledArray<char>(bufferLength);
        int charsWritten = reader.CopyString(charBuffer);

        ConcurrentDictionary<string, ulong> lookup = _nameCacheForReading;
        string source = charBuffer.AsSpan(0, charsWritten).Trim().ToString();

        if (lookup.TryGetValue(source, out ulong key)) {
            result = ConvertFromUInt64(key);
            success = true;
            goto End;
        }

        if (IntegerRegex.IsMatch(source)) {
            // We found an integer that is not an enum field name.
            success = Enum.TryParse(source, out result);
        }
        else {
            success = TryParseNamedEnum(source, out result);
        }

    End:
        charBuffer.AsSpan(0, charsWritten).Clear();

        return success;

    }

    private bool TryParseNamedEnum(ReadOnlySpan<char> source, out TEnum result) {

        Dictionary<string, EnumFieldInfo> lookup = _enumFieldInfoIndex;
        ReadOnlySpan<char> rest = source;

        ulong key = 0;

        do {
            ReadOnlySpan<char> next;
            int i = source.IndexOf(',');
            if (i == -1) {
                next = rest;
                rest = default;
            }
            else {
                next = rest[..i].TrimEnd();
                rest = rest[(i + 1)..].TrimStart();
            }
            if (lookup.TryGetValue(next.ToString(), out EnumFieldInfo? firstResult) &&
                    firstResult.GetMatchingField(next) is EnumFieldInfo match) {
                key |= match.Key;
                continue;
            }
            result = default;
            return false;

        } while (!rest.IsEmpty);

        result = ConvertFromUInt64(key);
        return true;

    }

    private record EnumFieldInfo(ulong Key, EnumFieldNameKind Kind, string OriginalName, string JsonName) {

        private List<EnumFieldInfo>? _conflictingFields;

        public void AppendConflictingField(EnumFieldInfo other) {
            Debug.Assert(JsonName.Equals(other.JsonName, StringComparison.OrdinalIgnoreCase), "The conflicting entry must be equal up to case insensitivity.");

            if (ConflictsWith(this, other)) {
                // Silently discard if the preceding entry is the default or has identical name.
                return;
            }

            List<EnumFieldInfo> conflictingFields = _conflictingFields ??= [];

            // Walk the existing list to ensure we do not add duplicates.
            foreach (EnumFieldInfo conflictingField in conflictingFields) {
                if (ConflictsWith(conflictingField, other)) {
                    return;
                }
            }

            conflictingFields.Add(other);

            // Determines whether the first field info matches everything that the second field info matches,
            // in which case the second field info is redundant and doesn't need to be added to the list.
            static bool ConflictsWith(EnumFieldInfo current, EnumFieldInfo other) {
                // The default name matches everything case-insensitively.
                if (current.Kind is EnumFieldNameKind.Default) {
                    return true;
                }

                // current matches case-sensitively since it's not the default name.
                // other matches case-insensitively, so it matches more than current.
                if (other.Kind is EnumFieldNameKind.Default) {
                    return false;
                }

                // Both are case-sensitive so they need to be identical.
                return current.JsonName.Equals(other.JsonName, StringComparison.Ordinal);
            }
        }

        public EnumFieldInfo? GetMatchingField(ReadOnlySpan<char> input) {
            Debug.Assert(input.Equals(JsonName.AsSpan(), StringComparison.OrdinalIgnoreCase), "Must equal the field name up to case insensitivity.");

            if (Kind is EnumFieldNameKind.Default || input.SequenceEqual(JsonName.AsSpan())) {
                // Default enum names use case insensitive parsing so are always a match.
                return this;
            }

            if (_conflictingFields is { } conflictingFields) {
                Debug.Assert(conflictingFields.Count > 0);
                foreach (EnumFieldInfo matchingField in conflictingFields) {
                    if (matchingField.Kind is EnumFieldNameKind.Default || input.SequenceEqual(matchingField.JsonName.AsSpan())) {
                        return matchingField;
                    }
                }
            }

            return null;
        }

    }

    private static EnumFieldInfo[] ResolveEnumFields(JsonNamingPolicy? namingPolicy) {
        string[] names = Enum.GetNames<TEnum>();
        TEnum[] values = Enum.GetValues<TEnum>();

        Debug.Assert(names.Length == values.Length);

        Dictionary<TEnum, (string Name, EnumFieldNameKind Kind)> includingMembers = [];
        HashSet<string>? ignoreMembers = null;
        Dictionary<string, string>? enumMemberAttributes = null;
        foreach (FieldInfo field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)) {

            if (field.GetCustomAttribute<JsonIgnoreAttribute>() is not null) {
                // (ignoreMembers ??= new(StringComparer.Ordinal)).Add(field.Name);
                continue;
            }

            var value = Enum.Parse<TEnum>(field.Name);

            if (field.GetCustomAttribute<EnumMemberAttribute>() is { Value: not null } attribute) {
                // (enumMemberAttributes ??= new(StringComparer.Ordinal)).Add(field.Name, attribute.Value);

                includingMembers.TryAdd(value, (attribute.Value, EnumFieldNameKind.Attribute));
                continue;
            }

            includingMembers.TryAdd(value, (field.Name, namingPolicy is not null ? EnumFieldNameKind.NamingPolicy : EnumFieldNameKind.Default));
        }

        var enumFields = new EnumFieldInfo[includingMembers.Count];
        int i = 0;
        foreach (var (value, (name, kind)) in includingMembers) {

            ulong key = ConvertToUInt64(value);
            string jsonName = ResolveAndValidateJsonName(name, namingPolicy, kind);

            enumFields[i++] = new EnumFieldInfo(key, kind, name, jsonName);
        }

        //var enumFields = new EnumFieldInfo[names.Length - (ignoreMembers?.Count ?? 0)];

        //for (int i = 0; i < names.Length; ++i) {
        //    string originalName = names[i];

        //    if (ignoreMembers is not null && ignoreMembers.Contains(originalName)) {
        //        --i;
        //        continue;
        //    }

        //    TEnum value = values[i];
        //    ulong key = ConvertToUInt64(value);
        //    EnumFieldNameKind kind;

        //    if (enumMemberAttributes is not null && enumMemberAttributes.TryGetValue(originalName, out string? attributeName)) {
        //        originalName = attributeName;
        //        kind = EnumFieldNameKind.Attribute;
        //    }
        //    else {
        //        kind = namingPolicy is not null ? EnumFieldNameKind.NamingPolicy : EnumFieldNameKind.Default;
        //    }

        //    string jsonName = ResolveAndValidateJsonName(originalName, namingPolicy, kind);
        //    enumFields[i] = new EnumFieldInfo(key, kind, originalName, jsonName);
        //}

        return enumFields;
    }

    private static string ResolveAndValidateJsonName(string name, JsonNamingPolicy? namingPolicy, EnumFieldNameKind kind) {
        if (kind is not EnumFieldNameKind.Attribute && namingPolicy is not null) {
            // Do not apply a naming policy to names that are explicitly set via attributes.
            // This is consistent with JsonPropertyNameAttribute semantics.
            name = namingPolicy.ConvertName(name);
        }

        if (string.IsNullOrEmpty(name) || char.IsWhiteSpace(name[0]) || char.IsWhiteSpace(name[^1]) ||
            (s_isFlagsEnum && name.AsSpan().IndexOf(',') >= 0)) {
            // Reject null or empty strings or strings with leading or trailing whitespace.
            // In the case of flags additionally reject strings containing commas.
            throw new NotSupportedException();
        }

        return name;
    }

    private static ulong ConvertToUInt64(TEnum value) {
        switch (s_enumTypeCode) {
            case TypeCode.Int32 or TypeCode.UInt32: return Unsafe.As<TEnum, uint>(ref value);
            case TypeCode.Int64 or TypeCode.UInt64: return Unsafe.As<TEnum, ulong>(ref value);
            case TypeCode.Int16 or TypeCode.UInt16: return Unsafe.As<TEnum, ushort>(ref value);
            default:
                Debug.Assert(s_enumTypeCode is TypeCode.SByte or TypeCode.Byte);
                return Unsafe.As<TEnum, byte>(ref value);
        };
    }

    private static TEnum ConvertFromUInt64(ulong value) {
        switch (s_enumTypeCode) {
            case TypeCode.Int32 or TypeCode.UInt32:
                uint uintValue = (uint)value;
                return Unsafe.As<uint, TEnum>(ref uintValue);

            case TypeCode.Int64 or TypeCode.UInt64:
                ulong ulongValue = value;
                return Unsafe.As<ulong, TEnum>(ref ulongValue);

            case TypeCode.Int16 or TypeCode.UInt16:
                ushort ushortValue = (ushort)value;
                return Unsafe.As<ushort, TEnum>(ref ushortValue);

            default:
                Debug.Assert(s_enumTypeCode is TypeCode.SByte or TypeCode.Byte);
                byte byteValue = (byte)value;
                return Unsafe.As<byte, TEnum>(ref byteValue);
        };
    }

    private void AddToEnumFieldIndex(EnumFieldInfo fieldInfo) {
        if (!_enumFieldInfoIndex.TryAdd(fieldInfo.JsonName, fieldInfo)) {
            // We have a casing conflict, append field to the existing entry.
            EnumFieldInfo existingFieldInfo = _enumFieldInfoIndex[fieldInfo.JsonName];
            existingFieldInfo.AppendConflictingField(fieldInfo);
        }
    }

    private enum EnumFieldNameKind {
        Default = 0,
        NamingPolicy = 1,
        Attribute = 2,
    }
}
