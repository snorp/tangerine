
PLUGIN_ASSEMBLY = tangerine-file.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) db4o.dll

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/FilePlugin.cs

PLUGIN_DEPS = 						\
	-r:$(top_builddir)/deps/daap-sharp/daap-sharp.dll	\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/entagged-sharp.dll	\
	-r:$(srcdir)/db4o.dll				\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:Mono.Posix					\
	-r:$(top_builddir)/src/tangerine.dll

tangerine-file.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES) db4o.dll
