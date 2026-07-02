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
                CurrentPlayerCount = availableGames.GamePlayers.Count,
                PricePerSquare = availableGames.PricePerSquare,
                SquareSelectionLimit = availableGames.SquareSelectionLimit,
                HostUserId = availableGames.GamePlayers.FirstOrDefault(p => p.IsHost)?.ApplicationUserId,
                IsTurnBased = availableGames.IsTurnBased,
                SelectionPhaseActive = availableGames.SelectionPhaseActive,
                CurrentTurnUserId = availableGames.CurrentTurnUserId,
                TurnTimeoutSeconds = availableGames.TurnTimeoutSeconds,
                TurnStartedAt = availableGames.TurnStartedAt,
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

        public SportScoreUpdateDTO ScoreDataMapper(SquareGames squaregame, Dictionary<int, string?> periodWinnerNames, decimal payoutPerPeriod)
        {
            return new SportScoreUpdateDTO
            {
                HomeTeamName = squaregame.DailySportGame.HomeTeam,
                AwayTeamName = squaregame.DailySportGame.AwayTeam,
                CurrentHomeScore = squaregame.DailySportGame.CurrentHomeScore,
                CurrentAwayScore = squaregame.DailySportGame.CurrentAwayScore,
                Status = squaregame.DailySportGame.Status,
                SportType = squaregame.DailySportGame.SportType,
                HomePeriodScores = squaregame.DailySportGame.HomePeriodScores,
                AwayPeriodScores = squaregame.DailySportGame.AwayPeriodScores,
                PeriodWinners = periodWinnerNames,
                PayoutPerPeriod = payoutPerPeriod,
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
            var tag = squares.GamePlayer.User.GamerTag ?? squares.GamePlayer.User.DisplayName;
            if (tag == null) return null;
            return new SelectedSquaresByGameDTO()
            {
                displayName = tag,
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
                DisplayName = square.GamePlayer?.User?.GamerTag ?? square.GamePlayer?.User?.DisplayName,
            }).ToList();
        }
    }
}
