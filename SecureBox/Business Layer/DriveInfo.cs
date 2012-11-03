using System;
using System.Collections.Generic;
using System.Text;

namespace SecureBox.BL
{
    public class DriveInfo
    {
        public DriveInfo(char letter, string label,
            string root, bool mounted)
        {
            Letter = char.ToUpper(letter);
            Label = label;
            Root = root;
            Mounted = mounted;
        }

        public char Letter
        {
            get;
            private set;
        }

        public string Label
        {
            get;
            private set;
        }

        public string Root
        {
            get;
            private set;
        }

        public bool Mounted
        {
            get;
            private set;
        }
    }
}
