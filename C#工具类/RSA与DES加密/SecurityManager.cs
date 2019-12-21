using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Xml;
using System.IO;
using Org.BouncyCastle.OpenSsl;
using System.Xml.Linq;


    public class SecurityManager
    {
        private static string _encryptKey = "og5j11a";//默认的对称加密秘钥(长度为8位，少于8位方法返回空，多于8位，截取前8位)
        private static string _iv = "7825d11a";//DES加密偏移量(8位)
        private static string ALLCHAR = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";//随机数字
        private static string RSA_PADDING = "RSA/ECB/PKCS1Padding";//使用该模式加密解密，为使js与java调用同一个加密解密方法，
                                                                   //私钥为Pkcs8格式，
        /// <summary>随机字符串，可做对称加密算法DES的秘钥</summary>
        /// <param name="length">随机字符串长度</param>
        /// <returns></returns>
        public static string GenerateString(int length)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(ALLCHAR[random.Next(ALLCHAR.Length)]);
            }
            return sb.ToString();
        }

        /// <summary>AES加密</summary>  
        /// <param name="text">明文</param>  
        /// <param name="key">密钥,长度为16的字符串</param>  
        /// <param name="iv">偏移量,长度为16的字符串</param>  
        /// <returns>密文</returns>  
        public static string EncodeAES(string text, string key, string iv)
        {
            int size = key.Length;
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.Zeros;
            rijndaelCipher.KeySize = 128;
            rijndaelCipher.BlockSize = 128;
            byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[16];
            int length = pwdBytes.Length;
            if (length > keyBytes.Length)
                length = keyBytes.Length;
            System.Array.Copy(pwdBytes, keyBytes, length);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = Encoding.UTF8.GetBytes(iv);
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(text);
            byte[] cipherBytes = transform.TransformFinalBlock(plainText, 0, plainText.Length);
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>AES解密</summary>  
        /// <param name="text">密文</param>  
        /// <param name="key">密钥,长度为16的字符串</param>  
        /// <param name="iv">偏移量,长度为16的字符串</param>  
        /// <returns>明文</returns>  
        public static string DecodeAES(string text, string key, string iv)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.Zeros;
            rijndaelCipher.KeySize = 128;
            rijndaelCipher.BlockSize = 128;
            byte[] encryptedData = Convert.FromBase64String(text);
            byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[16];
            int length = pwdBytes.Length;
            if (length > keyBytes.Length)
                length = keyBytes.Length;
            System.Array.Copy(pwdBytes, keyBytes, length);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = Encoding.UTF8.GetBytes(iv);
            ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
            byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return Encoding.UTF8.GetString(plainText);
        }

        #region RSA加密-使用三方插件BouncyCastleCrypto

        /// <summary>    
        /// RSA私钥格式转换，java->.net    
        /// </summary>    
        /// <param name="privateKey">java生成的RSA私钥</param>    
        /// <returns></returns>    
        public static string RSAPrivateKeyJava2DotNet(string privateKey)
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));

            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary>    
        /// RSA私钥格式转换，.net->java    
        /// </summary>    
        /// <param name="privateKey">.net生成的私钥</param>    
        /// <returns></returns>    
        public static string RSAPrivateKeyDotNet2Java(string privateKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(privateKey);
            BigInteger m = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
            BigInteger exp = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
            BigInteger d = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("D")[0].InnerText));
            BigInteger p = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("P")[0].InnerText));
            BigInteger q = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Q")[0].InnerText));
            BigInteger dp = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DP")[0].InnerText));
            BigInteger dq = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DQ")[0].InnerText));
            BigInteger qinv = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("InverseQ")[0].InnerText));

            RsaPrivateCrtKeyParameters privateKeyParam = new RsaPrivateCrtKeyParameters(m, exp, d, p, q, dp, dq, qinv);

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParam);
            byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetEncoded();
            return Convert.ToBase64String(serializedPrivateBytes);
        }

        /// <summary>    
        /// RSA公钥格式转换，java->.net    
        /// </summary>    
        /// <param name="publicKey">java生成的公钥</param>    
        /// <returns></returns>    
        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary>    
        /// RSA公钥格式转换，.net->java    
        /// </summary>    
        /// <param name="publicKey">.net生成的公钥</param>    
        /// <returns></returns>    
        public static string RSAPublicKeyDotNet2Java(string publicKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(publicKey);
            BigInteger m = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
            BigInteger p = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
            RsaKeyParameters pub = new RsaKeyParameters(false, m, p);

            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pub);
            byte[] serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            return Convert.ToBase64String(serializedPublicBytes);
        }

        /// <summary>
        /// Public Key Convert pem->xml
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string RSAPublicKeyPemToXml(string publicKey)
        {
            publicKey = RSAPublicKeyFormat(publicKey);

            PemReader pr = new PemReader(new StringReader(publicKey));
            RsaKeyParameters rsaKey = pr.ReadObject() as RsaKeyParameters;
            if (rsaKey == null)
            {
                throw new Exception("Public key format is incorrect");
            }
            XElement publicElement = new XElement("RSAKeyValue");
            //Modulus
            XElement pubmodulus = new XElement("Modulus", Convert.ToBase64String(rsaKey.Modulus.ToByteArrayUnsigned()));
            //Exponent
            XElement pubexponent = new XElement("Exponent", Convert.ToBase64String(rsaKey.Exponent.ToByteArrayUnsigned()));

            publicElement.Add(pubmodulus);
            publicElement.Add(pubexponent);
            return publicElement.ToString();
        }

        /// <summary>
        /// Public Key Convert xml->pem
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string RSAPublicKeyXmlToPem(string publicKey)
        {
            XElement root = XElement.Parse(publicKey);
            //Modulus
            var modulus = root.Element("Modulus");
            //Exponent
            var exponent = root.Element("Exponent");

            RsaKeyParameters rsaKeyParameters = new RsaKeyParameters(false, new BigInteger(1, Convert.FromBase64String(modulus.Value)), new BigInteger(1, Convert.FromBase64String(exponent.Value)));

            StringWriter sw = new StringWriter();
            PemWriter pWrt = new PemWriter(sw);
            pWrt.WriteObject(rsaKeyParameters);
            pWrt.Writer.Close();
            return sw.ToString();
        }

        /// <summary>
        /// Private Key Convert Pkcs1->xml
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSAPrivateKeyPkcs1ToXml(string privateKey)
        {
            privateKey = RSAPkcs1PrivateKeyFormat(privateKey);

            PemReader pr = new PemReader(new StringReader(privateKey));
            AsymmetricCipherKeyPair asymmetricCipherKeyPair = pr.ReadObject() as AsymmetricCipherKeyPair;
            if (asymmetricCipherKeyPair == null)
            {
                throw new Exception("Private key format is incorrect");
            }

            RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters =
                (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(
                    PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymmetricCipherKeyPair.Private));

            XElement privatElement = new XElement("RSAKeyValue");
            //Modulus
            XElement primodulus = new XElement("Modulus", Convert.ToBase64String(rsaPrivateCrtKeyParameters.Modulus.ToByteArrayUnsigned()));
            //Exponent
            XElement priexponent = new XElement("Exponent", Convert.ToBase64String(rsaPrivateCrtKeyParameters.PublicExponent.ToByteArrayUnsigned()));
            //P
            XElement prip = new XElement("P", Convert.ToBase64String(rsaPrivateCrtKeyParameters.P.ToByteArrayUnsigned()));
            //Q
            XElement priq = new XElement("Q", Convert.ToBase64String(rsaPrivateCrtKeyParameters.Q.ToByteArrayUnsigned()));
            //DP
            XElement pridp = new XElement("DP", Convert.ToBase64String(rsaPrivateCrtKeyParameters.DP.ToByteArrayUnsigned()));
            //DQ
            XElement pridq = new XElement("DQ", Convert.ToBase64String(rsaPrivateCrtKeyParameters.DQ.ToByteArrayUnsigned()));
            //InverseQ
            XElement priinverseQ = new XElement("InverseQ", Convert.ToBase64String(rsaPrivateCrtKeyParameters.QInv.ToByteArrayUnsigned()));
            //D
            XElement prid = new XElement("D", Convert.ToBase64String(rsaPrivateCrtKeyParameters.Exponent.ToByteArrayUnsigned()));

            privatElement.Add(primodulus);
            privatElement.Add(priexponent);
            privatElement.Add(prip);
            privatElement.Add(priq);
            privatElement.Add(pridp);
            privatElement.Add(pridq);
            privatElement.Add(priinverseQ);
            privatElement.Add(prid);

            return privatElement.ToString();
        }

        /// <summary>
        /// Private Key Convert xml->Pkcs1
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSAPrivateKeyXmlToPkcs1(string privateKey)
        {
            XElement root = XElement.Parse(privateKey);
            //Modulus
            var modulus = root.Element("Modulus");
            //Exponent
            var exponent = root.Element("Exponent");
            //P
            var p = root.Element("P");
            //Q
            var q = root.Element("Q");
            //DP
            var dp = root.Element("DP");
            //DQ
            var dq = root.Element("DQ");
            //InverseQ
            var inverseQ = root.Element("InverseQ");
            //D
            var d = root.Element("D");

            RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters = new RsaPrivateCrtKeyParameters(
                new BigInteger(1, Convert.FromBase64String(modulus.Value)),
                new BigInteger(1, Convert.FromBase64String(exponent.Value)),
                new BigInteger(1, Convert.FromBase64String(d.Value)),
                new BigInteger(1, Convert.FromBase64String(p.Value)),
                new BigInteger(1, Convert.FromBase64String(q.Value)),
                new BigInteger(1, Convert.FromBase64String(dp.Value)),
                new BigInteger(1, Convert.FromBase64String(dq.Value)),
                new BigInteger(1, Convert.FromBase64String(inverseQ.Value)));

            StringWriter sw = new StringWriter();
            PemWriter pWrt = new PemWriter(sw);
            pWrt.WriteObject(rsaPrivateCrtKeyParameters);
            pWrt.Writer.Close();
            return sw.ToString();

        }


        /// <summary>
        /// Private Key Convert Pkcs8->xml
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSAPrivateKeyPkcs8ToXml(string privateKey)
        {
            privateKey = RSAPkcs8PrivateKeyFormatRemove(privateKey);
            RsaPrivateCrtKeyParameters privateKeyParam =
                (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));

            XElement privatElement = new XElement("RSAKeyValue");
            //Modulus
            XElement primodulus = new XElement("Modulus", Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()));
            //Exponent
            XElement priexponent = new XElement("Exponent", Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()));
            //P
            XElement prip = new XElement("P", Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()));
            //Q
            XElement priq = new XElement("Q", Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()));
            //DP
            XElement pridp = new XElement("DP", Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()));
            //DQ
            XElement pridq = new XElement("DQ", Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()));
            //InverseQ
            XElement priinverseQ = new XElement("InverseQ", Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()));
            //D
            XElement prid = new XElement("D", Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));

            privatElement.Add(primodulus);
            privatElement.Add(priexponent);
            privatElement.Add(prip);
            privatElement.Add(priq);
            privatElement.Add(pridp);
            privatElement.Add(pridq);
            privatElement.Add(priinverseQ);
            privatElement.Add(prid);

            return privatElement.ToString();
        }

        /// <summary>
        /// Private Key Convert xml->Pkcs8
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSAPrivateKeyXmlToPkcs8(string privateKey)
        {
            XElement root = XElement.Parse(privateKey);
            //Modulus
            var modulus = root.Element("Modulus");
            //Exponent
            var exponent = root.Element("Exponent");
            //P
            var p = root.Element("P");
            //Q
            var q = root.Element("Q");
            //DP
            var dp = root.Element("DP");
            //DQ
            var dq = root.Element("DQ");
            //InverseQ
            var inverseQ = root.Element("InverseQ");
            //D
            var d = root.Element("D");

            RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters = new RsaPrivateCrtKeyParameters(
                new BigInteger(1, Convert.FromBase64String(modulus.Value)),
                new BigInteger(1, Convert.FromBase64String(exponent.Value)),
                new BigInteger(1, Convert.FromBase64String(d.Value)),
                new BigInteger(1, Convert.FromBase64String(p.Value)),
                new BigInteger(1, Convert.FromBase64String(q.Value)),
                new BigInteger(1, Convert.FromBase64String(dp.Value)),
                new BigInteger(1, Convert.FromBase64String(dq.Value)),
                new BigInteger(1, Convert.FromBase64String(inverseQ.Value)));

            StringWriter swpri = new StringWriter();
            PemWriter pWrtpri = new PemWriter(swpri);
            Pkcs8Generator pkcs8 = new Pkcs8Generator(rsaPrivateCrtKeyParameters);
            pWrtpri.WriteObject(pkcs8);
            pWrtpri.Writer.Close();
            return swpri.ToString();

        }

        /// <summary>
        /// Private Key Convert Pkcs1->Pkcs8
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSAPrivateKeyPkcs1ToPkcs8(string privateKey)
        {
            privateKey = RSAPkcs1PrivateKeyFormat(privateKey);
            PemReader pr = new PemReader(new StringReader(privateKey));

            AsymmetricCipherKeyPair kp = pr.ReadObject() as AsymmetricCipherKeyPair;
            StringWriter sw = new StringWriter();
            PemWriter pWrt = new PemWriter(sw);
            Pkcs8Generator pkcs8 = new Pkcs8Generator(kp.Private);
            pWrt.WriteObject(pkcs8);
            pWrt.Writer.Close();
            string result = sw.ToString();
            return result;
        }

        /// <summary>
        /// Private Key Convert Pkcs8->Pkcs1
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSAPrivateKeyPkcs8ToPkcs1(string privateKey)
        {
            privateKey = RSAPkcs8PrivateKeyFormat(privateKey);
            PemReader pr = new PemReader(new StringReader(privateKey));

            RsaPrivateCrtKeyParameters kp = pr.ReadObject() as RsaPrivateCrtKeyParameters;

            var keyParameter = PrivateKeyFactory.CreateKey(PrivateKeyInfoFactory.CreatePrivateKeyInfo(kp));

            StringWriter sw = new StringWriter();
            PemWriter pWrt = new PemWriter(sw);
            pWrt.WriteObject(keyParameter);
            pWrt.Writer.Close();
            string result = sw.ToString();
            return result;
        }

        /// <summary>
        /// RSA生成公钥和私钥Java
        /// </summary>
        /// <returns></returns>
        public static RSAKey RSAGenerateKeysJava()
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                string publicKey = RSAPublicKeyDotNet2Java(rsa.ToXmlString(false));
                string privateKey = RSAPrivateKeyDotNet2Java(rsa.ToXmlString(true));
                RSAKey key = new RSAKey();
                key.PublicKey = publicKey;
                key.PrivateKey = privateKey;
                return key;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// RSA生成公钥和私钥.net
        /// </summary>
        /// <returns></returns>
        public static RSAKey RSAGenerateKeysDotNet()
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                RSAKey key = new RSAKey();
                key.PublicKey = rsa.ToXmlString(false);
                key.PrivateKey = rsa.ToXmlString(true);
                return key;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// RSA公钥加密
        /// </summary>
        /// <param name="toEncryptString">需要进行加密的字符串</param>
        /// <param name="publicKey">公钥pem格式</param>
        /// <returns></returns>
        public static string RSAEncrypt(string toEncryptString, string key)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher(RSA_PADDING);
            PemReader r = new PemReader(new StringReader(key));//载入公钥文件
            Object readObject = r.ReadObject();
            //实例化参数实例
            AsymmetricKeyParameter priKey = readObject as AsymmetricKeyParameter;
            //AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)readObject;//PKCS1
            cipher.Init(true, priKey);//false表示解密
            byte[] data = Encoding.UTF8.GetBytes(toEncryptString);
            string encryptData = Convert.ToBase64String(cipher.DoFinal(data));
            return encryptData;
        }

        /// <summary>
        /// RSA公钥解密
        /// </summary>
        /// <param name="base64code">需要进行解密的字符串</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static string RSADecrypt(string base64code, string key)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher(RSA_PADDING);
            PemReader r = new PemReader(new StringReader(key));//载入公钥文件
            Object readObject = r.ReadObject();
            //实例化参数实例
            AsymmetricKeyParameter priKey = readObject as AsymmetricKeyParameter;
            //AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)readObject;//PKCS1
            cipher.Init(false, priKey);//false表示解密
            byte[] encryptData = Convert.FromBase64String(base64code);
            string decryptData = Encoding.UTF8.GetString(cipher.DoFinal(encryptData));
            return decryptData;
        }

        /// <summary>
        /// Format Pkcs1 format private key
        /// Author:Zhiqiang Li
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RSAPkcs1PrivateKeyFormat(string str)
        {
            if (str.StartsWith("-----BEGIN RSA PRIVATE KEY-----"))
            {
                return str;
            }

            List<string> res = new List<string>();
            res.Add("-----BEGIN RSA PRIVATE KEY-----");

            int pos = 0;

            while (pos < str.Length)
            {
                var count = str.Length - pos < 64 ? str.Length - pos : 64;
                res.Add(str.Substring(pos, count));
                pos += count;
            }

            res.Add("-----END RSA PRIVATE KEY-----");
            var resStr = string.Join("\r\n", res.ToArray());
            return resStr;
        }

        /// <summary>
        /// Remove the Pkcs1 format private key format
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RSAPkcs1PrivateKeyFormatRemove(string str)
        {
            if (!str.StartsWith("-----BEGIN RSA PRIVATE KEY-----"))
            {
                return str;
            }
            return str.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "")
                .Replace("\r\n", "");
        }

        /// <summary>
        /// Format Pkcs8 format private key
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RSAPkcs8PrivateKeyFormat(string str)
        {
            if (str.StartsWith("-----BEGIN PRIVATE KEY-----"))
            {
                return str;
            }
            List<string> res = new List<string>();
            res.Add("-----BEGIN PRIVATE KEY-----");

            int pos = 0;

            while (pos < str.Length)
            {
                var count = str.Length - pos < 64 ? str.Length - pos : 64;
                res.Add(str.Substring(pos, count));
                pos += count;
            }

            res.Add("-----END PRIVATE KEY-----");
            var resStr = string.Join("\r\n", res.ToArray());
            return resStr;
        }

        /// <summary>
        /// Remove the Pkcs8 format private key format
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RSAPkcs8PrivateKeyFormatRemove(string str)
        {
            if (!str.StartsWith("-----BEGIN PRIVATE KEY-----"))
            {
                return str;
            }
            return str.Replace("-----BEGIN PRIVATE KEY-----", "").Replace("-----END PRIVATE KEY-----", "")
                .Replace("\r\n", "");
        }

        /// <summary>
        /// Format public key，变成公钥文件格式，带着-----BEGIN PUBLIC KEY-----，换行符等
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RSAPublicKeyFormat(string str)
        {
            if (str.StartsWith("-----BEGIN PUBLIC KEY-----"))
            {
                return str;
            }
            List<string> res = new List<string>();
            res.Add("-----BEGIN PUBLIC KEY-----");
            int pos = 0;

            while (pos < str.Length)
            {
                var count = str.Length - pos < 64 ? str.Length - pos : 64;
                res.Add(str.Substring(pos, count));
                pos += count;
            }
            res.Add("-----END PUBLIC KEY-----");
            var resStr = string.Join("\r\n", res.ToArray());
            return resStr;
        }

        /// <summary>
        /// Public key format removed
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RSAPublicKeyFormatRemove(string str)
        {
            if (!str.StartsWith("-----BEGIN PUBLIC KEY-----"))
            {
                return str;
            }
            return str.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "")
                .Replace("\r\n", "");
        }

        #endregion

        #region DES加密

        /// <summary>
        /// 对称加密算法DES加密,如果没有传递密钥则默认
        /// </summary>
        /// <param name="toEncryptString">需要进行加密的字符串</param>
        /// <param name="encryptKey">对称秘钥</param>
        /// <returns></returns>
        public static string DESEncrypt(string toEncryptString, string encryptKey)
        {
            if (string.IsNullOrEmpty(encryptKey))
            {
                encryptKey = _encryptKey;
            }
            string cipher = string.Empty;
            if (encryptKey.Length > 8)
            {
                encryptKey = encryptKey.Remove(8);
            }
            else if (encryptKey.Length < 8)
            {
                return cipher;
            }

            if (_iv.Length > 8)
            {
                _iv = _iv.Remove(8);
            }
            else if (_iv.Length < 8)
            {
                return cipher;
            }
            SymmetricAlgorithm serviceProvider = new DESCryptoServiceProvider();
            serviceProvider.Key = Encoding.ASCII.GetBytes(encryptKey);
            serviceProvider.IV = Encoding.ASCII.GetBytes(_iv);

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, serviceProvider.CreateEncryptor(), CryptoStreamMode.Write);
            StreamWriter streamWriter = new StreamWriter(cryptoStream);

            streamWriter.Write(toEncryptString);
            streamWriter.Dispose();
            cryptoStream.Dispose();

            byte[] signData = memoryStream.ToArray();
            memoryStream.Dispose();
            serviceProvider.Clear();
            cipher = Convert.ToBase64String(signData);

            return cipher;
        }

        /// <summary>
        /// 对称加密算法DES解密,如果没有传递密钥则默认
        /// </summary>
        /// <param name="toDecryptString">需要进行解密的字符串</param>
        /// <param name="decryptKey">对称秘钥</param>
        /// <returns></returns>
        public static string DESDecrypt(string toDecryptString, string decryptKey)
        {
            if (string.IsNullOrEmpty(decryptKey))
            {
                decryptKey = _encryptKey;
            }
            string cipher = string.Empty;
            if (decryptKey.Length > 8)
            {
                decryptKey = decryptKey.Remove(8);
            }
            else if (decryptKey.Length < 8)
            {
                return cipher;
            }

            if (_iv.Length > 8)
            {
                _iv = _iv.Remove(8);
            }
            else if (_iv.Length < 8)
            {
                return cipher;
            }

            SymmetricAlgorithm serviceProvider = new DESCryptoServiceProvider();
            serviceProvider.Key = Encoding.ASCII.GetBytes(decryptKey);
            serviceProvider.IV = Encoding.ASCII.GetBytes(_iv);

            byte[] contentArray = Convert.FromBase64String(toDecryptString);
            MemoryStream memoryStream = new MemoryStream(contentArray);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, serviceProvider.CreateDecryptor(), CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(cryptoStream);

            cipher = streamReader.ReadToEnd();

            streamReader.Dispose();
            cryptoStream.Dispose();
            memoryStream.Dispose();
            serviceProvider.Clear();
            return cipher;
        }

        /// <summary>
        /// 对称加密算法DES的秘钥，随机字符串
        /// </summary>
        /// <param name="length">随机字符串长度</param>
        /// <returns></returns>
        public static string DESGenerateString(int length)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(ALLCHAR[random.Next(ALLCHAR.Length)]);
            }
            return sb.ToString();
        }

        #endregion

        #region MD5加密

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="toEncryptString">需要进行加密的字符串</param>
        /// <returns></returns>
        public static string MD5Encrypt(string toEncryptString)
        {
            StringBuilder sb = new StringBuilder();

            using (MD5 md5 = MD5.Create())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(toEncryptString);
                byte[] newB = md5.ComputeHash(buffer);

                foreach (byte item in newB)
                {
                    sb.Append(item.ToString("x2"));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// MD5加盐加密，将随机字符加入到加密后字符串中的算法为网上所找的,非自己写出的算法
        /// </summary>
        /// <param name="toEncryptString"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string MD5EncryptSalt(string toEncryptString, string salt)
        {
            toEncryptString = MD5Encrypt(toEncryptString + salt);
            char[] cs = new char[48];
            for (int i = 0; i < 48; i += 3)
            {
                cs[i] = toEncryptString[i / 3 * 2];
                char c = salt[i / 3];
                cs[i + 1] = c;
                cs[i + 2] = toEncryptString[i / 3 * 2 + 1];
            }
            return new string(cs);
        }

        /// <summary>
        /// 校验MD5加盐后字符串是否正确
        /// </summary>
        /// <param name="toEncryptString">需要加密的字符串</param>
        /// <param name="encryptMd5">md5加盐加密后字符串</param>
        /// <returns></returns>
        public static bool MD5SaltVerify(string toEncryptString, string md5Salt)
        {
            char[] cs1 = new char[32];
            char[] cs2 = new char[16];
            for (int i = 0; i < 48; i += 3)
            {
                cs1[i / 3 * 2] = md5Salt[i];
                cs1[i / 3 * 2 + 1] = md5Salt[i + 2];
                cs2[i / 3] = md5Salt[i + 1];
            }
            String salt = new String(cs2);
            return MD5Encrypt(toEncryptString + salt) == new String(cs1);
        }

        #endregion
    }

    public class RSAKey
    {
        public string PublicKey;
        public string PrivateKey;
    }

