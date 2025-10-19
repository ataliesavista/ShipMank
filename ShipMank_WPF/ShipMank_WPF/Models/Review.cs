using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Review
    {
        public int ReviewID { get; private set; }
        public int UserID { get; private set; }
        public int KapalID { get; private set; }
        public int Rating { get; private set; }
        public DateTime DateReview { get; private set; }

        public User User { get; private set; }
        public Kapal Kapal { get; private set; }

        public Review(int reviewID, int userID, int kapalID, int rating, User user, Kapal kapal)
        {
            ReviewID = reviewID;
            UserID = userID;
            KapalID = kapalID;
            Rating = ValidateRating(rating);
            DateReview = DateTime.Now;
            User = user;
            Kapal = kapal;
        }

        private int ValidateRating(int rating) => rating < 1 ? 1 : (rating > 5 ? 5 : rating);

        public bool Add()
        {
            Kapal?.Reviews.Add(this);
            Kapal?.UpdateRating();
            User?.Reviews.Add(this);
            return true;
        }

        public bool Delete()
        {
            Kapal?.Reviews.Remove(this);
            Kapal?.UpdateRating();
            User?.Reviews.Remove(this);
            return true;
        }

        public bool Update(int newRating)
        {
            Rating = ValidateRating(newRating);
            Kapal?.UpdateRating();
            return true;
        }

        public string TampilkanDetail()
        {
            string stars = new string('★', Rating) + new string('☆', 5 - Rating);

            return $"Review ID: {ReviewID}\n" +
                   $"User: {User?.Name ?? "Unknown"}\n" +
                   $"Kapal: {Kapal?.NamaKapal ?? "Unknown"}\n" +
                   $"Rating: {Rating}/5\n" +
                   $"Rating Visual: {stars}\n" +
                   $"Tanggal: {DateReview:dd/MM/yyyy HH:mm}";
        }

        public override string ToString() => $"Review #{ReviewID} - {Rating}/5 stars by {User?.Name ?? "Unknown"}";
    }
}
