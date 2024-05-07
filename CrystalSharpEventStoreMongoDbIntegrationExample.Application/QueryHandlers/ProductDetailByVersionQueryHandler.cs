﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Application;
using CrystalSharp.Application.Handlers;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions;
using CrystalSharpEventStoreMongoDbIntegrationExample.Application.Domain.Aggregates.ProductAggregate;
using CrystalSharpEventStoreMongoDbIntegrationExample.Application.Queries;
using CrystalSharpEventStoreMongoDbIntegrationExample.Application.ReadModels;

namespace CrystalSharpEventStoreMongoDbIntegrationExample.Application.QueryHandlers
{
    public class ProductDetailByVersionQueryHandler : QueryHandler<ProductDetailByVersionQuery, ProductReadModel>
    {
        private readonly IAggregateEventStore<string> _eventStore;

        public ProductDetailByVersionQueryHandler(IAggregateEventStore<string> eventStore)
        {
            _eventStore = eventStore;
        }

        public override async Task<QueryExecutionResult<ProductReadModel>> Handle(ProductDetailByVersionQuery request, CancellationToken cancellationToken = default)
        {
            if (request == null) return await Fail("Invalid query.");

            ProductReadModel readModel = null;

            try
            {
                Product product = await _eventStore.GetByVersion<Product>(request.GlobalUId, request.Version, cancellationToken).ConfigureAwait(false);

                if (product == null)
                {
                    return await Fail("Product not found.");
                }

                readModel = new()
                {
                    GlobalUId = product.GlobalUId,
                    Name = product.Name,
                    Sku = product.ProductInfo?.Sku,
                    Price = product.ProductInfo?.Price,
                    Version = product.Version
                };
            }
            catch (EventStoreInvalidVersionException ex)
            {
                return await Fail($"Invalid product version {ex.Version}");
            }
            catch (EventStoreNegativeVersionException ex)
            {
                return await Fail($"Negative product version {ex.Version}");
            }
            catch (Exception ex)
            {
                return await Fail(ex.Message);
            }

            return await Ok(readModel);
        }
    }
}
