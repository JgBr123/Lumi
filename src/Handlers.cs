namespace Lumi
{
    internal class Handlers
    {
        public static void CancelHandler(object? sender, ConsoleCancelEventArgs args)
        {
            Console.ResetColor();
            Console.CursorVisible = true;
            LoadingAnimation.Stop("The operation was cancelled by the user.");
        }
    }
}
