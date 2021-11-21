namespace Lab1.Registration
{
    using System.Collections.Generic;

    public class UserRegistrar
    {
        public bool CreateUser(string login, string password)
        {
            var journalHolder = new CredentialsJournalHolder();
            var isUserExist = CheckCollision(journalHolder.Journal, login);
            if (isUserExist) return false;
            journalHolder.Journal.Add(new UserRecord
            {
                Login = login,
                Password = password
            });
            return true;
        }

        private bool CheckCollision(List<UserRecord> journal, string login)
        {
            foreach (var userRecord in journal)
            {
                if (userRecord.Login == login) return true;
            }

            return false;
        }
    }
}