\ LEDClock in Forth with buffer
\ Sven Muehlberg

16 buffer: dispBuffer

\ config area for modules, just comment out if you don"t have that hardware
: mqtt ; \ defines mqtt output, comment out if not needed

include dht11.fs

include button.fs

include buzzer.fs


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

7200 VARIABLE TIMEZONE 

2019 VARIABLE YEAR
05 VARIABLE MONTH
20 VARIABLE DAY
23 VARIABLE HOUR
45 VARIABLE MINUTE
50 VARIABLE SECOND

0 VARIABLE DIMM

CREATE MONTHLENGTHS
31 , 28 , 31 , 30 , 31 , 30 , 31 , 31 , 30, 31 , 30 , 31
,

CREATE MONTHLENGTHSLEAP
31 , 29 , 31 , 30 , 31 , 30 , 31 , 31 , 30, 31 , 30 , 31
,

0 VARIABLE MLENGTHS


include asci_7segment.fs



: breaktime
now
timezone @ +
60 /mod swap SECOND !
60 /mod swap MINUTE !
24 /mod swap HOUR !
365 +
1461 /mod 4 * swap 1095 /mod 3 * swap 365 /mod rot + rot + 1969 + dup YEAR !
4 mod 0= if MONTHLENGTHSLEAP else MONTHLENGTHS then MLENGTHS !
12 0 DO
 dup 
 MLENGTHS @ I CELLS + @ < if i 1+ MONTH ! leave then MLENGTHS @ I CELLS + @ -
loop
1+ DAY !

dispBuffer 15 + c@

dup $08 and 0= if
    ClockLED iox!
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
		DAY @ 10 /mod $30 + dispBuffer 8 + tuck c!
		swap $B0 + over 1+ c!
		MONTH @ 10 /mod $30 + rot 2+ tuck c!
		swap $B0 + over 1+ c!
		YEAR @ 100 mod 10 /mod $30 + rot 2+ tuck c!
		swap $30 + swap 1+ c!
	endcase
then
;

: time.
CR
HOUR @ u.2 $3a emit MINUTE @ u.2 $3a emit SECOND @ u.2
CR
DAY @ u.2 $2e emit MONTH @ u.2  $2e emit YEAR @ .
CR
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



: display.
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
dispBuffer + 4 + c@ \ hole Inhalt aus ddisplay Puffer
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

DIMM @ 0<> if
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
then
;

: resume
    0 dispbuffer 15 + c! 
    0 DIMM !
    timed-init
    clrClock.
    ['] breaktime 1000 0 call-every
    ['] display. 1 1 call-every
    [ifdef] readDHT11
	readDHT11
	['] readDHT11 60000 2 call-every
    [then]
    [ifdef] pollkbd
	['] pollkbd 100 3 call-every
    [then]
;

: StartClock
CR ." Starting Clock v2.0"
rtc-init
imode-float pc13 io-mode! \ LED pin imode-float, da sonst rtc beeinflusst
depth 0= if CR ." Keeping time" else
  now!
then

dispBuffer 4 61 fill
s" /[]\Hello." dispbuffer 4 + swap move
0 dispBuffer 14 + c!
4 dispBuffer 15 + c!

\ setup

ClockLED ioc!

resume

[ifdef] m1
    m1 \ starting sound, if buzzer is loaded
[then]

1000 ms
0 dispBuffer 15 + c!

time.
    
;

: suspend
$0f dispbuffer 15 + c!
0 call-never
1 call-never
clrClock.
;

: dimm
  1 DIMM !
  ['] breaktime 1000 0 call-every
  ['] display. 5 1 call-every
;

: dimm $3b emit CR dimm ." POWER2;false" CR $3b emit ;
: resume $3b emit CR resume ." POWER2;true" CR $3b emit ;




