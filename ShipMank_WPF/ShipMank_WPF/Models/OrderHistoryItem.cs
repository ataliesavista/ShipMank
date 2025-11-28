using System;
using System.Windows;

namespace ShipMank_WPF.Models
{
    public class OrderHistoryItem
    {
        public string OrderID { get; set; }
        public int OriginalBookingID { get; set; }
        public int KapalID { get; set; }
        public string ItemName { get; set; }
        public string ShipType { get; set; }
        public DateTime Date { get; set; }
        public string Route { get; set; }
        public string TotalString { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public bool IsRated { get; set; }

        public Visibility RateButtonVisibility =>
            (Status == "Completed") ? Visibility.Visible : Visibility.Collapsed;
    }
}