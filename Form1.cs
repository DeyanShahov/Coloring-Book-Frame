using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using static System.Net.Mime.MediaTypeNames;

namespace Coloring_Book_Frame
{
    public partial class Form1 : Form
    {
        bool includeAdditionInfo = true;
        bool includeStoryName = true;
        bool includeTitleInText = true;
        bool pictureStretch = false;
        bool includeBorderOnImage = true;
        bool isVerticalOriented = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolderPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnCreatePDF_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFolderPath.Text) || !Directory.Exists(txtFolderPath.Text))
            {
                MessageBox.Show("Моля, изберете валидна папка.", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string outputPath = Path.Combine(txtFolderPath.Text, "high_res_output.pdf");

            try
            {
                CreateHighResPDF(txtFolderPath.Text, outputPath);
                MessageBox.Show($"PDF файлът е създаден успешно: {outputPath}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Възникна грешка: {ex.Message}", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void CreateHighResPDF(string folderPath, string outputPath)
        {         
            using (var document = new Document(PageSize.A4))
            using (var writer = PdfWriter.GetInstance(document, new FileStream(outputPath, FileMode.Create)))
            {
                document.Open();

                // Read URLs from the input field and split them into a list
                string urlListContent = txtUrlList.Text.Trim();
                string[] urlList = urlListContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                int currentLinkIndex = 0;

                // Read texts from the input field and split them into a list
                string textListContent = txtTextList.Text.Trim();
                string[] textList = textListContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                int currentTextIndex = 0;

                // Check for Title name in text
                if (includeTitleInText)
                { 
                    // Filter collection if need to add Title name
                    textList = textList.Where((f, index) => includeStoryName || index % 2 == 1).ToArray();
                };


                // Get all image files in the folder
                var imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".jpeg"))
                    .OrderBy(f => ExtractNumber(Path.GetFileNameWithoutExtension(f))) // Sort by extracted number
                    .ToList();

                // Get only first 30 images, if phots is more 
                if (imageFiles.Count > 30)
                {
                    imageFiles = imageFiles.Take(30).ToList();
                }

                // Loop through all image files in the folder
                foreach (string filePath in imageFiles)
                {
                    // Resize and convert the image to iTextSharp's format
                    using (var img = ResizeImage(System.Drawing.Image.FromFile(filePath), isVerticalOriented ? 2480 : 3560, isVerticalOriented ? 3508 : 2480))
                    {
                        //------------------------------------ Visual elements ----------------------------------------

                        //------------------------------------------ Image --------------------------------------------
                        if (includeBorderOnImage)
                        {
                            var pdfImage = iTextSharp.text.Image.GetInstance(ImageToByteArray(img));
                           
                            //pdfImage.ScaleToFit(PageSize.A4.Width - 25, PageSize.A4.Height);

                            if (isVerticalOriented)
                            {
                                pdfImage.SetAbsolutePosition(13, 16);
                                pdfImage.ScaleToFit(PageSize.A4.Width - 25, PageSize.A4.Height);
                            } else
                            {
                                // Завъртете страницата на 90 градуса
                                document.SetPageSize(PageSize.A4.Rotate());

                                pdfImage.SetAbsolutePosition(15, 15);
                                pdfImage.ScaleToFit(PageSize.A4.Height - 27, PageSize.A4.Width - 27);
                            }                                              

                            // Add a new page and insert the image
                            document.NewPage();

                            //DrawShadow(writer);
                            DrawSimpleShadow(writer, isVerticalOriented);

                            // Then, draw the black frame
                            DrawBlackFrame(writer, isVerticalOriented);

                            document.Add(pdfImage);
                        }                                                                            

                        //------------------------------ Second Image Page Full Strech --------------------------------
                        if (pictureStretch)
                        {
                            var pdfImage2 = iTextSharp.text.Image.GetInstance(ImageToByteArray(img));
                            pdfImage2.SetAbsolutePosition(0, 0);
                            pdfImage2.ScaleToFit(isVerticalOriented ? PageSize.A4.Width : PageSize.A4.Height, isVerticalOriented ? PageSize.A4.Height : PageSize.A4.Width);

                            document.NewPage();
                            document.Add(pdfImage2);
                        }
                    }

                    // Add a new page for the Title, Text, QR code, link
                    if (includeAdditionInfo)
                    {                       
                        document.NewPage();                  

                        // ---------------------------------------- Title ---------------------------------------------
                        if (includeTitleInText && includeStoryName)
                        {
                            CreateTitleContent(writer, textList, currentTextIndex, isVerticalOriented);
                            currentTextIndex++;
                        }

                        // ---------------------------------------- Text ----------------------------------------------

                        CreateTextContent(writer, textList, currentTextIndex, isVerticalOriented);
                        currentTextIndex++;

                        //----------------------------------------- QR Cod --------------------------------------------

                        CreateQrAndLinks(document, writer, urlList, ref currentLinkIndex, isVerticalOriented);

                        //------------------------------------ Visual elements ----------------------------------------

                        //DrawShadow(writer);
                        DrawSimpleShadow(writer, isVerticalOriented);

                        // Then, draw the black frame 
                        DrawBlackFrame(writer, isVerticalOriented);
                    }                
                }

                document.Close();
            }
        }

        private void CreateQrAndLinks(Document document, PdfWriter writer, string[] urlList, ref int currentLinkIndex, bool isVertical)
        {
            // Define positions for the 3 QR codes in millimeters
            float[] qrXPositions = isVertical ? new float[] { 25f, 86f, 147f } : new float[] { 64f, 137f, 212f }; // in mm
            float qrYPositionFromBottom = isVertical ? 227f : 140f; // in mm (from bottom of the page)

            // Convert mm to points (1mm = 2.83465 points)
            float mmToPoints = 2.83465f;
            float qrCodeSize = 37f * mmToPoints; // 37mm in points

            // Calculate Y position relative to the bottom-left corner of the page
            float pageHeight = isVertical ? PageSize.A4.Height : PageSize.A4.Width; // A4 page height in points
            float qrYPosition = pageHeight - (qrCodeSize * 1.5f) - (qrYPositionFromBottom * mmToPoints);



            for (int i = 0; i < 3; i++)
            {
                // Calculate the x and y positions for the QR code
                float xPos = qrXPositions[i] * mmToPoints;
                float yPos = qrYPosition;  // QR code's Y position (calculated from the bottom)

                // Generate and insert QR code image
                using (var qrCodeImage = GenerateQRCode(urlList[currentLinkIndex]))
                {
                    var qrPdfImage = iTextSharp.text.Image.GetInstance(qrCodeImage, ImageFormat.Png);
                    qrPdfImage.SetAbsolutePosition(xPos, yPos);  // Set the position
                    qrPdfImage.ScaleAbsolute(qrCodeSize, qrCodeSize);  // Set the size
                    document.Add(qrPdfImage);
                }

                // Calculate the center of the QR code for centering the link
                float qrCenter = xPos + (qrCodeSize / 2);

                // Add the centered link above the QR code
                AddCenteredLink(document, writer, urlList[currentLinkIndex], qrCenter, yPos + qrCodeSize + 20, i); // Adjust the position as needed

                currentLinkIndex++;
            }     
        }

        private static void CreateTitleContent(PdfWriter writer, string[] textList, int currentTextIndex, bool isVertical)
        {
            var baseFont = BaseFont.CreateFont("C:\\Users\\redfo\\AppData\\Local\\Microsoft\\Windows\\Fonts\\YesevaOne-Regular.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new iTextSharp.text.Font(baseFont, 40);
            font.SetColor(0, 0, 0);

            float marginLeft = 30;
            float marginRight = (isVertical ? PageSize.A4.Width : PageSize.A4.Height) - 30;
            float marginTop = (isVertical ? PageSize.A4.Height : PageSize.A4.Width) - 50;
            float marginBottom = 50;
            float mmToPoints = 2.83465f; // Convert mm to points (1mm = 2.83465 points)

            ColumnText columnTitle = new ColumnText(writer.DirectContent);
            columnTitle.SetSimpleColumn(marginLeft, marginBottom, marginRight, marginTop);

            Paragraph paragraphTitle = new Paragraph(textList[currentTextIndex].ToUpper(), font);
            paragraphTitle.Leading = 11f * mmToPoints;
            paragraphTitle.Alignment = Element.ALIGN_CENTER;
            columnTitle.AddElement(paragraphTitle);
            columnTitle.Go();
        }

        private static void CreateTextContent(PdfWriter writer, string[] textList, int currentTextIndex, bool isVertical)
        {          
            var baseFont = BaseFont.CreateFont("C:\\Users\\redfo\\AppData\\Local\\Microsoft\\Windows\\Fonts\\Sunday Regular.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new iTextSharp.text.Font(baseFont, 23);

            float marginLeft = 50;
            float marginRight = (isVertical ? PageSize.A4.Width : PageSize.A4.Height) - 50;
            float marginTop = isVertical ? PageSize.A4.Height - 170 : PageSize.A4.Width - 140;
            float marginBottom = 50;

            // Create a ColumnText object to wrap the text inside the bounding box
            ColumnText columnText = new ColumnText(writer.DirectContent);
            columnText.SetSimpleColumn(marginLeft, marginBottom, marginRight, marginTop);

            Paragraph paragraph = new Paragraph(textList[currentTextIndex], font);
            paragraph.Alignment = Element.ALIGN_CENTER;
            columnText.AddElement(paragraph);
            columnText.Go();
        }

        // Method to extract the number from the file name
        int ExtractNumber(string fileName)
        {
            // Extract the digits from the file name
            var numberPart = new string(fileName.Where(char.IsDigit).ToArray());
            return int.TryParse(numberPart, out int result) ? result : 0;
        }

        // Method to draw a black frame (1mm thick) 1 cm inside the page edges
        private void DrawBlackFrame(PdfWriter writer, bool toRotate)
        {
            float mmToPoints = 2.83465f; // 1mm = 2.83465 points
            float lineThickness = 0.5f * mmToPoints; // 1mm thick line
            float margin = 4 * mmToPoints; // 1cm margin

            PdfContentByte canvas = writer.DirectContent;
            canvas.SetLineWidth(lineThickness); // Set line thickness
            canvas.SetColorStroke(BaseColor.BLACK); // Set the line color to black

            // Draw the rectangle (frame)
            canvas.Rectangle(margin, margin, (!toRotate ? PageSize.A4.Height : PageSize.A4.Width) - 2 * margin, (!toRotate ? PageSize.A4.Width : PageSize.A4.Height) - 2 * margin);
            canvas.Stroke(); // Apply the stroke
        }


        // Method to draw a shadow effect from the bottom-right of the frame
        private void DrawSimpleShadow(PdfWriter writer, bool toRotate)
        {
            float mmToPoints = 2.83465f; // 1mm = 2.83465 points
            float margin = 4 * mmToPoints; // 1cm margin
            float shadowOffset = 3 * mmToPoints; // Shadow offset (3mm)

            PdfContentByte canvas = writer.DirectContent;

            // Set a semi-transparent gray color for the shadow
            BaseColor shadowColor = new BaseColor(160, 160, 160, 160); // Light gray with 50/255 opacity
            canvas.SetColorFill(shadowColor);

            // Draw the bottom shadow
            canvas.Rectangle(
                margin + shadowOffset,
                margin - shadowOffset,
                (!toRotate ? PageSize.A4.Height : PageSize.A4.Width) - 2 * margin - shadowOffset,
                shadowOffset
            );
            canvas.Fill();

            // Draw the right shadow
            canvas.Rectangle(
                (!toRotate ? PageSize.A4.Height : PageSize.A4.Width) - margin,
                margin - shadowOffset,
                shadowOffset,
                (!toRotate ? PageSize.A4.Width : PageSize.A4.Height) - 2 * margin - shadowOffset
            );
            canvas.Fill();
        }

        // Method to generate QR code using ZXing.Net
        private Bitmap GenerateQRCode(string text)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 300,
                    Width = 300,
                    Margin = 1
                },
                Renderer = new BitmapRenderer()
            };
            return writer.Write(text);
        }

        private void AddCenteredLink(Document document, PdfWriter writer, string qrCodeUrl, float centerX, float yPos, int linkPosition)
        {
            var baseFont = BaseFont.CreateFont("C:\\Users\\redfo\\AppData\\Local\\Microsoft\\Windows\\Fonts\\JS Sunsanee Normal.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new iTextSharp.text.Font(baseFont, 22, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
            
            // Create the paragraph to hold the link Inspiration Gallery:
            Paragraph paragraph = new Paragraph();

            // Create an anchor (clickable link) with the text "Inspiration Gallery" and the URL embedded in it
            Anchor link = new Anchor(ChoiceLinkName(linkPosition), font);
            link.Reference = qrCodeUrl;  // Embed the URL as the hyperlink

            // Add the link to the paragraph
            paragraph.Add(link);

            // Set alignment for the paragraph to center it
            paragraph.Alignment = Element.ALIGN_CENTER;

            // Add the paragraph directly to the document at the given Y position
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_CENTER, new Phrase(paragraph), centerX, yPos, 0);
        }

        
        private System.Drawing.Image ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(300, 300);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private byte[] ImageToByteArray(System.Drawing.Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        //------------------------------------------------------------------------

        private string ChoiceLinkName(int position)
        {
            string toReturn = string.Empty;

            switch (position)
            {
                case 0:
                    toReturn = "Web Coloring Page:";
                    break;
                case 1:
                    toReturn = "Web Puzzle:";
                    break;
                case 2:
                    toReturn = "Inspiration Gallery:";
                    break;
            }

            return toReturn;
        }

        private void checkBoxTitle_CheckedChanged(object sender, EventArgs e)
        {
            includeStoryName = !includeStoryName;
        }

        private void checkBoxAdditionInfo_CheckedChanged(object sender, EventArgs e)
        {
            includeAdditionInfo = !includeAdditionInfo;
        }

        private void checkBoxContainsTitle_CheckedChanged(object sender, EventArgs e)
        {
            includeTitleInText = !includeTitleInText;
        }

        private void checkBoxPictureFrame_CheckedChanged(object sender, EventArgs e)
        {
            includeBorderOnImage = !includeBorderOnImage;
        }

        private void checkBoxImageStrech_CheckedChanged(object sender, EventArgs e)
        {
            pictureStretch = !pictureStretch;
        }

        private void checkBoxIsVertical_CheckedChanged(object sender, EventArgs e)
        {
            isVerticalOriented = !isVerticalOriented;
        }
    }
}
