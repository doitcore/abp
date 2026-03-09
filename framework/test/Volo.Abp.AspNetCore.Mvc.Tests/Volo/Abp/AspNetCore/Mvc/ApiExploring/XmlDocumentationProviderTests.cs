#nullable enable
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Shouldly;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.AspNetCore.Mvc.ApiExploring;

public class XmlDocumentationProviderTests
{
    // A stub type so we can construct member-name keys that the provider can look up.
    private class StubType { }

    private static XmlDocumentationProvider CreateProvider(string xmlDocBody)
    {
        var xml = $@"<?xml version=""1.0""?>
<doc>
  <members>
    {xmlDocBody}
  </members>
</doc>";
        return new FakeXmlDocumentationProvider(xml);
    }

    private static string StubTypeMemberName(string elementName, string xmlContent)
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        return $@"<member name=""{elementName}:{typeName}"">
      {xmlContent}
    </member>";
    }

    // Tests for CleanXmlText via GetSummaryAsync(Type)

    [Fact]
    public async Task GetSummary_Returns_PlainText()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>A simple summary.</summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBe("A simple summary.");
    }

    [Fact]
    public async Task GetSummary_Expands_SeeCref_To_ShortTypeName()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>Returns a <see cref=""T:System.String"" /> value.</summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBe("Returns a String value.");
    }

    [Fact]
    public async Task GetSummary_Expands_SeeCref_NestedType()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>See <see cref=""T:System.Collections.Generic.List`1"" /> for details.</summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBe("See List`1 for details.");
    }

    [Fact]
    public async Task GetSummary_Expands_SeeLangword()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>Returns <see langword=""null"" /> when not found.</summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBe("Returns null when not found.");
    }

    [Fact]
    public async Task GetSummary_Strips_CodeTag()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>Use <c>DoSomething()</c> to start.</summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBe("Use DoSomething() to start.");
    }

    [Fact]
    public async Task GetSummary_Strips_ParaTag()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary><para>First paragraph.</para></summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBe("First paragraph.");
    }

    [Fact]
    public async Task GetSummary_Collapses_Whitespace()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>
              Multiple
              spaces    here.
            </summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBe("Multiple spaces here.");
    }

    [Fact]
    public async Task GetSummary_Returns_Null_When_Member_Not_Found()
    {
        var provider = CreateProvider(string.Empty);

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetSummary_Returns_Null_When_Summary_Is_Empty()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>   </summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetSummary_Expands_Paramref()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>Use the <paramref name=""input"" /> parameter.</summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBe("Use the input parameter.");
    }

    [Fact]
    public async Task GetSummary_Expands_Typeparamref()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>Returns <typeparamref name=""T"" /> instance.</summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBe("Returns T instance.");
    }

    [Fact]
    public async Task GetSummary_Expands_Mixed_Tags()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>Returns <see cref=""T:System.String"" /> or <see langword=""null"" /> if <paramref name=""key"" /> not found.</summary></member>");

        var result = await provider.GetSummaryAsync(typeof(StubType));

        result.ShouldBe("Returns String or null if key not found.");
    }

    [Fact]
    public async Task GetRemarks_Returns_Null_When_No_Remarks_Element()
    {
        var typeName = typeof(StubType).FullName!.Replace('+', '.');
        var provider = CreateProvider(
            $@"<member name=""T:{typeName}""><summary>Only summary.</summary></member>");

        var result = await provider.GetRemarksAsync(typeof(StubType));

        result.ShouldBeNull();
    }

    /// <summary>
    /// A fake provider that loads XML from an in-memory string instead of the file system.
    /// </summary>
    [DisableConventionalRegistration]
    private sealed class FakeXmlDocumentationProvider : XmlDocumentationProvider
    {
        private readonly XDocument _document;

        public FakeXmlDocumentationProvider(string xml)
        {
            _document = XDocument.Parse(xml);
        }

        protected override Task<XDocument?> LoadXmlDocumentationFromDiskAsync(Assembly assembly)
        {
            return Task.FromResult<XDocument?>(_document);
        }
    }
}
