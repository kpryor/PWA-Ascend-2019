﻿using System;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels;
using EPiServer.ServiceLocation;
using EPiServer.Tracking.Commerce.Data;
using EPiServer.Tracking.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Tracking.Commerce;
using Mediachase.Commerce.Catalog;
using EPiServer.Personalization.CMS.Model;
using PersonalizationRecommendation = EPiServer.Personalization.CMS.Recommendation;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.Services
{
    [ServiceConfiguration(typeof(IRecommendationService), Lifecycle = ServiceInstanceScope.Transient)]
    public class RecommendationService : IRecommendationService
    {
        private readonly ServiceAccessor<IContentRouteHelper> _contentRouteHelperAccessor;
        private readonly IContextModeResolver _contextModeResolver;
        private readonly IProductService _productService;
        private readonly CartTrackingDataFactory _trackingDataFactory;
        private readonly ITrackingService _trackingService;
        private readonly IContentLoader _contentLoader;
        private readonly LanguageService _languageService;
        private readonly HttpContextBase _httpContextBase;
        private readonly LanguageResolver _languageResolver;
        private readonly ILineItemCalculator _lineItemCalculator;
        private readonly IRequestTrackingDataService _requestTrackingDataService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IRelationRepository _relationRepository;
        private readonly PersonalizationRecommendation.IRecommendationService _personalizationRecommendationService;

        public RecommendationService(
            ServiceAccessor<IContentRouteHelper> contentRouteHelperAccessor,
            IContextModeResolver contextModeResolver,
            IProductService productService,
            CartTrackingDataFactory trackingDataFactory,
            ITrackingService trackingService,
            IContentLoader contentLoader,
            LanguageService languageService,
            HttpContextBase httpContextBase,
            LanguageResolver languageResolver,
            ILineItemCalculator lineItemCalculator,
            IRequestTrackingDataService requestTrackingDataService,
            ReferenceConverter referenceConverter,
            IRelationRepository relationRepository,
            PersonalizationRecommendation.IRecommendationService personalizationRecommendationService)
        {
            _contentRouteHelperAccessor = contentRouteHelperAccessor;
            _contextModeResolver = contextModeResolver;
            _productService = productService;
            _trackingDataFactory = trackingDataFactory;
            _trackingService = trackingService;
            _contentLoader = contentLoader;
            _languageService = languageService;
            _httpContextBase = httpContextBase;
            _languageResolver = languageResolver;
            _lineItemCalculator = lineItemCalculator;
            _requestTrackingDataService = requestTrackingDataService;
            _referenceConverter = referenceConverter;
            _relationRepository = relationRepository;
            _personalizationRecommendationService = personalizationRecommendationService;
        }

        public async Task<TrackingResponseData> TrackProduct(HttpContextBase httpContext, string productCode, bool skipRecommendations)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateProductTrackingData(productCode, httpContext);

            if (skipRecommendations)
            {
                trackingData.SkipRecommendations();
            }

            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackSearch(HttpContextBase httpContext, string searchTerm, int pageSize, IEnumerable<string> productCodes)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default || string.IsNullOrWhiteSpace(searchTerm))
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateSearchTrackingData(searchTerm, productCodes, pageSize, httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackOrder(HttpContextBase httpContext, IPurchaseOrder order)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateOrderTrackingData(order, httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackCategory(HttpContextBase httpContext, NodeContent category)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateCategoryTrackingData(category, httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackCart(HttpContextBase httpContext, ICart cart)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }
            if (cart != null)
            {
                var items = GetCartDataItems(cart);
                var trackingData = new CartTrackingData(
                    items,
                    cart.Currency.CurrencyCode,
                    _languageResolver.GetPreferredCulture().Name,
                    GetRequestData(httpContext),
                    GetCommerceUserData(httpContext));
                return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
            }
            return null;
        }

        public async Task<TrackingResponseData> TrackWishlist(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateWishListTrackingData(httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackCheckout(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateCheckoutTrackingData(httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackHome(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateHomeTrackingData(_httpContextBase);
            return await _trackingService.TrackAsync(trackingData, _httpContextBase, _contentRouteHelperAccessor().Content).ConfigureAwait(false);
        }

        public async Task<TrackingResponseData> TrackBrand(HttpContextBase httpContext, string brandName)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateBrandTrackingData(brandName, httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackAttribute(HttpContextBase httpContext, string attributeName, string attributeValue)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateAttributeTrackingData(attributeName, attributeValue, httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackDefault(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateOtherTrackingData(httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public IEnumerable<RecommendedProductTileViewModel> GetRecommendedProductTileViewModels(IEnumerable<Recommendation> recommendations)
        {
            try
            {
                var returnValue = new List<RecommendedProductTileViewModel>();
                var language = _languageService.GetCurrentLanguage();

                foreach (var recommendation in recommendations)
                {
                    try
                    {
                        returnValue.Add(
                            new RecommendedProductTileViewModel(
                                recommendation.RecommendationId,
                                _productService.GetProductTileViewModel(
                                    _contentLoader.Get<EntryContentBase>(recommendation.ContentLink, language))));
                    }
                    catch { }
                }

                return returnValue;
            }
            catch
            {
                return new List<RecommendedProductTileViewModel>();
            }

        }

        public IEnumerable<RecommendationResult> GetRecommendationContent(HttpContextBase httpContext, ContentRecommendationRequestModel requestModel)
        {
            var recommendationRequest = new RecommendationRequest
            {
                siteId = requestModel.SiteId,
                context = new Context { contentId = requestModel.ContentId, languageId = requestModel.LanguageId },
                numberOfRecommendations = requestModel.NumberOfRecommendations == 0 ? 3 : requestModel.NumberOfRecommendations
            };

            return Task.Run(async () => await _personalizationRecommendationService.Get(httpContext, recommendationRequest)).Result;
        }

        private List<CartItemData> GetCartDataItems(IOrderGroup orderGroup)
        {
            var cartItemDataList = new List<CartItemData>();
            if (orderGroup != null)
            {
                var currency = orderGroup.Currency;
                foreach (var allLineItem in orderGroup.GetAllLineItems())
                {
                    var price = currency.Round(_lineItemCalculator.GetDiscountedPrice(allLineItem, orderGroup.Currency).Divide(allLineItem.Quantity).Amount);
                    var productCode = GetProductCode(allLineItem.Code);
                    cartItemDataList.Add(new CartItemData(productCode, allLineItem.Code == productCode ? null : allLineItem.Code, RoundQuantity(allLineItem.Quantity), price));
                }
            }
            return cartItemDataList;
        }

        private RequestData GetRequestData(HttpContextBase httpContext) => _requestTrackingDataService.GetRequestData(httpContext);
        private CommerceUserData GetCommerceUserData(HttpContextBase httpContext) => _requestTrackingDataService.GetUser(httpContext);

        private int RoundQuantity(decimal quantity)
        {
            var num = (int)decimal.Round(quantity);
            return num != 0 ? num : 1;
        }

        private string GetProductCode(string variantCode)
        {
            return _referenceConverter.GetCode(GetRootProduct(_referenceConverter.GetContentLink(variantCode))) ?? variantCode;
        }

        private ContentReference GetRootProduct(ContentReference targetReference)
        {
            var productVariation = _relationRepository.GetParents<ProductVariation>(targetReference).FirstOrDefault();
            return productVariation == null ? targetReference : GetRootProduct(productVariation.Parent);
        }
    }
}