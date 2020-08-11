$PUBLISH_PATH="../../publish/DotNetBlog-"

function DotNetBlog-Zip() {
    Param(
        [string] $key
    )

	cd "../../publish/"
    ../tools/7z.exe a "DotNetBlog-$($key).zip" "DotNetBlog-$($key)"
	cd ../src/DotNetBlog.Web
}

function DotNetBlog-Publish
{
    Param(
        [string] $key
    )

    dotnet publish -c Release -r $key -p:PublishSingleFile=true -p:PublishTrimmed=true -o "$($PUBLISH_PATH)$($key)"
    DotNetBlog-Zip $key
}

cd src/DotNetBlog.Web

DotNetBlog-Publish "win-x64"
# DotNetBlog-Publish "win-x86"
DotNetBlog-Publish "linux-x64"
# DotNetBlog-Publish "linux-x86"
DotNetBlog-Publish "osx-x64"

dotnet publish -c Release -p:UseAppHost=true --self-contained false -o "${PUBLISH_PATH}framework-depend"
DotNetBlog-Zip "framework-depend"

cd ../..