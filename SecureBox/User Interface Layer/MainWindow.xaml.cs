using System.Windows;
using System.Windows.Controls;
using SecureBox.User_Interface_Layer;
using SecureBox.BL;

namespace SecureBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BL.SecureBox secBox;

        public MainWindow()
        {
            InitializeComponent();
            secBox = new BL.SecureBox();
            UpdateList();
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private DriveInfo GetSelectedDrive()
        {
            if (drivesList.SelectedItem != null)
            {
                return (DriveInfo)drivesList.SelectedItem;
            }

            return null;
        }

        private void UpdateToolbar()
        {
            if (drivesList.SelectedItem != null)
            {
                DriveInfo drive = GetSelectedDrive();
                if (drive.Mounted)
                {
                    mountDrive.IsEnabled = false;
                    unmountDrive.IsEnabled = true;
                }
                else
                {
                    mountDrive.IsEnabled = true;
                    unmountDrive.IsEnabled = false;
                }
                removeDrive.IsEnabled = true;
                changePassword.IsEnabled = true;
            }
            else
            {
                mountDrive.IsEnabled = false;
                unmountDrive.IsEnabled = false;
                removeDrive.IsEnabled = false;
                changePassword.IsEnabled = false;
            }
        }

        private void UpdateList()
        {
            drivesList.ItemsSource = secBox.DrivesList;
            UpdateToolbar();
        }

        private void removeDrive_Click(object sender, RoutedEventArgs e)
        {
            DriveInfo drive = GetSelectedDrive();
            secBox.RemoveDrive(drive);
            UpdateList();
        }

        private void mountDrive_Click(object sender, RoutedEventArgs e)
        {
            DriveInfo drive = GetSelectedDrive();
            secBox.MountDrive(drive);
            UpdateList();
        }

        private void unmountDrive_Click(object sender, RoutedEventArgs e)
        {
            DriveInfo drive = GetSelectedDrive();
            secBox.UnmountDrive(drive);
            UpdateList();
        }

        private void changePassword_Click(object sender, RoutedEventArgs e)
        {
            DriveInfo drive = GetSelectedDrive();
            PasswordChange passChange = new PasswordChange(secBox, drive);
            passChange.ShowDialog();
            UpdateList();
        }

        private void addDrive_Click(object sender, RoutedEventArgs e)
        {
            AddDrive addDrv = new AddDrive(secBox);
            addDrv.ShowDialog();
            UpdateList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            secBox.UnmountAllDrives();
        }

        private void drivesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateToolbar();
        }
    }
}
