FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /App

# Copy and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Build and publish
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App
COPY --from=build-env /App/out .

# .NET 10 defaults to 8080 internally
EXPOSE 8080
ENTRYPOINT ["dotnet", "OTS_INV.dll"]