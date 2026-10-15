FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY MyPlantCare/MyPlantCare.csproj MyPlantCare/
RUN dotnet restore MyPlantCare/MyPlantCare.csproj
COPY MyPlantCare/ MyPlantCare/
RUN dotnet publish MyPlantCare/MyPlantCare.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyPlantCare.dll"]
