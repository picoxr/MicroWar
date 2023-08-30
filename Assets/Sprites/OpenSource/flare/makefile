
PACKAGEDIR=flare

all: preview package

preview:
	montage *.png -geometry +0+0 -background grey0 preview.jpg

package:
	# tar -czf ../$(PACKAGEDIR)_$(shell date +%Y%m%d).tar.gz ../$(PACKAGEDIR)
	cd ..; zip -r $(PACKAGEDIR)_$(shell date +%Y%m%d).zip $(PACKAGEDIR)/*

