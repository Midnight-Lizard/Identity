#===========================================#
#				PROJECT	BUILD				#
#===========================================#
FROM microsoft/aspnetcore-build:2 as proj-build
COPY . /build
WORKDIR /build
RUN dotnet restore
RUN dotnet publish -c Release -o ./results

#===========================================#
#				IMAGE	BUILD				#
#===========================================#
FROM microsoft/aspnetcore:2 as image
WORKDIR /app
EXPOSE 80
COPY --from="proj-build" /build/results .
ENTRYPOINT ["dotnet", "MidnightLizard.Web.Identity.dll"]
