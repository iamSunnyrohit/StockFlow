using Moq;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Suppliers;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Services;
using StockFlow.Domain.Entities;

namespace StockFlow.Api.Tests.Suppliers;

public class SupplierServiceTests
{
    private readonly Mock<ISupplierRepository> _repository = new();

    private SupplierService Service => new(_repository.Object);

    private void NoSupplierNamed(string name) =>
        _repository
            .Setup(x => x.GetByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

    [Fact]
    public async Task GetAll_NormalizesParameters_BeforeQueryingRepository()
    {
        SupplierQueryParameters? captured = null;

        _repository
            .Setup(x => x.GetAllAsync(
                It.IsAny<SupplierQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .Callback<SupplierQueryParameters, CancellationToken>(
                (p, _) => captured = p)
            .ReturnsAsync(new List<Supplier>());

        _repository
            .Setup(x => x.CountAsync(
                It.IsAny<SupplierQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await Service.GetAllAsync(new SupplierQueryParameters
        {
            Search = "  acme  ",
            Page = 0,
            PageSize = 500
        });

        Assert.NotNull(captured);
        Assert.Equal("acme", captured!.Search);
        Assert.Equal(1, captured.Page);
        Assert.Equal(100, captured.PageSize);
        Assert.Equal(1, result.Page);
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task GetAll_ReturnsMappedItemsAndTotalCount()
    {
        _repository
            .Setup(x => x.GetAllAsync(
                It.IsAny<SupplierQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Supplier>
            {
                new() { Id = Guid.NewGuid(), Name = "Acme", City = "Pune" }
            });

        _repository
            .Setup(x => x.CountAsync(
                It.IsAny<SupplierQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        var result = await Service.GetAllAsync(new SupplierQueryParameters());

        var item = Assert.Single(result.Items);
        Assert.Equal("Acme", item.Name);
        Assert.Equal("Pune", item.City);
        Assert.Equal(7, result.TotalCount);
    }

    [Fact]
    public async Task Create_WhenNameIsFree_TrimsAndSaves()
    {
        NoSupplierNamed("Acme Traders");

        var result = await Service.CreateAsync(new CreateSupplierRequest
        {
            Name = "  Acme Traders  ",
            Email = "priya@acme.example",
            Phone = "   "
        });

        Assert.Equal("Acme Traders", result.Name);
        Assert.Null(result.Phone);
        Assert.True(result.IsActive);

        _repository.Verify(x => x.AddAsync(
            It.IsAny<Supplier>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenNameExists_ThrowsConflictAndSavesNothing()
    {
        _repository
            .Setup(x => x.GetByNameAsync(
                "Acme Traders", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Supplier { Id = Guid.NewGuid(), Name = "Acme Traders" });

        await Assert.ThrowsAsync<ConflictException>(() =>
            Service.CreateAsync(new CreateSupplierRequest { Name = "Acme Traders" }));

        _repository.Verify(x => x.AddAsync(
            It.IsAny<Supplier>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_WhenNameBlank_ThrowsValidation(string name)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Service.CreateAsync(new CreateSupplierRequest { Name = name }));
    }

    [Fact]
    public async Task Create_WhenNameTooLong_ThrowsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Service.CreateAsync(new CreateSupplierRequest
            {
                Name = new string('x', 151)
            }));
    }

    [Fact]
    public async Task Create_WhenEmailInvalid_ThrowsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Service.CreateAsync(new CreateSupplierRequest
            {
                Name = "Acme",
                Email = "not-an-email"
            }));
    }

    [Fact]
    public async Task Update_WhenSupplierMissing_ReturnsNull()
    {
        _repository
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

        var result = await Service.UpdateAsync(
            Guid.NewGuid(), new UpdateSupplierRequest { Name = "Acme" });

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_WhenNameBelongsToAnotherSupplier_ThrowsConflict()
    {
        var id = Guid.NewGuid();

        _repository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Supplier { Id = id, Name = "Old" });

        _repository
            .Setup(x => x.GetByNameAsync("Taken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Supplier { Id = Guid.NewGuid(), Name = "Taken" });

        await Assert.ThrowsAsync<ConflictException>(() =>
            Service.UpdateAsync(id, new UpdateSupplierRequest { Name = "Taken" }));

        _repository.Verify(x => x.UpdateAsync(
            It.IsAny<Supplier>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenKeepingOwnName_Succeeds()
    {
        var id = Guid.NewGuid();
        var supplier = new Supplier { Id = id, Name = "Acme" };

        _repository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        _repository
            .Setup(x => x.GetByNameAsync("Acme", It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        var result = await Service.UpdateAsync(
            id, new UpdateSupplierRequest { Name = "Acme", City = "Pune" });

        Assert.NotNull(result);
        Assert.Equal("Pune", result!.City);
        Assert.NotNull(supplier.UpdatedAtUtc);
    }

    [Fact]
    public async Task Deactivate_SetsSupplierInactive()
    {
        var id = Guid.NewGuid();
        var supplier = new Supplier { Id = id, Name = "Acme", IsActive = true };

        _repository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        var result = await Service.DeactivateAsync(id);

        Assert.True(result);
        Assert.False(supplier.IsActive);
    }

    [Fact]
    public async Task Deactivate_WhenSupplierMissing_ReturnsFalse()
    {
        _repository
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

        Assert.False(await Service.DeactivateAsync(Guid.NewGuid()));
    }
}