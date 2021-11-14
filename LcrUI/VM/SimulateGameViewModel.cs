using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LcrSimulation;
using LcrUI.Models;
using WpfCommon;

namespace LcrUI.VM
{
    public class SimulateGameViewModel : NotifyPropertyChanged
    {
        #region static
        public static List<Preset> AvailablePresets { get; }
        static SimulateGameViewModel()
        {
            AvailablePresets = new List<Preset>();
            AvailablePresets.Add(new Preset(3, 100));
            AvailablePresets.Add(new Preset(4, 100));
            AvailablePresets.Add(new Preset(5, 100));
            AvailablePresets.Add(new Preset(5, 1000));
            AvailablePresets.Add(new Preset(5, 10000));
            AvailablePresets.Add(new Preset(5, 100000));
            AvailablePresets.Add(new Preset(6, 100));
            AvailablePresets.Add(new Preset(7, 100));
            AvailablePresets.Add(new Preset(20, 1000000));
        }
        #endregion

        #region properties/members

        private Preset selectedPreset;
        public Preset SelectedPreset
        {
            get { return selectedPreset; }
            set
            {
                selectedPreset = value;
                OnPropertyChanged(nameof(SelectedPreset));
            }
        }

        private ICommand? playCommand;
        public ICommand? PlayCommand
        {
            get
            {
                return playCommand ??= new RelayCommand(Play, CanPlay);
            }
        }

        private ICommand? cancelCommand;
        public ICommand? CancelCommand
        {
            get
            {
                return cancelCommand ??= new RelayCommand(Cancel, CanCancel);
            }
        }

        private bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        private string progressText = String.Empty;
        public string ProgressText
        {
            get { return progressText; }
            set
            {
                progressText = value;
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        private int currentProgress;
        public int CurrentProgress
        {
            get { return currentProgress; }
            set
            {
                currentProgress = value;
                OnPropertyChanged(nameof(CurrentProgress));
            }
        }

        private bool updatePlot = true;
        public bool UpdatePlot
        {
            get { return updatePlot; }
            set
            {
                updatePlot = value;
                OnPropertyChanged(nameof(UpdatePlot));
            }
        }

        public List<KeyValuePair<int, int>> SimulationResults { set; get; }
        public List<KeyValuePair<int, double>> SimulationAverage { set; get; }

        public ObservableCollection<PlayerInfo> PlayersInfo { get; } = new ObservableCollection<PlayerInfo>();

        private bool cancelPending = false;
        #endregion

        #region constructor
        public SimulateGameViewModel()
        {
            selectedPreset = AvailablePresets.First();
        }
        #endregion

        #region private methods
        private void Play(object? obj)
        {
            if (SelectedPreset.NumGames > 1000 && UpdatePlot)
            {
                if (MessageBox.Show(
                    "You have choosen the option to update the plot." + 
                    Environment.NewLine + Environment.NewLine +
                    "It is advised that the 'Update Plot' option to be turned off when there are a large number of datapoints, " +
                    "as doing so may lock up the UI." +
                    Environment.NewLine + Environment.NewLine +
                    "Are you sure you wish to continue?", 
                    "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation
                    ) != MessageBoxResult.Yes)
                    return;
            }
            IsBusy = true;
            cancelPending = false;
            PlayersInfo.Clear();
            Task.Factory.StartNew(() => RunSimulation(SelectedPreset.NumGames, SelectedPreset.NumPlayers));
        }

        private bool CanPlay(object? obj) => !IsBusy && SelectedPreset != null;

        private void Cancel(object? obj)
        {
            cancelPending = true;
        }

        private bool CanCancel(object? obj) => IsBusy && !cancelPending;

        private void RunSimulation(int numGames, int numPlayers)
        {
            var simulationResults = new List<KeyValuePair<int, int>>();
            var simulationAverage = new List<KeyValuePair<int, double>>(); 

            var playerInfoDict = Enumerable.Range(0, numPlayers).ToDictionary(x => x, x => new PlayerInfo(x + 1));
            Application.Current?.Dispatcher?.Invoke(new Action(() => 
                playerInfoDict.Values.ToList().ForEach(PlayersInfo.Add)), DispatcherPriority.Background);

            var reportProgressMod = numGames / 100;
            for (int i = 0; i < numGames; i++)
            {
                if (cancelPending)
                    break;

                var result = Simulator.SimulateGame(numPlayers);
                playerInfoDict[result.GameWinnerNumber].NumOfWins++;
                simulationResults.Add(new KeyValuePair<int, int>(i, result.TotalGameRounds));

                // only report progress at every 1 percent increments
                if (i % reportProgressMod == 0)
                    Application.Current?.Dispatcher?.Invoke(new Action(() =>
                    {
                        ProgressText = $"Simulating games {i}/{numGames}.";
                        CurrentProgress = i;
                    }), DispatcherPriority.Background);
            }

            var average = Enumerable.Average(simulationResults.Select(kvp => kvp.Value));
            if (simulationResults.Count > 1)
            {
                simulationAverage.Add(new KeyValuePair<int, double>(0, average));
                simulationAverage.Add(new KeyValuePair<int, double>(simulationResults.Count - 1, average));
            }

            var maxWins = PlayersInfo.Max(p => p.NumOfWins);

            Application.Current?.Dispatcher?.Invoke(new Action(() =>
            {
                PlayersInfo.Where(p => p.NumOfWins == maxWins).ToList().ForEach(p => p.IsWinner = true);
                ProgressText = $"Updating Plot..";
                if (UpdatePlot)
                {
                    SimulationResults = simulationResults;
                    SimulationAverage = simulationAverage;
                }
                OnPropertyChanged(nameof(SimulationResults));
                OnPropertyChanged(nameof(SimulationAverage));
                IsBusy = false;
                cancelPending = false;
                ProgressText = $"Simulations Completed.";
            }), DispatcherPriority.Background);
        }
        #endregion
    }
}
