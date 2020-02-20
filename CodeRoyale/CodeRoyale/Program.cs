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

                var emptySites = GetEmptySites(buildings);

                var myBuildings = buildings.Where(b =>
                    b.RelationshipType.Equals(RelationshipType.Friendly)).ToList();

                var touchedBuilding = buildings.Find(b => b.SiteId.Equals(touchedSite));

                if (myQueen.IsUnderAttack(nearestEnemyUnitPosition))
                {
                    var theNearestSite = FindTheNearestSite(myQueen.Position, sites);
                    BuildTowerInThisSite(theNearestSite);
                    var archerBuildings = buildings.Where(b => b.UnitTypeBuilt.Equals(UnitType.Archer)).ToList();
                    if (archerBuildings.Count.Equals(0))
                    {
                        NoUnitToTrain();
                        continue;
                    }

                    var theNearestArcherBuild = FindTheNearestBuilding(myQueen.Position, archerBuildings);
                    TrainUnitsInThisBuilding(theNearestArcherBuild);
                    continue;
                }

                if (touchedBuilding == null)
                {
                    var theNearestSite = FindTheNearestSite(myQueen.Position, emptySites);
                    if (theNearestSite == null)
                    {
                        var towers = myBuildings.Where(b => b.StructureType.Equals(StructureType.Tower)).ToList();
                        var theNearestTower = FindTheNearestBuilding(myQueen.Position, towers);
                        theNearestSite = theNearestTower.Site;
                    }
                    MoveToThisSite(theNearestSite);
                    TryToTrainUnits(myBuildings, gold);
                    continue;
                }

                Console.Error.WriteLine(touchedBuilding);
                var mines = myBuildings.Where(b => b.StructureType.Equals(StructureType.Mine)).ToList();

                if (!FinishToBuildAllMines(mines, touchedBuilding))
                {
                    BuildMineInThisSite(touchedBuilding.Site);
                    TryToTrainUnits(myBuildings, gold);
                    continue;
                }

                if (touchedBuilding.RelationshipType.Equals(RelationshipType.Friendly))
                {
                    var theNearestSite = FindTheNearestSite(myQueen.Position, emptySites);
                    if (theNearestSite == null)
                    {
                        var towers = myBuildings.Where(b => b.StructureType.Equals(StructureType.Tower)).ToList();
                        var theNearestTower = FindTheNearestBuilding(myQueen.Position, towers);
                        theNearestSite = theNearestTower.Site;
                    }
                    MoveToThisSite(theNearestSite);
                    TryToTrainUnits(myBuildings, gold);
                    continue;
                }

                var archerBarracks = myBuildings.Where(b => b.UnitTypeBuilt.Equals(UnitType.Archer)).ToList();
                if (!FinishToBuildAllArcherBarracks(archerBarracks))
                {
                    BuildArcherBarrackInThisSite(touchedBuilding.Site);
                    TryToTrainUnits(myBuildings, gold);
                    continue;
                }

                var knightBarracks = myBuildings.Where(b => b.UnitTypeBuilt.Equals(UnitType.Knight)).ToList();
                if (!FinishToBuildAllKnightBarracks(knightBarracks))
                {
                    BuildKnightBarrackInThisSite(touchedBuilding.Site);
                    TryToTrainUnits(myBuildings, gold);
                    continue;
                }

                var giantBarracks = myBuildings.Where(b => b.UnitTypeBuilt.Equals(UnitType.Giant)).ToList();
                if (!FinishToBuildAllGiantBarracks(giantBarracks))
                {
                    BuildGiantBarrackInThisSite(touchedBuilding.Site);
                    TryToTrainUnits(myBuildings, gold);
                    continue;
                }

                var towers = myBuildings.Where(b => b.StructureType.Equals(StructureType.Tower)).ToList();
                if (!FinishToBuildAllTowers(towers))
                {
                    BuildTowerInThisSite(touchedBuilding.Site);
                    TryToTrainUnits(myBuildings, gold);
                    continue;
                }
                var theNearestTower = FindTheNearestBuilding(myQueen.Position, towers);
                BuildTowerInThisSite(theNearestTower.Site); // level up the tower
            }
        }

        private static void TryToTrainUnits(List<Building> myBarracks, int gold)
        {
            myBarracks = myBarracks.OrderByDescending(b =>
                b.GetTrainPrice()).ToList();
            var barracksToTrain = new List<Building>();
            foreach (var barrack in myBarracks)
            {
                if (!barrack.CanTrain(gold)) continue;

                barracksToTrain.Add(barrack);
                gold -= barrack.GetTrainPrice();
            }

            TrainUnitsInBuildings(barracksToTrain);
        }

        private static bool FinishToBuildAllTowers(List<Building> towers)
        {
            return towers.Count > 2;
        }

        private static void BuildGiantBarrackInThisSite(Site touchedBuildingSite)
        {
            Console.WriteLine($"BUILD {touchedBuildingSite.Id} BARRACKS-GIANT");
        }

        private static bool FinishToBuildAllGiantBarracks(List<Building> giantBarracks)
        {
            return giantBarracks.Any();
        }

        private static void BuildKnightBarrackInThisSite(Site touchedBuildingSite)
        {
            Console.WriteLine($"BUILD {touchedBuildingSite.Id} BARRACKS-KNIGHT");
        }

        private static bool FinishToBuildAllKnightBarracks(List<Building> knightBarracks)
        {
            return knightBarracks.Any();
        }

        private static void BuildArcherBarrackInThisSite(Site touchedBuildingSite)
        {
            Console.WriteLine($"BUILD {touchedBuildingSite.Id} BARRACKS-ARCHER");
        }

        private static bool FinishToBuildAllArcherBarracks(List<Building> archerBarracks)
        {
            return archerBarracks.Any();
        }

        private static void MoveToThisSite(Site theNearestSite)
        {
            Console.WriteLine($"MOVE {theNearestSite.Position.X} {theNearestSite.Position.Y}");
        }

        private static void BuildMineInThisSite(Site touchedBuildingSite)
        {
            Console.WriteLine($"BUILD {touchedBuildingSite.Id} MINE");
        }

        private static bool FinishToBuildAllMines(List<Building> mines, Building touchedBuilding)
        {
            return (mines.Count > 1 && !touchedBuilding.StructureType.Equals(StructureType.Mine))
                   || (touchedBuilding.StructureType.Equals(StructureType.Mine) &&
                       touchedBuilding.HasMaximumProduction());
        }

        private static void NoUnitToTrain()
        {
            Console.WriteLine("TRAIN");
        }

        private static void TrainUnitsInThisBuilding(Building theNearestArcherBuild)
        {
            Console.WriteLine($"TRAIN {theNearestArcherBuild.SiteId}");
        }
        private static void TrainUnitsInBuildings(List<Building> buildings)
        {
            var concatenatedSiteId = buildings.Aggregate("", (current, building) => current + $" {building.SiteId}");
            Console.WriteLine($"TRAIN{concatenatedSiteId}");
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

        private static void LinkBuildingsAndSites(List<Building> buildings, List<Site> sites)
        {
            foreach (var building in buildings)
            {
                building.Site = sites.Find(s => s.Id.Equals(building.SiteId));
            }
        }

        public static bool CanBuild(Building touchedBuilding)
        {
            if (touchedBuilding == null) return false;
            Console.Error.WriteLine(touchedBuilding.StructureType);
            Console.Error.WriteLine(touchedBuilding.HasFinishToBuild());
            return (touchedBuilding.StructureType.Equals(StructureType.Empty) || !touchedBuilding.HasFinishToBuild());
        }

        private static List<Site> GetEmptySites(List<Building> buildings)
        {
            return buildings.Where(b =>
                    b.RelationshipType.Equals(RelationshipType.NoStructure))
                .Select(b => b.Site).ToList();
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
                    RelationshipType = (RelationshipType)int.Parse(inputs[2]),
                    UnitType = (UnitType)int.Parse(inputs[3]),
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
                    StructureType = (StructureType)int.Parse(inputs[3]),
                    RelationshipType = (RelationshipType)int.Parse(inputs[4]),
                    Param1 = int.Parse(inputs[5]),
                    UnitTypeBuilt = (UnitType)int.Parse(inputs[6])
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

        private static Position FindTheNearestPosition(Position currentPosition, List<Position> positions)
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