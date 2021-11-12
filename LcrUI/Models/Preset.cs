namespace LcrUI.Models
{
    public class Preset
    {
        public int NumPlayers { get; }
        public int NumGames { get; }

        public Preset(int numPlayers, int numGames)
        {
            NumPlayers = numPlayers;
            NumGames = numGames;
        }
    }
}
