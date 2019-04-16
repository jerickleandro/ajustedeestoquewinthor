

namespace AjustaEstoques.Entities
{
    class Product
    {
        public int Codprod { get; set; }
        public decimal Qtestger { get; set; }
        public decimal Giro { get; set; }
        public decimal SaldoFinal { get; set; }

        public Product(int codprod, decimal qtestger, decimal giro, decimal saldoFinal)
        {
            Codprod = codprod;
            Qtestger = qtestger;
            Giro = giro;
            SaldoFinal = saldoFinal;
        }


        public override string ToString()
        {
            return Codprod
                + " - "
                + Qtestger
                + " - "
                + Giro
                + " - "
                + SaldoFinal;
                
        }
    }
}
