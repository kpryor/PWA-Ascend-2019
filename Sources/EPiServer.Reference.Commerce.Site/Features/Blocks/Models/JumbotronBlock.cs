using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.Alloy;
using EPiServer.Reference.Commerce.Site.Infrastructure;
using EPiServer.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Blocks.Models
{
    /// <summary>
    /// Used for a primary message on a page, commonly used on start pages and landing pages
    /// </summary>
    [SiteContentType(
        GroupName = SiteTabs.Specialized,
        GUID = "9FD1C860-7183-4122-8CD4-FF4C55E096F9")]
    [Infrastructure.Attributes.SiteImageUrl]
    public class JumbotronBlock : SiteBlockData
    {
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 1
            )]
        [CultureSpecific]
        [UIHint(UIHint.Image)]
        public virtual ContentReference Image { get; set; }
        
        /// <summary>
        /// Gets or sets a description for the image, for example used as the alt text for the image when rendered
        /// </summary>
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 1
            )]
        [CultureSpecific]
        [UIHint(UIHint.Textarea)]
        public virtual string ImageDescription
        {
            get
            {
                var propertyValue = this["ImageDescription"] as string;

                // Return image description with fall back to the heading if no description has been specified
                return string.IsNullOrWhiteSpace(propertyValue) ? Heading : propertyValue;
            }
            set { this["ImageDescription"] = value; }
        }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 1
            )]
        [CultureSpecific]
        public virtual string Heading { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 2
            )]
        [CultureSpecific]
        [UIHint(UIHint.Textarea)]
        public virtual string SubHeading { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 3
            )]
        [CultureSpecific]
        [Required]
        public virtual string ButtonText { get; set; }

        //The link must be required as an anchor tag requires an href in order to be valid and focusable
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 4
            )]
        [CultureSpecific]
        [Required]
        public virtual Url ButtonLink { get; set; }
    }
}
