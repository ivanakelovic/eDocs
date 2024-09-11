using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPF_fakture_otpremnice
{
    public static class FirmaService
    {
        public static DataTable DohvatiImenaFirmi(SqlConnection konekcija)
        {
            DataTable dt = new DataTable();
            try
            {
                if (konekcija.State == ConnectionState.Closed)
                {
                    konekcija.Open();
                }

                string upit = "SELECT ID, ime FROM firme ORDER BY ime";
                SqlCommand komanda = new SqlCommand(upit, konekcija);
                SqlDataAdapter da = new SqlDataAdapter(komanda);
                da.Fill(dt);

                DataRow defaultRow = dt.NewRow();
                defaultRow["ID"] = 0;
                defaultRow["ime"] = "Odaberite firmu...";
                dt.Rows.InsertAt(defaultRow, 0);
            }
            catch (Exception ex)
            {  
                throw new Exception("Greška prilikom popunjavanja liste firmi: " + ex.Message);
            }

            return dt;
        }
    }


}
