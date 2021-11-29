namespace Lab1.Credentials
{
    using System;
    using System.Collections.Generic;

    public class UserRegistrar
    {
        private readonly CredentialsJournalHolder credentialsJournalHolder;

        public UserRegistrar()
        {
            credentialsJournalHolder = new CredentialsJournalHolder();
        }
        public void CreateUser(string login, string password)
        {
            var isUserExist = CheckCollision(credentialsJournalHolder.Journal, login);
            if (isUserExist)
            {
                Console.WriteLine("User with this login already exists");
                return;
            }

            if (!ValidatePassword(password))
            {
                Console.WriteLine("Password is invalid");
                return;
            }
            
            credentialsJournalHolder.Journal.Add(new UserRecord
            {
                Login = login,
                Password = password
            });
            credentialsJournalHolder.SaveOnDisk();
            Console.WriteLine("User has been registered");
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