FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# csproj গুলো আগে কপি (restore cache)
COPY src/Quizora.Domain/Quizora.Domain.csproj Quizora.Domain/
COPY src/Quizora.Application/Quizora.Application.csproj Quizora.Application/
COPY src/Quizora.Infrastructure/Quizora.Infrastructure.csproj Quizora.Infrastructure/
COPY src/Quizora.API/Quizora.API.csproj Quizora.API/

RUN dotnet restore Quizora.API/Quizora.API.csproj

# পুরো source
COPY src/Quizora.Domain/ Quizora.Domain/
COPY src/Quizora.Application/ Quizora.Application/
COPY src/Quizora.Infrastructure/ Quizora.Infrastructure/
COPY src/Quizora.API/ Quizora.API/

RUN dotnet publish Quizora.API/Quizora.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Quizora.API.dll"]