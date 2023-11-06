FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
ENV DOTNET_RUNNING_IN_CONTAINER=true
RUN apk add --no-cache icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY *.sln .
COPY /CertificateValidatorSample/CertificateValidatorSample.csproj ./CertificateValidatorSample/

RUN dotnet restore

COPY . .
RUN dotnet build --configuration Release --no-restore

FROM build AS publish
WORKDIR /app/CertificateValidatorSample
RUN dotnet publish --configuration Release -o /publish --no-build  

FROM base AS release
COPY --from=publish /publish ./
ENTRYPOINT ["dotnet", "ClausAppel.CertificateValidatorSample.dll"]