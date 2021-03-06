﻿using System;
using System.Data.SQLite;
using System.Text;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using SecureBox.BL;

namespace SecureBox.User_Interface_Layer
{
    /// <summary>
    /// Interaction logic for AddDrive.xaml
    /// </summary>
    public partial class AddDrive : Window
    {
        private const string errorMatch = "The Confirm field must match Password field!";
        private const string errorEmpty = "The Password field can't be empty!";
        private const string errorPath = "Please enter a valid location!";
        private const string defaultLabel = "SecureBox";
        private const string endRow = "\n";
        private string customPath = "";
        private string lastRB;
        BL.SecureBox secBox;
        private const int first = 0;
        private const int empty = 0;

        public AddDrive(BL.SecureBox secBox)
        {
            InitializeComponent();
            this.secBox = secBox;
            comboBoxLetters.ItemsSource = secBox.GetFreeDriveLetters();
            comboBoxLetters.SelectedIndex = first;
            CheckCloudStorages();
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog =
                new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void textBoxLabel_LostFocus(object sender, RoutedEventArgs e)
        {
            string driveLabel = textBoxLabel.Text.TrimStart();
            driveLabel = driveLabel.TrimEnd();
            if (driveLabel.Length == empty)
            {
                textBoxLabel.Text = defaultLabel;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool CheckFields()
        {
            bool result = true;
            string errorString = "";

            if (!Directory.Exists(textBoxFolder.Text) || textBoxFolder.Text.EndsWith("."))
            {
                errorString += errorPath + endRow;
                result = false;
            }

            foreach (BL.DriveInfo di in secBox.DrivesList)
            {
                if (char.ToUpper(textBoxFolder.Text[0]) == di.Letter)
                {
                    errorString += errorPath + endRow;
                    result = false;
                }
            }

            if (textBoxPass.Password != textBoxConf.Password)
            {
                errorString += errorMatch + endRow;
                result = false;
            }

            if (textBoxPass.Password.Length == empty)
            {
                errorString += errorEmpty;
                result = false;
            }

            if (!result)
            {
                System.Windows.Forms.MessageBox.Show(errorString);
            }

            return result;
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            textBoxPass.Password = textBoxPass.Password.TrimEnd();
            textBoxPass.Password = textBoxPass.Password.TrimStart();
            textBoxConf.Password = textBoxConf.Password.TrimEnd();
            textBoxConf.Password = textBoxConf.Password.TrimStart();
            textBoxLabel.Text = textBoxLabel.Text.TrimEnd();
            textBoxLabel.Text = textBoxLabel.Text.TrimStart();

            if (CheckFields() == false)
            {
                return;
            }

            char letter = (char)comboBoxLetters.SelectedItem;
            string label = textBoxLabel.Text;
            string root = textBoxFolder.Text;
            string password = textBoxPass.Password;

            BL.DriveInfo drive = new BL.DriveInfo(letter, label, root, true);

            secBox.AddDrive(drive, password);

            this.Close();
        }

        private void CheckCloudStorages()
        {
            if (!secBox.IsDropboxAvailable())
            {
                rbDropbox.IsEnabled = false;
            }

            if (!secBox.IsGoogleDriveAvailable())
            {
                rbGoogleDrive.IsEnabled = false;
            }

            if (!secBox.IsOneDriveAvailable())
            {
                rbOneDrive.IsEnabled = false;
            }
        }

        private void rbCustom_Checked(object sender, RoutedEventArgs e)
        {
            lastRB = rbCustom.Name;

            if (textBoxFolder != null)
            {
                textBoxFolder.Text = customPath;
            }
        }

        private void rbDropbox_Checked(object sender, RoutedEventArgs e)
        {
            if (lastRB == rbCustom.Name)
            {
                customPath = textBoxFolder.Text;
            }
            lastRB = rbDropbox.Name;
            textBoxFolder.Text = secBox.GetDropboxPath();
        }

        private void rbGoogleDrive_Checked(object sender, RoutedEventArgs e)
        {
            if (lastRB == rbCustom.Name)
            {
                customPath = textBoxFolder.Text;
            }
            lastRB = rbGoogleDrive.Name;
            textBoxFolder.Text = secBox.GetGoogleDrivePath();
        }

        private void rbOneDrive_Checked(object sender, RoutedEventArgs e)
        {
            if (lastRB == rbCustom.Name)
            {
                customPath = textBoxFolder.Text;
            }
            lastRB = rbOneDrive.Name;
            textBoxFolder.Text = secBox.GetOneDrivePath();
        }
    }
}
