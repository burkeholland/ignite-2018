FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["InventoryManagerWeb/InventoryManager.csproj", "InventoryManagerWeb/"]
RUN dotnet restore "InventoryManagerWeb/InventoryManager.csproj"
COPY . .
WORKDIR "/src/InventoryManagerWeb"
RUN dotnet build "InventoryManager.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "InventoryManager.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "InventoryManager.dll"]