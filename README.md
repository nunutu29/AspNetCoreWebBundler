# ASP.NET Core Web Bundler

A NuGet Package for CSS, JavaScript, and HTML Bundling and Minification

# Features
- Combine CSS, JavaScript, and HTML files into one or more output files.
- Minify one or multiple files into one or more output files
- Bundle files at build time using MSBuild Tasks.
- Automatically bundle files during `Debug` mode

# Setup
Add the NuGet package.

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
## Configuration
In the AspNetCoreWebBundler.json configuration file, all paths specified for source (src) and destination (dest) files are relative to the location of the configuration file itself. This means that the paths should be defined based on the directory structure starting from where the AspNetCoreWebBundler.json file resides.

### `dest`
The output to generate. Can be a file, a directory or a glob pattern.

### `src`
An array of files to bundle together. Can contain a file, a directory or a glob pattern.

### `minify`
Minification options for the output type.

- `enabled` - Enable or disable the minification process. Defaults to `true`.
- `gZip` - Compress bundle as gzip. If the minification is enabled, only the minified file will be compressed. Defaults to `false`.

Other options differ depending on the type of input files (JavaScript, CSS or HTML).
#### JavaScript
- `termSemicolons` - Add a semicolon at the end of the parsed code. Defaults to `false`.
- `alwaysEscapeNonAscii` - Always escape non-ASCII characters as \uXXXX or to let the output encoding object handle that via the JsEncoderFallback object for the specified output encoding format. Default is `false` (let the Encoding object handle it).
- `preserveImportantComments` - Remove all comments except those marked as important. Defaults to `true`.
- `evalTreatment` - Settings for how to treat eval statements. Defaults to `ignore`.
  - `ignore` - Ignore all eval statements (default). This assumes that code that is eval'd will not attempt to access any local variables or functions, as those variables and function may be renamed.
  - `makeImmediateSafe` - Assume any code that is eval'd will attempt to access local variables and functions declared in the same scope as the eval statement. This will turn off local variable and function renaming in any scope that contains an eval statement.
  - `makeAllSafe` - Assume that any local variable or function in any accessible scope chain may be referenced by code that is eval'd. This will turn off local variable and function renaming for all scopes that contain an eval statement, and all their parent scopes up the chain to the global scope.

#### CSS
- `adjustRelativePaths` - Adjusts the paths based on the output file position. Defaults to `true`.
- `termSemicolons` - Add a semicolon at the end of the parsed code. Defaults to `false`.
- `decodeEscapes` - Unicode escape strings (eg. '\ff0e') should be replaced by it's actual character or not. Default is true. Defaults to `true`.
- `colorNames` - How to treat known color names. Defaults to `strict`.
  - `strict` - Convert strict names to hex values if shorter; hex values to strict names if shorter. Leave all other color names or hex values as-specified.
  - `hex` - Always use hex values; do not convert any hex values to color names.
  - `major` - Convert known hex values to major-browser color names if shorter; and known major-browser color names to hex if shorter.
  - `noSwap` - Don't swap names for hex or hex for names, whether or not one is shorter.
- `preserveImportantComments` - Remove all comments except those marked as important. Defaults to `true`.

#### HTML
TODO

### `sourceMap`
If true, will generate a source map for the bundled file.

### `sourceMapRootPath`
Source root URI that will be added to the map object as the sourceRoot property

# Development runtime
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

# License
[Apache 2.0](LICENSE)
