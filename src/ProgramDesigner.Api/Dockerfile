FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

WORKDIR /src/src/ProgramDesigner.Api

RUN dotnet restore ProgramDesigner.Api.csproj
RUN dotnet publish ProgramDesigner.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ProgramDesigner.Api.dll"]