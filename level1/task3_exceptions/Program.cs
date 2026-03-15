using System;
using System.IO;

namespace CodvedaExceptions
{
    // Custom exceptions
    public class InsufficientFundsException : Exception
    {
        public decimal Balance { get; }
        public decimal Amount { get; }
        public InsufficientFundsException(decimal bal, decimal amt)
            : base($"Insufficient funds. Balance: ${bal:N2}, Attempted: ${amt:N2}") { Balance = bal; Amount = amt; }
    }

    public class AccountNotFoundException : Exception
    {
        public AccountNotFoundException(string id) : base($"Account '{id}' not found.") { }
    }

    public class InvalidTransactionException : Exception
    {
        public InvalidTransactionException(string type, string reason)
            : base($"Invalid {type}: {reason}") { }
    }

    public class BankAccount
    {
        public string Id { get; }
        public string Owner { get; }
        private decimal _balance;
        public decimal Balance => _balance;

        public BankAccount(string id, string owner, decimal initial)
        {
            if (initial < 0) throw new InvalidTransactionException("deposit", "Initial balance cannot be negative.");
            Id = id; Owner = owner; _balance = initial;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0) throw new InvalidTransactionException("deposit", "Amount must be > 0.");
            _balance += amount;
            Console.WriteLine($"  [LOG] Deposit ${amount:N2} → {Id}. Balance: ${_balance:N2}");
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0) throw new InvalidTransactionException("withdrawal", "Amount must be > 0.");
            if (amount > _balance) throw new InsufficientFundsException(_balance, amount);
            _balance -= amount;
            Console.WriteLine($"  [LOG] Withdrawal ${amount:N2} ← {Id}. Balance: ${_balance:N2}");
        }

        public void Transfer(BankAccount target, decimal amount)
        {
            Withdraw(amount);
            target.Deposit(amount);
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║  Codveda Bank — Exception Handling   ║");
            Console.WriteLine("╚══════════════════════════════════════╝\n");

            var acc1 = new BankAccount("ACC001", "Gildas", 1000);
            var acc2 = new BankAccount("ACC002", "Alice", 500);

            RunTest("Valid withdrawal", () => acc1.Withdraw(200));
            RunTest("Insufficient funds", () => acc1.Withdraw(5000));
            RunTest("Invalid amount", () => acc1.Withdraw(-50));
            RunTest("Valid transfer", () => acc1.Transfer(acc2, 100));
            RunTest("Save report", () => SaveReport(acc1, acc2));

            Console.WriteLine($"\n  Final Balances:");
            Console.WriteLine($"  ACC001: ${acc1.Balance:N2}");
            Console.WriteLine($"  ACC002: ${acc2.Balance:N2}");
            Console.ReadKey();
        }

        static void RunTest(string name, Action action)
        {
            Console.WriteLine($"\n--- Test: {name} ---");
            try
            {
                action();
                Console.WriteLine("  ✅ Success");
            }
            catch (InsufficientFundsException ex) { Console.WriteLine($"  ❌ {ex.Message}"); }
            catch (InvalidTransactionException ex) { Console.WriteLine($"  ❌ {ex.Message}"); }
            catch (AccountNotFoundException ex)   { Console.WriteLine($"  ❌ {ex.Message}"); }
            catch (Exception ex)                  { Console.WriteLine($"  💥 Unexpected: {ex.Message}"); throw; }
            finally { Console.WriteLine("  📋 Audit log written."); }
        }

        static void SaveReport(params BankAccount[] accounts)
        {
            using var writer = new StreamWriter("bank_report.txt", append: true);
            writer.WriteLine($"=== Report {DateTime.Now} ===");
            foreach (var a in accounts)
                writer.WriteLine($"{a.Id} | {a.Owner} | ${a.Balance:N2}");
        }
    }
}
