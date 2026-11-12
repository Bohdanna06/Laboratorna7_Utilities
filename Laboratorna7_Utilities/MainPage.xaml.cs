using Laboratorna7_Utilities.Data;
using Laboratorna7_Utilities.Models;
using Microsoft.EntityFrameworkCore;

namespace Laboratorna7_Utilities;

public partial class MainPage : ContentPage
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public MainPage(IDbContextFactory<AppDbContext> factory)
    {
        InitializeComponent();
        _factory = factory;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            using var db = _factory.CreateDbContext();
            TenantsView.ItemsSource = await db.Tenants
                .AsNoTracking()
                .OrderBy(t => t.AccountNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка БД", ex.InnerException?.Message ?? ex.Message, "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TenantEditPage(_factory, null));
    }

    private async void OnTenantSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Tenant tenant)
        {
            TenantsView.SelectedItem = null;
            await Navigation.PushAsync(new TenantEditPage(_factory, tenant));
        }
    }
}