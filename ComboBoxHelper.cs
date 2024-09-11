using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPF_fakture_otpremnice
{
    public static class ComboBoxHelper
    {

        public static void FilterComboBox(ComboBox comboBox, TextBox filterTextBox)
        {
            if (comboBox.ItemsSource is DataView dataView)
            {
                var originalTable = dataView.ToTable();

                var filteredRows = originalTable.AsEnumerable()
                    .Where(row => row.Field<string>("ime")
                    .IndexOf(filterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);

                if (filteredRows.Any())
                {
                    var filteredTable = filteredRows.CopyToDataTable();
                    comboBox.ItemsSource = filteredTable.DefaultView;
                }
                else
                {
                      comboBox.ItemsSource = null;
                }
            }
        }
    }
}
