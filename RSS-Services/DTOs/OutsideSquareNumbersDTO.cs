namespace RSS.DTOs
{
    public class OutsideSquareNumbersDTO
    {
        public List<OutsideSquareItem> OutsideSquares { get; set; } = new List<OutsideSquareItem>();
    }

    public class OutsideSquareItem
    {
        public string SquareName { get; set; }
        public int SquareValue { get; set; }
        public string Id { get; set; }
    }
}
