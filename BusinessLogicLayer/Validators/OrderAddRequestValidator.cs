using BusinessLogicLayer.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Validators
{
    public class OrderAddRequestValidator: AbstractValidator<OrderAddedRequest>
    {
        public OrderAddRequestValidator() 
        {
            RuleFor(x => x.UserID).NotEmpty().WithMessage("UserID is required");

            RuleFor(x => x.OrderDate).NotEmpty().WithMessage("OrderDate is required");

            RuleFor(x => x.OrderItems).NotEmpty().WithMessage("OrderItems is required");
        }
        
    }
}
