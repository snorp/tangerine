if ENABLE_BEAGLE

PLUGIN_ASSEMBLY = tangerine-beagle.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY) $(BEAGLE_ASSEMBLIES) beagle.provider

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/BeaglePlugin.cs

PLUGIN_DEPS = 						\
	$(BEAGLE_LIBS)					\
	-r:$(top_builddir)/deps/daap-sharp/daap-sharp.dll	\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/entagged-sharp.dll	\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:Mono.Posix					\
	-r:$(top_builddir)/src/tangerine.dll

tangerine-beagle.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

endif

EXTRA_DIST = $(PLUGINSOURCES)
