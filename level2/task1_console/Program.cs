using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CodvedaTaskManager
{
    public enum Priority { Low, Medium, High, Critical }
    public enum TaskStatus { Pending, InProgress, Completed, Cancelled }

    public class TodoTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Priority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && Status != TaskStatus.Completed;
    }

    class Program
    {
        static List<TodoTask> tasks = new();
        static int nextId = 1;
        const string FILE = "tasks.json";

        static void Main(string[] args)
        {
            Load();

            // Command-line mode
            if (args.Length > 0)
            {
                if (args[0] == "--list") { tasks.ForEach(PrintTask); return; }
                if (args[0] == "--add" && args.Length > 1) { Add(args[1]); return; }
                if (args[0] == "--overdue") { tasks.Where(t => t.IsOverdue).ToList().ForEach(PrintTask); return; }
                Console.WriteLine("Usage: --list | --add \"title\" | --overdue"); return;
            }

            // Interactive menu
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔══════════════════════════════════╗");
                Console.WriteLine("║    Codveda Task Manager v1.0     ║");
                Console.WriteLine("╚══════════════════════════════════╝");
                Console.WriteLine(" [1] View Tasks  [2] Add  [3] Update Status");
                Console.WriteLine(" [4] Delete      [5] Stats [0] Exit");
                Console.Write(" Choose: ");

                switch (Console.ReadKey(true).KeyChar)
                {
                    case '1': Console.WriteLine("1"); ViewAll(); break;
                    case '2': Console.WriteLine("2"); AddInteractive(); break;
                    case '3': Console.WriteLine("3"); UpdateStatus(); break;
                    case '4': Console.WriteLine("4"); Delete(); break;
                    case '5': Console.WriteLine("5"); Stats(); break;
                    case '0': Console.WriteLine("\nGoodbye!"); return;
                }
                Console.Write("\nPress any key..."); Console.ReadKey();
            }
        }

        static void ViewAll()
        {
            Console.WriteLine($"\n  {"ID",-5} {"Title",-25} {"Priority",-10} {"Status",-12}");
            Console.WriteLine($"  {"─",5} {"─",25} {"─",10} {"─",12}");
            foreach (var t in tasks.OrderByDescending(t => t.Priority))
            {
                if (t.IsOverdue) Console.ForegroundColor = ConsoleColor.Red;
                else if (t.Status == TaskStatus.Completed) Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  [{t.Id:D3}] {t.Title,-25} {t.Priority,-10} {t.Status,-12} {(t.IsOverdue ? "⚠OVERDUE" : "")}");
                Console.ResetColor();
            }
        }

        static void AddInteractive()
        {
            Console.Write("\n  Title: "); var title = Console.ReadLine();
            Console.Write("  Priority (0=Low,1=Medium,2=High,3=Critical): ");
            Enum.TryParse<Priority>(Console.ReadLine(), out var p);
            Add(title, p);
        }

        static void Add(string title, Priority p = Priority.Medium)
        {
            tasks.Add(new TodoTask { Id = nextId++, Title = title, Priority = p, Status = TaskStatus.Pending, CreatedAt = DateTime.Now });
            Save();
            Console.WriteLine($"\n  ✅ Task added: {title}");
        }

        static void UpdateStatus()
        {
            Console.Write("\n  Task ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            var t = tasks.FirstOrDefault(x => x.Id == id);
            if (t == null) { Console.WriteLine("  ❌ Not found"); return; }
            Console.Write("  Status (0=Pending,1=InProgress,2=Completed,3=Cancelled): ");
            Enum.TryParse<TaskStatus>(Console.ReadLine(), out var s);
            t.Status = s; Save();
            Console.WriteLine($"  ✅ Updated to {s}");
        }

        static void Delete()
        {
            Console.Write("\n  Task ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            bool ok = tasks.RemoveAll(t => t.Id == id) > 0;
            if (ok) Save();
            Console.WriteLine(ok ? "  ✅ Deleted" : "  ❌ Not found");
        }

        static void Stats()
        {
            Console.WriteLine($"\n  Total: {tasks.Count}");
            Console.WriteLine($"  Overdue: {tasks.Count(t => t.IsOverdue)}");
            foreach (var g in tasks.GroupBy(t => t.Status))
                Console.WriteLine($"  {g.Key}: {g.Count()}");
        }

        static void PrintTask(TodoTask t) => Console.WriteLine($"[{t.Id:D3}] {t.Title,-25} {t.Priority,-10} {t.Status}");
        static void Save() => File.WriteAllText(FILE, JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true }));
        static void Load()
        {
            if (!File.Exists(FILE)) return;
            tasks = JsonSerializer.Deserialize<List<TodoTask>>(File.ReadAllText(FILE)) ?? new();
            nextId = tasks.Any() ? tasks.Max(t => t.Id) + 1 : 1;
        }
    }
}
