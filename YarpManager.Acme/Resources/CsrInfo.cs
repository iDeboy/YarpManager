namespace YarpManager.Acme.Resources;
public readonly struct CsrInfo {
    public string? OrganizationName { get; init; }
    public string? OrganizationUnitName { get; init; }
    public string? CountryOrRegion { get; init; }
    public string? LocalityName { get; init; }
    public string? StateOrProvinceName { get; init; }
}
