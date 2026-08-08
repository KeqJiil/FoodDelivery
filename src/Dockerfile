FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Ordering/Ordering.csproj", "src/Ordering/"]
COPY ["src/Restaurants/Restaurants.csproj", "src/Restaurants/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
COPY ["src/OrderRequests/OrderRequests.csproj", "src/OrderRequests/"]
COPY ["src/Payments/Payments.csproj", "src/Payments/"]
COPY ["src/Deliveries/Deliveries.csproj", "src/Deliveries/"]
COPY ["src/Saga/Saga.csproj", "src/Saga/"]
RUN dotnet restore "src/Api/Api.csproj"
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
