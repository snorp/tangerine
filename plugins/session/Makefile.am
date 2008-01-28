PLUGIN_ASSEMBLY = tangerine-session.dll

if BUILD_SESSION_PLUGIN
plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) $(SESSION_ASSEMBLIES)
endif

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/SessionPlugin.cs

PLUGIN_DEPS = 						\
	$(SESSION_LIBS)					\
	$(GNOME_SHARP_LIBS)				\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:Mono.Posix					\
	-r:$(top_builddir)/src/tangerine.dll

tangerine-session.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

if BUILD_SESSION_PLUGIN
gluelibdir = $(pkglibdir)
gluelib_LTLIBRARIES = libsessionglue.la

libsessionglue_la_SOURCES = session-glue.c
libsessionglue_la_LDFLAGS = -L/usr/X11R6/lib
libsessionglue_la_LIBADD = -lSM
endif

EXTRA_DIST = $(PLUGINSOURCES)
