namespace Lab1
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class OperationsJournalHolder
    {
        private const string Path = "./Storage/System/operations_journal.txt";
        private List<OperationRecord> journal;

        public OperationsJournalHolder()
        {
            LoadJournalFromDisk();
        }

        private void LoadJournalFromDisk()
        {
            if (!File.Exists(Path)) return;
            
            var credentialsList = File.ReadLines(Path)
                .Select(line => line.Split(","))
                .Select(record => new OperationRecord
                {
                    Login = record[0],
                    Time = record[1],
                    Action = record[2],
                    Args = record[3]
                })
                .ToList();

            journal = credentialsList;
        }

        public void AddRecord(string login, string time, string action, string args = "")
        {
            journal.Add(new OperationRecord
            {
                Login = login,
                Time = time,
                Action = action,
                Args = args
            });
            SaveOnDisk();
        }

        private void SaveOnDisk()
        {
            var lines = journal.Select(record => 
                $"{record.Login},{record.Time},{record.Action},{record.Args}");
            File.WriteAllLines(Path, lines);
        }
    }

    public struct OperationRecord
    {
        public string Login;
        public string Time;
        public string Action;
        public string Args;
    }
}