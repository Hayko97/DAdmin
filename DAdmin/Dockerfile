FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DAdmin/DAdmin.csproj", "DAdmin/"]
RUN dotnet restore "DAdmin/DAdmin.csproj"
COPY . .
WORKDIR "/src/DAdmin"
RUN dotnet build "DAdmin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DAdmin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DAdmin.dll"]
