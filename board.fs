( board start: ) here dup hex.
include jcw/flib/mecrisp/calltrace.fs
( calltrace end: ) here dup hex.
include jcw/flib/mecrisp/cond.fs
( cond end: ) here dup hex.
include jcw/flib/mecrisp/hexdump.fs
( hexdump end: ) here dup hex.
include jcw/flib/stm32f1/io.fs
( io end: ) here dup hex.
include jcw/flib/pkg/pins48.fs
( pins48 end: ) here dup hex.

3 constant io-ports
include jcw/flib/stm32f1/hal.fs
: flash-kb ( -- u ) 128 ; \ patched to 128kb
( hal end: ) here dup hex.

include jcw/flib/stm32f1/rtc.fs
( rtc end: ) here dup hex.
include jcw/flib/stm32f1/timer.fs
( timer end: ) here dup hex.
include jcw/flib/stm32f1/pwm.fs
( pwm end: ) here dup hex.
include jcw/flib/mecrisp/multi.fs
( multi end: ) here dup hex.
include jcw/flib/any/timed.fs
( timed end: ) here dup hex.
include usb-f1.fs

: hello ( -- ) flash-kb . ." KB <STM32F103> " hwid hex.
  $20000 compiletoflash here -  flashvar-here compiletoram here -
  ." ram/flash: " . . ." free " ;

: init ( -- )
    ['] ct-irq irq-fault !  \ show call trace in unhandled exceptions
    jtag-deinit  \ disable JTAG, we only need SWD
    72MHz \ faster :)
    1000 systick-hz \ enable systicks
    imode-float PB2 io-mode!
    PB2 io@
    0= if
	." con@USB" CR
	+usb
    else
	." con@SER" CR
    then
;    

: cornerstone ( "name" -- )  \ define a flash memory cornerstone
  <builds begin here dup flash-pagesize 1- and while 0 h, repeat
  does>   begin dup  dup flash-pagesize 1- and while 2+   repeat  cr
  eraseflashfrom ;

Cornerstone <<<usb>>>
( board end: ) here dup hex.
