using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;

namespace TradingToolClient.WPF.ViewModel
{
    public partial class TickerDataViewModel : ObservableObject
    {
        private readonly HubConnection _hubConnection;
        private readonly HttpClient _httpClient;

        public ObservableCollection<string> Currencies { get; } = new();
        public ObservableCollection<TickerData> TickerDataList { get; } = new();
        public ObservableCollection<TickerData> FilteredTickerDataList { get; } = new();

        [ObservableProperty]
        private string selectedCurrency;

        public TickerDataViewModel()
        {
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(httpClientHandler);
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://192.168.178.36:7139/tickerHub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => httpClientHandler;
                })
                .Build();

            _hubConnection.On("NewTickerDataAvailable", async () =>
            {
                await FetchTickerData();
            });

            InitializeSignalR();
            LoadCurrencies();
        }

        private async void InitializeSignalR()
        {
            try
            {
                await _hubConnection.StartAsync();
                await FetchTickerData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to SignalR hub: {ex.Message}");
            }
        }

        private async void LoadCurrencies()
        {
            try
            {
                var currencies = await _httpClient.GetFromJsonAsync<ObservableCollection<string>>("https://192.168.178.36:7139/crypto/update"); // Replace with your server URL
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Currencies.Clear();
                    foreach (var currency in currencies)
                    {
                        Currencies.Add(currency);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading currencies: {ex.Message}");
            }
        }

        partial void OnSelectedCurrencyChanged(string value)
        {
            FetchTickerDataForSelectedCurrency();
        }

        private void FilterTickerData()
        {
            if (string.IsNullOrEmpty(SelectedCurrency))
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredTickerDataList.Clear();
                foreach (var data in TickerDataList.Where(t => t.Crypto == SelectedCurrency))
                {
                    FilteredTickerDataList.Add(data);
                }
            });
        }

        private async void FetchTickerDataForSelectedCurrency()
        {
            if (string.IsNullOrEmpty(SelectedCurrency))
                return;

            try
            {
                var tickerData = await _httpClient.GetFromJsonAsync<ObservableCollection<TickerData>>($"https://192.168.178.36:7139/ticker/{SelectedCurrency}"); // Replace with your server URL
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TickerDataList.Clear();
                    foreach (var data in tickerData)
                    {
                        TickerDataList.Add(data);
                    }
                    FilterTickerData();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching ticker data for {SelectedCurrency}: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task FetchTickerData()
        {
            try
            {
                var tickerData = await _httpClient.GetFromJsonAsync<ObservableCollection<TickerData>>("https://192.168.178.36:7139/ticker/all"); // Replace with your server URL
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TickerDataList.Clear();
                    foreach (var data in tickerData)
                    {
                        TickerDataList.Add(data);
                    }
                    FilterTickerData();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching ticker data: {ex.Message}");
            }
        }
    }
}


