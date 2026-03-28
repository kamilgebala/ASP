using CoreApp.Entities;
using CoreApp.Repositories;
using Infrastructure.Memory;

namespace UnitTest;

public class MemoryGenericRepositoryTest
{
    private readonly IGenericRepositoryAsync<Vehicle> _repository = new MemoryGenericRepository<Vehicle>();

    private static Vehicle MakeVehicle(string plate = "TK 8434Y") => new Vehicle
    {
        Id = Guid.NewGuid(),
        LicensePlate = plate,
        Brand = "Toyota",
        Color = "Red"
    };
    
    [Fact]
    public async Task AddAsync_ShouldStoreVehicle()
    {
        var expected = MakeVehicle();

        await _repository.AddAsync(expected);
        var actual = await _repository.FindByIdAsync(expected.Id);

        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.LicensePlate, actual.LicensePlate);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnAddedVehicle()
    {
        var vehicle = MakeVehicle();

        var result = await _repository.AddAsync(vehicle);

        Assert.Equal(vehicle, result);
    }
    
    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.FindByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnVehicle_WhenExists()
    {
        var vehicle = MakeVehicle();
        await _repository.AddAsync(vehicle);

        var result = await _repository.FindByIdAsync(vehicle.Id);

        Assert.NotNull(result);
        Assert.Equal(vehicle.Id, result.Id);
    }
    
    [Fact]
    public async Task FindAllAsync_ShouldReturnAllVehicles()
    {
        await _repository.AddAsync(MakeVehicle("TK 0001A"));
        await _repository.AddAsync(MakeVehicle("TK 0002B"));
        await _repository.AddAsync(MakeVehicle("TK 0003C"));

        var result = await _repository.FindAllAsync();

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task FindAllAsync_ShouldReturnEmpty_WhenRepositoryIsEmpty()
    {
        var result = await _repository.FindAllAsync();

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task FindPagedAsync_ShouldReturnCorrectPage()
    {
        for (int i = 1; i <= 5; i++)
            await _repository.AddAsync(MakeVehicle($"TK {i:D4}"));

        var result = await _repository.FindPagedAsync(page: 2, pageSize: 2);

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
        for (int i = 1; i <= 4; i++)
            await _repository.AddAsync(MakeVehicle($"TK {i:D4}"));

        var result = await _repository.FindPagedAsync(page: 2, pageSize: 2);

        Assert.Equal(2, result.Items.Count);
        Assert.False(result.HasNext);
        Assert.True(result.HasPrevious);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingVehicle()
    {
        var vehicle = MakeVehicle();
        await _repository.AddAsync(vehicle);

        vehicle.Color = "Blue";
        await _repository.UpdateAsync(vehicle);

        var updated = await _repository.FindByIdAsync(vehicle.Id);
        Assert.Equal("Blue", updated?.Color);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenVehicleNotFound()
    {
        var vehicle = MakeVehicle();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(vehicle));
    }
    
    [Fact]
    public async Task RemoveByIdAsync_ShouldDeleteVehicle()
    {
        var vehicle = MakeVehicle();
        await _repository.AddAsync(vehicle);

        await _repository.RemoveByIdAsync(vehicle.Id);
        var result = await _repository.FindByIdAsync(vehicle.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveByIdAsync_ShouldThrow_WhenVehicleNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.RemoveByIdAsync(Guid.NewGuid()));
    }
}
