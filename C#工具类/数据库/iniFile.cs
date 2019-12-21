using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace GetEnviData
{
    class iniFile
    {
        private string m_Path;
        # region Windows API Functions
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string lpAppName,
            string lpKeyName, string lpReturnString, string lpFilePath);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string lpAppName,
            string lpKeyName, string lpDefault, byte[] byBuffer, int size, string lpFilePath);

        [DllImport("kernel32")]
        public static extern int WritePrivateProfileSection(string lpAppName,
            string lpString, string lpFileName);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileSection(string lpAppName,
            string lpReturnString, string lpFilePath);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileInt(string lpAppName,
            string lpKeyName, int iDefault, string lpFilePath);
        #endregion

        public iniFile(string strIniPath)
        {
            m_Path = strIniPath;
        }

        public long SetValue(string section, string key, string value)
        {
            return WritePrivateProfileString(section, key, value, m_Path);
        }

        private string[] GetValues(string section, string key)
        {
            byte[] buff;
            int BUFFER_SIZE = 0xff;
            int iCount, iBufferSize, iMultiple = 0;
            do
            {
                iMultiple++;
                iBufferSize = BUFFER_SIZE * iMultiple;
                buff = new byte[iBufferSize];
                iCount = GetPrivateProfileString(section, key, "", buff, iBufferSize, m_Path);
            }
            while (iCount == iBufferSize - 2);

            for (int i = 0; i < iCount; ++i)
            {
                if (buff[i] == 0 && iCount != i + 1 && buff[i + 1] != 0)
                {
                    buff[i] = (int)'\n';
                    continue;
                }
                if (i > 0 && buff[i - 1] == 0 && buff[i] == 0) break;
            }
            string strResult = Encoding.Default.GetString(buff).Trim();
            return strResult.Trim(new char[] { '\0' }).Split(new char[] { '\n' });
        }

        public string GetValue(string section, string key)
        {
            if (section == null || key == null)
            {
                throw new NullReferenceException("Section or Key");
            }
            return GetValues(section, key)[0];
        }

        public string[] GetKeys(string section)
        {
            if (section == null)
            {
                throw new NullReferenceException("Section");
            }
            return GetValues(section, null);
        }

        public string[] GetSections()
        {
            return GetValues(null, null);
        }

        public long SetValueInt(string section, string key, int value)
        {
            return SetValue(section, key, value.ToString());
        }

        public int GetValueInt(string section, string key)
        {
            return GetPrivateProfileInt(section, key, -1, m_Path);
        }

        public int SetSection(string strSection, string strKeyValue)
        {
            return WritePrivateProfileSection(strSection, strKeyValue, m_Path);
        }

        public int DeleteSection(string strSection)
        {
            return SetSection(strSection, null);
        }
    }
}
