PLUGIN_ASSEMBLY = tangerine-session.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) $(SESSION_ASSEMBLIES)

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/SessionPlugin.cs

PLUGIN_DEPS = 						\
	$(SESSION_LIBS)					\
	$(GNOME_SHARP_LIBS)				\
	-r:$(top_srcdir)/deps/daap-sharp/daap-sharp.dll	\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:Mono.Posix					\
	-r:$(top_builddir)/src/tangerine.dll

tangerine-session.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES)
