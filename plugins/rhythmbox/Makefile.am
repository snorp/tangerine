
PLUGIN_ASSEMBLY = tangerine-rhythmbox.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) rhythmbox.provider

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/RhythmboxPlugin.cs

PLUGIN_DEPS = 						\
	$(DAAP_SHARP_LIBS)				\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:$(top_builddir)/src/tangerine.dll		\
	-r:Mono.Posix

tangerine-rhythmbox.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES) rhythmbox.provider
