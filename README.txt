=======================================================================
Welcome to:
 * Apahce.NMS : The .NET Messaging Service API
=======================================================================

For more information see http://activemq.apache.org/nms

=======================================================================
Building With NAnt 0.85 http://nant.sourceforge.net/
=======================================================================
To build the code using NAnt, run:

  nant
    
To generate the documentation, run:

  nant doc


=======================================================================
Building With Visual Studio 2005
=======================================================================

First build the project with nant, this will download and install 
all the 3rd party dependencies for you.

Open the solution File.  Build using "Build"->"Build Solution" 
menu option.

The resulting DLLs will be in build\${framework}\debug or the 
build\${framework}\release directories depending on you settings 
under "Build"->"Configuration Manager"

If you have the Resharper plugin installed in Visual Studio, you can run 
all the Unit Tests by using the "ReSharper"->"Unit Testing"->"Run All 
Tests from Solution" menu option.  Please note that you must run an 
Apache ActiveMQ Broker before kicking off the unit tests.

