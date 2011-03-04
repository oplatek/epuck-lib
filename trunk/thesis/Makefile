NAME = main
NAMEOUT= thesis
all: ${NAME}.dvi

# Vyroba PDF primo z DVI by byla prijemnejsi, ale vetsina verzi dvipdfm nici obrazky
# ${NAME}.pdf: ${NAME}.dvi
#	dvipdfm -o $@ -p a4 -r 600 $<
pdf:  ${NAME}.pdf
	mv ${NAME}.pdf ../actual/${NAMEOUT}.pdf
	
pdfview: acroread ./actual/${NAME}.pdf 
${NAME}.pdf: ${NAME}.ps
	ps2pdf $< $@

${NAME}.ps: ${NAME}.dvi
	dvips -o $@ -D600 -t a4 $<

# LaTeX je potreba spustit dvakrat, aby spravne spocital odkazy
${NAME}.dvi: ${NAME}.tex $(wildcard *.tex)
	cslatex -src $<
	cslatex -src $<


clean:
	rm -f *.log  *.dvi *.aux *.toc *.lof *.out *.idx *.ilg ${NAME}.ps ${NAME}.pdf
