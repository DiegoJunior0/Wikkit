using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Reflection;

namespace Wikkit.Services;

public class SettingsService
{

    private SettingsData currentSettings { get; set; }

    public IJSRuntime JSRuntime { get; }

    public SettingsService(IJSRuntime JSRuntime)
    {

        LoadSettings();       

        this.JSRuntime = JSRuntime;
    }

    private void LoadSettings()
    {
        currentSettings = new SettingsData
        {
            DarkMode = Preferences.Default.Get(nameof(currentSettings.DarkMode), true),
            FontSize = Preferences.Default.Get(nameof(currentSettings.FontSize), "medium"),
            RandomizePictures = Preferences.Default.Get(nameof(currentSettings.RandomizePictures), false)
        };
    }

    private void SaveSettings()
    {

        Preferences.Default.Set(nameof(currentSettings.DarkMode), currentSettings.DarkMode);
        Preferences.Default.Set(nameof(currentSettings.FontSize), currentSettings.FontSize);
        Preferences.Default.Set(nameof(currentSettings.RandomizePictures), currentSettings.RandomizePictures);

    }

    public SettingsData GetSettings()
    {
        return currentSettings;
    }

    public async Task UpdateTheme()
    {

        if (currentSettings.DarkMode)
        {
            var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/setOptions.js");
            await module.InvokeVoidAsync("setTheme", "dark");
        }
        else
        {
            var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/setOptions.js");
            await module.InvokeVoidAsync("setTheme", "");
        }
    }

    public async Task UpdateFontSize()
    {

        var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/setOptions.js");
        await module.InvokeVoidAsync("setFontSize", currentSettings.FontSize);

    }

    public async Task UpdateSettings(SettingsData newSettings)
    {
        currentSettings = newSettings;

        SaveSettings();

        await UpdateTheme();
        await UpdateFontSize();
    }

    public async void SetDarkMode(bool value)
    {
        currentSettings.DarkMode = value;

        SaveSettings();

        await UpdateTheme();
    }

    public async void SetFontSize(string value)
    {
        currentSettings.FontSize = value;

        SaveSettings();

        await UpdateFontSize();
    }

}
