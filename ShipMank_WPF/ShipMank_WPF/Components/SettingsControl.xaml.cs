using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private bool isEditing = false;
        public event EventHandler DeleteAccountRequested;

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
            SetEditMode(false);
            try
            {
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfiguration configuration = builder.Build();

                string clientId = configuration["GoogleAuth:ClientId"];
                string clientSecret = configuration["GoogleAuth:ClientSecret"];

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

                        _currentUser = User.GetUserByEmail(profile.Email);

                        if (_currentUser == null)
                        {
                            var newUser = new User();
                            bool registered = newUser.Register(
                                username: profile.Email.Split('@')[0],
                                passwordRaw: Guid.NewGuid().ToString(), 
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

                        LoadUserDataToUI();
                    }
                }
                else
                {
                    LoadDummyUserData();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat profil: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                LoadDummyUserData();
            }
        }

        private void LoadDummyUserData()
        {
            FullNameTextBox.Text = "User Local";
            EmailTextBox.Text = "user@local.com";
            CityTextBox.Text = "Unknown City";
            PhoneTextBox.Text = "08123456789";
            GenderComboBox.SelectedIndex = 0;
            CmbBirthDay.SelectedIndex = 0;
            CmbBirthMonth.SelectedIndex = 0;
            CmbBirthYear.SelectedIndex = 0;
        }

        private void LoadUserDataToUI()
        {
            if (_currentUser == null) return;

            FullNameTextBox.Text = _currentUser.Name ?? "";
            EmailTextBox.Text = _currentUser.Email ?? "";

            CityTextBox.Text = _currentUser.Alamat ?? "";
            PhoneTextBox.Text = _currentUser.NoTelp ?? "";

            if (!string.IsNullOrEmpty(_currentUser.Gender))
            {
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
                GenderComboBox.SelectedIndex = 0; 
            }

            if (_currentUser.TTL.HasValue)
            {
                DateTime birthDate = _currentUser.TTL.Value;
                CmbBirthDay.SelectedItem = birthDate.Day.ToString();
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(birthDate.Month);
                CmbBirthMonth.SelectedItem = monthName;
                CmbBirthYear.SelectedItem = birthDate.Year.ToString();
            }
            else
            {
                CmbBirthDay.SelectedIndex = 0;
                CmbBirthMonth.SelectedIndex = 0;
                CmbBirthYear.SelectedIndex = 0;
            }
        }

        private void PopulateDateComboBoxes()
        {
            CmbBirthDay.Items.Add("Hari");
            for (int i = 1; i <= 31; i++) CmbBirthDay.Items.Add(i.ToString());
            CmbBirthDay.SelectedIndex = 0;

            CmbBirthMonth.Items.Add("Bulan");
            string[] monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames;
            foreach (string month in monthNames)
            {
                if (!string.IsNullOrEmpty(month)) CmbBirthMonth.Items.Add(month);
            }
            CmbBirthMonth.SelectedIndex = 0;

            CmbBirthYear.Items.Add("Tahun");
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i >= currentYear - 100; i--) CmbBirthYear.Items.Add(i.ToString());
            CmbBirthYear.SelectedIndex = 0;
        }

        private void EditSaveButton_Click(object sender, RoutedEventArgs e)
        {
            isEditing = !isEditing;
            if (!isEditing)
            {
                if (SaveUserData())
                {
                    MessageBox.Show("Personal information updated successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    isEditing = true;
                }
            }

            SetEditMode(isEditing);
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

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

                string fullName = FullNameTextBox.Text.Trim();
                string address = CityTextBox.Text.Trim();
                string phoneNumber = PhoneTextBox.Text.Trim();

                if (string.IsNullOrEmpty(fullName))
                {
                    MessageBox.Show("Full name cannot be empty.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                string gender = null;
                if (GenderComboBox.SelectedItem is ComboBoxItem selectedGender)
                {
                    string genderValue = selectedGender.Content.ToString();
                    if (genderValue != "Gender") 
                    {
                        gender = genderValue;
                    }
                }

                DateTime? birthDate = ParseBirthDate();
                bool success = UpdateUserInDatabase(_currentUser.UserID, fullName, gender, birthDate, address, phoneNumber);

                if (!success)
                {
                    MessageBox.Show("Failed to update user data.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                _currentUser.Name = fullName;
                _currentUser.Gender = gender;
                _currentUser.TTL = birthDate;
                _currentUser.Alamat = address;
                _currentUser.NoTelp = phoneNumber;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool UpdateUserInDatabase(int userID, string name, string gender, DateTime? ttl, string alamat, string noTelp)
        {
            string sql = @"
                UPDATE Users 
                SET name = @name, 
                    gender = @gender, 
                    ttl = @ttl,
                    alamat = @alamat,
                    noTelp = @noTelp
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
                        cmd.Parameters.AddWithValue("alamat", (object)alamat ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("noTelp", (object)noTelp ?? DBNull.Value);

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

        private DateTime? ParseBirthDate()
        {
            if (CmbBirthDay.SelectedIndex <= 0 ||
                CmbBirthMonth.SelectedIndex <= 0 ||
                CmbBirthYear.SelectedIndex <= 0)
            {
                return null;
            }

            try
            {
                int day = int.Parse(CmbBirthDay.SelectedItem.ToString());

                string monthName = CmbBirthMonth.SelectedItem.ToString();
                int month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.CurrentCulture).Month;

                int year = int.Parse(CmbBirthYear.SelectedItem.ToString());

                DateTime birthDate = new DateTime(year, month, day);

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
            CityTextBox.IsReadOnly = !isEnabled;      
            PhoneTextBox.IsReadOnly = !isEnabled;     

            GenderComboBox.IsEnabled = isEnabled;
            CmbBirthDay.IsEnabled = isEnabled;
            CmbBirthMonth.IsEnabled = isEnabled;
            CmbBirthYear.IsEnabled = isEnabled;

            if (isEnabled)
            {
                EditSaveButton.Content = "Save";
                EditSaveButton.Background = (Brush)new BrushConverter().ConvertFrom("#10B981"); // Hijau
                EditSaveButton.Foreground = Brushes.White;
            }
            else
            {
                EditSaveButton.Content = "Edit";
                EditSaveButton.Background = Brushes.White;
                EditSaveButton.Foreground = (Brush)FindResource("PrimaryBlueBrush");
            }
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
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