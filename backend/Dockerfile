# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Copy tessdata for OCR
COPY tessdata ./tessdata

# Expose port (Render will set PORT env variable)
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000}

# Run the app
ENTRYPOINT ["dotnet", "AutoReportGenerator.dll"]
