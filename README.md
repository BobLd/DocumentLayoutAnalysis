# Document Layout Analysis
Document Layout Analysis repos for development with [PdfPig](https://github.com/UglyToad/PdfPig).

## Definition
From [wikipedia](https://en.wikipedia.org/wiki/Document_layout_analysis): ___Document layout analysis__ is the process of identifying and categorizing the regions of interest in the scanned image of a text document. A reading system requires the segmentation of text zones from non-textual ones and the arrangement in their correct reading order. Detection and labeling of the different zones (or blocks) as text body, illustrations, math symbols, and tables embedded in a document is called __geometric layout analysis__. But text zones play different logical roles inside the document (titles, captions, footnotes, etc.) and this kind of semantic labeling is the scope of the __logical layout analysis__._

In this repos, we will not considere scanned documents, but classic pdf documents and leverage all available information (e.g. letters bounding boxes, fonts).

## Resources
Research papers on __page segmentation__, __table extraction__ and __chart and diagram extraction__ are available in the [Resources](DocumentLayoutAnalysis/DocumentLayoutAnalysis/Resources).

## Progress
### Done
- [Recursive XY Cut](https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig/DocumentLayoutAnalysis/RecursiveXYCut.cs)

![alt text](DocumentLayoutAnalysis/DocumentLayoutAnalysis/doc/xy%20cut_small.gif)
- [Docstrum for bounding boxes](https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig/DocumentLayoutAnalysis/DocstrumBB.cs)
### To do
- Page segmentation: Constrained text-line detection
- Table extraction
- Diagram extraction

# Pdf page to image converter
A [Pdf page to image converter](DocumentLayoutAnalysis/ImageConverter) is available to help in the research proces. It relies on the [_mupdf_](https://github.com/sumatrapdfreader/sumatrapdf/tree/master/mupdf/include/mupdf) library, available in the [sumatra pdf reader](https://github.com/sumatrapdfreader/sumatrapdf).
 
