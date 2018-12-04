#!/bin/bash

echo "Script started."
echo "$(date +"%r")"

# Install package dependencies
# https://dotnet.microsoft.com/download/linux-package-manager/fedora28/sdk-current
# Add the dotnet product feed
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
wget -q https://packages.microsoft.com/config/fedora/27/prod.repo
sudo mv prod.repo /etc/yum.repos.d/microsoft-prod.repo
sudo chown root:root /etc/yum.repos.d/microsoft-prod.repo

# Install the .NET SDK
sudo dnf update
sudo dnf install dotnet-sdk-2.1

# Install zip package
sudo dnf install zip

# Create Release dir
cd ../
RELEASEDIR=Release
rm -rf $RELEASEDIR
mkdir -p $RELEASEDIR

# Publish .NET Core build
PRODUCTNAME="de4dot"
NETCOREVER="netcoreapp2.1"
echo "Publishing "${NETCOREVER}"release..."
OUTPUTPATH="publish-"${NETCOREVER}
dotnet publish -c Release -f $NETCOREVER -o $OUTPUTPATH $PRODUCTNAME
dotnet clean -c Release -f $NETCOREVER -o $OUTPUTPATH $PRODUCTNAME

rm -rf ./${RELEASEDIR}/${NETCOREVER}

rm -rf ./${PRODUCTNAME}/${OUTPUTPATH}/*.pdb
rm -rf ./${PRODUCTNAME}/${OUTPUTPATH}/*.xml

# Output build
echo "Output files..."
OUTPUT2=${PRODUCTNAME}"-"${NETCOREVER}".zip"

cd ./${PRODUCTNAME}/${OUTPUTPATH}
zip -9 -q -r $OUTPUT2 .
cp -xar $OUTPUT2 ../../${RELEASEDIR}/

cd ../../
rm -rf ./${PRODUCTNAME}/${OUTPUTPATH}

echo "$(date +"%r")"
echo "Script ended."
exit 0

