namespace Lab1
{
    using System;
    using System.Threading.Tasks;

    public class QuestionsController
    {
        public event Action OnTimeToAskQuestions;
        
        private const int A = 15;
        private const int QuestionsInterval = 240;
        private const int QuestionsCount = 2;
        
        public async void RunQuestionsTimer()
        {
            await Task.Delay(QuestionsInterval * 1000);
            OnTimeToAskQuestions?.Invoke();
        }

        public bool AskQuestions()
        {
            for (var i = 0; i < QuestionsCount - 1; i++)
            {
                var correctAnswer = AskQuestion();
                var answer = Console.ReadLine();
                var isAnswerCorrect = ValidateAnswer(correctAnswer, answer);
                if (!isAnswerCorrect) return false;
            }

            return true;
        }

        public int AskQuestion()
        {
            var random = new Random();
            var randomInt = random.Next(100, 10000);
            Console.WriteLine($"Enter answer for number {randomInt}");
            var correctAnswer = (int)Math.Round(MathF.Tan(A * randomInt));
            return correctAnswer;
        }
        
        public bool ValidateAnswer(int correctValue, string userInput)
        {
            var formattedInput = userInput.Replace("\r", "").Trim();
            if (int.TryParse(formattedInput, out var integer))
            {
                if (correctValue != integer)
                {
                    return false;
                }

                Console.WriteLine("Correct");
                return true;
            }
            return false;
        }
    }
}