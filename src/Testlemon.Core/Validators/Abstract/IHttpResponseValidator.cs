namespace Testlemon.Core.Validators.Abstract
{
    /// <summary>
    /// Validator which validates the HTTP response, meaning the http request should be executed before validation
    /// </summary>
    public interface IHttpResponseValidator : IValidator
    {
    }
}