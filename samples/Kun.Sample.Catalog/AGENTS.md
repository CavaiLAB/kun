# Kun.Sample.Catalog — Agent Guide

## Architecture wiring

```
HTTP POST /products
  → MapCommand (Kun.AspNetCore)
  → ISender.Send(CreateProductCommand)
  → ValidationPipelineBehavior (Kun.Validation)
  → UnitOfWorkPipelineBehavior (Kun.EntityFrameworkCore)
  → CreateProductCommandHandler
      → Product.Create()  [raises ProductCreatedDomainEvent]
      → db.Products.Add(product)
      → IUnitOfWork.SaveChangesAsync()
          → dispatches ProductCreatedDomainEvent via MediatorDomainEventDispatcher (same scope, before commit)
              → ProductCreatedHandler.Handle()
                  → IIntegrationEventBus.Publish(ProductPublishedIntegrationEvent)
                     (staged in the current IntegrationEventBuffer)
          → drains buffer and commits product + OutboxMessages together
```
