using System;
using System.Collections.Generic;
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
using System.IO;
using Microsoft.Data.SqlClient;
using CG.Web.MegaApiClient;

namespace WPF_fakture_otpremnice
{

    public partial class forma_protokoli : Window
    {
        public event Action OnSaveCompleted;
        private SqlConnection konekcija;
        private string brojProtokola;
        private bool izmjena;
        private MegaApiClient megaClient;
        private bool isLoggedIn;
        private int? editId;


        public forma_protokoli(SqlConnection konekcija, MegaApiClient megaClient,bool isLoggedIn)
        {
            InitializeComponent();
            this.konekcija= konekcija;
            this.megaClient= megaClient;
            this.isLoggedIn=isLoggedIn;
         
        }

        public forma_protokoli(SqlConnection konekcija, MegaApiClient megaClient, bool isLoggedIn,int id)
        {
            InitializeComponent();
            this.konekcija = konekcija;
            this.megaClient = megaClient;
            this.isLoggedIn = isLoggedIn;
            this.editId = id;
         
            LoadProtokolData(id);
        }

        public event Action<string> OnDataChanged;
       
        private void dodaj_protokol_click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtBrojProtokola.Text)||
                string.IsNullOrWhiteSpace(txtNazivProtokola.Text)||
                string.IsNullOrWhiteSpace(txtVrstaProtokola.Text)||
                !dpDatumProtokoli.SelectedDate.HasValue
               
                )
            {
                MessageBox.Show("Molimo popunite sva polja i provjerite unesene vrijednosti!", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

     

            string upit = "";

            if (editId==null)
            {
               upit = "INSERT INTO protokoli (broj_protokola, naziv, vrsta_protokola, ulaz_izlaz, datum) VALUES (@broj_protokola, @naziv, @vrsta_protokola, @ulaz_izlaz, @datum)";
            }
            else
            {
                upit = "UPDATE protokoli SET broj_protokola=@broj_protokola, naziv = @naziv, vrsta_protokola = @vrsta_protokola, ulaz_izlaz = @ulaz_izlaz, datum = @datum WHERE id = @id";
            }

            try
            {
                using(SqlCommand komanda=new SqlCommand(upit, konekcija))
                {
                    if (editId.HasValue)
                    {
                        komanda.Parameters.AddWithValue("@id", editId);
                    }

                    string brojProtokola = txtBrojProtokola.Text;
                    string vrsta = txtVrstaProtokola.Text;
                    string naziv = txtNazivProtokola.Text;
                    bool ulaz_izlaz = rbProtokolI.IsChecked ?? false;
                    DateTime datum = dpDatumProtokoli.SelectedDate.GetValueOrDefault();


                    komanda.Parameters.AddWithValue("@broj_protokola", brojProtokola);
                    komanda.Parameters.AddWithValue("@naziv", naziv);
                    komanda.Parameters.AddWithValue("@vrsta_protokola", vrsta);
                    komanda.Parameters.AddWithValue("@ulaz_izlaz", ulaz_izlaz ? 1 : 0);
                    komanda.Parameters.AddWithValue("@datum", datum);

                    komanda.ExecuteNonQuery();
                    MessageBox.Show("Protokol je uspjesno sacuvan.");

                    OnSaveCompleted?.Invoke();
                    OnDataChanged?.Invoke(null);
                    this.Close();

                }

            }
            catch (SqlException ex)
            {
                if (ex.Number == 1062)
                {
                    MessageBox.Show("Broj protokola vec postoji. Molimo unesite jedinstveni broj protokola.","Greska",MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Greska prilikom cuvanja dokumenta" + ex.Message);
                }
            }

        }

        private int ID;

        private void LoadProtokolData(int id)
        {
            try
            {
                string upit = "SELECT * FROM protokoli WHERE id=@ID";

                using (SqlCommand komanda = new SqlCommand(upit, konekcija))
                {
                    komanda.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = komanda.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ID = Convert.ToInt32(reader["id"]);
                            txtBrojProtokola.Text = reader["broj_protokola"].ToString();
                            txtNazivProtokola.Text = reader["naziv"].ToString();
                            txtVrstaProtokola.Text = reader["vrsta_protokola"].ToString();
                            DateTime datum = Convert.ToDateTime(reader["datum"]);
                            dpDatumProtokoli.SelectedDate = datum;

                            bool ulazIzlaz = Convert.ToBoolean(reader["ulaz_izlaz"]);
                            rbProtokolU.IsChecked = !ulazIzlaz;
                            rbProtokolI.IsChecked = ulazIzlaz;

                        }
                        else
                        {
                            MessageBox.Show("Protokol sa zadatim brojem nije pronađena.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            this.Close();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom učitavanja podataka protokola: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }




    }
}
