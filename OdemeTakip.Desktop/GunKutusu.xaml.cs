using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class GunKutusu : UserControl
    {
        public DateTime Tarih { get; set; }

        private string _baslik = "";
        public string Baslik
        {
            get => _baslik;
            set
            {
                _baslik = value;
                txtGun.Text = value;
            }
        }

        private List<string> _odemeler = new();
        public List<string> Odemeler
        {
            get => _odemeler;
            set
            {
                _odemeler = value;
                lstOdemeler.ItemsSource = _odemeler;
            }
        }

        public GunKutusu()
        {
            InitializeComponent();
        }
    }
}
