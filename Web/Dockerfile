# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia o arquivo de solução e projetos
COPY ["OrderRegistration_BackEnd.sln", "."]
COPY ["Web/Web.csproj", "Web/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infra/Infra.csproj", "Infra/"]
COPY ["Common/Common.csproj", "Common/"]

# 2. Restaura todos os projetos
RUN dotnet restore "OrderRegistration_BackEnd.sln"

# 3. Copia todo o código fonte
COPY . .

# 4. Build da solução completa
RUN dotnet build "OrderRegistration_BackEnd.sln" -c Release --no-restore -o /app/build

# 5. Publicação otimizada
FROM build AS publish
WORKDIR "/src/Web"
RUN dotnet publish "Web.csproj" -c Release -o /app/publish \
    /p:UseAppHost=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Web.dll"]
