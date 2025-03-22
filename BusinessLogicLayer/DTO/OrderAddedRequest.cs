using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO
{
    public record OrderAddedRequest(Guid UserID, DateTime OrderDate, List<OrderItemAddRequest> OrderItems)
    {
        public OrderAddedRequest(): this(default, default, default)
        {

        }
    }
}
