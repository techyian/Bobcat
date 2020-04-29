# Bobcat - Websocket Streaming with MMALSharp

Welcome to the Bobcat project which aims to demonstrate how to stream raw YUV420 video frames to a web browser
using Websockets and the Raspberry Pi camera module. In order to achieve this, the project makes use of a number of 
technologies:

1) [MMALSharp](https://github.com/techyian/MMALSharp)
1) [JSmpeg](https://github.com/phoboslab/jsmpeg)
1) [websocket-client](https://github.com/Marfusios/websocket-client)
1) [Protobuf-net](https://github.com/protobuf-net/protobuf-net)
1) FFmpeg
1) ASP.NET Core
1) Knockout.JS

### Architecture

Bobcat features 3 main actors to support its use:

- Pi Client with camera module
- ASP.NET Core "relay" server
- Client Internet browser

The architecture surrounding Bobcat runs on the principal of there being a "Pi Client" which will be streaming the video; this client uses Websockets to stream raw YUV420 video frames to a server running ASP.NET Core. The server intercepts the video frames and will push them out to any Internet browser which has connected again via Websockets to the server. 

### Dependencies

- NodeJS
- .NET Core 3.1 SDK & Runtime
- FFmpeg (Client only)

### Setup

#### Windows

##### Step 1 - Deploy Client build to your Raspberry Pi

Run the `build.cmd` file which will do a build of the solution and also download the client-side dependencies required. The output files will be in `Bobcat.Client\bin\Release\netcoreapp3.1\linux-arm\publish`. These files
need moving over to a folder on your Raspberry Pi.

##### Step 2 - Publish and deploy Web project


The Cake script does not publish the Web project as you are free to deploy to whatever architecture you wish using the `dotnet publish` command. If you are deploying to a Linux environment, you should ensure that you are using
a proper Web server such as NginX to host the application.

##### Step 3 - Update appsettings.json files

Both the Client and Web projects have their own respective appsettings.json files which contain configuration strings. The current values to be concerned with are `RelayServerHostname` (both projects) and `UniqueId` (Client only), where the 
`RelayServerHostname` represents the websocket URL of the relay server, e.g. `ws://localhost:44369`, and `UniqueId` is a unique identifier for the client (I tend to use GUIDs).

##### Step 4 - Start Client application

You should be able to run the client by running `./Bobcat.Client` on your Raspberry Pi (you may need to run `chmod +x ./Bobcat.Client` first). If your configuration is correct, FFmpeg should start and the camera should now be streaming.

##### Step 5 - Navigate to Web application and create a connection


If your setup has gone smoothly you should now be able to browse to the URL where your web application is hosted. Click on the `Create Connection` button and find your Pi.


#### Linux

##### Step 1 - Deploy Client build to your Raspberry Pi

Using the .NET Core SDK, navigate to the location of `Bobcat.Client.csproj` and run `dotnet build && dotnet publish -c Release -r linux-arm`. The output files will be in `Bobcat.Client\bin\Release\netcoreapp3.1\linux-arm\publish`.
These files now need moving over to a folder on your Raspberry Pi.

##### Step 2 - Publish and deploy Web project

Navigate to the location of `Bobcat.Web.csproj` and run `dotnet build && dotnet publish -c Release -r ENVIRONMENT` (where ENVIRONMENT is the environment of where the Web application will be hosted). If you are deploying to a Linux environment, you should ensure that you are using
a proper Web server such as NginX to host the application.

##### Step 3 - Update appsettings.json files


Both the Client and Web projects have their own respective appsettings.json files which contain configuration strings. The current values to be concerned with are `RelayServerHostname` (both projects) and `UniqueId` (Client only), where the 
`RelayServerHostname` represents the websocket URL of the relay server, e.g. `ws://localhost:44369`, and `UniqueId` is a unique identifier for the client (I tend to use GUIDs).

##### Step 4 - Start Client application

You should be able to run the client by running `./Bobcat.Client` on your Raspberry Pi (you may need to run `chmod +x ./Bobcat.Client` first). If your configuration is correct, FFmpeg should start and the camera should now be streaming.

##### Step 5 - Navigate to Web application and create a connection

If your setup has gone smoothly you should now be able to browse to the URL where your web application is hosted. Click on the `Create Connection` button and find your Pi.


### FAQ

**Pi Client - The server returned status code '400' when status code '101' was expected.**

This error can occur when the server returns a HTTP 400 error due to the IP address or hostname not being recognised. If launching
the web application using IISExpress, you will need to add bindings to the `applicationhost.config` file, this can be found  within the solution directory in a hidden folder `.vs\Bobcat\config`. Once you've opened `applicationhost.config`, search for the text block which looks similar to the below:

```
<bindings>
  <binding protocol="http" bindingInformation=":8080:localhost" />
  <binding protocol="http" bindingInformation="192.168.1.92:44369:*" />
</bindings>
```

Here I have added `192.168.1.92:44369` as my IISExpress instance is running on that IP address and port - you will need to add one similar for your IP address and port or hostname.


### Known issues

**Client websocket send sometimes includes additional rubbish data at end of payload** - Sometimes additional UTF-8 payload data will be seen at the end of the send payload which causes
an issue parsing the data on the relay server. This is caught in a try/catch but no action is taken currently - if you find that a config change hasn't been carried out, simply try again
and it should be ok.

**Memory access denied** - Occasionally when viewing a video stream I have noticed Javascript console errors relating to memory access being denied. This issue causes the video to freeze with green stripes. To rectify this, reload the 
webpage and try again.

**Latency** - If latency increases you will begin to notice artifacts appearing on your stream. I will be looking into ways to decrease latency.