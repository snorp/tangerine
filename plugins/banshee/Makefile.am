
PLUGIN_ASSEMBLY = tangerine-banshee.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) banshee.provider

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/BansheePlugin.cs

PLUGIN_DEPS = 						\
	$(DAAP_SHARP_LIBS)				\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:$(top_builddir)/src/tangerine.dll		\
	-r:System.Data					\
	-r:Mono.Posix					\
	-r:Mono.Data.SqliteClient

tangerine-banshee.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES) banshee.provider
