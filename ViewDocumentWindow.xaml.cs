using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_fakture_otpremnice
{
    /// <summary>
    /// Interaction logic for ViewDocumentWindow.xaml
    /// </summary>
    public partial class ViewDocumentWindow : Window
    {
        private byte[] fileBytes;

        public ViewDocumentWindow(byte[] fileBytes)
        {
            InitializeComponent();
            // OpenExternalBrowser("https://www.mega.nz");
          
            this.fileBytes = fileBytes;
            InitializeWebView2();
        }///

        private async void InitializeWebView2()
        {
            try
            {
                await webView2Control.EnsureCoreWebView2Async(null);
                PrikaziFajl();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Greška prilikom inicijalizacije WebView2: {ex.Message}", "Greška", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void PrikaziFajl()
        {
            try
            {
                string tempFilePath = System.IO.Path.GetTempFileName() + ".pdf"; // Dodajte ekstenziju PDF
                System.IO.File.WriteAllBytes(tempFilePath, fileBytes);
                webView2Control.Source = new Uri(tempFilePath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Greška prilikom prikazivanja fajla: {ex.Message}", "Greška", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
