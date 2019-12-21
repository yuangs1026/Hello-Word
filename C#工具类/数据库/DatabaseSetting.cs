using System;
using System.Collections.Generic;
using System.Text;

namespace GetEnviData
{
    class DatabaseSetting
    {
        public iniFile ProfileFile;
        public string ServerName;
        public string DatabaseName;
        public string LogId;
        public string LogPass;
        public string LastTime; //最近一次上传时间，格式：yyyy-MM-dd HH:mm:ss
        public DatabaseSetting()
        {
            ServerName = "";
            DatabaseName = "";
            LogId = "";
            LogPass = "";
            ProfileFile = new iniFile(AppDomain.CurrentDomain.BaseDirectory + "\\config.ini");
        }
        public DatabaseSetting(string profileName)
        {
            ProfileFile= new iniFile(profileName);
            GetSetting();
        }
        public void GetSetting()
        {
            ServerName = ProfileFile.GetValue("Database","ServerName");
            DatabaseName = ProfileFile.GetValue("Database","DatabaseName");
            LogId = ProfileFile.GetValue("Database","LogId");
            LogPass = ProfileFile.GetValue("Database","LogPass");
        }
        public void SetSetting()
        {
            ProfileFile.SetValue("Database", "ServerName",ServerName);
            ProfileFile.SetValue("Database", "DatabaseName", DatabaseName);
            ProfileFile.SetValue("Database", "LogId", LogId);
            ProfileFile.SetValue("Database", "LogPass", LogPass);
        }

        public void GetLastTime()
        {
            LastTime = ProfileFile.GetValue("Addition", "LastTime");
        }
        public void SetLastTime()
        {
            ProfileFile.SetValue("Addition", "LastTime", LastTime);
        }
    }
}
