#!/bin/bash

PUBLISH_PATH="../../publish/DotNetBlog-"

zip() {
	cd "../../publish/"
	zip -r "DotNetBlog-${1}.zip" "DotNetBlog-${1}"
	cd ../src/DotNetBlog.Web
}

build() {
	dotnet publish -c Release -r $1 -p:PublishSingleFile=true -p:PublishTrimmed=true -o "${PUBLISH_PATH}${1}" 
	zip $1
}

cd src/DotNetBlog.Web

build win-x64
build win-x86
build linux-x64
build linux-x86
build osx-x64

dotnet publish -c Release -p:UseAppHost=true --self-contained false -o "${PUBLISH_PATH}framework-depend"
zip "framework-depend"

cd ..