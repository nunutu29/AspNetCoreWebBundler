# ASP.NET Core Web Bundler

ASP.NET Core NuGet for bundling and minification of CSS, JavaScript and HTML files.

## Features
- Bundles CSS, JavaScript or HTML files into a single/multiple output file/s.
- Runtime bundle during `Debug` mode.

## Setup
Add the NuGet package.
This package is primarly needed at compilation time, so one should set `PrivateAssets="All"` to avoid populating the bin folder with this package during publish.

```xml
<ItemGroup>
    <PackageReference Include="AspNetCoreWebBundler" Version="<version number>" PrivateAssets="All" />
</ItemGroup>
```

If you want to add the runtime support during debug, modify the *ConfigureServices* method:

```csharp
using AspNetCoreWebBundler;
...

public void ConfigureServices(IServiceCollection services)
{
#if DEBUG
    services.AddRuntimeWebBundler();
#endif
    // other code 
}
```

## License
[Apache 2.0](LICENSE)
