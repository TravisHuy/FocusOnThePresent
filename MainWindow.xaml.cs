using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace FocusOnThePresent
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DispatcherTimer timer;
        private TimeSpan remainingTime;
        private bool isTimerRunning;

        public event PropertyChangedEventHandler PropertyChanged;

        public string StatusMessage { get; set; }
        public string TimerDisplay { get; set; }
        public string ButtonText { get; set; }
        public int FocusedTime { get; set; }
        public ObservableCollection<TreeItem> Trees { get; set; }
        public ObservableCollection<int> FocusTimes { get; set; }
        public ObservableCollection<Tag> Tags { get; set; }
        public TreeItem SelectedTree { get; set; }
        public int SelectedFocusTime { get; set; }
        public Tag SelectedTag { get; set; }

        // Commands
        public ICommand ToggleTimerCommand { get; set; }
        public ICommand PlantCommand { get; set; }
        public ICommand ShowBottomSheetCommand { get; set; }
        public ICommand HideBottomSheetCommand { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            ToggleTimerCommand = new RelayCommand(ToggleTimer);
            PlantCommand = new RelayCommand(Plant);
            ShowBottomSheetCommand = new RelayCommand(ShowBottomSheet);
            HideBottomSheetCommand = new RelayCommand(HideBottomSheet);

            // Initialize collections
            Trees = new ObservableCollection<TreeItem>
            {
                  new TreeItem { Name = "Pine", ImageSource = "pack://application:,,,/FocusOnThePresent;component/Images/ic_edit.png" },
                new TreeItem { Name = "Oak", ImageSource = "pack://application:,,,/FocusOnThePresent;component/Images/tree.png" },
                new TreeItem { Name = "Apple", ImageSource = "pack://application:,,,/FocusOnThePresent;component/Images/tree.png" },
                // Add more trees as needed
            };
            FocusTimes = new ObservableCollection<int> { 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 115, 120 };
            Tags = new ObservableCollection<Tag>
            {
                new Tag { Name = "Study", Color = "Blue" },
                new Tag { Name = "Work", Color = "Red" },
                new Tag { Name = "Exercise", Color = "Green" },
                new Tag { Name = "Other", Color = "Yellow" }
            };

            ResetTimer();
        }

        private void ToggleTimer()
        {
            if (isTimerRunning)
            {
                StopTimer();
            }
            else
            {

                StartTimer(TimeSpan.FromMinutes(SelectedFocusTime > 0 ? SelectedFocusTime : 10));

            }
        }

        private void StartTimer(TimeSpan duration)
        {
            remainingTime = duration;
            timer.Start();
            isTimerRunning = true;
            ButtonText = "Cancel";
            StatusMessage = "Stop phubbing!";
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(StatusMessage));
        }

        private void StopTimer()
        {
            timer.Stop();
            isTimerRunning = false;
            ResetTimer();
        }

        private void ResetTimer()
        {
            remainingTime = TimeSpan.FromMinutes(10); // Default to 10 minutes
            UpdateTimerDisplay();
            ButtonText = "Plant";
            StatusMessage = "Ready to focus?";
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(StatusMessage));
            ResetBottomSheetValues();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (remainingTime.TotalSeconds > 0)
            {
                remainingTime = remainingTime.Add(TimeSpan.FromSeconds(-1));
                UpdateTimerDisplay();
            }
            else
            {
                StopTimer();
                MessageBox.Show("Focus time completed!");
                FocusedTime += SelectedFocusTime; // Add selected focus time to total focused time
                OnPropertyChanged(nameof(FocusedTime));
            }
        }

        private void UpdateTimerDisplay()
        {
            TimerDisplay = remainingTime.TotalHours >= 1
        ? remainingTime.ToString(@"hh\:mm\:ss")
        : remainingTime.ToString(@"mm\:ss");
            OnPropertyChanged(nameof(TimerDisplay));
        }

        private void ShowBottomSheet()
        {
            Storyboard sb = (Storyboard)FindResource("ShowBottomSheet");
            sb.Begin();
        }

        private void HideBottomSheet()
        {
            Storyboard sb = (Storyboard)FindResource("HideBottomSheet");
            sb.Begin();
        }

        private void Plant()
        {
            if (SelectedTree != null && SelectedFocusTime > 0 && SelectedTag != null)
            {
                MessageBox.Show($"Planted a {SelectedTree.Name} tree for {SelectedFocusTime} minutes with {SelectedTag.Name} tag!");
                StartTimer(TimeSpan.FromMinutes(SelectedFocusTime));
                HideBottomSheet();
            }
            else
            {
                MessageBox.Show("Please select a tree, focus time, and tag before planting.");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResetBottomSheetValues()
        {
            SelectedFocusTime = 10;
            SelectedTree = Trees.FirstOrDefault();
            SelectedTag = Tags.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedFocusTime));
            OnPropertyChanged(nameof(SelectedTree));
            OnPropertyChanged(nameof(SelectedTag));
        }
    }

    public class Tag
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class TreeItem
    {
        public string Name { get; set; }
        public string ImageSource { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute) => _execute = execute;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute();
    }
}