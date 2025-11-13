using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Articles.Requests;

/// <summary>
/// Валидатор для обновления статьи.
/// </summary>
public class UpdateArticleRequestValidator : AbstractValidator<UpdateArticleRequest>
{
    public UpdateArticleRequestValidator()
    {
        RuleFor(x => x.Name)
            .OptionalTextWithinRange("Article name", ValidationLengths.Article.Name);

        RuleFor(x => x.Content)
            .OptionalTextWithinRange("Content", ValidationLengths.Article.Content);

        RuleFor(x => x.Tags)
            .ValidTagsCollection(ValidationLengths.Article.TagsMaxCount);

        RuleForEach(x => x.Tags!)
            .ValidTagName()
            .When(x => x.Tags != null);
    }
}
