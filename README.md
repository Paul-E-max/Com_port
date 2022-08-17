# Introduction 
This is a simple COM port application geared up to the Foster & Freeman products.  On entry, there is a bold Connect/Disconnect button with the product name and baudrate next to it.


![](Documents/COMport_view.png)


# Getting Started
1.	Copy the files from bin\Release directory to your C:\WorkArea
2.	Create a shortcut to ProjectRelabel.exe on your desktop
3.	Product names, baudrates and commands for getting product ID are held in COMport.CSV
4.	Start the application using the shortcut
5.  Select the product and port from there drop down boxes
6.  The baudrate should be filled in by the contents of COMport.CSV
7.  Click on the Connect button and wait for the button to go green


![](Documents/COMport_connected.png)


# Build and Test
*   Use Visual Studio 2019 to open the solution file (sln)
*   Select Release in Solution Configuration
*   Do Build\Rebuild Solution
*   Execute the exe found in bin\Release directory
