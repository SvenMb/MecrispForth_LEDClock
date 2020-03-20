PB3 constant DHT11

6 buffer: dht11data

: readDHT11

0 dht11data 5 + c! \ reset error

omode-pp DHT11 io-mode!
DHT11 ioc!
18 ms
DHT11 ios!
IMODE-FLOAT DHT11 io-mode!
40 us

DHT11 io@
 if
	1 dht11data 5 + c! \ errorcode 1
	quit
 then
 
80 us 
DHT11 io@
 not if
	2 dht11data 5 + c! \ errorcode 2
	quit
 then
 
80 us

5 0 do
	0
	8 0 do
		begin DHT11 io@ 0<> until
		30 us
		dht11 io@ 0<> if 1 else 0 then 7 I - lshift or
		1000 0 do DHT11 io@ 0= 
			if leave then
			I 999 =
			if 3 dht11data 5 + c! then \ errorcode 3
		loop
	loop
	dht11data I + c!
loop

0
4 0 do
dht11data I + c@ +
loop
dht11data 4 + c@ <>
if 4 dht11data 5 + c! then

\ ab hier ausgabe fuer mqtt

dht11data 5 + c@
0<>
if 
	." dht11;DHT Error# " dht11data 5 + c@ . CR 
	quit
then

\ SENSOR = {"Time":"2020-02-22T11:20:23","DHT11":{"Temperature":18.2,"Humidity":42.0},"TempUnit":"C"}

." SENSOR;{"
$22 emit ." DHT11" $22 emit $3a emit $7b emit
$22 emit ." Temperature" $22 emit $3a emit

dht11data 2+ c@
dup $80 and
if $2d emit then
$7f and
10 /mod
.digit emit .digit emit 
$2e emit 
dht11data 3 + c@ 10 mod .digit emit $2c emit

$22 emit ." Humidity" $22 emit $3a emit

dht11data c@
10 /mod
.digit emit .digit emit 
$2e emit 
dht11data 1+ c@ 10 mod .digit emit
$7d emit $2c emit $22 emit ." TempUnit" $22 emit $3a emit $22 emit $43 emit $22 emit $7d emit CR
;

: temp.
dht11data 5 + c@
0<>
if 
	s" DHT E" dispbuffer 8 + swap move
	dht11data 5 + c@ 10 mod .digit dispbuffer 13 + c! 
else

dht11data 2+ c@
dup $80 and
if char -  else $20 then
dispbuffer 8 + c!

$7f and
10 /mod
.digit dispbuffer 9 + c!
.digit 128 + dispbuffer 10 + c!
dht11data 3 + c@ 10 mod
.digit dispbuffer 11 + c!
127 dispbuffer 12 + c!
$43 dispbuffer 13 + c!
then
;

: humi.
dht11data 5 + c@
0<>
if 
	s" DHT E" dispbuffer 8 + swap move
	dht11data 5 + c@ 10 mod .digit dispbuffer 13 + c! 
else

$20 dispbuffer 8 + c!

dht11data c@
10 /mod
.digit dispbuffer 9 + c!
.digit 128 + dispbuffer 10 + c!
dht11data 1 + c@ 10 mod
.digit dispbuffer 11 + c!
$20 dispbuffer 12 + c!
$48 dispbuffer 13 + c!
then
;

: DHT11.
dht11data 5 + c@
0<>
if 
	CR ." DHT Error# " dht11data 5 + c@ . 
	quit
then

dht11data 2+ c@
dup $80 and
if ." -" then
$7f and
CR 10 /mod
.digit emit .digit emit 
$2c emit 
dht11data 3 + c@ 10 mod .
." °C"

dht11data c@
CR 10 /mod
.digit emit .digit emit 
$2c emit 
dht11data 1+ c@ 10 mod .
." % Feuchtigkeit"

;

