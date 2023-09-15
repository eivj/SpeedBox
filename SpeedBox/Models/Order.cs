namespace SpeedBox.Models
{
    public class Order
    {
        public byte currency { get; set; }
        public List<packages> packages { get; set; }
        public ToLocation to_location { get; set; }
        public FromLocation from_location { get; set; }
    }
}
