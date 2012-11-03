﻿using System;
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
using System.IO;

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
        SecureBox.BL.SecureBox secBox;
        private const int first = 0;
        private const int empty = 0;

        public AddDrive(SecureBox.BL.SecureBox secBox)
        {
            InitializeComponent();
            this.secBox = secBox;
            comboBoxLetters.ItemsSource = secBox.GetFreeDriveLetters();
            comboBoxLetters.SelectedIndex = first;
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

            if (!Directory.Exists(textBoxFolder.Text))
            {
                errorString += errorPath + endRow;
                result = false;
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

            SecureBox.BL.DriveInfo drive = new SecureBox.BL.DriveInfo(letter, label, root, true);

            secBox.AddDrive(drive, password);

            this.Close();
        }
    }
}