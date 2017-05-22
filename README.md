# XamarinMidiUsbHost
Xamarin Example Code for Usb Host implementation with example on MIDI

This repository contains some code I wrote for a just acquired Casio XW-P1.
This is a multitimbral MIDI keyboard and I wanted to change patches from my Android Phone.
See www.ermers.org/casio_stuff.html for some description.

It turned out that one of the options I have was to implement USB Host code for my phone.
This turned out to be quite a challenge due to the following:
1. My phones don't have USB Host mode
2. My tablets only have Android Jellybean, so no MIDI library
3. I did not want to code in Android Studio / JAVA again

The result of these challenges is this example code. Because I could not find existing code for 
Xamarin with USB Host I decided to upload this, so you can use this for other projects.

So, a disclaimer:
This code does not have all quality components it could have. It is no production code.
There is no MVVM, Xaml, generic MIDI device code, documentation. Abstraction layers are a bit ad-hoc to fill my needs,
depency injection is not following Xamarin guidelines, nor is the .Net code in general. 
No fancy lambda's, a bit of old fashioned coding. Moreover, I will not maintain this code as 
I am moving to my next project.

However, there are three things which are implemented and which you can study:
1. Sharing code over two Xamarin projects with Dependency Injection
2. USB Host implementation for a MIDI device
3. MIDI Raw mode over USB.

Before going into detail, please let me explain the functionality.

The code generates two (almost identical) applications. Both can send patchchanges to the XW-P1.
This is not trivial, since XW-P1 uses SysEx as format for extended Program Changes.
The first application, CasioXPW1Controller, send its data over USB. Therefor a connection between your
Android device and the XW-P1 (Usb Host) is needed.
The second application (AppWK1200) does the same functionally, however it sends it's data over a 
TCP connection (This is what I use for my phone, since it USB-OTG does not work). 
In (my) real life, this TCP connection goes to an Arduino, with an ESP8266 Wireless Ethernet chip attached, 
and this Arduino converts the received data to Serial MIDI data, which is connected from the Arduino to Midi In on the XW-P1.
If you like, I can send or upload the Arduino code as well.

Now to go back to the interesting stuff:

1. Sharing code over two Xamarin projects with Dependency Injection

Aa you can read from the above, both projects do functionally th same, the only difference is USB Host connection or a TCP connection.
Therefor, all code in de directories FormUI and IDeviceConnection is shared over the project.  
Note that I did not use references here (because it's pretty tricky to share the code with a PCL project). I just use "Add As Link".
The IDeviceConnection code implents an interface DeviceConnection which will be instantiated by different classes in both applications.
There is also some DI here, and a lot of code specific for my devices.

2. USB Host implementation for a MIDI device

In CasioXPW1Controller there's the USB Host implementation in CasioXWP1.cs file. Note that most code is 
Android specific and therefor its in the Droid namespace. See the webpage for an explanation.

(Note that the TCP variant is in the AppWK1200 directory, where DeviceConnectionWK1200 inherits from DeviceConnection.
As a final note on that: here all requests are send to the WIFI from a queue, on a pace of 200 msec. This is needed for the slow ESP8266, otherwise it crashes)

3. AS explained on my website the MIDI over USB protocol is different from  the normal protocol. There are some control bytes needed for the USB connection.
This, however is well known and well-documented, but you can see in the code how it works both ways (only for Control Changes, Program Changes and SysEx).

 


 



 
