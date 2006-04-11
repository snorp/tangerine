
PLUGIN_ASSEMBLY = tangerine-banshee.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY)

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/BansheePlugin.cs

PLUGIN_DEPS = 						\
	-pkg:daap-sharp					\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:Mono.Posix					\
	-r:System.Data					\
	-r:Mono.Data.SqliteClient			\
	-r:$(top_builddir)/src/tangerine.dll

tangerine-banshee.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES)
