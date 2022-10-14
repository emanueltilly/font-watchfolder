# font-watchfolder
Tool for automatically installing new fonts from a watchfolder in Windows

Font Watchfolder listens for changes in the watchfolder and will install all fonts not available on the local machine.

The application needs to run with administrative privileges.

Start the executable with watchfolder argument.
Example:
--watchfolder "\\192.168.1.100\Shared Folder\Fonts"

Tips:
Use Windows Task Scheduler to automatically start the application with highest privileges and correct arguments.
