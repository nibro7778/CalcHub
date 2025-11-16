namespace CalcHub.Domain
{
    public record Money
    {
        public decimal Amount { get; init; }
        public string Currency { get; init; }

        public Money(decimal amount, string currency = "AUD")
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot add money with different currencies");

            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot subtract money with different currencies");

            return new Money(a.Amount - b.Amount, a.Currency);
        }

        public static Money operator *(Money a, decimal multiplier)
        {
            return new Money(a.Amount * multiplier, a.Currency);
        }

        public override string ToString() => $"${Amount:N2} {Currency}";
    }
}
