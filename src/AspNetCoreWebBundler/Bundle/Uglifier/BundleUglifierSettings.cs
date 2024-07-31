using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUglify.Css;
using NUglify.Html;
using NUglify.JavaScript;

namespace AspNetCoreWebBundler;

[JsonConverter(typeof(BundleUglifierSettingsJsonConverter))]
internal class BundleUglifierSettings() : Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
{
    public T GetValue<T>(string key, T defaultValue)
    {
        if (TryGetValue(key, out var value))
        {
            if (value is T tValue)
            {
                return tValue;
            }

            // enums are also parsed as strings
            if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), value.ToString(), true);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        return defaultValue ?? default;
    }

    public HtmlSettings HtmlSettings()
    {
        var settings = new HtmlSettings
        {
            AttributesCaseSensitive = GetValue("attributesCaseSensitive", false),
            TagsCaseSensitive = GetValue("tagsCaseSensitive", true),
            CollapseWhitespaces = GetValue("collapseWhitespaces", true),
            RemoveComments = GetValue("removeComments", true),
            RemoveAttributeQuotes = GetValue("removeAttributeQuotes", true),
            RemoveOptionalTags = GetValue("removeOptionalTags", false),
            DecodeEntityCharacters = GetValue("decodeEntityCharacters", true),
            ShortBooleanAttribute = GetValue("shortBooleanAttribute", true),
            IsFragmentOnly = GetValue("isFragmentOnly", true),
            MinifyJs = GetValue("minifyJs", true),
            MinifyJsAttributes = GetValue("minifyJsAttributes", true),
            MinifyCss = GetValue("minifyCss", true),
            MinifyCssAttributes = GetValue("minifyCssAttributes", false),
            KeepOneSpaceWhenCollapsing = GetValue("keepOneSpaceWhenCollapsing", false),
            Indent = GetValue("indent", "  ")
        };

        return settings;
    }

    public CssSettings CssSettings()
    {
        var settings = new CssSettings
        {
            TermSemicolons = GetValue("termSemicolons", false),
            DecodeEscapes = GetValue("decodeEscapes", true),
            ColorNames = GetValue("colorNames", CssColor.Strict),
            CommentMode = GetValue("preserveImportantComments", true) ? CssComment.Important : CssComment.None
        };

        return settings;
    }

    public CodeSettings JavaScriptSettings()
    {
        var settings = new CodeSettings
        {
            TermSemicolons = GetValue("termSemicolons", false),
            AlwaysEscapeNonAscii = GetValue("alwaysEscapeNonAscii", false),
            PreserveImportantComments = GetValue("preserveImportantComments", true),
            EvalTreatment = GetValue("evalTreatment", EvalTreatment.Ignore)
        };

        return settings;
    }
}