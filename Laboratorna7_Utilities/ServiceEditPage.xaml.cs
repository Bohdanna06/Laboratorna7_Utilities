using Laboratorna7_Utilities.Data;
using Laboratorna7_Utilities.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Laboratorna7_Utilities;

public partial class ServiceEditPage : ContentPage
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly Service? _service;

    public ServiceEditPage(IDbContextFactory<AppDbContext> factory, Service? service)
    {
        InitializeComponent();
        _factory = factory;
        _service = service;
        // Ensure that the XAML file associated with this code-behind file has the correct class name and namespace.  
        // The XAML file should have the root element like this:  
        // <ContentPage x:Class="Laboratorna7_Utilities.ServiceEditPage" ... >

        // If the XAML file is missing or incorrectly named, create or rename it to match the class name and namespace.  
        // Example XAML file: Laboratorna7_Utilities/ServiceEditPage.xaml
        if (service == null)
        {
            Title = "Нова послуга";
            DeleteButton.IsVisible = false;
        }
        else
        {
            Title = "Редагування послуги";
            NameEntry.Text = service.ServiceName;
            SqMeterEntry.Text = service.RatePerSqMeter?.ToString(CultureInfo.CurrentCulture);
            PersonEntry.Text = service.RatePerPerson?.ToString(CultureInfo.CurrentCulture);
        }
    }

    private static bool TryParseRate(string? text, out decimal? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text)) return true;
        if (decimal.TryParse(text.Replace('.', ','), out decimal d) && d >= 0)
        {
            value = d;
            return true;
        }
        return false;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            !TryParseRate(SqMeterEntry.Text, out decimal? perSqMeter) ||
            !TryParseRate(PersonEntry.Text, out decimal? perPerson))
        {
            await DisplayAlert("Помилка", "Перевірте правильність заповнення полів", "OK");
            return;
        }

        if (perSqMeter == null && perPerson == null)
        {
            await DisplayAlert("Помилка", "Вкажіть хоча б один тариф", "OK");
            return;
        }

        try
        {
            using var db = _factory.CreateDbContext();

            Service? entity = _service == null
                ? new Service()
                : await db.Services.FindAsync(_service.ID);

            if (entity == null) return;

            entity.ServiceName = NameEntry.Text.Trim();
            entity.RatePerSqMeter = perSqMeter;
            entity.RatePerPerson = perPerson;

            if (_service == null)
                db.Services.Add(entity);

            await db.SaveChangesAsync();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка збереження", ex.InnerException?.Message ?? ex.Message, "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_service == null) return;

        bool confirm = await DisplayAlert("Підтвердження", "Видалити цю послугу?", "Так", "Ні");
        if (!confirm) return;

        try
        {
            using var db = _factory.CreateDbContext();

            db.TenantServices.RemoveRange(db.TenantServices.Where(ts => ts.ServiceID == _service.ID));

            var entity = await db.Services.FindAsync(_service.ID);
            if (entity != null)
                db.Services.Remove(entity);

            await db.SaveChangesAsync();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка видалення", ex.InnerException?.Message ?? ex.Message, "OK");
        }
    }
}