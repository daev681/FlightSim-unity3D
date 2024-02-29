protoc.exe -I=./ --csharp_out=./ --csharp_opt=file_extension=.cs ./test.proto
IF ERRORLEVEL 1 PAUSE