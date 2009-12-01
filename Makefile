

bird_cv.pdf: bird_cv.tex bird.bib
	pdflatex bird_cv
	bibtex bird_cv
	pdflatex bird_cv
	pdflatex bird_cv
