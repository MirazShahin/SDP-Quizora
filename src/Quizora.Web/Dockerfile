FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Quizora.Web/Quizora.Web.csproj
RUN dotnet publish src/Quizora.Web/Quizora.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE 8080
ENTRYPOINT ["dotnet", "Quizora.Web.dll"]