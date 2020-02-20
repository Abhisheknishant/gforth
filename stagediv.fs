\ 2-stage division and optimizing division by constants

\ Authors: Anton Ertl
\ Copyright (C) 2020 Free Software Foundation, Inc.

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

0
field: staged/-inverse \ for staged u/: the low cell of the inverse
field: staged/-shift   \ for staged /f
' staged/-shift alias staged/-inverse-hi \ high cell of the inverse for u/
field: staged/-divisor \ for computing the modulus
\ field: staged/-offset  \ the b value of Robison's algorithms
constant staged/-size ( -- u )
\g size of buffer for @code{u/-stage1m} or @code{/f-stage1m}.


\ unsigned division

\ We use the method described below, because it leads to shorter
\ dependence chains (at least if compiled optimally) than Robison's
\ algorithm 1.

\ @InProceedings{ertl19kps,
\   author =       {M. Anton Ertl},
\   title =        {Integer Division by Multiplying with the
\                   Double-Width Reciprocal},
\   crossref =     {kps19},
\   pages =        {75--84},
\   url =          {http://www.complang.tuwien.ac.at/papers/ertl19kps.pdf},
\   url-slides =   {http://www.complang.tuwien.ac.at/papers/ertl19kps-slides.pdf},
\   abstract =     {Earlier work on integer division by multiplying with
\                   the reciprocal has focused on multiplying with a
\                   single-width reciprocal, combined with a correction
\                   and followed by a shift.  The present work explores
\                   using a double-width reciprocal to allow getting rid
\                   of the correction and shift.}
\ }
\ 
\ @Proceedings{kps19,
\   title =        {20. Kolloquium Programmiersprachen und Grundlagen
\                   der Programmierung (KPS)},
\   booktitle =    {20. Kolloquium Programmiersprachen und Grundlagen
\                   der Programmierung (KPS)},
\   year =         {2019},
\   key =          {kps19},
\   editor =       {Martin Pl\"umicke and Fayez Abu Alia},
\   url =          {https://www.hb.dhbw-stuttgart.de/kps2019/kps2019_Tagungsband.pdf}
\ }

0 [if]
    \ commented out because it is a primitive
: u/-stage2m {: udividend addr -- uquotient :}
    udividend addr staged/-inverse @ um* nip 0
    udividend addr staged/-inverse-hi @ um* d+ nip ;

: umod-stage2m {: udividend addr -- umodulus :}
    udividend dup addr u/-stage2m addr staged/-divisor @ * - ;

: u/mod-stage2m {: udividend addr -- umodulus uquotient :}
    udividend addr u/-stage2m udividend over addr staged/-divisor @ * - swap ;
[then]

: u/-stage1m {: udivisor addr -- :}
    udivisor 2 u< -24 and throw
    udivisor addr staged/-divisor !
    0 1 udivisor um/mod addr staged/-inverse-hi !
    udivisor 1- swap udivisor um/mod addr staged/-inverse ! drop ;

\ floored division

\ a mixture of Robison's algorithms 1 and 3.

\ @InProceedings{robison05,
\   author =	"Arch D. Robison",
\   title =	"{$N$}-Bit Unsigned Division Via {$N$}-Bit
\ 		 Multiply-Add",
\   OPTeditor =	"Paolo Montuschi and Eric (Eric Mark) Schwarz",
\   booktitle =	"{Proceedings of the 17th IEEE Symposium on Computer
\ 		 Arithmetic (ARITH-17)}",
\   publisher =	"IEEE Computer Society Press",
\   ISBN = 	"0-7695-2366-8",
\   ISBN-13 =	"978-0-7695-2366-8",
\   year = 	"2005",
\   bibdate =	"Wed Jun 22 07:02:55 2005",
\   bibsource =	"http://www.math.utah.edu/pub/tex/bib/fparith.bib",
\   URL =  	"http://arith17.polito.it/final/paper-104.pdf",
\   URL =       "http://www.acsel-lab.com/arithmetic/arith17/papers/ARITH17_Robison.pdf"
\   abstract =	"Integer division on modern processors is expensive
\ 		 compared to multiplication. Previous algorithms for
\ 		 performing unsigned division by an invariant divisor,
\ 		 via reciprocal approximation, suffer in the worst case
\ 		 from a common requirement for $ n + 1 $ bit
\ 		 multiplication, which typically must be synthesized
\ 		 from $n$-bit multiplication and extra arithmetic
\ 		 operations. This paper presents, and proves, a hybrid
\ 		 of previous algorithms that replaces $ n + 1 $ bit
\ 		 multiplication with a single fused multiply-add
\ 		 operation on $n$-bit operands, thus reducing any
\ 		 $n$-bit unsigned division to the upper $n$ bits of a
\ 		 multiply-add, followed by a single right shift. An
\ 		 additional benefit is that the prerequisite
\ 		 calculations are simple and fast. On the Itanium 2
\ 		 processor, the technique is advantageous for as few as
\ 		 two quotients that share a common run-time divisor.",
\   acknowledgement = "Nelson H. F. Beebe, University of Utah, Department
\ 		 of Mathematics, 110 LCB, 155 S 1400 E RM 233, Salt Lake
\ 		 City, UT 84112-0090, USA, Tel: +1 801 581 5254, FAX: +1
\ 		 801 581 4148, e-mail: \path|beebe@math.utah.edu|,
\ 		 \path|beebe@acm.org|, \path|beebe@computer.org|
\ 		 (Internet), URL:
\ 		 \path|http://www.math.utah.edu/~beebe/|",
\   keywords =	"ARITH-17",
\   pagecount =	"9",
\ }

: log2 ( x -- n )
    \ integer binary logarithm
    -1 swap begin
	dup while
	    1 rshift 1 under+ repeat
    drop ;

: pow2? ( u -- f )
    dup dup 1- and 0= and 0<> ;

: ctz ( x -- u )
    \g count trailing zeros in binary representation of x
    \ !! slow implementation
    dup 0= if
	drop 8 cells exit then
    0 swap begin
	dup 1 and 0= while
	    1 rshift 1 under+ repeat
    drop ;


0 [if]
    \ these are now primitives
: /f-stage2m {: ndividend addr -- :}
    ndividend addr staged/-inverse @ tuck m* rot 1 rshift 0 d+
    nip ndividend + addr staged/-shift @ arshift ;

: modf-stage2m {: ndividend addr -- :}
    ndividend dup addr /f-stage2m addr staged/-divisor @ * - ;

: /modf-stage2m {: ndividend addr -- :}
    ndividend addr /f-stage2m ndividend over addr staged/-divisor @ * - swap ;
[then]

: /f-stage1m {: ndivisor addr -- :}
    ndivisor 1 < -24 and throw
    ndivisor log2 {: um :}
    ndivisor pow2? if
        -1
    else
        ndivisor 2/ 1 um lshift ndivisor um/mod nip
    then {: ua :}
    ua addr staged/-inverse !
    ndivisor addr staged/-divisor !
    \ ua 1 rshift addr staged/-offset !
    um addr staged/-shift ! ;


\ optimize division instructions

: lit/, {: divisor xt: stage1 xt: stage2 -- :}
    next-section staged/-size small-allot previous-section {: addr :}
    divisor addr stage1 ]] addr stage2 [[ ;

: opt-/f ( xt -- )
    lits# 1 = if
        lits> dup 0> if
            dup pow2? if
                ctz ]] literal arshift [[ drop exit then
            ['] /f-stage1m ['] /f-stage2m lit/, drop exit then
	>lits then
    fold2-1 ;
' opt-/f optimizes /f

: opt-u/ ( xt -- )
    lits# 1 = if
        lits> dup 0<> if
            dup pow2? if
                ctz ]] literal rshift [[ drop exit then
            ['] u/-stage1m ['] u/-stage2m lit/, drop exit then
        >lits then
    fold2-1 ;
' opt-u/ optimizes u/

: opt-modf ( xt -- )
    lits# 1 = if
        lits> dup 0> if
            dup pow2? if
                1- ]] literal and [[ drop exit then
            ['] /f-stage1m ['] modf-stage2m lit/, drop exit then
	>lits then
    fold2-1 ;
' opt-modf optimizes modf

: opt-umod ( xt -- )
    lits# 1 = if
        lits> dup 0<> if
            dup pow2? if
                1- ]] literal and [[ drop exit then
            ['] u/-stage1m ['] umod-stage2m lit/, drop exit then
        >lits then
    fold2-1 ;
' opt-umod optimizes umod

: opt-/modf ( xt -- )
    lits# 1 = if
        lits> dup 0> if
            dup pow2? if
                dup 1- ]] dup literal and swap [[ ctz ]] literal arshift [[
                drop exit then
            ['] /f-stage1m ['] /modf-stage2m lit/, drop exit then
	>lits then
    fold2-2 ;
' opt-/modf optimizes /modf

: opt-u/mod ( xt -- )
    lits# 1 = if
        lits> dup 0<> if
            dup pow2? if
                dup 1- ]] dup literal and swap [[ ctz ]] literal rshift [[
                drop exit then
            ['] u/-stage1m ['] u/mod-stage2m lit/, drop exit then
        >lits then
    fold2-2 ;
' opt-u/mod optimizes u/mod


\ tests and benchmarks
1 [if]
    -1 1 rshift dup constant max-n
    1+ constant min-n

    : check ( f -- )
	0= abort" test failed" ;

    : ntest {: n | buf[ staged/-size ] -- :}
        n buf[ /f-stage1m
	0 buf[ /f-stage2m 0 = check
	-1 buf[ /f-stage2m -1 = check
	min-n n /f {: q1 :}
        min-n buf[ /f-stage2m q1 = check
	q1 1+ n * 1- {: lo :}
	lo buf[ /f-stage2m q1 = check
        lo buf[ modf-stage2m n 1- = check
        lo buf[ /modf-stage2m n 1- q1 d= check
        lo 1+ buf[ /f-stage2m q1 1+ = check
        lo 1+ buf[ modf-stage2m 0 = check
	max-n n /f {: q2 :}
	max-n buf[ /f-stage2m q2 = check
	q2 n * {: hi :}
	hi buf[ /f-stage2m q2 = check
        hi 1- buf[ /f-stage2m q2 1- = check
        max-n buf[ /modf-stage2m n * + max-n = check ;

    : ntests ( -- )
	10000000 1 do i ntest loop
        max-n max-n 10000000 - do i ntest loop ;
    
    : /fstagebench ( -- )
        {: | buf[ staged/-size ] :}
        3 buf[ /f-stage1m
        -1 -1 -100000000 do i - buf[ /f-stage2m loop drop ;

    : /fbench ( -- )
        3 {: x :} -1 -1 -100000000 do i - x /f loop drop ;

    : modfstagebench ( -- )
        {: | buf[ staged/-size ] :}
        3 buf[ /f-stage1m
        -1 -1 -100000000 do i - buf[ modf-stage2m loop drop ;

    : modfbench ( -- )
        3 {: x :} -1 -1 -100000000 do i - x modf loop drop ;

    : /modfstagebench ( -- )
        {: | buf[ staged/-size ] :}
        3 buf[ /f-stage1m
        -1 -1 -100000000 do i - buf[ /modf-stage2m - loop drop ;

    : /modfbench ( -- )
        3 {: x :} -1 -1 -100000000 do i - x /modf - loop drop ;

    : /fstage1bench ( -- )
        {: | buf[ staged/-size ] :}
        10000001 2 do i buf[ /f-stage1m loop ;

    : utest {: u | buf[ staged/-size ] -- :}
	u buf[ u/-stage1m
	0 buf[ u/-stage2m 0= check
	u 1- buf[ u/-stage2m 0= check
	u buf[ u/-stage2m 1 = check
	-1 u u/ dup {: q1 :} u * {: hi :}
	hi buf[ u/-stage2m q1 = check
        hi 1- buf[ u/-stage2m q1 1- = check
        hi buf[ umod-stage2m 0= check
        hi 1- buf[ umod-stage2m u 1- = check
        -1 buf[ u/mod-stage2m u * + -1 = check ;

    : utests ( -- )
	10000000 2 do i utest loop
	min-n 5000000 + min-n 5000000 - do i utest loop
	-1 -10000000 do i utest loop ;

    : u/stagebench ( -- )
        {: | buf[ staged/-size ] :}
        3 buf[ u/-stage1m
        -1 -1 -100000000 do i - buf[ u/-stage2m loop drop ;

    : u/bench ( -- )
        3 {: x :} -1 -1 -100000000 do i - x u/ loop drop ;
    
    : umodstagebench ( -- )
        {: | buf[ staged/-size ] :}
        3 buf[ u/-stage1m
        -1 -1 -100000000 do i - buf[ umod-stage2m loop drop ;

    : umodbench ( -- )
        3 {: x :} -1 -1 -100000000 do i - x umod loop drop ;

    : u/modstagebench ( -- )
        {: | buf[ staged/-size ] :}
        3 buf[ u/-stage1m
        -1 -1 -100000000 do i - buf[ u/mod-stage2m - loop drop ;

    : u/modbench ( -- )
        3 {: x :} -1 -1 -100000000 do i - x u/mod - loop drop ;

    : u/stage1bench ( -- )
        {: | buf[ staged/-size ] :}
        10000001 2 do i buf[ u/-stage1m loop ;
    0 [if]
        \ test with:
        gforth-fast stagediv.fs -e "ntests utests bye"

        \ Benchmark with:
        for i in u/ umod u/mod /f modf /modf; do
            cyc=`perf stat -x " " -e cycles gforth-fast stagediv.fs -e "${i}bench bye" 2>&1 | awk '{printf("%5.1f",$1/100000000)}'`;
            cycstage=`perf stat -x " " -e cycles gforth-fast stagediv.fs -e "${i}stagebench bye" 2>&1 | awk '{printf("%5.1f",$1/100000000)}'`;
            echo $cyc $cycstage" "$i;
         done; \
         for i in u/stage1 /fstage1; do
             perf stat -x " " -e cycles gforth-fast stagediv.fs -e "${i}bench bye" 2>&1 | awk '{printf("%10.1f '$i'\n",$1/10000000)}';
         done
        \ Results (in cycles per iteration of the microbenchmark):
         Haswell              Zen2
        norm stag           norm stag
        48.1 21.5 u/        35.2 21.4 u/   
        46.6 26.0 umod      36.9 25.8 umod 
        53.4 32.1 u/mod     43.0 33.9 u/mod
        56.4 23.4 /f        36.2 22.5 /f   
        55.8 27.2 modf      37.9 27.2 modf 
        64.4 33.3 /modf     45.8 35.3 /modf
            224.3 u/stage1     102.2 u/stage1
            592.5 /fstage1     546.0 /fstage1
    [then]
[then]