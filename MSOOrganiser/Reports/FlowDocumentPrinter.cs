using MSOOrganiser.UIUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MSOOrganiser.Reports
{
    public class FlowDocumentPrinter
    {
        public void PrintFlowDocument(Func<FlowDocument> documentProvider,
            PageOrientation pageOrientation = PageOrientation.Portrait,
            bool includeFooter = true)
        {
            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                using (new SpinnyCursor())
                {
                    var doc = documentProvider();

                    DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                    FooteredDocumentPaginator wrapper = new FooteredDocumentPaginator(paginator,
                        paginator.PageSize, new Size(0.0, 0.0), includeFooter);
                    dlg.PrintTicket.PageOrientation = pageOrientation;
                    dlg.PrintDocument(wrapper, "MSO Report");
                }
            }
        }

        public void PrintFlowDocuments(Func<IEnumerable<FlowDocument>> documentProvider,
            PageOrientation pageOrientation = PageOrientation.Portrait)
        {
            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                using (new SpinnyCursor())
                {
                    foreach (var doc in documentProvider())
                    {
                        DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                        FooteredDocumentPaginator wrapper = new FooteredDocumentPaginator(paginator,
                            paginator.PageSize, new Size(0.0, 0.0));
                        dlg.PrintTicket.PageOrientation = pageOrientation;
                        dlg.PrintDocument(wrapper, "MSO Report");
                    }
                }
            }
        }
    }


    public class FooteredDocumentPaginator : DocumentPaginator
    {
        private readonly Size _pageSize;
        private readonly Size _margin;
        private readonly DocumentPaginator _originalPaginator;
        private readonly Typeface _typeface = new Typeface("Verdana");
        private readonly DateTime _printingDate;
        private readonly double _scaleFactor = 1.0;
        private readonly bool _includeFooter = true;

        public FooteredDocumentPaginator(DocumentPaginator paginator, Size pageSize, Size margin, bool includeFooter = true)
        {
            _pageSize = pageSize;
            _margin = margin;
            _originalPaginator = paginator;
            _originalPaginator.PageSize = new Size(_pageSize.Width - 2 * margin.Width,
                                    _pageSize.Height - 2 * margin.Height);
            _printingDate = DateTime.Now;
            _includeFooter = includeFooter;
        }

        Rect Move(Rect rect)
        {
            if (rect.IsEmpty)
                return rect;
            else
                return new Rect(rect.Left + _margin.Width, rect.Top + _margin.Height,

                                rect.Width, rect.Height);
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            DocumentPage page = _originalPaginator.GetPage(pageNumber);
            // Create a wrapper visual for transformation and add extras
            ContainerVisual newpage = new ContainerVisual();
            DrawingVisual title = new DrawingVisual();
            using (DrawingContext ctx = title.RenderOpen())
            {
                FormattedText text = new FormattedText("Page " + (pageNumber + 1) + " of " + PageCount,
                    System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    _typeface, 8, Brushes.Black);
                FormattedText dateText = new FormattedText("Printed at " + _printingDate.ToString("ddd dd MMM yyyy, HH:mm"),
                    System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    _typeface, 8, Brushes.Black);


                // Coordinates are in 96ths of an inch,
                ctx.DrawText(dateText, new Point(_pageSize.Width - 160, _pageSize.Height - 16));
                ctx.DrawText(text, new Point(24, _pageSize.Height - 16));
            }

            // This was for doing a background, but it wastes ink
            DrawingVisual background = new DrawingVisual();
            using (DrawingContext ctx = background.RenderOpen())
            {
                // ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(240, 240, 240)), null, page.ContentBox);
            }
            //  newpage.Children.Add(background); // Scale down page and center

            ContainerVisual smallerPage = new ContainerVisual();
            smallerPage.Children.Add(page.Visual);

            // Shrink the real content
            var indentScale = (1.0 - _scaleFactor) / 2;
            smallerPage.Transform = new MatrixTransform(_scaleFactor, 0, 0, _scaleFactor,
                indentScale * page.ContentBox.Width, indentScale * page.ContentBox.Height);

            newpage.Children.Add(smallerPage);
            if (_includeFooter)
                newpage.Children.Add(title);
            newpage.Transform = new TranslateTransform(_margin.Width, _margin.Height);
            return new DocumentPage(newpage, _pageSize, Move(page.BleedBox), Move(page.ContentBox));
        }

        public override bool IsPageCountValid
        {
            get
            {
                return _originalPaginator.IsPageCountValid;
            }
        }

        public override int PageCount
        {
            get
            {
                return _originalPaginator.PageCount;
            }
        }

        public override Size PageSize
        {
            get
            {
                return _originalPaginator.PageSize;
            }
            set
            {
                _originalPaginator.PageSize = value;
            }
        }

        public override IDocumentPaginatorSource Source
        {
            get
            {
                return _originalPaginator.Source;
            }
        }
    }
}
