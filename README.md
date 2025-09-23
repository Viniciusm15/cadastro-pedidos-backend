# 🛠️ Back-end - API E-commerce & Painel Administrativo

Este é o back-end do sistema de **Gestão de E-commerce e Painel Administrativo**.  
A API foi desenvolvida em **.NET com SQL Server** e fornece os recursos necessários para o gerenciamento de produtos, categorias, clientes e pedidos.

---

## 🚀 Funcionalidades

- **Gerenciamento de Produtos**
  - CRUD completo de produtos
  - Upload de imagens

- **Gerenciamento de Categorias**
  - CRUD completo
  - Listagem com contagem de produtos

- **Gerenciamento de Clientes**
  - CRUD completo
  - Histórico de pedidos

- **Gerenciamento de Pedidos**
  - Listagem com filtros (status, cliente, data)
  - Detalhes de pedido (itens, cliente, status)
  - Atualização de status
  - Relatórios de vendas

---

## 🗂️ Estrutura do Projeto
- **Application** → casos de uso, orquestração de regras e DTOs  
- **Common** → utilitários e helpers compartilhados  
- **docker-compose** → arquivos de configuração para containers
- **Domain** → entidades, regras de negócio centrais e contratos  
- **Infra** → repositórios, configuração de banco de dados e integrações externas  
- **Tests** → testes unitários e de integração  
- **Web** → camada de apresentação (controllers, endpoints e configuração da API)  

---

## 🛠️ Tecnologias
- **Linguagem:** C# / .NET 8  
- **Banco de Dados:** SQL Server  
- **ORM:** Entity Framework Core  
- **Testes:** xUnit + FluentAssertions + Testcontainers  

---

## 📋 Considerações Adicionais
- **Soft Delete** implementado com `@Where`  
- **CORS** restrito ao domínio local  
- **Migrações automáticas** no container de testes  

---

## 🐳 Docker & Docker Compose

O projeto está containerizado com Docker para facilitar o desenvolvimento e o deploy.

### ▶️ Como Rodar com Docker Compose

```bash
# Clone o repositório
git clone https://github.com/Viniciusm15/cadastro-pedidos-backend.git

# Execute o docker compose na raíz do projeto
docker-compose up -d

# Por fim defina na IDE o projeto padrão para ser o docker-compose
```

### 🔧 Configuração do Docker

Dockerfile:

```bash
# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Web/Web.csproj", "Web/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infra/Infra.csproj", "Infra/"]
RUN dotnet restore "./Web/Web.csproj"
COPY . .
WORKDIR "/src/Web"
RUN dotnet build "./Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.dll"]
```

### 📝 Arquivo docker-compose.yml

```bash
services:
  web:
    image: ${DOCKER_REGISTRY-}web
    container_name: order-registration-application
    build:
      context: .
      dockerfile: Web/Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};TrustServerCertificate=True;Connect Timeout=60;
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
    ports:
      - "5000:5000"
      - "5001:5001"
    depends_on:
      - database

  database:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: order-registration-sqlserver
    environment:
      SA_PASSWORD: "${SQL_SA_PASSWORD}"
      ACCEPT_EULA: "${ACCEPT_EULA}"
      MSSQL_PID: "${MSSQL_PID}"
    ports:
      - "1433:1433"
    volumes:
      - mssql_data:/var/opt/mssql

volumes:
  mssql_data:
```
