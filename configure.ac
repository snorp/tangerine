AC_INIT(README)

MAJOR_VERSION=0
MINOR_VERSION=3
MICRO_VERSION=1
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

dnl daap-sharp, taglib-sharp
PKG_CHECK_MODULES(DAAP_SHARP, daap-sharp)
PKG_CHECK_MODULES(TAGLIB_SHARP, taglib-sharp)


dnl mcs	   
AC_PATH_PROG(MCS, gmcs)
if test "x$MCS" = "x" ; then
   AC_MSG_ERROR([Can not find "gmcs" in your PATH])
fi

AC_ARG_ENABLE(beagle, AC_HELP_STRING([--disable-beagle], [Do not build the beagle plugin]), enable_beagle="no", enable_beagle="yes")

have_inotify="no"
build_gui="no"
build_session_plugin="no"
build_itunes_plugin="no"
build_spotlight_plugin="no"
build_prefpane="no"
platform="none"
AC_MSG_CHECKING([host platform characteristics])
case "$host" in
    *-*-linux*)
        PKG_CHECK_MODULES(GLIB_SHARP, glib-sharp-2.0)
        AC_SUBST(GLIB_SHARP_LIBS)

        PKG_CHECK_MODULES(GTK_SHARP, gtk-sharp-2.0)
        AC_SUBST(GTK_SHARP_LIBS)

        PKG_CHECK_MODULES(GLADE_SHARP, glade-sharp-2.0)
        AC_SUBST(GLADE_SHARP_LIBS)

        have_inotify="yes"
        build_gui="yes"
        build_session_plugin="yes"
        platform="LINUX"
        ;;
    *-*-darwin*)
        have_inotify="no"
        build_gui="no"
        build_session_plugin="no"
        build_itunes_plugin="yes"
        build_spotlight_plugin="yes"
	build_prefpane="yes"
        platform="MACOSX"
        ;;
esac
AC_MSG_RESULT(ok)

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


AM_CONDITIONAL(HAVE_INOTIFY, test "x$have_inotify" = "xyes")       
AM_CONDITIONAL(BUILD_GUI, test "x$build_gui" = "xyes")       
AM_CONDITIONAL(BUILD_SESSION_PLUGIN, test "x$build_session_plugin" = "xyes")
AM_CONDITIONAL(BUILD_ITUNES_PLUGIN, test "x$build_itunes_plugin" = "xyes")
AM_CONDITIONAL(BUILD_SPOTLIGHT_PLUGIN, test "x$build_spotlight_plugin" = "xyes")
AM_CONDITIONAL(BUILD_PREFPANE, test "x$build_prefpane" = "xyes")

MCS_FLAGS="$MCS_FLAGS -define:$platform"
AC_SUBST(MCS_FLAGS)

dnl muine
have_muine=no

AC_PATH_PROG(MUINE, muine)
if test "x$MUINE" != "x" ; then
    LIBMUINE32=`dirname $MUINE | sed -e "s/bin/lib\/muine\/libmuine.so/"`
    LIBMUINE64=`dirname $MUINE | sed -e "s/bin/lib64\/muine\/libmuine.so/"`

    LIBMUINE=bogus

    if test -f $LIBMUINE32; then \
        have_muine=yes
        LIBMUINE=$LIBMUINE32
    elif test -f $LIBMUINE64; then \
        have_muine=yes
        LIBMUINE=$LIBMUINE64
    fi
    
    AC_SUBST(LIBMUINE)
fi

AM_CONDITIONAL(ENABLE_BEAGLE, test "x$have_beagle" = "xyes")
AM_CONDITIONAL(ENABLE_MUINE, test "x$have_muine" = "xyes")

AC_OUTPUT([
Makefile
deps/Makefile
icons/Makefile
src/Makefile
plugins/Makefile
plugins/file/Makefile
plugins/beagle/Makefile
plugins/session/Makefile
plugins/banshee/Makefile
plugins/lsongs/Makefile
plugins/rhythmbox/Makefile
plugins/amarok/Makefile
plugins/muine/Makefile
plugins/muine/MuineDatabase.cs
plugins/itunes/Makefile
plugins/spotlight/Makefile
plugins/spotlight/SpotlightHelper/Makefile
TangerinePrefPane/Makefile
tangerine.pc
])

echo
echo "tangerine prefix:    $prefix"
echo "   Beagle plugin:    $have_beagle"
echo "    Muine plugin:    $have_muine"

echo
