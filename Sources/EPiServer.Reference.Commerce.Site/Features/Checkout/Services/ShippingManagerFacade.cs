using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class ShippingManagerFacade
    {
        public const string PickupShippingMethodName = ShippingManager.PickupShippingMethodName;
        private ServiceCollectionAccessor<IShippingPlugin> _shippingPluginsAccessor;
        private ServiceCollectionAccessor<IShippingGateway> _shippingGatewaysAccessor;

        public ShippingManagerFacade(ServiceCollectionAccessor<IShippingPlugin> shippingPluginsAccessor, ServiceCollectionAccessor<IShippingGateway> shippingGatewaysAccessor)
        {
            _shippingPluginsAccessor = shippingPluginsAccessor;
            _shippingGatewaysAccessor = shippingGatewaysAccessor;
        }

        public virtual IList<ShippingMethodInfoModel> GetShippingMethodsByMarket(string marketid, bool returnInactive)
        {
            var methods = ShippingManager.GetShippingMethodsByMarket(marketid, returnInactive);
            return methods.ShippingMethod.Select(method => new ShippingMethodInfoModel
            {
                MethodId = method.ShippingMethodId,
                Currency = method.Currency,
                LanguageId = method.LanguageId,
                Ordering = method.Ordering,
                Name = method.DisplayName,
                ClassName = methods.ShippingOption.FindByShippingOptionId(method.ShippingOptionId).ClassName
            }).ToList();
        }

        public virtual ShippingMethodInfoModel GetInstorePickupModel()
        {
            var methodDto = ShippingManager.GetShippingMethods(null);
            if (methodDto == null || !methodDto.ShippingMethod.Any())
            {
                return null;
            }

            var method = methodDto.ShippingMethod.FirstOrDefault(x => x.Name.Equals(ShippingManager.PickupShippingMethodName));
            if (method == null)
            {
                return null;
            }

            return new ShippingMethodInfoModel
            {
                MethodId = method.ShippingMethodId,
                Currency = method.Currency,
                LanguageId = method.LanguageId,
                Ordering = method.Ordering,
                ClassName = methodDto.ShippingOption.FindByShippingOptionId(method.ShippingOptionId).ClassName
            };
        }

        public virtual ShippingRate GetRate(IShipment shipment, ShippingMethodInfoModel shippingMethodInfoModel, IMarket currentMarket)
        {
            var type = Type.GetType(shippingMethodInfoModel.ClassName);
            if (type == null)
            {
                throw new TypeInitializationException(shippingMethodInfoModel.ClassName, null);
            }

            string message = null;

            var shippingPlugin = _shippingPluginsAccessor().FirstOrDefault(s => s.GetType() == type);
            if (shippingPlugin != null)
            {
                return shippingPlugin.GetRate(currentMarket, shippingMethodInfoModel.MethodId, shipment, ref message);
            }
            var shippingGateway = _shippingGatewaysAccessor().FirstOrDefault(s => s.GetType() == type);
            if (shippingGateway != null)
            {
                return shippingGateway.GetRate(currentMarket, shippingMethodInfoModel.MethodId, (Shipment)shipment, ref message);
            }
            throw new InvalidOperationException($"There is no registered {nameof(IShippingPlugin)} or {nameof(IShippingGateway)} instance.");
        }
    }
}
