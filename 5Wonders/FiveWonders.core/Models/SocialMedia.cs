using FiveWonders.DataAccess.InMemory;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FiveWonders.core.Models
{
    public class SocialMedia : BaseEntity
    {
        [Required(ErrorMessage = "A Url to your Social Media Acount is required.")]
        [Display(Name = "URL")]
        public string mUrl { get; set; }

        [Display(Name = "Icon")]
        public string m64x64Icon { get; set; }

        public bool isCommunicative { get; set; }

        public SocialMedia()
        {
            isCommunicative = true;
        }
    }

    public class SocialMediaValidator : AbstractValidator<SocialMedia>
    {
        IRepository<SocialMedia> socialMediaContext;

        public SocialMediaValidator(IRepository<SocialMedia> socialMediaRepository, HttpPostedFileBase imgFile)
        {
            socialMediaContext = socialMediaRepository;

            RuleFor(socialMedia => socialMedia.mUrl)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage("Social Media Url cannot be empty.")
                .Must((sm, smUrl) => isUniqueUrl(smUrl, sm.mID))
                    .WithMessage("Social Media Url must be unique.");

            RuleFor(socialMedia => socialMedia.m64x64Icon)
                .Must((sm, imgUrl) => willHaveImg(imgUrl, imgFile))
                    .WithMessage("A 64x64 pixels icon is missing.");
        }

        private bool willHaveImg(string imgUrl, HttpPostedFileBase imgFile)
        {
            return !String.IsNullOrWhiteSpace(imgUrl) || imgFile != null;
        }

        private bool isUniqueUrl(string url, string Id)
        {
            SocialMedia[] socialMedias = socialMediaContext.GetCollection().ToArray();

            if(socialMedias == null || socialMedias.Length <= 0)
            {
                return true;
            }

            return !socialMedias.Any(sm => sm.mUrl == url && sm.mID != Id);
        }
    }
}
