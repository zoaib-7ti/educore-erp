using System.Windows;
using SchoolERP.Services;

namespace SchoolERP
{
    public partial class MainWindow : Window
    {
        private readonly AuthResult authResult;

        public MainWindow(AuthResult authResult)
        {
            InitializeComponent();
            this.authResult = authResult;

            if (this.authResult != null)
            {
                var fullName = string.IsNullOrWhiteSpace(this.authResult.FullName) ? this.authResult.Username : this.authResult.FullName;
                WelcomeTextBlock.Text = string.IsNullOrWhiteSpace(fullName)
                    ? "Welcome to School ERP"
                    : "Welcome, " + fullName + (this.authResult.Roles.Count > 0 ? " (" + string.Join(", ", this.authResult.Roles) + ")" : string.Empty);
            }
        }
    }
}
