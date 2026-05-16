namespace CoreApp.Services;

public interface IDataSeeder
{
    int Order { get; }
    Task SeedAsync();
}