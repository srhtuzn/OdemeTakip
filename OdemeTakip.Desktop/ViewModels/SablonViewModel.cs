// OdemeTakip.Desktop.ViewModels/SablonViewModel.cs
using System;
using System.ComponentModel;

namespace OdemeTakip.Desktop.ViewModels
{
    public class SablonViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }

        private string _giderTuru = "";
        public string GiderTuru
        {
            get => _giderTuru;
            set
            {
                if (_giderTuru != value)
                {
                    _giderTuru = value;
                    OnPropertyChanged(nameof(GiderTuru));
                }
            }
        }

        private string _aciklama = "";
        public string Aciklama
        {
            get => _aciklama;
            set
            {
                if (_aciklama != value)
                {
                    _aciklama = value;
                    OnPropertyChanged(nameof(Aciklama));
                }
            }
        }

        private int _gun;
        public int Gun
        {
            get => _gun;
            set
            {
                if (_gun != value)
                {
                    _gun = value;
                    OnPropertyChanged(nameof(Gun));
                }
            }
        }

        private string _sirketAdi = "";
        public string SirketAdi
        {
            get => _sirketAdi;
            set
            {
                if (_sirketAdi != value)
                {
                    _sirketAdi = value;
                    OnPropertyChanged(nameof(SirketAdi));
                }
            }
        }

        private string _cariFirmaAdi = "";
        public string CariFirmaAdi
        {
            get => _cariFirmaAdi;
            set
            {
                if (_cariFirmaAdi != value)
                {
                    _cariFirmaAdi = value;
                    OnPropertyChanged(nameof(CariFirmaAdi));
                }
            }
        }

        private string _paraBirimi = "TL";
        public string ParaBirimi
        {
            get => _paraBirimi;
            set
            {
                if (_paraBirimi != value)
                {
                    _paraBirimi = value;
                    OnPropertyChanged(nameof(ParaBirimi));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}