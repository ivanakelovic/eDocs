using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace WPF_fakture_otpremnice
{
    internal class Konekcija
    {
        public static SqlConnection konekcija;

        public static SqlConnection GetConnection()
        {
            if(konekcija==null)
            {
                string imeRacunara = Environment.MachineName;
                string connectionString;

                if(imeRacunara== "DESKTOP")
                {
                    connectionString = "Server=DESKTOP\\SQLEXPRESS; Database=dokumenti; User Id=sa; Password=Proinzenjering1*; TrustServerCertificate=True;MultipleActiveResultSets=True;";

                }
                else
                {
                    connectionString = "Server=.175,1433; Database=dokumenti; User Id=sa; Password=Proinzenjering1*; TrustServerCertificate=True;MultipleActiveResultSets=True;";

                }
                konekcija = new SqlConnection(connectionString);

                 try
                {
                    konekcija.Open();
                }
                catch(Exception ex) {
                    MessageBox.Show($"Greška prilikom otvaranja konekcije: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
            }
            return konekcija;
        }

        public static void CloseConnection()
        {
            if(konekcija!=null)
            {
                konekcija.Close();
                konekcija = null;
            }
        }

    }
}
