namespace tone.Directives;

public interface IDirective<T>
{
    public T Apply(T subject);
}