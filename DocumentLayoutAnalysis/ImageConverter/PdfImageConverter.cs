using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using CC = System.Runtime.InteropServices.CallingConvention;

/*************************************************************************************************
 * https://www.codeproject.com/articles/498317/rendering-pdf-documents-with-mupdf-and-p-invoke-in
 * https://github.com/wmjordan/mupdf/blob/master/MupdfSharp/Program.cs
 * https://github.com/reliak/moonpdf/blob/master/src/MoonPdfLib/MuPdf/MuPdfWrapper.cs
 * lib: https://github.com/sumatrapdfreader/sumatrapdf/tree/master/mupdf/include/mupdf/fitz
 *************************************************************************************************/

namespace ImageConverter
{
    public class PdfImageConverter : IDisposable
    {
        private string _path;
        //private float _zoom;
        private IntPtr _ctx;
        private IntPtr _stm;
        private IntPtr _doc;

        /// <summary>
        /// The number of pages in the document.
        /// </summary>
        public int PagesCount { get; }

        public PdfImageConverter(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("PdfImageConverter(): " + path);
            }

            _path = path;
            //_zoom = zoom;
            _ctx = NativeMethods.NewContext(); // Creates the context
            _stm = NativeMethods.OpenFile(_ctx, _path); // opens file test.pdf as a stream
            _doc = NativeMethods.OpenDocumentStream(_ctx, _stm); // opens the document
            PagesCount = NativeMethods.CountPages(_doc); // gets the number of pages in the document
        }

        public void Dispose()
        {
            NativeMethods.CloseDocument(_doc); // releases the resources
            NativeMethods.CloseStream(_stm);
            NativeMethods.FreeContext(_ctx);
        }

        /// <summary>
        /// Get the page with the specified page number (1 indexed).
        /// </summary>
        /// <param name="pageNumber">The number of the page to return, this starts from 1.</param>
        /// <returns></returns>
        public Bitmap GetPage(int pageNumber, float zoom = 1.0f)
        {
            if (pageNumber <= 0 || pageNumber > PagesCount)
            {
                throw new ArgumentOutOfRangeException("pageNumber");
            }

            if (zoom <= 0)
            {
                throw new ArgumentException("PdfImageConverter(): zoom must be positive and bigger than 0.");
            }

            IntPtr p = NativeMethods.LoadPage(_doc, pageNumber - 1); // loads the page (first page number is 0)
            RectangleMu b = new RectangleMu();
            NativeMethods.BoundPage(_doc, p, ref b); // gets the page size
            var page = RenderPage(_ctx, _doc, p, b, zoom, zoom);
            NativeMethods.FreePage(_doc, p); // releases the resources consumed by the page
            return page;
        }

        private enum ColorSpace
        {
            Rgb,
            Bgr,
            Cmyk,
            Gray
        }

        public static int GetBitmapHeight(RectangleMu pageBound, float zoomY = 1.0f)
        {
            return (int)(zoomY * (pageBound.Bottom - pageBound.Top));
        }

        public static int GetBitmapWidth(RectangleMu pageBound, float zoomX = 1.0f)
        {
            return (int)(zoomX * (pageBound.Right - pageBound.Left));
        }

        #region 32/64bit
        public static IEnumerable<Bitmap> GetBitmapPagesEnum(string path, float zoom = 1.0f)
        {
            IntPtr ctx = NativeMethods.NewContext(); // Creates the context
            IntPtr stm = NativeMethods.OpenFile(ctx, path); // opens file test.pdf as a stream
            IntPtr doc = NativeMethods.OpenDocumentStream(ctx, stm); // opens the document
            int pn = NativeMethods.CountPages(doc); // gets the number of pages in the document

            for (int i = 0; i < pn; i++) // iterate through each pages
            {
                IntPtr p = NativeMethods.LoadPage(doc, i); // loads the page (first page number is 0)
                RectangleMu b = new RectangleMu();
                NativeMethods.BoundPage(doc, p, ref b); // gets the page size
                var page = RenderPage(ctx, doc, p, b, zoom, zoom);
                NativeMethods.FreePage(doc, p); // releases the resources consumed by the page
                yield return page;
            }
            NativeMethods.CloseDocument(doc); // releases the resources
            NativeMethods.CloseStream(stm);
            NativeMethods.FreeContext(ctx);
        }

        public static Bitmap[] GetBitmapPages(string path, float zoom = 1.0f)
        {
            IntPtr ctx = NativeMethods.NewContext(); // Creates the context
            IntPtr stm = NativeMethods.OpenFile(ctx, path); // opens file test.pdf as a stream
            IntPtr doc = NativeMethods.OpenDocumentStream(ctx, stm); // opens the document
            int pn = NativeMethods.CountPages(doc); // gets the number of pages in the document
            Bitmap[] images = new Bitmap[pn];

            for (int i = 0; i < pn; i++) // iterate through each pages
            {
                IntPtr p = NativeMethods.LoadPage(doc, i); // loads the page (first page number is 0)
                RectangleMu b = new RectangleMu();
                NativeMethods.BoundPage(doc, p, ref b); // gets the page size
                images[i] = RenderPage(ctx, doc, p, b, zoom, zoom);

                NativeMethods.FreePage(doc, p); // releases the resources consumed by the page
            }
            NativeMethods.CloseDocument(doc); // releases the resources
            NativeMethods.CloseStream(stm);
            NativeMethods.FreeContext(ctx);
            return images;
        }

        private static Bitmap RenderPage(IntPtr context, IntPtr document, IntPtr page, RectangleMu pageBound, float zoomX = 1.0f, float zoomY = 1.0f)
        {
            MatrixMu ctm = new MatrixMu();
            IntPtr pix = IntPtr.Zero;
            IntPtr dev = IntPtr.Zero;

            // gets the size of the scaled page
            int width = GetBitmapWidth(pageBound, zoomX); // (int)(zoomX * (pageBound.Right - pageBound.Left)); 
            int height = GetBitmapHeight(pageBound, zoomY); // (int)(zoomY * (pageBound.Bottom - pageBound.Top));

            ctm.A = zoomX;
            ctm.D = zoomY; // sets the matrix as (zoomX,0,0,zoomY,0,0) 

            // creates a pixmap the same size as the width and height of the page
            pix = NativeMethods.NewPixmap(context, NativeMethods.LookupDeviceColorSpace(context, "DeviceRGB"), width, height);
            // sets white color as the background color of the pixmap
            NativeMethods.ClearPixmap(context, pix, 0xFF);

            // creates a drawing device
            dev = NativeMethods.NewDrawDevice(context, pix);
            // draws the page on the device created from the pixmap
            NativeMethods.RunPage(document, page, dev, ref ctm, IntPtr.Zero);

            NativeMethods.FreeDevice(dev); // frees the resources consumed by the device
            dev = IntPtr.Zero;

            // creates a colorful bitmap of the same size of the pixmap
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            //bmp.SetResolution(300, 300);
            var imageData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            unsafe
            { // converts the pixmap data to Bitmap data
              // gets the rendered data from the pixmap
                byte* ptrSrc = (byte*)NativeMethods.GetSamples(context, pix);
                byte* ptrDest = (byte*)imageData.Scan0;
                for (int y = 0; y < height; y++)
                {
                    byte* pl = ptrDest;
                    byte* sl = ptrSrc;
                    for (int x = 0; x < width; x++)
                    {
                        //Swap these here instead of in MuPDF because most pdf images will be rgb or cmyk.
                        //Since we are going through the pixels one by one
                        //anyway swap here to save a conversion from rgb to bgr.
                        pl[2] = sl[0]; //b-r
                        pl[1] = sl[1]; //g-g
                        pl[0] = sl[2]; //r-b
                                       //sl[3] is the alpha channel, we will skip it here
                        pl += 3;
                        sl += 4;
                    }
                    ptrDest += imageData.Stride;
                    ptrSrc += width * 4;
                }
            }
            // free bitmap in memory
            bmp.UnlockBits(imageData);
            NativeMethods.DropPixmap(context, pix);
            return bmp;
        }

        private static class NativeMethods
        {
            public static bool Is64bit => IntPtr.Size == 8;
            const uint FZ_STORE_DEFAULT = 256 << 20;
            const string DLL = "libmupdf.dll";
            const string MuPDFVersion = "1.6";

            static NativeMethods()
            {
                var subfolder = Is64bit ? "x64" : "x86";
                LoadLibrary(subfolder + @"\" + DLL);
            }

            [DllImport("kernel32.dll")]
            private static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport(DLL, EntryPoint = "fz_new_context_imp", CallingConvention = CC.Cdecl, BestFitMapping = false)]
            static extern IntPtr NewContext(IntPtr alloc, IntPtr locks, uint max_store, [MarshalAs(UnmanagedType.LPStr)] string fz_version);

            public static IntPtr NewContext()
            {
                return NewContext(IntPtr.Zero, IntPtr.Zero, FZ_STORE_DEFAULT, MuPDFVersion);
            }

            [DllImport(DLL, EntryPoint = "fz_free_context", CallingConvention = CC.Cdecl)]
            public static extern IntPtr FreeContext(IntPtr ctx);

            [DllImport(DLL, EntryPoint = "fz_open_file_w", CharSet = CharSet.Unicode, CallingConvention = CC.Cdecl)]
            public static extern IntPtr OpenFile(IntPtr ctx, string fileName);

            [DllImport(DLL, EntryPoint = "pdf_open_document_with_stream", CallingConvention = CC.Cdecl)]
            public static extern IntPtr OpenDocumentStream(IntPtr ctx, IntPtr stm);

            [DllImport(DLL, EntryPoint = "fz_close", CallingConvention = CC.Cdecl)]
            public static extern IntPtr CloseStream(IntPtr stm);

            [DllImport(DLL, EntryPoint = "pdf_close_document", CallingConvention = CC.Cdecl)]
            public static extern IntPtr CloseDocument(IntPtr doc);

            [DllImport(DLL, EntryPoint = "pdf_count_pages", CallingConvention = CC.Cdecl)]
            public static extern int CountPages(IntPtr doc);

            [DllImport(DLL, EntryPoint = "pdf_bound_page", CallingConvention = CC.Cdecl)]
            public static extern void BoundPage(IntPtr doc, IntPtr page, ref RectangleMu bound);

            [DllImport(DLL, EntryPoint = "fz_clear_pixmap_with_value", CallingConvention = CC.Cdecl)]
            public static extern void ClearPixmap(IntPtr ctx, IntPtr pix, int byteValue);

            [DllImport(DLL, EntryPoint = "fz_lookup_device_colorspace", CallingConvention = CC.Cdecl)]
            public static extern IntPtr LookupDeviceColorSpace(IntPtr ctx, string colorspace);

            [DllImport(DLL, EntryPoint = "fz_free_device", CallingConvention = CC.Cdecl)]
            public static extern void FreeDevice(IntPtr dev);

            [DllImport(DLL, EntryPoint = "pdf_free_page", CallingConvention = CC.Cdecl)]
            public static extern void FreePage(IntPtr doc, IntPtr page);

            [DllImport(DLL, EntryPoint = "pdf_load_page", CallingConvention = CC.Cdecl)]
            public static extern IntPtr LoadPage(IntPtr doc, int pageNumber);

            [DllImport(DLL, EntryPoint = "fz_new_draw_device", CallingConvention = CC.Cdecl)]
            public static extern IntPtr NewDrawDevice(IntPtr ctx, IntPtr pix);

            [DllImport(DLL, EntryPoint = "fz_new_pixmap", CallingConvention = CC.Cdecl)]
            public static extern IntPtr NewPixmap(IntPtr ctx, IntPtr colorspace, int width, int height);

            [DllImport(DLL, EntryPoint = "pdf_run_page", CallingConvention = CC.Cdecl)]
            public static extern void RunPage(IntPtr doc, IntPtr page, IntPtr dev, ref MatrixMu transform, IntPtr cookie);

            [DllImport(DLL, EntryPoint = "fz_drop_pixmap", CallingConvention = CC.Cdecl)]
            public static extern void DropPixmap(IntPtr ctx, IntPtr pix);

            [DllImport(DLL, EntryPoint = "fz_pixmap_samples", CallingConvention = CC.Cdecl)]
            public static extern IntPtr GetSamples(IntPtr ctx, IntPtr pix);
        }
        #endregion
    }
}
