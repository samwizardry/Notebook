### Пример публикации события

```C#
private readonly IPublishEndpoint _publishEndpoint;

var notifyOrderCreated = _publishEndpoint.Publish(new OrderCreated()
{
    Id = createdOrder.Id,
    OrderId = createdOrder.OrderId,
    CreatedAt = createdOrder.OrderDate,
    TotalAmount = createdOrder.OrderItems.Sum(x => x.Price * x.Quantity)
});
```
