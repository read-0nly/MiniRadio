# MiniRadio
An MP3 internet radio server built purely out of sockets, no external libraries. 

To start it, enter the IP you want to bind (your own IP, but in some networks there's more than one of those), the port you want to bind for hosting, then enter a path to a folder containing album subfolders that contain MP3s, it'll create a /album path for each. Browse to the endpoint to stream the album.


For example: localhost:8001/Crisis to listen to the album titled Crisis. Capitalization matters, I plan to fix it.


Each album has a "playhead" and tries to keep clients relatively in sync, and I've tried to tune it so it doesn't send many more frames that can be listened to in the period between framebursts - buffer still builds up and causes desync depending on listen time though. 

The playhead advances after updating all connected clients, and does not advance if there are no clients. 

So if you disconnect and were the only listener on that album, the stream will continue at the last frame it sent you - as long as you reconnect before you run out of buffer on the same connection, there should be no interruption. There is some drift the longer you're connected though, so for instance a page refresh after listening for a bit will start feeding you where the playhead is, not the last part you heard.





At some point I'll add support for artist/album/song instead of just album/song



Don't point this at a huge mess of music folders - if you have a ton of music it'd be better to place all the mp3 files together into an artist folder. It's a restriction of how the playheads are implemented for each album. It'll be less bad once I implement artist/album/song, artist will manage which albums get playheads and will destroy playheads that have been abandoned past a timeout.

Useful documentation

https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-10.0

http://mpgedit.org/mpgedit/mpeg_format/mpeghdr.htm (I love you thank you so much Predrag Supurovic you will probably never see this because the article is almost 30 years old but you have taught me so much in that one document)

https://requestly.com/blog/chunked-encoding/
