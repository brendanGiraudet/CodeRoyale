using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRoyal
{
    public static class Player
    {
        private static void Main(string[] args)
        {
            var numSites = int.Parse(Console.ReadLine() ?? throw new ArgumentNullException());
            var sites = GetSites(numSites);

            // game loop
            while (true)
            {
                var inputs = Console.ReadLine()?.Split(' ');
                if (inputs == null) throw new ArgumentNullException();

                var gold = int.Parse(inputs[0]);

                var touchedSite = int.Parse(inputs[1]); // -1 if none

                var buildings = GetBuildings(numSites);
                LinkBuildingsAndSites(buildings, sites);

                var units = GetUnits();
                var myUnits = units.Where(u => u.RelationshipType.Equals(RelationshipType.Friendly)).ToList();
                var myQueen = myUnits.First(u => u.UnitType == UnitType.Queen);
                var enemyUnits = units.Where(u =>
                    u.RelationshipType.Equals(RelationshipType.Enemy)).ToList();
                var enemyUnitPositions = enemyUnits.Select(u => u.Position).ToList();
                var nearestEnemyUnitPosition = FindTheNearestPosition(myQueen.Position, enemyUnitPositions);

                var emptySites = GetEmptySites(sites, buildings);
                var emptySitePositions = emptySites.Select(s => s.Position).ToList();
                var theNearestEmptySitePosition = FindTheNearestPosition(myQueen.Position, emptySitePositions);

                var firstAction = "WAIT";
                const string secondAction = "TRAIN";

                var myBuildings = buildings.Where(b =>
                    b.RelationshipType.Equals(RelationshipType.Friendly)).ToList();

                var concatenedSiteId = "";

                var touchedBuilding = buildings.Find(b => b.SiteId.Equals(touchedSite));

                if (myQueen.IsUnderAttack(nearestEnemyUnitPosition))
                {
                    var theNearestSite = FindTheNearestSite(myQueen.Position, sites);
                    BuildTowerInThisSite(theNearestSite);
                    var archerBuildings = buildings.Where(b => b.UnitTypeBuilt.Equals(UnitType.Archer)).ToList();
                    if (archerBuildings.Count().Equals(0))
                    {
                        NoUnitToTrain();
                        continue;
                    }
                    
                    var theNearestArcherBuild = FindTheNearestBuilding(myQueen.Position, archerBuildings);
                    TrainUnitsInThisBuilding(theNearestArcherBuild);
                    continue;
                }

                if (HasEnoughTower(myBuildings))
                {
                    var randomTower = myBuildings.First(b => b.StructureType.Equals(StructureType.Tower));
                    var siteId = randomTower.SiteId;
                    firstAction = $"BUILD {siteId} TOWER";
                }
                else if (CanBuild(touchedBuilding))
                {
                    firstAction = $"BUILD {touchedSite} ";
                    if (!HasEnoughMine(myBuildings) || touchedBuilding.HasFinishToBuild())
                    {
                        firstAction += "MINE";
                    }
                    else if (!HasEnoughBarracks(myBuildings))
                    {
                        var barrackType = "ARCHER";
                        if (!myBuildings.Any(b => b.UnitTypeBuilt.Equals(UnitType.Giant)))
                            barrackType = "GIANT";
                        else if (!myBuildings.Any(b => b.UnitTypeBuilt.Equals(UnitType.Knight)))
                            barrackType = "KNIGHT";

                        firstAction += $"BARRACKS-{barrackType}";
                    }
                    else
                    {
                        firstAction += "TOWER";
                    }
                }
                else if (theNearestEmptySitePosition != null)
                {
                    firstAction = $"MOVE {theNearestEmptySitePosition.X} {theNearestEmptySitePosition.Y}";
                }

                var needArcher = !myUnits.Any(u => u.UnitType.Equals(UnitType.Archer));
                if (!needArcher)
                {
                    myBuildings = myBuildings.OrderByDescending(b =>
                        b.GetTrainPrice()).ToList();

                    foreach (var building in myBuildings.Where(building => building.CanTrain(gold)))
                    {
                        concatenedSiteId += $" {building.SiteId}";
                        gold -= building.GetTrainPrice();
                    }
                }
                else
                {
                    var archerBuildings = myBuildings.Where(b =>
                        b.UnitTypeBuilt.Equals(UnitType.Archer)).ToList();

                    foreach (var building in archerBuildings.Where(building => building.CanTrain(gold)))
                    {
                        concatenedSiteId += $" {building.SiteId}";
                        gold -= building.GetTrainPrice();
                    }
                }

                Console.WriteLine(firstAction);
                Console.WriteLine(secondAction + concatenedSiteId);
            }
        }

        private static void NoUnitToTrain()
        {
            Console.WriteLine("TRAIN");
        }

        private static void TrainUnitsInThisBuilding(Building theNearestArcherBuild)
        {
            Console.WriteLine($"TRAIN {theNearestArcherBuild.SiteId}");
        }

        private static Building FindTheNearestBuilding(Position myQueenPosition, List<Building> archerBuildings)
        {
            var nearestArcherBuilding = archerBuildings.First();
            var nearestDistance = GetDistance(myQueenPosition, nearestArcherBuilding.Site.Position);
            foreach (var archerBuilding in archerBuildings)
            {
                var distance = GetDistance(myQueenPosition, archerBuilding.Site.Position);

                if (nearestDistance <= distance) continue;
                nearestDistance = distance;
                nearestArcherBuilding = archerBuilding;
            }

            return nearestArcherBuilding;
        }

        private static void BuildTowerInThisSite(Site theNearestSite)
        {
            Console.WriteLine($"BUILD {theNearestSite.Id} TOWER");
        }

        private static bool HasEnoughMine(List<Building> myBuildings)
        {
            var mines = myBuildings.Where(b => b.StructureType.Equals(StructureType.Mine)).ToList();
            if (!mines.Any()) return false;
            var hasMaximumProduction = true;
            foreach (var mine in mines.Where(mine => !mine.HasMaximumProduction()))
            {
                hasMaximumProduction = false;
            }

            Console.Error.WriteLine("max : " + hasMaximumProduction);

            return hasMaximumProduction;
        }

        private static bool HasEnoughTower(List<Building> myBuildings)
        {
            return myBuildings.Count(b => b.StructureType.Equals(StructureType.Tower)) > 2;
        }

        private static Position GetOpposedPosition(Position myQueenPosition, Position nearestEnemyUnitPosition)
        {
            var opposedPosition = nearestEnemyUnitPosition;
            var distance = GetDistance(myQueenPosition, nearestEnemyUnitPosition);
            if (myQueenPosition.X < nearestEnemyUnitPosition.X)
                opposedPosition.X -= distance;
            else if (myQueenPosition.X > nearestEnemyUnitPosition.X)
            {
                opposedPosition.X += distance;
            }

            if (myQueenPosition.Y < nearestEnemyUnitPosition.Y)
            {
                opposedPosition.Y -= distance;
            }
            else if (myQueenPosition.Y > nearestEnemyUnitPosition.Y)
            {
                opposedPosition.Y += distance;
            }

            return opposedPosition;
        }

        private static void LinkBuildingsAndSites(List<Building> buildings, List<Site> sites)
        {
            foreach (var building in buildings)
            {
                building.Site = sites.Find(s => s.Id.Equals(building.SiteId));
            }
        }

        private static bool IsBeside(Position queenPosition, Position unitPosition)
        {
            return GetDistance(queenPosition, unitPosition) < 300;
        }

        private static bool HasEnoughBarracks(List<Building> myBuildings)
        {
            return myBuildings.Count(b => b.StructureType.Equals(StructureType.Barrack)) > 2;
        }

        public static bool CanBuild(Building touchedBuilding)
        {
            if (touchedBuilding == null) return false;
            Console.Error.WriteLine(touchedBuilding.StructureType);
            Console.Error.WriteLine(touchedBuilding.HasFinishToBuild());
            return (touchedBuilding.StructureType.Equals(StructureType.Empty) || !touchedBuilding.HasFinishToBuild());
        }

        private static List<Site> GetEmptySites(List<Site> sites, List<Building> buildings)
        {
            var emptyBuildings = buildings.Where(b =>
                    b.RelationshipType.Equals(RelationshipType.NoStructure))
                .Select(b => b.SiteId).ToList();
            return sites.Where(s => emptyBuildings.Contains(s.Id)).ToList();
        }

        private static List<Unit> GetUnits()
        {
            var units = new List<Unit>();
            var numUnits = int.Parse(Console.ReadLine() ?? throw new ArgumentNullException());

            for (var i = 0; i < numUnits; i++)
            {
                var inputs = Console.ReadLine()?.Split(' ');
                if (inputs == null) return units;
                units.Add(new Unit
                {
                    Position = new Position
                    {
                        X = int.Parse(inputs[0]),
                        Y = int.Parse(inputs[1])
                    },
                    RelationshipType = (RelationshipType) int.Parse(inputs[2]),
                    UnitType = (UnitType) int.Parse(inputs[3]),
                    Health = int.Parse(inputs[4])
                });
            }

            return units;
        }

        private static List<Building> GetBuildings(in int numSites)
        {
            var buildings = new List<Building>();
            for (var i = 0; i < numSites; i++)
            {
                var inputs = Console.ReadLine()?.Split(' ');
                if (inputs == null) return buildings;
                buildings.Add(new Building
                {
                    SiteId = int.Parse(inputs[0]),
                    RemainingGold = int.Parse(inputs[1]),
                    MaxMineSize = int.Parse(inputs[2]),
                    StructureType = (StructureType) int.Parse(inputs[3]),
                    RelationshipType = (RelationshipType) int.Parse(inputs[4]),
                    Param1 = int.Parse(inputs[5]),
                    UnitTypeBuilt = (UnitType) int.Parse(inputs[6])
                });
            }

            return buildings;
        }

        private static List<Site> GetSites(int numOfSites)
        {
            var sites = new List<Site>();
            for (var i = 0; i < numOfSites; i++)
            {
                var inputs = Console.ReadLine()?.Split(' ');
                if (inputs == null) return sites;
                sites.Add(new Site
                {
                    Id = int.Parse(inputs[0]),
                    Position = new Position
                    {
                        X = int.Parse(inputs[1]),
                        Y = int.Parse(inputs[2])
                    },
                    Radius = int.Parse(inputs[3])
                });
            }

            return sites;
        }

        public static Position FindTheNearestPosition(Position currentPosition, List<Position> positions)
        {
            var nearestPosition = positions.First();
            var nearestDistance = GetDistance(currentPosition, nearestPosition);
            foreach (var position in positions)
            {
                var distance = GetDistance(currentPosition, position);

                if (nearestDistance <= distance) continue;
                nearestDistance = distance;
                nearestPosition = position;
            }

            return nearestPosition;
        }

        public static Site FindTheNearestSite(Position currentPosition, List<Site> sites)
        {
            var theNearestSite = sites.First();
            var nearestDistance = GetDistance(currentPosition, theNearestSite.Position);
            foreach (var site in sites)
            {
                var distance = GetDistance(currentPosition, site.Position);

                if (nearestDistance <= distance) continue;
                nearestDistance = distance;
                theNearestSite = site;
            }

            return theNearestSite;
        }

        private static int GetDistance(Position currentPosition, Position expectedPosition)
        {
            return Math.Abs(currentPosition.X - expectedPosition.X) +
                   Math.Abs(currentPosition.Y - expectedPosition.Y);
        }
    }

    public class Site
    {
        public int Id { get; set; }
        public Position Position { get; set; }
        public int Radius { get; set; }

        public override string ToString()
        {
            return "Site\n\t" +
                   $"Id : {Id}\n\t" +
                   $"Position : {Position}\n\t" +
                   $"Radius : {Radius}";
        }
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int GetDistanceWithThisPosition(Position expectedPosition)
        {
            return Math.Abs(this.X - expectedPosition.X) +
                   Math.Abs(this.Y - expectedPosition.Y);
        }

        public override string ToString()
        {
            return $"X : {X}, Y : {Y}";
        }
    }

    public class Building
    {
        private const int KnightPrice = 80;
        private const int ArcherPrice = 100;
        private const int GiantPrice = 140;
        public int SiteId { get; set; }
        public Site Site { get; set; }
        public int RemainingGold { get; set; }
        public int MaxMineSize { get; set; }
        public StructureType StructureType { get; set; }
        public RelationshipType RelationshipType { get; set; }
        public int Param1 { get; set; }
        public UnitType UnitTypeBuilt { get; set; }

        public override string ToString()
        {
            return "Building\n\t" +
                   $"Site : {SiteId}\n\t" +
                   $"RemainingGold : {RemainingGold}\n\t" +
                   $"MaxMineSize : {MaxMineSize}\n\t" +
                   $"StructureType : {StructureType}\n\t" +
                   $"Relationship : {RelationshipType}\n\t" +
                   $"Param1 : {Param1}\n\t" +
                   $"UnitTypeBuilt : {UnitTypeBuilt}\n\t";
        }

        public bool CanTrain(in int gold)
        {
            switch (UnitTypeBuilt)
            {
                case UnitType.Archer:
                    return gold >= ArcherPrice;
                case UnitType.Knight:
                    return gold >= KnightPrice;
                case UnitType.Giant:
                    return gold >= GiantPrice;
                default:
                    return false;
            }
        }

        public int GetTrainPrice()
        {
            switch (UnitTypeBuilt)
            {
                case UnitType.Archer:
                    return ArcherPrice;
                case UnitType.Knight:
                    return KnightPrice;
                case UnitType.Giant:
                    return GiantPrice;
                default:
                    return 0;
            }
        }

        public bool HasMaximumProduction()
        {
            return MaxMineSize.Equals(Param1);
        }

        public bool HasFinishToBuild()
        {
            switch (StructureType)
            {
                case StructureType.Mine:
                    return !HasMaximumProduction();
                default:
                    return true;
            }
        }
    }

    public class Unit
    {
        public Position Position { get; set; }
        public RelationshipType RelationshipType { get; set; }
        public UnitType UnitType { get; set; }
        public int Health { get; set; }

        public override string ToString()
        {
            return "Unit\n\t" +
                   $"Position : {Position}\n\t" +
                   $"Relation : {RelationshipType} \n\t" +
                   $"Type: {UnitType}\n\t" +
                   $"Health : {Health}";
        }

        public bool IsUnderAttack(Position nearestEnemyUnitPosition)
        {
            return Position.GetDistanceWithThisPosition(nearestEnemyUnitPosition) < 300;
        }
    }

    public enum StructureType
    {
        Empty = -1,
        Mine = 0,
        Tower = 1,
        Barrack = 2
    }

    public enum RelationshipType
    {
        NoStructure = -1,
        Friendly = 0,
        Enemy = 1
    }

    public enum UnitType
    {
        Queen = -1,
        Knight = 0,
        Archer = 1,
        Giant = 2
    }
}