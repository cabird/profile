
all: bird_cv.pdf rs.pdf ts.pdf

bird_cv.pdf: bird_cv.tex bird.bib
	pdflatex bird_cv
	bibtex bird_cv
	pdflatex bird_cv
	pdflatex bird_cv

rs.pdf: rs.tex
	pdflatex rs

ts.pdf: ts.tex
	pdflatex ts

push: bird_cv.pdf
	scp bird_cv.pdf bird@pc10.cs.ucdavis.edu:public_html:cv.pdf
