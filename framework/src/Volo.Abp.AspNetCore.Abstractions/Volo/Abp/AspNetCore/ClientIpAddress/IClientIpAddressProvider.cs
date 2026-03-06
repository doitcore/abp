namespace Volo.Abp.AspNetCore.ClientIpAddress;

public interface IClientIpAddressProvider
{
    string? ClientIpAddress { get; }
}
