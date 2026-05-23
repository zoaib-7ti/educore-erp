using System;
using System.Windows;
using System.Windows.Input;
using SchoolERP.Services;

namespace SchoolERP
{
    public partial class LoginWindow : Window
    {
        private readonly AuthenticationService authenticationService = new AuthenticationService();

        public LoginWindow()
        {
            InitializeComponent();
            UsernameTextBox.Focus();
            PasswordBox.KeyDown += PasswordBox_KeyDown;
            UsernameTextBox.KeyDown += UsernameTextBox_KeyDown;
        }

        private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AttemptLogin();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private void AttemptLogin()
        {
            StatusTextBlock.Text = string.Empty;

            var username = UsernameTextBox.Text?.Trim();
            var password = PasswordBox.Password;

            try
            {
                var result = authenticationService.Login(username, password);

                if (!result.Success)
                {
                    StatusTextBlock.Text = result.ErrorMessage ?? "Login failed.";
                    return;
                }

                var dashboard = new MainWindow(result);
                dashboard.Show();
                Close();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Unable to sign in: " + ex.Message;
            }
        }
    }
}