namespace CSL.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class AllowNoSessionAttribute : Attribute
{
}
