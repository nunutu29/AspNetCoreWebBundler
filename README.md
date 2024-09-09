# ASP.NET Core Web Bundler

A NuGet Package for CSS, JavaScript, and HTML bundling and minification at build time and runtime during development.

[![build](https://github.com/nunutu29/AspNetCoreWebBundler/actions/workflows/build.yml/badge.svg)](https://github.com/nunutu29/AspNetCoreWebBundler/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/AspNetCoreWebBundler)](https://www.nuget.org/packages/AspNetCoreWebBundler/)


# Features
- Combine CSS, JavaScript, and HTML files into one or more output files.
- Minify one or multiple files into one or more output files.
- Bundle files at build time using MSBuild Tasks.
- Automatically bundle files during `Debug` mode. Check [here](#development).

# Setup
Add the NuGet package to your project.

Since this package is primarily required at compilation time, set PrivateAssets="All" in the package reference. This ensures that the package does not get included in the output during the publishing process, keeping the bin folder clean.

```xml
<ItemGroup>
    <PackageReference Include="AspNetCoreWebBundler" Version="x.y.z" PrivateAssets="All" />
</ItemGroup>
```

Add the configuration `AspNetCoreWebBundler.json` file in project root directory. To separate paths in segments `/` should be used.  
Example of a configuration file:

```json
[
  // creates 2 javascript files, one bundled and one minified
  {
    "dest": "wwwroot/js/dist/all.js",
    "src": [
      "wwwroot/js/src/site.js",
      "wwwroot/js/src/**/*.js"
    ],
    "minify": {
        "enabled": true // this is by default
    },
    "sourceMap": false
  },
  // creates 2 css files, one bundled and one minified
  {
    "dest": "wwwroot/css/dist/all.css",
    "src": [
      "wwwroot/css/src/site.css",
      "wwwroot/css/src/**/*.css",
    ]
  },
  // creates multiple files (bundled and minified)
  {
    "dest": "wwwroot/js/dist2/",
    "src": [
      "wwwroot/js/src2/site.js",
      "wwwroot/js/src2/**/*.js"
    ]
  },
  // creates multiple files (minified only) preserving the source directory tree 
  {
    "dest": "wwwroot/js/dist2/**/*.min.js",
    "src": [
      "wwwroot/js/src2/**/*.js"
    ]
  }
]
```
## Configuration specs
In the AspNetCoreWebBundler.json configuration file, all paths specified for source (src) and destination (dest) files are relative to the location of the configuration file itself. This means that the paths should be defined based on the directory structure starting from where the AspNetCoreWebBundler.json file resides.

### `dest` (`string`)
The output to generate. Can be a file, a directory or a glob pattern.

### `src` (`string[]`)
An array of files to bundle together. Can contain a file, a directory or a glob pattern.

### `minify` (`object`)
Minification options for the output type.

- `enabled` - Enable or disable the minification process. Default is `true`.
- `gZip` - Compress bundle as gzip. If the minification is enabled, only the minified file will be compressed. Default is `false`.

Other options differ depending on the type of input files (JavaScript, CSS or HTML).
#### JavaScript
- `termSemicolons` (`bool`) - Add a semicolon at the end of the parsed code. Default is `false`.
- `alwaysEscapeNonAscii` (`bool`) - Always escape non-ASCII characters as \uXXXX or to let the output encoding object handle that via the JsEncoderFallback object for the specified output encoding format. Default is `false` (let the Encoding object handle it).
- `preserveImportantComments` (`bool`) - Remove all comments except those marked as important. Default is `true`.
- `evalTreatment` (`string`) - Settings for how to treat eval statements. Default is `ignore`.
  - `ignore` - Ignore all eval statements (default). This assumes that code that is eval'd will not attempt to access any local variables or functions, as those variables and function may be renamed.
  - `makeImmediateSafe` - Assume any code that is eval'd will attempt to access local variables and functions declared in the same scope as the eval statement. This will turn off local variable and function renaming in any scope that contains an eval statement.
  - `makeAllSafe` - Assume that any local variable or function in any accessible scope chain may be referenced by code that is eval'd. This will turn off local variable and function renaming for all scopes that contain an eval statement, and all their parent scopes up the chain to the global scope.

#### CSS
- `adjustRelativePaths` (`bool`) - Adjusts the paths based on the output file position. Default is `true`.
- `termSemicolons` (`bool`) - Add a semicolon at the end of the parsed code. Default is `false`.
- `decodeEscapes` (`bool`) - Unicode escape strings (eg. '\ff0e') should be replaced by it's actual character or not. Default is true. Default is `true`.
- `colorNames` (`string`) - How to treat known color names. Default is `strict`.
  - `strict` - Convert strict names to hex values if shorter; hex values to strict names if shorter. Leave all other color names or hex values as-specified.
  - `hex` - Always use hex values; do not convert any hex values to color names.
  - `major` - Convert known hex values to major-browser color names if shorter; and known major-browser color names to hex if shorter.
  - `noSwap` - Don't swap names for hex or hex for names, whether or not one is shorter.
- `preserveImportantComments` (`bool`) - Remove all comments except those marked as important. Default is `true`.

#### HTML
- `attributesCaseSensitive` (`bool`) - Treat attributes as case sensitive. Default is `false`.
- `tagsCaseSensitive` (`bool`) - Treat tag names as case sensitive. Default is `false`.
- `collapseWhitespaces` (`bool`) - Collapse whitespaces. Default is `true`.
- `removeComments` (`bool`) - Remove all comments (except those marked as important). Default is `true`.
- `removeAttributeQuotes` (`bool`) - Remove the quotes around attributes when possible. Default is `true`.
- `removeOptionalTags` (`bool`) - Remove optional tags (e.g: `</p>` or `</li>`). Default is `false`.
- `decodeEntityCharacters` (`bool`) - Decode entity characters to their shorter character equivalents. Default is `true`.
- `shortBooleanAttribute` (`bool`) -  use the short version of a boolean attribute if value is true. Default is `true`.
- `isFragmentOnly` (`bool`) - The parsing is occuring on an HTML fragment to avoid creating missing tags (like html, body, head). Default is `true`.
- `minifyJs` (`bool`) -  Minify js inside <script> tags. Default is `true`.
- `minifyJsAttributes` (`bool`) - Minify js inside JS event attributes (e.g. onclick, onfocus). Default is `true`.
- `minifyCss` (`bool`) - Minify css inside <style> tags. Default is `true`.
- `minifyCssAttributes` (`bool`) - Minify css inside style attribute. Default is `true`.
- `keepOneSpaceWhenCollapsing` (`bool`) - Keep one space when collapsing multiple adjacent whitespace characters. Default is `false`.
- `indent` (`string`) - The string used for one level of indent. Default is two spaces.

### `sourceMap` (`bool`)
If `true`, will generate a source map for the bundled file. Default is `false`.

### `sourceMapRootPath` (`string`)
Source root URI that will be added to the map object as the sourceRoot property.

# Development
To enable runtime bundling support during development, you need to modify the *ConfigureServices* method.

```csharp
using AspNetCoreWebBundler;
...

public void ConfigureServices(IServiceCollection services)
{
#if DEBUG
     // Add runtime bundling services only during Debug mode
    services.AddRuntimeWebBundler();
#endif
    // Other service configurations...
}
```

`AddRuntimeWebBundler`: This method sets up an `IHostedService` that watches for changes in the source files (CSS, JavaScript and HTML) within the solution directories. 
It specifically monitors projects directories within the solution containing configuration files.

To disable the bundling/minification process, you can include the following property in your .csproj file:
```xml
<PropertyGroup>
  <EnableAspNetCoreWebBundler>false</EnableAspNetCoreWebBundler>
</PropertyGroup>
```

# DesignTimeBuild
By default the Bundler is disabled on DesignTimeBuild. To enable it include the following property in your .csproj file:

```xml
<PropertyGroup>
  <EnableAspNetCoreWebBundlerDesignTimeBuild>true</EnableAspNetCoreWebBundlerDesignTimeBuild>
</PropertyGroup>
```

# TODO
- More tests

# License
[Apache 2.0](LICENSE)
