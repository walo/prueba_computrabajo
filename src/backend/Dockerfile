FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Core/Domain/Domain.csproj", "src/Core/Domain/"]
COPY ["src/Core/Application/Application.csproj", "src/Core/Application/"]
COPY ["src/Infrastructure/Authentication/Authentication.csproj", "src/Infrastructure/Authentication/"]
COPY ["src/Infrastructure/ExternalServices/ExternalServices.csproj", "src/Infrastructure/ExternalServices/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Infrastructure/Shared/Shared.csproj", "src/Infrastructure/Shared/"]
COPY ["src/Presentation/Web.API/Web.API.csproj", "src/Presentation/Web.API/"]

RUN dotnet restore "src/Presentation/Web.API/Web.API.csproj"

COPY . .
WORKDIR "/src/src/Presentation/Web.API"
RUN dotnet publish "Web.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Web.API.dll"]
