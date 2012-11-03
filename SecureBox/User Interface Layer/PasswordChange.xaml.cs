using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SecureBox.User_Interface_Layer
{
    /// <summary>
    /// Interaction logic for PasswordChange.xaml
    /// </summary>
    public partial class PasswordChange : Window
    {
        private const string errorMatch = "The Confirm field must match New Password field!";
        private const string errorWrongPass = "Entered password is invalid!";
        private const string errorEmpty = "The New Password field can't be empty!";
        private const string endRow = "\n";
        private const int empty = 0;
        private SecureBox.BL.SecureBox secBox;
        private SecureBox.BL.DriveInfo drive;

        public PasswordChange(SecureBox.BL.SecureBox secBox, SecureBox.BL.DriveInfo drive)
        {
            InitializeComponent();
            this.secBox = secBox;
            this.drive = drive;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            bool error = false;
            string errorString = "";

            textBoxNewPass.Password = textBoxNewPass.Password.TrimEnd();
            textBoxNewPass.Password = textBoxNewPass.Password.TrimStart();
            textBoxCurrPass.Password = textBoxCurrPass.Password.TrimEnd();
            textBoxCurrPass.Password = textBoxCurrPass.Password.TrimStart();
            textBoxConf.Password = textBoxConf.Password.TrimEnd();
            textBoxConf.Password = textBoxConf.Password.TrimStart();

            if (textBoxNewPass.Password != textBoxConf.Password)
            {
                errorString += errorMatch + endRow;
                error = true;
            }

            if (textBoxNewPass.Password.Length == empty)
            {
                errorString += errorEmpty + endRow;
                error = true;
            }

            bool success = secBox.ChangePassword(drive, textBoxCurrPass.Password,
                textBoxNewPass.Password);

            if (!success)
            {
                errorString += errorWrongPass + endRow;
                error = true;
            }

            if (error)
            {
                System.Windows.Forms.MessageBox.Show(errorString);
                return;
            }

            this.Close();
        }
    }
}
