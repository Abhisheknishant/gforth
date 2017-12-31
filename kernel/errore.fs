\ ERRORE.FS English error strings                      9may93jaw

\ Copyright (C) 1995,1996,1997,1998,1999,2000,2003,2006,2007,2012,2014,2015,2016,2017 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.


\ The errors are defined by a linked list, for easy adding
\ and deleting. Speed is not neccassary at this point.

require ./io.fs
require ./nio.fs

AVariable ErrLink              \ Linked list entry point
NIL ErrLink !

decimal

\ error numbers between -256 and -511 represent signals
\ signals are handled with strsignal
\ but some signals produce throw-codes > -256, e.g., -28

\ error numbers between -512 and -2047 are for OS errors and are
\ handled with strerror

: c(warning") ( c-addr -- )
    count true ['] type ?warning ;

has? OS [IF]
: >stderr ( -- )
    r> op-vector @ >r debug-vector @ op-vector !
    >exec  r> op-vector ! ;

: do-debug ( xt -- )
    op-vector @ >r debug-vector @ op-vector !
    catch  r> op-vector !  throw ;
[THEN]

: error$ ( n -- addr u ) \ gforth
    \G converts an error to a string
    ErrLink
    BEGIN @ dup
    WHILE
	2dup cell+ @ =
	IF
	    2 cells + count rot drop EXIT THEN
    REPEAT
    drop
[ has? os [IF] ]
    dup -511 -255 within
    IF
	256 + negate strsignal EXIT
    THEN
    dup -2047 -511 within
    IF
	512 + negate strerror EXIT
    THEN
[ [THEN] ]
    base @ >r decimal
    s>d tuck dabs <# #s rot sign s" error #" holds #> r> base ! ;

has? OS [IF]
    $6600 Value default-color
    $E600 Value err-color
    $B600 Value warn-color
    $D600 Value info-color
    : white-colors ( -- )
	\G for white background
	$6600 to default-color
	$E600 to err-color
	$B600 to warn-color
	$D600 to info-color ;
    : black-colors ( -- )
	\G for black background
	$6600 to default-color
	$C601 to err-color
	$9601 to warn-color
	$D601 to info-color ;
[THEN]

: .error ( n -- )
[ has? OS [IF] ]
    >stderr
[ [THEN] ]
    error$ type ;