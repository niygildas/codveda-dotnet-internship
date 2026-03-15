using System;
using System.Collections.Generic;
using System.Linq;

namespace CodvedaOOP
{
    public interface IPayable { decimal CalculateSalary(); void DisplayPayInfo(); }
    public interface IReportable { string GenerateReport(); }

    public abstract class Employee : IPayable, IReportable
    {
        private string _name;
        private decimal _baseSalary;

        public string Name
        {
            get => _name;
            set { if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Name cannot be empty."); _name = value; }
        }
        public string EmployeeId { get; private set; }
        protected decimal BaseSalary
        {
            get => _baseSalary;
            set { if (value < 0) throw new ArgumentException("Salary cannot be negative."); _baseSalary = value; }
        }
        public string Department { get; set; }
        public DateTime HireDate { get; set; }

        protected Employee(string name, string id, decimal baseSalary, string dept)
        {
            Name = name; EmployeeId = id; BaseSalary = baseSalary; Department = dept; HireDate = DateTime.Now;
        }

        public abstract decimal CalculateSalary();

        public virtual void DisplayInfo()
        {
            Console.WriteLine($"\n  Employee  : {Name}");
            Console.WriteLine($"  ID        : {EmployeeId}");
            Console.WriteLine($"  Department: {Department}");
            Console.WriteLine($"  Salary    : ${CalculateSalary():N2}");
        }

        public virtual void DisplayPayInfo() => Console.WriteLine($"  {Name} earns ${CalculateSalary():N2}/month");
        public virtual string GenerateReport() => $"[{EmployeeId}] {Name} | {Department} | ${CalculateSalary():N2}";
    }

    public class FullTimeEmployee : Employee
    {
        public decimal Bonus { get; set; }
        public FullTimeEmployee(string name, string id, decimal salary, string dept, decimal bonus)
            : base(name, id, salary, dept) { Bonus = bonus; }
        public override decimal CalculateSalary() => BaseSalary + Bonus;
        public override void DisplayInfo() { base.DisplayInfo(); Console.WriteLine($"  Type: Full-Time | Bonus: ${Bonus:N2}"); }
    }

    public class PartTimeEmployee : Employee
    {
        public decimal HourlyRate { get; set; }
        public int HoursWorked { get; set; }
        public PartTimeEmployee(string name, string id, decimal rate, string dept, int hours)
            : base(name, id, 0, dept) { HourlyRate = rate; HoursWorked = hours; }
        public override decimal CalculateSalary() => HourlyRate * HoursWorked;
        public override void DisplayInfo() { base.DisplayInfo(); Console.WriteLine($"  Type: Part-Time | ${HourlyRate}/hr x {HoursWorked}hrs"); }
    }

    public class Contractor : Employee
    {
        public decimal DailyRate { get; set; }
        public int DaysWorked { get; set; }
        public string Agency { get; set; }
        public Contractor(string name, string id, decimal rate, string dept, int days, string agency)
            : base(name, id, 0, dept) { DailyRate = rate; DaysWorked = days; Agency = agency; }
        public override decimal CalculateSalary() => DailyRate * DaysWorked;
        public override void DisplayInfo() { base.DisplayInfo(); Console.WriteLine($"  Type: Contractor | Agency: {Agency} | ${DailyRate}/day x {DaysWorked}d"); }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║  Codveda HR System — C# OOP Demo     ║");
            Console.WriteLine("╚══════════════════════════════════════╝");

            var employees = new List<Employee>
            {
                new FullTimeEmployee("Gildas Niyonkuru", "EMP001", 3000, "Engineering", 500),
                new FullTimeEmployee("Alice Mutoni",     "EMP002", 2800, "Engineering", 400),
                new PartTimeEmployee("Bob Kariuki",      "EMP003", 25,   "Design",      80),
                new Contractor("Sara Okonkwo",           "EMP004", 200,  "Engineering", 20, "TechAgency")
            };

            Console.WriteLine("\n  ALL EMPLOYEES:");
            foreach (var e in employees) e.DisplayInfo();

            Console.WriteLine("\n  PAYROLL (Polymorphism Demo):");
            foreach (var e in employees) e.DisplayPayInfo();

            Console.WriteLine("\n  REPORTS (Interface Demo):");
            foreach (IReportable r in employees) Console.WriteLine("  " + r.GenerateReport());

            Console.WriteLine($"\n  Total Payroll: ${employees.Sum(e => e.CalculateSalary()):N2}");
            Console.ReadKey();
        }
    }
}
