using YarpManager.Acme.Attributes;
using YarpManager.Acme.Jws;

namespace YarpManager.Acme.Tests.UnitTests.Json.Models;
internal sealed class Base64PropertyClass {

    public long Field1 { get; set; } = 1;

    [Base64Url]
    public double Field2 { get; set; } = 2.0;

    public char Field3 { get; set; } = '2';
    [Base64Url]
    public string Field4 { get; set; } = "4";

    public NormalClass NormalClass { get; set; } = new();

    [Base64Url]
    public NormalStruct NormalStruct { get; set; } = new();

}

internal struct Base64PropertyStruct {
    public Base64PropertyStruct() {
    }

    public long Field1 { get; set; } = 1;

    [Base64Url]
    public double Field2 { get; set; } = 2.0;

    public char Field3 { get; set; } = '2';
    [Base64Url]
    public string Field4 { get; set; } = "4";

    [Base64Url]
    public NormalClass NormalClass { get; set; } = new();

    public NormalStruct NormalStruct { get; set; } = new();

}
