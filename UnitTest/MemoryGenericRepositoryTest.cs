using CoreApp.Entities;
using CoreApp.Repositories;
using Infrastructure.Memory;

namespace UnitTest;

public class MemoryGenericRepositoryTest
{
    private IGenericRepositoryAsync<Vehicle> CreateRepo() => new MemoryGenericRepository<Vehicle>();

    private static Vehicle MakeVehicle(string plate = "TK 8434Y") => new Vehicle
    {
        Id = Guid.NewGuid(),
        LicensePlate = plate,
        Brand = "Toyota",
        Color = "Red"
    };

    // --- AddAsync ---

    [Fact]
    public async Task AddAsync_ShouldStoreVehicle()
    {
        var repo = CreateRepo();
        var expected = MakeVehicle();

        await repo.AddAsync(expected);
        var actual = await repo.FindByIdAsync(expected.Id);

        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.LicensePlate, actual.LicensePlate);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnAddedVehicle()
    {
        var repo = CreateRepo();
        var vehicle = MakeVehicle();

        var result = await repo.AddAsync(vehicle);

        Assert.Equal(vehicle, result);
    }

    // --- FindByIdAsync ---

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var repo = CreateRepo();

        var result = await repo.FindByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnVehicle_WhenExists()
    {
        var repo = CreateRepo();
        var vehicle = MakeVehicle();
        await repo.AddAsync(vehicle);

        var result = await repo.FindByIdAsync(vehicle.Id);

        Assert.NotNull(result);
        Assert.Equal(vehicle.Id, result.Id);
    }

    // --- FindAllAsync ---

    [Fact]
    public async Task FindAllAsync_ShouldReturnAllVehicles()
    {
        var repo = CreateRepo();
        await repo.AddAsync(MakeVehicle("TK 0001A"));
        await repo.AddAsync(MakeVehicle("TK 0002B"));
        await repo.AddAsync(MakeVehicle("TK 0003C"));

        var result = await repo.FindAllAsync();

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task FindAllAsync_ShouldReturnEmpty_WhenRepositoryIsEmpty()
    {
        var repo = CreateRepo();

        var result = await repo.FindAllAsync();

        Assert.Empty(result);
    }

    // --- FindPagedAsync ---

    [Fact]
    public async Task FindPagedAsync_ShouldReturnCorrectPage()
    {
        var repo = CreateRepo();
        for (int i = 1; i <= 5; i++)
            await repo.AddAsync(MakeVehicle($"TK {i:D4}"));

        var result = await repo.FindPagedAsync(page: 2, pageSize: 2);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPrevious);
        Assert.True(result.HasNext);
    }

    [Fact]
    public async Task FindPagedAsync_LastPage_ShouldHaveNoNext()
    {
        var repo = CreateRepo();
        for (int i = 1; i <= 4; i++)
            await repo.AddAsync(MakeVehicle($"TK {i:D4}"));

        var result = await repo.FindPagedAsync(page: 2, pageSize: 2);

        Assert.Equal(2, result.Items.Count);
        Assert.False(result.HasNext);
        Assert.True(result.HasPrevious);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingVehicle()
    {
        var repo = CreateRepo();
        var vehicle = MakeVehicle();
        await repo.AddAsync(vehicle);

        vehicle.Color = "Blue";
        await repo.UpdateAsync(vehicle);

        var updated = await repo.FindByIdAsync(vehicle.Id);
        Assert.Equal("Blue", updated?.Color);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenVehicleNotFound()
    {
        var repo = CreateRepo();
        var vehicle = MakeVehicle();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateAsync(vehicle));
    }

    // --- RemoveByIdAsync ---

    [Fact]
    public async Task RemoveByIdAsync_ShouldDeleteVehicle()
    {
        var repo = CreateRepo();
        var vehicle = MakeVehicle();
        await repo.AddAsync(vehicle);

        await repo.RemoveByIdAsync(vehicle.Id);
        var result = await repo.FindByIdAsync(vehicle.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveByIdAsync_ShouldThrow_WhenVehicleNotFound()
    {
        var repo = CreateRepo();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.RemoveByIdAsync(Guid.NewGuid()));
    }
}
