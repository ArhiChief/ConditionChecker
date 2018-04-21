using System;
using System.Collections.Generic;

namespace CategorySelector
{
    class Program
    {
        static void Main(string[] args)
        {
            string query = "a> -2.2 && c=true";

            ICategoryQueryTransformator categoryQueryTransformator = new CategoryQueryTransformator();

            var result = categoryQueryTransformator.Transform(query, out var transformedQuery, out var queryChecks);

            if (result.isOk)
            {
                var willPass = new Dictionary<string, object>
                {
                    { "a", 10 },
                    { "c", true }
                };

                var wontPass = new Dictionary<string, object>
                {
                    { "a", -10 },       // won't pass "a>-2.2" in query
                    { "c", true }
                };

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
    }
}
