## Подключение

```C#
builder.Services.AddMassTransit(options =>
{
    options.UsingRabbitMq((context, cfg) =>
    {
        // Это не обязательно, так как используются дефолтные параметры
        //cfg.Host("rabbitmq://localhost", "/", hostCfg =>
        //{
        //    hostCfg.Username("guest");
        //    hostCfg.Password("guest");
        //});

        cfg.ConfigureEndpoints(context);
    });
});
```

### Наименование

```C#
builder.Services.AddMassTransit(options =>
{
    // Наименование становиться таким OrderCreated -> order-created
    // options.SetKebabCaseEndpointNameFormatter();
    // Кастомный формат с KebabCaseEndpointNameFormatter
    options.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("hellos", true));

    options.UsingRabbitMq((context, cfg) =>
    {
        //cfg.Host("rabbitmq://localhost", "/", hostCfg =>
        //{
        //    hostCfg.Username("guest");
        //    hostCfg.Password("guest");
        //});

        // Если мы знаем определенный queue name, мы можем привязать для него определенного консьюмера
        cfg.ReceiveEndpoint("order-created", endpoint =>
        {
            endpoint.ConfigureConsumer<OrderCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);

        // Тут важен порядок, сначала ReceiveEndpoint, потом ConfigureEndpoints
        // из документации:
        // When configuring endpoints manually, ConfigureEndpoints should be excluded or be called after any explicitly configured receive endpoints.
    });
});
```

### ConsumerDefinition

```C#
public class OrderCreatedConsumerDefinition : ConsumerDefinition<OrderCreatedConsumer>
{
    public OrderCreatedConsumerDefinition()
    {
        EndpointName = "my-named-order";
        ConcurrentMessageLimit = 10;

        //Endpoint(options =>
        //{
        //    options.Name = "my-named-order";
        //    options.ConcurrentMessageLimit = 10;
        //});
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<OrderCreatedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        consumerConfigurator.UseMessageRetry(options =>
        {
            options.Immediate(5);
        });
    }
}

builder.Services.AddMassTransit(options =>
{
    options.AddConsumer<OrderCreatedConsumer, OrderCreatedConsumerDefinition>();
});
```