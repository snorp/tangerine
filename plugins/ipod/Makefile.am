if ENABLE_IPOD

PLUGIN_ASSEMBLY = tangerine-ipod.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) $(IPOD_SHARP_ASSEMBLIES)

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/IPodPlugin.cs

PLUGIN_DEPS = 						\
	$(IPOD_SHARP_LIBS)				\
	-pkg:daap-sharp					\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:Mono.Posix					\
	-r:$(top_builddir)/src/tangerine.dll

tangerine-ipod.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

endif

EXTRA_DIST = $(PLUGINSOURCES)
