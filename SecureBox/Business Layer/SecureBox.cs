using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Dokan;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace SecureBox.BL
{
    public class SecureBox
    {
        private XmlAppSettings appSettings;
        private Encryptor encryptor;
        private const string formatConvert = "x2";
        private const string internalKey = "9af6d18eed58756ea968c4e555c59d82";
        private const string appDirectory = ".\\";
        private const string driveLetters = "CDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int firstLetter = 0;
        private Dictionary<char, Thread> threads;

        public SecureBox()
        {
            appSettings = new XmlAppSettings(appDirectory);
            encryptor = new Encryptor(Encoding.Default.GetBytes(internalKey));
            threads = new Dictionary<char, Thread>();
            Initialize();
        }

        public bool AddDrive(DriveInfo drive, string password)
        {
            foreach (DriveInfo dInfo in DrivesList)
            {
                if (dInfo.Root == drive.Root ||
                    dInfo.Letter == drive.Letter)
                {
                    return false;
                }
            }

            Directory.CreateDirectory(drive.Root);

            CreateDriveSettings(drive, password);

            MountDrive(drive);
            appSettings.AddDrive(drive);

            return true;
        }

        public bool RemoveDrive(DriveInfo drive)
        {
            if (ValidPassword(drive))
            {
                UnmountDrive(drive);
                appSettings.RemoveDrive(drive);
                return true;
            }

            return false;
        }

        public bool MountDrive(DriveInfo drive)
        {
            
            if (ValidPassword(drive))
            {
                CryptoMirror crptMirr = new CryptoMirror(drive.Root, drive.Letter, drive.Label, internalKey);
                Thread newThread = new Thread(CreateDrive);
                newThread.Start(crptMirr);
                threads.Add(drive.Letter, newThread);

                appSettings.UnMountDrive(drive, true);
                return true;
            }

            return false;
        }

        public bool UnmountDrive(DriveInfo drive)
        {
            try
            {
                if (drive.Mounted)
                {
                    DokanNet.DokanUnmount(drive.Letter);
                    threads[drive.Letter].Abort();
                    threads.Remove(drive.Letter);

                    appSettings.UnMountDrive(drive, false);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public void UnmountAllDrives()
        {
            foreach (DriveInfo dIfno in DrivesList)
            {
                UnmountDrive(dIfno);
            }
        }

        public bool ChangePassword(DriveInfo drive, string oldPassword, string newPassword)
        {
            if (ValidPassword(drive))
            {
                string password = GetDrivePassword(drive);

                if (password == oldPassword)
                {
                    XmlDriveSettings dSettings = new XmlDriveSettings(drive.Root);

                    byte[] encPassword = encryptor.EnDecrypt(true, Encoding.Default.GetBytes(newPassword));
                    password = Encoding.Default.GetString(encPassword);
                    dSettings.WritePassword(password);

                    string md5Password = GetMd5Hash(newPassword);
                    dSettings.WriteMd5Password(md5Password);

                    return true;
                }
            }

            return false;
        }

        public bool CheckPassword(DriveInfo drive, string password)
        {
            return password == GetDrivePassword(drive);
        }

        public char[] GetFreeDriveLetters()
        {
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            List<char> letters = new List<char>(driveLetters.ToCharArray());

            foreach (System.IO.DriveInfo dInfo in drives)
            {
                letters.Remove(dInfo.Name[firstLetter]);
            }

            foreach (DriveInfo dInfo in DrivesList)
            {
                letters.Remove(dInfo.Letter);
            }

            return letters.ToArray();
        }

        public string GetDropboxPath()
        {
            string jsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Dropbox\\info.json");

            JObject o = JObject.Parse(File.ReadAllText(jsPath));

            return o["personal"]["path"].ToString();
        }

        public bool IsDropboxAvailable()
        {
            string jsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Dropbox\\info.json");

            return File.Exists(jsPath);
        }

        public string GetGoogleDrivePath()
        {
            string path;
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google\\Drive\\sync_config.db");

            string csGdrive = @"Data Source=" + dbPath + ";Version=3;New=False;Compress=True;";

            using (var con = new SQLiteConnection(csGdrive))
            {
                con.Open();

                using (var sqLitecmd = new SQLiteCommand(con))
                {
                    sqLitecmd.CommandText = "select * from data where entry_key='local_sync_root_path'";

                    using (var reader = sqLitecmd.ExecuteReader())
                    {
                        reader.Read();
                        path = reader["data_value"].ToString().Substring(4);
                    }
                }
            }

            return path;
        }

        public bool IsGoogleDriveAvailable()
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google\\Drive\\sync_config.db");

            return File.Exists(dbPath);
        }

        public string GetOneDrivePath()
        {
            string path = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\SkyDrive\\",
            "UserFolder",
            null);

            return path;
        }

        public bool IsOneDriveAvailable()
        {
            string exePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft\\SkyDrive\\SkyDrive.exe");

            return File.Exists(exePath);
        }

        private void Initialize()
        {
            foreach (DriveInfo dInfo in appSettings.GetDrives())
            {
                if (dInfo.Mounted)
                {
                    MountDrive(dInfo);
                }
            }
        }

        private string GetMd5Hash(string input)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString(formatConvert));
            }

            return sBuilder.ToString();
        }

        private bool VerifyMd5Hash(string input, string hash)
        {
            string hashOfInput = GetMd5Hash(input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ValidPassword(DriveInfo drive)
        {
            string password = GetDrivePassword(drive);
            XmlDriveSettings dSettings = new XmlDriveSettings(drive.Root);
            string md5Password = dSettings.ReadMd5Password();

            if (VerifyMd5Hash(password, md5Password))
            {
                return true;
            }

            return false;
        }

        private string GetDrivePassword(DriveInfo drive)
        {
            XmlDriveSettings dSettings = new XmlDriveSettings(drive.Root);
            string password = dSettings.ReadPassword();
            byte[] encPassword = encryptor.EnDecrypt(false, Encoding.Default.GetBytes(password));
            password = Encoding.Default.GetString(encPassword);

            return password;
        }

        private void CreateDrive(object opt)
        {
            try
            {
                DokanNet.DokanMain(((CryptoMirror)opt).Options, (CryptoMirror)opt);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        private void CreateDriveSettings(DriveInfo drive, string password)
        {
            XmlDriveSettings driveSettings = new XmlDriveSettings(drive.Root);
            driveSettings.WriteMd5Password(GetMd5Hash(password));

            byte[] encPassword = encryptor.EnDecrypt(true, Encoding.Default.GetBytes(password));
            password = Encoding.Default.GetString(encPassword);
            driveSettings.WritePassword(password);
        }

        public List<DriveInfo> DrivesList
        {
            get { return appSettings.GetDrives(); }
            private set {}
        }
    }
}