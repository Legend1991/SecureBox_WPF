using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace SecureBox.BL
{
    class Encryptor
    {
        private IBufferedCipher cipher;
        private KeyParameter key;
        private const string algorithm = "AES/CFB/NOPADDING";
        private const int keySize = 256;

        public Encryptor(byte[] key)
        {
            this.key = new KeyParameter(key);
        }

        public static  byte[] GenerateKey()
        {
            SecureRandom sr = new SecureRandom();

            KeyGenerationParameters kgp = new KeyGenerationParameters(sr, keySize);

            CipherKeyGenerator kg = new CipherKeyGenerator();
            kg.Init(kgp);

            return kg.GenerateKey();
        }

        public byte[] EnDecrypt(bool forEncrypt, byte[] inBytes)
        {
            cipher = CipherUtilities.GetCipher(algorithm);
            cipher.Init(forEncrypt, key);
            int outBytesSize = cipher.GetOutputSize(inBytes.Length);
            byte[] outBytes = new byte[outBytesSize];
            int outLentgh;

            outLentgh = cipher.ProcessBytes(inBytes, 0, inBytes.Length, outBytes, 0);
            outLentgh += cipher.DoFinal(outBytes, outLentgh);

            if (outLentgh != outBytesSize)
            {
                return null;
            }

            return outBytes;
        }
    }
}
