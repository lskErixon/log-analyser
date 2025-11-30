FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["LogAnalyzerApi/LogAnalyzerApi.csproj", "LogAnalyzerApi/"]
COPY ["LogAnalyzerBusiness/LogAnalyzerBusiness.csproj", "LogAnalyzerBusiness/"]
COPY ["LogAnalyzerData/LogAnalyzerData.csproj", "LogAnalyzerData/"]

RUN dotnet restore "LogAnalyzerApi/LogAnalyzerApi.csproj"

COPY . .

WORKDIR "/src/LogAnalyzerApi"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LogAnalyzerApi.dll"]
