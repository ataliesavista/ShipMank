using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Review
    {
        public int ReviewID { get; set; }
        public int BookingID { get; set; }
        public Booking Booking { get; set; }
        public int RatingValue { get; set; }
        public DateTime DateReview { get; set; }

        public Review(int bookingID, int rating)
        {
            BookingID = bookingID;
            RatingValue = ValidateRating(rating);
            DateReview = DateTime.Now;
        }

        private int ValidateRating(int rating) => rating < 1 ? 1 : (rating > 5 ? 5 : rating);

        public string TampilkanDetail()
        {
            string stars = new string('★', RatingValue) + new string('☆', 5 - RatingValue);
            string userDisplay = Booking?.User?.Name ?? "Unknown";
            string kapalDisplay = Booking?.Kapal?.NamaKapal ?? "Unknown";

            return $"Review ID: {ReviewID}\n" +
                   $"User: {userDisplay}\n" +
                   $"Kapal: {kapalDisplay}\n" +
                   $"Rating: {RatingValue}/5\n" +
                   $"Visual: {stars}\n" +
                   $"Tanggal: {DateReview:dd/MM/yyyy HH:mm}";
        }
    }
}
