namespace Lab1
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Credentials;

    class Program
    {
        private static Authenticator authenticator;
        private static QuestionsController questionsController;
        private static Role role;
        private static string login;
        private static bool isTimeToAskQuestions;
        private static int correctAnswer;

        private static int userInputOffset;
        
        private static CredentialsJournalHolder credentialsJournalHolder;
        private static OperationsJournalHolder operationsJournalHolder;

        private static readonly Dictionary<Role, Dictionary<Disk, List<Right>>> RolesRights = new()
        {
            {
                Role.Admin, new Dictionary<Disk, List<Right>>
                {
                    {Disk.A, new List<Right> {Right.Read, Right.Write, Right.Execute}},
                    {Disk.B, new List<Right> {Right.Read, Right.Write, Right.Execute}},
                    {Disk.C, new List<Right> {Right.Read, Right.Write, Right.Execute}},
                    {Disk.System, new List<Right> {Right.Read, Right.Write, Right.Execute}},
                    {Disk.None, new List<Right>()}
                }
            },
            {
                Role.User, new Dictionary<Disk, List<Right>>
                {
                    {Disk.A, new List<Right> {Right.Read}},
                    {Disk.B, new List<Right>()},
                    {Disk.C, new List<Right> {Right.Read, Right.Write}},
                    {Disk.System, new List<Right>()},
                    {Disk.None, new List<Right>()}
                }
            }
        };

        private static readonly Dictionary<Command, Right> CommandsRights = new()
        {
            {Command.Ls, Right.Read},
            {Command.Cat, Right.Read},
            {Command.Touch, Right.Write},
            {Command.Rm, Right.Write},
            {Command.Execute, Right.Execute}
        };

        private static async Task Main(string[] args)
        {
            credentialsJournalHolder = new CredentialsJournalHolder();
            operationsJournalHolder = new OperationsJournalHolder();
            authenticator = new Authenticator(credentialsJournalHolder, operationsJournalHolder);
            questionsController = new QuestionsController();
            questionsController.OnTimeToAskQuestions += OnTimeToAskQuestions;
            Authorize();
        }
        
        private static void OnTimeToAskQuestions()
        {
            userInputOffset = Console.CursorLeft;
            ClearCurrentConsoleLine();
            correctAnswer = questionsController.AskQuestion();
            isTimeToAskQuestions = true;
        }
        
        private static void ClearCurrentConsoleLine()
        {
            var currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, currentLineCursor);
        }

        private static async void Authorize()
        {
            await Authenticate();
            RunApplicationLoop();
        }

        private static async Task Authenticate()
        {
            login = await authenticator.Authenticate();
            role = login == "admin" ? Role.Admin : Role.User;
            
            questionsController.RunQuestionsTimer();
            Console.WriteLine("Successful authorization");
        }

        private static void RunApplicationLoop()
        {
            var inputValidator = new InputValidator();
            while (true)
            {
                var input = Console.ReadLine();
                if (isTimeToAskQuestions)
                {
                    isTimeToAskQuestions = false;
                    input = input?[userInputOffset..];
                    var isAnswerCorrect = questionsController.ValidateAnswer(correctAnswer, input);
                    if (isAnswerCorrect)
                    {
                        var areAnswersCorrect = questionsController.AskQuestions();
                        if (areAnswersCorrect)
                        {
                            questionsController.RunQuestionsTimer();
                            continue;
                        }
                    }
                    authenticator.QuestionMistakeHandler(login);
                    Console.WriteLine("Answer is wrong. You need to log in again");
                    operationsJournalHolder.AddRecord(login, DateTime.Now.ToString(), "Wrong answer");
                    break;
                }

                var inputCommand = input?.Split(" ");
                if (inputValidator.ValidateCommand(inputCommand?[0], out var command))
                {
                    HandleInput(command, inputCommand);
                    continue;
                }

                Console.WriteLine($"Unknown command: {inputCommand?[0]}");
            }

            Authorize();
        }

        private static void HandleInput(Command command, string[] args)
        {
            var path = args[1];
            
            if (CommandsRights.TryGetValue(command, out var right))
            {
                var disk = GetDisk(path);
                    
                var isRoleGrantedAccess = IsRoleGrantedAccess(disk, right);
                if (!isRoleGrantedAccess)
                {
                    Console.WriteLine("You don't have permissions to perform this operation");
                    operationsJournalHolder.AddRecord(
                        login, DateTime.Now.ToString(), command.ToString(), string.Join(';', args[1..]));
                    return;
                }
                
                if (args.Length == 3) PerformOperation(command, path, args[2]);
                else PerformOperation(command, path);
                return;
            }
            
            if (command == Command.Register)
            {
                var isRoleGrantedAccess = IsRoleGrantedAccess(Disk.System, Right.Write);
                if (!isRoleGrantedAccess)
                {
                    Console.WriteLine("You don't have permissions to perform this operation");
                    operationsJournalHolder.AddRecord(
                        login, DateTime.Now.ToString(), command.ToString(), string.Join(';', args[1..]));
                    return;
                }

                var userRegistrar = new UserRegistrar(credentialsJournalHolder);
                userRegistrar.CreateUser(args[1], args[2]);
                return;
            }

            Console.WriteLine("Command has not specified rights");
        }

        private static bool IsRoleGrantedAccess(Disk disk, Right right)
        {
            return RolesRights[role][disk].Contains(right);
        }

        private static Disk GetDisk(string path)
        {
            var absPath = Path.GetFullPath(path);
            var currentPath = AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                var disk = absPath[$"{currentPath}Storage\\".Length..][0];
                switch (disk)
                {
                    case 'A':
                        return Disk.A;
                    case 'B':
                        return Disk.B;
                    case 'C':
                        return Disk.C;
                }
            }
            catch
            {
                return Disk.None;
            }

            return Disk.None;
        }

        private static void PerformOperation(Command command, string path, string additionalArg = null)
        {
            switch (command)
            {
                case Command.Ls:
                    PerformLs(path);
                    break;
                case Command.Cat:
                    PerformCat(path);
                    break;
                case Command.Touch:
                    PerformTouch(path, additionalArg);
                    break;
                case Command.Rm:
                    PerformRm(path);
                    break;
                case Command.Execute:
                default:
                    PerformExecute(path);
                    break;
            }
        }

        private static void PerformLs(string path)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path)
                    .Select(filePath => filePath[(path.Length + 1)..]);
                Console.WriteLine(string.Join('\n', files));
            }
            else
            {
                Console.WriteLine($"{path} is not a valid directory.");
            }
        }

        private static void PerformCat(string path)
        {
            if (File.Exists(path))
            {
                var file = File.ReadAllText(path);
                Console.WriteLine(file);
            }
            else
            {
                Console.WriteLine($"{path} is not a valid file.");
            }
        }

        private static void PerformTouch(string path, string filename)
        {
            if (Directory.Exists(path))
            {
                File.Create($"{path}/{filename}");
            }
            else
            {
                Console.WriteLine($"{path} is not a valid directory.");
            }
        }

        private static void PerformRm(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                Console.WriteLine($"{path} is not a valid directory.");
            }
        }

        private static void PerformExecute(string path)
        {
            if (File.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                Console.WriteLine($"{path} is not a valid file.");
            }
        }
    }
}