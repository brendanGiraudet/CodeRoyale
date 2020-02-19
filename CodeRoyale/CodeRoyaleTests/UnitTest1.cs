using System.Collections.Generic;
using CodeRoyal;
using NUnit.Framework;

namespace CodeRoyaleTests
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
            var nearestPosition = CodeRoyal.Player.FindTheNearestSite(currentPosition, sites);

            // Assert
            Assert.AreEqual(nearestPosition, expectedSite);
        }

        [Test]
        public void ShouldCheckIfCanBuild()
        {
            // Arrange
            var emptyBuilding = new Building
            {
                RelationshipType = RelationshipType.NoStructure,
                StructureType = StructureType.Empty,
                SiteId = 5
            };

            // Act
            var canBuild = Player.CanBuild(emptyBuilding);

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

        [Test]
        public void ShouldGetDistanceWithThisPosition()
        {
            // Arrange
            var expectedDistance = 2;
            var currentPosition = new Position
            {
                X = 1,
                Y = 1
            };
            var otherPosition = new Position
            {
                X = 2,
                Y = 2
            };

            // Act
            var distance = currentPosition.GetDistanceWithThisPosition(otherPosition);
            
            // Assert
            Assert.AreEqual(distance,expectedDistance);
        }
    }
}