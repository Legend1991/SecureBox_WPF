using System;
using System.IO;
using System.Collections;
using System.Text;
using Dokan;


namespace SecureBox.BL
{
    class CryptoMirror : DokanOperations
    {
        private string root;
        private string mountPoint;
        private string label;
        private Encryptor encryptor;
        private const string settingsFile = ".settings.xml";
        private const string mountPointEnd = ":\\";
        private const string internalKey = "9af6d18eed58756ea968c4e555c59d82";
        private const int threadCount = 0;
        private const int blockSize = 65536;

        public CryptoMirror(string root, char letter, string label)
        {
            this.root = root;
            this.mountPoint = letter + mountPointEnd;
            this.label = label;
            encryptor = new Encryptor(Encoding.Default.GetBytes(internalKey));
            Options = new DokanOptions();
            Options.DebugMode = false;
            Options.MountPoint = mountPoint;
            Options.ThreadCount = threadCount;
            Options.VolumeLabel = label;
        }

        private string GetPath(string filename)
        {
            return root + filename;
        }

        public int CreateFile(String filename, FileAccess access, FileShare share,
            FileMode mode, FileOptions options, DokanFileInfo info)
        {
            string path = GetPath(filename);

            try
            {
                if (Directory.Exists(path))
                {
                    info.IsDirectory = true;
                }
                else
                {
                    FileStream fs = new FileStream(path, mode, access, share, 8, options);
                    fs.Close();
                }

                //Console.WriteLine("Create file: {0}\t access: {1}", filename, access);
                return DokanNet.DOKAN_SUCCESS;
            }
            catch (Exception e)
            {
                return -DokanNet.DOKAN_ERROR;
            }
        }

        public int OpenDirectory(String filename, DokanFileInfo info)
        {
            if (Directory.Exists(GetPath(filename)))
            {
                return DokanNet.DOKAN_SUCCESS;
            }
            
            return -DokanNet.ERROR_PATH_NOT_FOUND;    
        }

        public int CreateDirectory(String filename, DokanFileInfo info)
        {
            if (!Directory.Exists(GetPath(filename)))
            {
                try
                {
                    Directory.CreateDirectory(GetPath(filename));
                }
                catch (Exception)
                {
                    return -DokanNet.DOKAN_ERROR;
                }

                return DokanNet.DOKAN_SUCCESS;
            }

            return -DokanNet.ERROR_ALREADY_EXISTS;
        }

        public int Cleanup(String filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int CloseFile(String filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int ReadFile(String filename, Byte[] buffer, ref uint readBytes,
            long offset, DokanFileInfo info)
        {
            string path = GetPath(filename);

            try
            {
                long offsetDif = offset % blockSize;
                long decOffset = offset - offsetDif;
                long decEndDiff = (offset + buffer.Length) % blockSize;

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                fs.Seek(decOffset, SeekOrigin.Begin);

                int i = 0;

                while ((fs.Position < offset + buffer.Length) && fs.Position < fs.Length)
                {
                    Byte[] crpBuff = new Byte[blockSize];
                    int lentgh = fs.Read(crpBuff, 0, crpBuff.Length);
                    int offsetStart = i > 0 ? 0 : (int)(offsetDif);
                    int offsetEnd = fs.Position > offset + buffer.Length ? lentgh - (int)decEndDiff : 0;
                    Array.Copy(encryptor.EnDecrypt(false, crpBuff), offsetStart, buffer, i, lentgh - offsetStart - offsetEnd);

                    if (i == 0)
                    {
                        i += lentgh - (int)offsetDif;
                    }
                    else
                    {
                        i += lentgh;
                    }
                }

                fs.Close();
                readBytes = (uint)buffer.Length;

                //Console.WriteLine("Read file: {0}\tBlock size: {1}\toffset: {2}", filename, buffer.Length, offset);
                return DokanNet.DOKAN_SUCCESS;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ReadFile exception \"{0}\": {1}", filename, e.Message);
                return DokanNet.DOKAN_ERROR;
            }
        }

        public int WriteFile(String filename, Byte[] buffer,
            ref uint writtenBytes, long offset, DokanFileInfo info)
        {
            string path = GetPath(filename);
            
            try
            {
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

                if (info.WriteToEndOfFile)
                {
                    fs.Seek(0, SeekOrigin.End);
                }
                else
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                }

                Byte[] crpBuff = encryptor.EnDecrypt(true, buffer);

                fs.Write(crpBuff, 0, crpBuff.Length);
                writtenBytes = (uint)crpBuff.Length;
                fs.Close();

                //Console.WriteLine("Write file: {0}\tBlock size: {1}\toffset: {2}", filename, buffer.Length, offset);
                return DokanNet.DOKAN_SUCCESS;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Write exception \"{0}\": {0}", filename, e.Message);
                return DokanNet.DOKAN_ERROR;
            }
        }

        public int FlushFileBuffers(String filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int GetFileInformation(String filename, FileInformation fileinfo, DokanFileInfo info)
        {
            string path = GetPath(filename);
            if (File.Exists(path))
            {
                FileInfo f = new FileInfo(path);

                fileinfo.Attributes = f.Attributes;
                fileinfo.CreationTime = f.CreationTime;
                fileinfo.LastAccessTime = f.LastAccessTime;
                fileinfo.LastWriteTime = f.LastWriteTime;
                fileinfo.Length = f.Length;
                return DokanNet.DOKAN_SUCCESS;
            }
            else if (Directory.Exists(path))
            {
                DirectoryInfo f = new DirectoryInfo(path);

                fileinfo.Attributes = f.Attributes;
                fileinfo.CreationTime = f.CreationTime;
                fileinfo.LastAccessTime = f.LastAccessTime;
                fileinfo.LastWriteTime = f.LastWriteTime;
                fileinfo.Length = 0;// f.Length;
                return DokanNet.DOKAN_SUCCESS;
            }
            
            return DokanNet.DOKAN_ERROR;
        }

        public int FindFiles(String filename, ArrayList files, DokanFileInfo info)
        {
            string path = GetPath(filename);
            if (Directory.Exists(path))
            {
                DirectoryInfo d = new DirectoryInfo(path);
                FileSystemInfo[] entries = d.GetFileSystemInfos();
                foreach (FileSystemInfo f in entries)
                {
                    if (f.Name == settingsFile)
                    {
                        continue;
                    }
                    FileInformation fi = new FileInformation();
                    fi.Attributes = f.Attributes;
                    fi.CreationTime = f.CreationTime;
                    fi.LastAccessTime = f.LastAccessTime;
                    fi.LastWriteTime = f.LastWriteTime;
                    fi.Length = (f is DirectoryInfo) ? 0 : ((FileInfo)f).Length;
                    fi.FileName = f.Name;
                    files.Add(fi);
                }
                return DokanNet.DOKAN_SUCCESS;
            }
            
            return DokanNet.DOKAN_ERROR;
        }

        public int SetFileAttributes(String filename, FileAttributes attr, DokanFileInfo info)
        {
            string path = GetPath(filename);
            try
            {
                if (File.Exists(path))
                {
                    File.SetAttributes(path, attr);
                    return DokanNet.DOKAN_SUCCESS;
                }
                else if (Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    di.Attributes = attr;
                    return DokanNet.DOKAN_SUCCESS;
                }
            }
            catch (Exception e)
            {
                return -DokanNet.DOKAN_ERROR;
            }

            return DokanNet.DOKAN_SUCCESS;
        }

        public int SetFileTime(String filename, DateTime ctime,
                DateTime atime, DateTime mtime, DokanFileInfo info)
        {
            string path = GetPath(filename);

            try
            {
                if (File.Exists(path))
                {

                    if (ctime == DateTime.MinValue)
                        ctime = File.GetCreationTime(path);
                    if (atime == DateTime.MinValue)
                        atime = File.GetLastAccessTime(path);
                    if (mtime == DateTime.MinValue)
                        mtime = File.GetLastWriteTime(path);

                    File.SetCreationTime(path, ctime);
                    File.SetLastAccessTime(path, atime);
                    File.SetLastWriteTime(path, mtime);

                    return DokanNet.DOKAN_SUCCESS;
                }
                else if (Directory.Exists(path))
                {

                    if (ctime == DateTime.MinValue)
                        ctime = Directory.GetCreationTime(path);
                    if (atime == DateTime.MinValue)
                        atime = Directory.GetLastAccessTime(path);
                    if (mtime == DateTime.MinValue)
                        mtime = Directory.GetLastWriteTime(path);

                    Directory.SetCreationTime(path, ctime);
                    Directory.SetLastAccessTime(path, atime);
                    Directory.SetLastWriteTime(path, mtime);

                    return DokanNet.DOKAN_SUCCESS;
                }

                return DokanNet.DOKAN_ERROR;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("FileTime exception: {0}", e.Message);
                return -DokanNet.DOKAN_ERROR;
            }
        }

        public int DeleteFile(String filename, DokanFileInfo info)
        {
            string path = GetPath(filename);

            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return DokanNet.DOKAN_ERROR;
                }

                return DokanNet.DOKAN_SUCCESS;
            }

            return -DokanNet.ERROR_FILE_NOT_FOUND;
        }

        public int DeleteDirectory(String filename, DokanFileInfo info)
        {
            string path = GetPath(filename);

            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return DokanNet.DOKAN_ERROR;
                }

                return DokanNet.DOKAN_SUCCESS;
            }

            return -DokanNet.ERROR_PATH_NOT_FOUND; ;
        }

        public int MoveFile(String filename, String newname, bool replace, DokanFileInfo info)
        {
            string sPath = GetPath(filename);
            string dPath = GetPath(newname);

            if (!replace && File.Exists(dPath))
            {
                return -DokanNet.ERROR_ALREADY_EXISTS;
            } else if (!replace && Directory.Exists(dPath))
            {
                return -DokanNet.ERROR_PATH_NOT_FOUND;
            }
            
            try
            {
                if (File.Exists(sPath))
                {
                    if (replace)
                    {
                        File.Replace(sPath, dPath, null);
                    }
                    else
                    {
                        File.Move(sPath, dPath);
                    }
                    return DokanNet.DOKAN_SUCCESS;
                }
                else if (Directory.Exists(sPath))
                {
                    DirectoryInfo k = new DirectoryInfo(sPath);
                    k.MoveTo(dPath);
                    return DokanNet.DOKAN_SUCCESS;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Move exception: {0}", e.Message);
                return -DokanNet.DOKAN_ERROR;
            }
            
            return DokanNet.DOKAN_ERROR;
        }

        public int SetEndOfFile(String filename, long length, DokanFileInfo info)
        {
            string path = GetPath(filename);

            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                fs.SetLength(length);
                fs.Close();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("SetEndOfFile exception: {0}", e.Message);
                return -DokanNet.DOKAN_ERROR;
            }

            return DokanNet.DOKAN_SUCCESS;
        }

        public int SetAllocationSize(String filename, long length, DokanFileInfo info)
        {
            string path = GetPath(filename);

            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                if (fs.Length < length)
                {
                    fs.SetLength(length);
                }
                fs.Close();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("SetAllocationSize exception: {0}", e.Message);
                return -DokanNet.DOKAN_ERROR;
            }

            return DokanNet.DOKAN_SUCCESS;
        }

        public int LockFile(String filename, long offset, long length, DokanFileInfo info)
        {
            string path = GetPath(filename);

            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                fs.Lock(offset, length);
                fs.Close();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("SetEndOfFile exception: {0}", e.Message);
                return -DokanNet.DOKAN_ERROR;
            }

            return DokanNet.DOKAN_SUCCESS;
        }

        public int UnlockFile(String filename, long offset, long length, DokanFileInfo info)
        {
            string path = GetPath(filename);

            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                fs.Unlock(offset, length);
                fs.Close();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("SetEndOfFile exception: {0}", e.Message);
                return -DokanNet.DOKAN_ERROR;
            }

            return DokanNet.DOKAN_SUCCESS;
        }

        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes,
            ref ulong totalFreeBytes, DokanFileInfo info)
        {
            Console.WriteLine("GetDiskFreeSpace");
            freeBytesAvailable = 512 * 1024 * 1024;
            totalBytes = 1024 * 1024 * 1024;
            totalFreeBytes = 512 * 1024 * 1024;
            return DokanNet.DOKAN_SUCCESS;
        }

        public int Unmount(DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public DokanOptions Options
        {
            get;
            private set;
        }
    }
}