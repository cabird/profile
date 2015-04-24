all: cv

everything: all $(COVER_LETTERS) $(BUNDLES) bird_cv.pdf rs.pdf ts.pdf references.pdf pub_list.pdf rs-short.pdf #rsts.pdf

cv: bird_cv.pdf

full: clean cv

clean:
	rm -f $ *aux *blg *bbl *log *dvi all_cites* \
		bird_cv.pdf pub_list.pdf references.pdf rs.pdf ts.pdf all_cites.pdf rs-short.pdf

bird_cv.pdf: bird_cv.tex bird.bib revnum.sty
	pdflatex bird_cv
	bibtex bird_cv
	pdflatex bird_cv
	pdflatex bird_cv

pub_list.pdf: pub_list.tex bird.bib all_cites.bbl
	cp all_cites.bbl pub_list.bbl
	pdflatex pub_list
	pdflatex pub_list

references.pdf: references.tex
	pdflatex references

rs.pdf: rs.tex
	pdflatex rs

rs-short.pdf: rs-short.tex
	pdflatex rs-short

ts.pdf: ts.tex
	pdflatex ts

rsts.pdf: rs.pdf ts.pdf
	gs -dBATCH -dNOPAUSE -q -sDEVICE=pdfwrite -sOutputFile=rsts.pdf rs.pdf ts.pdf

%-cl.pdf: %-cl.tex cover_letter_template.tex cover_letter_defs.tex
	pdflatex $<

%-bundle.pdf: %-bundle.tex $(COVER_LETTERS) rs.pdf rs-short.pdf ts.pdf bird_cv.pdf references.pdf
	pdflatex $<

push: bird_cv.pdf
	scp bird_cv.pdf cabird@cabird.com:cabird.com/public/cv.pdf
