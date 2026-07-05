using System.Windows;
using System.Windows.Input;
using Crime_analysis.Services;

namespace Crime_analysis.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // ── Login button clicked ──────────────
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            // Validation
            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            // Try login
            bool success = AuthService.Login(username, password);

            if (success)
            {
                var dashboard = new MainWindow();
                dashboard.Show();
                this.Close();
            }
            else
            {
                ShowError("Invalid username or password.");
            }
        }

        // ── Exit clicked ──────────────────────
        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ── Show error ────────────────────────
        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}