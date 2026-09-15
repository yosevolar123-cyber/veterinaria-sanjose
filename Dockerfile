FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["vet San Jose/vet San Jose.csproj", "vet San Jose/"]
COPY ["VetSanJose.Client/VetSanJose.Client.csproj", "VetSanJose.Client/"]
COPY ["VetSanJose.Application/VetSanJose.Application.csproj", "VetSanJose.Application/"]
COPY ["VetSanJose.Domain/VetSanJose.Domain.csproj", "VetSanJose.Domain/"]
COPY ["VetSanJose.Infrastructure/VetSanJose.Infrastructure.csproj", "VetSanJose.Infrastructure/"]
COPY ["VetSanJose.Shared/VetSanJose.Shared.csproj", "VetSanJose.Shared/"]

RUN dotnet restore "vet San Jose/vet San Jose.csproj"

COPY . .
WORKDIR "/src/vet San Jose"
RUN dotnet publish "vet San Jose.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "vet San Jose.dll"]