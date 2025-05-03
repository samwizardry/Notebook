### Пример обработки событий

```C#
public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        Console.WriteLine($"I just consumed message with Order ID {context.Message.OrderId}, that was created at {context.Message.CreatedAt}");
    }
}

builder.Services.AddMassTransit(options =>
{
    // Добавить конкретного консьюмера
    options.AddConsumer<OrderCreatedConsumer>();
    // Добавить всех консьюмеров в домене
    // options.AddConsumers(Assembly.GetExecutingAssembly());
});
```