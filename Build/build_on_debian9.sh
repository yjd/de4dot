#!/bin/bash

echo "Script started."
echo "$(date +"%r")"

# Check if Debian
if [ ! -f "/etc/debian_version" ];
then
    echo "Debian required."
    exit 1
fi

# Install package dependencies
# https://dotnet.microsoft.com/download/linux-package-manager/debian9/sdk-current
# Register Microsoft key and feed
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg
sudo mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/
wget -q https://packages.microsoft.com/config/debian/9/prod.list
sudo mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
sudo chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg
sudo chown root:root /etc/apt/sources.list.d/microsoft-prod.list

# Install the .NET SDK
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1

# Install zip package
sudo apt-get install zip

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

