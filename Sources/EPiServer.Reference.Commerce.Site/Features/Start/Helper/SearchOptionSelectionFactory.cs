﻿using EPiServer.Shell.ObjectEditing;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Start.Helper
{
    public class SearchOptionSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new ISelectItem[] {
                new SelectItem() { Text = "Quick search", Value = "QuickSearch" },
                new SelectItem() { Text = "Auto search", Value = "AutoSearch" }
            };
        }
    }
}