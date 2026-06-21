namespace AetherisOne;

using System.Text.Json;
using AetherisOne.Models;


public partial class ReflectionPage : ContentPage

{
    private List<Quote> _quotes = new();    

    private int _currentQuoteIndex = 0;

    private readonly Random _random = new();

    private HashSet<string> _favoriteQuoteIds = [];

    private bool _showFavoritesOnly = false;

    private string _selectedCategory = "All";

    public ReflectionPage()
    {
        InitializeComponent();

        _ = InitializeAppAsync();
    }

    private async Task InitializeAppAsync()
    {
        await LoadQuotesAsync();
        await LoadFavoritesAsync();

        _currentQuoteIndex = _random.Next(_quotes.Count);

        DisplayCurrentQuote();

        PrepareIntroAnimation();

        await RunIntroAnimation();
    }

    
    private void DisplayCurrentQuote()
    {
        if (_quotes.Count == 0 || _currentQuoteIndex < 0)
        {
            QuoteLabel.Text = "No quotes loaded.";
            AuthorLabel.Text = "— Aetheris One";
            ReflectionLabel.Text = "Check quotes.json.";
            return;
        }

        Quote currentQuote = _quotes[_currentQuoteIndex];

        QuoteLabel.Text = currentQuote.Text;
        AuthorLabel.Text = $"— {currentQuote.Author}";
        ReflectionLabel.Text = $"Reflection: {currentQuote.Reflection}";

        UpdateFavoriteButton();
    }

    private async void OnNextReflectionClicked(object? sender, EventArgs e)
    {
        List<Quote> activePool = GetActiveQuotePool();

        if (activePool.Count == 0)
        {
            QuoteLabel.Text = "No favourite quotes yet.";
            AuthorLabel.Text = "— Aetheris One";
            ReflectionLabel.Text = "Tap the heart to save reflections.";
            return;
        }

        int newIndex;

        if (activePool.Count == 1)
        {
            Quote selectedQuote = activePool[0];
            newIndex = _quotes.FindIndex(q => q.Id == selectedQuote.Id);
        }
        else
        {
            do
            {
                Quote selectedQuote = activePool[_random.Next(activePool.Count)];
                newIndex = _quotes.FindIndex(q => q.Id == selectedQuote.Id);
            }
            while (newIndex == _currentQuoteIndex);
        }

        await Task.WhenAll(
            QuoteLabel.FadeToAsync(0, 180),
            AuthorLabel.FadeToAsync(0, 180),
            ReflectionLabel.FadeToAsync(0, 180)
        );

        _currentQuoteIndex = newIndex;

        DisplayCurrentQuote();

        await Task.WhenAll(
            QuoteLabel.FadeToAsync(1, 220),
            AuthorLabel.FadeToAsync(1, 220),
            ReflectionLabel.FadeToAsync(1, 220)
        );
    }

    private async void OnFavoriteClicked(object? sender, EventArgs e)
    {
        if (_quotes.Count == 0)
        {
            return;
        }

        Quote currentQuote = _quotes[_currentQuoteIndex];

        if (_favoriteQuoteIds.Contains(currentQuote.Id))
        {
            _favoriteQuoteIds.Remove(currentQuote.Id);
        }
        else
        {
            _favoriteQuoteIds.Add(currentQuote.Id);
        }

        await SaveFavoritesAsync();

        UpdateFavoriteButton();

        await FavoriteButton.ScaleToAsync(1.15, 120);
        await FavoriteButton.ScaleToAsync(1.0, 120);
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

        FavoriteButton.Opacity = 0;
        FavoriteButton.Scale = 0.95;
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

        await Task.Delay(120);

        await FavoriteButton.FadeToAsync(1, 250);
        await FavoriteButton.ScaleToAsync(1.10, 160, Easing.CubicOut);
        await FavoriteButton.ScaleToAsync(1, 160, Easing.CubicIn);
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

    private async Task SaveFavoritesAsync()
    {
        string json = JsonSerializer.Serialize(_favoriteQuoteIds);

        string filePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "favorites.json");

        await File.WriteAllTextAsync(filePath, json);
    }

    private async Task LoadFavoritesAsync()
    {
        string filePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "favorites.json");

        if (!File.Exists(filePath))
        {
            _favoriteQuoteIds = [];
            return;
        }

        string json = await File.ReadAllTextAsync(filePath);

        HashSet<string>? favoriteIds =
            JsonSerializer.Deserialize<HashSet<string>>(json);

        if (favoriteIds is not null)
        {
            _favoriteQuoteIds = favoriteIds;
        }
    }

    private void UpdateFavoriteButton()
    {
        if (_quotes.Count == 0)
        {
            return;
        }

        string currentId = _quotes[_currentQuoteIndex].Id;

        if (_favoriteQuoteIds.Contains(currentId))
        {
            FavoriteButton.Text = "✓";
            FavoriteButton.BackgroundColor = Color.FromArgb("#2ECC71");
        }
        else
        {
            FavoriteButton.Text = "♥";
            FavoriteButton.BackgroundColor = Color.FromArgb("#C0392B");
        }
    }

    private void OnAllFilterClicked(object? sender, EventArgs e)
    {
        _showFavoritesOnly = false;

        AllFilterButton.BackgroundColor = Color.FromArgb("#1F6FEB");
        AllFilterButton.TextColor = Colors.White;

        FavoritesFilterButton.BackgroundColor = Color.FromArgb("#1A2840");
        FavoritesFilterButton.TextColor = Color.FromArgb("#D6E4F0");

        DisplayRandomQuoteFromActivePool();
    }

    private void OnFavoritesFilterClicked(object? sender, EventArgs e)
    {
        _showFavoritesOnly = true;

        FavoritesFilterButton.BackgroundColor = Color.FromArgb("#1F6FEB");
        FavoritesFilterButton.TextColor = Colors.White;

        AllFilterButton.BackgroundColor = Color.FromArgb("#1A2840");
        AllFilterButton.TextColor = Color.FromArgb("#D6E4F0");

        DisplayRandomQuoteFromActivePool();
    }

    private List<Quote> GetActiveQuotePool()
    {
        IEnumerable<Quote> activePool = _quotes;

        if (_showFavoritesOnly)
        {
            activePool = activePool
                .Where(q => _favoriteQuoteIds.Contains(q.Id));
        }

        string categoryValue = MapCategoryToJsonValue(_selectedCategory);

        if (categoryValue != "All")
        {
            activePool = activePool
                .Where(q => q.Category == categoryValue);
        }

        return activePool.ToList();
    }

    private void DisplayRandomQuoteFromActivePool()
    {
        List<Quote> activePool = GetActiveQuotePool();

        if (activePool.Count == 0)
        {
            QuoteLabel.Text = "No matching quotes found.";
            AuthorLabel.Text = "— Aetheris One";
            ReflectionLabel.Text = "Try another category or save more favourites.";

            FavoriteButton.Text = "♥";
            FavoriteButton.BackgroundColor = Color.FromArgb("#C0392B");

            return;
        }

        Quote selectedQuote = activePool[_random.Next(activePool.Count)];

        _currentQuoteIndex = _quotes.FindIndex(q => q.Id == selectedQuote.Id);

        DisplayCurrentQuote();
    }

    private string MapCategoryToJsonValue(string selectedCategory)
    {
        return selectedCategory switch
        {
            "Stoicism" => "Stoicism",
            "Greek" => "Greek Philosophy",
            "Eastern" => "Eastern Philosophy",
            "Practical" => "Practical Wisdom",
            _ => "All"
        };
    }

    private async void OnCategorySelectorClicked(object? sender, EventArgs e)
    {
        string selected = await DisplayActionSheetAsync(
            "Select Category",
            "Cancel",
            null,
            "All",
            "Stoicism",
            "Greek",
            "Eastern",
            "Practical");

        if (selected == "Cancel" || string.IsNullOrWhiteSpace(selected))
        {
            return;
        }

        _selectedCategory = selected;

        CategorySelectorButton.Text = $"{selected} ▼";

        DisplayRandomQuoteFromActivePool();
    }

    private async void OnLogoTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

}