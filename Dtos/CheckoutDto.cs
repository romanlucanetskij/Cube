namespace DripCube.Dtos
{
    public class CheckoutDto
    {
        public Guid UserId { get; set; }
        public string? PromoCode { get; set; }


        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }
}