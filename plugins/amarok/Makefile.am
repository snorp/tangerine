
PLUGIN_ASSEMBLY = tangerine-amarok.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) amarok.provider

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/AmarokPlugin.cs

PLUGIN_DEPS = 						\
	-r:$(top_builddir)/deps/daap-sharp/daap-sharp.dll	\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/entagged-sharp.dll	\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:$(top_builddir)/src/tangerine.dll		\
	-r:System.Data					\
	-r:Mono.Posix					\
	-r:Mono.Data.SqliteClient

tangerine-amarok.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES) amarok.provider
