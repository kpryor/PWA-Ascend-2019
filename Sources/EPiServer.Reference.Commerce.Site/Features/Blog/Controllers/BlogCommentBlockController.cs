﻿using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Blog.Models;
using EPiServer.Reference.Commerce.Site.Features.Blog.Models.Blocks;
using EPiServer.Reference.Commerce.Site.Features.Blog.Models.Comment;
using EPiServer.Reference.Commerce.Site.Features.Blog.Models.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Blog.Repository;
using EPiServer.Reference.Commerce.Site.Features.Social.Models.Comments;
using EPiServer.Reference.Commerce.Site.Features.Social.Repositories.Common;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Blog.Controllers
{
    /// <summary>
    /// The CommentsBlockController handles the rendering of the comment block frontend view as well
    /// as the posting of new comments.
    /// </summary>
    public class BlogCommentBlockController : BlockController<BlogCommentBlock>
    {
        private readonly IBlogCommentRepository commentRepository;
        private readonly IPageRepository pageRepository;
        private const string MessageKey = "BlogCommentBlock";
        private const string SubmitSuccessMessage = "Your comment was submitted successfully!";
        private const string BodyValidationErrorMessage = "Cannot add an empty comment.";
        private const string NameValidationErrorMessage = "Cannot add an empty name.";
        private const string EmailValidationErrorMessage = "Cannot add an empty email.";
        private const string ErrorMessage = "Error";
        private const string SuccessMessage = "Success";
        private const int RecordPerPage = 5;

        protected readonly IPageRouteHelper pageRouteHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        public BlogCommentBlockController()
        {
            commentRepository = ServiceLocator.Current.GetInstance<IBlogCommentRepository>();
            pageRepository = ServiceLocator.Current.GetInstance<IPageRepository>();
            pageRouteHelper = ServiceLocator.Current.GetInstance<IPageRouteHelper>();
        }

        /// <summary>
        /// Render the comment block frontend view.
        /// </summary>
        /// <param name="currentBlock">The current frontend block instance.</param>
        /// <returns>The action's result.</returns>
        public override ActionResult Index(BlogCommentBlock currentBlock)
        {

            var pagingInfo = new PagingInfo(pageRouteHelper.PageLink.ID, currentBlock.RecordPerPage == 0 ? RecordPerPage : currentBlock.RecordPerPage, 1);
            return GetComment(pagingInfo);
        }

        /// <summary>
        /// Render the comment block frontend view.
        /// </summary>
        /// <param name="pageId">ID of current page link that contain blogCommentBlock</param>
        /// <param name="pageIndex">Current page index of comments</param>
        /// <param name="recordPerPage">Records of comments per page</param>
        /// <returns>The action's result.</returns>
        public ActionResult GetComment(PagingInfo pagingInfo)
        {
            var pageId = pagingInfo.PageId;
            var pageIndex = pagingInfo.PageIndex;
            var pageSize = pagingInfo.PageSize;

            var pageReference = new PageReference(pageId);
            var pageContentGuid = pageRepository.GetPageId(pageReference);

            // Create a comments block view model to fill the frontend block view
            var blockViewModel = new BlogCommentsBlockViewModel(pageReference);

            // Try to get recent comments
            try
            {
                long totalComments;
                var blogComments = commentRepository.Get(
                    new PageCommentFilter
                    {
                        Target = pageContentGuid.ToString(),
                        PageSize = pageSize,
                        PageOffset = pageIndex - 1
                    },
                    out totalComments
                );

                blockViewModel.Comments = blogComments;
                blockViewModel.PagingInfo = pagingInfo;
                blockViewModel.PagingInfo.TotalCount = (int)totalComments;
            }
            catch (Exception ex)
            {
                blockViewModel.Messages.Add(ex.Message);
            }

            return PartialView("~/Features/Blog/Views/BlogCommentBlock/Index.cshtml", blockViewModel);
        }

        /// <summary>
        /// Submit handles the submitting of new comments.  It accepts a comment form model,
        /// validates the form, stores the submitted comment then redirects back to the current page.
        /// </summary>
        /// <param name="formViewModel">The comment form being submitted.</param>
        /// <returns>The submit action result.</returns>
        [HttpPost]
        public ActionResult Submit(BlogCommentFormViewModel formViewModel)
        {
            var errors = ValidateCommentForm(formViewModel);

            if (errors.Count() == 0)
            {
                var addedComment = AddComment(formViewModel);
            }
            else
            {
                // Flag the CommentBody model state with validation error
                AddMessage(MessageKey, errors.First());
            }

            return Redirect(UrlResolver.Current.GetUrl(formViewModel.CurrentPageLink));
        }

        /// <summary>
        /// Adds the comment in the CommentFormViewModel to the Episerver Social repository.
        /// </summary>
        /// <param name="formViewModel">The submitted comment form view model.</param>
        /// <returns>The added PageComment</returns>
        private BlogComment AddComment(BlogCommentFormViewModel formViewModel)
        {
            var newComment = AdaptCommentFormViewModelToSocialComment(formViewModel);
            BlogComment addedComment = null;

            try
            {
                addedComment = commentRepository.Add(newComment);
                AddMessage(MessageKey, SubmitSuccessMessage);
            }
            catch (Exception ex)
            {
                AddMessage(MessageKey, ex.Message);
            }

            return addedComment;
        }

        /// <summary>
        /// Adapts a CommentFormViewModel to a PageComment.
        /// </summary>
        /// <param name="formViewModel">The comment form view model.</param>
        /// <returns>PageComment</returns>
        private BlogComment AdaptCommentFormViewModelToSocialComment(BlogCommentFormViewModel formViewModel)
        {
            return new BlogComment
            {
                Target = pageRepository.GetPageId(formViewModel.CurrentPageLink),
                Name = formViewModel.Name,
                Email = formViewModel.Email,
                Body = formViewModel.Body
            };
        }

        /// <summary>
        /// Validates the comment form.
        /// </summary>
        /// <param name="formViewModel">The comment form view model.</param>
        /// <returns>Returns a list of validation errors.</returns>
        private List<string> ValidateCommentForm(BlogCommentFormViewModel formViewModel)
        {
            var errors = new List<string>();

            // Make sure the comment name has some text
            if (string.IsNullOrWhiteSpace(formViewModel.Name))
            {
                errors.Add(NameValidationErrorMessage);
            }

            // Make sure the comment email has some text
            if (string.IsNullOrWhiteSpace(formViewModel.Email))
            {
                errors.Add(EmailValidationErrorMessage);
            }

            // Make sure the comment body has some text
            if (string.IsNullOrWhiteSpace(formViewModel.Body))
            {
                errors.Add(BodyValidationErrorMessage);
            }

            return errors;
        }

        /// <summary>
        /// Used to retrieve the TempData stored for a specific controller
        /// </summary>
        /// <param name="key">Sring value of the TempData key</param>
        /// <returns>The list of MessageViewModels that was requested</returns>
        public List<string> RetrieveMessages(string key)
        {
            var listOfMessages = (List<string>)TempData[key];

            return (listOfMessages != null) && (listOfMessages.Any()) ? listOfMessages : new List<string>();
        }

        /// <summary>
        /// Stores a desired key / value in the TempData dictionary 
        /// </summary>
        /// <param name="key">The key used to reference the stored value upon retrieval</param>
        /// <param name="value">The value that is being stored in TempData</param>
        public void AddMessage(string key, string value)
        {
            var listOfMessages = RetrieveMessages(key);
            listOfMessages.Add(value);
            TempData[key] = listOfMessages;
        }
    }
}