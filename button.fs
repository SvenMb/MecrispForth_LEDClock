PA0 constant BUTTON0
PA1 constant BUTTON1
PA2 constant BUTTON2

imode-float BUTTON0 io-mode!
imode-float BUTTON1 io-mode!
imode-float BUTTON2 io-mode!

0 variable KBDWAIT

: pollkbd
  kbdwait @ 0= if
  	  BUTTON0 io@ if ." POWER;true" CR 5 kbdwait ! then
	  BUTTON1 io@ if ." POWER1;true" CR 5 kbdwait ! then
	  BUTTON2 io@ if ." POWER2;true" CR 5 kbdwait ! then
  else
	  kbdwait dup @ 1- swap !
  then ; 

\ ['] pollkbd 100 3 call-every
