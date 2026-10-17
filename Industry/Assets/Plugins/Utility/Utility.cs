using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Utility
{
    public class Directory
    {
        public static long Size(string path)
        {
            long Size = 0;
            DirectoryInfo DirectoryInfo = new DirectoryInfo(path);
            FileInfo[] FileInfos = DirectoryInfo.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo FileInfo in FileInfos) { Size += FileInfo.Length; }
            return Size;
        }

        public static void Move(string Source, string Destination)
        {
            foreach (string String in System.IO.Directory.GetDirectories(Source, "*", SearchOption.AllDirectories))
            {
                string Path = String.Replace(Source, Destination);
                System.IO.Directory.CreateDirectory(Path);
            }

            foreach (string String in System.IO.Directory.GetFiles(Source, "*.*", SearchOption.AllDirectories))
            {
                string Path = String.Replace(Source, Destination);
                System.IO.File.Copy(String, Path, true);
            }

            System.IO.Directory.Delete(Source, true);
        }
    }

        public class File
        {
            public static bool IsLocked(string path)
            {
                try
                {
                    FileInfo FileInfo = new FileInfo(path);
                    using (FileStream stream = FileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None)) { stream.Close(); }
                }
                catch (IOException) { return true; }
                return false;
            }

            public class Async
            {
                private const int BUFFER_SIZE = 0x4096;

                public static FileStream OpenRead(string path)
                {
                    return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true);
                }

                public static FileStream OpenWrite(string path)
                {
                    return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, BUFFER_SIZE, true);
                }

                public static async Task<byte[]> ReadAllBytes(string path)
                {
                    using (var fs = OpenRead(path))
                    {
                        var buff = new byte[fs.Length];
                        await fs.ReadAsync(buff, 0, (int)fs.Length);
                        return buff;
                    }
                }

                public static async Task WriteAllBytes(string path, byte[] bytes)
                {
                    if (path == null) throw new ArgumentException("path");
                    if (bytes == null) throw new ArgumentException("bytes");

                    using (var fs = OpenWrite(path))
                    {
                        await fs.WriteAsync(bytes, 0, bytes.Length);
                    }
                }

                public static async Task<string> ReadAllText(string path, Encoding encoding = null)
                {
                    encoding = encoding ?? Encoding.UTF8;
                    using (var reader = new StreamReader(path, encoding))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }

                public static async Task WriteAllText(string path, string contents, Encoding encoding = null)
                {
                    encoding = encoding ?? Encoding.UTF8;
                    await WriteAllBytes(path, encoding.GetBytes(contents));
                }

                public static async Task<string[]> ReadAllLines(string path, Encoding encoding = null)
                {
                    encoding = encoding ?? Encoding.UTF8;
                    var lines = new List<string>();
                    using (var reader = new StreamReader(path, encoding))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                            lines.Add(line);
                    }
                    return lines.ToArray();
                }

                public static async Task Copy(string sourceFileName, string destFileName)
                {
                    using (var sourceStream = System.IO.File.Open(sourceFileName, FileMode.Open))
                    {
                        using (var destinationStream = System.IO.File.Create(destFileName))
                        {
                            await sourceStream.CopyToAsync(destinationStream);
                        }
                    }
                }

                public static async Task Move(string sourceFileName, string destFileName)
                {
                    using (var sourceStream = System.IO.File.Open(sourceFileName, FileMode.Open))
                    {
                        using (var destinationStream = System.IO.File.Create(destFileName))
                        {
                            await sourceStream.CopyToAsync(destinationStream);
                        }
                    }
                    System.IO.File.Delete(sourceFileName);
                }
            }

            public enum FileTypes
            {
                Unknown,
                Jpeg,
                Bmp,
                Gif,
                Png,
                Pdf,
                Zip,
                WebM,
                Mp4
            }

            private static readonly Dictionary<FileTypes, byte[]> FileHeaders = new Dictionary<FileTypes, byte[]>()
        {
            { FileTypes.Jpeg, new byte[]{ 0xFF, 0xD8 }},
            { FileTypes.Bmp, new byte[]{ 0x42, 0x4D }},
            { FileTypes.Gif, new byte[]{ 0x47, 0x49, 0x46 }},
            { FileTypes.Png, new byte[]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }},
            { FileTypes.Pdf, new byte[]{ 0x25, 0x50, 0x44, 0x46 }},
            { FileTypes.Zip, new byte[]{ 0x50, 0x4B, 0x03, 0x04 }},
            { FileTypes.WebM, new byte[]{ 0x1A, 0x45, 0xDF, 0xA3 }},
            { FileTypes.Mp4, new byte[]{ 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 }}
        };

            public static FileTypes FileType(ReadOnlySpan<byte> Bytes)
            {
                foreach (KeyValuePair<FileTypes, byte[]> KeyValuePair in FileHeaders)
                {
                    switch (Bytes.Length >= KeyValuePair.Value.Length)
                    {
                        case true:
                            ReadOnlySpan<byte> ReadOnlySpan = Bytes.Slice(0, KeyValuePair.Value.Length);
                            switch (ReadOnlySpan.SequenceEqual(KeyValuePair.Value)) { case true: return KeyValuePair.Key; }
                            break;
                    }
                }
                return FileTypes.Unknown;
            }
        }

        public class Suffixe
        {
            public static string Byte(Int64 value, int decimalPlaces = 1)
            {
                string[] SizeSuffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
                if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
                if (value < 0) { return "-" + Byte(-value, decimalPlaces); }
                if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} Bytes", 0); }
                int mag = (int)Math.Log(value, 1024);
                decimal adjustedSize = (decimal)value / (1L << (mag * 10));
                if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
                {
                    mag += 1;
                    adjustedSize /= 1024;
                }
                return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
            }

            public static string Number(long value)
            {
                if (value >= 100000000000)
                    return (value / 1000000000).ToString("#,0") + "B";
                if (value >= 10000000000)
                    return (value / 1000000000D).ToString("0.#") + "B";
                if (value >= 100000000)
                    return (value / 1000000).ToString("#,0") + "M";
                if (value >= 10000000)
                    return (value / 1000000D).ToString("0.#") + "M";
                if (value >= 100000)
                    return (value / 1000).ToString("#,0") + "K";
                if (value >= 10000)
                    return (value / 1000D).ToString("0.#") + "K";
                return value.ToString("#,0");
            }

            public static string Duration(string format, double seconds)
            {
                TimeSpan duration = TimeSpan.FromSeconds(seconds);
                return string.Format(format, duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);
            }
        }

        public class Random
        {
            private static string Characters = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            public static string Randomization(int length)
            {
                System.Random Random = new System.Random();
                StringBuilder StringBuilder = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    char Char = Characters[Random.Next(0, Characters.Length)];
                    StringBuilder.Append(Char);
                }
                return StringBuilder.ToString();
            }
        }

        public class Stream
        {
            public static T[] Slice<T>(T[] source, int index, int length, bool padToLength = false)
            {
                int n = length;
                T[] slice = null;
                if (source.Length < index + length)
                {
                    n = source.Length - index;
                    if (padToLength)
                    {
                        slice = new T[length];
                    }
                }
                if (slice == null) slice = new T[n];
                Array.Copy(source, index, slice, 0, n);
                return slice;
            }

            public static IEnumerable<T[]> Slices<T>(T[] source, int count, bool padToLength = false)
            {
                for (int i = 0; i < source.Length; i += count)
                    yield return Slice(source, i, count, padToLength);
            }
        }

        public class Cryptography
        {
            public class Algorithm
            {
                public class AES
                {
                    public string PrivateKey { get; set; }
                    public byte[] InitializationVector { get; set; }

                    public string Encrypt(string data)
                    {
                        SHA256 PBKDF2SHA256 = SHA256Managed.Create();
                        byte[] key = PBKDF2SHA256.ComputeHash(Encoding.ASCII.GetBytes(this.PrivateKey));
                        Aes encryptor = Aes.Create();
                        encryptor.Mode = CipherMode.CBC;
                        byte[] aesKey = new byte[32];
                        Array.Copy(key, 0, aesKey, 0, 32);
                        encryptor.Key = aesKey;
                        encryptor.IV = this.InitializationVector;
                        MemoryStream memoryStream = new MemoryStream();
                        ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();
                        CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);
                        byte[] plainBytes = Encoding.ASCII.GetBytes(data);
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        byte[] cipherBytes = memoryStream.ToArray();
                        memoryStream.Close();
                        cryptoStream.Close();
                        string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
                        return cipherText;
                    }

                    public string Decrypt(string data)
                    {
                        SHA256 PBKDF2SHA256 = SHA256Managed.Create();
                        byte[] key = PBKDF2SHA256.ComputeHash(Encoding.ASCII.GetBytes(this.PrivateKey));
                        Aes encryptor = Aes.Create();
                        encryptor.Mode = CipherMode.CBC;
                        byte[] aesKey = new byte[32];
                        Array.Copy(key, 0, aesKey, 0, 32);
                        encryptor.Key = aesKey;
                        encryptor.IV = this.InitializationVector;
                        MemoryStream memoryStream = new MemoryStream();
                        ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();
                        CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);
                        string plainText = String.Empty;
                        try
                        {
                            byte[] cipherBytes = Convert.FromBase64String(data);
                            cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            byte[] plainBytes = memoryStream.ToArray();
                            plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
                        }
                        finally
                        {
                            memoryStream.Close();
                            cryptoStream.Close();
                        }
                        return plainText;
                    }
                }

                public class RSA
                {
                    public string PrivateKey { get; set; }
                    public string PublicKey { get; set; }

                    public string Encrypt(string data, bool usePublicKey = true)
                    {
                        var rsa = this.asRSACryptoServiceProvider(usePublicKey: usePublicKey);
                        byte[] encryptedData = rsa.Encrypt(System.Text.Encoding.UTF8.GetBytes(data), true);
                        var str = Convert.ToBase64String(encryptedData);
                        return (str);
                    }

                    public string Decrypt(string data, bool usePublicKey = false)
                    {
                        var rsa = this.asRSACryptoServiceProvider(usePublicKey: usePublicKey);
                        byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(data), true);
                        var str = System.Text.Encoding.UTF8.GetString(decryptedData);
                        return (str);
                    }

                    public string Hash(string data, bool usePublicKey = false)
                    {
                        var rsa = this.asRSACryptoServiceProvider(usePublicKey: usePublicKey);
                        byte[] encryptedData = rsa.SignData(System.Text.Encoding.UTF8.GetBytes(data), new SHA512CryptoServiceProvider());
                        var str = Convert.ToBase64String(encryptedData);
                        return (str);
                    }

                    public void VerifyHash(string data, string hash, bool usePublicKey = false)
                    {
                        var rsa = this.asRSACryptoServiceProvider(usePublicKey: usePublicKey);
                        var isValid = rsa.VerifyData(System.Text.Encoding.UTF8.GetBytes(data), new SHA512CryptoServiceProvider(), Convert.FromBase64String(hash));
                        if (!isValid) { throw new CryptographicException("Invalid hash."); }
                    }

                    public RSACryptoServiceProvider asRSACryptoServiceProvider(bool usePublicKey = false)
                    {
                        var keyStr = usePublicKey ? this.PublicKey : this.PrivateKey;
                        var pemReader = new PemReader(new StringReader(keyStr));
                        if (usePublicKey)
                        {
                            var asymmetricKey = (AsymmetricKeyParameter)pemReader.ReadObject();
                            pemReader.Reader.Close();
                            var rsaKey = (Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters)asymmetricKey;
                            var rsaKeyInfo = Org.BouncyCastle.Security.DotNetUtilities.ToRSAParameters(rsaKey);
                            var rsa = new RSACryptoServiceProvider();
                            rsa.ImportParameters(rsaKeyInfo);
                            return (rsa);
                        }
                        else
                        {
                            var pemObject = pemReader.ReadObject();
                            Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters rsaKey;
                            if (pemObject is Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)
                            {
                                rsaKey = (Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)pemObject;
                            }
                            else
                            {
                                var asymmetricKey = (AsymmetricCipherKeyPair)pemObject;
                                rsaKey = (Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)asymmetricKey.Private;
                            }
                            pemReader.Reader.Close();
                            var rsaKeyInfo = Org.BouncyCastle.Security.DotNetUtilities.ToRSAParameters(rsaKey);
                            var rsa = new RSACryptoServiceProvider();
                            rsa.ImportParameters(rsaKeyInfo);
                            return (rsa);
                        }
                    }

                    public static RSA GenerateKeyPair(string signatureAlgorithm = null, int rsaKeyLength = 2048)
                    {
                        if (string.IsNullOrEmpty(signatureAlgorithm)) { signatureAlgorithm = PkcsObjectIdentifiers.Sha512WithRsaEncryption.Id; }
                        var rsaKeyPairGenerator = new RsaKeyPairGenerator();
                        rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), rsaKeyLength));
                        var pair = rsaKeyPairGenerator.GenerateKeyPair();
                        var configuration = new RSA();
                        var privateKeyStrBuilder = new StringBuilder();
                        var privateKeyPemWriter = new PemWriter(new StringWriter(privateKeyStrBuilder));
                        privateKeyPemWriter.WriteObject(pair.Private);
                        privateKeyPemWriter.Writer.Flush();
                        privateKeyPemWriter.Writer.Close();
                        configuration.PrivateKey = privateKeyStrBuilder.ToString();
                        var publicKeyStrBuilder = new StringBuilder();
                        var publicKeyPemWriter = new PemWriter(new StringWriter(publicKeyStrBuilder));
                        publicKeyPemWriter.WriteObject(pair.Public);
                        publicKeyPemWriter.Writer.Flush();
                        publicKeyPemWriter.Writer.Close();
                        configuration.PublicKey = publicKeyStrBuilder.ToString();
                        return (configuration);
                    }
                }
            }

            public class Signature
            {
                public static string MD5(byte[] bytes)
                {
                    using (System.Security.Cryptography.MD5 MD5 = System.Security.Cryptography.MD5.Create())
                    {
                        byte[] dataBytes = bytes;
                        byte[] hashBytes = MD5.ComputeHash(dataBytes);
                        StringBuilder StringBuilder = new StringBuilder();
                        for (int i = 0; i < hashBytes.Length; i++) { StringBuilder.Append(hashBytes[i].ToString("X2")); }
                        return StringBuilder.ToString();
                    }
                }

                public static string SHA256(byte[] bytes)
                {
                    using (SHA256 SHA256 = System.Security.Cryptography.SHA256.Create())
                    {
                        StringBuilder StringBuilder = new StringBuilder();
                        for (int i = 0; i < bytes.Length; i++) { StringBuilder.Append(bytes[i].ToString("x2")); }
                        return StringBuilder.ToString();
                    }
                }
            }
        }

        public class Net
        {
            public static IPAddress LocalIPAddress()
            {
                IPHostEntry IPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress IPAddress in IPHostEntry.AddressList)
                {
                    switch (IPAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        case true: return IPAddress;
                    }
                }
                throw new Exception("No network adapters with an IPv4 address in the system!");
            }

            public static string MacAddress()
            {
                string String = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where( nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback )
                    .Select( nic => nic.GetPhysicalAddress().ToString())
                    .FirstOrDefault();
                
                return  String;
            }

            public static int AvailablePort(int startingPort)
            {
                IPGlobalProperties IPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var TcpConnectionEnumerable = IPGlobalProperties.GetActiveTcpConnections()
                .Where(n => n.LocalEndPoint.Port >= startingPort)
                .Select(n => n.LocalEndPoint.Port);
                var TcpListenerEnumerable = IPGlobalProperties.GetActiveTcpListeners()
                .Where(n => n.Port >= startingPort)
                .Select(n => n.Port);
                var UdpListenerEnumerable = IPGlobalProperties.GetActiveUdpListeners()
                .Where(n => n.Port >= startingPort)
                .Select(n => n.Port);
                int Index = Enumerable
                    .Range(startingPort, ushort.MaxValue)
                    .Where(i => !TcpConnectionEnumerable.Contains(i))
                    .Where(i => !TcpListenerEnumerable.Contains(i))
                    .FirstOrDefault(i => !UdpListenerEnumerable.Contains(i));
                return Index;
            }
        }
    }

// Copyright © 2023 KendallDawson. All rights reserved.