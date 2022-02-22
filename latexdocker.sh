echo Running on Linux
docker run -i --rm -w /data -v "$PWD:/data" aergus/latex latexmk