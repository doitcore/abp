using System;
using System.Collections.Generic;
using TickerQ.Utilities;
using TickerQ.Utilities.Enums;

namespace Volo.Abp.TickerQ;

public class AbpTickerQOptions
{
    public Dictionary<string, (string, TickerTaskPriority, TickerFunctionDelegate)> Functions { get;}

    public Dictionary<string, (string, Type)> RequestTypes { get; }

    public AbpTickerQOptions()
    {
        Functions = new Dictionary<string, (string, TickerTaskPriority, TickerFunctionDelegate)>();
        RequestTypes = new Dictionary<string, (string, Type)>();
    }
}
