using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.AspNetCore.Mvc.ApiExploring;

public class XmlDocumentationProvider : IXmlDocumentationProvider, ISingletonDependency
{
    private readonly ConcurrentDictionary<Assembly, XDocument?> _xmlDocCache = new();

    public string? GetSummary(Type type)
    {
        var memberName = GetMemberNameForType(type);
        return GetDocumentationElement(type.Assembly, memberName, "summary");
    }

    public string? GetRemarks(Type type)
    {
        var memberName = GetMemberNameForType(type);
        return GetDocumentationElement(type.Assembly, memberName, "remarks");
    }

    public string? GetSummary(MethodInfo method)
    {
        var memberName = GetMemberNameForMethod(method);
        return GetDocumentationElement(method.DeclaringType!.Assembly, memberName, "summary");
    }

    public string? GetRemarks(MethodInfo method)
    {
        var memberName = GetMemberNameForMethod(method);
        return GetDocumentationElement(method.DeclaringType!.Assembly, memberName, "remarks");
    }

    public string? GetReturns(MethodInfo method)
    {
        var memberName = GetMemberNameForMethod(method);
        return GetDocumentationElement(method.DeclaringType!.Assembly, memberName, "returns");
    }

    public string? GetParameterSummary(MethodInfo method, string parameterName)
    {
        var memberName = GetMemberNameForMethod(method);
        var doc = LoadXmlDocumentation(method.DeclaringType!.Assembly);
        if (doc == null)
        {
            return null;
        }

        var memberNode = doc.XPathSelectElement($"//member[@name='{memberName}']");
        var paramNode = memberNode?.XPathSelectElement($"param[@name='{parameterName}']");
        return CleanXmlText(paramNode);
    }

    public string? GetSummary(PropertyInfo property)
    {
        var memberName = GetMemberNameForProperty(property);
        return GetDocumentationElement(property.DeclaringType!.Assembly, memberName, "summary");
    }

    private string? GetDocumentationElement(Assembly assembly, string memberName, string elementName)
    {
        var doc = LoadXmlDocumentation(assembly);
        if (doc == null)
        {
            return null;
        }

        var memberNode = doc.XPathSelectElement($"//member[@name='{memberName}']");
        var element = memberNode?.Element(elementName);
        return CleanXmlText(element);
    }

    private XDocument? LoadXmlDocumentation(Assembly assembly)
    {
        return _xmlDocCache.GetOrAdd(assembly, static asm =>
        {
            if (string.IsNullOrEmpty(asm.Location))
            {
                return null;
            }

            var xmlFilePath = Path.ChangeExtension(asm.Location, ".xml");
            if (!File.Exists(xmlFilePath))
            {
                return null;
            }

            try
            {
                return XDocument.Load(xmlFilePath);
            }
            catch
            {
                return null;
            }
        });
    }

    private static string? CleanXmlText(XElement? element)
    {
        if (element == null)
        {
            return null;
        }

        var text = element.Value;
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return Regex.Replace(text.Trim(), @"\s+", " ");
    }

    private static string GetMemberNameForType(Type type)
    {
        return $"T:{GetTypeFullName(type)}";
    }

    private static string GetMemberNameForMethod(MethodInfo method)
    {
        var typeName = GetTypeFullName(method.DeclaringType!);
        var parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            return $"M:{typeName}.{method.Name}";
        }

        var paramTypes = string.Join(",",
            parameters.Select(p => GetParameterTypeName(p.ParameterType)));
        return $"M:{typeName}.{method.Name}({paramTypes})";
    }

    private static string GetMemberNameForProperty(PropertyInfo property)
    {
        var typeName = GetTypeFullName(property.DeclaringType!);
        return $"P:{typeName}.{property.Name}";
    }

    private static string GetTypeFullName(Type type)
    {
        return type.FullName?.Replace('+', '.') ?? type.Name;
    }

    private static string GetParameterTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            var defName = genericDef.FullName!;
            defName = defName[..defName.IndexOf('`')];
            var args = string.Join(",", type.GetGenericArguments().Select(GetParameterTypeName));
            return $"{defName}{{{args}}}";
        }

        if (type.IsArray)
        {
            return GetParameterTypeName(type.GetElementType()!) + "[]";
        }

        if (type.IsByRef)
        {
            return GetParameterTypeName(type.GetElementType()!) + "@";
        }

        return type.FullName ?? type.Name;
    }
}
