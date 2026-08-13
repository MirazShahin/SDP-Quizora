FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# csproj আগে কপি → restore cache ভালো হয়
COPY ["src/Quizora.Web/Quizora.Web.csproj", "src/Quizora.Web/"]
COPY ["src/Quizora.Application/Quizora.Application.csproj", "src/Quizora.Application/"]
COPY ["src/Quizora.Domain/Quizora.Domain.csproj", "src/Quizora.Domain/"]
COPY ["src/Quizora.Infrastructure/Quizora.Infrastructure.csproj", "src/Quizora.Infrastructure/"]

RUN dotnet restore "src/Quizora.Web/Quizora.Web.csproj"

COPY . .
WORKDIR /src/src/Quizora.Web
RUN dotnet publish "Quizora.Web.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Quizora.Web.dll"]