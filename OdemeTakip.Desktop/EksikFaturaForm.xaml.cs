using OdemeTakip.Data;
using OdemeTakip.Desktop.ViewModels;
using OdemeTakip.Entities;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OdemeTakip.Desktop
{
    public partial class EksikFaturaForm : Window
    {
        private readonly AppDbContext _db;
        private readonly bool _isEdit;
        private readonly int? _faturaId;

        public string Kod { get; private set; }
        public string GiderTuru => cmbGiderTuru.Text.Trim();
        public string Aciklama => txtAciklama.Text.Trim();
        public DateTime? Tarih => dpTarih.SelectedDate;
        public decimal? Tutar => decimal.TryParse(txtTutar.Text.Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : (decimal?)null;
        public string FaturaNo => txtFaturaNo.Text.Trim(); // ✅ YENİ EKLENDİ
        public string ParaBirimi => cmbParaBirimi.SelectedItem?.ToString();
        public int? SirketId => cmbSirket.SelectedValue as int?;
        public int? CariFirmaId => cmbCariFirma.SelectedValue as int?;

        public string Sirket => (cmbSirket.SelectedItem as Company)?.Name;
        public string CariFirma => (cmbCariFirma.SelectedItem as CariFirma)?.Name;

        public bool Kaydedildi { get; private set; }

        public EksikFaturaForm(AppDbContext db, int? faturaId = null)
        {
            InitializeComponent();
            _db = db;
            _faturaId = faturaId;
            _isEdit = faturaId.HasValue;

            YukleComboBoxlar();

            if (_isEdit)
            {
                FaturaYukle(faturaId.Value);
            }
            else
            {
                Kod = KodUret();
                txtKod.Text = Kod;
            }
        }

        private void YukleComboBoxlar()
        {
            cmbGiderTuru.ItemsSource = new[] { "Elektrik", "Su", "Doğalgaz", "Telefon", "İnternet" };

            cmbSirket.ItemsSource = _db.Companies
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToList();
            cmbSirket.DisplayMemberPath = "Name";
            cmbSirket.SelectedValuePath = "Id";

            cmbCariFirma.ItemsSource = _db.CariFirmalar
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToList();
            cmbCariFirma.DisplayMemberPath = "Name";
            cmbCariFirma.SelectedValuePath = "Id";

            cmbParaBirimi.ItemsSource = new[] { "TL", "USD", "EUR" };
            cmbParaBirimi.SelectedIndex = 0;
        }

        private void FaturaYukle(int id)
        {
            var fatura = _db.DegiskenOdemeler.FirstOrDefault(x => x.Id == id);
            if (fatura != null)
            {
                txtKod.Text = fatura.OdemeKodu;
                cmbGiderTuru.Text = fatura.GiderTuru;
                txtAciklama.Text = fatura.Aciklama;
                dpTarih.SelectedDate = fatura.OdemeTarihi;
                txtTutar.Text = fatura.Tutar.ToString("N2");
                txtFaturaNo.Text = fatura.FaturaNo; // ✅ YENİ EKLENDİ
                cmbParaBirimi.Text = fatura.ParaBirimi;
                cmbSirket.SelectedValue = fatura.CompanyId;
                cmbCariFirma.SelectedValue = fatura.CariFirmaId;
            }
        }

        public void LoadFromViewModel(EksikFaturaViewModel vm)
        {
            cmbGiderTuru.Text = vm.GiderTuru;
            txtAciklama.Text = vm.Aciklama;
            dpTarih.SelectedDate = vm.Tarih;
            txtTutar.Text = vm.Tutar.ToString("N2");
            txtFaturaNo.Text = vm.FaturaNo; // ✅ YENİ EKLENDİ
            cmbParaBirimi.Text = vm.ParaBirimi;

            var sirket = _db.Companies.FirstOrDefault(x => x.Name == vm.SirketAdi);
            if (sirket != null)
                cmbSirket.SelectedValue = sirket.Id;

            var cariFirma = _db.CariFirmalar.FirstOrDefault(x => x.Name == vm.CariFirmaAdi);
            if (cariFirma != null)
                cmbCariFirma.SelectedValue = cariFirma.Id;
        }

        private string KodUret()
        {
            int adet = _db.DegiskenOdemeler.Count(x => x.OdemeKodu.StartsWith("AO")) + 1;
            return $"AO{adet:D4}";
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                if (_isEdit)
                {
                    var fatura = _db.DegiskenOdemeler.FirstOrDefault(x => x.Id == _faturaId);
                    if (fatura != null)
                    {
                        fatura.GiderTuru = GiderTuru;
                        fatura.Aciklama = Aciklama;
                        fatura.OdemeTarihi = Tarih ?? DateTime.Now;
                        fatura.Tutar = Tutar ?? 0;
                        fatura.FaturaNo = FaturaNo; // ✅ Fatura No Kaydı
                        fatura.ParaBirimi = ParaBirimi;
                        fatura.CompanyId = SirketId;
                        fatura.CariFirmaId = CariFirmaId;
                    }
                }
                else
                {
                    var yeniFatura = new DegiskenOdeme
                    {
                        OdemeKodu = Kod,
                        GiderTuru = GiderTuru,
                        Aciklama = Aciklama,
                        OdemeTarihi = Tarih ?? DateTime.Now,
                        Tutar = Tutar ?? 0,
                        FaturaNo = FaturaNo, // ✅ Fatura No Kaydı
                        ParaBirimi = ParaBirimi,
                        CompanyId = SirketId,
                        CariFirmaId = CariFirmaId,
                        IsActive = true,
                        OdenmeDurumu = false
                    };

                    _db.DegiskenOdemeler.Add(yeniFatura);
                }

                _db.SaveChanges(); // ✅ Veritabanına kaydet

                Kaydedildi = true;
                this.DialogResult = true;
                this.Close();
            }
        }


        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(GiderTuru))
            {
                MessageBox.Show("Lütfen gider türü seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Aciklama))
            {
                MessageBox.Show("Lütfen açıklama girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Tarih.HasValue)
            {
                MessageBox.Show("Lütfen tarih seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Tutar.HasValue || Tutar <= 0)
            {
                MessageBox.Show("Geçerli bir tutar girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ParaBirimi))
            {
                MessageBox.Show("Lütfen para birimi seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!SirketId.HasValue)
            {
                MessageBox.Show("Lütfen şirket seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!CariFirmaId.HasValue)
            {
                MessageBox.Show("Lütfen cari firma seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
