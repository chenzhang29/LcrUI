namespace LcrSimulation
{
    public static class Simulator
    {
        private static Random random = new Random();

        public static SimulationResult SimulateGame(int numOfPlayers)
        {
            // create array to represent initial state for players
            // initially each player gets 3 chips, and center with no chips
            var playersChipCount = Enumerable.Repeat(3, numOfPlayers).ToArray();
            var centerChipCount = 0;

            var totalGameRounds = 0;
            int gameWinnerNumber;
            do
            {
                totalGameRounds++;
                gameWinnerNumber = SimulateRound(playersChipCount, ref centerChipCount);
            }
            while (gameWinnerNumber < 0);

            return new SimulationResult() { GameWinnerNumber = gameWinnerNumber, TotalGameRounds = totalGameRounds };
        }

        /// <summary>
        /// returns the integer representing the player who won the game
        /// returns -1 if the round ended without a winner
        /// </summary>
        private static int SimulateRound(int[] playersChipCount, ref int centerChipCount)
        {
            var totalChipCount = 3 * playersChipCount.Length;

            for (int i = 0; i < playersChipCount.Length; i++)
            {
                // if player's current chip count and center chip count account for all existing chips
                // then non of the other players has any chips and current player is the winner
                if (playersChipCount[i] + centerChipCount == totalChipCount)
                    return i;

                // player can roll at most 3 dice
                // if player has less than 3 chips, then number of dice roll equals player's chip count
                var timesToRollDice = Math.Min(3, playersChipCount[i]);

                for (int d = 0; d < timesToRollDice; d++)
                {
                    // simulate dice roll, a random number from 0-5
                    // 0: L, 1: C, 2: R, 3 to 5: dot
                    var dieValue = random.Next(6);

                    // die landed on dot, do nothing
                    if (dieValue > 2)
                        continue;

                    // player loses a chip
                    playersChipCount[i]--;

                    if (dieValue == 0) // left
                    {
                        var leftIndex = (i == 0) ? playersChipCount.Length - 1 : i - 1;
                        playersChipCount[leftIndex]++;

                        // note: we could also do a check backwards for the case
                        // when the previous player happened to get all the chips in the current round then the game ends
                        // but not sure if this technicality is explicitly stated in the rules
                        //if (playersChipCount[leftIndex] + centerChipCount == totalChipCount)
                        //    return leftIndex;
                    }
                    else if (dieValue == 1) // center
                    {
                        centerChipCount++;
                    }
                    else if (dieValue == 2) // right
                    {
                        var rightIndex = (i == playersChipCount.Length - 1) ? 0 : i + 1;
                        playersChipCount[rightIndex]++;
                    }
                }
            }

            return -1;
        }
    }
}
