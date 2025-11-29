using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    // 1. ABSTRACTION (INTERFACE)
    // Kontrak apa yang harus dimiliki oleh sebuah transaksi
    public interface ITransaction
    {
        int ID { get; }
        DateTime DateCreated { get; }
        bool ProcessTransaction();
        string GetDetail();
    }

    // 2. INHERITANCE (BASE CLASS)
    // Implementasi dasar untuk mengurangi duplikasi kode
    public abstract class TransactionBase : ITransaction
    {
        // ENCAPSULATION: Protected set agar hanya child yang bisa ubah
        public int ID { get; protected set; }
        public DateTime DateCreated { get; protected set; }

        protected TransactionBase()
        {
            DateCreated = DateTime.Now;
        }

        // ABSTRACT METHOD (Polymorphism)
        // Setiap child class WAJIB punya logic prosesnya sendiri
        public abstract bool ProcessTransaction();

        // VIRTUAL METHOD (Polymorphism)
        // Child class BISA override ini jika butuh detail khusus
        public virtual string GetDetail()
        {
            return $"ID: {ID} | Date: {DateCreated:dd/MM/yyyy HH:mm}";
        }
    }
}
