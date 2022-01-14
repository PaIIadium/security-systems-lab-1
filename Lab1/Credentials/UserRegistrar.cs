namespace Lab1.Credentials
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class UserRegistrar
    {
        private readonly CredentialsJournalHolder credentialsJournalHolder;

        private const int PasswordMinLength = 8;

        private static readonly List<PasswordRule> PasswordRules = new()
        {
            new PasswordRule
            {
                Regex = new Regex("[A-Z]"),
                MatchesCount = 2,
                Comment = "Password should contain at least 2 capital letters"
            },
            new PasswordRule
            {
                Regex = new Regex("[a-z]"),
                MatchesCount = 2,
                Comment = "Password should contain at least 2 lowercase letters"
            },

            new PasswordRule
            {
                Regex = new Regex("[0-9]"),
                MatchesCount = 2,
                Comment = "Password should contain at least 2 numbers"
            },

            new PasswordRule
            {
                Regex = new Regex("[^A-z0-9]"),
                MatchesCount = 1,
                Comment = "Password should contain at least 1 non-alphanumeric character"
            },

            new PasswordRule
            {
                Regex = new Regex(@"."),
                MatchesCount = PasswordMinLength,
                Comment = $"Password should contain at least {PasswordMinLength} characters"
            }
        };

        public UserRegistrar(CredentialsJournalHolder credentialsJournalHolder)
        {
            this.credentialsJournalHolder = credentialsJournalHolder;
        }
        public void CreateUser(string login, string password)
        {
            var isUserExist = CheckCollision(credentialsJournalHolder.Journal, login);
            if (isUserExist)
            {
                Console.WriteLine("User with this login already exists");
                return;
            }

            if (!ValidatePassword(password)) return;
            
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
            if (CheckWhitespaces(password))
            {
                Console.WriteLine("Password should not contain any whitespace character");
                return false;
            }
            return CheckPasswordRules(password);
        }

        private bool CheckWhitespaces(string password)
        {
            return new Regex(@"\s").IsMatch(password);
        }
        
        private bool CheckPasswordRules(string password)
        {
            foreach (var rule in PasswordRules)
            {
                if (rule.Regex.Matches(password).Count < rule.MatchesCount)
                {
                    Console.WriteLine(rule.Comment);
                    return false;
                }
            }
            return true;
        }
    }

    public struct PasswordRule
    {
        public Regex Regex;
        public int MatchesCount;
        public string Comment;
    }
}