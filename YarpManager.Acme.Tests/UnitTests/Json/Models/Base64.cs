using YarpManager.Acme.Attributes;
using YarpManager.Acme.Jws;

namespace YarpManager.Acme.Tests.UnitTests.Json.Models;

[Base64Url]
internal sealed class Base64Class {

    public long Field1 { get; set; } = 1;

    [Base64Url]
    public double Field2 { get; set; } = 2.0;

    public char Field3 { get; set; } = '2';
    [Base64Url]
    public string Field4 { get; set; } = "4";

    public Base64PropertyClass Base64PropertyClass { get; set; } = new();

    [Base64Url]
    public Base64PropertyStruct Base64PropertyStruct { get; set; } = new();

}

[Base64Url]
internal struct Base64Struct {
    public Base64Struct() {
    }

    public long Field1 { get; set; } = 1;

    [Base64Url]
    public double Field2 { get; set; } = 2.0;

    public char Field3 { get; set; } = '2';
    [Base64Url]
    public string Field4 { get; set; } = "4";

    [Base64Url]
    public Base64PropertyClass Base64PropertyClass { get; set; } = new();

    public Base64PropertyStruct Base64PropertyStruct { get; set; } = new();

}
