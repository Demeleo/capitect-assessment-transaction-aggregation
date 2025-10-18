FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and all project files
COPY CapitecTransactionAggregatorAssessment.sln ./
COPY TransactionAggregator.Api/TransactionAggregator.Api.csproj TransactionAggregator.Api/
COPY TransactionAggregator.Application/TransactionAggregator.Application.csproj TransactionAggregator.Application/
COPY TransactionAggregator.Infrastructure/TransactionAggregator.Infrastructure.csproj TransactionAggregator.Infrastructure/

# Restore dependencies
RUN dotnet restore CapitecTransactionAggregatorAssessment.sln

# Copy full source
COPY . .  
WORKDIR /app/TransactionAggregator.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TransactionAggregator.Api.dll"]