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
using SecureBox.BL;

namespace SecureBox.User_Interface_Layer
{
    /// <summary>
    /// Interaction logic for CheckPassword.xaml
    /// </summary>
    public partial class CheckPassword : Window
    {
        private SecureBox.BL.SecureBox secBox;
        private SecureBox.BL.DriveInfo drive;

        public CheckPassword(BL.SecureBox secBox, DriveInfo drive)
        {
            InitializeComponent();
            this.drive = drive;
            this.secBox = secBox;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            textBoxPass.Password = textBoxPass.Password.TrimEnd();
            textBoxPass.Password = textBoxPass.Password.TrimStart();

            if (secBox.CheckPassword(drive, textBoxPass.Password))
            {
                secBox.MountDrive(drive);
                this.Close();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Password is incorrect!");
            }
        }
    }
}
