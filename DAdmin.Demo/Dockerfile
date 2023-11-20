FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DAdmin.Demo/DAdmin.Demo.csproj", "DAdmin.Demo/"]
RUN dotnet restore "DAdmin.Demo/DAdmin.Demo.csproj"
COPY . .
WORKDIR "/src/DAdmin.Demo"
RUN dotnet build "DAdmin.Demo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DAdmin.Demo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DAdmin.Demo.dll"]
