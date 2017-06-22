#!/bin/bash
#Copyright (C) 2015,2016 Free Software Foundation, Inc.

#This file is part of Gforth.

#Gforth is free software; you can redistribute it and/or
#modify it under the terms of the GNU General Public License
#as published by the Free Software Foundation, either version 3
#of the License, or (at your option) any later version.

#This program is distributed in the hope that it will be useful,
#but WITHOUT ANY WARRANTY; without even the implied warranty of
#MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
#GNU General Public License for more details.

#You should have received a copy of the GNU General Public License
#along with this program. If not, see http://www.gnu.org/licenses/.

# Generate stuff needed for android Gforth

. build.local
TOOLCHAIN=$(which $TARGET-gcc | sed -e s,/bin/.*-gcc,,g)

FREETYPE=freetype-2.8
HARFBUZZ=harfbuzz-1.4.6

fine=yes
for i in git wget ragel hg
do
    if ! which $i >/dev/null 2>/dev/null
    then
	fine=no
	echo install $i please
    fi
done
if [ $fine = no ]
then
    Missing stuff, exiting
    exit 1
fi

#get dependent files

(cd ~/Downloads
 test -f $FREETYPE.tar.bz2 || wget http://download.savannah.gnu.org/releases/freetype/$FREETYPE.tar.bz2
 test -f $HARFBUZZ.tar.bz2 || wget http://www.freedesktop.org/software/harfbuzz/release/$HARFBUZZ.tar.bz2)

tar jxvf ~/Downloads/$FREETYPE.tar.bz2
tar jxvf ~/Downloads/$HARFBUZZ.tar.bz2

# support stuff

#make and install freetype, part 1 (no harfbuzz)

(cd $FREETYPE
./autogen.sh # get fresh libtool&co
./configure --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/ --with-png=no --with-bzip2=no --with-zlib=no --with-harfbuzz=no 
make -j4
make install)

#make and install harfbuzz

(cd $HARFBUZZ
./autogen.sh --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/ --with-glib=no --with-icu=no --with-uniscribe=no --with-cairo=no
make -j4
make install)

#now freetype with harfbuzz support

(cd $FREETYPE
./configure --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/ --with-png=no --with-bzip2=no --with-zlib=no --with-harfbuzz=yes
make clean
make -j4
make install)

#freetype GL

if [ -f freetype-gl/.git/config ]
then
    (cd freetype-gl; git pull)
else
    git clone -b android https://github.com/forthy42/freetype-gl.git
fi

(cd freetype-gl
./autogen.sh --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/
make
make install
)

# SOIL2

if [ -f soil2/.hg/hgrc ]
then
    (cd soil2; hg pull; hg up)
else
    hg clone https://bitbucket.org/forthy42/soil2
fi

(cd soil2
 if [ "$machine" = 386 ]
 then
     machine=x86
 fi
 premake4 --platform=$machine-android gmake
 (cd make/linux
  make config=release)
 (cd lib/linux
  cp libsoil2.a $TOOLCHAIN/sysroot/usr/lib)
 (cd src/SOIL2
  cp SOIL2.h $TOOLCHAIN/sysroot/usr/include))

$TARGET-libtool  --tag=CC   --mode=link $TARGET-gcc  -O2   -o libtypeset.la -rpath $TOOLCHAIN/sysroot/usr/lib $(find $FREETYPE $HARFBUZZ freetype-gl -name '*.lo') -lm -lGLESv2 -lz -llog

cp .libs/libtypeset.{a,so} $TOOLCHAIN/sysroot/usr/lib
