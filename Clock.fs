\ LEDClock in Forth with buffer
\ Sven Muehlberg

( start Clock: ) here dup hex.

16 buffer: dispBuffer

\ config area for modules, just comment out if you don't have that hardware

: mqtt ; \ defines mqtt output, comment out if not needed

\ use DHT11 module
include dht11.fs

\ use buttons for some actions
include button.fs

\ use buzzer for sound, alarm not implemented now
include ../MecrispForth_buzzer/buzzer.fs
include ../MecrispForth_buzzer/notes.fs
include ../MecrispForth_buzzer/odeandiefreude.fs
include ../MecrispForth_buzzer/forelise.fs
include ../MecrispForth_buzzer/impmarch.fs


\ time conversion - not optional
include time.fs

\ definition NVIC
$E000E100 constant NVIC_ISER0

\ definition Ports 
PB0 constant DP \ PB8-PB15
\ need config for which pins the data port is starting
\ 8 constant DP.pin \ not in use now
DP io-base GPIO.BSRR + constant DP.BSRR
DP io-base GPIO.BRR + constant DP.BRR
PA15 constant A0 \ 5V 
PA8 constant A1 \ 5V
PB6 constant A2 \ 5V
PB7 constant WR \ 5V
PB4 constant ClockLED \ 5V

7200 VARIABLE TIMEZONE \ 2h in front

2019 VARIABLE YEAR
05 VARIABLE MONTH
20 VARIABLE DAY
0 VARIABLE WDAY
23 VARIABLE HOUR
45 VARIABLE MINUTE
50 VARIABLE SECOND

0 VARIABLE DIMMstat
0 VARIABLE BEEP

include asci_7segment.fs

\ define days - german names :)
: days s" SonntaMontagDienstMittwoDonnerFreitaSamsta" ;

: day.
    WDAY @ 6 *
    days drop +
    dispbuffer 8 +
    6 move
;

: breaktime ( - - )
    now timezone @ + dup
    time HOUR ! MINUTE ! SECOND !
    date DAY ! MONTH ! YEAR ! WDAY !

    dispBuffer 15 + c@ \ visibility flags

    dup $08 and 0= if 
	ClockLED iox! \ blink
    then

    dup $01 and 0= if
    SECOND @ dispBuffer tuck c!
	minute @ swap 1+ tuck c!
	hour @ 5 * minute @ 12 / + dup 60 mod rot 1+ tuck c! 
	swap 1+ 60 mod swap 1+ c!
    then

    dup $02 and 0= if
	HOUR @ 10 /mod $30 + dispBuffer 4 + tuck c!
	swap $30 + over 1+ c!
	MINUTE @ 10 /mod $30 + rot 2+ tuck c!
	swap $30 + swap 1+ c!
    then

    $04 and 0= if
	SECOND @ 3 / 4 mod case
	    0 of temp. endof
	    1 of humi. endof
	    2 of day.  endof
	    DAY @ 10 /mod $30 + dispBuffer 8 + tuck c!
	    swap $B0 + over 1+ c!
	    MONTH @ 10 /mod $30 + rot 2+ tuck c!
	    swap $B0 + over 1+ c!
	    YEAR @ 100 mod 10 /mod $30 + rot 2+ tuck c!
	    swap $30 + swap 1+ c!
	endcase
    then

    \ moved to display. since using rtc irq for seconds conflicts with timer irq
    [ifdef] tone
	dispbuffer 15 + @ $10 and 0= if
	    minute @ 0= second @ 0= and if
		\ 360 440 tone
		1 beep !
	    then
	    minute @ 59 - 0=
	    second @ 57 - 0< not and if
		\ 80 1760 tone
		2 beep !
	    then
	then
    [then]
;

: clrClock.

omode-od DP $ff00 io-modes! \ PB8 - PB15 als output

$ff00 DP.BSRR ! \ all data high

omode-od A0 io-mode!
omode-od A1 io-mode!
omode-od A2 io-mode!

omode-od WR io-mode!

omode-pp ClockLED io-mode!

wr ios!
A0 ioc! A1 ioc! A2 ioc!
WR dup ioc! ios!
A0 ios!
WR dup ioc! ios!
a1 ios!
WR dup ioc! ios!
A1 ioc! A2 ios!
WR dup ioc! ios!

ClockLED ios!
;



: display. ( - - )
dispBuffer 14 + c@ \ get actual position
dup 12 u>= if drop 0 then \ set 0 if 12 or more

\ display ring
dup 4 mod

WR ios!
$ff00 DP.BSRR ! \ all data high
A0 ios! A1 ioc! A2 ioc! \ Addr $01
WR dup ioc! ios!

A0 ioc! \ Addr $00

dispBuffer + c@
1 over 3 rshift lshift \ select ring sector
8 lshift DP.BRR ! \ set port bits zero 
10 us
WR ioc! 10 us WR ios!

$ff00 DP.BSRR ! \ all data high
A0 ios! \ Addr $01

$07 and 1 swap lshift \ select pixel
8 lshift DP.BRR ! 
10 us
WR ioc! 10 us WR ios! 

\ DIMM @ 0<> if
\  $ff00 DP.BSRR ! \ all data high
\  wr ios!
\  A0 ios! A1 ioc! A2 ioc! \ 001 Pixels
\  WR dup ioc! ios!
\ then

\ Display hours
dup 4 mod
$ff00 DP.BSRR ! \ all data high (dark)
A0 ios! A1 ios! A2 ioc! \ 011 addr day segmente
1 us
WR dup ioc! ios!

A0 ioc! \ 100 addr day digits
dup $01 swap lshift
8 lshift DP.BRR ! 
1 us
WR dup ioc! ios!

$ff00 DP.BSRR ! \ all data high
A0 ios!
dispBuffer + 4 + c@ \ hole Inhalt aus display Puffer
ascii7seg + $20 - c@ \ wandle ascii
8 lshift DP.BRR !
1 us
WR dup ioc! ios!


\ Display days
dup 6 mod
$ff00 DP.BSRR ! \ all data high (dark)
A0 ios! A1 ioc! A2 ios! \ 100 addr day segmente
1 us
WR dup ioc! ios!

A0 ioc! \ 101 addr day digits
dup $01 swap lshift
8 lshift DP.BRR ! 
1 us
WR dup ioc! ios!

$ff00 DP.BSRR ! \ all data high
A0 ios!
dispBuffer + 8 + c@ \ hole Inhalt aus ddisplay Puffer
dup $7F and
ascii7seg + $20 - c@ \ wandle ascii
swap $80 and or
8 lshift DP.BRR !
1 us
WR dup ioc! ios!
10 us

\ increment displaypos 
1+ dispBuffer 14 + c!

\ darken if dimmstat
DIMMstat @ 0<> if
  $ff00 DP.BSRR ! \ all data high
  wr ios!
  A0 ios! A1 ioc! A2 ioc! \ 001 Pixels
  WR dup ioc! ios!
  10 us
  A1 ios!
  WR dup ioc! ios!
  A1 ioc! A2 ios!
  WR dup ioc! ios!
    ClockLED ios!
else
    \ beep only if not dimmstat
    beep @
    case
	1 of
	    360 440 tone
	    0 beep !
	endof
	2 of
	    80 1760 tone
	    0 beep !
	endof
    endcase
then

;


: rtc_isr
    1 rtc-crl bit@ not if \ check if new second
	exit
    then
    1 rtc-crl bic! \ reset second flag

    breaktime
;

: rtc_isr_init
    rtc-init
    500 ms
    ['] rtc_isr irq-rtc ! \ write my isr to irq vector tab
    3 bit nvic_iser0 bis! \ enable irq vector
    500 ms
    0 bit rtc bis!        \ enable second irq in rtc-crh
;



: resume
    0 dispbuffer 15 + c! 
    0 DIMMstat !
    clrClock.
    \ ['] breaktime 1000 0 call-every
    ['] display. 1 1 call-every
    [ifdef] readDHT11
	['] readDHT11 60000 2 call-every
    [then]
    [ifdef] BTinit
	['] BTpoll 100 3 call-every
    [then]
;

: dimm
    1 DIMMstat !
    \ ['] breaktime 1000 0 call-every
    ['] display. 5 1 call-every
;

: suspend
    $0f dispbuffer 15 + c!
    \ 0 call-never
    1 call-never
    clrClock.
;

\ [ifdef] mqtt
\    : dimm dimm ." POWER2;false" CR ;
\    : resume resume ." POWER2;true" CR ;
\ [then]

[ifdef] BTinit
    \ should be called via button press
    : dimmer
	drop \ throw away the button number

	DIMMstat @ 0= if \ check if bright
	    dimm
	else
	    resume
	then
    ;
[then]

[ifdef] mqtt
    : bueropower drop ." >mqtt:cmnd/Buero/Power,toggle" CR ;
    
    : shutter
	3 swap - \ reverse buttons 2-4
	50 *
	." >mqtt:cmnd/Buero_Rollo/ShutterPosition," 0 <# #s #> type CR
    ;
    
[then]




: StartClock
    CR ." Starting Clock v2.0"
    \ rtc-init
    rtc_isr_init \ rtc-init and second irq
    imode-float pc13 io-mode! \ LED pin imode-float, da sonst rtc beeinflusst
    depth 0= if CR ." Keeping time" else
	now!
    then
    CR

    dispBuffer 4 61 fill
    s" /[]\Hello." dispbuffer 4 + swap move
    0 dispBuffer 14 + c!
    4 dispBuffer 15 + c!

    \ setup

    ClockLED ioc!


    timed-init

    [ifdef] BTinit
	BTinit
	\    ['] dimmer BTv 2 cells + ! \ dimmer on button 2
	[ifdef] mqtt
	    
	    ['] bueropower BTv 0 cells + ! \ PC on/off
	    4 1 do \ starting with second button shutter 3 buttons for shutter
		['] shutter BTv I cells + ! \ initialising vector with default
	    loop

	[then]
    [then]

    resume
    [ifdef] readDHT11
	readDHT11
    [then]

    [ifdef] m1
	m1 play \ starting sound, if buzzer is loaded
    [then]

    1000 ms
    0 dispBuffer 15 + c!
;

[ifdef] m1
    : alarm
	4 dispbuffer 15 + c!
	s" Alarm!" dispbuffer 8 + swap move
	m1 play
	0 dispbuffer 15 + c!
    ;
[then]
[ifdef] m2
    : alarm1
	4 dispbuffer 15 + c!
	s" ALARM!" dispbuffer 8 + swap move
	m2 play
	0 dispbuffer 15 + c!
    ;
[then]
[ifdef] impmarch
    : panik
	4 dispbuffer 15 + c!
	s" Panik!" dispbuffer 8 + swap move
	impmarch play
	0 dispbuffer 15 + c!
    ;
[then]
[ifdef] ode
    : freude
	4 dispbuffer 15 + c!
	s" Freude" dispbuffer 8 + swap move
	ode play
	0 dispbuffer 15 + c!
    ;
[then]
[ifdef] ForElise
    : elise
	4 dispbuffer 15 + c!
	s" Elise." dispbuffer 8 + swap move
	ForElise play
	0 dispbuffer 15 + c!
    ;
[then]

( end Clock: ) here dup hex.
( size: ) swap - hex.



