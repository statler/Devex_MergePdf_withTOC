using DevExpress.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PdfMergeExample
{

    class Program
    {
        static void Main(string[] args)
        {
            using (PdfDocumentProcessor pdfDocumentProcessor = new PdfDocumentProcessor())
            {
                List<LinksToCreate> links = new List<LinksToCreate>();
                var lstDocs = new List<string>() { "TextMerge1.pdf", "TextMerge2.pdf", "2023 Bare root list_v3.pdf", "doc3.pdf" };
                pdfDocumentProcessor.CreateEmptyDocument("Merged.pdf");
                foreach (var fn in lstDocs)
                {
                    var l = new LinksToCreate(fn);
                    l.PageNumber = pdfDocumentProcessor.Document.Pages.Count + 1;
                    pdfDocumentProcessor.AppendDocument($@"..\..\docs\{fn}");
                    links.Add(l);
                }

                if (pdfDocumentProcessor.Document.Pages.Count > 0)
                {
                    using (PdfGraphics graph = pdfDocumentProcessor.CreateGraphics())
                    {
                        var tocManager = new TOCManager(pdfDocumentProcessor);
                        PdfRectangle pageRect = pdfDocumentProcessor.Document.Pages[0].MediaBox;
                        PdfPage newPage = pdfDocumentProcessor.InsertNewPage(1, pageRect);

                        var y = 150F.GetWorldCoordFromPage();

                        using (var font = new Font("Arial", 12, FontStyle.Underline | FontStyle.Bold))
                        {
                            tocManager.PrintLink(graph, font, null, "Document", tocManager.xPosnDescription, y, tocManager.MaxDescWidth);
                            tocManager.PrintLink(graph, font, null, "Page", tocManager.xPosnPageNumber, y, tocManager.MaxPageNumberWidth, true);
                            y += tocManager.YIncrement + 10;
                        }

                        using (var font = new Font("Arial", 11, FontStyle.Underline))
                        {

                            foreach (LinksToCreate _l in links)
                            {
                                tocManager.PrintLink(graph, font, _l.PageNumber, _l.LinkDescription, tocManager.xPosnDescription, y, tocManager.MaxDescWidth);
                                tocManager.PrintLink(graph, font, _l.PageNumber, _l.PageNumber.ToString(), tocManager.xPosnPageNumber, y, tocManager.MaxPageNumberWidth, true);
                                y += tocManager.YIncrement;
                            }
                            //--- Render a page with graphics.
                            graph.AddToPageForeground(newPage);
                        }
                    }
                }
            }
            System.Diagnostics.Process.Start("Merged.pdf");
        }

        public class TOCManager
        {
            public float LeftMargin { get; set; } = 30F;
            public float YIncrement { get; set; } = 40F;
            public float RightMargin { get; set; } = 30F;
            public float Padding { get; set; } = 20F;
            public float PageWidth { get; set; }
            public float PageLeft { get; set; }
            public float PageRight { get; set; }
            public float MaxPageNumberWidth { get; set; } = 100F;
            public float MaxDescWidth { get; set; }
            public SolidBrush brush { get; set; } = (SolidBrush)Brushes.Black;

            public float xPosnDescription => PageLeft + LeftMargin;
            public float xPosnPageNumber => PageRight - MaxPageNumberWidth - RightMargin;
            public TOCManager(PdfDocumentProcessor pdfDocumentProcessor)
            {
                PdfRectangle pageRect = pdfDocumentProcessor.Document.Pages[0].MediaBox;

                //Margins are in WorldCoord
                PageLeft = pageRect.Left.GetWorldCoordFromPage();
                PageWidth = pageRect.Width.GetWorldCoordFromPage();
                PageRight = pageRect.Right.GetWorldCoordFromPage();
                MaxDescWidth = PageRight - xPosnDescription - MaxPageNumberWidth - Padding;
            }

            public void PrintLink(PdfGraphics graph, Font font, int? PageNumber, string Text, float X, float Y, float MaxWidth, bool alignRight = false)
            {
                var _linkSize = graph.MeasureString(Text, font);
                var _linkHeight = (int)Math.Ceiling(_linkSize.Height);
                var _linkTextWidth = Math.Min(_linkSize.Width + 0.1F, MaxWidth);
                if (alignRight) X = X + MaxWidth - _linkTextWidth;
                //Draw description
                var rctDescriptionF = new RectangleF(X, Y, _linkTextWidth, _linkHeight);
                graph.DrawString(Text, font, brush, rctDescriptionF);
                //For some unknown reason, the text does not quite fit within the exact bounding box calculated by MeasureString
                if (PageNumber != null) graph.AddLinkToPage(rctDescriptionF, PageNumber.Value, 0, 0);
            }

        }

        internal class LinksToCreate
        {
            public string FileName { get; set; }
            public int PageNumber { get; set; }
            string _linkDescription;
            public string LinkDescription { get => string.IsNullOrWhiteSpace(_linkDescription) ? FileName : _linkDescription; set => _linkDescription = value; }
            public LinksToCreate(string _fileName)
            {
                FileName = _fileName;
            }
        }


    }

    public static class Ext
    {

        public static float GetWorldCoordFromPage(this float x)
        {
            return x / 72 * PdfGraphics.DefaultDpi;
        }
        public static float GetWorldCoordFromPage(this double x)
        {
            return GetWorldCoordFromPage((float)x);
        }
    }
}
