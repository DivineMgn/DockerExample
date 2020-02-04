# DockerExample
simple example webapi application using docker

## 1 Create web app
1.1 Create dir "src" in root directory and create solution file with dotnet cli
```ps
mkdir src; cd src; dotnet new sln -n DockerExample
```
1.2 Then create web api project
```ps
dotnet new webapi -n WebAPI
```
1.3 Add project to solution
```ps
dotnet sln add .\WebAPI\WebAPI.csproj
```
1.4 Start app and [open url](http://localhost:5000/weatherforecast)
```ps
# move to project dir and run app
cd .\WebAPI\; dotnet run
```
