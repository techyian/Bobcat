# Bobcat - Websocket Streaming with MMALSharp

Welcome to the Bobcat project which aims to demonstrate how to stream raw YUV420 video frames to a web browser
using Websockets and the Raspberry Pi camera module. In order to achieve this, the project makes use of a number of 
technologies:

1) [MMALSharp](https://github.com/techyian/MMALSharp)
1) [JSmpeg](https://github.com/phoboslab/jsmpeg)
1) [websocket-client](https://github.com/Marfusios/websocket-client)
1) FFmpeg
1) ASP.NET Core
1) Knockout.JS

### Architecture

Bobcat features 3 main actors to support its use:

- Pi Client with camera module
- ASP.NET Core server
- Client Internet browser

The architecture surrounding Bobcat runs on the principal of there being a "Pi Client" which will be streaming the video; this client uses Websockets to stream raw YUV420 video frames to a server running ASP.NET Core. The server intercepts the video frames and will push them out to any Internet browser which has connected again via Websockets to the server. 

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

