using WpfCommon;

namespace LcrUI.Models
{
    public class PlayerInfo : NotifyPropertyChanged
    {
        public PlayerInfo(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }

        private int numOfWins = 0;
        public int NumOfWins
        {
            get { return numOfWins; }
            set
            {
                numOfWins = value;
                OnPropertyChanged(nameof(NumOfWins));
            }
        }

        private bool isWinner;
        public bool IsWinner
        {
            get { return isWinner; }
            set
            {
                isWinner = value;
                OnPropertyChanged(nameof(IsWinner));
            }
        }
    }
}
