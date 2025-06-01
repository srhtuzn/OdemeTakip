using OdemeTakip.Data;
using OdemeTakip.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;

namespace OdemeTakip.Desktop.ViewModels
{
    public partial class UserManagementPanelView : UserControl
    {
        private User? _selectedUserEntity; // Düzenleme için seçili olan entity'yi tutar

        public UserManagementPanelView()
        {
            InitializeComponent();
            LoadRolesIntoComboBox();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            YenidenYukle();
        }

        public void YenidenYukle()
        {
            try
            {
                // Kullanıcıları veritabanından çekip DataGrid'e yükle
                UsersDataGrid.ItemsSource = App.DbContext.Users.AsNoTracking().ToList();
                ClearForm();
                SetInfoMessage("Kullanıcı listesi yenilendi.", Brushes.Green);
            }
            catch (Exception ex)
            {
                HandleError("Kullanıcılar yüklenirken bir hata oluştu.", ex);
            }
        }

        private void LoadRolesIntoComboBox()
        {
            RoleComboBox.ItemsSource = Enum.GetValues(typeof(UserRole))
                                          .Cast<UserRole>()
                                          .Select(role => new { Value = role, DisplayName = role.ToString() })
                                          .ToList();
            RoleComboBox.DisplayMemberPath = "DisplayName";
            RoleComboBox.SelectedValuePath = "Value";
        }

        private void ClearForm()
        {
            _selectedUserEntity = null; // Seçili kullanıcıyı temizle
            UsernameTextBox.Clear();
            FullNameTextBox.Clear(); // Tam ad alanını temizle
            EmailTextBox.Clear();
            PasswordBox.Clear();
            RoleComboBox.SelectedIndex = -1;
            IsActiveCheckBox.IsChecked = true;
            UsernameTextBox.IsEnabled = true; // Yeni kullanıcı için kullanıcı adı alanı aktif

            DeleteUserButton.IsEnabled = false;
            SaveUserButton.Content = "💾 Kaydet";
            FormInfoTextBlock.Text = "";
        }

        private void SetInfoMessage(string message, Brush color)
        {
            FormInfoTextBlock.Text = message;
            FormInfoTextBlock.Foreground = color;
        }

        private void HandleError(string userMessage, Exception? ex = null)
        {
            SetInfoMessage(userMessage, Brushes.Red);
            if (ex != null)
            {
                // Geliştirme aşamasında hatanın detayını görmek için:
                MessageBox.Show($"{userMessage}\n\nDetay: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Hata Oluştu", MessageBoxButton.OK, MessageBoxImage.Error);
                // Canlıda loglama mekanizması kullanılmalı.
            }
        }

        private void NewUserButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            UsernameTextBox.Focus();
            SetInfoMessage("Yeni kullanıcı bilgilerini girin.", Brushes.DodgerBlue);
        }

        private void RefreshUsersButton_Click(object sender, RoutedEventArgs e)
        {
            YenidenYukle();
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUserFromGrid)
            {
                _selectedUserEntity = selectedUserFromGrid; // Sadece Id'yi değil, tüm AsNoTracking entity'yi alıyoruz

                UsernameTextBox.Text = _selectedUserEntity.Username;
                FullNameTextBox.Text = _selectedUserEntity.FullName; // Tam ad alanını doldur
                EmailTextBox.Text = _selectedUserEntity.Email;
                PasswordBox.Clear(); // Şifre güvenlik için gösterilmez/doldurulmaz
                RoleComboBox.SelectedValue = _selectedUserEntity.Role;
                IsActiveCheckBox.IsChecked = _selectedUserEntity.IsActive;

                UsernameTextBox.IsEnabled = false; // Mevcut kullanıcının adı değiştirilemez
                DeleteUserButton.IsEnabled = true;
                SaveUserButton.Content = "💾 Güncelle";
                SetInfoMessage($"'{_selectedUserEntity.Username}' kullanıcısı düzenleniyor.", Brushes.DodgerBlue);
            }
        }

        private void SaveUserButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string fullName = FullNameTextBox.Text.Trim(); // Tam adı al
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                SetInfoMessage("Kullanıcı adı boş bırakılamaz.", Brushes.Red);
                return;
            }

            if (RoleComboBox.SelectedValue == null)
            {
                SetInfoMessage("Lütfen bir rol seçin.", Brushes.Red);
                return;
            }
            UserRole selectedRoleEnum = (UserRole)RoleComboBox.SelectedValue;

            try
            {
                if (_selectedUserEntity == null) // Yeni Kullanıcı Ekleme
                {
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        SetInfoMessage("Yeni kullanıcı için şifre boş bırakılamaz.", Brushes.Red);
                        return;
                    }
                    if (App.DbContext.Users.Any(u => u.Username == username))
                    {
                        SetInfoMessage("Bu kullanıcı adı zaten mevcut.", Brushes.Red);
                        return;
                    }

                    User newUser = new ()
                    {
                        Username = username,
                        PasswordHash = LoginWindow.HashPassword(password),
                        FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName, // Boşsa null ata
                        Email = string.IsNullOrWhiteSpace(email) ? null : email, // Boşsa null ata
                        Role = selectedRoleEnum,
                        IsActive = IsActiveCheckBox.IsChecked ?? true,
                    };
                    App.DbContext.Users.Add(newUser);
                    App.DbContext.SaveChanges();
                    SetInfoMessage($"'{newUser.Username}' kullanıcısı başarıyla eklendi.", Brushes.Green);
                }
                else // Mevcut Kullanıcı Güncelleme (Entity Framework Takip Hatası Düzeltildi)
                {
                    // Veritabanından asıl, izlenen (tracked) kullanıcıyı bul
                    var userToUpdate = App.DbContext.Users.FirstOrDefault(u => u.Id == _selectedUserEntity.Id);

                    if (userToUpdate == null)
                    {
                        HandleError("Güncellenecek kullanıcı veritabanında bulunamadı. Lütfen listeyi yenileyin.");
                        return;
                    }

                    // Değişiklikleri formdan alıp izlenen entity'ye aktar
                    // Username değiştirilmiyor (UsernameTextBox.IsEnabled = false;)
                    userToUpdate.FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName;
                    userToUpdate.Email = string.IsNullOrWhiteSpace(email) ? null : email;
                    userToUpdate.Role = selectedRoleEnum;
                    userToUpdate.IsActive = IsActiveCheckBox.IsChecked ?? true;

                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        userToUpdate.PasswordHash = LoginWindow.HashPassword(password);
                    }

                    // EF Core zaten 'userToUpdate' nesnesini izliyor, değişiklikleri algılayacak.
                    // App.DbContext.Users.Update(userToUpdate); // Bu satıra gerek yok
                    App.DbContext.SaveChanges();
                    SetInfoMessage($"'{userToUpdate.Username}' kullanıcısı başarıyla güncellendi.", Brushes.Green);
                }
                YenidenYukle(); // Listeyi ve formu güncelle
            }
            catch (Exception ex)
            {
                HandleError("Kullanıcı kaydedilirken bir hata oluştu.", ex);
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserEntity == null)
            {
                SetInfoMessage("Lütfen silmek için bir kullanıcı seçin.", Brushes.OrangeRed);
                return;
            }

            if (App.CurrentUser != null && _selectedUserEntity.Id == App.CurrentUser.Id)
            {
                HandleError("Aktif kullanıcı kendisini silemez!");
                return;
            }

            MessageBoxResult result = MessageBox.Show($"'{_selectedUserEntity.Username}' kullanıcısını silmek istediğinizden emin misiniz?\nBu işlem geri alınamaz.",
                                                      "Kullanıcı Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Silmeden önce entity'nin context tarafından izlendiğinden emin olalım
                    var userToDelete = App.DbContext.Users.FirstOrDefault(u => u.Id == _selectedUserEntity.Id);
                    if (userToDelete != null)
                    {
                        App.DbContext.Users.Remove(userToDelete);
                        App.DbContext.SaveChanges();
                        SetInfoMessage($"'{_selectedUserEntity.Username}' kullanıcısı başarıyla silindi.", Brushes.Green);
                    }
                    else
                    {
                        HandleError("Silinecek kullanıcı bulunamadı.");
                    }
                    YenidenYukle();
                }
                catch (Exception ex)
                {
                    HandleError("Kullanıcı silinirken bir hata oluştu.", ex);
                }
            }
        }
    }
}