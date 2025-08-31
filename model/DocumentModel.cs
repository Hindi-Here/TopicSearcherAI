using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;

namespace ThemeBuilder.model
{
    internal class DocumentModel : IDefault<DocumentModel>
    {
        public FileFormat Format { get; set; }
        public DocumentMargins? Margin { get; set; }
        public HorizontalAlignment Aligment { get; set; }
        public float FirstLine { get; set; }
        public float LineSpacing { get; set; }
        public string? FontFamily { get; set; }
        public float FontSize { get; set; }

        public static DocumentModel SetDefault()
        {
            return new DocumentModel
            {
                Format = FileFormat.Docx,
                Margin = new DocumentMargins
                {
                    Left = 3,
                    Top = 2,
                    Right = 1.5f,
                    Bottom = 2
                },
                Aligment = HorizontalAlignment.Justify,
                FirstLine = 1.25f,
                LineSpacing = 1.5f,
                FontFamily = "Times New Roman",
                FontSize = 12
            };
        }

        public override string ToString()
        {
            return $"   Формат документа: {Format}\n" +
                   $"   Отступы от краев: {Margin}\n" +
                   $"   Выравнивание: {Aligment}\n" +
                   $"   Отступ первой строки: {FirstLine}\n" +
                   $"   Межстрочный интервал: {LineSpacing}\n" +
                   $"   Шрифт: {FontFamily}\n" +
                   $"   Размер шрифта: {FontSize}\n";
        }
    }

    public class DocumentMargins
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public override string ToString() => $"{Left}; {Top}; {Right}; {Bottom}";
    }

    internal static class DocumentBuilder
    {
        public static void Build(DocumentModel model, string content, string theme)
        {
            Document doc = new();
            Section section = doc.AddSection();

            DocumentMargins? margins = model.Margin;
            section.PageSetup.Margins.Left = ToPoint(margins!.Left);
            section.PageSetup.Margins.Right = ToPoint(margins.Right);
            section.PageSetup.Margins.Top = ToPoint(margins.Top);
            section.PageSetup.Margins.Bottom = ToPoint(margins.Bottom);

            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.HorizontalAlignment = model.Aligment;
            paragraph.Format.FirstLineIndent = ToPoint(model.FirstLine);
            paragraph.Format.LineSpacing = model.LineSpacing * 12f;

            TextRange text = paragraph.AppendText(content);
            text.CharacterFormat.FontName = model.FontFamily;
            text.CharacterFormat.FontSize = model.FontSize;

            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string file = Path.Combine(desktopFolder, $"{theme}.docx");

            doc.SaveToFile(file, model.Format);
            doc.Close();
        }

        private static float ToPoint(float centimer) => centimer * 28.3464567f;
    }
}
