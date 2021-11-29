namespace Lab1.Credentials
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Authenticator
    {
        private const int MaxMistakesCount = 5;
        
        private Dictionary<string, int> usersMistakes = new ();

        private readonly CredentialsJournalHolder credentialsJournalHolder;
        
        public Authenticator()
        {
            credentialsJournalHolder = new CredentialsJournalHolder();
        }
        
        public async Task<string> Authenticate()
        {
            Console.WriteLine("Enter login and password (login:password)");
            string login;
            while (true)
            {
                var inputCredentials = Console.ReadLine()?.Split(":");
                if (inputCredentials is {Length: 2})
                {
                    login = inputCredentials[0];
                    var password = inputCredentials[1];
                    var isCorrectCredentials = CheckCredentials(login, password);
                    if (isCorrectCredentials)
                    {
                        if (!usersMistakes.ContainsKey(login)) usersMistakes.Add(login, 0);
                        break;
                    }
                    Console.WriteLine("Login or password is incorrect. Try again");
                }
                else
                {
                    Console.WriteLine("Invalid input format. Try again");
                }
            }

            return login;
        }
        
        private bool CheckCredentials(string login, string password)
        {
            return credentialsJournalHolder.Journal
                .Exists(record => record.Login == login && record.Password == password);
        }

        public void QuestionMistakeHandler(string login)
        {
            usersMistakes[login]++;
            if (usersMistakes[login] == MaxMistakesCount)
            {
                BlockAccount(login);
            }
        }

        private void BlockAccount(string login)
        {
            credentialsJournalHolder.Journal.RemoveAll(record => record.Login == login);
            credentialsJournalHolder.SaveOnDisk();

            usersMistakes.Remove(login);
            Console.WriteLine("Your account has been blocked. Contact with administrator");
        }
    }
}