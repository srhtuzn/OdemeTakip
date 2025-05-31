using OdemeTakip.Data;
using OdemeTakip.Entities;
using System;
using System.Linq;
using System.Windows;

namespace OdemeTakip.Desktop
{
    public partial class CariFirmaForm : Window
    {
        private readonly AppDbContext _db;
        private readonly CariFirma _firma;
        private readonly bool _isEdit;

        public CariFirmaForm(AppDbContext db, CariFirma? firma = null)
        {
            InitializeComponent();

            _db = db;
            _firma = firma ?? new CariFirma();
            _isEdit = firma != null;

            BankalariYukle();

            if (_isEdit)
            {
                txtName.Text = _firma.Name;
                txtTaxNumber.Text = _firma.TaxNumber;
                txtTaxOffice.Text = _firma.TaxOffice;
                txtPhone.Text = _firma.Phone;
                txtEmail.Text = _firma.Email;
                txtIban.Text = _firma.Iban;
                cmbBanka.Text = _firma.Banka;
                txtContact.Text = _firma.ContactPerson;
                txtAddress.Text = _firma.Address;
                txtNotes.Text = _firma.Notes;
            }
        }

        private void BankalariYukle()
        {
            cmbBanka.ItemsSource = _db.Bankalar
                .Where(b => b.IsActive)
                .Select(b => b.Adi)
                .ToList();
        }
        private string CariKoduUret()
        {
            int mevcut = _db.CariFirmalar.Count() + 1;
            return $"C{mevcut.ToString("D4")}";
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            _firma.Name = txtName.Text.Trim();
            _firma.TaxNumber = txtTaxNumber.Text.Trim();
            _firma.TaxOffice = txtTaxOffice.Text.Trim();
            _firma.Phone = txtPhone.Text.Trim();
            _firma.Email = txtEmail.Text.Trim();
            _firma.Iban = txtIban.Text.Trim();
            _firma.Banka = cmbBanka.Text.Trim(); // Bank → Banka
            _firma.ContactPerson = txtContact.Text.Trim();
            _firma.Address = txtAddress.Text.Trim();
            _firma.Notes = txtNotes.Text.Trim();
            _firma.IsActive = true;
            if (!_isEdit)
                _firma.CariKodu = CariKoduUret();

            if (_isEdit)
                _db.CariFirmalar.Update(_firma);
            else
                _db.CariFirmalar.Add(_firma);

            _db.SaveChanges();
            DialogResult = true;
            Close();
        }
    }
}
