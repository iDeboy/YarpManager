namespace YarpManager.Acme.Tests.UnitTests.Json.Models;
internal sealed class NormalClass {

    public long Field1 { get; set; } = 1;
    public double Field2 { get; set; } = 2.0;
    public char Field3 { get; set; } = '2';
    public string Field4 { get; set; } = "4";

}

internal struct NormalStruct {
    public NormalStruct() {
    }

    public long Field1 { get; set; } = 1;
    public double Field2 { get; set; } = 2.0;
    public char Field3 { get; set; } = '2';
    public string Field4 { get; set; } = "4";

}
