
all: bird_cv.pdf rs.pdf ts.pdf cover_letter.pdf

bird_cv.pdf: bird_cv.tex bird.bib
	pdflatex bird_cv
	bibtex bird_cv
	pdflatex bird_cv
	pdflatex bird_cv

rs.pdf: rs.tex
	pdflatex rs

ts.pdf: ts.tex
	pdflatex ts

cover_letter.pdf: cover_letter.tex
	pdflatex cover_letter

push: bird_cv.pdf
	scp bird_cv.pdf bird@pc10.cs.ucdavis.edu:public_html:cv.pdf
