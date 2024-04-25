using System;
using MyCourse.Models.Enums;

namespace MyCourse.Models.ValueObjects
{
    /*
     * Questa classe "Money" è un object value che rappresenta una cifra
     * monetaria.Ha una variabile privata "amount" per contenere la quantità di 
       denaro e una proprietà "Currency" di tipo "Currency" 
       (che è un Enum in un altro file).La classe ha due costruttori: 
       il costruttore senza parametri imposta i valori predefiniti di 
       "Valuta" e "Importo" rispettivamente su "EUR" e "0,00 milioni". 
       Il secondo costruttore accetta "Currency" e "Amount".Il metodo "Equals" 
       controlla se l'oggetto passato è un'istanza "Money" e 
       se ha gli stessi valori "Amount" e "Currency". "GetHashCode" 
       restituisce un codice hash per questa istanza e il metodo "ToString" 
       restituisce una stringa che rappresenta la "Valuta" e l'"Importo".Puoi vedere questa classe "Money" 
       utilizzata come proprietà "FullPrice" e "CurrentPrice" nella classe 
       "CourseViewModel".
     */
    public class Money
    {
        public Money() : this(Currency.EUR, 0.00m)
        {
        }
        public Money(Currency currency, decimal amount)
        {
            Amount = amount;
            Currency = currency;
        }
        private decimal amount = 0;
        public decimal Amount
        { 
            get
            {
                return amount;
            }
            set
            {
                if (value < 0) {
                    throw new InvalidOperationException("The amount cannot be negative");
                }
                amount = value;
            }
        }
        public Currency Currency
        {
            get; set;
        }

        public override bool Equals(object obj)
        {
            var money = obj as Money;
            return money != null &&
                   Amount == money.Amount &&
                   Currency == money.Currency;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }
        
        public override string ToString()
        {
            return $"{Currency} {Amount:#0.00}";
        }
        
    }
}