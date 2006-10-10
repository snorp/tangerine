SUBDIRS = icons deps src plugins TangerinePrefPane

docdir = $(datadir)/doc/tangerine
doc_DATA_DIST = README sample.conf

pkgcfgdir = $(libdir)/pkgconfig
pkgcfg_DATA = tangerine.pc

release: dist
	svn cp http://snorp.net/source/trunk/tangerine \
	http://snorp.net/source/tags/tangerine-$(VERSION) -m "tag for $(VERSION)"
	scp tangerine-$(VERSION).tar.gz root@snorp.net:/var/www/files/tangerine

EXTRA_DIST = tangerine.pc.in
