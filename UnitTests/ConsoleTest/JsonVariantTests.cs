using FiveDChessDataInterface.Builders;
using FiveDChessDataInterface.Variants;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UnitTests.ConsoleTest
{
    [TestClass]
    public class JsonVariantTests
    {
        [TestMethod]
        public void EnsureJsonIsParsable()
        {
            var variants = GithubVariantGetter.GetAllVariants(true, true);
            Assert.IsTrue(variants.Any(), "No variants exist!");

            foreach (var variant in variants)
            {
                Console.WriteLine($"Checking variant '{variant.Name}'...");

                Assert.IsTrue(!string.IsNullOrWhiteSpace(variant.Name), "Variant name was empty!"); // ensure variant name is there
                Assert.IsTrue(!string.IsNullOrWhiteSpace(variant.Author), "Author name was empty!"); // ensure authro name is there
                Assert.IsTrue(variant.Timelines.Any(), "There are no timelines!"); // ensure a timeline exists
                Assert.IsTrue(variant.Timelines.All(x => x.Value.Any(b => b != null)), "One or more timelines do not have any non-null boards!"); // ensure every timeline has a non-null board

                foreach (var (tIndexString, boards) in variant.Timelines)
                {
                    try
                    {
                        var timelineIndex = (BaseGameBuilder.Timeline.TimelineIndex)tIndexString;
                    }
                    catch (InvalidCastException)
                    {
                        Assert.Fail($"Casting of timeline string {tIndexString} of variant '{variant.Name}' failed!");
                    }

                }

                var built = variant.GetGameBuilder().Build();
            }
        }
    }
}
