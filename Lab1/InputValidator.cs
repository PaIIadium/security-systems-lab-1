namespace Lab1
{
    public class InputValidator
    {
        public bool ValidateCommand(string input, out Command command)
        {
            switch (input)
            {
                case "ls":
                    command = Command.Ls;
                    return true;
                case "cat":
                    command = Command.Cat;
                    return true;
                case "touch":
                    command = Command.Touch;
                    return true;
                case "rm":
                    command = Command.Rm;
                    return true;
                case "exec":
                    command = Command.Execute;
                    return true;
                case "register":
                    command = Command.Register;
                    return true;
                default:
                    command = default;
                    return false;
            }
        }
    }
}