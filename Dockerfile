FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./src/Onyx.ProductManagement.Api/Onyx.ProductManagement.Api.csproj ./Onyx.ProductManagement.Api/
COPY ./src/Onyx.ProductManagement.Data/Onyx.ProductManagement.Data.csproj ./Onyx.ProductManagement.Data/

RUN dotnet restore "./Onyx.ProductManagement.Api/Onyx.ProductManagement.Api.csproj"

COPY ./src/Onyx.ProductManagement.Api/ ./Onyx.ProductManagement.Api/
COPY ./src/Onyx.ProductManagement.Data/ ./Onyx.ProductManagement.Data/

WORKDIR /src/Onyx.ProductManagement.Api
RUN dotnet build "./Onyx.ProductManagement.Api.csproj" -c Release -o /app/build/

FROM build AS publish
WORKDIR /src/Onyx.ProductManagement.Api
RUN dotnet publish "./Onyx.ProductManagement.Api.csproj" -c Release -o /app/publish/ /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish/ ./

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "Onyx.ProductManagement.Api.dll"]