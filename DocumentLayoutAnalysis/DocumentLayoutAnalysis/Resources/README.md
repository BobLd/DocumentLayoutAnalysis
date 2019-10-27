# Resources
## Text extraction
- [High precision text extraction from PDF documents](Text%20extraction/High%20precision%20text%20extraction%20from%20PDF%20documents.pdf) | Øyvind Raddum Berg
- [User-Guided Information Extraction from Print-Oriented Documents](https://www.dbai.tuwien.ac.at/staff/hassan/files/dissertation.pdf) | Tamir Hassan
- [Combining Linguistic and Spatial Information for Document Analysis](https://arxiv.org/ftp/cs/papers/0009/0009014.pdf) | Aiello, Monz and Todoran
- [New Methods for Metadata Extraction from Scientific Literature](https://arxiv.org/pdf/1710.10201.pdf) | Dominika Tkaczyk
- [A System for Converting PDF Documents into Structured XML Format](https://www.researchgate.net/publication/220933081_A_System_for_Converting_PDF_Documents_into_Structured_XML_Format) | Hervé Déjean, Jean-Luc Meunier
## Page segmentation
- [Performance Comparison of Six Algorithms for Page Segmentation](Page%20segmentation/Performance%20Comparison%20of%20Six%20Algorithms%20for%20Page%20Segmentation.pdf) | Faisal Shafait, Daniel Keysers, and Thomas M. Breuel
- [A Fast Algorithm for Bottom-Up Document Layout Analysis](Page%20segmentation/A%20Fast%20Algorithm%20for%20Bottom-Up%20Document%20Layout%20Analysis.pdf) | Anikó Simon, Jean-Christophe Pret, and A. Peter Johnson
- [Empirical Performance Evaluation Methodology and its Application to Page Segmentation Algorithms: A Review](Page%20segmentation/Empirical%20Performance%20Evaluation%20Methodology%20and%20its%20Application%20to%20Page%20Segmentation%20Algorithms%20-%20Review.pdf) | Pinky Gather, Avininder Singh
- [Layout Analysis based on Text Line Segment Hypotheses](Page%20segmentation/Layout%20Analysis%20based%20on%20Text%20Line%20Segment%20Hypotheses.pdf) | Thomas M. Breuel
- [Hybrid Page Layout Analysis via Tab-Stop Detection](Page%20segmentation/tab%20stop%20-%20tesseract.pdf) | Ray Smith
- Extending the Page Segmentation Algorithms of the Ocropus Documentation Layout Analysis System | Amy Alison Winder
- [Object-Level Document Analysis of PDF Files](https://www.dbai.tuwien.ac.at/staff/hassan/files/p47-hassan.pdf) | Tamir Hassan
- [Document Image Segmentation as a Spectral Partitioning Problem](https://cdn.iiit.ac.in/cdn/cvit.iiit.ac.in/images/ConferencePapers/2008/Praveen08Document.pdf) | Dasigi, Jain and Jawahar
### Recursive XY Cut [`code`](https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig/DocumentLayoutAnalysis/RecursiveXYCut.cs)
  The X-Y cut segmentation algorithm, also referred to as recursive X-Y cuts (RXYC) algorithm, is a tree-based __top-down__ algorithm.
The root of the tree represents the entire document page. All the leaf nodes together represent the final segmentation. The RXYC algorithm __recursively splits the document into two or more smaller rectangular blocks which represent the nodes of the tree. At each step of the recursion, the horizontal and vertical projection profiles of each node are computed.__ Then, the valleys along the horizontal and vertical directions, _VX_ and _VY_, are compared to corresponding predefined thresholds _TX_ and _TY_. If the valley is larger than the threshold, the node is split at the mid-point of the wider of _VX_ and _VY_ into two children nodes. The process continues until no leaf node can be split further. Then, noise regions are removed using noise removal thresholds _TnX_ and _TnY_. [`source`](Page%20segmentation/Performance%20Comparison%20of%20Six%20Algorithms%20for%20Page%20Segmentation.pdf)
![alt text](https://github.com/BobLd/DocumentLayoutAnalysis/blob/master/DocumentLayoutAnalysis/DocumentLayoutAnalysis/doc/xy%20cut.gif)
- [Recursive X-Y Cut using Bounding Boxes of Connected Components](Page%20segmentation/Recursive%20X-Y%20Cut%20using%20Bounding%20Boxes%20of%20Connected%20Components.pdf) | Jaekyu Ha, Robert M. Haralick and Ihsin T. Phillips
### Docstrum [`code`](https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig/DocumentLayoutAnalysis/DocstrumBB.cs)
  The Docstrum algorithm by Gorman is a __bottom-up__ approach based on __nearest-neighborhood clustering__ of connected components extracted from the document image. After noise removal, the connected components are separated into two groups, one with dominant characters and another one with characters in titles and section heading, using a character size ratio factor _fd_. Then, _K_ nearest neighbors are found for each connected component. Then, text-lines are found by computing the transitive closure on within-line nearest neighbor pairings using a threshold _ft_. Finally, text-lines are merged to form text blocks using a parallel distance threshold _fpa_ and a perpendicular distance threshold _fpe_. [`source`](Page%20segmentation/Performance%20Comparison%20of%20Six%20Algorithms%20for%20Page%20Segmentation.pdf)
- The Document Spectrum for Page Layout Analysis | Lawrence O’Gorman
- [Document Structure and Layout Analysis](Page%20segmentation/Document%20Structure%20and%20Layout%20Analysis--DocStructure.pdf) | Anoop M. Namboodiri and Anil K. Jain
- [Document Layout Analysis](Page%20segmentation/Document%20Layout%20Analysis%20-%20Inside%20Mines%20-%20Hoch.pdf) | Garrett Hoch
### Voronoi
  The Voronoi-diagram based segmentation algorithm by Kise et al. is also a bottom-up algorithm. In the first step, it extracts sample points from the boundaries of the connected components using a sampling rate _sr_. Then, noise removal is done using a maximum noise zone size threshold _nm_, in addition to width, height, and aspect ratio thresholds. After that the Voronoi diagram is generated using sample points obtained from the borders of the connected components. Superfluous Voronoi edges are deleted using a criterion involving the area ratio threshold _ta_, and the inter-line spacing margin control factor _fr_. Since we evaluate all algorithms on document pages with Manhattan layouts, a modified version of the algorithm is used to generate rectangular zones.[`source`](Page%20segmentation/Performance%20Comparison%20of%20Six%20Algorithms%20for%20Page%20Segmentation.pdf)
- [Voronoi++: A Dynamic Page Segmentation approach based on Voronoi and Docstrum features](Page%20segmentation/Voronoi%2B%2B.pdf) | Mudit Agrawal and David Doermann
### Constrained text-line detection
  The layout analysis approach by Breuel finds text-lines as a two step process:
1. Find tall whitespace rectangles and evaluate them as candidates for gutters, column separators, etc. The algorithm for finding maximal empty whitespace is described in Breuel. The whitespace rectangles are returned in order of decreasing quality and are allowed a maximum overlap of _Om_.
2. The whitespace rectangles representing the columns are used as obstacles in a robust least square, globally optimal text-line detection algorithm. Then, the bounding box of all the characters making the text-line is computed.
The method was merely intended by its author as a demonstration of the application of two geometric algorithms, and not as a complete layout analysis system; nevertheless, we included it in the comparison because it has already proven useful in some applications. It is also nearly parameter free and resolution independent.[`source`](Page%20segmentation/Performance%20Comparison%20of%20Six%20Algorithms%20for%20Page%20Segmentation.pdf)
- [Two Geometric Algorithms for Layout Analysis](Page%20segmentation/Two%20Geometric%20Algorithms%20for%20Layout%20Analysis--breuel-das.pdf) | Thomas M. Breuel
- [High precision text extraction from PDF documents](Text%20extraction/High%20precision%20text%20extraction%20from%20PDF%20documents.pdf) | Øyvind Raddum Berg
- High Performance Document Layout Analysis | Thomas M. Breuel
### PDF/A standard
[PDF/A-1a](https://en.wikipedia.org/wiki/PDF/A#PDF/A-1) compliant document make the following information available:
  - Language specification
  - Hierarchical document structure
  - Tagged text spans and descriptive text for images and symbols
  - Character mappings to Unicode
## Table extraction
- [Extracting Tables from Documents using Conditional Generative Adversarial Networks and Genetic Algorithms](Table%20extraction/Extracting%20Tables%20from%20Documents%20using%20Conditional%20Generative%20Adversarial%20Networks%20and%20Genetic%20Algorithms.pdf) | Nataliya Le Vine, Matthew Zeigenfuse, Mark Rowany
- [Detecting Table Region in PDF Documents Using Distant Supervision](Table%20extraction/Detecting%20Table%20Region%20in%20PDF%20Documents%20Using%20Distant%20Supervision.pdf) | Miao Fan and Doo Soon Kim
- [Automatic Tabular Data Extraction and Understanding](Table%20extraction/Automatic%20Tabular%20Data%20Extraction%20and%20Understanding.pdf) | Roya Rastan
- [Algorithmic Extraction of Data in Tables in PDF Documents](Table%20extraction/Algorithmic%20Extraction%20of%20Data%20in%20Tables%20in%20Pdf%20Documents%20-%20Nurminen.pdf) | Anssi Nurminen
- [A Multi-Layered Approach to Information Extraction from Tables in Biomedical Documents](https://www.research.manchester.ac.uk/portal/files/70405100/FULL_TEXT.PDF) | Nikola Milosevic
- [Integrating and querying similar tables from PDF documentsusing deep learning](https://arxiv.org/pdf/1901.04672.pdf) | Rahul Anand, Hye-young Paik and Chen Wang
- [Locating Tables in Scanned Documents for Reconstructing and Republishing ](https://arxiv.org/search/?query=document+layout+analysis&searchtype=all&source=header) | MAC Akmal Jahan, Roshan G Ragel 
### Systems
- [pdf2table: A Method to Extract Table Information from PDF Files](Table%20extraction/pdf2table.pdf) | Burcu Yildiz, Katharina Kaiser, and Silvia Miksch
- [PDF-TREX: An Approach for Recognizing and Extracting Tables from PDF Documents](Table%20extraction/PDF-TREX.pdf) | Ermelinda Oro, Massimo Ruffolo
- [TAO: System for Table Detection and Extraction from PDF Documents](https://pdfs.semanticscholar.org/22c9/f2d80e0f0546a54c82dbc9cfc9e68ce9a1ff.pdf) | Martha O. Perez-Arriaga, Trilce Estrada, and Soraya Abad-Mota
### Sparse line
- [Identifying Table Boundaries in Digital Documents via Sparse Line Detection](Table%20extraction/Identifying%20Table%20Boundaries%20in%20Digital%20Documents%20via%20Sparse%20Line%20Detection.pdf) | Ying Liu, Prasenjit Mitra, C. Lee Giles
- [A Fast Preprocessing Method for Table Boundary Detection: Narrowing Down the Sparse Lines using Solely Coordinate Information](Table%20extraction/A%20Fast%20Preprocessing%20Method%20for%20Table%20Boundary%20Detection%20-%20Narrowing%20Down%20the%20Sparse%20Lines%20using%20Solely%20Coordinate%20Information.pdf) | Ying Liu, Prasenjit Mitra, C. Lee Giles
- [Improving the Table Boundary Detection in PDFs by Fixing the Sequence Error of the Sparse Lines](Table%20extraction/Improving%20the%20Table%20Boundary%20Detection%20in%20PDFs%20by%20Fixing%20the%20Sequence%20Error%20of%20the%20Sparse%20Lines.pdf) | Ying Liu, Kun Bai, Prasenjit Mitra, C. Lee Giles
- [Automatic Table Ground Truth Generation and A Background-analysis-based Table Structure Extraction Method](Table%20extraction/Automatic%20Table%20Ground%20Truth%20Generation%20and%20A%20Background-analysis-based%20Table%20Structure%20Extraction%20Method.pdf) | Yalin Wangt, Ihsin T. Phillips and Robert Haralickt
## Chart and diagram extraction
- [Extraction, layout analysis and classification of diagrams in PDF documents](Chart%20and%20diagram%20extraction/Extraction%2C%20layout%20analysis%20and%20classification%20of%20diagrams%20in%20PDF%20documents.pdf) | Robert P. Futrelle, Mingyan Shao, Chris Cieslik and Andrea Elaina Grimes
- [Graphics Recognition in PDF documents](https://pdfs.semanticscholar.org/9c18/d90f1988d1d98f061bdd076d56983a82803d.pdf) | Mingyan Shao and Robert P. Futrelle
- [A Study on the Document Zone Content Classification Problem](http://gsl.lab.asu.edu/doc/zonedas02.pdf)
 | Yalin Wang, Ihsin T. Phillips, and Robert M. Haralick
- Text/Figure Separation in Document Images Using Docstrum Descriptor and Two-Level Clustering | Valery Anisimovskiy, Ilya Kurilin, Andrey Shcherbinin, Petr Pohl
- [CHART-Synthetic](https://github.com/adobe-research/CHART-Synthetic)
## Mathematical expression extraction
- [A Font Setting Based Bayesian Model to Extract Mathematical Expression in PDF Files](https://www.researchgate.net/publication/322779502_A_Font_Setting_Based_Bayesian_Model_to_Extract_Mathematical_Expression_in_PDF_Files) | Xing Wang, Jyh-Charn Liu
- [Mathematical Formula Identification in PDF Documents](https://www.researchgate.net/publication/220860588_Mathematical_Formula_Identification_in_PDF_Documents) | Xiaoyan Lin, Liangcai Gao, Zhi Tang, Xiaofan Lin
- [Faithful Mathematical Formula Recognition from PDF Documents](https://www.cs.bham.ac.uk/~jbb/documents/das10.pdf) | Josef B. Baker, Alan P. Sexton and Volker Sorge
- [Extracting Precise Data from PDF Documents for Mathematical Formula Recognition](https://www.cs.bham.ac.uk/~jbb/documents/das08.pdf) | Josef B. Baker, Alan P. Sexton and Volker Sorge
- [Mathematical formula identification and performance evaluation in PDF documents](https://link.springer.com/article/10.1007%2Fs10032-013-0216-1) | Xiaoyan Lin, Liangcai Gao, Zhi Tang, Josef Baker, Volker Sorge
## Margins recognition
- [Finding blocks of text in an image using Python, OpenCV and numpy](https://www.danvk.org/2015/01/07/finding-blocks-of-text-in-an-image-using-python-opencv-and-numpy.html)
- [Notes on the margins: how to extract them using image segmentation, Google Vision API, and R](http://travelerslab.research.wesleyan.edu/2016/09/13/notes-on-the-margins/)
- [A mixed approach to auto-detection of page body](https://www.researchgate.net/publication/221253859_A_mixed_approach_to_auto-detection_of_page_body) | Liangcai Gao, Zhi Tang, Ruiheng Qiu
- [Header and Footer Extraction by Page-Association](https://www.hpl.hp.com/techreports/2002/HPL-2002-129.pdf) | Xiaofan Lin
## Image related
- [Analysing layout information: searching PDF documents for pictures](https://pdfs.semanticscholar.org/2bec/a8fcdbdcbe02bc30d40113432daed61f9660.pdf) | B. Mathiak et al.
# Other
## Shape detection
- [Polygon Detection from a Set of Lines](https://web.ist.utl.pt/alfredo.ferreira/publications/12EPCG-PolygonDetection.pdf) |  Alfredo Ferreira, Manuel J. Fonseca, Joaquim A. Jorge
- [A Simple Approach to Recognise Geometric Shapes Interactively](http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.24.2707&rep=rep1&type=pdf) | Joaquim A. Jorge and Manuel J. Fonseca
- [The Detection of Rectangular Shape Objects Using Matching Schema](https://pdfs.semanticscholar.org/2a73/b9533532b7426fd5a7c47ac3f40e0dc88d7f.pdf) | Soo-Young Ye, Joon-Young Choi and Ki-Gon Nam
- [Edge Detection Based Shape Identification](https://arxiv.org/ftp/arxiv/papers/1604/1604.02030.pdf) | Vivek Kumar, Sumit Pandey, Amrindra Pal, Sandeep Sharma
- [Algorithms for the Reduction of the Number of Points Required to Represent a Digitized Line or its Caricature](http://www2.ipcku.kansai-u.ac.jp/~yasumuro/M_InfoMedia/paper/Douglas73.pdf) | David H. Douglas and Thomas K. Peucker
- [Shape description using cubic polynomial Bezier curves](https://www.sciencedirect.com/science/article/abs/pii/S0167865598000695) | L. Cinque, S. Levialdi, A. Malizia
- [New Algorithm for Medial Axis Transform of Plane Domain](https://pdfs.semanticscholar.org/70ae/5b583303af0b4d60d356d08f8ed84e1babbc.pdf) and details from [stackoverflow](https://stackoverflow.com/questions/29921826/how-do-i-calculate-the-medial-axis-for-a-2d-vector-shape) | Choi, Choi, Moon and Wee
## Character Recognition
### Bézier curves
- [RNN-Based Handwriting Recognition in Gboard](https://ai.googleblog.com/2019/03/rnn-based-handwriting-recognition-in.html) | Sandro Feuz and Pedro Gonnet | [`arxiv`](https://arxiv.org/abs/1902.10525)
- [Handwritten Arabic Digits Recognition Using Bézier Curves](http://ijcsi.org/papers/IJCSI-10-5-2-257-263.pdf) | Aissa Kerkour El Miad and Azzeddine Mazroui
## Layout Similarity
- [A Retrieval Framework and Implementation for Electronic Documents with Similar Layouts](https://arxiv.org/ftp/arxiv/papers/1810/1810.07237.pdf) | Chung
# Datasets
- [PubLayNet: largest dataset ever for document layout analysis](https://arxiv.org/pdf/1908.07836.pdf) | Zhong, Tang and Yepes | [`github`](https://github.com/ibm-aur-nlp/PubLayNet)
