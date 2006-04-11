TANG_ASSEMBLY = tangerine.dll
TANG_DAEMON = tangerine-daemon.exe

asmdir = $(prefix)/lib/tangerine
asm_DATA =			\
	$(TANG_ASSEMBLY)	\
	$(TANG_DAEMON)		\
	$(TANG_PLUGINS)		\
	$(DAAP_LIBRARY)

gluelibdir = $(pkglibdir)
gluelib_LTLIBRARIES = libtangglue.la

libtangglue_la_SOURCES = inotify-glue.c inotify.h inotify-syscalls.h

bin_SCRIPTS = tangerine

TANGSOURCES =				\
	$(srcdir)/AssemblyInfo.cs	\
	$(srcdir)/Daemon.cs		\
	$(srcdir)/Inotify.cs		\
	$(srcdir)/PluginManager.cs	\
	$(srcdir)/UnixSignal.cs

TANG_DEPS =						\
	-pkg:daap-sharp					\
	-r:$(top_srcdir)/deps/Nini.dll			\
	-r:$(top_srcdir)/deps/log4net.dll		\
	-r:Mono.Posix					\
	-pkg:avahi-sharp

$(TANG_ASSEMBLY): $(TANGSOURCES)
	$(MCS) $(MCS_FLAGS) -unsafe -target:library -out:$@ $(TANGSOURCES) \
	$(TANG_DEPS)

TANG_DAEMONSOURCES = $(srcdir)/EntryPoint.cs

$(TANG_DAEMON): $(TANG_DAEMONSOURCES)
	$(MCS) $(MCS_FLAGS) -out:$@ $(TANG_DAEMONSOURCES) -r:tangerine.dll \
	$(TANG_DEPS)

tangerine: tangerine.in
	sed -e 's,@libdir\@,$(libdir),g' \
	    -e 's,\${exec_prefix},$(prefix),g' \
	    -e 's,\@prefix\@,$(prefix),g' $< > $@


EXTRA_DIST =			\
	$(TANGSOURCES)		\
	$(TANG_DAEMONSOURCES)	\
	tangerine.in

CLEANFILES = $(TANG_ASSEMBLY) $(TANG_DAEMON) tangerine
