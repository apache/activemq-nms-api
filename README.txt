=======================================================================
Welcome to:
 * Apache.NMS : The .NET Messaging Service Client API
=======================================================================

For more information see http://activemq.apache.org/nms

=======================================================================
Building With NAnt 0.86 see http://nant.sourceforge.net/
=======================================================================

NAnt version 0.86 or newer is required to build Apache.NMS.  Version 0.90
or newer is highly recommended.
To build the code using NAnt, run:

  nant

To generate the documentation, run:

  nant doc

=======================================================================
Building With Visual Studio 2008
=======================================================================

First build the project with nant, this will download and install 
all the 3rd party dependencies for you.

Open the solution File.  Build using "Build"->"Build Solution" 
menu option.

The resulting DLLs will be in build\${framework}\debug or the 
build\${framework}\release directories depending on your settings 
under "Build"->"Configuration Manager"

If you have the Resharper plugin installed in Visual Studio, you can run 
all the Unit Tests by using the "ReSharper"->"Unit Testing"->"Run All 
Tests from Solution" menu option.  Please note that you must run an 
Apache ActiveMQ Broker before kicking off the unit tests.  Otherwise,
the standalone NUnit test runner can be used.  NUnit version 2.5.8
is required to build and run the unit tests.

