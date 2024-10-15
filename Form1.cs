using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Coloring_Book_Frame
{
    public partial class Form1 : Form
    {
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

                // Loop through all image files in the folder
                foreach (string filePath in Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".jpeg")))
                {
                    // Resize and convert the image to iTextSharp's format
                    using (var img = ResizeImage(System.Drawing.Image.FromFile(filePath), 2480, 3508))
                    {
                        var pdfImage = iTextSharp.text.Image.GetInstance(ImageToByteArray(img));
                        pdfImage.SetAbsolutePosition(0, 0);
                        pdfImage.ScaleToFit(PageSize.A4.Width, PageSize.A4.Height);

                        // Add a new page and insert the image
                        document.NewPage();
                        document.Add(pdfImage);

                        // Add a new page and a blank placeholder (to ensure a blank page is added)
                        document.NewPage();
                        document.Add(new Paragraph(" "));  // Add an empty paragraph to mark it as a blank page
                    }
                }

                document.Close();
            }
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
    }
}
