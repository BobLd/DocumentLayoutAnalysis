Document Layout Analysis repos for development with [PdfPig](https://github.com/UglyToad/PdfPig).

>From [wikipedia](https://en.wikipedia.org/wiki/Document_layout_analysis): ___Document layout analysis__ is the process of identifying and categorizing the regions of interest in the scanned image of a text document. A reading system requires the segmentation of text zones from non-textual ones and the arrangement in their correct reading order. Detection and labeling of the different zones (or blocks) as text body, illustrations, math symbols, and tables embedded in a document is called __geometric layout analysis__. But text zones play different logical roles inside the document (titles, captions, footnotes, etc.) and this kind of semantic labeling is the scope of the __logical layout analysis__._

**In this repos, we will not considere scanned documents, but classic pdf documents and leverage all available information (e.g. letters bounding boxes, fonts).**


# Related projects
- [PdfPig](https://github.com/UglyToad/PdfPig) - Read text content from PDFs in C# (port of PdfBox)
- [PublayNetSharp](https://github.com/BobLd/PublayNetSharp) - Extract and convert PubLayNet data to PageXml format
- [PdfPig MLNet Block Classifier](https://github.com/BobLd/PdfPigMLNetBlockClassifier) - Proof of concept of training a simple Region Classifier using PdfPig and ML.NET (LightGBM).
- [PdfPig SVM Region Classifier](https://github.com/BobLd/PdfPigSvmRegionClassifier) - Proof of concept of a simple SVM Region Classifier using PdfPig and Accord.Net.


# Resources
- [Text extraction](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#text-extraction)
- [Word segmentation](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#word-segmentation)
- [Page segmentation](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#page-segmentation)
	- [Recursive XY Cut](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#recursive-xy-cut-implementation)
	- [Docstrum](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#docstrum-implementation)
	- [Voronoi](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#voronoi)
	- [Constrained text-line detection](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#constrained-text-line-detection-implementation-wip)
	- [PDF/A standard](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#pdfa-standard)
- [Zone classification/extraction & Reading order](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#zone-classificationextraction--reading-order)
	- [Reading order](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#reading-order)
	- [Table](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#table)
		- [Systems](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#systems)
		- [Sparse line](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#sparse-line)
	- [Chart and diagram](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#chart-and-diagram)
	- [Mathematical expression](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#mathematical-expression)
	- [Margins recognition](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#margins-recognition)
- [NLP & ML](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#nlp--ml)
	- [Workshops](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#workshops)
- [Related topics](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#related-topics)
	- [Bounding boxes](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#bounding-boxes)
	- [Images](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#images)
	- [Shape detection](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#shape-detection)
	- [Character Recognition](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#character-recognition)
	- [Layout Similarity](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#layout-similarity)
	- [Dehyphenation](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#dehyphenation)
	- [Data structure](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#data-structure)
- [Datasets](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#datasets)
- [Output file format](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/README.md#output-file-format)



## Text extraction
- [High precision text extraction from PDF documents](https://www.duo.uio.no/bitstream/handle/10852/8893/Berg.pdf?sequence=1) | Øyvind Raddum Berg
- [User-Guided Information Extraction from Print-Oriented Documents](https://www.dbai.tuwien.ac.at/staff/hassan/files/dissertation.pdf) | Tamir Hassan
- [Combining Linguistic and Spatial Information for Document Analysis](https://arxiv.org/ftp/cs/papers/0009/0009014.pdf) | Aiello, Monz and Todoran
- [New Methods for Metadata Extraction from Scientific Literature](https://arxiv.org/pdf/1710.10201.pdf) | Dominika Tkaczyk
- [A System for Converting PDF Documents into Structured XML Format](https://www.researchgate.net/publication/220933081_A_System_for_Converting_PDF_Documents_into_Structured_XML_Format) | Hervé Déjean, Jean-Luc Meunier
- [Layout and Content Extraction for PDF Documents](https://www.researchgate.net/publication/220932927_Layout_and_Content_Extraction_for_PDF_Documents) | Hui Chao, Jian Fan
- [DocParser: Hierarchical Structure Parsing of Document Renderings](https://arxiv.org/pdf/1911.01702.pdf) | J. Rausch, O. Martinez, F. Bissig, C. Zhang, and S. Feuerriegel


## Word segmentation
- [An Efficient Word Segmentation Technique for Historical and Degraded Machine-Printed Documents](https://citeseerx.ist.psu.edu/viewdoc/summary?doi=10.1.1.145.2079) | M. Makridis, N. Nikolaou, B. Gatos
- [Word Extraction Using Area Voronoi Diagram](http://andrei.clubcisco.ro/cursuri/f/f-sym/5master/analiza-extragerea-continutului/prezentari/Word%20Extraction%20Using%20Area%20Voronoi%20Diagram.pdf) | Zhe Wang, Yue Lu, Chew Lim Tan

![example](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/DocumentLayoutAnalysis/DocumentLayoutAnalysis/doc/nearest%20neighbour%20word%20example%20v2.png)


## Page segmentation
- [Performance Comparison of Six Algorithms for Page Segmentation](https://www.researchgate.net/publication/220932988_Performance_Comparison_of_Six_Algorithms_for_Page_Segmentation) | Faisal Shafait, Daniel Keysers, and Thomas M. Breuel
- [A Fast Algorithm for Bottom-Up Document Layout Analysis](https://dl.acm.org/doi/10.1109/34.584106) | Anikó Simon, Jean-Christophe Pret, and A. Peter Johnson
- [Empirical Performance Evaluation Methodology and its Application to Page Segmentation Algorithms: A Review](https://www.researchgate.net/publication/220932988_Performance_Comparison_of_Six_Algorithms_for_Page_Segmentation) | Pinky Gather, Avininder Singh
- [Layout Analysis based on Text Line Segment Hypotheses](https://citeseerx.ist.psu.edu/viewdoc/summary?doi=10.1.1.124.3478) | Thomas M. Breuel
- [Hybrid Page Layout Analysis via Tab-Stop Detection](http://www.cvc.uab.es/icdar2009/papers/3725a241.pdf) | [presentation](https://tesseract-ocr.github.io/docs/das_tutorial2016/5LayoutAnalysis.pdf) | Ray Smith
- Extending the Page Segmentation Algorithms of the Ocropus Documentation Layout Analysis System | Amy Alison Winder
- [Object-Level Document Analysis of PDF Files](https://www.dbai.tuwien.ac.at/staff/hassan/files/p47-hassan.pdf) | Tamir Hassan
- [Document Image Segmentation as a Spectral Partitioning Problem](https://cdn.iiit.ac.in/cdn/cvit.iiit.ac.in/images/ConferencePapers/2008/Praveen08Document.pdf) | Dasigi, Jain and Jawahar
- [Benchmarking Page Segmentation Algorithms](http://www2.vincent-net.com/luc/papers/94cvpr_seg_benchmark.pdf) | S. Randriamasy, L. Vincent

#### Recursive XY Cut [`implementation`](https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig.DocumentLayoutAnalysis/PageSegmenter/RecursiveXYCut.cs)
  The X-Y cut segmentation algorithm, also referred to as recursive X-Y cuts (RXYC) algorithm, is a tree-based __top-down__ algorithm.
The root of the tree represents the entire document page. All the leaf nodes together represent the final segmentation. The RXYC algorithm __recursively splits the document into two or more smaller rectangular blocks which represent the nodes of the tree. At each step of the recursion, the horizontal and vertical projection profiles of each node are computed.__ Then, the valleys along the horizontal and vertical directions, _VX_ and _VY_, are compared to corresponding predefined thresholds _TX_ and _TY_. If the valley is larger than the threshold, the node is split at the mid-point of the wider of _VX_ and _VY_ into two children nodes. The process continues until no leaf node can be split further. Then, noise regions are removed using noise removal thresholds _TnX_ and _TnY_. [`source`](https://www.researchgate.net/publication/220932988_Performance_Comparison_of_Six_Algorithms_for_Page_Segmentation)
![example](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/DocumentLayoutAnalysis/DocumentLayoutAnalysis/doc/rxyc%20example.png)
- [Recursive X-Y Cut using Bounding Boxes of Connected Components](https://www.researchgate.net/publication/220860850_Recursive_X-Y_cut_using_bounding_boxes_of_connected_components) | Jaekyu Ha, Robert M. Haralick and Ihsin T. Phillips

#### Docstrum [`implementation`](https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig.DocumentLayoutAnalysis/PageSegmenter/DocstrumBoundingBoxes.cs)
  The Docstrum algorithm by Gorman is a __bottom-up__ approach based on __nearest-neighborhood clustering__ of connected components extracted from the document image. After noise removal, the connected components are separated into two groups, one with dominant characters and another one with characters in titles and section heading, using a character size ratio factor _fd_. Then, _K_ nearest neighbors are found for each connected component. Then, text-lines are found by computing the transitive closure on within-line nearest neighbor pairings using a threshold _ft_. Finally, text-lines are merged to form text blocks using a parallel distance threshold _fpa_ and a perpendicular distance threshold _fpe_. [`source`](https://www.researchgate.net/publication/220932988_Performance_Comparison_of_Six_Algorithms_for_Page_Segmentation)
![example or](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/DocumentLayoutAnalysis/DocumentLayoutAnalysis/doc/docstrum%20example%202.png)
![example](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/DocumentLayoutAnalysis/DocumentLayoutAnalysis/doc/docstrum%20example%201.png)
- [The Document Spectrum for Page Layout Analysis](https://ieeexplore.ieee.org/document/244677) | Lawrence O'Gorman
- [Document Structure and Layout Analysis](https://www.semanticscholar.org/paper/Document-Structure-and-Layout-Analysis-Namboodiri-Jain/42b2bc874b1b080cde919c2d9220f32a0023ac66) | Anoop M. Namboodiri and Anil K. Jain
- [Document Layout Analysis](https://inside.mines.edu/~whoff/courses/EENG510/projects/2015/Hoch.pdf) | Garrett Hoch

#### Voronoi
  The Voronoi-diagram based segmentation algorithm by Kise et al. is also a bottom-up algorithm. In the first step, it extracts sample points from the boundaries of the connected components using a sampling rate _sr_. Then, noise removal is done using a maximum noise zone size threshold _nm_, in addition to width, height, and aspect ratio thresholds. After that the Voronoi diagram is generated using sample points obtained from the borders of the connected components. Superfluous Voronoi edges are deleted using a criterion involving the area ratio threshold _ta_, and the inter-line spacing margin control factor _fr_. Since we evaluate all algorithms on document pages with Manhattan layouts, a modified version of the algorithm is used to generate rectangular zones.[`source`](https://www.researchgate.net/publication/220932988_Performance_Comparison_of_Six_Algorithms_for_Page_Segmentation)
- [Voronoi++: A Dynamic Page Segmentation approach based on Voronoi and Docstrum features](http://www.cvc.uab.es/icdar2009/papers/3725b011.pdf) | Mudit Agrawal and David Doermann

#### Constrained text-line detection [`implementation (wip)`](https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig.DocumentLayoutAnalysis/WhitespaceCoverExtractor.cs)
  The layout analysis approach by Breuel finds text-lines as a two step process:
1. Find tall whitespace rectangles and evaluate them as candidates for gutters, column separators, etc. The algorithm for finding maximal empty whitespace is described in Breuel. The whitespace rectangles are returned in order of decreasing quality and are allowed a maximum overlap of _Om_.
2. The whitespace rectangles representing the columns are used as obstacles in a robust least square, globally optimal text-line detection algorithm. Then, the bounding box of all the characters making the text-line is computed.
The method was merely intended by its author as a demonstration of the application of two geometric algorithms, and not as a complete layout analysis system; nevertheless, we included it in the comparison because it has already proven useful in some applications. It is also nearly parameter free and resolution independent.[`source`]()
- [Two Geometric Algorithms for Layout Analysis](https://static.aminer.org/pdf/PDF/000/140/219/two_geometric_algorithms_for_layout_analysis.pdf) | Thomas M. Breuel
- [High precision text extraction from PDF documents](https://www.duo.uio.no/bitstream/handle/10852/8893/Berg.pdf?sequence=1) | Øyvind Raddum Berg
- High Performance Document Layout Analysis | Thomas M. Breuel

#### PDF/A standard
[PDF/A-1a](https://en.wikipedia.org/wiki/PDF/A#PDF/A-1) compliant document make the following information available:
1. Language specification
2. Hierarchical document structure
3. Tagged text spans and descriptive text for images and symbols
4. Character mappings to Unicode


## Zone classification/extraction & Reading order
- [Page Segmentation and Zone Classification: The State of the Art](https://www.researchgate.net/publication/235068649_Page_Segmentation_and_Zone_Classification_The_State_of_the_Art) | Oleg Okun, David Doermann, Matti Pietikainen
- [Looking Beyond Text: Extracting Figures, Tables and Captions from Computer Science Papers](http://ai2-website.s3.amazonaws.com/publications/clark_divvala.pdf) | C. Clark, S. Divvala
- [PDFFigures 2.0: Mining Figures from Research Papers](http://ai2-website.s3.amazonaws.com/publications/pdf2.0.pdf) | C. Clark, S. Divvala | [github](https://github.com/allenai/pdffigures2)
- [Document image zone classification : A simple high-performance approach](https://www.researchgate.net/publication/221415986_Document_image_zone_classification_A_simple_high-performance_approach) | Daniel Keysers, Faisal Shafait, Thomas M. Breuel
- [Document-Zone Classification using Partial Least Squares and Hybrid Classifiers](https://www.researchgate.net/publication/224375306_Document-Zone_Classification_using_Partial_Least_Squares_and_Hybrid_Classifiers) | Wael Abd-Almageed, Mudit Agrawal, Wontaek Seo, David Doermann
- [The Zonemap Metric for Page Segmentation and Area Classification Inscanned Documents](https://projet.liris.cnrs.fr/imagine/pub/proceedings/ICIP-2014/Papers/1569886529.pdf) | Olivier Galibert, Juliette Kahn and Ilya Oparin
- [Layout analysis and content classification indigitized books](http://imagelab.ing.unimore.it/imagelab/pubblicazioni/2016_IRCDL.pdf) | Andrea Corbelli, Lorenzo Baraldi, Fabrizio Balducci,Costantino Grana, Rita Cucchiara

### Reading order
- [Unsupervised document structure analysis of digital scientific articles](http://www.know-center.tugraz.at/download_extern/papers/ijdl-2013.pdf) | S. Klampfl, M. Granitzer, K. Jack, R. Kern
- [Document understanding for a broad class of documents](http://www.cs.rug.nl/~aiellom/publications/ijdar.pdf) | M. Aiello, C. Monz, L. Todoran, M. Worring
- [A Data Mining Approach to Reading Order Detection](http://www.di.uniba.it/~ceci/Papers/Pubblicazioni/International%20Collections/IC.32__ICDAR07.pdf) | M. Ceci, M. Berardi, G. A. Porcelli

### Table
- [Design of an end-to-end method to extract information from tables](https://www.researchgate.net/publication/225153001_Design_of_an_end-to-end_method_to_extract_information_from_tables) | A. Costa e Silva, A. Jorge, L. Torgo
- [A Table Detection Method for PDF Documents Based on Convolutional Neural Networks](https://www.researchgate.net/publication/303950713_A_Table_Detection_Method_for_PDF_Documents_Based_on_Convolutional_Neural_Networks) | L. Hao, L. Gao, X. Yi, Z. Tang
- [Extracting Tables from Documents using Conditional Generative Adversarial Networks and Genetic Algorithms](https://arxiv.org/pdf/1904.01947.pdf) | Nataliya Le Vine, Matthew Zeigenfuse, Mark Rowany
- [Detecting Table Region in PDF Documents Using Distant Supervision](https://arxiv.org/pdf/1506.08891v6.pdf) | Miao Fan and Doo Soon Kim
- Automatic Tabular Data Extraction and Understanding | Roya Rastan
- [Algorithmic Extraction of Data in Tables in PDF Documents](https://pdfs.semanticscholar.org/a9b1/67a86fb189bfcd366c3839f33f0404db9c10.pdf) | Anssi Nurminen
- [A Multi-Layered Approach to Information Extraction from Tables in Biomedical Documents](https://www.research.manchester.ac.uk/portal/files/70405100/FULL_TEXT.PDF) | Nikola Milosevic
- [Integrating and querying similar tables from PDF documentsusing deep learning](https://arxiv.org/pdf/1901.04672.pdf) | Rahul Anand, Hye-young Paik and Chen Wang
- [Locating Tables in Scanned Documents for Reconstructing and Republishing ](https://arxiv.org/search/?query=document+layout+analysis&searchtype=all&source=header) | MAC Akmal Jahan, Roshan G Ragel 
- [Recognition of Tables and Forms](https://hal.inria.fr/hal-01087230/document) | Bertrand Coüasnon, Aurélie Lemaitre
- [TableBank: Table Benchmark for Image-based Table Detection and Recognition](https://arxiv.org/pdf/1903.01949v1.pdf) | M. Li, L. Cui, S. Huang, F. Wei, M. Zhou and Z. Li
- [Looking Beyond Text: Extracting Figures, Tables and Captions from Computer Science Papers](https://ai2-website.s3.amazonaws.com/publications/clark_divvala.pdf) | Christopher Clark and Santosh Divvala | [`website`](http://pdffigures.allenai.org/)
- [A Table Detection Method for Multipage PDF Documents via Visual Seperators and Tabular Structures](http://www.iapr-tc11.org/archive/icdar2011/fileup/PDF/4520a779.pdf) | J. Fang, L. Gao, K. Bai, R. Qiu, X. Tao, Z. Tang
- [A Rectangle Mining Method for Understandingthe Semantics of Financial Tables](https://www.cs.cornell.edu/~xlchen/resources/papers/TableExtraction.pdf) | X. Chen, L. Chiticariu, M. Danilevsky, A. Evfimievski and P. Sen
- [Table Header Detection and Classification](https://clgiles.ist.psu.edu/pubs/AAAI2012-table-header.pdf) | J. Fang, P. Mitra, Z. Tang, C. L. Giles
- [Configurable Table Structure Recognition in Untagged PDF Documents](https://www.researchgate.net/publication/307174717_Configurable_Table_Structure_Recognition_in_Untagged_PDF_Documents) | A. Shigarov, A. Mikhailov, A. Altaev | [`ppt`](https://www.slideshare.net/shig/configurable-table-structure-recognition-in-untagged-pdf-documents)
- [Complicated Table Structure Recognition](https://arxiv.org/pdf/1908.04729.pdf) | Z. Chi, H. Huang, H. Xu, H. Yu, W. Yin, X. Mao | [github](https://github.com/Academic-Hammer/SciTSR)

#### Systems
- [pdf2table: A Method to Extract Table Information from PDF Files](http://ieg.ifs.tuwien.ac.at/pub/yildiz_iicai_2005.pdf) | Burcu Yildiz, Katharina Kaiser, and Silvia Miksch
- [PDF-TREX: An Approach for Recognizing and Extracting Tables from PDF Documents](http://www.cvc.uab.es/icdar2009/papers/3725a906.pdf) | Ermelinda Oro, Massimo Ruffolo
- [TAO: System for Table Detection and Extraction from PDF Documents](https://pdfs.semanticscholar.org/22c9/f2d80e0f0546a54c82dbc9cfc9e68ce9a1ff.pdf) | Martha O. Perez-Arriaga, Trilce Estrada, and Soraya Abad-Mota

#### Sparse line
- [Identifying Table Boundaries in Digital Documents via Sparse Line Detection](https://clgiles.ist.psu.edu/pubs/CIKM2008-table-boundaries.pdf) | Ying Liu, Prasenjit Mitra, C. Lee Giles
- [A Fast Preprocessing Method for Table Boundary Detection: Narrowing Down the Sparse Lines using Solely Coordinate Information](https://www.researchgate.net/publication/220933121_A_Fast_Preprocessing_Method_for_Table_Boundary_Detection_Narrowing_Down_the_Sparse_Lines_Using_Solely_Coordinate_Information) | Ying Liu, Prasenjit Mitra, C. Lee Giles
- [Improving the Table Boundary Detection in PDFs by Fixing the Sequence Error of the Sparse Lines](https://clgiles.ist.psu.edu/pubs/ICDAR2009-table-boundary-detection.pdf) | Ying Liu, Kun Bai, Prasenjit Mitra, C. Lee Giles
- [Automatic Table Ground Truth Generation and A Background-analysis-based Table Structure Extraction Method](http://gsl.lab.asu.edu/doc/tableicdar01.pdf) | Yalin Wangt, Ihsin T. Phillips and Robert Haralickt

### Chart and diagram
- [FigureSeer: Parsing Result-Figures in Research Papers](http://ai2-website.s3.amazonaws.com/publications/Siegel16eccv.pdf) | N. Siegel, Z. Horvitz, R. Levin, S. Divvala, and A. Farhadi
- [Extraction, layout analysis and classification of diagrams in PDF documents](http://www.ccs.northeastern.edu/home/futrelle/pubs37/diagrams/DiagramPapers/ExtractionLayout2003.pdf) | Robert P. Futrelle, Mingyan Shao, Chris Cieslik and Andrea Elaina Grimes
- [Graphics Recognition in PDF documents](https://pdfs.semanticscholar.org/9c18/d90f1988d1d98f061bdd076d56983a82803d.pdf) | Mingyan Shao and Robert P. Futrelle
- [A Study on the Document Zone Content Classification Problem](http://gsl.lab.asu.edu/doc/zonedas02.pdf)
 | Yalin Wang, Ihsin T. Phillips, and Robert M. Haralick
- Text/Figure Separation in Document Images Using Docstrum Descriptor and Two-Level Clustering | Valery Anisimovskiy, Ilya Kurilin, Andrey Shcherbinin, Petr Pohl
- [CHART-Synthetic](https://github.com/adobe-research/CHART-Synthetic)
- [Looking Beyond Text: Extracting Figures, Tables and Captions from Computer Science Papers](https://ai2-website.s3.amazonaws.com/publications/clark_divvala.pdf) | Christopher Clark and Santosh Divvala | [`website`](http://pdffigures.allenai.org/)
- [Metrics for Evaluating Data Extraction from Charts](https://github.com/adobe-research/CHART-Synthetic/blob/master/metric6b.pdf) | Adobe Research | [github](https://github.com/adobe-research/CHART-Synthetic)

### Mathematical expression
- [A Font Setting Based Bayesian Model to Extract Mathematical Expression in PDF Files](https://www.researchgate.net/publication/322779502_A_Font_Setting_Based_Bayesian_Model_to_Extract_Mathematical_Expression_in_PDF_Files) | Xing Wang, Jyh-Charn Liu
- [Mathematical Formula Identification in PDF Documents](https://www.researchgate.net/publication/220860588_Mathematical_Formula_Identification_in_PDF_Documents) | Xiaoyan Lin, Liangcai Gao, Zhi Tang, Xiaofan Lin
- [Faithful Mathematical Formula Recognition from PDF Documents](https://www.cs.bham.ac.uk/~jbb/documents/das10.pdf) | Josef B. Baker, Alan P. Sexton and Volker Sorge
- [Extracting Precise Data from PDF Documents for Mathematical Formula Recognition](https://www.cs.bham.ac.uk/~jbb/documents/das08.pdf) | Josef B. Baker, Alan P. Sexton and Volker Sorge
- [Mathematical formula identification and performance evaluation in PDF documents](https://link.springer.com/article/10.1007%2Fs10032-013-0216-1) | Xiaoyan Lin, Liangcai Gao, Zhi Tang, Josef Baker, Volker Sorge

### Margins recognition
- [Finding blocks of text in an image using Python, OpenCV and numpy](https://www.danvk.org/2015/01/07/finding-blocks-of-text-in-an-image-using-python-opencv-and-numpy.html)
- [Notes on the margins: how to extract them using image segmentation, Google Vision API, and R](http://travelerslab.research.wesleyan.edu/2016/09/13/notes-on-the-margins/)
- [A mixed approach to auto-detection of page body](https://www.researchgate.net/publication/221253859_A_mixed_approach_to_auto-detection_of_page_body) | Liangcai Gao, Zhi Tang, Ruiheng Qiu
- [Header and Footer Extraction by Page-Association](https://www.hpl.hp.com/techreports/2002/HPL-2002-129.pdf) | Xiaofan Lin
- [A System for Converting PDF Documents into Structured XML Format](https://www.researchgate.net/publication/220933081_A_System_for_Converting_PDF_Documents_into_Structured_XML_Format) | Hervé Déjean, Jean-Luc Meunier


## NLP & ML
- [Chargrid: Towards Understanding 2D Documents](https://arxiv.org/pdf/1809.08799.pdf) | A. R. Katti, C. Reisswig, C. Guder, S. Brarda, S. Bickel, J. Höhne, J. B. Faddoul | [medium](https://medium.com/sap-machine-learning-research/chargrid-77aa75e6d605)
- [Chargrid-OCR: End-to-end trainable Optical Character Recognition through Semantic Segmentation and Object Detection](https://openreview.net/forum?id=SkxhzT5qIB) | C. Reisswig, A. R. Katti, M. Spinaci, J. Höhne | [slides](https://drive.google.com/file/d/1a6qIszxXl7FaXurW3MCbZzSTnmYRKDuQ/view)
- [BERTgrid: Contextualized Embedding for 2D Document Representation and Understanding](https://openreview.net/forum?id=H1gsGaq9US) | Timo I. Denk, Christian Reisswig | [slides](https://drive.google.com/file/d/1WsgZ5QZFw8GBfxknb96AbH-dLzu0DIAN/view)
- [LayoutLM: Pre-Training of Text and Layout for Document Image Understanding](https://arxiv.org/pdf/1912.13318.pdf) | Yiheng Xu, Minghao Li, Lei Cui, Shaohan Huang, Furu Wei, Ming Zhou | [github](https://github.com/microsoft/unilm/tree/master/layoutlm)
- [Detect2Rank: Combining Object Detectors UsingLearning to Rank](https://arxiv.org/pdf/1412.7957.pdf) | S. Karaoglu, Y. Liu., T. Gevers

### Workshops
- [Workshop on Document Intelligence (DI 2019) at NeurIPS 2019](https://sites.google.com/view/di2019)


## Related topics
- [Improving typography and minimising computation for documents with scalable layouts](http://eprints.nottingham.ac.uk/28872/1/alex-pinkney-thesis.pdf) | Pinkney, Alexander J.
- [Breaking Paragraphs into Lines](https://jimfix.github.io/math382/knuth-plass-breaking.pdf) | D. E. Knuth, M. F. Plass

### Bounding boxes
- [Fast Visual Object Tracking with Rotated Bounding Boxes](https://arxiv.org/abs/1907.03892) | Bao Xin Chen, John K. Tsotsos
- [Building Non-Overlapping Polygons for Image Document Layout Analysis Results](https://pdfs.semanticscholar.org/86cb/00fb775af8d2d3970b1ccf31f5c3c83fdd89.pdf) | C.-A. Boiangiu, Mihai Zaharescu, I. Bucur
- [Ensure Non-Overlapping in Document Layout Analysis](https://www.researchgate.net/publication/235419952_Ensure_Non-Overlapping_in_Document_Layout_Analysis) | C.-A. Boiangiu, B. Raducanu, S. Petrescu, I. Bucur
- [Beta-Shape Using Delaunay-Based Triangle Erosion](https://www.academia.edu/39847463/Beta-Shape_Using_Delaunay-Based_Triangle_Erosion) | C.-A. Boiangiu

### Images
- [Analysing layout information: searching PDF documents for pictures](https://pdfs.semanticscholar.org/2bec/a8fcdbdcbe02bc30d40113432daed61f9660.pdf) | B. Mathiak et al.

### Shape detection
- [Polygon Detection from a Set of Lines](https://web.ist.utl.pt/alfredo.ferreira/publications/12EPCG-PolygonDetection.pdf) |  Alfredo Ferreira, Manuel J. Fonseca, Joaquim A. Jorge
- [A Simple Approach to Recognise Geometric Shapes Interactively](http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.24.2707&rep=rep1&type=pdf) | Joaquim A. Jorge and Manuel J. Fonseca
- [The Detection of Rectangular Shape Objects Using Matching Schema](https://pdfs.semanticscholar.org/2a73/b9533532b7426fd5a7c47ac3f40e0dc88d7f.pdf) | Soo-Young Ye, Joon-Young Choi and Ki-Gon Nam
- [Edge Detection Based Shape Identification](https://arxiv.org/ftp/arxiv/papers/1604/1604.02030.pdf) | Vivek Kumar, Sumit Pandey, Amrindra Pal, Sandeep Sharma
- [Algorithms for the Reduction of the Number of Points Required to Represent a Digitized Line or its Caricature](http://www2.ipcku.kansai-u.ac.jp/~yasumuro/M_InfoMedia/paper/Douglas73.pdf) | David H. Douglas and Thomas K. Peucker
- [Shape description using cubic polynomial Bezier curves](https://www.sciencedirect.com/science/article/abs/pii/S0167865598000695) | L. Cinque, S. Levialdi, A. Malizia
- [New Algorithm for Medial Axis Transform of Plane Domain](https://pdfs.semanticscholar.org/70ae/5b583303af0b4d60d356d08f8ed84e1babbc.pdf) and details from [stackoverflow](https://stackoverflow.com/questions/29921826/how-do-i-calculate-the-medial-axis-for-a-2d-vector-shape) | Choi, Choi, Moon and Wee

### Character Recognition
- [RNN-Based Handwriting Recognition in Gboard](https://ai.googleblog.com/2019/03/rnn-based-handwriting-recognition-in.html) | Sandro Feuz and Pedro Gonnet | [`arxiv`](https://arxiv.org/abs/1902.10525)
- [Handwritten Arabic Digits Recognition Using Bézier Curves](http://ijcsi.org/papers/IJCSI-10-5-2-257-263.pdf) | Aissa Kerkour El Miad and Azzeddine Mazroui

### Layout Similarity
- [A Retrieval Framework and Implementation for Electronic Documents with Similar Layouts](https://arxiv.org/ftp/arxiv/papers/1810/1810.07237.pdf) | Chung

### Dehyphenation
- [Dehyphenation - Some empirical methods](https://www.duo.uio.no/bitstream/handle/10852/9043/Bauge2012-dehyphenation.pdf) | Ola S. Bauge
- [Improved Dehyphenation of Line Breaks for PDF Text Extraction](http://ad-publications.informatik.uni-freiburg.de/theses/Bachelor_Mari_Hernaes_2019.pdf) | Mari Sverresdatter Hernæs
- [Dehyphenation of Words and Guessing Ligatures](http://ad-publications.informatik.uni-freiburg.de/theses/Master_Sumitra_Corraya_2018.pdf) | Sumitra Magdalin Corraya
- [How Document Pre-processing affects Keyphrase Extraction Performance](https://arxiv.org/pdf/1610.07809.pdf) | F. Boudin, H. Mougard, D. Cram

### Data structure
- [Kd-Trees for Document Layout Analysis](https://www.researchgate.net/publication/281267378_Kd-Trees_for_Document_Layout_Analysis) | Christoph Dalitz


## Datasets
- [PubLayNet: largest dataset ever for document layout analysis](https://arxiv.org/pdf/1908.07836.pdf) | Zhong, Tang and Yepes | [`github`](https://github.com/ibm-aur-nlp/PubLayNet)
- [DocParser: Hierarchical Structure Parsing of Document Renderings](https://arxiv.org/pdf/1911.01702.pdf) | J. Rausch, O. Martinez, F. Bissig, C. Zhang, and S. Feuerriegel
- [TableBank: Table Benchmark for Image-based Table Detection and Recognition](https://arxiv.org/pdf/1903.01949v1.pdf) | M. Li, L. Cui, S. Huang, F. Wei, M. Zhou and Z. Li
- [Document Image Datasets](https://medium.com/@jdegange85/document-image-datasets-b7f8df01010d) | Jonathan DeGange

## Output file format
- hOCR: [hocr spec](https://github.com/kba/hocr-spec) | [`implementation`](https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig/Export/HOcrTextExporter.cs)
- ALTO XML: [alto schema](https://github.com/altoxml/schema) | [`implementation`](https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig/Export/AltoXmlTextExporter.cs)
- TEI: [tei-ocr](https://github.com/OpenPhilology/tei-ocr) | [schema](https://tei-c.org/guidelines/customization/)
- PAGE: [PAGE-XML](https://github.com/PRImA-Research-Lab/PAGE-XML) | [`implementation`](https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig/Export/PageXmlTextExporter.cs)

[Validate and transform between OCR file formats (hOCR, ALTO, PAGE, FineReader)](https://github.com/UB-Mannheim/ocr-fileformat)

# Pdf page to image converter
A [Pdf page to image converter](DocumentLayoutAnalysis/ImageConverter) is available to help in the research proces. It relies on the [_mupdf_](https://github.com/sumatrapdfreader/sumatrapdf/tree/master/mupdf/include/mupdf) library, available in the [sumatra pdf reader](https://github.com/sumatrapdfreader/sumatrapdf).

# Pdf layout analysis viewer
A [Pdf layout analysis viewer](https://github.com/BobLd/DocumentLayoutAnalysis/tree/master/DocumentLayoutAnalysis/DlaViewer) is available, also relies on the [_mupdf_](https://github.com/sumatrapdfreader/sumatrapdf/tree/master/mupdf/include/mupdf) library.


![viewer](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/DocumentLayoutAnalysis/DocumentLayoutAnalysis/doc/viewer.png)
