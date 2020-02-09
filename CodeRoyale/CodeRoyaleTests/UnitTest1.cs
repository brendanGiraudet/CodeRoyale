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

        [Test]
        public void ShouldCheckIfCanBuild()
        {
            // Arrange
            var idOfSiteTouched = 5;
            var buildings = new List<Building>
            {
                new Building
                {
                    RelationshipType = RelationshipType.Friendly,
                    SiteId = 2
                },
                new Building
                {
                    RelationshipType = RelationshipType.Enemy,
                    SiteId = 1
                }
            };

            // Act
            var canBuild = CodeRoyale.Player.CanBuild(idOfSiteTouched, buildings);

            // Assert
            Assert.IsTrue(canBuild);
        }

        [Test]
        public void ShouldCheckIfCanTrain()
        {
            // Arrange
            var sumOfGoldInMyPocket = 100;
            var building = new Building
            {
                UnitTypeBuilt = UnitType.Archer
            };

            // Act
            var canTrain = building.CanTrain(sumOfGoldInMyPocket);

            // Assert
            Assert.IsTrue(canTrain);
        }
    }
}