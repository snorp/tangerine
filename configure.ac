AC_INIT(README)

MAJOR_VERSION=0
MINOR_VERSION=2
MICRO_VERSION=2
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

dnl gtk-sharp-2.0, gnome-sharp-2.0, glade-sharp-2.0, daap-sharp
PKG_CHECK_MODULES(GTK_SHARP, gtk-sharp-2.0)
AC_SUBST(GTK_SHARP_LIBS)

PKG_CHECK_MODULES(GNOME_SHARP, gnome-sharp-2.0)
AC_SUBST(GNOME_SHARP_LIBS)

PKG_CHECK_MODULES(GLADE_SHARP, glade-sharp-2.0)
AC_SUBST(GLADE_SHARP_LIBS)

PKG_CHECK_MODULES(DAAP_SHARP, daap-sharp)
AC_SUBST(DAAP_SHARP_LIBS)

dnl beagle
BEAGLE_REQUIRED=0.1.4
PKG_CHECK_MODULES(BEAGLE, beagle-0.0 >= $BEAGLE_REQUIRED,
                  have_beagle=yes, have_beagle=no)
if test "x$have_beagle" = "xyes"; then \
    BEAGLE_PATH="`$PKG_CONFIG --variable=libdir beagle-0.0`/beagle"
    BEAGLE_ASSEMBLIES="$BEAGLE_PATH/Beagle.dll $BEAGLE_PATH/Util.dll"
    AC_SUBST(BEAGLE_LIBS)
    AC_SUBST(BEAGLE_ASSEMBLIES)
fi

dnl ipod-sharp
IPOD_SHARP_REQUIRED=0.5.16
PKG_CHECK_MODULES(IPOD_SHARP, ipod-sharp >= $IPOD_SHARP_REQUIRED,
                  have_ipod=yes, have_ipod=no)
if test "x$have_ipod" = "xyes"; then \
    IPOD_SHARP_ASSEMBLIES=`$PKG_CONFIG --variable=Libraries ipod-sharp`
    AC_SUBST(IPOD_SHARP_LIBS)
    AC_SUBST(IPOD_SHARP_ASSEMBLIES)
fi

PKG_CHECK_MODULES(TANGERINE, daap-sharp >= 0.2.0)

DAAP_LIBRARY=`pkg-config --variable=Libraries daap-sharp`
AC_SUBST(DAAP_LIBRARY)

AM_CONDITIONAL(ENABLE_IPOD, test "x$have_ipod" = "xyes")
AM_CONDITIONAL(ENABLE_BEAGLE, test "x$have_beagle" = "xyes")

AC_OUTPUT([
Makefile
deps/Makefile
src/Makefile
plugins/Makefile
plugins/file/Makefile
plugins/ipod/Makefile
plugins/beagle/Makefile
plugins/session/Makefile
tangerine.pc
])

echo
echo "tangerine prefix:    $prefix"
echo "     iPod plugin:    $have_ipod"
echo "   Beagle plugin:    $have_beagle"
echo
