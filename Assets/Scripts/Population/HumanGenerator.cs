using UnityEngine;

namespace Population
{
    public static class HumanGenerator
    {
        private static readonly string[] FemaleNames = {
            "Sabine",
            "Susanne",
            "Petra",
            "Monika",
            "Claudia",
            "Birgit",
            "Andrea",
            "Stephanie",
            "Karin",
            "Kathrin",
            "Julia",
            "Anna",
            "Nicole",
            "Anja",
            "Gabriele",
            "Angelika",
            "Martina",
            "Brigitte",
            "Heike",
            "Sandra",
            "Renate",
            "Barbara",
            "Christina",
            "Katharina",
            "Christine",
            "Kerstin",
            "Ursula",
            "Maria",
            "Ute",
            "Sarah/Sara",
            "Marion",
            "Silvia",
            "Melanie",
            "Ulrike",
            "Bettina",
            "Jutta",
            "Silke",
            "Tanja",
            "Christiane",
            "Elke",
            "Nadine",
            "Ingrid",
            "Daniela",
            "Angela",
            "Jennifer",
            "Anke",
            "Cornelia",
            "Nina",
            "Sonja",
            "Katja",
            "Regina",
            "Britta",
            "Eva",
            "Annika",
            "Beate",
            "Jessica",
            "Manuela",
            "Gisela",
            "Anne",
            "Lisa",
            "Maike",
            "Alexandra",
            "Annette",
            "Bärbel",
            "Kirsten",
            "Laura",
            "Astrid",
            "Dagmar",
            "Helga",
            "Franziska",
            "Caroline",
            "Yasmin",
            "Jana",
            "Yvonne",
            "Doris",
            "Krista",
            "Michaela",
            "Sabrina",
            "Lena",
            "Maren",
            "Elisabeth",
            "Vanessa",
            "Simone",
            "Marie",
            "Antje",
            "Hannelore",
            "Johanna",
            "Sigrid",
            "Hannah",
            "Svenja",
            "Marlies",
            "Bianca",
            "Rita",
            "Gudrun",
            "Janina",
            "Marina",
            "Marianne",
            "Nathalie",
            "Heidi",
            "Jacqueline"
        };
        private static readonly string[] MaleNames = {
            "Michael",
            "Thomas",
            "Andreas",
            "Peter",
            "Stefan",
            "Christian",
            "Hans",
            "Klaus",
            "Jan",
            "Wolfgang",
            "Frank",
            "Matthias",
            "Jürgen",
            "Alexander",
            "Martin",
            "Bernd",
            "Markus",
            "Thorsten",
            "Uwe",
            "Jens",
            "Jörg",
            "Ralph",
            "Sven",
            "Daniel",
            "Sebastian",
            "Reiner",
            "Manfred",
            "Oliver",
            "Holger",
            "Dirk",
            "Tobias",
            "Philipp",
            "Karsten",
            "Florian",
            "Dieter",
            "Christoph",
            "Dennis",
            "Joachim",
            "Patrick",
            "Marc",
            "Karl",
            "Andre",
            "Marco",
            "Tim",
            "Kai",
            "Marcel",
            "Robert",
            "Werner",
            "Lars",
            "Rolf",
            "Günther",
            "Norbert",
            "Lukas",
            "Volker",
            "Gerhard",
            "Felix",
            "Nils",
            "Benjamin",
            "Ulrich",
            "Olaf",
            "Horst",
            "Johannes",
            "David",
            "Harald",
            "Sascha",
            "Helmut",
            "Heinz",
            "Björn",
            "Mike",
            "Fabian",
            "René",
            "Axel",
            "Timo",
            "Detlev",
            "Gerd",
            "Reinhardt",
            "Kevin",
            "Heiko",
            "Maximilian",
            "Jonas",
            "Niklas",
            "Paul",
            "Ingo",
            "Simon",
            "Christopher",
            "Mario",
            "Georg",
            "Steffen",
            "Julian",
            "Niko",
            "Rüdiger",
            "Dominik",
            "Roland",
            "Mohammed",
            "Bernhard",
            "Walther",
            "Jörn",
            "Moritz",
            "Victor",
            "Hartmut"
        };
        private static readonly string[] LastNames = {
            "Müller",
            "Schmidt",
            "Schneider",
            "Fischer",
            "Meyer",
            "Weber",
            "Berger",
            "Wagner",
            "Bergmann",
            "Becker",
            "Böhm",
            "Schulz",
            "Brandt",
            "Hoffmann",
            "Braun",
            "Schäfer",
            "Busch",
            "Koch",
            "Dietrich",
            "Bauer",
            "Engel",
            "Richter",
            "Ernst",
            "Klein",
            "Fischer",
            "Schröder",
            "Frank",
            "Wolf",
            "Franke",
            "Neumann",
            "Friedrich",
            "Schwarz",
            "Fuchs",
            "Zimmermann",
            "Graf",
            "Krüger",
            "Groß",
            "Braun",
            "Günther",
            "Hofmann",
            "Haas",
            "Schmitz",
            "Hahn",
            "Hartmann",
            "Hartmann",
            "Lange",
            "Heinrich",
            "Schmitt",
            "Herrmann",
            "Werner",
            "Hoffmann",
            "Krause",
            "Hofmann",
            "Meier"
        };
        
        public static Human GenerateHuman()
        {
            Human human;

            if (Random.value > 0.5f)
            {
                human = new Human(GetFemaleName(), GetLastName(), GetAge(), Gender.Female);
            }
            else
            {
                human = new Human(GetMaleName(), GetLastName(), GetAge(), Gender.Male);
            }
            if (Random.value > 0.5f)
            {
                PopulationManager.Instance.NeededCommerceJobs++;
            }
            else
            {
                PopulationManager.Instance.NeededIndustrialJobs++;
            }

            PopulationManager.Instance.NeededResidents--;

            return human;
        }

        private static string GetFemaleName()
        {
            return FemaleNames[(int) (Random.value * FemaleNames.Length)];
        }

        private static string GetMaleName()
        {
            return MaleNames[(int) (Random.value * MaleNames.Length)];
        }

        private static string GetLastName()
        {
            return LastNames[(int) (Random.value * LastNames.Length)];
        }

        private static int GetAge()
        {
            return 18 + (int) (Random.value * 82);
        }
    }
}