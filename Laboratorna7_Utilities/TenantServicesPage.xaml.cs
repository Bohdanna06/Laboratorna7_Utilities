using Laboratorna7_Utilities.Data;
using Laboratorna7_Utilities.Models;
using Microsoft.EntityFrameworkCore;
using Windows.Devices.Enumeration;

namespace Laboratorna7_Utilities;

public class ChargeItem
{
    public int ServiceID { get; set; }
    public string ServiceName { get; set; } = "";
    public decimal Charge { get; set; }
}

public partial class TenantServicesPage : ContentPage
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly Tenant _tenant;

    public TenantServicesPage(IDbContextFactory<AppDbContext> factory, Tenant tenant)
    {
        InitializeComponent();
        _factory = factory;
        _tenant = tenant;
        Title = "Послуги мешканця";
        HeaderLabel.Text = $"{tenant.FullName} ({tenant.AccountNumber})";
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

            var links = await db.TenantServices
                .AsNoTracking()
                .Include(ts => ts.Service)
                .Where(ts => ts.TenantID == _tenant.ID)
                .ToListAsync();

            var items = links.Select(ts => new ChargeItem
            {
                ServiceID = ts.ServiceID,
                ServiceName = ts.Service.ServiceName,
                Charge = ts.Service.RatePerSqMeter != null
                    ? _tenant.Area * ts.Service.RatePerSqMeter.Value
                    : _tenant.OccupantsCount * (ts.Service.RatePerPerson ?? 0)
            }).ToList();

            LinksView.ItemsSource = items;
            TotalLabel.Text = $"Разом: {items.Sum(i => i.Charge):F2} грн";

            var usedIds = items.Select(i => i.ServiceID).ToList();
            ServicePicker.ItemsSource = await db.Services
                .AsNoTracking()
                .Where(s => !usedIds.Contains(s.ID))
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
        if (ServicePicker.SelectedItem is not Service service)
        {
            await DisplayAlert("Увага", "Спочатку оберіть послугу", "OK");
            return;
        }

        try
        {
            using var db = _factory.CreateDbContext();
            db.TenantServices.Add(new TenantService
            {
                TenantID = _tenant.ID,
                ServiceID = service.ID
            });
            await db.SaveChangesAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка збереження", ex.InnerException?.Message ?? ex.Message, "OK");
        }
    }
    private async void OnDoneClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnLinkSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ChargeItem item) return;
        LinksView.SelectedItem = null;

        bool confirm = await DisplayAlert("Видалення",
            $"Прибрати послугу \"{item.ServiceName}\" у цього мешканця?", "Так", "Ні");
        if (!confirm) return;

        try
        {
            using var db = _factory.CreateDbContext();
            var link = await db.TenantServices.FindAsync(_tenant.ID, item.ServiceID);
            if (link != null)
            {
                db.TenantServices.Remove(link);
                await db.SaveChangesAsync();
            }
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка видалення", ex.InnerException?.Message ?? ex.Message, "OK");
        }
    }
    // Ensure that the TenantServicesPage.xaml file exists in the same project and is properly linked to this code-behind file.  
    // The InitializeComponent method is generated automatically during the build process for XAML files.  
    // If the file is missing or not linked correctly, the method will not be available.  

    // Steps to fix:  
    // 1. Verify that the TenantServicesPage.xaml file exists in the same namespace as this code-behind file.  
    // 2. Ensure the XAML file has the correct `x:Class` attribute pointing to the namespace and class name of this code-behind file.  

    // Example of the XAML file:  
    // File: Laboratorna7_Utilities/TenantServicesPage.xaml  
    // Content:  
    // <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"  
    //              xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"  
    //              x:Class="Laboratorna7_Utilities.TenantServicesPage">  
    //     <ContentPage.Content>  
    //         <!-- UI elements go here -->  
    //     </ContentPage.Content>  
    // </ContentPage>  

    // 3. If the XAML file exists and is correctly linked, clean and rebuild the project to regenerate the InitializeComponent method.  
    // 4. If the issue persists, ensure that the build action for the XAML file is set to "MauiXaml" (or "Page" for older frameworks) in the file properties.
}