Epuck-lib
This directory contains tools for robot e-Puck.
Developed by Ondrej Platek and supervised by RNDr. Frantisek Mraz, CSc.

Requirements programs of this folder: 
	- .Net 2.0 or Mono 2.0 and higher to compile and run the programs	
	- e-Puck and Bluetooth to use them reasonably
	

Content of folders with source code
	testelib - TestElib project: Contains Elib.dll (source codes in elib folder and introduce how to controll e-Puck
		 - Start here or even better at http://code.google.com/p/epuck-lib/
	elib - Library for controlling e-Puck
	joystick - Elib Joystick application use WPF(Windows Presentation Foundation)
		   available only in .Net 2.0 and above.
                   Mono runtime does not implement WPF!!! -> not available on Linux
	et - Elib Tools: tools for logging e-Puck actions
	simulator - very simple testing tool: It helped me with debugging (not worth to use it)




Microsoft Visual Studio Solution(MSVS)  Upgrades/Downgrades Problems:
	Remark: Solution is file *.sln
	Upgrade MSVS 2005-> MSVS 2008->MSVS 2010  ... NO problem
	Conversion MSVS 2005,2008,2010 to monodevelop project no problem
	Monodevelop project to MSVS 2005,2008,2010 to ..IRRELEVANT (I publish in MSVS project-but POSSIBLE currently I develop under Mono)
	DOWNGRADE (OF SOLUTION IN THIS FOLDER: it is not general guide for other projects)
		By manual editing *.sln files:
			MSVS 2010 -> MSVS 2008
			replace:	
				Microsoft Visual Studio Solution File, Format Version 11.00
				# Visual Studio 2010
			by following lines:
				Microsoft Visual Studio Solution File, Format Version 10.00
				# Visual Studio 2008
	

SVN - you can obtain source code by command
	svn checkout http://epuck-lib.googlecode.com/svn/trunk/ epuck-lib-read-only

Remarks for linux users:
	Do not be afraid using Elib.dll and *.exe files.
	The files are "interpreted" by Mono runtime.

More info on http://code.google.com/p/epuck-lib/

Thank you 
Ondrej Platek
