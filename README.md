# Auto Admin for ASP.NET Core

![.NET Core 3.1](https://github.com/edandersen/auto-admin/workflows/.NET%20Core%203.1/badge.svg)

Fully automatic admin site generator for ASP.NET Core. Add one line of code, get loads of stuff.

## How to use

Add via nuget:

```
dotnet add package DotNetEd.AutoAdmin -v 0.2.1-beta
```

Add this line at the bottom of ConfigureServices() in Startup.cs:

```
services.AddAutoAdmin();
```

Run your app with with /autoadmin on the end of the URL, for example https://localhost:5001/autoadmin and you'll get a little something like this -

![Screenshot of auto admin](/docs/screenshot-1.png "Auto Admin")

The above screenshot is of the [Contoso University sample](https://github.com/dotnet/AspNetCore.Docs/tree/master/aspnetcore/data/ef-rp/intro/samples/cu30) with Auto Admin added to it.

### License

MIT licensed. Depends on the snazzy [NonFactors.Grid.Mvc6](https://github.com/NonFactors/AspNetCore.Grid) which is also MIT licensed.

### Authors

Ed Andersen ([@edandersen](https://twitter.com/edandersen))