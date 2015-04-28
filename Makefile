all: cv

everything: all bird_cv.pdf 

cv: bird_cv.pdf

clean:
	rm -f *aux *blg *bbl *log *dvi *out bird_cv.pdf 

bird_cv.pdf: bird_cv.tex bird.bib
	lualatex bird_cv
	bibtex bird_cv
	lualatex bird_cv
	lualatex bird_cv

push: bird_cv.pdf
	scp bird_cv.pdf cabird@cabird.com:cabird.com/public/cv.pdf
