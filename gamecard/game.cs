using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CardGame
{
    #region GameLogic and Processor
    class game
    {
        static void Main(string[] args)
        {
            // Check if the correct number of arguments is provided
            if (args.Length != 4)
            {
                Console.WriteLine("Incorrect number of parameters supplied");
                return;
            }

            string inputFile = string.Empty;
            string outputFile = string.Empty;
            string parameter1name = string.Empty;
            string parameter2name = string.Empty;
            List<string> listofparameters = new List<string>();
            // Loop through the command line arguments
            for (int i = 0; i < args.Length; i++)
            {
                // Check if the current argument is "-param1"
                if (args[i] == "--in")
                {
                    parameter1name = "--in";
                    // Get the value of the next argument as parameter1
                    inputFile = args[i + 1];
                }
                else if (args[i] == "--out")
                {
                    parameter2name = "--out";
                    // Get the value of the next argument as parameter1
                    outputFile = args[i + 1];
                }
            }

            listofparameters.Add(parameter1name);
            listofparameters.Add(parameter2name);

            bool containsItem1 = listofparameters.Contains("--in");
            bool containsItem2 = listofparameters.Contains("--out");
            if (containsItem1 == false)
            {
                Console.WriteLine("--in parameter missing");
                return;
            }
            else if (containsItem2 == false)
            {
                Console.WriteLine("--out parameter missing");
                return;
            }


            if (!inputFile.Contains(".txt"))
            {
                Console.WriteLine("Only .txt files are required for input");
                return;
            }
            if (!outputFile.Contains(".txt"))
            {
                Console.WriteLine("Only .txt files are required for output");
                return;
            }


            try
            {

                List<Player> tiedTeams = null;
                int tieScore = 0;

                // Read the data from the input file
                string[] data = File.ReadAllLines(inputFile);

                if (data.Length == 0)
                {
                    using (StreamWriter writer = new StreamWriter(outputFile))
                    {
                        writer.WriteLine("Error: The text file is empty.");
                    }
                    return;
                }

                var nonBlankLines = new List<string>();

                // Iterate through each line and check if it is blank
                foreach (string line in data)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // Add non-blank lines to the list
                        nonBlankLines.Add(line);
                    }
                }

                // Write the non-blank lines to the temporary file
                File.WriteAllLines(inputFile, nonBlankLines);

                // Replace the original file with the temporary file
                //File.Replace(inputFile, inputFile, null);

                Dictionary<string, string[]> userCards = new Dictionary<string, string[]>();

                List<PlayerCards> cardValuesStatement = new List<PlayerCards>();
                List<string> lst = new List<string>();

                string[] lines = File.ReadAllLines(inputFile);

                // Check if each line contains a valid card

                try
                {
                    foreach (string line in lines)
                    {
                        if (line.Contains(":"))
                        {
                            // Split the line into player name and card values
                            string[] parts = line.Split(':');
                            string playerName = parts[0];
                            string[] cardValues = parts[1].Trim().Split(',');
                            if (cardValues.Contains("") || line.EndsWith(","))
                            {
                                if (line.EndsWith(","))
                                {
                                    using (StreamWriter writer = new StreamWriter(outputFile))
                                    {
                                        writer.WriteLine("Error: Please remove comma from end of players cards");
                                    }
                                    return;
                                }
                                else if (cardValues.Contains(""))
                                {
                                    using (StreamWriter writer = new StreamWriter(outputFile))
                                    {
                                        writer.WriteLine("Error: 5 cards is required to play");
                                    }
                                    return;
                                }
                            }
                            else
                            {

                                userCards.Add(playerName, cardValues);
                            }
                        }
                        else
                        {
                            using (StreamWriter writer = new StreamWriter(outputFile))
                            {
                                writer.WriteLine("Error: Please enter player name seperated by a colon");
                            }
                            return;
                        }
                        var x = !Regex.IsMatch(line, "[^,]");

                        if (x == true)
                        {
                            using (StreamWriter writer = new StreamWriter(outputFile))
                            {
                                writer.WriteLine("Error: A comma seperated card array is required to play");
                            }
                            return;
                        }
                    }
                }
                catch (Exception e)
                {

                    using (StreamWriter writer = new StreamWriter(outputFile))
                    {
                        writer.WriteLine(e.Message);
                    }
                    return;
                }


                if (userCards.Count != 7)
                {
                    using (StreamWriter writer = new StreamWriter(outputFile))
                    {
                        writer.WriteLine("Error: 7 players required to play.");
                    }
                    return;
                }
                else if (userCards.Count == 7)
                {
                    var x = AlphanumericCheck.ValidateCards.ValidateInputCards(lines);
                    if (x.Count() == 3)
                    {
                        using (StreamWriter writer = new StreamWriter(outputFile))
                        {
                            writer.WriteLine("Duplicate card suit found, please remove");
                        }
                        return;
                    }
                    else if (x.Count() >= 1)
                    {

                        foreach (var item in x)
                        {
                            using (StreamWriter writer = new StreamWriter(outputFile))
                            {
                                writer.WriteLine(String.Join("\n", item + " " + "has in-correct player cards"));
                            }
                        }
                        return;
                    }

                }



                // Create a list to store the players and their scores
                List<Player> players = new List<Player>();

                // Create a list to store the players and their scores
                List<Player> playersTied = new List<Player>();

                // Iterate over each line in the input file
                foreach (string line in lines)
                {
                    // Split the line into player name and card values
                    string[] parts = line.Split(':');
                    string playerName = parts[0];
                    string[] cardValues = parts[1].Trim().Split(',');

                    for (int i = 0; i < cardValues.Length; i++)
                    {
                        cardValues[i] = cardValues[i].Replace(" ", "");
                    }

                    lst = cardValues.OfType<string>().ToList();



                    foreach (var item in lst)
                    {
                        item.Replace(" ", "");
                    }

                    cardValuesStatement.Add(new PlayerCards()
                    {
                        Name = playerName,
                        CardValue = lst
                    });

                    // Calculate the score for each player
                    int score = 0;
                    foreach (string cardValue in cardValues)
                    {
                        score += GetCardValue(cardValue);
                    }


                    // Create a new player object and add it to the list
                    Player player = new Player(playerName, score, cardValues);
                    players.Add(player);


                }

                var duplicate = players.GroupBy(x => new { x.Score }).Where(x => x.Skip(1).Any()).Select(x => x.Key).ToList();

                if(duplicate.Count() != 0)
                {
                    tieScore = duplicate.ToList().Max(x => x.Score);
                    var orderedTeams = players.OrderBy(t => t.Score).ToList();
                    tiedTeams = orderedTeams.Where(t => t.Score == tieScore).ToList();
                }
                
                CardShapesViewModel csvm = new CardShapesViewModel();
                csvm.CardShapes = new List<AlphanumericCheck.CardShapes>();
               
                List<Player> winners = new List<Player>();
                int highestScore = 0;

                highestScore = players.Max(x => x.Score);

                foreach (Player player in players)
                {
                    if (player.Score == highestScore)
                    {
                        winners.Add(player);
                    }
                }
                string gametype = "";
                if (tiedTeams != null)
                {
                    try
                    {
                        for (int i = 0; i <= tiedTeams.Count(); i++)
                        {
                            var name = players.Where(x => x.Name == tiedTeams[i].Name).Select(x => x.Name).FirstOrDefault().ToString();
                            var query = players.Where(x => x.Name == tiedTeams[i].Name).Select(x => x.CardValue);
                            var q = query;

                            foreach (var item in q)
                            {
                                csvm.CardShapes.Add(new AlphanumericCheck.CardShapes()
                                {
                                    CardShape = item,
                                    Name = name
                                });
                            }

                            if (csvm.CardShapes.Count() > 1)
                            {
                                break;
                            }
                        }
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        // Handle the exception
                        Console.WriteLine("An error occurred: " + ex.Message);
                    }

                    AlphanumericCheck.CardViewModel duplicatealpha = new AlphanumericCheck.CardViewModel();
                    AlphanumericCheck.CardModeler nc = new AlphanumericCheck.CardModeler();
                    duplicatealpha = AlphanumericCheck.CardHelper.dupcard(csvm);
                    nc.MyProperty = new List<AlphanumericCheck.CardViewModel>();
                    nc.MyProperty.Add(duplicatealpha);
                    bool tie = false;
                    List<string> newcards = new List<string>();
                    string newcard;
                    bool containsThree = false;

                    if (nc.MyProperty != null)
                    {
                        foreach (var item in nc.MyProperty)
                        {
                            if (item.Cardshapes.Count() > 1)
                            {
                                foreach (var x in item.Cardshapes)
                                {
                                    foreach (var s in x.Suit)
                                    {
                                        newcard = s;
                                        newcards.Add(newcard);
                                    }
                                }
                                Dictionary<string, int> wordCount = new Dictionary<string, int>();
                                PlayerCards pc = new PlayerCards();
                                pc.CardValue = new List<string>();

                                bool checkif2 = false;
                                foreach (var x in newcards)
                                {
                                    pc.CardValue.Add(x);
                                }

                                foreach (string word in pc.CardValue)
                                {
                                    if (wordCount.ContainsKey(word))
                                    {
                                        wordCount[word]++;
                                    }
                                    else
                                    {
                                        wordCount[word] = 1;
                                    }
                                }
                                var sumOfDuplicates = wordCount.Select(x => x.Value).ToList();
                                int itemToCheck = 2, itemToCheckForThree = 3;
                                //if specified once then tie
                                tie = sumOfDuplicates.Count(x => x == itemToCheck) > 1;
                                containsThree = sumOfDuplicates.Count(x => x == itemToCheckForThree) == 3;
                                checkif2 = sumOfDuplicates.Contains(2);

                                foreach (Player player in players)
                                {
                                    if (player.Score == highestScore)
                                    {
                                        winners.Add(player);
                                    }
                                }

                                if (containsThree)
                                {
                                    using (StreamWriter writer = new StreamWriter(outputFile))
                                    {
                                        writer.WriteLine("Extra card suit found, please remove");
                                    }
                                    return;
                                }

                                if (tie == false && tiedTeams.Count() == 2 && tieScore == highestScore)
                                {
                                    gametype = "tie";
                                }
                                else if (tie == false && checkif2 == true)
                                {
                                    gametype = "highestscore";
                                }
                                else
                                {
                                    gametype = "score";
                                }

                            }
                        }
                    }

                    // Create a list to store the winners

                    AlphanumericCheck.Card card = new AlphanumericCheck.Card();
                    List<string> playerNames = new List<string>();

                    List<KeyValuePair<string, string>> suitScores = new List<KeyValuePair<string, string>>();
                    List<KeyValuePair<string, string>> u = new List<KeyValuePair<string, string>>();

                    if (gametype == "score")
                    {
                        List<PlayerScores> p = new List<PlayerScores>();
                        using (StreamWriter writer = new StreamWriter(outputFile))
                        {
                            foreach (Player winner in winners.Distinct())
                            {
                                var player = new Player(winner.Name, winner.Score, new string[0]);
                                p.Add(new PlayerScores()
                                {
                                    Name = player.Name,
                                    Score = player.Score
                                });
                            }
                            PlayerScores[] names = p.ToArray();
                            string[] name = names.Select(c => c.Name.ToString()).ToArray();
                            string commaSeparatedString = string.Join(",", name);

                            string score = p.Max(x => x.Score).ToString();
                            writer.WriteLine(commaSeparatedString + ":" + score);
                        }
                    }
                    if (gametype == "tie")
                    {
                        foreach (string line in lines)
                        {
                            string[] parts = line.Split(':');
                            string playerName = parts[0];
                            playerNames.Add(playerName);
                            string[] cardValues = parts[1].Trim().Split(',');

                            for (int i = 0; i < cardValues.Length; i++)
                            {
                                cardValues[i] = cardValues[i].Replace(" ", "");
                            }


                            foreach (var tied in tiedTeams)
                            {
                                foreach (string k in lines.Where(x => x.Contains(tied.Name)))
                                {
                                    if (k.Contains(tied.Name))
                                    {
                                        string[] parts2 = k.Split(':');
                                        string playerName2 = parts2[0];
                                        playerNames.Add(playerName);
                                        string[] cardValues2 = parts2[1].Split(',');

                                        for (int i = 0; i < cardValues2.Length; i++)
                                        {
                                            cardValues2[i] = cardValues2[i].Replace(" ", "");
                                        }


                                        u = AlphanumericCheck.CardHelper.FindHighestCard(tied.Name, cardValues2);

                                        suitScores.Add(new KeyValuePair<string, string>(u.Max(y => y.Key), u.Max(z => z.Value)));

                                    }
                                }
                            }
                        }
                        foreach (var item in u)
                        {
                            suitScores.Add(new KeyValuePair<string, string>(item.Key, item.Value));
                        }

                        var v = AlphanumericCheck.CardHelper.FindMax(suitScores);
                        card = (from i in v.Cards
                                let maxId = v.Cards.Max(m => m.Value)
                                where i.Value == maxId
                                select i).FirstOrDefault();

                        var baseValue = AlphanumericCheck.CardHelper.GetBaseCardValue(card.Suit);

                        using (StreamWriter writer = new StreamWriter(outputFile))
                        {
                            foreach (Player winner in winners.Distinct())
                            {
                                if (winner.Name == card.Name)
                                {
                                    writer.WriteLine(winner.Name + ":" + (winner.Score + baseValue));
                                }
                            }
                        }
                    }
                    if (gametype == "highestscore")
                    {


                        foreach (Player player in players)
                        {
                            if (player.Score == highestScore)
                            {
                                winners.Add(player);
                            }
                        }

                        using (StreamWriter writer = new StreamWriter(outputFile))
                        {
                            foreach (Player winner in winners.Distinct())
                            {
                                writer.WriteLine(winner.Name + ":" + winner.Score);
                            }
                        }
                    }
                }
                else
                {
  

                    foreach (Player player in players)
                    {
                        if (player.Score == highestScore)
                        {
                            winners.Add(player);
                        }
                    }

                    using (StreamWriter writer = new StreamWriter(outputFile))
                    {
                        foreach (Player winner in winners.Distinct())
                        {
                            writer.WriteLine(winner.Name + ":" + winner.Score);
                        }
                    }
                }

                Console.WriteLine("The winners have been written to the output file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        static int GetCardValue(string cardValue)
        {
            switch (cardValue.ToUpper())
            {
                case "2C":
                    return 2;
                case "2D":
                    return 2;
                case "2S":
                    return 2;
                case "2H":
                    return 2;
                case "3C":
                    return 3;
                case "3H":
                    return 3;
                case "3S":
                    return 3;
                case "3D":
                    return 3;
                case "4H":
                    return 4;
                case "4D":
                    return 4;
                case "4S":
                    return 4;
                case "4C":
                    return 4;
                case "5D":
                    return 5;
                case "5S":
                    return 5;
                case "5C":
                    return 5;
                case "5H":
                    return 5;
                case "6C":
                    return 6;
                case "6D":
                    return 6;
                case "6S":
                    return 6;
                case "6H":
                    return 6;
                case "7H":
                    return 7;
                case "7D":
                    return 7;
                case "7C":
                    return 7;
                case "7S":
                    return 7;
                case "8D":
                    return 8;
                case "8S":
                    return 8;
                case "8C":
                    return 8;
                case "8H":
                    return 8;
                case "9D":
                    return 9;
                case "9C":
                    return 9;
                case "9S":
                    return 9;
                case "9H":
                    return 9;
                case "10D":
                    return 10;
                case "10S":
                    return 10;
                case "10C":
                    return 10;
                case "10H":
                    return 10;
                case "AH":
                    return 11;
                case "AD":
                    return 11;
                case "AC":
                    return 11;
                case "AS":
                    return 11;
                case "KH":
                    return 13;
                case "KC":
                    return 13;
                case "KS":
                    return 13;
                case "KD":
                    return 13;
                case "QD":
                    return 12;
                case "QC":
                    return 12;
                case "QH":
                    return 12;
                case "QS":
                    return 12;
                case "JH":
                    return 11;
                case "JC":
                    return 11;
                case "JD":
                    return 11;
                case "JS":
                    return 11;
                default:
                    return 0;
            }
        }
    }
    #endregion

    #region ViewModels, Classes and Methods
    public class PlayerCards
    {
        public string Name { get; set; }
        public List<string> CardValue { get; set; }
    }

    class Player
    {
        public string Name { get; set; }
        public int Score { get; set; }

        public string[] CardValue { get; set; }

        public Player(string name, int score, string[] cardValue)
        {
            Name = name;
            Score = score;
            CardValue = cardValue;
        }

    }

    public class CardShapesViewModel
    {
        public List<AlphanumericCheck.CardShapes> CardShapes { get; set; }
    }

    class PlayerScores
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }

    public class AlphanumericCheck
    {

        public class Card
        {
            public string Name;
            public int Value;
            public string Suit;
        }

        public class CardShape
        {
            public string Name;
            public int Value;
            public List<string> Suit;
        }

        public class CardShapes
        {
            public string Name { get; set; }
            public string[] CardShape { get; set; }
            public CardShapes() { }
            public CardShapes(string name, string[] cardShapes)
            {
                Name = name;
                CardShape = cardShapes;
            }
        }

        public class CardViewModel
        {
            public List<Card> Cards { get; set; }

            public List<CardShape> Cardshapes { get; set; }
        }

        public class CardModeler
        {
            public List<CardViewModel> MyProperty { get; set; }
        }

        public class ValidateCards
        {
            public static List<string> ValidateInputCards(string[] lines)
            {
                // Create a dictionary to store the player's hand
                Dictionary<string, List<string>> playerHands = new Dictionary<string, List<string>>();
                List<string> playerWithIncorrect = new List<string>();

                // Process each line in the input file
                foreach (string line in lines)
                {
                    // Split the line into player name and cards
                    string[] parts = line.Split(':');
                    string playerName = parts[0].Trim();
                    string[] cards = parts[1].Split(',');

                    // Create a list to store the player's cards
                    List<string> playerCards = new List<string>();

                    // Process each card in the line
                    foreach (string c in cards)
                    {
                        // Remove any spaces and convert to uppercase
                        string trimmedCard = c.Trim().ToUpper();
                        trimmedCard = trimmedCard.Replace(" ", "");

                        // Add the card to the player's hand
                        playerCards.Add(trimmedCard);
                    }

                    // Add the player's hand to the dictionary
                    playerHands.Add(playerName, playerCards);
                }

                // Check if an item appears three times
                List<string> usercards = new List<string>();
                foreach (KeyValuePair<string, List<string>> playerHand in playerHands)
                {
                    foreach (var item in playerHand.Value)
                    {
                        usercards.Add(item);
                    }
                }
                string[] usercardarray = usercards.ToArray();

                bool containsDuplicates = usercardarray.GroupBy(x => x).Any(g => g.Count() >= 3);

                var duplicates = usercardarray.GroupBy(x => x).Select(x => x.Key);

                var duplicatecards = usercards.GroupBy(x => x)
                                    .Where(g => g.Count() >= 3)
                                    .Select(g => g.Key);

                if (containsDuplicates == true)
                {
                    foreach (var item in duplicatecards)
                    {
                        foreach (KeyValuePair<string, List<string>> playerHand in playerHands)
                        {
                            foreach (var x in playerHand.Value)
                            {
                                if (item.Equals(x))
                                {
                                    playerWithIncorrect.Add(playerHand.Key);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, List<string>> playerHand in playerHands)
                    {
                        foreach (var item in playerHand.Value)
                        {
                            var x = CardHelper.GetBaseCardValue(item);

                            if (x == 0)
                            {
                                playerWithIncorrect.Add(playerHand.Key);
                            }
                        }
                        if (playerHand.Value.Count() != 5)
                        {
                            playerWithIncorrect.Add(playerHand.Key);
                        }
                    }
                }



                return playerWithIncorrect;
            }

        }

        public static class CardHelper
        {
            public static List<KeyValuePair<string, string>> FindHighestCard(string playername, string[] card)
            {
                List<string> list = card.Cast<string>().ToList();
                var userShapes1 = new List<KeyValuePair<string, string>>();
                var userNumbers = new List<KeyValuePair<string, string>>();
                var scores = new List<KeyValuePair<string, string>>();

                Dictionary<string, int> usertiedCard = new Dictionary<string, int>();

                foreach (var item in list)
                {
                    userShapes1.Add(new KeyValuePair<string, string>(playername, item));
                }

                foreach (var x in userShapes1)
                {
                    string cardValue = GetCardValue(x.Value);
                    scores.Add(new KeyValuePair<string, string>(playername, cardValue));
                }

                return scores;
            }

            public static string GetCardValue(string card)
            {
                switch (card.ToUpper())
                {
                    case "KD":
                        return 17.ToString() + "," + card;
                    case "KC":
                        return 16.ToString() + "," + card;
                    case "KS":
                        return 15.ToString() + "," + card;
                    case "KH":
                        return 14.ToString() + "," + card;
                    case "QD":
                        return 16.ToString() + "," + card;
                    case "QC":
                        return 15.ToString() + "," + card;
                    case "QS":
                        return 14.ToString() + "," + card;
                    case "QH":
                        return 13.ToString() + "," + card;
                    case "JD":
                        return 15.ToString() + "," + card;
                    case "JC":
                        return 14.ToString() + "," + card;
                    case "JS":
                        return 13.ToString() + "," + card;
                    case "JH":
                        return 12.ToString() + "," + card;
                    case "AD":
                        return 15.ToString() + "," + card;
                    case "AC":
                        return 14.ToString() + "," + card;
                    case "AS":
                        return 13.ToString() + "," + card;
                    case "AH":
                        return 12.ToString() + "," + card;
                    case "2D":
                        return 8.ToString() + "," + card;
                    case "2C":
                        return 5.ToString() + "," + card;
                    case "2S":
                        return 4.ToString() + "," + card;
                    case "2H":
                        return 3.ToString() + "," + card;
                    case "3D":
                        return 7.ToString() + "," + card;
                    case "3C":
                        return 6.ToString() + "," + card;
                    case "3S":
                        return 5.ToString() + "," + card;
                    case "3H":
                        return 4.ToString() + "," + card;
                    case "4D":
                        return 8.ToString() + "," + card;
                    case "4C":
                        return 7.ToString() + "," + card;
                    case "4S":
                        return 6.ToString() + "," + card;
                    case "4H":
                        return 5.ToString() + "," + card;
                    case "5D":
                        return 9.ToString() + "," + card;
                    case "5C":
                        return 8.ToString() + "," + card;
                    case "5S":
                        return 7.ToString() + "," + card;
                    case "5H":
                        return 6.ToString() + "," + card;
                    case "6D":
                        return 10.ToString() + "," + card;
                    case "6C":
                        return 9.ToString() + "," + card;
                    case "6S":
                        return 8.ToString() + "," + card;
                    case "6H":
                        return 7.ToString() + "," + card;
                    case "7D":
                        return 11.ToString() + "," + card;
                    case "7C":
                        return 10.ToString() + "," + card;
                    case "7S":
                        return 9.ToString() + "," + card;
                    case "7H":
                        return 8.ToString() + "," + card;
                    case "8D":
                        return 12.ToString() + "," + card;
                    case "8C":
                        return 11.ToString() + "," + card;
                    case "8S":
                        return 10.ToString() + "," + card;
                    case "8H":
                        return 9.ToString() + "," + card;
                    case "9D":
                        return 13.ToString() + "," + card;
                    case "9C":
                        return 12.ToString() + "," + card;
                    case "9S":
                        return 11.ToString() + "," + card;
                    case "9H":
                        return 10.ToString() + "," + card;
                    case "10D":
                        return 14.ToString() + "," + card;
                    case "10C":
                        return 13.ToString() + "," + card;
                    case "10S":
                        return 12.ToString() + "," + card;
                    case "10H":
                        return 11.ToString() + "," + card;
                    default:
                        return card;
                }
            }

            public static CardViewModel FindMax(IEnumerable<KeyValuePair<string, string>> lsd)
            {
                var cardviewmodel = new CardViewModel();
                cardviewmodel.Cards = new List<Card>();
                foreach (KeyValuePair<string, string> pair in lsd)
                {
                    string originalValue = pair.Value;
                    string shape = originalValue.Substring(pair.Value.IndexOf(",") + 1);
                    string number = originalValue.Substring(0, pair.Value.IndexOf(","));
                    cardviewmodel.Cards.Add(new Card()
                    {
                        Name = pair.Key,
                        Suit = shape,
                        Value = Convert.ToInt32(number)
                    });
                }

                return cardviewmodel;
            }


            public static CardViewModel dupcard(CardShapesViewModel lsd)
            {
                var cardviewmodel = new CardViewModel();
                cardviewmodel.Cards = new List<Card>();
                List<string> list = new List<string>();
                cardviewmodel.Cardshapes = new List<CardShape>();

                foreach (var pair in lsd.CardShapes)
                {
                    List<string> lst = pair.CardShape.Cast<string>().ToList();
                    cardviewmodel.Cardshapes.Add(new CardShape()
                    {
                        Suit = lst,
                        Name = pair.Name
                    });
                }

                return cardviewmodel;
            }

            public static int GetBaseCardValue(string suit)
            {
                switch (suit.ToUpper())
                {
                    case "KD":
                        return 4;
                    case "KC":
                        return 3;
                    case "KS":
                        return 2;
                    case "KH":
                        return 1;
                    case "QD":
                        return 4;
                    case "QC":
                        return 3;
                    case "QS":
                        return 2;
                    case "QH":
                        return 1;
                    case "JD":
                        return 4;
                    case "JC":
                        return 3;
                    case "JS":
                        return 2;
                    case "JH":
                        return 1;
                    case "AD":
                        return 4;
                    case "AC":
                        return 3;
                    case "AS":
                        return 2;
                    case "AH":
                        return 1;
                    case "2D":
                        return 4;
                    case "2C":
                        return 3;
                    case "2S":
                        return 2;
                    case "2H":
                        return 1;
                    case "3D":
                        return 4;
                    case "3C":
                        return 3;
                    case "3S":
                        return 2;
                    case "3H":
                        return 1;
                    case "4D":
                        return 4;
                    case "4C":
                        return 3;
                    case "4S":
                        return 2;
                    case "4H":
                        return 1;
                    case "5D":
                        return 4;
                    case "5C":
                        return 3;
                    case "5S":
                        return 2;
                    case "5H":
                        return 1;
                    case "6D":
                        return 4;
                    case "6C":
                        return 3;
                    case "6S":
                        return 2;
                    case "6H":
                        return 1;
                    case "7D":
                        return 4;
                    case "7C":
                        return 3;
                    case "7S":
                        return 2;
                    case "7H":
                        return 1;
                    case "8D":
                        return 4;
                    case "8C":
                        return 3;
                    case "8S":
                        return 2;
                    case "8H":
                        return 1;
                    case "9D":
                        return 4;
                    case "9C":
                        return 3;
                    case "9S":
                        return 2;
                    case "9H":
                        return 1;
                    case "10D":
                        return 4;
                    case "10C":
                        return 3;
                    case "10S":
                        return 2;
                    case "10H":
                        return 1;
                    default:
                        return 0;
                }
            }
        }
    }
    #endregion
}