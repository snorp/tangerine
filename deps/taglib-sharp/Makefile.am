MCS_FLAGS = -debug
ASSEMBLY_NAME = taglib-sharp
ASSEMBLY = $(top_builddir)/deps/taglib-sharp/$(ASSEMBLY_NAME).dll

taglibdir = $(prefix)/lib/tangerine
taglib_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

ASSEMBLY_SOURCES = \
	$(srcdir)/TagLib/*.cs \
	$(srcdir)/TagLib/*/*.cs \
	$(srcdir)/TagLib/*/*/*.cs

all: $(ASSEMBLY)

$(ASSEMBLY): $(ASSEMBLY_SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$@ -target:library -r:Mono.Posix $(ASSEMBLY_SOURCES)

EXTRA_DIST = $(ASSEMBLY_SOURCES)
CLEANFILES = $(taglib_SCRIPTS)
MAINTAINERCLEANFILES = Makefile.in

