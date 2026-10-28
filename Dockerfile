# Dockerfile for Legacy GadgetsOnline (.NET Framework 4.8)

# Use the official Microsoft .NET Framework image
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019

# Set the working directory in the container
WORKDIR /inetpub/wwwroot

# Copy the legacy application files to the container
# Note: In a real scenario, we would build the app first or copy the published output.
# For this setup, we assume the files are located in .legacy-source/GadgetsOnline/GadgetsOnline/
COPY GadgetsOnline/GadgetsOnline/ .

# Expose port 80 for web access
EXPOSE 80

# The base image already starts IIS, so no CMD is strictly required unless we want to override it.
