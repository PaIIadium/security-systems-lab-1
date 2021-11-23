namespace Lab1.Registration
{
    using System;
    using System.Collections.Generic;

    public class UserRegistrar
    {
        public bool CreateUser(string login, string password)
        {
            var journalHolder = new CredentialsJournalHolder();
            var isUserExist = CheckCollision(journalHolder.Journal, login);
            if (isUserExist)
            {
                Console.WriteLine("User with this login already exists");
                return false;
            }

            if (!ValidatePassword(password))
            {
                Console.WriteLine("Password is invalid");
                return false;
            }
            
            journalHolder.Journal.Add(new UserRecord
            {
                Login = login,
                Password = password
            });
            journalHolder.SaveOnDisk();
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

        private bool ValidatePassword(string password)
        {
            return password.Length >= 4;
        }
    }
}