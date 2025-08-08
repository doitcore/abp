using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Volo.Abp.AI.Delegates;

namespace Volo.Abp.AI.Extensions;

public static class ChatClientWithTemperatureExtensions 
{
    public static ChatClientBuilder UseTemperature(this ChatClientBuilder builder, float temperature)
    {
        return builder.Use(chatClient => new ChatClientWithTemperature(chatClient, temperature));
    }
}