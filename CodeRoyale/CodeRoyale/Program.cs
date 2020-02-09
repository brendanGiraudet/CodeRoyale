using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRoyale
{
    /**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
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
                
                var units = GetUnits();

                var myQueen = units.First(u =>
                    u.UnitType == UnitType.Queen && u.RelationshipType == RelationshipType.Friendly);

                var emptySites = GetEmptySites(sites, buildings);
                var theNearestEmptySite = FindTheNearestSite(myQueen.Position, emptySites);

                var firstAction = "WAIT";
                var secondAction = "TRAIN";

                if (CanBuild(touchedSite, buildings))
                {
                    const string barrackType = "ARCHER";
                    firstAction = $"BUILD {touchedSite} BARRACKS-{barrackType}";
                }
                else if (theNearestEmptySite != null)
                {
                    firstAction = $"MOVE {theNearestEmptySite.Position.X} {theNearestEmptySite.Position.Y}";
                }

                var myBuildings = buildings.Where(b => b.RelationshipType.Equals(RelationshipType.Friendly)).ToList();
                var siteIdConcatened = "";
                myBuildings.ForEach(Console.Error.WriteLine);

                foreach (var building in myBuildings)
                {
                    if (building.CanTrain(gold))
                    {
                        siteIdConcatened += $" {building.SiteId}";
                        gold -= building.GetTrainPrice();
                    }    
                }

                Console.WriteLine(firstAction);
                Console.WriteLine(secondAction + siteIdConcatened);
            }
        }

        public static bool CanBuild(int touchedSite, List<Building> buildings)
        {
            return touchedSite != -1 && !HasBuilt(touchedSite, buildings);
        }

        private static List<Site> GetEmptySites(List<Site> sites, List<Building> buildings)
        {
            var emptyBuildings = buildings.Where(b => b.RelationshipType.Equals(RelationshipType.NoStructure))
                .Select(b => b.SiteId).ToList();
            return sites.Where(s => emptyBuildings.Contains(s.Id)).ToList();
        }

        private static bool HasBuilt(int expectedSiteId, List<Building> buildings)
        {
            return buildings.Any(b =>
                b.SiteId.Equals(expectedSiteId) && b.RelationshipType.Equals(RelationshipType.Friendly));
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
                    Ignore1 = int.Parse(inputs[1]),
                    Ignore2 = int.Parse(inputs[2]),
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

        public static Site FindTheNearestSite(Position currentPosition, List<Site> sites)
        {
            var nearestSite = sites.First();
            var nearestDistance = Getdistance(currentPosition, nearestSite.Position);
            foreach (var site in sites)
            {
                var distance = Getdistance(currentPosition, site.Position);

                if (nearestDistance <= distance) continue;
                nearestDistance = distance;
                nearestSite = site;
            }

            return nearestSite;
        }

        private static int Getdistance(Position currentPosition, Position expectedPosition)
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

        public override string ToString()
        {
            return $"X : {X}, Y : {Y}";
        }
    }

    public class Building
    {
        private const int KnightPrice = 80;
        private const int ArcherPrice = 100;
        public int SiteId { get; set; }
        public int Ignore1 { get; set; }
        public int Ignore2 { get; set; }
        public StructureType StructureType { get; set; }
        public RelationshipType RelationshipType { get; set; }
        public int Param1 { get; set; }
        public UnitType UnitTypeBuilt { get; set; }

        public override string ToString()
        {
            return "Building\n\t" +
                   $"Site : {SiteId}\n\t" +
                   $"Ignore1 : {Ignore1}\n\t" +
                   $"Ignore2 : {Ignore2}\n\t" +
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
                default:
                    return 0;
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
    }

    public enum StructureType
    {
        Empty = -1,
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
        Archer = 1
    }
}