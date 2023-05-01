FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
#EXPOSE 80
#EXPOSE 443
#EXPOSE 5001

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY . .
COPY ["epiwebsurvey_api.csproj", ""]
RUN dotnet restore "./epiwebsurvey_api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "epiwebsurvey_api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "epiwebsurvey_api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ARG client_id
ARG client_secret
ARG endpoint_authorization
ARG endpoint_token
ARG endpoint_user_info
ARG endpoint_token_validation
ARG endpoint_user_info_sys
ARG callback_url
ARG epiinfosurveyapiContext
ENV client_id ${client_id}
ENV client_secret ${client_secret}
ENV endpoint_authorization ${endpoint_authorization}
ENV endpoint_token ${endpoint_token}
ENV endpoint_user_info ${endpoint_user_info}
ENV endpoint_token_validation ${endpoint_token_validation}
ENV endpoint_user_info_sys ${endpoint_user_info_sys}
ENV callback_url ${callback_url}
ENV epiinfosurveyapiContext ${epiinfosurveyapiContext}
# exposing port via tcp
EXPOSE 9015/tcp
ENV ASPNETCORE_URLS http://*:9015
RUN chown 1001:0 epiwebsurvey_api.dll
RUN chmod g+rwx epiwebsurvey_api.dll
USER 1001
ENTRYPOINT ["dotnet", "epiwebsurvey_api.dll"]