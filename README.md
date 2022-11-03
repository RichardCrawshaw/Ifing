# Ifing
A Windows-only C#.NET project to display multiple video streams from local webcams on screen.

# Purpose
This is another model railway related project.
There are a number of areas where the track is obscured, but it's desirable to be able to monitor them.
I have a couple of laptops that will be running other aspects of the layout and was able to source a 
number of webcams relatively cheaply, so this made more sense than a security camera approach.

# Credit
The credit for the webcam and display code must be given to Chun lin.
The majority of the code was taken from https://github.com/goh-chunlin/WebcamWinForm
and the blog post https://cuteprogramming.wordpress.com/2020/12/12/look-at-me-webcam-recording-with-net-5-winforms/
All I have done is modified it to support multiple concurrent video streams.
For this project I'm not interested in recording the streams so that part has been removed.
