AC_INIT(README)

MAJOR_VERSION=0
MINOR_VERSION=2
MICRO_VERSION=3
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

AC_ARG_ENABLE(mdnsd, AC_HELP_STRING([--enable-mdnsd], [Use mDNSResponder instead of Avahi]), enable_mdnsd="yes", enable_mdnsd="no")
AM_CONDITIONAL(ENABLE_MDNSD, test "x$enable_mdnsd" = "xyes")

if test "x$enable_mdnsd" = "xyes"; then
        MCS_FLAGS="-define:ENABLE_MDNSD"
        AC_SUBST(MCS_FLAGS)
else
        PKG_CHECK_MODULES(AVAHI, avahi-sharp >= 0.6.10)
        MCS_FLAGS=$AVAHI_LIBS
        AC_SUBST(MCS_FLAGS)
fi

dnl gtk-sharp-2.0, gnome-sharp-2.0, glade-sharp-2.0
PKG_CHECK_MODULES(GTK_SHARP, gtk-sharp-2.0)
AC_SUBST(GTK_SHARP_LIBS)

PKG_CHECK_MODULES(GNOME_SHARP, gnome-sharp-2.0)
AC_SUBST(GNOME_SHARP_LIBS)

PKG_CHECK_MODULES(GLADE_SHARP, glade-sharp-2.0)
AC_SUBST(GLADE_SHARP_LIBS)

AC_ARG_ENABLE(beagle, AC_HELP_STRING([--disable-beagle], [Do not build the beagle plugin]), enable_beagle="no", enable_beagle="yes")

if test "x$enable_beagle" = "xyes"; then \
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
else
    have_beagle=no
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

AM_CONDITIONAL(ENABLE_IPOD, test "x$have_ipod" = "xyes")
AM_CONDITIONAL(ENABLE_BEAGLE, test "x$have_beagle" = "xyes")

AC_OUTPUT([
Makefile
deps/Makefile
deps/daap-sharp/Makefile
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

if test "x$enable_mdnsd" = "xyes"; then \
    echo "        Zeroconf:    mDNSResponder"
else
    echo "        Zeroconf:    Avahi"
fi

echo
