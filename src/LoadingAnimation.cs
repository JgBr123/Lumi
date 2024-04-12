namespace Lumi
{
    class LoadingAnimation
    {
        private static Thread animationThread;
        private static bool runAnimation;
        private static string stopMessage;

        private static Dictionary<string, string[]> animations = new()
        {
            { "0", [ "⠋", "⠙", "⠚", "⠞", "⠖", "⠦", "⠴", "⠲", "⠳", "⠓" ] },
            { "1", [ "⠄", "⠆", "⠇", "⠋", "⠙", "⠸", "⠰", "⠠", "⠰", "⠸", "⠙", "⠋", "⠇", "⠆" ] },
            { "2", [ "⠋", "⠙", "⠚", "⠒", "⠂", "⠂", "⠒", "⠲", "⠴", "⠦", "⠖", "⠒", "⠐", "⠐", "⠒", "⠓", "⠋" ] },
            { "3", [ "⠁", "⠉", "⠙", "⠚", "⠒", "⠂", "⠂", "⠒", "⠲", "⠴", "⠤", "⠄", "⠄", "⠤", "⠴", "⠲", "⠒", "⠂", "⠂", "⠒", "⠚", "⠙", "⠉", "⠁" ] },
            { "4", [ "⠈", "⠉", "⠋", "⠓", "⠒", "⠐", "⠐", "⠒", "⠖", "⠦", "⠤", "⠠", "⠠", "⠤", "⠦", "⠖", "⠒", "⠐", "⠐", "⠒", "⠓", "⠋", "⠉", "⠈" ] },
            { "5", [ "⠁", "⠁", "⠉", "⠙", "⠚", "⠒", "⠂", "⠂", "⠒", "⠲", "⠴", "⠤", "⠄", "⠄", "⠤", "⠠", "⠠", "⠤", "⠦", "⠖", "⠒", "⠐", "⠐", "⠒", "⠓", "⠋", "⠉", "⠈", "⠈" ] }
        };
        public static void Start(string message)
        {
            runAnimation = true;
            animationThread = new Thread(() => Animation(message));
            animationThread.Start();
        }
        public static void Stop(string message)
        {
            runAnimation = false;
            stopMessage = message;
            if (animationThread != null) animationThread.Join();
        }
        public static void Animation(string message)
        {
            var animationIndex = Random.Shared.Next(6); //Generates random number from 0 to 5
            string[] frames = animations[animationIndex.ToString()];

            var cursorPosition = Console.GetCursorPosition();
            while (runAnimation)
            {
                foreach (string frame in frames)
                {
                    Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);
                    Utils.Print($"*{frame}* {message}");
                    Console.Title = $"{frame} {message}";
                    Thread.Sleep(75);
                    Console.CursorVisible = false;

                    if (!runAnimation) break;
                }
            }
            Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);
            Utils.PrintLine(stopMessage);
            Console.CursorVisible = true;
        }
    }
}
