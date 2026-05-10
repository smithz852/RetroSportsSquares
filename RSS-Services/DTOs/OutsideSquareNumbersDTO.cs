namespace RSS.DTOs
{
    //public class OutsideSquareNumbersDTO
    //{
    //    public List<OutsideSquareItem> OutsideSquares { get; set; } = new List<OutsideSquareItem>();
    //}

    public class OutsideSquareNumbersDTO
    {
        public string GameId { get; set; }
        public List<int> TopNumbers { get; set; }
        public List<int> LeftNumbers { get; set; }
    }
}
