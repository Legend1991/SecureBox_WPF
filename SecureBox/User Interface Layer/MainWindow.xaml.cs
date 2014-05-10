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
        private SecureBox.BL.SecureBox secBox;
        private const int first = 0;
        private const int empty = 0;

        public MainWindow()
        {
            InitializeComponent();
            secBox = new SecureBox.BL.SecureBox();
            UpdateList();
            if (drivesList.SelectedItems.Count == empty)
            {
                mountDrive.IsEnabled = false;
                unmountDrive.IsEnabled = false;
                removeDrive.IsEnabled = false;
                changePassword.IsEnabled = false;
            }
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
                char letter = ((DriveInfo)drivesList.SelectedItem).Letter;
                string label = ((DriveInfo)drivesList.SelectedItem).Label;
                bool mounted = ((DriveInfo)drivesList.SelectedItem).Mounted;
                string root = ((DriveInfo)drivesList.SelectedItem).Root;
                return new DriveInfo(letter, label, root, mounted);
            }

            return null;
        }

        private void SwitchUnMountEnable()
        {
            if (drivesList.SelectedItems.Count != empty)
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
        }

        private void UpdateList()
        {
            drivesList.ItemsSource = secBox.DrivesList;
            SwitchUnMountEnable();
        }

        private void removeDrive_Click(object sender, RoutedEventArgs e)
        {
            DriveInfo drive = GetSelectedDrive();
            secBox.RemoveDrive(drive);
            UpdateList();
            if (drivesList.SelectedItems.Count == empty)
            {
                mountDrive.IsEnabled = false;
                unmountDrive.IsEnabled = false;
                removeDrive.IsEnabled = false;
                changePassword.IsEnabled = false;
            }
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
            if (drivesList.SelectedItems.Count == empty)
            {
                mountDrive.IsEnabled = true;
                unmountDrive.IsEnabled = true;
                removeDrive.IsEnabled = true;
                changePassword.IsEnabled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            secBox.UnmountAllDrives();
        }

        private void drivesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SwitchUnMountEnable();
        }
    }
}
