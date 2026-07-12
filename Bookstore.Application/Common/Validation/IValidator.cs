namespace Bookstore.Application.Common.Validation
{
    public interface IValidator<in T>
    {
        IReadOnlyList<string> Validate(T instance);
    }
}
