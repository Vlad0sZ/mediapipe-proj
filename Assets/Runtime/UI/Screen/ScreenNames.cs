namespace Runtime.UI.Screen
{
    public sealed record ScreenNames(string Name)
    {
        public static ScreenNames NoCamera => new ScreenNames(nameof(NoCamera));
        public static ScreenNames MainMenu => new ScreenNames(nameof(MainMenu));
        public static ScreenNames Settings => new ScreenNames(nameof(Settings));
        public static ScreenNames GamePrepare => new ScreenNames(nameof(GamePrepare));
        public static ScreenNames GameOver => new ScreenNames(nameof(GameOver));
        public static ScreenNames Game => new ScreenNames(nameof(Game));
        public static ScreenNames GameMode => new ScreenNames(nameof(GameMode));
        public static ScreenNames Loading => new ScreenNames(nameof(Loading));
        public static ScreenNames Records => new ScreenNames(nameof(Records));
        public static ScreenNames Pause => new ScreenNames(nameof(Pause));
        public static ScreenNames Character => new ScreenNames(nameof(Character));

        public static implicit operator string(ScreenNames name) =>
            name.Name;
    }
}