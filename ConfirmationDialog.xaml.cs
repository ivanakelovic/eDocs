﻿using System;
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

namespace WPF_fakture_otpremnice
{
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog(string message)
        {
            InitializeComponent();
            lblMessage.Text = message;
        }

        private void neButton_Click(object sender, RoutedEventArgs e)
        {
          this.Close();
        }

        private void daButton_Click(object sender,RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
