﻿using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels
{
    public class MultiAddressViewModel : ContentViewModel<CheckoutPage>
    {
        public MultiAddressViewModel()
        {

        }
        public MultiAddressViewModel(CheckoutPage multiShipmentPage) : base(multiShipmentPage)
        {
        }

        public IList<AddressModel> AvailableAddresses { get; set; }

        public CartItemViewModel[] CartItems { get; set; }

        public string ReferrerUrl { get; set; }
    }
    
}