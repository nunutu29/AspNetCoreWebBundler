# ASP.NET Core Web Bundler

A NuGet Package for CSS, JavaScript, and HTML Bundling and Minification

## Features
- Combine CSS, JavaScript, and HTML files into one or more output files.
- Minify one or multiple files into one or more output files
- Bundle files at build time using MSBuild Tasks.
- Automatically bundle files during `Debug` mode

## Setup
Add the NuGet package.

Since this package is primarily required at compilation time, set PrivateAssets="All" in the package reference. This ensures that the package does not get included in the output during the publishing process, keeping the bin folder clean.

```xml
<ItemGroup>
    <PackageReference Include="AspNetCoreWebBundler" Version="x.y.z" PrivateAssets="All" />
</ItemGroup>
```

To enable runtime bundling support during debug mode, you need to modify the *ConfigureServices* method. 
Here's an example of how you can set it up:

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

## License
[Apache 2.0](LICENSE)
