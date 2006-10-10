
PLUGIN_ASSEMBLY = tangerine-spotlight.dll

if BUILD_SPOTLIGHT_PLUGIN
plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) spotlight.provider
endif

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/SpotlightPlugin.cs

PLUGIN_DEPS = 						\
	-r:$(top_builddir)/deps/daap-sharp/daap-sharp.dll	\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/taglib-sharp/taglib-sharp.dll	\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:$(top_builddir)/src/tangerine.dll		\
	-r:Mono.Posix

tangerine-spotlight.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES) spotlight.provider
