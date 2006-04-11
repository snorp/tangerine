
PLUGIN_ASSEMBLY = tangerine-rb.dll

plugindir = $(prefix)/lib/tangerine/plugins
plugin_DATA = $(PLUGIN_ASSEMBLY)

CLEANFILES = $(PLUGIN_ASSEMBLY)

PLUGINSOURCES = $(srcdir)/RBPlugin.cs

PLUGIN_DEPS = 						\
	-pkg:daap-sharp					\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:Mono.Posix					\
	-r:$(top_builddir)/src/tangerine.dll

tangerine-rb.dll: $(PLUGINSOURCES)
	$(MCS) $(MCS_FLAGS) -target:library -out:$@ $(PLUGINSOURCES) \
	$(PLUGIN_DEPS)

EXTRA_DIST = $(PLUGINSOURCES)
