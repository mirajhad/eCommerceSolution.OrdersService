using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;


namespace BusinessLogicLayer.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IValidator<OrderAddedRequest> _orderAddRequestValidator;
        private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
        private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
        private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
        private readonly IMapper _mapper;
        private readonly IOrdersRepository _ordersRepository;
        public OrdersService(IOrdersRepository ordersRepository, IMapper mapper, 
            IValidator<OrderAddedRequest> orderAddRequestValidator,
            IValidator<OrderItemAddRequest> orderItemAddRequestValidator,
            IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
            IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator
            ) 
        {
            _orderAddRequestValidator = orderAddRequestValidator;
            _orderItemAddRequestValidator = orderItemAddRequestValidator;
            _orderUpdateRequestValidator = orderUpdateRequestValidator;
            _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
            _mapper = mapper;
            _ordersRepository = ordersRepository;
        }
        public async Task<OrderResponse?> AddOrder(OrderAddedRequest orderAddedRequest)
        {
            if (orderAddedRequest == null)
            {
                throw new ArgumentNullException(nameof(orderAddedRequest));
            }
            ValidationResult orderAddRequestValidationResult = await _orderAddRequestValidator.ValidateAsync(orderAddedRequest);

            if(!orderAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errors);
            }

            foreach(OrderItemAddRequest orderItemAddRequest in orderAddedRequest.OrderItems)
            {
                ValidationResult orderItemAddRequestValidationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);
                if (!orderItemAddRequestValidationResult.IsValid)
                {
                    string errors = string.Join(", ", orderItemAddRequestValidationResult.Errors.Select(e => e.ErrorMessage));
                    throw new ValidationException(errors);
                }
            }

            Order orderInput = _mapper.Map<Order>(orderAddedRequest);

            foreach(OrderItem orderItem in orderInput.OrderItems)
            {
                orderItem.TotalPrice = (decimal)(orderItem.Quantity * orderItem.Price);
            }
            orderInput.TotalBill = orderInput.OrderItems.Sum(oi => oi.TotalPrice);

            Order? addedOrder = await _ordersRepository.AddOrder(orderInput);

            if(addedOrder == null)
            {
                return null;
            }

            OrderResponse addedOrderResponse = _mapper.Map<OrderResponse>(addedOrder);
            return addedOrderResponse;

        }

        public async Task<bool> DeleteOrder(Guid orderID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(o => o.OrderID, orderID);
            Order? existingOrder = (Order?)await _ordersRepository.GetOrderByCondition(filter);

            if (existingOrder == null)
            {
                return false;
            }

            bool isDeleted = await _ordersRepository.DeleteOrder(orderID);
            return isDeleted;
        }

        public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            Order? order = (Order?)await _ordersRepository.GetOrderByCondition(filter);
            if (order == null)
            {
                return null;
            }
            OrderResponse orderResponse = _mapper.Map<OrderResponse>(order);
            return orderResponse;
        }

        public async Task<List<OrderResponse?>> GetOrders()
        {
            IEnumerable<Order?> orders = await _ordersRepository.GetOrders();

            IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);
            return [.. orderResponses];
        }

        public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
        {
            IEnumerable<Order?> orders = await _ordersRepository.GetOrdersByCondition(filter);

            IEnumerable<OrderResponse> orderResponse = _mapper.Map<IEnumerable<OrderResponse>>(orders);
            return [.. orderResponse];
        }

        public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
        {
            if (orderUpdateRequest == null)
            {
                throw new ArgumentNullException(nameof(orderUpdateRequest));
            }
            ValidationResult orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);

            if (!orderUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errors);
            }

            foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
            {
                ValidationResult orderItemUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
                if (!orderItemUpdateRequestValidationResult.IsValid)
                {
                    string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(e => e.ErrorMessage));
                    throw new ValidationException(errors);
                }
            }

            Order orderInput = _mapper.Map<Order>(orderUpdateRequest);

            foreach (OrderItem orderItem in orderInput.OrderItems)
            {
                orderItem.TotalPrice = (decimal)(orderItem.Quantity * orderItem.Price);
            }
            orderInput.TotalBill = orderInput.OrderItems.Sum(oi => oi.TotalPrice);

            Order? updatedOrder = await _ordersRepository.UpdateOrder(orderInput);

            if (updatedOrder == null)
            {
                return null;
            }

            OrderResponse addedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder);
            return addedOrderResponse;
        }
    }
}
