using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Kit.Services.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Formula = DocumentFormat.OpenXml.Spreadsheet.Formula;
namespace Kit.Services
{
    public class ExcelService : IDisposable
    {
        public string FilePath { get; private set; }
        public SpreadsheetDocument Document { get; private set; }

        private ExcelService(string FilePath, SpreadsheetDocument Document)
        {
            this.FilePath = FilePath;
            this.Document = Document;
        }

        public static ExcelService GenerateExcel(String fileName)
        {
            // Creating the SpreadsheetDocument in the indicated FilePath
            string FilePath = Path.Combine(Kit.Tools.Instance.LibraryPath, fileName);
            return new(FilePath, SpreadsheetDocument.Create(FilePath, SpreadsheetDocumentType.Workbook, true));
        }

        public Cell ConstructCell(object value)
        {
            if (value is Formula formula)
            {
                Cell cell = new Cell();
                CellFormula cellformula = new CellFormula();
                cellformula.Text = formula.Text;
                CellValue cellValue = new CellValue();
                cellValue.Text = "0";
                cell.Append(cellformula);
                cell.Append(cellValue);
                return cell;
            }
            return ConstructCell(value.ToString(), GetType(value));
        }

        public CellValues GetType(object value)
        {
            switch (value)
            {
                case int:
                case float:
                case decimal:
                case double:
                    return CellValues.Number;

                case string:
                    return CellValues.String;

                case TimeSpan:
                case DateTime:
                    return CellValues.Date;

                case bool:
                    return CellValues.Boolean;

                default:
                    return CellValues.String;
            }
        }

        public Cell ConstructCell(string value, CellValues dataTypes) =>
            new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataTypes)
            };

        public CalculationCell ConstructCell(string formula) =>
             new CalculationCell()
             {
                 InnerXml = formula
             };

        public ExcelService AddWorkbook(string Name = null, uint SheetId = 0)
        {
            var wbPart = this.Document.AddWorkbookPart();
            wbPart.Workbook = new Workbook();

            var part = wbPart.AddNewPart<WorksheetPart>();
            part.Worksheet = new Worksheet(new SheetData());
            if (SheetId <= 0)
            {
                SheetId = (uint)(wbPart.Workbook.Sheets?.ChildElements?.Count ?? 0) + (uint)1;
            }
            if (string.IsNullOrEmpty(Name))
            {
                Name = $"Sheet{SheetId}";
            }
            //  Here are created the sheets, you can add all the child sheets that you need.
            var sheets = wbPart.Workbook.AppendChild
                (
                   new Sheets(
                            new Sheet()
                            {
                                Id = wbPart.GetIdOfPart(part),
                                SheetId = SheetId,
                                Name = Name
                            }
                        )
                );
            return this;
        }

        private ExcelService SetStylesheet(ExcelStructure structure)
        {
            WorkbookStylesPart stylesheet = Document.WorkbookPart
                .AddNewPart<WorkbookStylesPart>();

            Stylesheet workbookstylesheet = new Stylesheet();
            //    <Fonts>
            Font font0 = new Font(); // Default font

            Font font1 = new Font(); // Bold font
            Bold bold = new Bold();
            font1.Append(bold);

            Fonts fonts = new Fonts(); // <APENDING Fonts>
            fonts.Append(font0);
            fonts.Append(font1);

            // <Fills>
            Fill fill0 = new Fill(); // Default fill

            Fills fills = new Fills(); // <APENDING Fills>
            fills.Append(fill0);
            //header fill
            fills.Append(new Fill() 
            {
                PatternFill = new PatternFill()
                {
                    PatternType = PatternValues.Solid,
                    BackgroundColor = new BackgroundColor() { Rgb = new HexBinaryValue(structure.HeaderColor) }
                }
            });

            // <Borders>
            Border border0 = new Border(); // Defualt border

            Borders borders = new Borders(); // <APENDING Borders>
            borders.Append(border0);
            borders.Append(new Border(
                    new LeftBorder(
                        new Color() { Auto = true }
                    )
                    { Style = BorderStyleValues.Thin },
                    new RightBorder(
                        new Color() { Auto = true }
                    )
                    { Style = BorderStyleValues.Thin },
                    new TopBorder(
                        new Color() { Auto = true }
                    )
                    { Style = BorderStyleValues.Thin },
                    new BottomBorder(
                        new Color() { Auto = true }
                    )
                    { Style = BorderStyleValues.Thin },
                    new DiagonalBorder()));

            // <CellFormats>
            CellFormat cellformat0 = new CellFormat()
            {
                FormatId = 0,
                FillId = 0,
                BorderId = 1
            };

            Alignment alignment = new Alignment()
            {
                Horizontal = HorizontalAlignmentValues.Center,
                Vertical = VerticalAlignmentValues.Center
            };

            CellFormat cellformat1 = new CellFormat(alignment)
            {
                FontId = 1,
                BorderId = 1,
                FillId = 1
            };

            // <APENDING CellFormats>
            CellFormats cellformats = new CellFormats();
            cellformats.Append(cellformat0);
            cellformats.Append(cellformat1);

            // Append FONTS, FILLS , BORDERS & CellFormats to stylesheet <Preserve the ORDER>
            workbookstylesheet.Append(fonts);
            workbookstylesheet.Append(fills);
            workbookstylesheet.Append(borders);
            workbookstylesheet.Append(cellformats);

            stylesheet.Stylesheet = workbookstylesheet;
            stylesheet.Stylesheet.Save();

            return this;
        }

        public ExcelService InsertDataIntoSheet(string sheetName, ExcelStructureDataTable data)
        {
            var table = data.Table;

            var wbPart = this.Document.WorkbookPart;
            var sheets = wbPart.Workbook.GetFirstChild<Sheets>().
                         Elements<Sheet>().FirstOrDefault().
                         Name = sheetName;
            SetStylesheet(data);
            var part = wbPart.WorksheetParts.First();
            var sheetData = part.Worksheet.Elements<SheetData>().First();

            Row row = null;
            if (data.MakeHeader)
            {
                row = sheetData.AppendChild(new Row());
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Cell cell = ConstructCell(table.Columns[i].ColumnName, CellValues.String);
                    cell.StyleIndex = 1U;
                    row.Append(cell);
                }
            }

            for (int r = 0; r < table.Rows.Count; r++)
            {
                row = sheetData.AppendChild(new Row());
                for (int c = 0; c < table.Columns.Count; c++)
                {
                    object obj = table.Rows[r][c];
                    row.Append(ConstructCell(obj));
                }
            }

            return this;
        }

        //public void InsertDataIntoSheet(string sheetName, ExcelListStructure data)
        //{
        //    var wbPart = this.Document.WorkbookPart;
        //    var sheets = wbPart.Workbook.GetFirstChild<Sheets>().
        //                 Elements<Sheet>().FirstOrDefault().
        //                 Name = sheetName;

        //    var part = wbPart.WorksheetParts.First();
        //    var sheetData = part.Worksheet.Elements<SheetData>().First();

        //    var row = sheetData.AppendChild(new Row());

        //    foreach (var header in data.Headers)
        //    {
        //        row.Append(ConstructCell(header, CellValues.String));
        //    }

        //    foreach (var value in data.Values)
        //    {
        //        var dataRow = sheetData.AppendChild(new Row());

        //        foreach (var dataElement in value)
        //        {
        //            dataRow.Append(ConstructCell(dataElement, CellValues.String));
        //        }
        //    }
        //    wbPart.Workbook.Save();
        //}

        public void Dispose()
        {
            this.Document.Save();
            this.Document.Close();
            this.Document.Dispose();
        }
    }
}