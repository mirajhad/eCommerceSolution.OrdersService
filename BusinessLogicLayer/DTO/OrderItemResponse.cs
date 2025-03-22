using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO
{
    public record OrderItemResponse(Guid ProductID, decimal UnitPrice, int Quantity, decimal TotalPrice)
    {
        public OrderItemResponse() : this(default, default, default, default)
        {
        }
    }
}
