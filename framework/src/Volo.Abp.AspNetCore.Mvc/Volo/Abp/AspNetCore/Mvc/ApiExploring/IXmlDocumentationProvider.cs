using System;
using System.Reflection;

namespace Volo.Abp.AspNetCore.Mvc.ApiExploring;

public interface IXmlDocumentationProvider
{
    string? GetSummary(Type type);

    string? GetRemarks(Type type);

    string? GetSummary(MethodInfo method);

    string? GetRemarks(MethodInfo method);

    string? GetReturns(MethodInfo method);

    string? GetParameterSummary(MethodInfo method, string parameterName);

    string? GetSummary(PropertyInfo property);
}
