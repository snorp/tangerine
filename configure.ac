AC_INIT(README)

MAJOR_VERSION=0
MINOR_VERSION=1
MICRO_VERSION=0
VERSION=$MAJOR_VERSION.$MINOR_VERSION.$MICRO_VERSION
                                                                                
AM_INIT_AUTOMAKE(tangerine, $VERSION)

AM_PROG_LIBTOOL
                                                                                
AC_SUBST(MAJOR_VERSION)
AC_SUBST(MINOR_VERSION)
AC_SUBST(MICRO_VERSION)
AC_SUBST(VERSION)
                                                                                
AM_MAINTAINER_MODE

dnl Mono	   
AC_PATH_PROG(MONO, mono)
if test "x$MONO" = "x" ; then
   AC_MSG_ERROR([Can not find "mono" in your PATH])
fi

dnl mcs	   
AC_PATH_PROG(MCS, mcs)
if test "x$MCS" = "x" ; then
   AC_MSG_ERROR([Can not find "mcs" in your PATH])
fi

PKG_CHECK_MODULES(TANGERINE, daap-sharp >= 0.2.0)

DAAP_LIBRARY=`pkg-config --variable=Libraries daap-sharp`
AC_SUBST(DAAP_LIBRARY)

enable_ipod="yes"
IPOD_LIBRARY=`pkg-config --variable=Libraries ipod-sharp`
AC_SUBST(IPOD_LIBRARY)

if test "x$IPOD_LIBRARY" = "x"; then \
   enable_ipod="no"
fi

AM_CONDITIONAL(ENABLE_IPOD, test "x$enable_ipod" = "xyes")

AC_OUTPUT([
Makefile
deps/Makefile
src/Makefile
plugins/Makefile
plugins/file/Makefile
plugins/ipod/Makefile
plugins/banshee/Makefile
plugins/rhythmbox/Makefile
tangerine.pc
])

echo
echo "tangerine prefix:    $prefix"
echo "iPod plugin:         $enable_ipod"
echo
