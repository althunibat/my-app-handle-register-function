FROM openfaas/of-watchdog:0.8.0 as watchdog

#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
WORKDIR /app
EXPOSE 80
COPY ./release .

COPY --from=watchdog /fwatchdog /usr/bin/fwatchdog
RUN chmod +x /usr/bin/fwatchdog

ENV fprocess="dotnet HandleRegistrationAction.dll"
ENV upstream_url="http://127.0.0.1:80"
ENV mode="http"
HEALTHCHECK --interval=5s --timeout=10s --retries=3 CMD curl --fail http://localhost:80/hc || exit 1
ENTRYPOINT ["fwatchdog"]