using System;

namespace Volo.Abp.Http.ProxyScripting;

public interface IProxyScriptManagerCache
{
    string GetOrAdd(string key, Func<string> factory);

    bool TryGet(string key, out string? value);

    void Set(string key, string value);
}
