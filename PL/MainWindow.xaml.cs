﻿using System.Windows;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BlApi.IBl bl = BlApi.Factory.Get();
        /// <summary>
        /// Constractor of MainWindow.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// A function that opens the ProductListWindow.
        /// </summary>
        private void ShowProductsButton_Click(object sender, RoutedEventArgs e)
        {
            new ProductListWindow(bl,"admin").Show();
            this.Hide();
        }

        private void ShowCustomerWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            new ProductListWindow(bl,"customer").Show();
            this.Hide();
        }
    }
}
