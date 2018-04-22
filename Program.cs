using System;
using System.Collections.Generic;
using System.Linq;

namespace CategorySelector
{
    class Program
    {
        static void Main(string[] args)
        {
            string query = "a > 2&&c=true";

            ICategoryQueryTransformator categoryQueryTransformator = new CategoryQueryTransformator();

            var result = categoryQueryTransformator.Transform(query, out var transformedQuery, out var queryChecks);

            if (result.isOk)
            {
                var willPass = new Dictionary<string, object>
                {
                    { "a", new Color { Val = 10 } },
                    { "c", true }
                };

                var wontPass = new Dictionary<string, object>
                {
                    { "a", -2.2 },       // won't pass "a>-2.2" in query
                    { "c", true }
                };

                var t = ((IComparable)willPass["a"]).CompareTo(2);

                var willSkipped = new Dictionary<string, object>
                {
                    { "c", true },
                };

                if (!queryChecks.IsSubsetOf(willSkipped.Keys))
                {
                    Console.WriteLine($"Skip {nameof(willSkipped)} because it doesn't contains all keys what query will check");
                }

                Console.WriteLine($"{nameof(willPass)} - {transformedQuery(willPass)}");
                Console.WriteLine($"{nameof(wontPass)} - {transformedQuery(wontPass)}");
            }
            else
            {
                foreach (var error in result.errors)
                {
                    Console.WriteLine(error);
                }
            }
        }

        class Color : IComparable
        {
            public int Val { get; set; }
            public int CompareTo(object obj)
            {
                return Val.CompareTo(obj);
            }
        }
    }
}
