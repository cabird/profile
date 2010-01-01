COVER_LETTERS=$(subst tex,pdf,$(wildcard *-cl.tex))

all: bird_cv.pdf rs.pdf ts.pdf cover_letter.pdf $(COVER_LETTERS) references.pdf pub_list.pdf

all_cites.bbl: all_cites.tex all_cites.tex bird.bib
	pdflatex all_cites
	bibtex all_cites
	pdflatex all_cites
	pdflatex all_cites

bird_cv.pdf: bird_cv.tex bird.bib all_cites.bbl
	cp all_cites.bbl bird_cv.bbl
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

ts.pdf: ts.tex
	pdflatex ts

%-cl.pdf: %-cl.tex cover_letter_template.tex cover_letter_defs.tex
	pdflatex $<

push: bird_cv.pdf
	scp bird_cv.pdf bird@pc10.cs.ucdavis.edu:public_html:cv.pdf
