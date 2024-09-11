using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using AutoUpdaterDotNET;
using CG.Web.MegaApiClient;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Configuration;
using WPF_fakture_otpremnice;

namespace WPF_fakture_otpremnice
{
    public partial class MainWindow : Window
    {
        private SqlConnection konekcija;
        private MegaApiClient megaClient;
        private bool isLoggedIn;
        private bool isAdmin;

        public MainWindow()
        {
            InitializeComponent();
            konekcija = Konekcija.GetConnection();
            megaClient=new MegaApiClient();

            AutoUpdater.Start("https://mega.nz/file/mcFCjarJ#JlDKjFLgXWM5eyORrm1Y6TxGcLJlWiVVBCghuhGMHS4");
            //UpdateUIBasedOnUser();

            AutoUpdater.CheckForUpdateEvent += HandleCheckForUpdateEvent;

            
            if (Settings.Default.RememberMe)
            {
                txtEmail.Text = Settings.Default.SavedEmail;
                txtLozinka.Password = Settings.Default.SavedPassword;
                chkRememberMe.IsChecked = true;
            }


        }
        private void HandleCheckForUpdateEvent(object sender) 
        {
            if (sender is UpdateInfoEventArgs e)
            {
                if (e.IsUpdateAvailable)
                {
                    MessageBox.Show($"Nova verzija aplikacije dostupna: {e.CurrentVersion}\n\n", "Ažuriranje dostupno", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    //MessageBox.Show("Vaša aplikacija je ažurirana na najnoviju verziju.", "Ažuriranje", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            { 
                MessageBox.Show("Unexpected event data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrijaviSe(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string lozinka = txtLozinka.Password;


            bool isValidUser = ProvjeriKorisnika(email, lozinka);

            if (isValidUser)
            {
                Settings.Default.SavedEmail = email;
                Settings.Default.SavedPassword = lozinka;
                Settings.Default.RememberMe = chkRememberMe.IsChecked ?? false;
                Settings.Default.Save();

                //UpdateUIBasedOnUser();
                Pocetna pocetnaProzor = new Pocetna(konekcija, isAdmin);
                pocetnaProzor.Show();

                this.Close();
            }
            else
            {
                MessageBox.Show("Neispravni podaci!", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool ProvjeriKorisnika(string email, string lozinka)
        {
            bool rezultat = false;
            try
            {

                string upit = "SELECT * FROM korisnici WHERE email = @Email";
                using (SqlCommand komanda = new SqlCommand(upit, konekcija))
                {
                    komanda.Parameters.AddWithValue("@Email", email);

                    using (SqlDataReader reader = komanda.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            string lozinkaHash = reader["lozinka"].ToString();
                            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(lozinka, lozinkaHash);
                            bool isAccountApproved = reader.GetBoolean(reader.GetOrdinal("odobren_nalog"));
                            string tipKorisnika = reader["tip_korisnika"].ToString();

                            if (tipKorisnika == "Administrator")
                            {
                                isAdmin = true;
                            }
                            else
                            {
                                isAdmin = false;
                            }


                            if (isPasswordValid && isAccountApproved)
                            {
                                rezultat = true;
                            }
                            else if (!isPasswordValid)
                            {
                                MessageBox.Show("Neispravni podaci!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                MessageBox.Show("Vaš nalog nije odobren!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                                Application.Current.Shutdown();
                            }

                        }
                        else
                        {
                            MessageBox.Show("Neispravni podaci!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                       

                    }
                }
            }


            catch (Exception e)
            {
                MessageBox.Show($"Greška prilikom provjere korisnika: {e.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return rezultat;
        }

        private void btnRegister(object sender, RoutedEventArgs e)
        {
            registerAccount forma=new registerAccount(konekcija);
            forma.Show();
        }

      
    }
}


