using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace TradingToolClient.WPF.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private Page currentPage;

        [ObservableProperty]
        private bool isMenuVisible;

        public MainWindowViewModel()
        {
            NavigateToTickerDataPageCommand = new RelayCommand(NavigateToTickerDataPage);
            NavigateToPage2Command = new RelayCommand(NavigateToPage2);
            IsMenuVisible = true; // Menu is visible by default
        }

        public RelayCommand NavigateToTickerDataPageCommand { get; }
        public RelayCommand NavigateToPage2Command { get; }

        private void NavigateToTickerDataPage()
        {
            CurrentPage = new View.TickerDataPage();
        }

        private void NavigateToPage2()
        {
            // Implement navigation to Page2
        }
    }
}
