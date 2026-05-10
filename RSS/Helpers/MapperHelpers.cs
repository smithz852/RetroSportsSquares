using RSS.DTOs;
using RSS_DB.Entities;
using RSS_Services.DTOs;
using System.Linq;

namespace RSS.Helpers
{
    public class MapperHelpers
    {
        public SquareGamesDTO AvailableGamesMapper(SquareGames availableGames)
        {

            return new SquareGamesDTO
            {
                GameId = availableGames.Id,
                GameName = availableGames.GameName,
                GameType = availableGames.GameType,
                CreatedAt = availableGames.CreatedAt,
                IsOpen = availableGames.isOpen,
                PlayerCount = availableGames.PlayerCount,
                PricePerSquare = availableGames.PricePerSquare,
                SportGameId = availableGames.DailySportGame.ApiGameId,
                HomeTeam = availableGames.DailySportGame.HomeTeam,
                AwayTeam = availableGames.DailySportGame.AwayTeam,
                StartTime = availableGames.DailySportGame.GameStartTime
            };
        }

        public AvailableSportsGamesOptionsDTO AvailableSportsGamesOptionsMapper(DailySportsGames dailySportsGameOption)
        {
            return new AvailableSportsGamesOptionsDTO
            {
                Id = dailySportsGameOption.Id,
                HomeTeam = dailySportsGameOption.HomeTeam,
                AwayTeam= dailySportsGameOption.AwayTeam,
                Status = dailySportsGameOption.Status,
            };
        }

        public SportScoreUpdateDTO ScoreDataMapper(SquareGames squaregame)
        {
            return new SportScoreUpdateDTO
            {
                HomeTeamName = squaregame.DailySportGame.HomeTeam,
                AwayTeamName = squaregame.DailySportGame.AwayTeam,
                CurrentHomeScore = squaregame.DailySportGame.CurrentHomeScore,
                CurrentAwayScore = squaregame.DailySportGame.CurrentAwayScore,
                Status = squaregame.DailySportGame.Status,
                Q1HomeScore = squaregame.DailySportGame.Q1HomeScore,
                Q1AwayScore = squaregame.DailySportGame.Q1AwayScore,
                Q2HomeScore = squaregame.DailySportGame.Q2HomeScore,
                Q2AwayScore = squaregame.DailySportGame.Q2AwayScore,
                Q3HomeScore = squaregame.DailySportGame.Q3HomeScore,
                Q3AwayScore = squaregame.DailySportGame.Q3AwayScore,
                Q4HomeScore = squaregame.DailySportGame.Q4HomeScore,
                Q4AwayScore = squaregame.DailySportGame.Q4AwayScore,
                OTHomeScore = squaregame.DailySportGame.OTHomeScore,
                OTAwayScore = squaregame.DailySportGame.OTAwayScore,
                WinnerQ1 = squaregame.WinnerQ1?.DisplayName,
                WinnerQ2 = squaregame.WinnerQ2?.DisplayName,
                WinnerQ3 = squaregame.WinnerQ3?.DisplayName,
                WinnerQ4 = squaregame.WinnerQ4?.DisplayName,
            };
        }

        public SelectedGamePlayerSquaresDTO SelectedGamePlayerSquaresMapper(GameSquares square)
        {
            var stringId = square.Id.ToString();
            var stringUserId = square.GamePlayerId.ToString();

            return new SelectedGamePlayerSquaresDTO()
            {
                Id = stringId,
                UserId = stringUserId,
                SelectedAt = square.CreatedAt,
            };

        }

        public SelectedSquaresByGameDTO SelectedSquaresByGameMapper(GameSquares squares)
        {
            var playerSquare = squares.GamePlayer.User.DisplayName;
            if (playerSquare == null) return null;
            return new SelectedSquaresByGameDTO()
            {
                displayName = squares.GamePlayer.User.DisplayName,
            };
        }

        public OutsideSquareNumbersDTO OutsideSquareMapper(SquareGames squareGames)
        {
            var gameSquareIdString = squareGames.Id.ToString();

            return new OutsideSquareNumbersDTO()
            {
                GameId = gameSquareIdString,
                TopNumbers = squareGames.TopNumbers,
                LeftNumbers = squareGames.LeftNumbers,
            };
        }

        public List<PreGameboardDTO> PreGameboardMapper(List<GameSquares> gameSquares)
        {
            return gameSquares.Select(square => new PreGameboardDTO
            {
                Id = square.Id,
                RowIndex = square.RowIndex,
                ColIndex = square.ColumnIndex,
                DisplayName = square.GamePlayer?.User?.DisplayName,
            }).ToList();
        }
    }
}
