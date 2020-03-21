\ needs pwm.fs from jcw
\ don't forget to start the systicks, else it will hang 

( start buzzer: ) here hex.
PB1 constant BUZ \ Port for buzzer, must be a pwm port

\ SOUND! this is just a simple square wave 
: tone ( length freq - - ) \ length in ms, freq in Hz 
    buz pwm-init 500 buz pwm ms 0 buz pwm 20 ms
;

\ some demos 
: m1 ( - - ) 
    250 440 tone 125 196 tone 125 196 tone 250 220 tone
    250 196 tone 250 ms  250 247 tone 250 440 tone
;

: m2 ( - - )
    250 523 tone 250 493 tone 250 392 tone 125 523 tone
    125 493 tone 250 329 tone 500 ms 250 523 tone 250 261 tone
    250 392 tone 125 440 tone 125 523 tone
;

: m3 ( - - )
    250 294 tone  250 440 tone 125 349 tone 250 261 tone
    250 294 tone 250 440 tone 250 294 tone 250 261 tone
    250 349 tone 250 294 tone 250 440 tone 250 261 tone
    250 294 tone 250 440 tone 125 349 tone
;

( end buzzer: ) here hex.

