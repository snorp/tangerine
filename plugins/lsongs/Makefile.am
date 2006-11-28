
PLUGIN_ASSEMBLY = tangerine-lsongs.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) lsongs.provider

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/LsongsPlugin.cs

PLUGIN_DEPS = 						\
	-r:$(top_builddir)/deps/daap-sharp/daap-sharp.dll	\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:$(top_builddir)/src/tangerine.dll		\
	-r:System.Data					\
	-r:Mono.Posix					\
	-r:Mono.Data.SqliteClient

tangerine-lsongs.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES) lsongs.provider
