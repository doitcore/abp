namespace Volo.Abp.AspNetCore.ClientIpAddress;

public class NullClientIpAddressProvider : IClientIpAddressProvider
{
    public string? ClientIpAddress => null;
}
