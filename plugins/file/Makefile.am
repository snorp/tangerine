
PLUGIN_ASSEMBLY = tangerine-file.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) Db4objects.Db4o.dll

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/FilePlugin.cs

PLUGIN_DEPS = 							\
	$(TAGLIB_SHARP_LIBS)					\
	$(DAAP_SHARP_LIBS)					\
	-r:$(top_srcdir)/deps/Nini.dll				\
	-r:$(srcdir)/Db4objects.Db4o.dll			\
	-r:$(top_srcdir)/deps/log4net.dll			\
	-r:Mono.Posix						\
	-r:$(top_builddir)/src/tangerine.dll

tangerine-file.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES) db4o.dll
