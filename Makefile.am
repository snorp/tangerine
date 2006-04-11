SUBDIRS = src plugins deps

docdir = $(datadir)/doc/tangerine
doc_DATA_DIST = README sample.conf

pkgcfgdir = $(libdir)/pkgconfig
pkgcfg_DATA = tangerine.pc

release: distcheck
	baz branch tangerine--release--$(VERSION)
	baz commit -s "tag for $(VERSION)"
	baz switch tangerine--dev--0.0
        scp tangerine-$(VERSION).tar.gz root@snorp.net:/var/www/html/files/tangerine

EXTRA_DIST = tangerine.pc.in
