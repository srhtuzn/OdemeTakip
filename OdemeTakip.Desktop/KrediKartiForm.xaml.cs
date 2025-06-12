using System;
using System.Windows;
using OdemeTakip.Data;
using OdemeTakip.Entities;
using OdemeTakip.Desktop.Helpers;


namespace OdemeTakip.Desktop
{
    public partial class KrediKartiForm : Window
    {
        private readonly AppDbContext _db;
        private readonly KrediKarti _kart;
        private readonly bool _isEdit;
        

        public KrediKartiForm(AppDbContext db, KrediKarti? kart = null)
        {
            InitializeComponent();

            _db = db;
            _kart = kart ?? new KrediKarti();
            _isEdit = kart != null;
            LoadCompanies();

           
            BankalariYukle();

            if (_isEdit)
            {
                txtCardName.Text = _kart.CardName;
                cmbOwnerCompany.SelectedValue = _kart.CompanyId;  // 🔥 Seçili Company I
                txtCardNumberLast4.Text = _kart.CardNumberLast4;
                txtLimit.Text = _kart.Limit.ToString();
                dpDueDate.SelectedDate = _kart.DueDate;
                dpPaymentDueDate.SelectedDate = _kart.PaymentDueDate;
                txtNotes.Text = _kart.Notes;
                cmbBanka.Text = _kart.Banka;
            }
        }
        private void LoadCompanies()
        {
            cmbOwnerCompany.ItemsSource = _db.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();

            cmbOwnerCompany.DisplayMemberPath = "Name";     // Şirket ismini göster
            cmbOwnerCompany.SelectedValuePath = "Id";       // Şirket Id'si kaydolacak
        }


        private void BankalariYukle()
        {
            cmbBanka.ItemsSource = _db.Bankalar
                .Where(b => b.IsActive)
                .Select(b => b.Adi)
                .ToList();
        }



        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            _kart.CardName = txtCardName.Text.Trim();

            if (cmbOwnerCompany.SelectedValue is int companyId)
                _kart.CompanyId = companyId;

            _kart.Banka = cmbBanka.Text.Trim();
            _kart.CardNumberLast4 = txtCardNumberLast4.Text.Trim();
            _kart.Notes = txtNotes.Text.Trim();
            _kart.IsActive = true;

            if (decimal.TryParse(txtLimit.Text, out var limit))
                _kart.Limit = limit;

            if (dpDueDate.SelectedDate != null)
                _kart.DueDate = dpDueDate.SelectedDate.Value;

            if (dpPaymentDueDate.SelectedDate != null)
                _kart.PaymentDueDate = dpPaymentDueDate.SelectedDate.Value;

            if (_isEdit)
            {
                _db.KrediKartlari.Update(_kart);

                // 🚀 Buraya EKLE
                var eskiOdemeler = _db.KrediKartiOdemeleri.Where(x => x.KartAdi == _kart.CardName).ToList();
                foreach (var odeme in eskiOdemeler)
                {
                    odeme.CompanyId = _kart.CompanyId;
                    odeme.KrediKartiId = _kart.Id;   // 🔥 Bu satırı EKLE!
                }
                _db.KrediKartiOdemeleri.UpdateRange(eskiOdemeler);

            }
            else
            {
                _db.KrediKartlari.Add(_kart);
            }

            _db.SaveChanges();

            DialogResult = true;
            Close();
        }

    }
}

