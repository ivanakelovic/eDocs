using CG.Web.MegaApiClient;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_fakture_otpremnice
{

    public partial class forma_projekti : Window
    {
        private event Action OnSaveCompleted;
        private SqlConnection konekcija;
        private MegaApiClient megaClient;
        private bool isLoggedIn;
        private int? editId;

        public forma_projekti(SqlConnection konekcija, MegaApiClient megaClient, bool isLoggedIn)
        {
            InitializeComponent();
            this.konekcija= konekcija;
            this.megaClient= megaClient;
            this.isLoggedIn=isLoggedIn;
            LoadFirmi();
            LoadVrstaProjekta();
        }

        public forma_projekti(SqlConnection konekcija, MegaApiClient megaClient, bool isLoggedIn, int id)
        {
            InitializeComponent();
            this.konekcija = konekcija;
            this.megaClient = megaClient;
            this.isLoggedIn = isLoggedIn;
            this.editId = id;
            LoadFirmi();
            LoadVrstaProjekta();
            LoadProjekatData(id);
        }

        public event Action<string> OnDataChanged;

        private void dodaj_projekat_click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBrojProjekta.Text) ||
                string.IsNullOrWhiteSpace(txtNazivProjekta.Text) ||
                 cmbVrsta.SelectedValue == null ||
                string.IsNullOrWhiteSpace(cmbVrsta.SelectedValue.ToString()) ||
                cmbInvestitor.SelectedValue == null ||
                string.IsNullOrWhiteSpace(cmbInvestitor.SelectedValue.ToString()))
            {
                MessageBox.Show("Molimo popunite sva polja i provjerite unesene vrijednosti!", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string broj=txtBrojProjekta.Text;
            string naziv=txtNazivProjekta.Text;
            string tip = cmbVrsta.SelectedValue.ToString();
            string investitor=cmbInvestitor.SelectedValue.ToString();

            string upit = "";
            if(editId == null)
            {
                upit = "INSERT INTO projekti (broj, naziv, tip, investitor) VALUES (@broj, @naziv, @tip, @investitor)";
            }
            else
            {
                upit = "UPDATE projekti SET broj=@broj, naziv=@naziv, tip=@tip, investitor=@investitor WHERE id=@id";
            }

            try
            {
                using(SqlCommand komanda=new SqlCommand(upit, konekcija))
                {
                    if (editId.HasValue)
                    {
                        komanda.Parameters.AddWithValue("@id", editId);
                    }

                    komanda.Parameters.AddWithValue("@broj", broj);
                    komanda.Parameters.AddWithValue("@naziv", naziv);
                    komanda.Parameters.AddWithValue("@tip", tip);
                    komanda.Parameters.AddWithValue("@investitor", investitor);

                    komanda.ExecuteNonQuery();
                    MessageBox.Show("Projekat je uspjesno sacuvan!");

                    OnSaveCompleted?.Invoke();
                    OnDataChanged?.Invoke("projekti");
                    this.Close();

                    }

            }catch(SqlException ex)
            {
                MessageBox.Show("Greska prilikom cuvanja podataka", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void LoadFirmi()
        {
            try
            {
                DataTable dt = FirmaService.DohvatiImenaFirmi(konekcija);
                cmbInvestitor.ItemsSource = dt.DefaultView;
                cmbInvestitor.SelectedValuePath = "ID";
                cmbInvestitor.DisplayMemberPath = "ime";
                cmbInvestitor.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom popunjavanja liste firmi: " + ex.Message);
            }
        }

        private void LoadVrstaProjekta()
        {

            try
            {
                DataTable dt = new DataTable();
                string upit = "SELECT * FROM vrsta_projekta ORDER BY vrsta";
                SqlCommand komanda = new SqlCommand(upit, konekcija);
                SqlDataAdapter da=new SqlDataAdapter(komanda);
                da.Fill(dt);

                DataRow defaultRow=dt.NewRow();
                defaultRow["id"] = 0;
                defaultRow["vrsta"] = "Odaberite vrstu...";
                dt.Rows.InsertAt(defaultRow, 0);

                cmbVrsta.ItemsSource = dt.DefaultView;
                cmbVrsta.SelectedValuePath = "id";
                cmbVrsta.DisplayMemberPath = "vrsta";
                cmbVrsta.SelectedIndex = 0;

            }catch(SqlException ex)
            {
                MessageBox.Show("Greska prilikom prikaza vrsti projekta!", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private int id;

        private void LoadProjekatData(int id)
        {
            try
            {
                string upit = "SELECT * FROM projekti WHERE id=@id";

                using (SqlCommand komanda = new SqlCommand(upit, konekcija)) {
                    komanda.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader= komanda.ExecuteReader())
                    {
                       
                        if (reader.Read()) { 
                        
                       // id=Convert.ToInt32(reader["id"]);
                            txtBrojProjekta.Text = reader["broj"].ToString();
                            txtNazivProjekta.Text = reader["naziv"].ToString();
                            cmbVrsta.SelectedValue = reader["tip"];
                            cmbInvestitor.SelectedValue = reader["investitor"];
                        }
                        else
                        {
                            MessageBox.Show("Projekat sa zadatim brojem nije pronađena.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            this.Close();
                        }
                    }
                
                }

            }catch(SqlException ex)
            {
                MessageBox.Show("Greška prilikom učitavanja podataka protokola: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearchInvestitor_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBoxHelper.FilterComboBox(cmbInvestitor, txtSearchInvestitor);
        }
    }
}
