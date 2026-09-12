using Ddd;
using DeliveryApp.Core.Domain.Model.OrderAggegate;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrderCommand;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeoClient _geoClient;
    public CreateOrderCommandHandler(IOrderRepository orderRepository, 
        IUnitOfWork unitOfWork,
        IGeoClient geoClient)
    {
    
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _geoClient = geoClient ?? throw new ArgumentNullException(nameof(geoClient));
    }
    public async Task<CreateResponse> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // Random Location
        var location = await _geoClient.GetLocationAsync(command.Street, cancellationToken);

        var order = new Order(command.OrderId, location, command.Volume);
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        return new CreateResponse(order.Id);
    }
}
