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