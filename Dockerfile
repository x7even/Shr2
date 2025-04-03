FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Shr2/Shr2.csproj", "Shr2/"]
COPY ["Shr2.Interfaces/Shr2.Interfaces.csproj", "Shr2.Interfaces/"]
RUN dotnet restore "Shr2/Shr2.csproj"
COPY . .
WORKDIR "/src/Shr2"
RUN dotnet build "Shr2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shr2.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shr2.dll"]
