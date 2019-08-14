TSBToolSupreme README
======================================================================================================
TSBToolSupreme Version 1.2.0.0 (11 Aug 2019)
1. Confirmed mono Support
2. 'ReplaceString' function.
3. Get/Set team abbrv. City, Name

To run under Mono on Macintosh:
	1. Install Mono (full)
	2. From Terminal type "mono --arch=32 TSBToolSupreme.exe" (without the quotes)

To run under Mono on Linux:
	1. Install Mono (full)
	2. From Terminal type "mono TSBToolSupreme.exe" (without the quotes)

'ReplaceString' functionality:
I added a 'ReplaceString' function to the Input parser of TSBToolSupreme (only use for strings that are the 
									same size or shorter [ new value will be padded with spaces if shorter]).
To use it, use the following syntax:
	ReplaceString("the original string","replacement string")
		or
	ReplaceString("the original string","replacement string", 2)
	Where the '2' argument specifies that you want to replace only the 2nd occurence in the ROM (works for all numbers, not just 2)
 
 Get/Set team abbrv. City, Name:
	In GUI view Menu, check "Show Team abb., City, Name"
	When viewing contants, you'll see lines like "TEAM_ABB=BUF.,TEAM_CITY=BUFFALO,TEAM_NAME=BILLS" in the teams sections.
	The team abbreviation must be 4 characters ling, Keep the strings a reasonable length; they won't show correctly in the team
		menus if you make them too long.
	
======================================================================================================
TSBToolSupreme Version 1.0.0.3 (1 Jan 2014)
1. Little fixup in 'Hacks' support.

Added a menu item that will check every 'SET' command loaded into the editor to ensure that
you're not setting the same location multiple times (it will however ignore warning you against 
identical lines because you're setting them to exactly the same value and hence there isn't a conflict).

Also, for SET commands like:
	>>>> SET(0x2224B, {28TeamNES PromptUser:Msg="Enter desired quarter length":int(1-15)} )
The line will only apply to a 28 Team ROM, it will not be applied to a SNES or 32 team ROM.

======================================================================================================
TSBToolSupreme Version 1.0.0.2 (22 Dec 2013)
1. Added Schedule GUI
2. Added support for CXROM 1.11 (Except formations)

Double click on schedule to bring up scheduler gui.
CXROM 1.11 Rom should not really be used, but I added support for it to make the program 
more compatible with it's java (android) counterpart.
======================================================================================================
TSBToolSupreme Version 1.0.0.1 (29 Sep 2013)
Defects Fixed:
1. Should work under mono on Linux & Mac now
2. Team Formation Fix.

Features Added:
1. Hacks Menu added
2. Pro Bowl editing support (text + GUI) [Menu added under 'View']

     -------------- Hacks feature ------------------
To use 'Hacks' feature, Create a 'HACKS' directory and populate with '.txt' files.
Name 'hack' files accordingly to what they affect.
A few examples are provided.
You can prompt the user for input in the follwing way:
	SET(0x2224B, {28TeamNES PromptUser:Msg="Enter desired quarter length":int(1-15)} )
When you hit 'Apply to ROM', this will get processed and prompt the user to enter a number and place it
at location '0x2224B' in the ROM.

In this case, '28TeamNES' specifies that this is only to be used for the original nes ROM.
For a hack that applies to both the 28 & 32 team ROM you can specify the line like this:
	SET(0x2224B, {32TeamNES,28TeamNES PromptUser:Msg="Enter desired number":int(0x1-0x15)} )

For a hack that applies to both the SNES ROM you can specify the line like this:
	SET(0x2224B, {SNES PromptUser:Msg="Enter desired number":int(0x1-0x15)} )
     -----------------------------------------------

NOTE:
Program still works with TSBSeasonGen when you have TSBSeasonGen.exe, TSBData\ + NFL_DATA\ in the same 
folder.

======================================================================================================
TSBToolSupreme Version 0.9.1 beta
Defects Fixed:
1. Uniform usage for the cutscenes was not being updated.

With this update, there are no defects I know of with regard to uniform usage.
The uniform usage for the NFC west teams in the 32 team ROM is buggy, but that's 
not my fault (cxrom may be able to fix that). I believe that it works correctly for the
28 team ROMs.
======================================================================================================
TSBToolSupreme Version 0.9.0 beta
Defects Fixed:
1. NFC West run/pass preference overwriting AFC east's run/pass preference
2. Show formations doesn't work for SNES ROMs
3. Show Playbooks doesn't work for SNES ROMs.

Features Added:
1. Uniform 'Colors' Editing (for nes ROMs)
2. Uniform 'Colors' Editing GUI (for nes ROMs)
3. Pasting Text into TSBTool main GUI removes any formatting (pastes as plain text).
4. Added 'GetBytes' feature.

'GetBytes' feature:
Due to the increased popularity of the 'SET' command, I have added a 'GetBytes' feature. 
The 'GetBytes' feature will extract bytes from a ROM into 'SET' commands.
The input of the 'GetBytes' feature will look like the following:

Example #1, this will extract the bytes from the ROM from locations 0x23BC6-0x23c51

0x23BC6-0x23c51

Example #2, this will extract the bytes from the ROM from locations 0x23BC6-0x23c51, and
            the comment '# mini helmet stuff' will be in the results.
            
# mini helmet stuff
0x23BC6-0x23c51

Example #3, this will extract the bytes from the ROM from locations 0x23BC6-0x23c51, 
            with a width of 5 bytes and the comment '# mini helmet stuff' will be in the results.
            
# mini helmet stuff
0x23BC6-0x23c51,0x5   

You can have as many lines as you want with as many comments as you like.

The result of example #3 is the like the following (rest of the results are cut off):
 -----------------
# mini helmet stuff
SET(0x23bc6,0x9495968b00)
SET(0x23bcb,0x88898a8b00)
SET(0x23bd0,0xc0c1ab8307)
SET(0x23c4d,0x8f898a8b35)
...
 -----------------

======================================================================================================

======================================================================================================
TSBToolSupreme Version 0.8.1 beta
Defects Fixed:
1. Fixed defect where setting a team's formation causes the NFL leaders screens to freeze up.
======================================================================================================
======================================================================================================
TSBToolSupreme Version 0.8 beta
Features Added
1. Ability to modify SNES TSB1
2. Ability to Modify cxrom's 32 team ROM (version 100, 120)
======================================================================================================

======================================================================================================
TSBTool Version 2.0.2
Defects Fixed
1. Cannot assign Accuracy attribute to a QB through the new QUI.
   Setting a QB results in a Tool error.
2. SS's Name and attributes get assigned to the Kicker when using the 'Next button' or the 
   Position Combobox.
======================================================================================================

======================================================================================================
TSBTool Version 2.0
Features Added:
1. Playbook editing capability.
2. Context menu 
3. Hide the command prompt when in GUI mode.
4. Offensive formation editing ability
5. Delete trailing commas in the text (useful when dealing with .csv files).
6. TSBM style GUI for editing players (text area context menu, view menu).
7. GUI for editing teams (text area context menu, view menu). 
======================================================================================================

=============================Kick /Punt return hack info==============================================
It's common knowledge that the KR's max speed is set to the RT's max speed. I believe I have discovered why.

In the ROM at 0x80FF is the command sequences for the KR. In goes like this, using my interpretation of the code:

Set Pos From KO(01,00,48)
Wait for Kick
E3(30)
Catch Ball from Kick
Boost MS(0)
Take Control

I really don't know exactly what the E3 command does, but apparently it causes the player to speed up so that it is guaranteed that he will be fast enough to catch the ball. (E0 is boost RP, E1 is boost MS, E2 is boost something, and E3 is also boost MS - and maybe something else too.)

Now, once you catch the ball, there's a problem. The KR's max speed is still set to be at lightning speed due to the E3 command. So, a Boost MS (0) command is issued which sets the MS back to the original speed.

However, this is where the glitch kicks in. Somehow TSB forgets that the 11th player is now the KR, and it loads the MS from the 11th slot (RT). Now, you have an ultra slow KR in most cases.

It is possible to fix the glitch by changing the bytes in boldface. 

SET( 0x08100, 0x0048ecefe4 )

======================================================================================================

TSBTool Version 1.2.0.2
Fixed 2 defects which resulted from my error checking for version 1.2.0.1.
Defects Fixed:
1. Player jersey numbers set to 0 (error checking statement used base 10 instead of base 16[hex] numbers
    'if(jerseyNumber > 99)' instead of 'if(jerseyNumber > 0x99)' ).
2. Bills team sim data not set (error checking statement used 'if(loc > 0)' instead of 'if(loc > -1)' ).

======================================================================================================
TSBTool Version 1.2.0.1

Fixed critical Defect where a 'bad' character in a player's name can mess up a ROM.
Sblueman encountered a defect in a CFL ROM where the incorrect handling of a single quote 
character messed up his ROM. 
I looked into it, and found that the error checking was minimal for player names.
I vastly improved error checking for 'bad' input at both the text input level and the 
ROM manipulation level. If you see an error with '(low level)' in it, please report it in the 
TSBToolSupreme thread at knobbe.org. "low level" errors just tell me that there's a hole in my high 
level error checking, and I should fix it in order to tell the user what line it occurred on.
Errors titled (low level) do not mean that your ROM is hosed, it's just an error that should
have been caught in the text parsing level.



=======================================================================================================
TSBTool Version 1.2

Defects Fixed:
1. Strong Safety Faces are all wrong.
2. Falcons Punter name gets messed up (data shift error at end of name segment).
3. NT - SS Sim data locations are incorrect.
4. Team Sim data getting over written.
5. KR/PR incorrect on starter reset.
6. "\r\n" Windows behavior not supported (for notepad).


Features added:
1. TsbSeasonGen add-in to a menu for TSBTool. 
2. Team Sim Offensive Preference (heavy rushing, little more rushing, heavy passing, little more passing)
3. Ability to change the amount of games in a week. 1-14 games Possible in a week.
4. SET function added for 'scripting' hex edit changes.
      SET(romLocation, value)
5. Display of warnings and errors in a message box ( in GUI mode ).
6. 'Number guys' Feature. (View -> Number Guys tool)

Version 1.2 of TSBTool offers the user more than just bug fixes to the original.

1. Since many people are uncomfortable with the command line interface of TSBSeasonGen, I've added
   a menu item for it. Simply select the menu, and provide a valid year (or a valid config file). 
   TSBSeasonGen will be exec'ed in another process, and it's output will be displayed in TSBTool's 
   Text Area.

2. You now have the ability to specify a team's offensive preference in simulated games 
   (thanks to jstout and malfreds for giving me the info on how to do this).

3. The scheduler is also much improved. With 1.0* you could not change the number of games in a week,
   and you had to stick to the confines of the original TSB with regard to games in a given week.
   With v 1.2 those limitations are gone. You can schedule up to 14 games in a week.
   
4. For you hardcore HEX guys I have included the 'SET' function. Which will allow you to script 
   hex edits that are otherwise unsupported by TSBTool. You use it like this:
   SET( <address>, <value> )

   Examples:    
   SET( 0x556677, 0xff)
   SET( 0x334455, 0xaabbccdd) 
   
   The first example will set the value at location '0x556677' to 'ff'.
   The second example will set the value at 0x334455 to 'aa', 0x334456 to 'bb', 0x334457 to 'cc', 0x334458 to 'dd'.
   The value can be as long as you want, but it can’t have any spaces in it.
   
   NOTE: the '0x' is the convention used by many programming languages to indicate that the value is in hexadecimal.

5. Warnings and Errors.
   Unlike TSBTool 1.0*, I now show error and warning messages in a GUI message box (these were simply printed to stderr before).
   A 'Warning' is classified as something that may not be optimal, but not necessarily bad.
   An 'Error' is classified as something that is most likely BAD.

6. 'Number Guys' Feature.
    This is the last feature added to TSBTool 1.2. This feature attempts to help the user assign numbers to the players
    whose numbers are 0.

    If you're like me, you probably have some 'Sporting News' or 'Street and Smiths' or some other Football preview
    magazines laying around. These (unfortunately) are  the best record for player jersey numbers. I have found no such source
    for player jersey numbers online. So I could not include all that data. I found many player numbers in various 'Good'
    TSB Roms, but still there are many numberless players. It's kinda painful to go through and assign numbers to all of
    them, so I added 'Number Guys' to take as much pain away as possible. The idea is that you have a magazine open
    to a roster page, start 'Number Guys' and assign numbers to the players. 

    To use it, put some Tecmo roster data into the text area (possibly with TSBSeasonGen), then choose View-> Number Guys tool.
    By default it will look for all the players whose jersey number is '0' and it will prompt you to enter their
    jersey number. To go through the data looking at every player clear the value in 'Stop On'. 
    You can go through all the data, or if you only want to do a certain area just highlight that area with the 
    mouse and then open up the tool.

    You can simply press 'Enter'(this is the preferred method) to move onto the next guy ot hit the next button.
    You will be prompted with a 'Done' message when there are no more guys to number.
    Select OK to apply the changes, select cancel to cancel the changes (all the numbers you assigned will be lost).
    If you want to stop using the tool and wish to apply the changes you have entered, select 'OK'.
    To cancel the action at any time, select cancel. 

======================================================================================
TSBTool Version 1.0.1

Added PR/KR feature to TSBTool
Fixed defect where comma had to be directly after player's name.
Fixed QB attribute defect where values for Pass Accuracy and Avoid Pass Block were also saved to passing speed and pass control.
Fixed Defect where FS face data would overwrite LILB Pass Interceptions and Quickness.
=======================================================================================

TSBTool Version 1.0

Requirements:
Microsoft.NET framework 1.1

You can obtain the .NET framework from Microsoft for free by going to 
http://www.dotnetgerman.com/links/dl_distri.asp

If you are a programmer, and do not have the .NET framework installed, you may want to
consider downloading the .NET sdk. It's also a free download from Microsoft.
http://www.dotnetgerman.com/links/dl_dotnetsdk.asp

Also, I have verified that it does work on Linux under Mono 1.1.10 (www.mono-project.com).
It should work from the command line in mono 1.1 and later. The GUI for TSBToolSupreme may work (depending on
your mono setup). The GUI does not seem to work on mono under Linux very well (due to mono's limitations).


============================== About =================================================
This tool reads human readable text files, and applies this data to a Tecmo Super Bowl ROM.
This tool was created because I wanted the ability to change a whole lot of data in TSB in 
a short amount of time. I use the command line every day, and I thought that I would write this
program in the old Unix style (give the option to read from standard in or from a file, write
errors to standard error, support command line options). One of my goals for this program was to make
it easy for other programs to interact with it.
Let's say you know a little about programming, you want to make your own season scheduling program.
You could write a program that simply writes to standard out stuff like:
49ers at rams
oilers at giants
....

Then you could use my program to help you change the schedule in TSB.
Something like this 'scheduleProg 1985 | TSBToolSupreme TSB.nes -out:TSB1985.nes'

What the program can do:
  Assign and display player names, attributes, sim data.
  Assign and display Teams' "Sim Data".
  Assign and display the ROM's Year.
  Show schedule and schedule Games.
  Show and change Team Formations
  Show and change Team Playbooks
  

Expected Input:
Note: Lines that start with the '#' character are ignored.

#To modify a team's roster and sim data do the following.
TEAM = bills SimData=0xab
# The line above will tell the program that you are editing the roster for the bills.
#the SimData is used when the cpu simulates games, this will give the bills an offense sim number
#of 'a' (10) and the defense a sim number of 'b' (11). 0-9, a-f are valid (hex digits).

# now to modify a player. The following line will set data for QB1 on the bills.
QB1, jim Kelly, Face=0x22, #12, 25, 69, 13, 13, 56, 81, 81, 81 ,[3, 12, 3 ]

#To start changing data on another team, simply type in something like this.
TEAM = browns SimData=0x38
QB1, Bernie Kosar, Face=0x22, #19, 25, 69, 13, 13, 56, 44, 44, 44 ,[3, 12, 3 ]

NOTE: to see how faces Map, consult the 'Faces' directory for the TSBM program.

#Schedule games.

# The following line will tell the program that you now plan to schedule games for week 2.
WEEK 2

49ers at falcons
giants at redskins

# We have just scheduled the first 2 games for week 2.
# NOTE: You CAN schedule 14 games every week.

==================================== Program Usage ==========================================
This tool was intended to be used from the command line or from the GUI.

GUI:
To invoke the GUI, simply double click on TSBTool.exe, or call it from the command line with no arguments 
(or with the '/gui' command line switch).
In the GUI version, all the error messages and warning messages are displayed in a message box.

In the GUI, you have 5 primary options to choose from:
1. Load TSB nes file:
   Loads a Tecmo super bowl file (nes only) into memory.
2. View Contents:
   Extracts all player and schedule data, dumps it to the text box.
3. Save Data:
   Saves the text in the text box to a file.
4. Load Data:
   Loads a text file into the text box.
5. Apply to ROM:
   Applies the contents of the text box to the ROM loaded.



Command Line:
To view the help message type 'TSBToolSupreme /?' from the command line.

To view all player and schedule data from a ROM called TSB.nes do the following:
cmd> TSBToolSupreme TSB.nes

To modify TSB.nes with the data contained in the file Data.txt do the following:
cmd> TSBToolSupreme TSB.nes Data.txt

To create a new TSB file based on the contents of TSB.nes, modified with the data contained 
in the file Data.txt do the following:
cmd> TSBToolSupreme TSB.nes Data.txt -out:MyFile.nes

Pipes and re-direction
This program was created with pipes and re-direction in mind. If you wanted to pipe the output 
from your own custom program into TSBTool, you can.
cmd> myCustomProgram -1985 | TSBToolSupreme TSB.nes -stdin -out:1985TSB.nes

You can also do something like:
cmd> type MyTSB_Data.txt > TSBToolSupreme TSB.nes -stdin -out:Special.nes
Note: 'type' is a windows program that reads the data from the file it was passed, and prints it's
contents to standard out (like 'cat' on UNIX Linux).

NOTE:
When using TSBTool, specification of input ROM means that all data from the input ROM is imported into memory.
'Applying to ROM' will take that rom's data, apply the text data that you provided, and write to the '.nes' file
you specify. 
-- If you read in Tsb_Rose.nes, changed the text (player/team data) and then Applied to
   TSPRBOWL.nes we will in effect overwrite TSBRBOWL.nes with Tsb_Rose plus your modifications.

================= What this program does not do that I wish it did ===============
1. This program does not change the number of games in a week. -- Now you can
2. This program does not allow you to assign a PR or KR.       -- Now You can

What this program does not do that I kinda think would be neat:
1. Does not assign Pro Bowl players.

=========================== Thanks to Emuware =====================================
I'd like to thank the guys at Emuware for making this program possible. Without the use of their TSBM program,
(which I used heavily during this program's development) this program would not exist.
I'd like to point out that you can modify more things with emuware's TSBM and TSBM2000 programs than you can
with TSBTool. 

============================ Defects ==============================================
At the time of this writing I don't know of any defects in this program.
This program has defects in it, I just haven't found them yet. If you find them
post your findings in a thread in the forums at www.knobbe.org .

=========================== Disclaimer ===============================================
Use this program at your own risk. TSBToolSupreme creator is not responsible for anything bad that happens.
User takes full responsibility for anything that happens as a result of using this program.

This Program is not endorsed or related to the Tecmo video game compny.
