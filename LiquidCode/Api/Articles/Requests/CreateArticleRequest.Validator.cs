using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Articles.Requests;

/// <summary>
/// Валидатор для создания статьи.
/// </summary>
public class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
{
    public CreateArticleRequestValidator()
    {
        RuleFor(x => x.Name)
            .RequiredText("Article name", ValidationLengths.Article.Name);

        RuleFor(x => x.Content)
            .RequiredText("Content", ValidationLengths.Article.Content);

        RuleFor(x => x.Tags)
            .ValidTagsCollection(ValidationLengths.Article.TagsMaxCount);

        RuleForEach(x => x.Tags!)
            .ValidTagName()
            .When(x => x.Tags != null);
    }
}
