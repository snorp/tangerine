SUBDIRS = src plugins deps

docdir = $(datadir)/doc/tangerine
doc_DATA_DIST = README sample.conf

pkgcfgdir = $(libdir)/pkgconfig
pkgcfg_DATA = tangerine.pc

release: distcheck
	svn cp svn://snorp.net/source/trunk/tangerine \
	svn://snorp.net/source/tags/tangerine-$(VERSION) -m "tag for $(VERSION)"
	scp tangerine-$(VERSION).tar.gz root@snorp.net:/var/www/files/dopi

EXTRA_DIST = tangerine.pc.in
