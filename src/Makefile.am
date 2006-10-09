TANG_ASSEMBLY = tangerine.dll
TANG_DAEMON = tangerine-daemon.exe

TANG_PROPS_ASSEMBLY = tangerine-properties.exe

if BUILD_GUI
GUI_ASSEMBLY = $(TANG_PROPS_ASSEMBLY)
else
GUI_ASSEMBLY =
endif

asmdir = $(prefix)/lib/tangerine
asm_DATA =				\
	$(TANG_ASSEMBLY)		\
	$(TANG_DAEMON)			\
	tangerine-properties.exe.config	\
	tangerine-daemon.exe.config	\
	$(GUI_ASSEMBLY)

if HAVE_INOTIFY
gluelibdir = $(pkglibdir)
gluelib_LTLIBRARIES = libtangglue.la

libtangglue_la_SOURCES = inotify-glue.c inotify.h inotify-syscalls.h
endif

bin_SCRIPTS = tangerine tangerine-properties

appdir = $(prefix)/share/applications
app_DATA = tangerine-properties.desktop

TANGSOURCES =				\
	$(srcdir)/AssemblyInfo.cs	\
	$(srcdir)/CreationWatcher.cs	\
	$(srcdir)/Daemon.cs		\
	$(srcdir)/Inotify.cs		\
	$(srcdir)/PluginManager.cs	\
	$(srcdir)/Provider.cs		\
	$(srcdir)/UnixSignal.cs

TANG_DEPS =						\
	-r:$(top_builddir)/deps/daap-sharp/daap-sharp.dll	\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:Mono.Posix					\
	$(GLIB_SHARP_LIBS)

$(TANG_ASSEMBLY): $(TANGSOURCES)
	$(MCS) $(MCS_FLAGS) -unsafe -target:library -out:$@ $(TANGSOURCES) \
	$(TANG_DEPS)

TANG_DAEMONSOURCES = $(srcdir)/EntryPoint.cs

$(TANG_DAEMON): $(TANG_DAEMONSOURCES)
	$(MCS) $(MCS_FLAGS) -out:$@ $(TANG_DAEMONSOURCES) -r:tangerine.dll \
	$(TANG_DEPS)

TANG_PROPSOURCES = $(srcdir)/PropertiesWindow.cs $(srcdir)/IconThemeUtils.cs

$(TANG_PROPS_ASSEMBLY): $(TANG_PROPSOURCES)
	$(MCS) $(MCS_FLAGS) -out:$@ $(TANG_PROPSOURCES) -r:tangerine.dll \
	$(TANG_DEPS) $(GNOME_SHARP_LIBS) $(GLADE_SHARP_LIBS) \
	-resource:$(srcdir)/tangerine-properties.glade,tangerine-properties.glade

tangerine: tangerine.in
	sed -e 's,@libdir\@,$(libdir),g' \
	    -e 's,\${exec_prefix},$(prefix),g' \
	    -e 's,\@prefix\@,$(prefix),g' $< > $@

tangerine-properties: tangerine-properties.in
	sed -e 's,@libdir\@,$(libdir),g' \
	    -e 's,\${exec_prefix},$(prefix),g' \
	    -e 's,\@prefix\@,$(prefix),g' $< > $@

EXTRA_DIST =				\
	$(TANGSOURCES)			\
	$(TANG_DAEMONSOURCES)		\
	$(TANG_PROPSOURCES)		\
	$(app_DATA)			\
	tangerine-properties.glade	\
	tangerine-properties.in		\
	tangerine-properties.exe.config	\
	tangerine-daemon.exe.config	\
	tangerine.in

CLEANFILES =			\
	$(TANG_ASSEMBLY)	\
	$(TANG_DAEMON)		\
	$(TANG_PROPS_ASSEMBLY)	\
	tangerine		\
	tangerine-properties
