using EPiServer.Reference.Commerce.Shared.Constants;
using EPiServer.Shell.ObjectEditing;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Shared.EditorDescriptors
{
    public class CalendarViewModeSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            var dic = new Dictionary<string, string>
            {
                { "Day", CalendarViewModes.Day },
                { "Week", CalendarViewModes.Week },
                { "Month", CalendarViewModes.Month },
                { "List", CalendarViewModes.List }
            };

            return dic.Select(x => new SelectItem { Text = x.Key, Value = x.Value });
        }
    }
}