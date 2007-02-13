=======================================================================
Welcome to:
 * NMS API : The .NET Messaging Service API
 * NMS Client for Apache ActiveMQ
 * NMS Client for MSMQ
=======================================================================

For more information see http://activemq.apache.org/nms.html

=======================================================================
Building With Visual Stuido 2005
=======================================================================

Open the vs2005.sln Solution File.  Build using "Build"->"Build Solution" 
menu option.

The resulting DLLs will be in bin\Debug or the bin\Release directories 
depending on you settings under "Build"->"Configuration Manager"

If you have the Resharper plugin installed in Visual Studio, you can run 
all the Unit Tests by using the "ReSharper"->"Unit Testing"->"Run All 
Tests from Solution" menu option.  Please note that you must run an 
Apache ActiveMQ Broker before kicking off the unit tests.

=======================================================================
Building With SharpDevelop 2.1
=======================================================================

SharpDevelop is an open source IDE that you can download from:
http://www.sharpdevelop.net/OpenSource/SD/Default.aspx

Open the sharpdevelop.sln Solution File.  Build using the 
"Build"->"Build Solution" menu option.

The resulting DLLs will be in bin\Debug or the bin\Release directories 
depending on you settings under "Build"->"Set Configuration"

To run the Unit Tests, you must first start an Apache ActiveMQ Broker 
and then use the "View"->"Tools"->"Unit Tests" menu option.  On the
newly displayed Unit Tests view, click the "Run tests" button.

Note: Seems like the nunit integration in SharpDevelop still needs a 
little work because on a WindowsXP x64 system, running the tests caused 
some Unexpected Exception errors.

=======================================================================
Building With monodevelop 
=======================================================================

monodevelop is an open source IDE that you can download from:
http://www.monodevelop.com/

Open the monodevelop.mds Solution File.  Build using the 
"Run"->"Build Solution" menu option.

The resulting DLLs will be in bin\Debug or the bin\Release directories 
depending on the configuration you have selected in the Build bar.

=======================================================================
Building With NAnt
=======================================================================
To build the code using NAnt type

  nant
  
To run the unit tests you need to run an Apache ActiveMQ Broker first then type

  nant test
  
To generate the documentation type

  nant doc

  
Assuming that you have checked out the ActiveMQ code and the Site in peer directories such as


activemq/
  activemq-dotnet/
  
activemq-site/
   nms/
      ndoc/
      
So that generating the ndoc will copy the generate content into the ndoc directory so it can be deployed on the Apache website.

