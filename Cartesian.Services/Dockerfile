FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Cartesian.Services/Cartesian.Services.csproj", "Cartesian.Services/"]
COPY ["Cartesian.ServiceDefaults/Cartesian.ServiceDefaults.csproj", "Cartesian.ServiceDefaults/"]
COPY ["global.json", "./"]
COPY [".editorconfig", "./"]
RUN dotnet restore "Cartesian.Services/Cartesian.Services.csproj"
COPY Cartesian.Services/ ./Cartesian.Services/
COPY Cartesian.ServiceDefaults/ ./Cartesian.ServiceDefaults/
WORKDIR "/src/Cartesian.Services"
RUN dotnet build "Cartesian.Services.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cartesian.Services.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cartesian.Services.dll"]
