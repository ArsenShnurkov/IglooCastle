all:
	xbuild
	cd IglooCastle.Tests/bin/Debug && nunit-console4 -noshadow -nothread IglooCastle.Tests.dll
	cd IglooCastle.CLI/bin/Debug && mono IglooCastle.CLI.exe

clean:
	rm IglooCastle.CLI/bin/Debug/*.*
	rm IglooCastle.Tests/bin/Debug/*.*

