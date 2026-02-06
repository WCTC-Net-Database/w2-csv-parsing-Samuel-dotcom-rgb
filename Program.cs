using System;
using System.IO;
// Like the start of it.
class Program
{
    static string[] lines;
    static string filePath = "input.csv";

    static void Main()
    {
        lines = File.ReadAllLines(filePath);

        while (true)
        {
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Display Characters");
            Console.WriteLine("2. Add Character");
            Console.WriteLine("3. Level Up Character");
            Console.WriteLine("4. Edit Equipment");
            Console.WriteLine("5. Exit");
            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    DisplayAllCharacters(lines);
                    break;
                case "2":
                    AddCharacter(ref lines);
                    break;
                case "3":
                    LevelUpCharacter();
                    break;
                case "4":
                    EditEquipment();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static void DisplayAllCharacters(string[] lines)
    {
        // Skip the header row
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            var parsed = ParseCsvLine(line);
            Console.WriteLine($"--- Character #{i} ---");
            Console.WriteLine($"Name: {parsed.name}");
            Console.WriteLine($"Class: {parsed.characterClass}");
            Console.WriteLine($"Level: {parsed.level}");
            Console.WriteLine($"HP: {parsed.hitPoints}");
            if (parsed.equipment.Length == 0)
            {
                Console.WriteLine("Equipment: (none)");
            }
            else
            {
                Console.WriteLine("Equipment:");
                for (int j = 0; j < parsed.equipment.Length; j++)
                {
                    Console.WriteLine($"  {j + 1}. {parsed.equipment[j]}");
                }
            }
        }
    }

    static void AddCharacter(ref string[] lines)
    {
        Console.Write("Enter name: ");
        string name = Console.ReadLine();

        Console.Write("Enter class: ");
        string characterClass = Console.ReadLine();

        int level = 1;
        while (true)
        {
            Console.Write("Enter level (integer): ");
            if (int.TryParse(Console.ReadLine(), out level)) break;
            Console.WriteLine("Invalid number, try again.");
        }

        int hitPoints = 0;
        while (true)
        {
            Console.Write("Enter hit points (integer): ");
            if (int.TryParse(Console.ReadLine(), out hitPoints)) break;
            Console.WriteLine("Invalid number, try again.");
        }

        var equipmentList = new System.Collections.Generic.List<string>();
        Console.WriteLine("Enter equipment items one at a time. Leave blank to finish.");
        while (true)
        {
            Console.Write("Add equipment: ");
            string item = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(item)) break;
            equipmentList.Add(item);
        }

        string newLine = BuildCsvLine(name, characterClass, level, hitPoints, equipmentList.ToArray());

        // Append preserving header at index 0
        var newLines = new string[lines.Length + 1];
        for (int i = 0; i < lines.Length; i++) newLines[i] = lines[i];
        newLines[newLines.Length - 1] = newLine;
        lines = newLines;
        Program.lines = lines;
        SaveLines();
        Console.WriteLine("Character added and saved.");
    }

    static void LevelUpCharacter()
    {
        Console.Write("Enter the name of the character to level up: ");
        string nameToLevelUp = Console.ReadLine();

        for (int i = 1; i < lines.Length; i++)
        {
            var parsed = ParseCsvLine(lines[i]);
            if (string.Equals(parsed.name, nameToLevelUp, StringComparison.OrdinalIgnoreCase))
            {
                parsed.level++;
                lines[i] = BuildCsvLine(parsed.name, parsed.characterClass, parsed.level, parsed.hitPoints, parsed.equipment);
                SaveLines();
                Console.WriteLine($"Character {parsed.name} leveled up to level {parsed.level}!");
                return;
            }
        }
        Console.WriteLine("Character not found.");
    }

    static void EditEquipment()
    {
        Console.Write("Enter the name of the character to edit equipment: ");
        string target = Console.ReadLine();
        for (int i = 1; i < lines.Length; i++)
        {
            var parsed = ParseCsvLine(lines[i]);
            if (string.Equals(parsed.name, target, StringComparison.OrdinalIgnoreCase))
            {
                var list = new System.Collections.Generic.List<string>(parsed.equipment);
                while (true)
                {
                    Console.WriteLine($"Editing equipment for {parsed.name}. Current items:");
                    for (int j = 0; j < list.Count; j++) Console.WriteLine($"  {j + 1}. {list[j]}");
                    Console.WriteLine("Options: (a)dd, (r)emove, (q)uit");
                    Console.Write("Choice: ");
                    string c = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(c)) continue;
                    c = c.ToLower();
                    if (c == "a")
                    {
                        Console.Write("Item to add: ");
                        string it = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(it)) list.Add(it);
                    }
                    else if (c == "r")
                    {
                        Console.Write("Index to remove: ");
                        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= list.Count)
                        {
                            list.RemoveAt(idx - 1);
                        }
                        else Console.WriteLine("Invalid index.");
                    }
                    else if (c == "q")
                    {
                        // save and exit
                        lines[i] = BuildCsvLine(parsed.name, parsed.characterClass, parsed.level, parsed.hitPoints, list.ToArray());
                        SaveLines();
                        Console.WriteLine("Equipment updated and saved.");
                        return;
                    }
                }
            }
        }
        Console.WriteLine("Character not found.");
    }

    static void SaveLines()
    {
        File.WriteAllLines(filePath, lines);
    }

    static (string name, string characterClass, int level, int hitPoints, string[] equipment) ParseCsvLine(string line)
    {
        string name = "";
        int pos = 0;
        if (line.StartsWith("\""))
        {
            int i = 1;
            while (i < line.Length)
            {
                if (line[i] == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        i += 2; // escaped quote
                        continue;
                    }
                    else break; // closing quote
                }
                i++;
            }
            int closing = i;
            if (closing > 0)
            {
                name = line.Substring(1, closing - 1).Replace("\"\"", "\"");
                pos = closing + 1; // position of comma
                if (pos < line.Length && line[pos] == ',') pos++;
            }
        }
        else
        {
            int comma = line.IndexOf(',');
            if (comma == -1) comma = line.Length;
            name = line.Substring(0, comma);
            pos = comma + 1;
        }

        string rest = pos <= line.Length ? line.Substring(pos) : "";
        string[] parts = rest.Split(new char[] { ',' }, 4);
        string characterClass = parts.Length > 0 ? parts[0] : "";
        int level = 0;
        if (parts.Length > 1) int.TryParse(parts[1], out level);
        int hitPoints = 0;
        if (parts.Length > 2) int.TryParse(parts[2], out hitPoints);
        string equipmentField = parts.Length > 3 ? parts[3] : "";
        string[] equipment = string.IsNullOrWhiteSpace(equipmentField) ? new string[0] : equipmentField.Split('|');
        return (name, characterClass, level, hitPoints, equipment);
    }

    static string BuildCsvLine(string name, string characterClass, int level, int hitPoints, string[] equipment)
    {
        string outName = name ?? "";
        if (outName.Contains("\"")) outName = outName.Replace("\"", "\"\"");
        if (outName.Contains(",") || outName.Contains("\"") || outName.Contains("\n")) outName = "\"" + outName + "\"";
        string equipField = (equipment == null || equipment.Length == 0) ? "" : string.Join("|", equipment);
        return $"{outName},{characterClass},{level},{hitPoints},{equipField}";
    }
}