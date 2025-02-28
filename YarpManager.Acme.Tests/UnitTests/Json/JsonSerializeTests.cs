using System;
using System.Text.Json;
using Xunit.Abstractions;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Tests.UnitTests.Json.Models;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Tests.UnitTests.Json;
public class JsonSerializeTests {

    private readonly ITestOutputHelper _output;

    public JsonSerializeTests(ITestOutputHelper output) {
        _output = output;
    }

    [Fact]
    public void JsonConvert_Normal() {

        _output.WriteLine("Starting serialization...");

        var normalClass = new NormalClass();
        var jsonClass = JsonSerializer.Serialize(normalClass, JsonUtils.SerializerOptions);
        Assert.Equal("""{"field1":1,"field2":2,"field3":"2","field4":"4"}""", jsonClass);
        _output.WriteLine("    NormalClass ok");

        var normalStruct = new NormalStruct();
        var jsonStruct = JsonSerializer.Serialize(normalStruct, JsonUtils.SerializerOptions);
        Assert.Equal("""{"field1":1,"field2":2,"field3":"2","field4":"4"}""", jsonStruct);
        _output.WriteLine("    NormalStruct ok");

        _output.WriteLine("Serialization ok");

        _output.WriteLine("Starting deserialization...");

        var normalClassResult = JsonSerializer.Deserialize<NormalClass>(jsonClass, JsonUtils.SerializerOptions);
        Assert.NotNull(normalClassResult);
        _output.WriteLine("    NormalClass ok");

        var normalStructResult = JsonSerializer.Deserialize<NormalStruct?>(jsonStruct, JsonUtils.SerializerOptions);
        Assert.NotNull(normalStructResult);
        _output.WriteLine("    NormalStruct ok");

        _output.WriteLine("Deserialization ok");

    }

    [Fact]
    public void Serialize_With_Base64UrlAtrribute_Property() {

        _output.WriteLine("Starting serialization...");

        var base64PropertyClass = new Base64PropertyClass();
        var jsonClass = JsonSerializer.Serialize(base64PropertyClass, JsonUtils.SerializerOptions);
        Assert.Equal("""{"field1":1,"field2":"Mg","field3":"2","field4":"NA","normalClass":{"field1":1,"field2":2,"field3":"2","field4":"4"},"normalStruct":"eyJmaWVsZDEiOjEsImZpZWxkMiI6MiwiZmllbGQzIjoiMiIsImZpZWxkNCI6IjQifQ"}""",
            jsonClass);
        _output.WriteLine("    Base64PropertyClass ok");

        var base64PropertyStruct = new Base64PropertyStruct();
        var jsonStruct = JsonSerializer.Serialize(base64PropertyStruct, JsonUtils.SerializerOptions);
        Assert.Equal("""{"field1":1,"field2":"Mg","field3":"2","field4":"NA","normalClass":"eyJmaWVsZDEiOjEsImZpZWxkMiI6MiwiZmllbGQzIjoiMiIsImZpZWxkNCI6IjQifQ","normalStruct":{"field1":1,"field2":2,"field3":"2","field4":"4"}}""",
            jsonStruct);
        _output.WriteLine("    Base64PropertyStruct ok");

        _output.WriteLine("Serialization ok");

        _output.WriteLine("Starting deserialization...");

        var base64PropertyClassResult = JsonSerializer.Deserialize<Base64PropertyClass>(jsonClass, JsonUtils.SerializerOptions);
        Assert.NotNull(base64PropertyClassResult);
        _output.WriteLine("    Base64PropertyClass ok");

        var base64PropertyStructResult = JsonSerializer.Deserialize<Base64PropertyStruct?>(jsonStruct, JsonUtils.SerializerOptions);
        Assert.NotNull(base64PropertyStructResult);
        _output.WriteLine("    Base64PropertyStruct ok");

        _output.WriteLine("Deserialization ok");
    }

    [Fact]
    public void Serialize_With_Base64UrlAtrribute_Class() {

        _output.WriteLine("Starting serialization...");

        var base64Class = new Base64Class();
        var jsonClass = JsonSerializer.Serialize(base64Class, JsonUtils.SerializerOptions);
        Assert.Equal("\"eyJmaWVsZDEiOjEsImZpZWxkMiI6Ik1nIiwiZmllbGQzIjoiMiIsImZpZWxkNCI6Ik5BIiwiYmFzZTY0UHJvcGVydHlDbGFzcyI6eyJmaWVsZDEiOjEsImZpZWxkMiI6Ik1nIiwiZmllbGQzIjoiMiIsImZpZWxkNCI6Ik5BIiwibm9ybWFsQ2xhc3MiOnsiZmllbGQxIjoxLCJmaWVsZDIiOjIsImZpZWxkMyI6IjIiLCJmaWVsZDQiOiI0In0sIm5vcm1hbFN0cnVjdCI6ImV5Sm1hV1ZzWkRFaU9qRXNJbVpwWld4a01pSTZNaXdpWm1sbGJHUXpJam9pTWlJc0ltWnBaV3hrTkNJNklqUWlmUSJ9LCJiYXNlNjRQcm9wZXJ0eVN0cnVjdCI6ImV5Sm1hV1ZzWkRFaU9qRXNJbVpwWld4a01pSTZJazFuSWl3aVptbGxiR1F6SWpvaU1pSXNJbVpwWld4a05DSTZJazVCSWl3aWJtOXliV0ZzUTJ4aGMzTWlPaUpsZVVwdFlWZFdjMXBFUldsUGFrVnpTVzFhY0ZwWGVHdE5hVWsyVFdsM2FWcHRiR3hpUjFGNlNXcHZhVTFwU1hOSmJWcHdXbGQ0YTA1RFNUWkphbEZwWmxFaUxDSnViM0p0WVd4VGRISjFZM1FpT25zaVptbGxiR1F4SWpveExDSm1hV1ZzWkRJaU9qSXNJbVpwWld4a015STZJaklpTENKbWFXVnNaRFFpT2lJMEluMTkifQ\"",
                    jsonClass);
        _output.WriteLine("    Base64Class ok");

        var base64Struct = new Base64Struct();
        var jsonStruct = JsonSerializer.Serialize(base64Struct, JsonUtils.SerializerOptions);
        Assert.Equal("\"eyJmaWVsZDEiOjEsImZpZWxkMiI6Ik1nIiwiZmllbGQzIjoiMiIsImZpZWxkNCI6Ik5BIiwiYmFzZTY0UHJvcGVydHlDbGFzcyI6ImV5Sm1hV1ZzWkRFaU9qRXNJbVpwWld4a01pSTZJazFuSWl3aVptbGxiR1F6SWpvaU1pSXNJbVpwWld4a05DSTZJazVCSWl3aWJtOXliV0ZzUTJ4aGMzTWlPbnNpWm1sbGJHUXhJam94TENKbWFXVnNaRElpT2pJc0ltWnBaV3hrTXlJNklqSWlMQ0ptYVdWc1pEUWlPaUkwSW4wc0ltNXZjbTFoYkZOMGNuVmpkQ0k2SW1WNVNtMWhWMVp6V2tSRmFVOXFSWE5KYlZwd1dsZDRhMDFwU1RaTmFYZHBXbTFzYkdKSFVYcEphbTlwVFdsSmMwbHRXbkJhVjNoclRrTkpOa2xxVVdsbVVTSjkiLCJiYXNlNjRQcm9wZXJ0eVN0cnVjdCI6eyJmaWVsZDEiOjEsImZpZWxkMiI6Ik1nIiwiZmllbGQzIjoiMiIsImZpZWxkNCI6Ik5BIiwibm9ybWFsQ2xhc3MiOiJleUptYVdWc1pERWlPakVzSW1acFpXeGtNaUk2TWl3aVptbGxiR1F6SWpvaU1pSXNJbVpwWld4a05DSTZJalFpZlEiLCJub3JtYWxTdHJ1Y3QiOnsiZmllbGQxIjoxLCJmaWVsZDIiOjIsImZpZWxkMyI6IjIiLCJmaWVsZDQiOiI0In19fQ\"",
            jsonStruct);
        _output.WriteLine("    Base64Struct ok");

        _output.WriteLine("Serialization ok");

        _output.WriteLine("Starting deserialization...");

        var Base64ClassResult = JsonSerializer.Deserialize<Base64Class>(jsonClass, JsonUtils.SerializerOptions);
        Assert.NotNull(Base64ClassResult);
        _output.WriteLine("    Base64Class ok");

        var base64StructResult = JsonSerializer.Deserialize<Base64Struct>(jsonStruct, JsonUtils.SerializerOptions);
        // Assert.NotNull(base64StructResult);
        _output.WriteLine("    Base64Struct ok");

        _output.WriteLine("Deserialization ok");
    }

    [Fact]
    public void Serialize_CustomStructure_Force_Base64Url() {

        _output.WriteLine("Starting serialization...");

        var normalClass = new NormalClass();
        var jsonClass = JsonSerializer.Serialize(normalClass, JsonUtils.Base64SerializerOptions);
        Assert.Equal("\"eyJmaWVsZDEiOjEsImZpZWxkMiI6MiwiZmllbGQzIjoiMiIsImZpZWxkNCI6IjQifQ\"",
            jsonClass);
        _output.WriteLine("    Force Base64Url NormalClass ok");

        var normalStruct = new NormalStruct();
        var jsonStruct = JsonSerializer.Serialize(normalStruct, JsonUtils.Base64SerializerOptions);
        Assert.Equal("\"eyJmaWVsZDEiOjEsImZpZWxkMiI6MiwiZmllbGQzIjoiMiIsImZpZWxkNCI6IjQifQ\"",
            jsonStruct);
        _output.WriteLine("    Force Base64Url NormalStruct ok");

        _output.WriteLine("Serialization ok");

        _output.WriteLine("Starting deserialization...");

        var Base64ClassResult = JsonSerializer.Deserialize<NormalClass>(jsonClass, JsonUtils.Base64SerializerOptions);
        Assert.NotNull(Base64ClassResult);
        _output.WriteLine("    Force Base64Url NormalClass ok");

        var Base64StructResult = JsonSerializer.Deserialize<NormalStruct>(jsonStruct, JsonUtils.Base64SerializerOptions);
        // Assert.NotNull(Base64StructResult);
        _output.WriteLine("    Force Base64Url NormalClass ok");

        _output.WriteLine("Deserialization ok");

    }

    [Fact]
    public void Serialize_BuiltIn_Force_Base64Url() {

        long int64Value = 100000L;
        var json = JsonSerializer.Serialize(int64Value, JsonUtils.Base64SerializerOptions);

        var int64Result = JsonSerializer.Deserialize<long>(json, JsonUtils.Base64SerializerOptions);
        Assert.Equal(int64Result, int64Value);

        _output.WriteLine("Force Base64Url long ok");

        double doubleValue = Math.PI;
        json = JsonSerializer.Serialize(doubleValue, JsonUtils.Base64SerializerOptions);

        var doubleResult = JsonSerializer.Deserialize<double>(json, JsonUtils.Base64SerializerOptions);
        Assert.Equal(doubleResult, doubleValue);
        _output.WriteLine("Force Base64Url double ok");

        char charValue = 'a';
        json = JsonSerializer.Serialize(charValue, JsonUtils.Base64SerializerOptions);

        var charResult = JsonSerializer.Deserialize<char>(json, JsonUtils.Base64SerializerOptions);
        Assert.Equal(charResult, charValue);
        _output.WriteLine("Force Base64Url char ok");

        string strValue = "\"RFC 7519\"";
        json = JsonSerializer.Serialize(strValue, JsonUtils.Base64SerializerOptions);

        var strResult = JsonSerializer.Deserialize<string>(json, JsonUtils.Base64SerializerOptions);
        Assert.Equal(strResult, strValue);
        _output.WriteLine("Force Base64Url string ok");

        byte[] bytesValue = Enumerable.Range(0, 10).Select(a => (byte)a).ToArray();
        json = JsonSerializer.Serialize(bytesValue, JsonUtils.Base64SerializerOptions);

        var bytesResult = JsonSerializer.Deserialize<byte[]>(json, JsonUtils.Base64SerializerOptions);
        Assert.Equal(bytesResult, bytesValue);
        _output.WriteLine("Force Base64Url byte[] ok");

        char[] charsValue = Enumerable.Range(65, 26).Select(a => (char)a).ToArray();
        json = JsonSerializer.Serialize(charsValue, JsonUtils.Base64SerializerOptions);

        var charsResult = JsonSerializer.Deserialize<char[]>(json, JsonUtils.Base64SerializerOptions);
        Assert.Equal(charsResult, charsValue);
        _output.WriteLine("Force Base64Url char[] ok");
    }

    [Fact]
    public void Serialize_Jws() {

        using EcKeyInfo key = (EcKeyInfo)AsymmetricKeyInfo.Create(JsonSignAlgorithm.ES256);

        var pem = key.Key.ExportPkcs8PrivateKeyPem();

        var jwh = new JsonWebHeader<EcJsonWebKey> {
            Algorithm = key.Algorithm,
            Url = new("https://google.com"),
            Nonce = "<Nonce>",
            JsonWebKey = new EcJsonWebKey(key.Key)
        };

        var jws = new JsonWebSignature<EcJsonWebKey, Base64PropertyClass> {
            Protected = jwh,
            Payload = new()
        };

        var json = JsonSerializer.Serialize(jws, JsonUtils.SerializerOptions);

        var jwsResult = JsonSerializer.Deserialize<JsonWebSignature<EcJsonWebKey, Base64PropertyClass>>(json, JsonUtils.SerializerOptions);
        Assert.NotNull(jwsResult);


    }

}