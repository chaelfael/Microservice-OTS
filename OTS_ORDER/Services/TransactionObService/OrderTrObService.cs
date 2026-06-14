
using Microsoft.EntityFrameworkCore;
using OTS_ORDER.Data;
using OTS_ORDER.Dtos;
using OTS_ORDER.Interface;
using OTS_ORDER.Models;

namespace OTS_ORDER.Services
{
    public class OrderTrObService : IOrderTrObService
    {

        private readonly OrderDbContext _context;

        public OrderTrObService(OrderDbContext context)
        {

            _context = context;
        }

        public async Task<ResOrderByOrderNoObj> GetOrderByOrderNo(string orderNo)
        {
            // FIX 1: Use FirstOrDefaultAsync because OrderNo is not the Primary Key.
            // FIX 2: Use .Include() to do a SQL JOIN, fetching the order and its details in 1 trip!
            Order order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderNo == orderNo);

            if (order == null)
            {
                throw new Exception($"Order with OrderNo {orderNo} not found");
            }

            return new ResOrderByOrderNoObj
            {
                OrderNo = order.OrderNo,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus, // Make sure your DTO includes Status!
                OrderDetails = order.OrderDetails.Select(od => new ResOrderDetailDto
                {
                    OrderDetailId = od.OrderDetailId,
                    TicketNumber = od.TicketNumber
                }).ToList()
            };
        }
    }

}