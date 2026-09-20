using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggegate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.PostgeSQL;
using MediatR;
using NSubstitute;
using Xunit;                                        


namespace DeliveryApp.IntegrationTests.Repositories;

public  class CourierRepositoryShould : IntegrationTestBase
{
    

    [Fact]
    public async Task CanAddCourier()
    {
        // Arrange
        var courierRepository = new CourierRepository(DbContext);
        var courier = new Courier(
            "Иванов",
            new Location(1, 2)
        );
        var mediator = Substitute.For<IMediator>();
        mediator.Publish(
            Arg.Any<DomainEvent>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await courierRepository.AddAsync(courier);
        var unitOfWork = new UnitOfWork(DbContext, mediator);
        await unitOfWork.SaveChangesAsync();
        // Assert
        var savedCourier = await courierRepository.GetByIdAsync(courier.Id, CancellationToken.None);
        Assert.NotNull(savedCourier);
        Assert.Equal(courier.Id, savedCourier.Id);
        Assert.Equal(courier.Name, savedCourier.Name);
        Assert.True(courier.Location.IsSameLocation(savedCourier.Location));
        Assert.Equal(courier.MaxVolume.Capatity, savedCourier.MaxVolume.Capatity);
    }

    [Fact]
    public async Task CanAddOrderandUpdate()
    {
        // Arrange
        var courierRepository = new CourierRepository(DbContext);
        var courier = new Courier(
            "Иванов",
            new Location(1, 2)
        );

        var orderrepository = new OrderRepository(DbContext);
        var order = new Order(
            Guid.NewGuid(),
            new Location(1, 2),
            new Volume(10)
        );

        var mediator = Substitute.For<IMediator>();
        mediator.Publish(
            Arg.Any<DomainEvent>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await orderrepository.AddAsync(order);
        courier.AddAssignment(order);
        await courierRepository.AddAsync(courier);

        var unitOfWork = new UnitOfWork(DbContext, mediator);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var savedOrder = await orderrepository.GetByIdAsync(order.Id, CancellationToken.None);
        var savedCourier = await courierRepository.GetByIdAsync(courier.Id, CancellationToken.None);

        Assert.NotNull(savedOrder);
        Assert.NotNull(savedCourier);
        Assert.Equal(courier.Assignments.Count, savedCourier.Assignments.Count);

    }

    [Fact]
    public async Task CanUpdateCourier()
    {
        // Arrange
        var courierRepository = new CourierRepository(DbContext);
        var courier = new Courier(
            "Иванов",
            new Location(1, 2)
        );
        var order = new Order(
            Guid.NewGuid(),
            new Location(1, 2),
            new Volume(10)
        );
        

        await courierRepository.AddAsync(courier);
        var mediator = Substitute.For<IMediator>();
        mediator.Publish(
            Arg.Any<DomainEvent>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var unitOfWork = new UnitOfWork(DbContext, mediator);
        await unitOfWork.SaveChangesAsync();
        // Act
        var orderrepository = new OrderRepository(DbContext);
        await orderrepository.AddAsync(order);
        courier.AddAssignment(order);
        await courierRepository.UpdateAsync(courier);
        await unitOfWork.SaveChangesAsync();
        // Assert
        var updatedCourier = await courierRepository.GetByIdAsync(courier.Id, CancellationToken.None);
        Assert.NotNull(updatedCourier);
        Assert.Equal("Иванов", updatedCourier.Name);
        Assert.Single(updatedCourier.Assignments);
    }

    [Fact]
    public async Task CanGetAllCouriers()
    {
        // Arrange
        var courierRepository = new CourierRepository(DbContext);
        var courier1 = new Courier(
            "Иванов",
            new Location(1, 2)
        );
        var courier2 = new Courier(
            "Петров",
            new Location(3, 4)
        );
        var mediator = Substitute.For<IMediator>();
        mediator.Publish(
            Arg.Any<DomainEvent>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        await courierRepository.AddAsync(courier1);
        await courierRepository.AddAsync(courier2);
        var unitOfWork = new UnitOfWork(DbContext, mediator);
        await unitOfWork.SaveChangesAsync();
        // Act
        var couriers = await courierRepository.GetAllAsync(CancellationToken.None);
        // Assert
        Assert.NotNull(couriers);
        Assert.True(couriers.Any());
    }


}
