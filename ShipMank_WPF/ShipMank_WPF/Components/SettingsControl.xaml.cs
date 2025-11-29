using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Components
{
    public partial class SettingsControl : UserControl
    {
        // Flag untuk melacak mode Edit/Save Personal Info
        private bool isEditing = false;
        public event EventHandler DeleteAccountRequested;

        // Menyimpan data user yang sedang login
        private User _currentUser;

        public SettingsControl()
        {
            InitializeComponent();
            PopulateDateComboBoxes();

            // Menghubungkan event Loaded
            this.Loaded += SettingsControl_Loaded;
        }

        private async void SettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Atur form ke mode ReadOnly saat pertama kali dimuat
            SetEditMode(false);

            // 2. Ambil data profil Google
            try
            {
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfiguration configuration = builder.Build();

                string clientId = configuration["GoogleAuth:ClientId"];
                string clientSecret = configuration["GoogleAuth:ClientSecret"];

                // Jika config belum diset, skip Google Auth
                if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
                {
                    string[] scopes = { Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile };

                    UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                        scopes,
                        "temp_user_id",
                        CancellationToken.None,
                        new FileDataStore("ShipMank.GoogleAuthStore")
                    );

                    if (credential != null)
                    {
                        var oauthService = new Oauth2Service(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = "ShipMank_WPF",
                        });

                        Userinfo profile = await oauthService.Userinfo.Get().ExecuteAsync();

                        // 3. Cari user di database berdasarkan email
                        _currentUser = User.GetUserByEmail(profile.Email);

                        // 4. Jika user belum ada, buat user baru (untuk Google Login)
                        if (_currentUser == null)
                        {
                            var newUser = new User();
                            bool registered = newUser.Register(
                                username: profile.Email.Split('@')[0], // Username dari email
                                passwordRaw: Guid.NewGuid().ToString(), // Password random untuk Google login
                                email: profile.Email,
                                name: profile.Name
                            );

                            if (registered)
                            {
                                _currentUser = User.GetUserByEmail(profile.Email);
                                if (_currentUser != null)
                                {
                                    _currentUser.IsGoogleLogin = true;
                                    UpdateIsGoogleLogin(_currentUser.UserID, true);
                                }
                            }
                        }

                        // 5. Load data user ke UI
                        LoadUserDataToUI();
                    }
                }
                else
                {
                    // Dummy data jika tidak ada Google Auth config
                    LoadDummyUserData();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat profil: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback ke offline user
                LoadDummyUserData();
            }
        }

        /// <summary>
        /// Load dummy data untuk development tanpa Google Auth
        /// </summary>
        private void LoadDummyUserData()
        {
            FullNameTextBox.Text = "User Local";
            EmailTextBox.Text = "user@local.com";
            GenderComboBox.SelectedIndex = 0;
            CmbBirthDay.SelectedIndex = 0;
            CmbBirthMonth.SelectedIndex = 0;
            CmbBirthYear.SelectedIndex = 0;
        }

        /// <summary>
        /// Load data user dari objek _currentUser ke UI
        /// </summary>
        private void LoadUserDataToUI()
        {
            if (_currentUser == null) return;

            FullNameTextBox.Text = _currentUser.Name ?? "";
            EmailTextBox.Text = _currentUser.Email ?? "";

            // Load Gender
            if (!string.IsNullOrEmpty(_currentUser.Gender))
            {
                // Cari index berdasarkan value gender
                for (int i = 0; i < GenderComboBox.Items.Count; i++)
                {
                    var item = GenderComboBox.Items[i] as ComboBoxItem;
                    if (item != null && item.Content.ToString().Equals(_currentUser.Gender, StringComparison.OrdinalIgnoreCase))
                    {
                        GenderComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                GenderComboBox.SelectedIndex = 0; // Default
            }

            // Load Birth Date
            if (_currentUser.TTL.HasValue)
            {
                DateTime birthDate = _currentUser.TTL.Value;

                // Set Hari
                CmbBirthDay.SelectedItem = birthDate.Day.ToString();

                // Set Bulan
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(birthDate.Month);
                CmbBirthMonth.SelectedItem = monthName;

                // Set Tahun
                CmbBirthYear.SelectedItem = birthDate.Year.ToString();
            }
            else
            {
                // Reset ke default jika tidak ada data
                CmbBirthDay.SelectedIndex = 0;
                CmbBirthMonth.SelectedIndex = 0;
                CmbBirthYear.SelectedIndex = 0;
            }
        }

        private void PopulateDateComboBoxes()
        {
            // Isi Hari
            CmbBirthDay.Items.Add("Hari");
            for (int i = 1; i <= 31; i++) CmbBirthDay.Items.Add(i.ToString());
            CmbBirthDay.SelectedIndex = 0;

            // Isi Bulan
            CmbBirthMonth.Items.Add("Bulan");
            string[] monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames;
            foreach (string month in monthNames)
            {
                if (!string.IsNullOrEmpty(month)) CmbBirthMonth.Items.Add(month);
            }
            CmbBirthMonth.SelectedIndex = 0;

            // Isi Tahun
            CmbBirthYear.Items.Add("Tahun");
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i >= currentYear - 100; i--) CmbBirthYear.Items.Add(i.ToString());
            CmbBirthYear.SelectedIndex = 0;
        }

        // ============================================================
        // LOGIC: PERSONAL INFO EDIT
        // ============================================================
        private void EditSaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Balikkan status editing
            isEditing = !isEditing;

            // Jika tombol "Save" baru saja diklik (sekarang mode = false)
            if (!isEditing)
            {
                // Validasi dan simpan data
                if (SaveUserData())
                {
                    MessageBox.Show("Personal information updated successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Kembalikan ke mode edit jika gagal save
                    isEditing = true;
                }
            }

            // Terapkan mode baru
            SetEditMode(isEditing);
        }

        /// <summary>
        /// Menyimpan data user ke database PostgreSQL
        /// </summary>
        private bool SaveUserData()
        {
            try
            {
                if (_currentUser == null)
                {
                    MessageBox.Show("User data not loaded.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Validasi input
                string fullName = FullNameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(fullName))
                {
                    MessageBox.Show("Full name cannot be empty.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Ambil Gender
                string gender = null;
                if (GenderComboBox.SelectedItem is ComboBoxItem selectedGender)
                {
                    string genderValue = selectedGender.Content.ToString();
                    if (genderValue != "Gender") // Jika bukan placeholder
                    {
                        gender = genderValue;
                    }
                }

                // Parse Birth Date
                DateTime? birthDate = ParseBirthDate();

                // Update ke database
                bool success = UpdateUserInDatabase(_currentUser.UserID, fullName, gender, birthDate);

                if (!success)
                {
                    MessageBox.Show("Failed to update user data.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Update objek _currentUser setelah sukses simpan
                _currentUser.Name = fullName;
                _currentUser.Gender = gender;
                _currentUser.TTL = birthDate;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Update data user ke database PostgreSQL
        /// </summary>
        private bool UpdateUserInDatabase(int userID, string name, string gender, DateTime? ttl)
        {
            string sql = @"
                UPDATE Users 
                SET name = @name, 
                    gender = @gender, 
                    ttl = @ttl
                WHERE userID = @userID";

            using (var conn = new Npgsql.NpgsqlConnection(DBHelper.GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("userID", userID);
                        cmd.Parameters.AddWithValue("name", (object)name ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("gender", (object)gender ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("ttl", (object)ttl ?? DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Npgsql.NpgsqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Update DB Error: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Update flag IsGoogleLogin di database
        /// </summary>
        private void UpdateIsGoogleLogin(int userID, bool isGoogleLogin)
        {
            string sql = "UPDATE Users SET isGoogleLogin = @isGoogleLogin WHERE userID = @userID";

            using (var conn = new Npgsql.NpgsqlConnection(DBHelper.GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("userID", userID);
                        cmd.Parameters.AddWithValue("isGoogleLogin", isGoogleLogin);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Npgsql.NpgsqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Update IsGoogleLogin Error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Parse tanggal lahir dari 3 ComboBox
        /// </summary>
        private DateTime? ParseBirthDate()
        {
            // Cek apakah semua ComboBox sudah dipilih (tidak di placeholder)
            if (CmbBirthDay.SelectedIndex <= 0 ||
                CmbBirthMonth.SelectedIndex <= 0 ||
                CmbBirthYear.SelectedIndex <= 0)
            {
                return null; // Tanggal lahir tidak diisi
            }

            try
            {
                int day = int.Parse(CmbBirthDay.SelectedItem.ToString());

                string monthName = CmbBirthMonth.SelectedItem.ToString();
                int month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.CurrentCulture).Month;

                int year = int.Parse(CmbBirthYear.SelectedItem.ToString());

                // Validasi tanggal
                DateTime birthDate = new DateTime(year, month, day);

                // Validasi tambahan: tidak boleh tanggal masa depan
                if (birthDate > DateTime.Now)
                {
                    MessageBox.Show("Birth date cannot be in the future.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                return birthDate;
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid birth date selected.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        private void SetEditMode(bool isEnabled)
        {
            FullNameTextBox.IsReadOnly = !isEnabled;

            GenderComboBox.IsEnabled = isEnabled;
            CmbBirthDay.IsEnabled = isEnabled;
            CmbBirthMonth.IsEnabled = isEnabled;
            CmbBirthYear.IsEnabled = isEnabled;

            // Ubah tampilan tombol Edit/Save
            if (isEnabled)
            {
                EditSaveButton.Content = "Save";
                EditSaveButton.Background = (Brush)new BrushConverter().ConvertFrom("#10B981"); // Hijau
                EditSaveButton.Foreground = Brushes.White;
            }
            else
            {
                EditSaveButton.Content = "Edit";
                // Mengambil resource warna biru dari XAML
                EditSaveButton.Background = Brushes.White;
                EditSaveButton.Foreground = (Brush)FindResource("PrimaryBlueBrush");
            }
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            // Konfirmasi penghapusan
            var result = MessageBox.Show(
                "Are you sure you want to delete your account? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_currentUser != null)
                    {
                        bool deleted = DeleteUserFromDatabase(_currentUser.UserID);

                        if (deleted)
                        {
                            MessageBox.Show("Account deleted successfully.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                            // Trigger event untuk logout atau navigasi
                            DeleteAccountRequested?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete account.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting account: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Hapus user dari database PostgreSQL
        /// </summary>
        private bool DeleteUserFromDatabase(int userID)
        {
            string sql = "DELETE FROM Users WHERE userID = @userID";

            using (var conn = new Npgsql.NpgsqlConnection(DBHelper.GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("userID", userID);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Npgsql.NpgsqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Delete DB Error: {ex.Message}");
                    return false;
                }
            }
        }
    }
}