\ lib for time conversions
\ you have to take care of timezones yourself

\ calculates time from timestamp (seconds since 1.1.1970)
\ maybe it would be usefull to switch to leapoch timestamp?

( start time: ) here hex.

: time ( timestamp - hour minutes seconds )
    86400 mod \ throw away the date part
    \ take care of dates before 1970

    dup 0< if
	86400 +
    then
    
    60 /mod
    60 /mod 
;

CREATE MONTHLEN
31 , 30 , 31 , 30 , 31 , 31 , 30 , 31 , 30 , 31 , 31 , 29
,

\ calculates date from timestamp
: date ( timestamp - year month mday wday )

    \ leapoch on 1.3.2000
    951868800 - \ 946684800 86400 60 * + -

    86400 / \ throw away the time part, make it days
    dup 0< if \ take care of dates before 1.3.2000
	1-
    then
    

    dup 3 + 7 mod \ weekday 0 - sunday 1 - monday ...
    dup 0< if \ take care of negative dates
	7 +
    then
    swap \ put days since leapoch in front again

    1461 /mod  \ 4 year incl leap day \ 365 4 * 1 +
    over 0< if \ take car of dates before leapoch
	1- swap 1461 + 
    else
	swap
    then
    365 /mod \ years since last leap
    dup 4 = if 1- then \ if leap day
    rot 4 * + \ years since 1.3.2000
    2000 +
    over 306 > if 1+ then \ add 1 year after 31.12.
    swap \ put days since last 1.3. in front again

    12 0 do
	dup
	monthlen I cells + @
	tuck
	< if
	    drop
	    i 2+ 12 mod 1+ \ correct month
	    leave
	then
	-
    loop
    swap \ put days in month on top
    
    1+ \ days should start with 1
;

\ examples
\ prints time in hh:mm:ss 
: time. ( hour minutes seconds - - )
    0 <# $3a hold # # #> type
    0 <# $3a hold # # #> type
    0 <# # # #> type
;

\ prints date in dd.mm.yyyy
: date. ( year month mday wday -  - )
    0 <# $2e hold # # #> type
    0 <# $2e hold # # #> type
    0 <# # # # # #> type
    drop
;

( end time: ) here hex.

