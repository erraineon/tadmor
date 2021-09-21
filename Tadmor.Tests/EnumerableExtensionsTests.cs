using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tadmor.Core.Extensions;

namespace Tadmor.Tests
{
    [TestClass]
    public class EnumerableExtensionsTests
    {
        [TestMethod]
        public void Ensure_NoDuplicates()
        {
            var items = new[] { ("a", weight: float.Epsilon), ("b", 1), ("c", weight: float.Epsilon) };
            var randomItems = items.RandomSubset(2, weightFunction: t => t.weight).ToList();
            CollectionAssert.AllItemsAreUnique(randomItems);
        }
    }
}