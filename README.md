# MiniRadio
An MP3 internet radio server built purely out of sockets, no external libraries. 

To start it, enter the IP you want to bind (your own IP, but in some networks there's more than one of those), the port you want to bind for hosting, then enter a path to a folder containing album subfolders that contain MP3s, it'll create a /album path for each. Browse to the endpoint to stream the album.

For example: localhost:8001/Crisis to listen to the album titled Crisis

Capitalization matters, I plan to fix it.
Tabstop is a mess, also next thing fixed.
