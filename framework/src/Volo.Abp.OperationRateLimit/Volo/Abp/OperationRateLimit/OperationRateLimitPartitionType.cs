namespace Volo.Abp.OperationRateLimit;

public enum OperationRateLimitPartitionType
{
    Parameter,
    CurrentUser,
    CurrentTenant,
    ClientIp,
    Email,
    PhoneNumber,
    Custom
}
