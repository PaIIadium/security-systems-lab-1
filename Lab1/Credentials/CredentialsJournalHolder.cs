namespace Lab1.Credentials
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class CredentialsJournalHolder
    {
        private const string CredentialsPath = "./Storage/System/credentials_journal.txt";
        public List<UserRecord> Journal;

        public CredentialsJournalHolder()
        {
            LoadJournalFromDisk();
        }

        private void LoadJournalFromDisk()
        {
            if (!File.Exists(CredentialsPath)) return;
            
            var credentialsList = File.ReadLines(CredentialsPath)
                .Select(line => line.Split(","))
                .Select(userInfo => new UserRecord
                {
                    Login = userInfo[0],
                    Password = userInfo[1]
                })
                .ToList();

            Journal = credentialsList;
        }

        public void SaveOnDisk()
        {
            var lines = Journal.Select(userInfo => $"{userInfo.Login},{userInfo.Password}");
            File.WriteAllLines(CredentialsPath, lines);
        }
    }

    public struct UserRecord
    {
        public string Login;
        public string Password;
    }
}