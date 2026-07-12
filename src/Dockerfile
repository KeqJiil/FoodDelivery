FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["FoodDelivery.slnx", "."]
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Ordering/Ordering.csproj", "src/Ordering/"]
COPY ["src/Restaurants/Restaurants.csproj", "src/Restaurants/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
RUN dotnet restore "FoodDelivery.slnx"
COPY . .
WORKDIR /src/src/Api
RUN dotnet build "Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8000
ENTRYPOINT ["dotnet", "Api.dll"]
