using System.Collections.Generic;
using CodeRoyale;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void ShouldFindTheNearestSite()
        {
            // Arrange
            var currentPosition = new Position
            {
                X = 8,
                Y = 8
            };
            var expectedSite = new Site
            {
                Position = new Position
                {
                    X = 9,
                    Y = 9
                }
            };

            var sites = new List<Site>
            {
                new Site
                {
                    Position = new Position
                    {
                        X = 5,
                        Y = 5
                    }
                },
                expectedSite
            };

            // Act
            var nearestPosition = CodeRoyale.Player.FindTheNearestSite(currentPosition, sites);

            // Assert
            Assert.AreEqual(nearestPosition, expectedSite);
        }
    }
}