using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Entities.OrderAggregate;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class OrderController : BaseApiController
    {
        private readonly StoreContext _context;
        public OrderController(StoreContext context)
        {
            _context = context;

        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetOrders()
        {
            return await _context.Orders
                .ProjectOrderToOrderDto()
                .Where(x => x.CustomerId == User.Identity.Name)
                .ToListAsync();
        }

        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            return await _context.Orders
            .ProjectOrderToOrderDto()
            .Where(x => x.CustomerId == User.Identity.Name && x.Id == id)
            .FirstOrDefaultAsync();
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateOrder(CreateOrderDto orderDto)
        {
            var basket = await _context.Baskets
                .RetrieveBasketWithItems(User.Identity.Name)
                .FirstOrDefaultAsync();

            //si no hay carrito disponible
            if (basket == null) return BadRequest(new ProblemDetails { Title = "Could not locate basket" });

            var items = new List<OrderItem>();

            foreach (var item in basket.Items)
            {
                var productItem = await _context.Products.FindAsync(item.ProductId);
                var itemOrdered = new ProductItemOrdered
                {
                    ProductId = productItem.Id,
                    Name = productItem.Name,
                    PictureUrl = productItem.PictureUrl
                };

                var orderItem = new OrderItem
                {
                    ItemOrdered = itemOrdered,
                    Price = productItem.Price,
                    Quantity = item.Quantity
                };
                items.Add(orderItem);
                productItem.QuantityInStock -= item.Quantity;
            }

            //precio subtotal
            var subtotal = items.Sum(item => item.Price * item.Quantity);
            var deliveryFee = subtotal > 20000 ? 0 : 500;

            var order = new Order
            {
                OrderItems = items,
                CustomerId = User.Identity.Name,
                ShippingAddress = orderDto.ShippingAddress,
                Subtotal = subtotal,
                DeliveryFee = deliveryFee
            };
            //se agrega la orden
            _context.Orders.Add(order);
            _context.Baskets.Remove(basket);

        
            //checar si el usuario ha guardado la direccion
            if (orderDto.SaveAddress)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
                user.Address = new UserAddress
                {
                    FullName = orderDto.ShippingAddress.FullName,
                    Address1 = orderDto.ShippingAddress.Address1,
                    Address2 = orderDto.ShippingAddress.Address2,
                    City = orderDto.ShippingAddress.City,
                    State = orderDto.ShippingAddress.State,
                    Zip = orderDto.ShippingAddress.Zip,
                    Country = orderDto.ShippingAddress.Country,
                };

                //actualizar el usuario
                _context.Update(user);
            }

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return CreatedAtRoute("GetOrder", new { id = order.Id }, order.Id);
            
            return BadRequest("Problem creating order");
        }

    }
}