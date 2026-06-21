namespace AetherisOne;

using System.Text.Json;
using AetherisOne.Models;

public partial class MainPage : ContentPage
{
    private List<Quote> _quotes = new();

    public MainPage()
    {
        InitializeComponent();

        _ = InitializeHomeAsync();
    }

    private async Task InitializeHomeAsync()
    {
        await LoadQuotesAsync();

        DisplayQuoteOfTheDay();
    }

    
    private async Task LoadQuotesAsync()
    {
        using Stream stream =
            await FileSystem.OpenAppPackageFileAsync("quotes.json");

        using StreamReader reader = new(stream);

        string json = await reader.ReadToEndAsync();

        List<Quote>? quotes =
            JsonSerializer.Deserialize<List<Quote>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (quotes is not null)
        {
            _quotes = quotes;
        }
    }

    private void DisplayQuoteOfTheDay()
    {
        if (_quotes.Count == 0)
        {
            DailyQuoteLabel.Text = "No quote loaded.";
            DailyAuthorLabel.Text = "— Aetheris One";
            return;
        }

        int dayNumber = DateTime.Today.DayOfYear;

        int quoteIndex = dayNumber % _quotes.Count;

        Quote dailyQuote = _quotes[quoteIndex];

        DailyQuoteLabel.Text = dailyQuote.Text;
        DailyAuthorLabel.Text = $"— {dailyQuote.Author}";
    }

    private async void OnDailyQuoteTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ReflectionPage());
    }
}