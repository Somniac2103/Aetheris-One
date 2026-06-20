namespace AetherisOne;

using System.Text.Json;
using AetherisOne.Models;


public partial class MainPage : ContentPage

{
    private List<Quote> _quotes = new();    

    private int _currentQuoteIndex = 0;

    private readonly Random _random = new();

    public MainPage()
    {
        InitializeComponent();

        _ = InitializeAppAsync();
    }

    private async Task InitializeAppAsync()
    {
        await LoadQuotesAsync();

        DisplayCurrentQuote();

        PrepareIntroAnimation();

        await RunIntroAnimation();
    }

    private void DisplayCurrentQuote()
    {
        _currentQuoteIndex = _random.Next(_quotes.Count);

        Quote currentQuote = _quotes[_currentQuoteIndex];

        QuoteLabel.Text = currentQuote.Text;
        AuthorLabel.Text = $"— {currentQuote.Author}";
        ReflectionLabel.Text = $"Reflection: {currentQuote.Reflection}";
    }

    private void OnNextReflectionClicked(object? sender, EventArgs e)
    {
        int newIndex;

        do
        {
            newIndex = _random.Next(_quotes.Count);
        }
        while (newIndex == _currentQuoteIndex);

        _currentQuoteIndex = newIndex;

        DisplayCurrentQuote();
    }

    private void PrepareIntroAnimation()
    {
        HeroSection.TranslationY = 60;

        ModuleCard.Opacity = 0;
        ModuleCard.TranslationY = 24;

        QuoteCard.Opacity = 0;
        QuoteCard.TranslationY = 24;

        NextReflectionButton.Opacity = 0;
        NextReflectionButton.Scale = 0.95;
    }

    private async Task RunIntroAnimation()
    {
        await Task.Delay(500);

        await Task.WhenAll(
            HeroSection.TranslateToAsync(0, 0, 450, Easing.CubicOut),
            ModuleCard.FadeToAsync(1, 450),
            ModuleCard.TranslateToAsync(0, 0, 450, Easing.CubicOut),
            QuoteCard.FadeToAsync(1, 550),
            QuoteCard.TranslateToAsync(0, 0, 550, Easing.CubicOut)
        );

        await NextReflectionButton.FadeToAsync(1, 250);
        await NextReflectionButton.ScaleToAsync(1.06, 160, Easing.CubicOut);
        await NextReflectionButton.ScaleToAsync(1, 160, Easing.CubicIn);
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
}