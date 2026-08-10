FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Quizora.Web/Quizora.Web.csproj", "Quizora.Web/"]
COPY ["src/Quizora.Application/Quizora.Application.csproj", "Quizora.Application/"]
COPY ["src/Quizora.Domain/Quizora.Domain.csproj", "Quizora.Domain/"]

RUN dotnet restore "Quizora.Web/Quizora.Web.csproj"

COPY . .
WORKDIR "/src/src/Quizora.Web"
RUN dotnet publish "Quizora.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Quizora.Web.dll"]