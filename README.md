# DockerExample
simple example webapi application using docker

## 1 Create web app solution
1.1 Create empty solution file in ROOT/src directory
```ps
(mkdir src); (cd src); (dotnet new sln -n DockerExample);
```
1.2 Then create webapi project and add it to the solution
```ps
(dotnet new webapi -n WebAPI); (dotnet sln add .\WebAPI\WebAPI.csproj);
```
1.3 Start app and [navigate to url](http://localhost:5000/weatherforecast)
```ps
# move to project dir and run app
(cd .\WebAPI\); (dotnet run)
```

## 2 Create app container
2.1 Create dockerfile for build image in ROOT/docker directory
```ps
New-Item -Path '.\docker\Dockerfile' -ItemType File -Force
```
with content
```
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy sln and csproj and restore as distinct layers
COPY ./*.sln .
COPY ./**/*.csproj ./
RUN dotnet restore

# copy everything else and build app
COPY . .
WORKDIR /app/WebAPI
RUN dotnet publish -c release -o published

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/WebAPI/published ./

EXPOSE 5000
ENTRYPOINT ["dotnet", "WebAPI.dll"]
```
2.2 Then build docker image with tag
```
docker build -f ./docker/Dockerfile ./src -t webapi-example:0.1.0
```
2.3 Start container in interactive mode and [navigate to url](http://localhost:5000/)
```
docker run -it --rm -p 5000:5000 --name webapi0.1.0 webapi-example:0.1.0
```

## 3 Add file logger to app (NLog)

3.1 Add NLog packages to webapi project ([_see more details_](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-3))
```ps
(cd ./src/WebAPI/); (dotnet add package NLog); (dotnet add package NLog.Web.AspNetCore)
```
3.2 Add NLog config file
```ps
New-Item -Path './nlog.config' -ItemType File -Force
```
with content
```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true">

  <!-- enable asp.net core layout renderers -->
  <extensions>
    <add assembly="NLog.Web.AspNetCore"/>
    <add assembly="NLog.Custom.LayoutRenderers" />
  </extensions>

  <!-- path variable -->
  <variable name="log-dir" value="${basedir}${ds}logs"/>
  <variable name="archive-log-dir" value="${basedir}${ds}logs${ds}archive"/>

  <!-- layout variables -->
  <variable name="time-fv" value="${date:format=yyyy-MM-dd HH\:mm\:ss.fff}"/>
  <variable name="level-fv" value="${pad:padding=5:inner=${level:uppercase=true}}"/>
  <variable name="event-fv" value="${event-properties:item=EventId_Id}"/>
  <variable name="logger-fv" value="${logger}"/>
  <variable name="message-fv" value="${message}"/>
  <variable name="error-fv" value="${onexception:inner=${newline}${exception:format=tostring:separator=${newline}}"/>
  <variable name="route-fv" value="url: ${aspnet-request-url} (action: ${aspnet-mvc-action})"/>

  <!-- the targets to write to -->
  <targets>
    <!-- write logs to file  -->
    <target xsi:type="File"
            name="allfile"
            encoding="utf-8"
            fileName="${log-dir}${ds}all-logs.log"
            layout="${time-fv} | ${event-fv} | ${level-fv} | ${logger-fv} | ${message-fv} ${error-fv}"
            maxArchiveFiles="14"
            archiveFileName="${archive-log-dir}${ds}all-logs.{#}.log"
            archiveNumbering="Date"
            archiveEvery="Day"
            archiveDateFormat="yyyyMMdd"/>

    <!-- another file log, only own logs. Uses some ASP.NET core renderers -->
    <target xsi:type="File"
            name="ownFile-web"
            encoding="utf-8"
            fileName="${log-dir}${ds}nlog-own.log"
            layout="${time-fv} | ${route-fv}${newline}
                    ${event-fv} | ${level-fv} | ${logger-fv} | ${message-fv} ${error-fv}"
            maxArchiveFiles="14"
            archiveFileName="${archive-log-dir}${ds}all-logs.{#}.log"
            archiveNumbering="Date"
            archiveEvery="Day"
            archiveDateFormat="yyyyMMdd"/>
  </targets>

  <!-- rules to map from logger name to target -->
  <rules>
    <!--All logs, including from Microsoft-->
    <logger name="*" minlevel="Trace" writeTo="allfile" />
    <!--Skip non-critical Microsoft logs and so log only own logs-->
    <logger name="Microsoft.*" maxlevel="Info" final="true" />
    <!-- BlackHole without writeTo -->
    <logger name="*" minlevel="Trace" writeTo="ownFile-web" />
  </rules>
</nlog>
```
Log files store in container by /app/logs.

3.3 For using correct direstry separator ("\\" or "/") depended on OS create custom layout render.
```
(dotnet new classlib -n NLog.Custom.LayoutRenderers); (dotnet sln add .\NLog.Custom.LayoutRenderers\NLog.Custom.LayoutRenderers.csproj); (cd NLog.Custom.LayoutRenderers); (dotnet add package NLog);
```
3.4 Add class to project
```csharp
[LayoutRenderer("ds")]
public class DirectorySeparatorLayoutRenderer : LayoutRenderer
{
    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        builder.Append(Path.DirectorySeparatorChar);
    }
}
```
This layout render help us to correct setup logs path for any OS platforms _(When run app on window or in linux docker container)_.

3.5 Then add reference on new project to webapi project and add it extension to nlog config.
```ps
# move to src dir
(cd ..); (dotnet add .\WebAPI\WebAPI.csproj reference .\NLog.Custom.LayoutRenderers\NLog.Custom.LayoutRenderers.csproj);
```
nlog.config:
```xml
...
<extensions>
  ...
  <add assembly="NLog.Custom.LayoutRenderers" />
</extensions>
...
```
3.6 Setup logging
Program.cs:
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        try
        {
            logger.Info("Starting up application ...");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while starting up application.");
            throw;
        }
        finally
        {
            // flush logs to targets before exit
            NLog.LogManager.Shutdown();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseKestrel()
                    .ConfigureAppConfiguration((context, config) => {
                        var env = context.HostingEnvironment;
                        config.SetBasePath(env.ContentRootPath);

                        config
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

                        config.AddEnvironmentVariables();

                        // secrets
                        if (env.IsDevelopment())
                        {
                            config.AddUserSecrets<Startup>(optional: true);
                        }

                        if (args != null)
                            config.AddCommandLine(args);
                    })
                    .ConfigureLogging((hostingContext, logging) => {
                        logging.ClearProviders();

                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        logging.AddDebug();
                    })
                    .UseNLog()
                    .UseStartup<Startup>();
            });
}
```
3.7 Build new docker image and attach volume to store container logs on host machine.
```
docker run -it --rm -v E:\docker\logs:/app/logs -p 5000:5000 --name webapi0.1.0 webapi-example:0.1.0
```