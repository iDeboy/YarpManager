using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Utils;

// var acme = new AcmeContext(WellKnownServers.LetsEncryptStagingV2);
// var account = await acme.NewAccount("admin@example.com", true);
// var pemKey = acme.AccountKey.ToPem();

//var h = new Header {
//    Alg = JsonWebAlgorithm.HS256,
//    Typ = "JWT"
//};

//var jws = new Jws {

//    Protected = h,

//    Payload = new Payload {
//        Sub = "1234567890",
//        Name = "John Doe",
//        Iat = 1516239022
//    },

//    Signature = "he0ErCNloe4J7Id0Ry2SEDg09lKkZkfsRiGsdX_vgEg"
//};

//var json =
//    """
//    {
//        "protected":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
//        "payload":"eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ",
//        "signature": "he0ErCNloe4J7Id0Ry2SEDg09lKkZkfsRiGsdX_vgEg"
//    }
//    """;

// var j = JsonSerializer.Serialize(h, JsonUtils.SerializerOptions);
// var d = JsonSerializer.Deserialize<Header>(j, JsonUtils.SerializerOptions);
// var d = JsonSerializer.Deserialize<JsonWebSignature<Payload>>(json, JsonUtils.SerializerOptions);
//_ = j;

using var rsa = RSA.Create();

var jwh = new JsonWebHeader<RsaJsonWebKey> {
    Algorithm = JsonSignAlgorithm.RS256,
    Nonce = "asdsad",
    Url = new("https://google.com"),
    JsonWebKey = new RsaJsonWebKey(rsa)
};

var payload = new Payload {
    Sub = "1234567890",
    Name = "John Doe",
    Iat = 1516239022
};

var jwt = new JsonWebSignature<RsaJsonWebKey, Payload> {
    Protected = jwh,
    Payload = payload
};

var s = JsonSerializer.Serialize(jwt, JsonUtils.SerializerOptions);

//var encodedHeader = JsonSerializer.Serialize(jwh, JsonUtils.SerializerOptions);
//var encodedPayload = JsonSerializer.Serialize(payload, JsonUtils.Base64SerializerOptions);

//var key = new HMACKey(jwh.Algorithm, [1]);
//var credentials = new CryptoCredencials<HMACKey>(key, jwh.Algorithm);

//var signature = JwsSigner.Sign(ref credentials, encodedHeader, encodedPayload);

[Base64Url]
class Header {

    [JsonPropertyName("alg")]
    [JsonConverter(typeof(JsonStringEnumConverter<JsonSignAlgorithm>))]
    public JsonSignAlgorithm Alg { get; set; }

    [JsonPropertyName("typ")]
    public string Typ { get; set; }

}

// [JsonBase64Url]
class Jws {

    [JsonPropertyName("protected")]
    public Header Protected { get; set; }

    [JsonPropertyName("payload")]
    [Base64Url]
    public Payload Payload { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }

    [JsonPropertyName("Test")]
    public Test Test { get; set; }

}

class Test {

    [JsonPropertyName("value1")]
    public int Value1 { get; set; }

    [JsonPropertyName("value2")]
    public int Value2 { get; set; }

    [JsonPropertyName("value3")]
    public string Value3 { get; set; }

}

class Payload {

    [JsonPropertyName("sub")]
    public string Sub { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("iat")]
    public long Iat { get; set; }


}

//var r = Base64Url.Encode("Esto es una Ѓ prueba");
//var r2 = Base64Url.Encode("Esto es una Ѓ prueba"u8);

//var r3 = Base64Url.Decode(r);
//var r4 = Base64Url.Decode(r2);

//Span<char> output = new char[512];
//byte[] data = AcmeUtils.Encoding.GetBytes("Esto es una prueba");

//int i = Base64Url.Encode(data, output);
//output = output[..i];

//Span<byte> bytes = new byte[1024];

//var j = Base64Url.Decode(output, bytes);
//bytes = bytes[..j];

//var str = AcmeUtils.Encoding.GetString(bytes);