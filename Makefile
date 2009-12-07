
all: bird_cv.pdf rs.pdf

bird_cv.pdf: bird_cv.tex bird.bib
	pdflatex bird_cv
	bibtex bird_cv
	pdflatex bird_cv
	pdflatex bird_cv

rs.pdf: rs.tex bird.bib
	pdflatex rs
	bibtex rs
	pdflatex rs
	pdflatex rs

push:
	scp bird_cv.pdf bird@pc10.cs.ucdavis.edu:public_html:cv.pdf
