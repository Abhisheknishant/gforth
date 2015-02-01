# First create a standalone toolchain directory
#mkdir ~/proj/android-toolchain
#cd  ~/proj/android-toolchain
#~/proj/android-ndk-r9c/build/tools/make-standalone-toolchain.sh --platform=android-14 --ndk-dir=/home/bernd/proj/android-ndk-r9c --install-dir=$PWD --toolchain=x86-4.8
#configure with
#./configure --host=arm-linux-android --with-cross=android --prefix= --datarootdir=/sdcard --libdir=/sdcard --libexecdir=/lib --enable-lib --with-ditc=gforth-ditc-x32
#and finally create an apk in this directory
#./build.sh
echo "Config for x86-android"
(cd engine
mkdir .libs
for i in sigaltstack.o __set_errno.o
do
  ar x ~/proj/android-toolchain-x86/sysroot/usr/lib/libc.a $i
  cp $i .libs
  echo "# ${i%o}lo - a libtool object file
# Generated by libtool (GNU libtool) 2.4.2
#
# Please DO NOT delete this file!
# It is necessary for linking the library.

# Name of the PIC object.
pic_object='.libs/$i'

# Name of the non-PIC object
non_pic_object='$i'" >${i%o}lo
done
)
skipcode=".skip 16"
kernel_fi=kernl64l.fi
ac_cv_sizeof_void_p=4
ac_cv_sizeof_char_p=4
ac_cv_sizeof_char=1
ac_cv_sizeof_short=2
ac_cv_sizeof_int=4
ac_cv_sizeof_long=4
ac_cv_sizeof_long_long=8
ac_cv_sizeof_intptr_t=4
ac_cv_sizeof_int128_t=0
ac_cv_c_bigendian=no
ac_cv_func_memcmp_working=yes
ac_cv_func_memmove=yes
ac_cv_file___arch_386_asm_fs=yes
ac_cv_file___arch_368_disasm_fs=yes
ac_cv_func_dlopen=yes
ac_export_dynamic=yes
CC=i686-linux-android-gcc
GNU_LIBTOOL="i686-linux-android-libtool"
build_libcc_named=build-libcc-named
#KBOX=/data/data/kevinboone.androidterm/kbox
#mi_prefix=$KBOX
#mi_prefix=/data/data/gnu.gforth/lib
extraccdir=/data/data/gnu.gforth/lib
asm_fs=arch/386/asm.fs
disasm_fs=arch/386/disasm.fs
EC_MODE="false"
NO_EC=""
EC=""
engine2='engine2$(OPT).o'
engine_fast2='engine-fast2$(OPT).o'
no_dynamic=""
image_i=""
LIBS="-llog -lz"
signals_o="io.o signals.o sigaltstack.o __set_errno.o androidmain.o zexpand.o"

