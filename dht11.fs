include ../MecrispForth_DHT11/dht11.fs

0 variable temp
0 variable humi \ negative values indicate error


\ regularly to call via timed
: readDHT11 ( - - )

    DHT11@
    dup humi ! \ save humidty or error
    0< if \ negative value indicates error
	[ifdef] mqtt
	    ." dht11;DHT Error# " humi @ . CR
	    quit
	[then]
    else
	temp !
    then

    \ ab hier ausgabe fuer mqtt
    \ SENSOR = {"Time":"2020-02-22T11:20:23","DHT11":{"Temperature":18.2,"Humidity":42.0},"TempUnit":"C"}
    [ifdef] mqtt
	." SENSOR;{"
	$22 emit ." DHT11" $22 emit $3a emit $7b emit
	$22 emit ." Temperature" $22 emit $3a emit
	
	temp @
	s>d swap over dabs <# # $2E hold #S rot sign #> type

	$2c emit $22 emit ." Humidity" $22 emit $3a emit
	
	humi @
	0 <# # $2E hold #S #> type

	$7d emit $2c emit $22 emit ." TempUnit" $22 emit $3a emit $22 emit $43 emit $22 emit $7d emit CR
    [then]
;

\ prints temperature into dispbuffer
: temp. ( - - )
    humi @
    0<
    if 
	s" DHT E" dispbuffer 8 + swap move
	humi @ .digit dispbuffer 13 + c! 
    else
	
	temp @
	dup 0< if \ check for negative value
	    char -  else $20
	then
	
	dispbuffer 8 + c! 

	abs
	10 /mod
	10 /mod
	.digit dispbuffer 9 + c! \ zehner
	.digit 128 + dispbuffer 10 + c! \ einer und Punkt 
	.digit dispbuffer 11 + c! \ zehntel
	127 dispbuffer 12 + c! \ Grad Zeichen im modif ASCII
	$43 dispbuffer 13 + c! \ C
    then
;

\ prints humidity into dispbuffer
: humi. ( - - )
    humi @
    dup 0<
    if 
	s" DHT E" dispbuffer 8 + swap move
	.digit dispbuffer 13 + c! 
    else
	
	$20 dispbuffer 8 + c! \ leer

	10 /mod
	10 /mod
	.digit dispbuffer 9 + c! \ zehner
	.digit 128 + dispbuffer 10 + c! \ einer
	.digit dispbuffer 11 + c! \ zehntel
	$20 dispbuffer 12 + c! \ leer
	$48 dispbuffer 13 + c! \ H
    then
;
