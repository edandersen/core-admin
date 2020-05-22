# Auto Admin for ASP.NET Core

![.NET Core 3.1](https://github.com/edandersen/auto-admin/workflows/.NET%20Core%203.1/badge.svg)

Fully automatic admin site generator for ASP.NET Core. Add one line of code, get loads of stuff.

## How to use

Add via nuget:

```
dotnet add package DotNetEd.AutoAdmin
```

Add this line at the bottom of ConfigureServices() in Startup.cs:

```
services.AddAutoAdmin();
```

Run your app with with /admin on the end of the URL, for example https://localhost:5001/admin and enjoy!


