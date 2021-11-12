using System;
using System.Collections.Generic;
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

        public List<KeyValuePair<int, int>> SimulationResults { set; get; }
        public List<KeyValuePair<int, double>> SimulationAverage { set; get; }

        private bool cancelPending = false;
        #endregion

        public SimulateGameViewModel()
        {
            selectedPreset = AvailablePresets.First();
        }

        private void Play(object? obj)
        {
            IsBusy = true;
            cancelPending = false;
            Task.Factory.StartNew(() => RunSimulation(SelectedPreset.NumGames, SelectedPreset.NumPlayers));
        }

        private bool CanPlay(object? obj)
        {
            return !IsBusy && SelectedPreset != null;
        }

        private void Cancel(object? obj)
        {
            cancelPending = true;
        }

        private bool CanCancel(object? obj)
        {
            return IsBusy && !cancelPending;
        }

        private void RunSimulation(int numGames, int numPlayers)
        {
            SimulationResults = new List<KeyValuePair<int, int>>();
            SimulationAverage = new List<KeyValuePair<int, double>>();

            for (int i = 0; i < numGames; i++)
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ProgressText = $"Simulating games {i}/{numGames}.";
                        CurrentProgress = i;
                    }
                    ), DispatcherPriority.Background);
                }

                if (cancelPending)
                    break;

                var result = Simulator.SimulateGame(numPlayers);
                SimulationResults.Add(new KeyValuePair<int, int>(i, result.TotalGameRounds));
            }

            var average = Enumerable.Average(SimulationResults.Select(kvp => kvp.Value));

            if (SimulationResults.Count > 1)
            {
                SimulationAverage.Add(new KeyValuePair<int, double>(0, average));
                SimulationAverage.Add(new KeyValuePair<int, double>(SimulationResults.Count - 1, average));
            }

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ProgressText = $"Simulations Completed.";
                    IsBusy = false;
                    cancelPending = false;
                    OnPropertyChanged(nameof(SimulationResults));
                    OnPropertyChanged(nameof(SimulationAverage));
                }), DispatcherPriority.Background);
            }
        }
    }
}
