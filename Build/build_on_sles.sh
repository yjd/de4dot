#!/bin/bash

echo "Script started."
echo "$(date +"%r")"

# Install package dependencies
# https://dotnet.microsoft.com/download/linux-package-manager/sles/sdk-current
# Add the dotnet product feed
sudo rpm -Uvh https://packages.microsoft.com/config/sles/12/packages-microsoft-prod.rpm

# Install the .NET SDK
sudo zypper update
sudo zypper install dotnet-sdk-2.1

# Install zip package
sudo zypper install zip

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

