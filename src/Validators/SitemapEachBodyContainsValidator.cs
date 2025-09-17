using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class SitemapEachBodyContainsValidator : SitemapBodiesValidator, IValidator
    {
        public string Name => "sitemap-each-body-contains";

        protected override IValidationResult CompareBodies(Dictionary<Uri, string> bodies, string keyword)
        {
            return new ValidationResult
            {
                IsSuccessful = bodies.All(x => x.Value.Contains(keyword)),
                Items = bodies.Select(x => new ValidationResult
                {
                    IsSuccessful = x.Value.Contains(keyword),
                    Value = x.Key.AbsoluteUri,
                    Message = x.Value.Contains(keyword) ? $"Found '{keyword}' at index: '{x.Value.IndexOf(keyword)}'. Snippet: ...{GetSubstringWithContext(x.Value, keyword, SNIPPET_LENGTH)}..." : string.Empty
                })
            };
        }
    }
}