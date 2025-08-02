# Use ASP.NET 8 runtime only (no SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copy the published backend output
COPY ./publish/ .

# Copy the certs (assumed to be generated with mkcert)
COPY ./certs/cert.pem /https/cert.pem
COPY ./certs/key.pem /https/key.pem

# Configure HTTPS
ENV ASPNETCORE_URLS=https://+:443
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/cert.pem
ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/https/key.pem

EXPOSE 443

ENTRYPOINT ["dotnet", "HttpLogger.Server.dll"]
