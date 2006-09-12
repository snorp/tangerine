
PLUGIN_ASSEMBLY = tangerine-itunes.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) itunes.provider

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/iTunesPlugin.cs

PLUGIN_DEPS = 						\
	-r:$(top_builddir)/deps/daap-sharp/daap-sharp.dll	\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/entagged-sharp.dll	\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:$(top_builddir)/src/tangerine.dll		\
	-r:Mono.Posix

tangerine-itunes.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES) itunes.provider
