VKLoader
========
This is a project I made in a couple of hours, since I found a Wallpaper Collection hosted at [VK](http://vk.com), and saw no _Download_ button anywhere.

The code is sluggish, and you'll find I've made horrible use of _if-else_ statements. It's got a bit of OOP and I should probably refactor everything so it is more clean.

**It currently downloads everything to the location wich the executable is located in. Say C:/Pictures/VKLoader.exe then all pictures will be downloaded there.**

Instructions
------------

1. Pass in a [valid VK Album url](http://vk.com/album-22382023_240601069)
2. It will start looking how much images are in the album and retrieve it's information.
..* ![alt text][imgQty]
3. It will start processing each image, getting available resolutions. By default it downloads the image in the highest found resolution. 
4. Download progress will be shown in a progress bar, just wait for it to finish.
5. Profit! 

[imgQty]: https://github.com/klujanrosas/VKLoader/raw/master/Screenshots/1.JPG "Find Images"

