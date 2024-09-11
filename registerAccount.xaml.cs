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
using Microsoft.Data.SqlClient;

namespace WPF_fakture_otpremnice
{
    /// <summary>
    /// Interaction logic for registerAccount.xaml
    /// </summary>
    public partial class registerAccount : Window
    {
        private SqlConnection konekcija;
        public registerAccount(SqlConnection konekcija)
        {
            InitializeComponent();
            this.konekcija = konekcija;
            dohvatiImenaFirmi();
        }

        private void btnRegistrujSe(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string lozinka = txtLozinka.Password;
            string ime = txtImeKorisnika.Text;
            string prezime = txtPrezime.Text;
            string firmaId = cmbFirma.SelectedValue.ToString();
       
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(lozinka);

            try
            {
                string query = "INSERT INTO korisnici (email, ime, prezime, firma, lozinka) VALUES (@email, @ime, @prezime, @firma, @lozinka)";

                using (SqlCommand command = new SqlCommand(query, konekcija))
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@ime", ime);
                    command.Parameters.AddWithValue("@prezime", prezime);
                    command.Parameters.AddWithValue("@firma", firmaId);
                    command.Parameters.AddWithValue("@lozinka", hashedPassword);

                    int result = command.ExecuteNonQuery();
                    konekcija.Close();

                    if (result > 0)
                    {
                        MessageBox.Show("Registracija uspešna!", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Registracija nije uspela.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom registracije: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
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




    }
}
