using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CG.Web.MegaApiClient;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Windows.Media.TextFormatting;
using System.Reflection.Metadata;
using System.Net;
using System.Text;


namespace WPF_fakture_otpremnice
{
    
    public partial class Pocetna : Window
    {
        private SqlConnection konekcija;
        private MegaApiClient megaClient;
        private bool isLoggedIn;
        private byte[] pdfData;
        private bool isAdmin;
      
        public Pocetna(SqlConnection postojecaKonekcija, bool isAdmin)
            
        {
            InitializeComponent();
            konekcija = postojecaKonekcija;
            megaClient = new MegaApiClient();
            LoginToMega("");
            UpdateUIBasedOnUser();
            PrikaziPDVpoMjesecima(null);
            LoadFirmi();
            this.isAdmin= isAdmin;
        }

        private void UpdateUIBasedOnUser()
        {
            if (isAdmin)
            {
                columnAdmin.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                columnAdmin.Width = new GridLength(0); 
            }
        }

        private void btnDodajFakturu(object sender, RoutedEventArgs e)
        {
            forma_za_unos forma = new forma_za_unos(konekcija,"fakture",megaClient,isLoggedIn);
           forma.stackPlaceno.Visibility= Visibility.Visible;
            forma.lblPlaceno.Visibility= Visibility.Visible;
            forma.btnSacuvaj.IsEnabled = false;

            forma.OnDataChanged += RefreshDataGrid;
           forma.OnSaveCompleted += PrikaziSveFirmePDV;
            forma.Show();
        }



        private void btnDodajFirmu(object sender, RoutedEventArgs e)
        {
            forma_za_unos_firmi forma = new forma_za_unos_firmi(konekcija,megaClient,isLoggedIn);
           
            forma.OnDataChanged += RefreshDataGridFirme;
            forma.Show();
        }

        private void btn_dodajOtpremnicu(object sender, RoutedEventArgs e)
        {
            forma_za_unos forma = new forma_za_unos(konekcija,"otpremnice",megaClient,isLoggedIn);
            forma.btnSacuvaj.IsEnabled = false;
            forma.OnDataChanged += RefreshDataGrid;

            forma.Show();
        }

        private void btn_dodajRevers(object sender, RoutedEventArgs e)
        {
            forma_za_unos forma = new forma_za_unos(konekcija, "reversi",megaClient,isLoggedIn);
            forma.btnSacuvaj.IsEnabled = false;
            forma.OnDataChanged += RefreshDataGrid;
            
            forma.Show();
        }

        private void btnDodajProtokol(object sender, RoutedEventArgs e)
        {
            forma_protokoli forma = new forma_protokoli(konekcija, megaClient, isLoggedIn);
           // forma.btnSacuvaj.IsEnabled = false;
            forma.OnDataChanged += RefreshDataGrid;

            forma.Show();
        }

        private void btnDodajPonudu(object sender, RoutedEventArgs e)
        {
            forma_za_unos forma = new forma_za_unos(konekcija, "ponude", megaClient, isLoggedIn);
            forma.btnSacuvaj.IsEnabled = false;
            forma.OnDataChanged += RefreshDataGrid;

            forma.Show();

        }

        private void prikaziDokumente(string tipDokumenta)
        {
          
           prikaziKolone(tipDokumenta);

            DataTable dt=new DataTable();

            string upit = tipDokumenta switch
            {
                "fakture" => " SELECT fakture.broj, firme.ime AS firma, fakture.datum, fakture.iznos, fakture.pdv, fakture.iznos+fakture.pdv AS ukupno, CASE WHEN fakture.placeno = 1 THEN 'Izlaz' ELSE 'Ulaz' END AS status, fakture.putanja_fajla FROM fakture " +
                "JOIN firme ON fakture.firma=firme.ID;",
                "otpremnice" => "SELECT otpremnice.broj, firme.ime AS firma, otpremnice.datum, otpremnice.iznos,otpremnice.pdv, otpremnice.iznos+otpremnice.pdv AS ukupno, otpremnice.putanja_fajla FROM otpremnice " +
                "JOIN firme ON otpremnice.firma=firme.ID;",
                "reversi" => "SELECT reversi.broj, firme.ime AS firma, reversi.datum, reversi.iznos, reversi.pdv, reversi.iznos+reversi.pdv AS ukupno, reversi.putanja_fajla FROM reversi " +
                "JOIN firme ON reversi.firma=firme.ID;",
                "ponude" => "SELECT ponude.broj, firme.ime AS firma, ponude.datum, ponude.iznos, ponude.pdv, ponude.iznos+ponude.pdv AS ukupno, ponude.putanja_fajla FROM ponude " +
                "JOIN firme ON ponude.firma=firme.ID;",
                _ => throw new ArgumentException("Nepoznat tip dokumenta")
            };

            SqlCommand komanda = new SqlCommand(upit, konekcija);

            try
            {
                using (SqlDataReader reader = komanda.ExecuteReader())
                {
                    dt.Load(reader);
                    dgDokumenti.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Greska prilikom ucitavanja podataka!", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void prikaziKolone(string tipDokumenta)
        {
            dgDokumenti.Columns.Clear();

            DataGridTextColumn columnBroj=new DataGridTextColumn { Header="Broj", Binding=new Binding("broj"), Width=new DataGridLength(1,DataGridLengthUnitType.Star)};
            DataGridTextColumn columnFirma = new DataGridTextColumn { Header="Firma", Binding=new Binding("firma"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnDatum = new DataGridTextColumn { Header = "Datum", Binding = new Binding("datum") { StringFormat = "dd.MM.yyyy" } , Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnIznos = new DataGridTextColumn { Header = "Iznos bez PDV-a", Binding = new Binding("iznos") { StringFormat = "N2" } , Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnPDV = new DataGridTextColumn { Header = "PDV", Binding = new Binding("pdv") { StringFormat = "N2" } , Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnUkupno = new DataGridTextColumn { Header = "Ukupno", Binding = new Binding("ukupno") { StringFormat = "N2" }, Width = new DataGridLength(1, DataGridLengthUnitType.Star) };

            dgDokumenti.Columns.Add(columnBroj);
            dgDokumenti.Columns.Add( columnFirma);
            dgDokumenti.Columns.Add(columnDatum);
            dgDokumenti.Columns.Add(columnIznos);
            dgDokumenti.Columns.Add(columnPDV);
            dgDokumenti.Columns.Add(columnUkupno);

            if (tipDokumenta == "fakture")
            {
                DataGridTextColumn columnPlaceno = new DataGridTextColumn
                {
                    Header = "Ulaz/Izlaz",
                    Binding = new Binding("status"), 
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                };

                dgDokumenti.Columns.Add(columnPlaceno);


            }

            var pogledajDokumentTemplate = new DataTemplate();
            var factory3 = new FrameworkElementFactory(typeof(Button));
            factory3.SetValue(Button.ContentProperty, "Pogledaj dokument");
            factory3.SetValue(Button.ForegroundProperty, Brushes.Blue);
            factory3.SetValue(Button.CursorProperty, Cursors.Hand);
            factory3.SetValue(Button.FontWeightProperty, FontWeights.Bold);
            factory3.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory3.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory3.AddHandler(Button.ClickEvent, new RoutedEventHandler(ViewDocument_Click));
            pogledajDokumentTemplate.VisualTree = factory3;

            DataGridTemplateColumn columnAction = new DataGridTemplateColumn
            {
                Header = "Pogledaj dokument",
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                CellTemplate=pogledajDokumentTemplate   
            };

            dgDokumenti.Columns.Add(columnAction);
            dgDokumenti.Columns.Add(CreateButtonColumn("Izmijeni", "Izmijeni", Brushes.DarkGreen, EditButton_Click));
            dgDokumenti.Columns.Add(CreateButtonColumn("Izbrisi", "Izbrisi", Brushes.Red, DeleteButton_Click));

        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataRowView = button?.DataContext as DataRowView;

            if (dataRowView == null)
            {
                MessageBox.Show("Nema podataka za brisanje.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string brojDokumenta = dataRowView["broj"].ToString();
            string tipDokumenta = lblTipDokumenta.Content.ToString().ToLower();
            string putanjaFajla = dataRowView["putanja_fajla"].ToString();


            ConfirmationDialog dialog = new ConfirmationDialog($"Da li ste sigurni da zelite obrisati dokument {brojDokumenta}?");
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string upit = tipDokumenta switch
                {
                    "fakture" => "DELETE FROM fakture WHERE broj=@broj",
                    "otpremnice" => "DELETE FROM otpremnice WHERE broj=@broj",
                    "reversi" => "DELETE FROM reversi WHERE broj=@broj",
                    "ponude"=>"DELETE FROM ponude WHERE broj=@broj",
                    _ => throw new ArgumentException("Nepoznat tip dokumenta!")
                };
                
                try
                {
                    using (SqlCommand komanda = new SqlCommand(upit, konekcija))
                    {
                        komanda.Parameters.AddWithValue("@broj", brojDokumenta);
                        komanda.ExecuteNonQuery();

                 
                        await DeleteFileFromMegaAsync(putanjaFajla);
                        MessageBox.Show("Dokument je uspješno obrisan!");

                        prikaziDokumente(tipDokumenta);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Greška prilikom brisanja dokumenta: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }


        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataRowView=button?.DataContext as DataRowView;

            if (dataRowView == null)
            {
                MessageBox.Show("Nema podataka za izmjenu.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string brojDokumenta = dataRowView["broj"].ToString();
            string tipDokumenta=lblTipDokumenta.Content.ToString().ToLower();            

            forma_za_unos forma = new forma_za_unos(konekcija, tipDokumenta,megaClient, isLoggedIn,brojDokumenta);

            forma.Show();
            forma.OnDataChanged += RefreshDataGrid;
        }
      
        private void btnPrikaziFakture(object sender, RoutedEventArgs e)
        {
            updateView("Fakture");
            obojiTekstNaslova();
            lblTipDokumenta.Content = "FAKTURE";
            prikaziDugmeADD(btnFakturaDodaj);
            prikaziDokumente("fakture");
            
        }

        private void btnPrikaziOtpremnice(object sender, RoutedEventArgs e)
        {
            updateView("Otpremnice");
            obojiTekstNaslova();
            lblTipDokumenta.Content = "OTPREMNICE";
            prikaziDugmeADD(btnOtpremnicaDodaj);
            prikaziDokumente("otpremnice");
        }

        private void btnPrikaziReverse(object sender, RoutedEventArgs e)
        {
            updateView("Reversi");
            obojiTekstNaslova();
            lblTipDokumenta.Content = "REVERSI";
            prikaziDugmeADD(btnReversDodaj);
            prikaziDokumente("reversi");
        }

        private void obojiTekstNaslova()
        {
            Color color = (Color)ColorConverter.ConvertFromString("#8B0000");

            SolidColorBrush brush = new SolidColorBrush(color);

            lblTipDokumenta.Foreground = brush;
            lblTipDokumenta.FontSize = 30;
            lblTipDokumenta.FontWeight=FontWeights.Bold;

        }

        public void RefreshDataGrid(string tipDokumenta=null)
        {
            try
            {
                if (tipDokumenta == null)
                {
                    PrikaziPodatke("protokoli");

                }
                else if (tipDokumenta == "projekti")
                {
                    PrikaziPodatke("projekti");
                }
                else
                {
                    prikaziDokumente(tipDokumenta);
                }
               
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Greška pri osvježavanju podataka: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RefreshDataGridFirme()
        {
            try
            {
                btnPrikaziFirme(null, null);
            }
            catch(ArgumentException ex)
            {
                MessageBox.Show($"Greška pri osvježavanju podataka: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        public class PDVData
        {
            public string Mjesec { get; set; }
            public decimal Ulaz { get; set; }
            public decimal Izlaz { get; set; }
            public decimal Saldo { get; set; }
        }

        private void PrikaziPDVpoMjesecima(string selektovanaFirma)
        {
            lblTipDokumenta.Visibility = Visibility.Visible;
            obojiTekstNaslova();
            lblTipDokumenta.Content = "PRIKAZ PDV-A PO GODINAMA I MJESECIMA";

            var pdvDataByYearAndMonth = new Dictionary<string, Dictionary<string, (decimal Ulaz, decimal Izlaz)>>();

            string upit = @"
    SELECT 
        YEAR(fakture.datum) AS godina, 
        MONTH(fakture.datum) AS mjesec, 
        SUM(CASE WHEN fakture.placeno = 0 THEN fakture.pdv ELSE 0 END) AS suma_pdv,
        SUM(CASE WHEN fakture.placeno = 1 THEN fakture.pdv ELSE 0 END) AS suma_placeno
    FROM fakture
JOIN firme ON fakture.firma=firme.ID";

            if(!string.IsNullOrEmpty(selektovanaFirma)&&selektovanaFirma!="Odaberite firmu...")
            {
                upit += " WHERE firme.ime = @firma";

            }

            upit += @" GROUP BY firme.ime, YEAR(fakture.datum), MONTH(fakture.datum)";


            try
            {
                using (SqlCommand komanda = new SqlCommand(upit, konekcija))
                {
                    if (!string.IsNullOrEmpty(selektovanaFirma) && selektovanaFirma != "Odaberite firmu...")
                    {
                        komanda.Parameters.AddWithValue("@firma", selektovanaFirma);
                    }

                    using (SqlDataReader reader = komanda.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string godina = reader["godina"].ToString();
                            string mjesec = reader["mjesec"].ToString().PadLeft(2, '0');
                            decimal sumaPdv = reader.GetDecimal("suma_pdv");
                            decimal sumaPlaceno = reader.GetDecimal("suma_placeno");

                            if (!pdvDataByYearAndMonth.ContainsKey(godina))
                            {
                                pdvDataByYearAndMonth[godina] = new Dictionary<string, (decimal Ulaz, decimal Izlaz)>();
                            }

                            if (!pdvDataByYearAndMonth[godina].ContainsKey(mjesec))
                            {
                                pdvDataByYearAndMonth[godina][mjesec] = (0, 0);
                            }

                            var currentData = pdvDataByYearAndMonth[godina][mjesec];
                            pdvDataByYearAndMonth[godina][mjesec] = (currentData.Ulaz + sumaPdv, currentData.Izlaz + sumaPlaceno);
                        }
                    }
                }

                var pdvData = new List<PDVData>();
                foreach (var godinaEntry in pdvDataByYearAndMonth)
                {
                    string godina = godinaEntry.Key;
                    foreach (var mjesecEntry in godinaEntry.Value)
                    {
                        string mjesec = mjesecEntry.Key;
                        var (ulaz, izlaz) = mjesecEntry.Value;

                        pdvData.Add(new PDVData
                        {
                            Mjesec = $"{GetMonthName(mjesec)} {godina}",
                            Ulaz = ulaz,
                            Izlaz = izlaz,
                            Saldo = ulaz - izlaz
                        });
                    }
                }

                dgPDV.ItemsSource = pdvData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška prilikom prikazivanja PDV-a: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetMonthName(string month)
        {
            switch (month)
            {
                case "01": return "Januar";
                case "02": return "Februar";
                case "03": return "Mart";
                case "04": return "April";
                case "05": return "Maj";
                case "06": return "Jun";
                case "07": return "Jul";
                case "08": return "Avgust";
                case "09": return "Septembar";
                case "10": return "Oktobar";
                case "11": return "Novembar";
                case "12": return "Decembar";
                default: return "Nepoznat";
            }
        }

        private void btnPocetna(object sender, RoutedEventArgs e)
        {
            updateView("Pocetna");
            prikaziDugmeADD(btnFiltriraj);

            cmbSelektovanaFirma.Visibility = Visibility.Visible;
            PrikaziPDVpoMjesecima(null);
        }

        private void updateView(string viewType)
        {
            switch (viewType)
            {
                case "Pocetna":
                    dgPDV.Visibility = Visibility.Visible;
                    dgDokumenti.Visibility = Visibility.Collapsed;
                    dgProtokoli.Visibility= Visibility.Collapsed;
                    dgProjekti.Visibility = Visibility.Collapsed;
                    cmbSelektovanaFirma.Visibility = Visibility.Visible;
                    break;
                case "Firme":
                    dgFirme.Visibility = Visibility.Visible;
                    dgPDV.Visibility = Visibility.Collapsed;
                    dgDokumenti.Visibility = Visibility.Collapsed;
                    dgProtokoli.Visibility = Visibility.Collapsed;
                    dgProjekti.Visibility = Visibility.Collapsed;
                    cmbSelektovanaFirma.Visibility = Visibility.Collapsed;
                    break;
                case "Fakture":
                case "Otpremnice":
                case "Reversi":
                case "Ponude":
                    dgPDV.Visibility=Visibility.Collapsed;
                    dgDokumenti.Visibility = Visibility.Visible;
                    dgFirme.Visibility = Visibility.Collapsed;
                    dgProtokoli.Visibility = Visibility.Collapsed;
                    dgProjekti.Visibility = Visibility.Collapsed;
                    lblTipDokumenta.Content = viewType.ToUpper();
                    cmbSelektovanaFirma.Visibility = Visibility.Collapsed;
                    break;
                case "Protokoli":
                    dgProtokoli.Visibility=Visibility.Visible;
                    dgPDV.Visibility = Visibility.Collapsed;
                    dgDokumenti.Visibility = Visibility.Visible;
                    dgFirme.Visibility = Visibility.Collapsed;
                    dgProjekti.Visibility = Visibility.Collapsed;
                    cmbSelektovanaFirma.Visibility = Visibility.Collapsed;
                    break;
                case "Projekti":
                    dgProjekti.Visibility = Visibility.Visible;
                    dgProtokoli.Visibility = Visibility.Visible;
                    dgPDV.Visibility = Visibility.Collapsed;
                    dgDokumenti.Visibility = Visibility.Visible;
                    dgFirme.Visibility = Visibility.Collapsed;
                    cmbSelektovanaFirma.Visibility = Visibility.Collapsed;
                    break;
                default:
                    throw new ArgumentException("Nepoznat tip prikaza!");
            }
        }

        private void btnPrikaziFirme(object sender, RoutedEventArgs e)
        {
            updateView("Firme");
            prikaziKoloneFirmi();
            prikaziDugmeADD(btnFirmaDodaj);
            lblTipDokumenta.Content = "FIRME";
            obojiTekstNaslova();

            DataTable dt = new DataTable();

            string upit = "SELECT * FROM firme";

            SqlCommand komanda = new SqlCommand(upit, konekcija);
            try
            {
                using (SqlDataReader reader = komanda.ExecuteReader())
                {
                    dt.Load(reader);
                    dgFirme.ItemsSource = dt.DefaultView;
                }

                dgFirme.Columns.Add(CreateButtonColumn("Izmijeni", "Izmijeni", Brushes.DarkGreen, EditButtonFirme_Click));
                dgFirme.Columns.Add(CreateButtonColumn("Izbrisi", "Izbrisi", Brushes.Red, DeleteButtonFirme_Click));


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greska prilikom ucitavanja podataka o firmama {ex.Message}", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void EditButtonFirme_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataRowView = button?.DataContext as DataRowView;

            if (dataRowView == null)
            {
                MessageBox.Show("Nema podataka za izmjenu.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (int.TryParse(dataRowView["ID"].ToString(), out int id))
            {
                
                forma_za_unos_firmi forma = new forma_za_unos_firmi(konekcija, megaClient, isLoggedIn, id);
                forma.Show();
                forma.OnDataChanged += RefreshDataGridFirme;
            }
        }


        private void DeleteButtonFirme_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataRowView = button?.DataContext as DataRowView;

            if (dataRowView == null)
            {
                MessageBox.Show("Nema podataka za brisanje.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string ID = dataRowView["ID"].ToString();
         
            ConfirmationDialog dialog = new ConfirmationDialog($"Da li ste sigurni da zelite obrisati {ID} i sve podatke koje je firma izdala?");
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string upit = "DELETE FROM firme WHERE ID=@ID";

                try
                {
                    using (SqlCommand komanda = new SqlCommand("DELETE FROM korisnici WHERE firma = @ID", konekcija))
                    {
                        komanda.Parameters.AddWithValue("@ID", ID);
                        komanda.ExecuteNonQuery();
                    }
                    using (SqlCommand komanda = new SqlCommand("DELETE FROM fakture WHERE firma = @ID", konekcija))
                    {
                        komanda.Parameters.AddWithValue("@ID", ID);
                        komanda.ExecuteNonQuery();
                    }
                    using (SqlCommand komanda = new SqlCommand("DELETE FROM otpremnice WHERE firma = @ID", konekcija))
                    {
                        komanda.Parameters.AddWithValue("@ID", ID);
                        komanda.ExecuteNonQuery();
                    }
                    using (SqlCommand komanda = new SqlCommand("DELETE FROM reversi WHERE firma = @ID", konekcija))
                    {
                        komanda.Parameters.AddWithValue("@ID", ID);
                        komanda.ExecuteNonQuery();
                    }
                    using (SqlCommand komanda = new SqlCommand("DELETE FROM projekti WHERE investitor=@ID", konekcija))
                    {
                        komanda.Parameters.AddWithValue("@ID", ID);
                        komanda.ExecuteNonQuery();
                    }
                    using (SqlCommand komanda = new SqlCommand(upit, konekcija))
                    {
                        komanda.Parameters.AddWithValue("@ID", ID);
                        komanda.ExecuteNonQuery();
                        MessageBox.Show("Firma je uspješno obrisana!");

                        btnPrikaziFirme(null, null);
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Greška prilikom brisanja firme: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }


        }

        private void prikaziKoloneFirmi()
        {
            dgFirme.Columns.Clear();

            DataGridTextColumn columnJIB = new DataGridTextColumn { Header = "JIB", Binding = new Binding("JIB"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnNaziv = new DataGridTextColumn { Header = "Naziv", Binding = new Binding("ime"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnAdresa = new DataGridTextColumn { Header = "Adresa", Binding = new Binding("adresa"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnTelefon = new DataGridTextColumn { Header = "Telefon", Binding = new Binding("telefon"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnEmail = new DataGridTextColumn { Header = "e-mail", Binding = new Binding("email"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnZiroRacun = new DataGridTextColumn { Header = "Ziro racun", Binding = new Binding("ziro_racun"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };

            dgFirme.Columns.Add(columnJIB);
            dgFirme.Columns.Add(columnNaziv);
            dgFirme.Columns.Add(columnAdresa);
            dgFirme.Columns.Add(columnTelefon);
            dgFirme.Columns.Add(columnEmail);
            dgFirme.Columns.Add(columnZiroRacun);
        }

        private async Task<string> GetDocumentLinkAsync(string putanja)
        {
 
            try
            {
                if (!isLoggedIn)
                {
                    throw new InvalidOperationException("Niste prijavljeni na MEGA.");
                }
                var nodes = await megaClient.GetNodesAsync();
               // MessageBox.Show($"putanjafajla {putanja}");
                string fileNameBezKvacica = StringHelper.PretvoriUBezKvacica(putanja);
               
                var fileNode = nodes.FirstOrDefault(n => n.Type == NodeType.File && StringHelper.PretvoriUBezKvacica(n.Name) == fileNameBezKvacica);

               // MessageBox.Show($"FileNode: {fileNode?.Name ?? "Not Found"}");
                if (fileNode == null)
                {
                    throw new FileNotFoundException("Dokument nije pronađen na MEGA-u.");
                }


                // var url = megaClient.GetDownloadLink(fileNode);
                var url = await megaClient.GetDownloadLinkAsync(fileNode);
                // MessageBox.Show($"url> {url}");
               // MessageBox.Show($"urll> {url}");

                return url.ToString();
            }

            catch (ApiException ex)
            {
                AppendToErrorTextBox($"API error: {ex.Message}\nStatus Code: {ex.StackTrace}");
                Console.WriteLine($"API error: {ex.Message}\nStatus Code: {ex.StackTrace}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                return null;
            }
            catch(SqlException ex)
            {
                MessageBox.Show("Desila se neočekivana greška! Pokušajte ponovo!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

     
        private async Task LoginToMega(string email, string password)
        {
            try
            {
                await megaClient.LoginAsync(email, password);

                isLoggedIn = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška prilikom prijave na MEGA: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                isLoggedIn = false;
            }
        }
        private async void ViewDocument_Click(object sender, RoutedEventArgs e)
        {
            
            var button = sender as Button;
            var dataRowView = button?.DataContext as DataRowView;

            if (dataRowView == null)
            {
                MessageBox.Show("Nema podataka za prikazivanje.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        
            string putanjaFajla = dataRowView["putanja_fajla"].ToString();
            //MessageBox.Show($"putanja {putanjaFajla} u view");

            string fileNameBezKvacica = StringHelper.PretvoriUBezKvacica(putanjaFajla);

           // MessageBox.Show($"putanja bez kvacica {fileNameBezKvacica} u view");


            try
            {           
                string documentLink = await GetDocumentLinkAsync(fileNameBezKvacica);
               // MessageBox.Show($"link> {documentLink}");
                if (!string.IsNullOrEmpty(documentLink))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = documentLink,
                        UseShellExecute = true
                    });
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Greška prilikom otvaranja dokumenta: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Nemate dozvole za ovu akciju, provjerite dozvole naloga. " + ex.Message, "Zabranjen pristup", MessageBoxButton.OK, MessageBoxImage.Error);
            }



        }

        private async Task DeleteFileFromMegaAsync(string filePath)
        {
            try
            {
                string fileNameBezKvacica=StringHelper.PretvoriUBezKvacica(filePath);
                var nodes = await megaClient.GetNodesAsync();

                // var fileNode = nodes.FirstOrDefault(n => n.Type == NodeType.File && n.Name == filePath);
                // var fileNode = nodes.FirstOrDefault(n => n.Type == NodeType.File);
                var fileNode = nodes.FirstOrDefault(n => n.Type == NodeType.File && StringHelper.PretvoriUBezKvacica(n.Name) == fileNameBezKvacica);

                if (fileNode != null)
                {
                    await megaClient.DeleteAsync(fileNode);
                  //  MessageBox.Show("Fajl je uspješno obrisan s MEGA-e.");
                }
                else
                {
                    MessageBox.Show("Fajl nije pronađen");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška prilikom brisanja fajla s MEGA-e: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AppendToErrorTextBox(string message)
        {
           // Ensure thread safety with Dispatcher
            Dispatcher.Invoke(() =>
            {
              
              //  ErrorTextBox.AppendText(message + Environment.NewLine);

               
              //  ErrorTextBox.ScrollToEnd();
            });
        }

        private void btnOdobriNaloge(object sender, RoutedEventArgs e)
        {
            
        }

      
        private void PrikaziPodatkeIzBaze(string upit, DataGrid dataGrid)
        {
            DataTable dt = new DataTable();

            using (SqlCommand komanda = new SqlCommand(upit, konekcija))
            {
                try
                {
                    using (SqlDataReader reader = komanda.ExecuteReader())
                    {
                        dt.Load(reader);
                        dataGrid.ItemsSource = dt.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Greška prilikom učitavanja podataka: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void btnPrikaziProtokole(object sender, RoutedEventArgs e)
        {
            updateView("Protokoli");
            lblTipDokumenta.Content = "PROTOKOLI";
            obojiTekstNaslova();
            prikaziDugmeADD(btnProtokolDodaj);
            PrikaziPodatke("protokoli");
        }


        private void ObrisiPodatak(string tip, int id)
        {
         
            string sql_upit = "";
            string upit_naziv = "";
            string naziv = "";

            switch (tip)
            {
                case "projekti":
                    upit_naziv = "SELECT naziv FROM projekti WHERE id=@id";
                    sql_upit = "DELETE FROM projekti WHERE id=@id";
                    break;
                case "protokoli":
                    upit_naziv = "SELECT naziv FROM protokoli WHERE id=@id";
                    sql_upit = "DELETE FROM protokoli WHERE id=@id";
                    break;
                default:
                    MessageBox.Show("Nepoznat tip podataka.", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }
         
            try
            {
                using(SqlCommand komanda=new SqlCommand(upit_naziv, konekcija))
                {
                    komanda.Parameters.AddWithValue("@id", id);
                    naziv = komanda.ExecuteScalar() as string;
                }
            }catch(SqlException ex)
            {
                MessageBox.Show("Podatak sa zadatim ID-jem nije pronađen.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
           
            ConfirmationDialog dialog = new ConfirmationDialog($"Da li ste sigurni da želite obrisati {tip} ciji je naziv: {naziv}?");
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                try
                {
                   
                    using (SqlCommand komanda = new SqlCommand(sql_upit, konekcija))
                    {
                        komanda.Parameters.AddWithValue("@id", id);
                        komanda.ExecuteNonQuery();
                        MessageBox.Show($"{naziv} je uspješno obrisan!");
                        PrikaziPodatke("projekti");
                        PrikaziPodatke("protokoli");
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Greška prilikom brisanja: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void DeleteButtonProtokoli_Click(object sender, RoutedEventArgs e) {

            var button = sender as Button;
            var dataRowView = button?.DataContext as DataRowView;

            if (dataRowView == null)
            {
                MessageBox.Show("Nema podataka za brisanje.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

            int id = Convert.ToInt32(dataRowView["id"]);

            ObrisiPodatak("protokoli", id);

        }

        private void DeleteButtonProjekti_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataRowView=button?.DataContext as DataRowView;

            if (dataRowView == null)
            {
                MessageBox.Show("Nema podataka za brisanje.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

            int id = Convert.ToInt32(dataRowView["id"]);
            
            ObrisiPodatak("projekti", id);
        }

        private void EditButtonProtokoli_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataRowView = button?.DataContext as DataRowView;

            if (dataRowView == null)
            {
                MessageBox.Show("Nema podataka za izmjenu.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (int.TryParse(dataRowView["id"].ToString(), out int id))
            {
                forma_protokoli forma = new forma_protokoli(konekcija, megaClient, isLoggedIn, id);
                forma.Show();
                forma.OnDataChanged += RefreshDataGrid;

            }
        }

        private void EditButtonProjekti_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataRowView=button?.DataContext as DataRowView;

            if (dataRowView == null)
            {
                MessageBox.Show("Nema podataka za izmjenu.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (int.TryParse(dataRowView["id"].ToString(), out int id))
            {
                forma_projekti forma = new forma_projekti(konekcija, megaClient, isLoggedIn, id);
                forma.Show();
                forma.OnDataChanged += RefreshDataGrid;

            }

        }

        private DataGridTemplateColumn CreateButtonColumn(string header, string buttonText, Brush foregroundColor, RoutedEventHandler clickEventHandler)
        {
            var buttonTemplate = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetValue(Button.ContentProperty, buttonText);
            factory.SetValue(Button.ForegroundProperty, foregroundColor);
            factory.SetValue(Button.CursorProperty, Cursors.Hand);
            factory.SetValue(Button.FontWeightProperty, FontWeights.Bold);
            factory.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AddHandler(Button.ClickEvent, clickEventHandler);
            buttonTemplate.VisualTree = factory;

            return new DataGridTemplateColumn
            {
                Header = header,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                CellTemplate = buttonTemplate
            };
        }

        private void PrikaziPodatke(string tipPodataka)
        {
           
            switch (tipPodataka)
            {
                case "firme":
                    prikaziKoloneFirmi();
                    PrikaziPodatkeIzBaze("SELECT * FROM firme", dgFirme);
                    break;
                case "protokoli":
                    prikaziKoloneProtokola();
                    PrikaziPodatkeIzBaze("SELECT id, broj_protokola, naziv , vrsta_protokola, datum , CASE WHEN ulaz_izlaz = 1 THEN 'Ulaz'  WHEN ulaz_izlaz = 0 THEN 'Izlaz' ELSE 'Nepoznat' END AS Ulaz_Izlaz  FROM protokoli", dgProtokoli);
                    break;
                case "projekti":
                    prikaziKoloneProjekta();
                    PrikaziPodatkeIzBaze("SELECT projekti.id, projekti.broj, projekti.naziv, " +
                        "CASE WHEN projekti.tip=1 THEN 'GLAVNI' WHEN projekti.tip=2 THEN 'IZVEDBENI' WHEN projekti.tip=3 THEN 'IDEJNI' ELSE 'NEPOZNAT' END AS tip, firme.ime AS investitor, vrsta_projekta.vrsta AS tip_projekta FROM projekti " +
                        "JOIN firme ON projekti.investitor=firme.ID " +
                        "JOIN vrsta_projekta ON projekti.tip=vrsta_projekta.id", dgProjekti);
                    break;
                default:
                    throw new ArgumentException("Nepoznat tip podataka");
            }
        }

        private void prikaziKoloneProtokola()
        {
            dgProtokoli.Columns.Clear();

            DataGridTextColumn columnBroj = new DataGridTextColumn { Header = "Broj", Binding = new Binding("broj_protokola"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnNaziv = new DataGridTextColumn { Header = "Naziv", Binding = new Binding("naziv"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnVrsta = new DataGridTextColumn { Header = "Vrsta", Binding = new Binding("vrsta_protokola"), Width = new DataGridLength(2, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnUlaz_izlaz = new DataGridTextColumn { Header = "Ulaz/Izlaz", Binding = new Binding("Ulaz_Izlaz"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnDatum = new DataGridTextColumn { Header = "Datum", Binding = new Binding("datum") { StringFormat = "dd.MM.yyyy" }, Width = new DataGridLength(1, DataGridLengthUnitType.Star) };

            dgProtokoli.Columns.Add(columnBroj);
            dgProtokoli.Columns.Add(columnNaziv);
            dgProtokoli.Columns.Add(columnVrsta);
            dgProtokoli.Columns.Add(columnUlaz_izlaz);
            dgProtokoli.Columns.Add(columnDatum);

            dgProtokoli.Columns.Add(CreateButtonColumn("Izmijeni", "Izmijeni", Brushes.DarkGreen, EditButtonProtokoli_Click));
            dgProtokoli.Columns.Add(CreateButtonColumn("Izbrisi", "Izbrisi", Brushes.Red, DeleteButtonProtokoli_Click));


        }

        private void prikaziKoloneProjekta()
        {
            dgProjekti.Columns.Clear();

            DataGridTextColumn columnBroj = new DataGridTextColumn { Header = "Broj", Binding = new Binding("broj"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnNaziv = new DataGridTextColumn { Header = "Naziv", Binding = new Binding("naziv"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnTip = new DataGridTextColumn { Header = "Vrsta", Binding = new Binding("tip"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnInvestitor = new DataGridTextColumn { Header = "Investitor", Binding = new Binding("investitor"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
            DataGridTextColumn columnId = new DataGridTextColumn
            {
                Header = "ID", 
                Binding = new Binding("id"),
                Visibility = Visibility.Collapsed 
            };

            dgProjekti.Columns.Add(columnBroj);
            dgProjekti.Columns.Add(columnNaziv);
            dgProjekti.Columns.Add(columnTip);
            dgProjekti.Columns.Add(columnInvestitor);
            dgProjekti.Columns.Add(columnId);

            dgProjekti.Columns.Add(CreateButtonColumn("Izmijeni", "Izmijeni", Brushes.DarkGreen, EditButtonProjekti_Click));
            dgProjekti.Columns.Add(CreateButtonColumn("Izbrisi", "Izbrisi", Brushes.Red, DeleteButtonProjekti_Click));


        }

        private void btnPrikaziPonude(object sender, RoutedEventArgs e)
        {
            updateView("Ponude");
            lblTipDokumenta.Content = "PONUDE";
            obojiTekstNaslova();
            prikaziDugmeADD(btnPonudaDodaj);
            prikaziDokumente("ponude");
        }

        private void btnPrikaziRacune(object sender, RoutedEventArgs e)
        {

        }

        private void pretraziPodatke(string tipPodataka,string pretraga)
        {
            DataTable dt = new DataTable();
            string upit = tipPodataka switch
            {
                "fakture" => "SELECT fakture.broj, firme.ime AS firma, fakture.datum, fakture.iznos, fakture.pdv, fakture.iznos+fakture.pdv AS ukupno, CASE WHEN fakture.placeno = 1 THEN 'Izlaz' ELSE 'Ulaz' END AS status, fakture.putanja_fajla FROM fakture " +
                "JOIN firme ON fakture.firma=firme.ID WHERE fakture.broj LIKE @pretraga OR firme.ime LIKE @pretraga;",
                "otpremnice" => "SELECT otpremnice.broj, firme.ime AS firma, otpremnice.datum, otpremnice.iznos, otpremnice.pdv, otpremnice.iznos+otpremnice.pdv AS ukupno, otpremnice.putanja_fajla FROM otpremnice " +
                "JOIN firme ON otpremnice.firma=firme.ID WHERE otpremnice.broj LIKE @pretraga OR firme.ime LIKE @pretraga;",
                "reversi" => "SELECT reversi.broj, firme.ime AS firma, reversi.datum, reversi.iznos, reversi.pdv, reversi.iznos+reversi.pdv AS ukupno, reversi.putanja_fajla FROM reversi " +
                "JOIN firme ON reversi.firma=firme.ID WHERE reversi.broj LIKE @pretraga OR firme.ime LIKE @pretraga;",
                "ponude" => "SELECT ponude.broj, firme.ime AS firma, ponude.datum, ponude.iznos, ponude.pdv, ponude.iznos+ponude.pdv AS ukupno, ponude.putanja_fajla FROM ponude " +
                "JOIN firme ON ponude.firma=firme.ID WHERE ponude.broj LIKE @pretraga OR firme.ime LIKE @pretraga;",
                "protokoli" => "SELECT id, broj_protokola, naziv, vrsta_protokola, datum, CASE WHEN ulaz_izlaz = 1 THEN 'Ulaz' ELSE 'Izlaz' END AS Ulaz_Izlaz FROM protokoli " +
                "WHERE broj_protokola LIKE @pretraga OR naziv LIKE @pretraga;",
                "firme" => "SELECT * FROM firme WHERE ime LIKE @pretraga OR JIB LIKE @pretraga;",
                "projekti"=> "SELECT id, broj, naziv, firme.ime AS investitor, vrsta_projekta.vrsta AS vrsta FROM projekti " +
                "JOIN firme ON projekti.investitor=firme.id " +
                "JOIN vrsta_projekta ON projekti.tip=vrsta_projekta.vrsta WHERE naziv LIKE @pretraga OR broj LIKE @pretraga OR firme.ime LIKE @pretraga OR vrsta_projekta.vrsta LIKE @pretraga;",
                _ => throw new ArgumentException("Nepoznat tip podataka")
            };

            SqlCommand komanda = new SqlCommand(upit, konekcija);
            komanda.Parameters.AddWithValue("@pretraga", "%" + pretraga + "%");

            try
            {
                using(SqlDataReader reader = komanda.ExecuteReader())
                {
                    dt.Load(reader);

                    switch (tipPodataka)
                    {
                        case "firme":
                            dgFirme.ItemsSource = dt.DefaultView;
                            break;
                        case "protokoli":
                            dgProtokoli.ItemsSource = dt.DefaultView; 
                            break;
                        case "fakture":
                        case "otpremnice":
                        case "reversi":
                        case "ponude":
                            dgDokumenti.ItemsSource = dt.DefaultView; 
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Greska prilikom pretrage podataka: {ex.Message}", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btnPretrazi_Click(object sender, RoutedEventArgs e)
        {
            string pretraga=txtPretraga.Text.Trim();
            string tipPodataka = lblTipDokumenta.Content.ToString().ToLower();

            if (string.IsNullOrEmpty(pretraga))
            {
                MessageBox.Show("Molimo unesite tekst za pretragu", "Informacija", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            pretraziPodatke(tipPodataka,pretraga);
        }

        private void btnPrikaziProjekte_Click(object sender, RoutedEventArgs e)
        {
            updateView("Projekti");
            lblTipDokumenta.Content = "Projekti";
            obojiTekstNaslova();
            prikaziDugmeADD(btnProjekatDodaj);
            PrikaziPodatke("projekti");
        }
    
        private void SakrijDugmad()
        {
            btnFakturaDodaj.Visibility=Visibility.Collapsed;
            btnOtpremnicaDodaj.Visibility=Visibility.Collapsed;
            btnReversDodaj.Visibility=Visibility.Collapsed;
            btnFirmaDodaj.Visibility=Visibility.Collapsed;
            btnProtokolDodaj.Visibility= Visibility.Collapsed;
            btnPonudaDodaj.Visibility = Visibility.Collapsed;
            btnProjekatDodaj.Visibility = Visibility.Collapsed;
            btnFiltriraj.Visibility=Visibility.Collapsed;
            cmbSelektovanaFirma.Visibility=Visibility.Collapsed;
        }

        private void prikaziDugmeADD(Button btn)
        {
            SakrijDugmad();
            btn.Visibility = Visibility.Visible;
        }

        private void btnDodajProjekat_Click(object sender, RoutedEventArgs e)
        {
            forma_projekti forma=new forma_projekti(konekcija, megaClient, isLoggedIn);
            // forma.btnSacuvaj.IsEnabled = false;
            forma.OnDataChanged += RefreshDataGrid;
          //  forma.OnSaveCompleted

            forma.Show();
        }

        private void LoadFirmi()
        {
            try
            {
                DataTable dt = FirmaService.DohvatiImenaFirmi(konekcija);
                cmbSelektovanaFirma.ItemsSource = dt.DefaultView;
                cmbSelektovanaFirma.SelectedValuePath = "jib";
                cmbSelektovanaFirma.DisplayMemberPath = "ime";
                cmbSelektovanaFirma.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom popunjavanja liste firmi: " + ex.Message);
            }
        }

        private void PrikaziSveFirmePDV()
        {
            PrikaziPDVpoMjesecima(null);
        }


        private void ShowError_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFiltrirajPoFirmama(object sender, RoutedEventArgs e)
        {
            string selektovanaFirma = cmbSelektovanaFirma.Text;
            PrikaziPDVpoMjesecima(selektovanaFirma);
        }

        private void cmbSelektovanaFirma_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string selektovanaFirma = cmbSelektovanaFirma.Text;
            PrikaziPDVpoMjesecima(selektovanaFirma);
        }
    }
}
