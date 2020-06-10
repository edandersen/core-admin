# Core Admin Panel for ASP.NET Core

![.NET Core 3.1](https://github.com/edandersen/auto-admin/workflows/.NET%20Core%203.1/badge.svg)

Fully automatic admin site generator for ASP.NET Core. Add one line of code, get loads of stuff.

## How to use

Add via nuget:

```
dotnet add package CoreAdmin -v 0.5.1-beta
```

Add this line at the bottom of ConfigureServices() in Startup.cs:

```
services.AddCoreAdmin();
```

Run your app with with /coreadmin on the end of the URL, for example https://localhost:5001/coreadmin and you'll get a little something like this -

![Screenshot of core admin](/docs/screenshot-1.PNG "Core Admin")

The above screenshot is of the [Contoso University sample](https://github.com/dotnet/AspNetCore.Docs/tree/master/aspnetcore/data/ef-rp/intro/samples/cu30) with Core Admin added to it.

Core Admin scans your app for Entity Framework DB Contexts and makes a nice set of CRUD screens for them.

### License

LGPL licensed. Depends on the snazzy [NonFactors.Grid.Mvc6](https://github.com/NonFactors/AspNetCore.Grid) and Bootstrap, both of which are MIT licensed.

### Authors

Ed Andersen ([@edandersen](https://twitter.com/edandersen))
