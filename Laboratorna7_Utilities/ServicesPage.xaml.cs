using Laboratorna7_Utilities.Data;
using Laboratorna7_Utilities.Models;
using Microsoft.EntityFrameworkCore;

namespace Laboratorna7_Utilities;

public partial class ServicesPage : ContentPage
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ServicesPage(IDbContextFactory<AppDbContext> factory)
    {
        InitializeComponent();
        _factory = factory;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            using var db = _factory.CreateDbContext();
            ServicesView.ItemsSource = await db.Services
                .AsNoTracking()
                .OrderBy(s => s.ServiceName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка БД", ex.InnerException?.Message ?? ex.Message, "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ServiceEditPage(_factory, null));
    }

    private async void OnServiceSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Service service)
        {
            ServicesView.SelectedItem = null;
            await Navigation.PushAsync(new ServiceEditPage(_factory, service));
        }
    }
}