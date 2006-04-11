if ENABLE_IPOD

PLUGIN_ASSEMBLY = tangerine-ipod.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY)

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/IPodPlugin.cs

PLUGIN_DEPS = 						\
	-pkg:daap-sharp					\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:Mono.Posix					\
	-pkg:ipod-sharp					\
	-r:$(top_builddir)/src/tangerine.dll

tangerine-ipod.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

install-data-hook:
	$(INSTALL) -m 644 `pkg-config --variable=Libraries ipod-sharp` $(prefix)/lib/tangerine

endif

EXTRA_DIST = $(PLUGINSOURCES)
