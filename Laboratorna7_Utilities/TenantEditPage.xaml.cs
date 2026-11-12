using Laboratorna7_Utilities.Data;
using Laboratorna7_Utilities.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Laboratorna7_Utilities;

public partial class TenantEditPage : ContentPage
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly Tenant? _tenant;

    public TenantEditPage(IDbContextFactory<AppDbContext> factory, Tenant? tenant)
    {
        InitializeComponent();
        _factory = factory;
        _tenant = tenant;

        if (tenant == null)
        {
           
            Title = "Новий мешканець";
            DeleteButton.IsVisible = false;
            ServicesButton.IsVisible = false;
        }
        else
        {
            Title = "Редагування";
            AccountEntry.Text = tenant.AccountNumber;
            NameEntry.Text = tenant.FullName;
            AddressEntry.Text = tenant.Address;
            OccupantsEntry.Text = tenant.OccupantsCount.ToString();
            AreaEntry.Text = tenant.Area.ToString(CultureInfo.CurrentCulture);
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AccountEntry.Text) ||
            string.IsNullOrWhiteSpace(NameEntry.Text) ||
            !int.TryParse(OccupantsEntry.Text, out int occupants) || occupants < 0 ||
            !decimal.TryParse(AreaEntry.Text?.Replace('.', ','), out decimal area) || area <= 0)
        {
            await DisplayAlert("Помилка", "Перевірте правильність заповнення полів", "OK");
            return;
        }

        try
        {
            using var db = _factory.CreateDbContext();

            Tenant? entity;
            if (_tenant == null)
                entity = new Tenant();
            else
                entity = await db.Tenants.FindAsync(_tenant.ID);

            if (entity == null) return;

            entity.AccountNumber = AccountEntry.Text.Trim();
            entity.FullName = NameEntry.Text.Trim();
            entity.Address = AddressEntry.Text?.Trim() ?? "";
            entity.OccupantsCount = occupants;
            entity.Area = area;

            if (_tenant == null)
                db.Tenants.Add(entity);

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
        if (_tenant == null) return;

        bool confirm = await DisplayAlert("Підтвердження", "Видалити цього мешканця?", "Так", "Ні");
        if (!confirm) return;

        try
        {
            using var db = _factory.CreateDbContext();

            // спочатку видаляємо пов'язані послуги (зовнішній ключ)
            var links = db.TenantServices.Where(ts => ts.TenantID == _tenant.ID);
            db.TenantServices.RemoveRange(links);

            var entity = await db.Tenants.FindAsync(_tenant.ID);
            if (entity != null)
                db.Tenants.Remove(entity);

            await db.SaveChangesAsync();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка видалення", ex.InnerException?.Message ?? ex.Message, "OK");
        }
    }
    private async void OnServicesClicked(object sender, EventArgs e)
    {
        if (_tenant == null) return;
        await Navigation.PushAsync(new TenantServicesPage(_factory, _tenant));
    }
}