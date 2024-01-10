pushd %~dp0
protoc.exe -I=./ --csharp_out=./ ./Enum.proto
protoc.exe -I=./ --csharp_out=./ ./Struct.proto
protoc.exe -I=./ --csharp_out=./ ./Protocol.proto
protoc.exe -I=./ --csharp_out=./ ./Test.proto



