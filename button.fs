( start button: ) here hex.

\ config area for button ports
4 constant BT# \ how many buttons

\ which ports
create BTp
PA0 ,
PA1 ,
PA2 ,
PA3
,

\ vectorlist for button words 
BT# cells buffer: BTv

\ default word for button
: BThello
    [ifdef] mqtt
	." POWER" dup 0<> if 0 <# #s #> type then ." ;true" CR	
    [else]
	." Hey! #" . CR
    [then]
;

0 variable BTwait

: BTpoll
    BTwait @ 0<> if
	BTwait dup @ abs 1- swap !
	exit
    then
    BT# 0 do
	BTp I cells + @ io@ if
	    I BTv I cells + @ execute \ start words with button# on stack
	    \ I . CR \debug
	    5 BTwait !
	    leave
	then
    loop
;

: BTinit
    BT# 0 do
	imode-float BTp I cells + @ io-mode! \ maybe imode-float not always good?
	['] BThello BTv I cells + ! \ initialising vector with default
    loop

;

\ check every 100ms and use timed 3
\ BTinit
\ ['] BTpoll 100 3 call-every \ modify this to your needs



( end button: ) here hex.
