import sys, re
print """
\\documentclass{article}
\\begin{document}
"""
for line in open(sys.argv[1]).readlines():
	m = re.match(r"@.*?\{(.*),", line)
	if m:
		print "\\cite{" + m.group(1) + "}"
print """ 
\\bibliographystyle{myacm}
\\bibliography{%s}
\\end{document} """ % sys.argv[1].split(".")[0]


