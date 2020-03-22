# MecrispForth_LEDClock
Clock based on multiplexed Leds, also a dht11 sensor and buttons. 
How it looks: https://youtu.be/bIr0jlOod5I

- hardware will be documented sooon

- used external forth words are all from Jean-Claude Wippler and
  can be found there: https://git.jeelabs.org/jcw/embello/src/branch/master/explore/1608-forth
 
- folie the console and upload programm for Forth from Jean-Claude Wippler can be found there: https://git.jeelabs.org/jcw/folie
  
- mecrisp stellaris from Matthias Koch : https://sourceforge.net/projects/mecrisp/files/

my sourcecode needs a real cleanup, but works. (with some bugs)

Install:
- programm a stm32f103c8 (blue pill will work) with mecrisp stellaris (2.5.3 is tested)

- connect with folie

- install board.fs
  + compiletoflash
  + !s board.fs
  + compiletoram

- after a reset Boot1 jumper defines now the console, if set to 0 you get USB-console, if set to 1 you get serial console

- install Clock.fs
  + compiletoflash
  + !s Clock.fs
  + compiletoram
  
- Clock can imediately started via word 'StartClock'
- time can be set via word now! from rtc.fs (jcw see there)

If clock works as expected you can put it to init for autostart. As you already have seen, command line is still usable even while running the clock, thanks to multi.fs and timed.fs from jcw.

- activate autostart
  + compiletoflash
  + cornerstone \<\<\<Clock\>\>\>
  + here hex. (note this address, in case something goes wrong you can erase flash from here and switch off autostart this way)
  + : init init StartClock ;
  + compiletoram
  
(do not rebuild this clock, because of very old LED and TTL chips it takes around 5W. It is over complicated to program. Take a look at my Forth WS8212 Clock, much easier to build and programm.) 
