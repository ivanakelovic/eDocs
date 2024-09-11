using Microsoft.Data.SqlClient;
using System.Windows;
using System.Data;
using CG.Web.MegaApiClient;
using System.IO;
using Path = System.IO.Path;
using System.Globalization;
using System.Windows.Controls;

namespace WPF_fakture_otpremnice
{
  
    public partial class forma_za_unos : Window
    {
        public event Action OnSaveCompleted;

        private SqlConnection konekcija;
        private string tipDokumenta;
        private string brojDokumenta;
        private bool izmjena;
        private MegaApiClient megaClient;
        private bool isLoggedIn;

       

        public forma_za_unos(SqlConnection postojecaKonekcija, string tipDokumenta,MegaApiClient megaClient,bool isLoggedIn,string brojDokumenta=null)
        {
            InitializeComponent();

            konekcija = postojecaKonekcija;
            this.tipDokumenta = tipDokumenta;
            dohvatiImenaFirmi();
            this.brojDokumenta = brojDokumenta;
            izmjena = !string.IsNullOrEmpty(brojDokumenta);
            this.megaClient = megaClient;
            this.isLoggedIn = isLoggedIn;
            LoadFirmi();
            
            if (izmjena)
            {
                LoadDataForEdit();
            }
        }


        private void LoadFirmi()
        {
            try
            {
                DataTable dt = FirmaService.DohvatiImenaFirmi(konekcija);
                cmbFirma.ItemsSource = dt.DefaultView;
                cmbFirma.SelectedValuePath = "ID";
                cmbFirma.DisplayMemberPath = "ime";
                cmbFirma.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom popunjavanja liste firmi: " + ex.Message);
            }
        }

        public void dohvatiImenaFirmi()
        {
            try
            {
                if (konekcija.State == ConnectionState.Closed)
                {
                    konekcija.Open();
                }

                string upit = "SELECT ID,ime from firme ORDER BY ime";

                SqlCommand komanda = new SqlCommand(upit, konekcija);
                SqlDataAdapter da = new SqlDataAdapter(komanda);
                DataTable dt = new DataTable();
                da.Fill(dt);

                DataRow defaultRow = dt.NewRow();
                defaultRow["ID"] = 0;
                defaultRow["ime"] = "Odaberite firmu...";
                dt.Rows.InsertAt(defaultRow, 0);

                cmbFirma.ItemsSource = dt.DefaultView;
                cmbFirma.SelectedValuePath = "ID";
                cmbFirma.DisplayMemberPath = "ime";

                cmbFirma.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom popunjavanja liste firmi: " + ex.Message);
            }

        }

        public event Action<string> OnDataChanged;

        private int id;
        private async void dodaj_dokument(object sender, RoutedEventArgs e)
        {
   
            if (
           string.IsNullOrWhiteSpace(txtBrojDokumenta.Text)||
           !dpDatum.SelectedDate.HasValue||
           !decimal.TryParse(txtIznos.Text, NumberStyles.Number, CultureInfo.InvariantCulture,out decimal iznosRacuna)||iznosRacuna==0||
           cmbFirma.SelectedValue == null || string.IsNullOrWhiteSpace(cmbFirma.SelectedValue.ToString())||
           (!izmjena && !fajlUspjesnoSacuvan)

           )
            {
                MessageBox.Show($"VRIJEDNOSTI: BR DOK: {fajlUspjesnoSacuvan}");
                MessageBox.Show("Molimo popunite sva polja ispravno i provjerite unesene vrijednosti.", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            

            string brojDokumenta = txtBrojDokumenta.Text;
            DateTime datum = dpDatum.SelectedDate.GetValueOrDefault();
                decimal iznos = decimal.Parse(txtIznos.Text);
            string firmaId = cmbFirma.SelectedValue.ToString();
            decimal pdv = 0;
            bool placeno = rbPlaceno.IsChecked ?? false;
           
            
          
            if (rbAutomatskiPDV.IsChecked == true)
            {
                pdv=iznos*0.17m;
            }
            else if (rbUnosPDV.IsChecked == true)
            {
                if (!decimal.TryParse(txtPDVstopa.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out pdv) || pdv == 0)
                {
                    MessageBox.Show("Unesite ispravan PDV.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            string putanja = Path.GetFileName(txtFajlPutanja.Text);

            
                string upit = "";

            if (izmjena)
            {
                 upit = tipDokumenta switch
                {
                    "fakture" => "UPDATE fakture SET broj=@brojDokumenta, firma=@firma, datum = @datum, iznos = @iznos, pdv = @pdv, placeno = @placeno, putanja_fajla = @putanja WHERE id = @id",
                    "otpremnice" => "UPDATE otpremnice SET broj=@brojDokumenta, datum = @datum, firma=@firma, iznos = @iznos, pdv = @pdv, putanja_fajla = @putanja WHERE id = @id",
                    "reversi" => "UPDATE reversi SET  broj=@brojDokumenta, firma = @firma, datum = @datum, iznos = @iznos, pdv = @pdv, putanja_fajla = @putanja WHERE id = @id",
                    "ponude" => "UPDATE ponude SET  broj=@brojDokumenta, firma = @firma, datum = @datum, iznos = @iznos, pdv = @pdv, putanja_fajla = @putanja WHERE id = @id",
                    _ => throw new ArgumentException("Nepoznat tip dokumenta")
                };

            }
            else
            {
                upit = tipDokumenta switch
                {
                    "fakture" => "INSERT INTO fakture (broj, firma, datum, iznos, pdv, placeno, putanja_fajla) VALUES (@broj, @firma, @datum, @iznos, @pdv, @placeno, @putanja)",
                    "otpremnice" => "INSERT INTO otpremnice (broj, firma, datum, iznos, pdv, putanja_fajla) VALUES (@broj, @firma, @datum, @iznos, @pdv, @putanja)",
                    "reversi" => "INSERT INTO reversi (broj, firma, datum, iznos, pdv, putanja_fajla) VALUES (@broj, @firma, @datum, @iznos, @pdv, @putanja)",
                    "ponude" => "INSERT INTO ponude (broj, firma, datum, iznos, pdv, putanja_fajla) VALUES (@broj, @firma, @datum, @iznos, @pdv, @putanja)",
                    _ => throw new ArgumentException("Nepoznat tip dokumenta")
                };
            }

           
                try
                {
                    using (SqlCommand komanda = new SqlCommand(upit, konekcija))
                    {

                    if (izmjena)
                    {
                        komanda.Parameters.AddWithValue("@id", id);
                        komanda.Parameters.AddWithValue("@brojDokumenta", brojDokumenta); 
                    }
                    komanda.Parameters.AddWithValue("@broj", brojDokumenta);
                    komanda.Parameters.AddWithValue("@firma", firmaId);
                        komanda.Parameters.AddWithValue("@datum", datum);
                        komanda.Parameters.AddWithValue("@iznos", iznos);
                        komanda.Parameters.AddWithValue("@pdv", pdv);
                        komanda.Parameters.AddWithValue("@placeno", placeno);
                    komanda.Parameters.AddWithValue("@putanja",  putanja);
                    await komanda.ExecuteNonQueryAsync();
                        MessageBox.Show("Dokument je uspjesno sacuvan!");

                 
                    OnSaveCompleted?.Invoke();
                    OnDataChanged?.Invoke(tipDokumenta);
                    this.Close();

                }



            }
                catch (SqlException ex)
                {
                
                    if(ex.Number==1062)
                {
                    MessageBox.Show("Broj dokumenta već postoji. Molimo unesite jedinstveni broj dokumenta.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Greska prilikom cuvanja dokumenta: " + ex.Message);
                }

            }
           

            }

        private void izracunajPDVAutomatski(object sender, RoutedEventArgs e)
        {
            if (rbAutomatskiPDV != null && rbAutomatskiPDV.IsChecked == true)
            {
               if(txtPDVstopa!=null)
                txtPDVstopa.Visibility = Visibility.Hidden;
            }
            else
            {
                return;
            }
        }

        private void izracunajManuelnoPDV(object sender, RoutedEventArgs e)
        {
            if (rbUnosPDV!=null && rbUnosPDV.IsChecked == true)
            {
                txtPDVstopa.Visibility = Visibility.Visible;
            }
        }

        private bool fajlUspjesnoSacuvan = false;
        private async void odaberiFajl(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*"
            };

            bool? result=openFileDialog.ShowDialog();

            if (result == true) { 
            string filePath=openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);

                string fileNameBezKvacica = StringHelper.PretvoriUBezKvacica(fileName);
                txtFajlPutanja.Text = filePath;

            //string fileName=Path.GetFileName(filePath);

             
                try
                {
                    MessageBox.Show("Sacekajte dok se fajl uploada, pa sacuvajte dokument klikom na dugme SACUVAJ!!!", "INFORMACIJA", MessageBoxButton.OK, MessageBoxImage.Information); ;

                    await UploadFileToMegaAsync(filePath, fileNameBezKvacica);
                    fajlUspjesnoSacuvan = true;
                    btnSacuvaj.IsEnabled = true;
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Greška prilikom spremanja datoteke: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                fajlUspjesnoSacuvan=false;
                    return;
                }
            }
        }

        private byte[] UcitajFajl(string putanja)
        {
            if (File.Exists(putanja))
            {
                return File.ReadAllBytes(putanja);
            }
            else
            {
                throw new FileNotFoundException("Fajl ne postoji na zadatoj putanji");
            }
        }

        private void LoadDataForEdit()
        {
            try
            {
                if (konekcija.State == ConnectionState.Closed)
                {
                    konekcija.Open();
                }

                string upit = tipDokumenta switch
                {
                    "fakture" => "SELECT * FROM fakture WHERE broj = @broj",
                    "otpremnice" => "SELECT * FROM otpremnice WHERE broj = @broj",
                    "reversi" => "SELECT * FROM reversi WHERE broj = @broj",
                    "ponude"=>"SELECT * FROM ponude WHERE broj=@broj",
                    _ => throw new ArgumentException("Nepoznat tip dokumenta")
                };

                using(SqlCommand komanda=new SqlCommand(upit, konekcija))
                {
                    komanda.Parameters.AddWithValue("@broj", brojDokumenta);
                    using(SqlDataReader reader = komanda.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            id = Convert.ToInt32(reader["id"]);
                            txtBrojDokumenta.Text = reader["broj"].ToString();
                            dpDatum.SelectedDate = Convert.ToDateTime(reader["datum"]);
                            txtIznos.Text = reader["iznos"].ToString();
                            txtPDVstopa.Text = reader["pdv"].ToString();
                            cmbFirma.SelectedValue = reader["firma"];
                            if (tipDokumenta == "fakture")
                            {
                                lblPlaceno.Visibility = Visibility.Visible;
                                stackPlaceno.Visibility = Visibility.Visible;
                                rbPlaceno.IsChecked = reader["placeno"] != DBNull.Value && Convert.ToBoolean(reader["placeno"]);

                            }
                            txtFajlPutanja.Text = reader["putanja_fajla"].ToString();

                            if (reader["pdv"].ToString()=="0")
                            {
                                rbAutomatskiPDV.IsChecked = true;
                                txtPDVstopa.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                rbUnosPDV.IsChecked = true;
                                txtPDVstopa.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Greška prilikom učitavanja podataka za izmenu: " + ex.Message);
            }
        }


        private async Task UploadFileToMegaAsync(string filePath, string fileNameBezKvacica)
        {
            if (!isLoggedIn)
            {
                throw new InvalidOperationException("Niste prijavljeni na MEGA.");
            }

            string folderName = "SharedFolder";
            INode folderNode = await FindSharedFolderAsync(folderName);

            if (folderNode == null)
            {
                MessageBox.Show("Folder nije pronađen!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                fajlUspjesnoSacuvan = false;
                return;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var fileName = Path.GetFileName(filePath);
                try
                {
                    await megaClient.UploadAsync(fileStream, fileNameBezKvacica, folderNode);
                    MessageBox.Show("Fajl je uspešno postavljen na MEGA!", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
                    fajlUspjesnoSacuvan = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Greška prilikom upload-a fajla na MEGA: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    fajlUspjesnoSacuvan = false;
                }
            }
        }

        private async Task<INode> FindSharedFolderAsync(string folderName)
        {
            var nodes = await megaClient.GetNodesAsync();
            return nodes.FirstOrDefault(node => node.Type == NodeType.Directory && node.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));
        }

        private void txtSearchFirma_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
           
                ComboBoxHelper.FilterComboBox(cmbFirma, txtSearchInvestitor);
            
        }
    }
}

