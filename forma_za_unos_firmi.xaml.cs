using System;
using System.Windows;
using CG.Web.MegaApiClient;
using Microsoft.Data.SqlClient;

namespace WPF_fakture_otpremnice
{
    public partial class forma_za_unos_firmi : Window
    {
        private SqlConnection konekcija;
        private MegaApiClient megaClient;
        private bool isLoggedIn;
        private int? editId;
       
        public forma_za_unos_firmi(SqlConnection existingConnection, MegaApiClient megaClient, bool isLoggedIn)
        {
            InitializeComponent();
            konekcija = existingConnection;
            this.megaClient = megaClient;
            this.isLoggedIn = isLoggedIn;
         editId= null;
         
        }

        public forma_za_unos_firmi(SqlConnection existingConnection, MegaApiClient megaClient, bool isLoggedIn, int id)
        {
            InitializeComponent();
            konekcija = existingConnection;
            this.megaClient = megaClient;
            this.isLoggedIn = isLoggedIn;
            editId = id;
            LoadFirmData(id);
        }

        public event Action OnDataChanged;
        private void btn_Sacuvaj(object sender, RoutedEventArgs e)
        {
            string jib= txtJIB.Text;
            string ime = txtIme.Text;
            string adresa = txtAdresa.Text.Trim();
            string telefon = txtTelefon.Text;
            string email = txtEmail.Text;
            string ziroRacun= txtZiroRacun.Text;

            if (
                string.IsNullOrEmpty(jib) ||
                string.IsNullOrEmpty(ime) ||
                string.IsNullOrEmpty(adresa)||
                string.IsNullOrEmpty(telefon)||
                    string.IsNullOrEmpty(email)||
                    string.IsNullOrEmpty(ziroRacun)
                )
            {
                MessageBox.Show("Molimo unesite sve potrebne informacije.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string upit;
                if (editId == null)
                {
                    upit = "INSERT INTO firme (JIB,ime, adresa,telefon,email,ziro_racun) VALUES (@JIB, @ime, @adresa,@telefon,@email,@ziro_racun)";

                }
                else
                {
                    upit = "UPDATE firme SET JIB=@JIB,ime=@ime, adresa=@adresa,telefon=@telefon,email=@email,ziro_racun=@ziro_racun WHERE ID=@ID";
                }

                using (SqlCommand komanda = new SqlCommand(upit, konekcija))
                {
                    if (editId != null)
                    {
                        komanda.Parameters.AddWithValue("@ID", editId.Value);
                    }
                    komanda.Parameters.AddWithValue("@JIB", jib);
                    komanda.Parameters.AddWithValue("@ime", ime);
                    komanda.Parameters.AddWithValue("@adresa", adresa);
                    komanda.Parameters.AddWithValue("@telefon", telefon);
                    komanda.Parameters.AddWithValue("@email", email);
                    komanda.Parameters.AddWithValue("@ziro_racun", ziroRacun);

                    int affectedRows = komanda.ExecuteNonQuery();
                    if (affectedRows > 0)
                    {
                        MessageBox.Show("Firma je uspješno spremljena.", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);

                        this.Close();
                        OnDataChanged?.Invoke();
                    }
                    else
                    {
                        MessageBox.Show("Spremanje firme nije uspjelo.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom spremanja firme: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private int ID;
        private void LoadFirmData(int id)
        {
            try
            {
                string upit = "SELECT * FROM firme WHERE ID=@ID";

                using (SqlCommand komanda = new SqlCommand(upit, konekcija))
                {
                    komanda.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = komanda.ExecuteReader())
                    {
                        if (reader.Read()) {
                            ID = Convert.ToInt32(reader["ID"]);
                            txtJIB.Text = reader["JIB"].ToString();
                            txtIme.Text = reader["ime"].ToString();
                            txtAdresa.Text = reader["adresa"].ToString();
                            txtTelefon.Text = reader["telefon"].ToString();
                            txtEmail.Text = reader["email"].ToString();
                            txtZiroRacun.Text = reader["ziro_racun"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Firma sa zadatim JIB-om nije pronađena.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            this.Close();
                        }

                    }
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom učitavanja podataka firme: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
    
    }
}
