using System.Collections.Generic;

using TransformedQuery = System.Func<System.Collections.Generic.IDictionary<string, object>, bool>;

namespace CategorySelector
{
    public interface ICategoryQueryTransformator
    {
        (bool isOk, string[] errors) Transform(string query, out TransformedQuery transformedQuery, out HashSet<string> queryChecks);
    }
}