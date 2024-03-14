using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

public class Seat
{
    public int Row { get; set; }
    public int Number { get; set; }
    public bool IsOccupied { get; set; }

    public Seat() { }

    public Seat(int row, int number)
    {
        Row = row;
        Number = number;
        IsOccupied = false;
    }
}

[Serializable]
public class Booking
{
    public string Name { get; set; }
    public List<Seat> Seats { get; set; }

    public Booking() { }

    public Booking(string name, List<Seat> seats)
    {
        Name = name;
        Seats = seats;
    }

    public override string ToString()
    {
        return $"Foglalás Név: {Name}, Helyek: {string.Join(", ", Seats.Select(s => $"Sor: {s.Row}, Szék: {s.Number}"))}";
    }
}

public class Program
{
    const int Rows = 16;
    const int SeatsPerRow = 15;
    const double RandomOccupiedPercentage = 0.1;
    const string DataFileName = "booking_data.xml";

    static bool[,] seats = new bool[Rows, SeatsPerRow];
    static List<Booking> bookings = new List<Booking>();

    public static void Main()
    {
        LoadData();

        while (true)
        {
            Console.WriteLine("----- Színházi Jegy Értékesítő Program -----");
            Console.WriteLine("1. Szabad és foglalt székek jelzése");
            Console.WriteLine("2. Szabad helyek választása");
            Console.WriteLine("3. Foglalás módosítása");
            Console.WriteLine("4. Foglalás törlése");
            Console.WriteLine("5. Foglalás végrehajtása");
            Console.WriteLine("6. Kilépés");

            Console.Write("Válasszon menüpontot (1-6): ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    DisplaySeatStatus();
                    break;
                case "2":
                    ChooseAvailableSeats();
                    break;
                case "3":
                    ModifyBooking();
                    break;
                case "4":
                    CancelBooking();
                    break;
                case "5":
                    ExecuteBookings();
                    break;
                case "6":
                    SaveData();
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Érvénytelen választás. Kérem, válasszon újra.");
                    break;
            }

            Console.WriteLine();
        }
    }

    static void DisplaySeatStatus()
    {
        Console.WriteLine("----- Színház nézőtere -----");
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < SeatsPerRow; j++)
            {
                Console.Write(seats[i, j] ? "X" : "-");
            }
            Console.WriteLine();
        }
    }

    static void ChooseAvailableSeats()
    {
        Console.Write("Hány szabad helyet szeretne választani? ");
        int numSeats = int.Parse(Console.ReadLine());

        List<Seat> availableSeats = GetAvailableSeats();
        if (numSeats > availableSeats.Count)
        {
            Console.WriteLine($"Nincs ennyi szabad hely. Jelenleg csak {availableSeats.Count} szabad hely van.");
            return;
        }

        Console.WriteLine("Elérhető szabad helyek:");
        for (int i = 0; i < availableSeats.Count; i++)
        {
            Console.WriteLine($"{i + 1}. Sor: {availableSeats[i].Row}, Szék: {availableSeats[i].Number}");
        }

        Console.Write("Válasszon helyeket (pl.: 1,3,5): ");
        string selectedSeatsInput = Console.ReadLine();
        string[] selectedSeatsArray = selectedSeatsInput.Split(',');

        List<Seat> selectedSeats = new List<Seat>();
        foreach (string seatIndex in selectedSeatsArray)
        {
            int index = int.Parse(seatIndex) - 1;
            selectedSeats.Add(availableSeats[index]);
        }

        Console.Write("Adja meg a nevét: ");
        string name = Console.ReadLine();

        bookings.Add(new Booking(name, selectedSeats));

        foreach (Seat seat in selectedSeats)
        {
            seats[seat.Row - 1, seat.Number - 1] = true;
        }

        Console.WriteLine("Helyek sikeresen foglalva!");
    }

    static void ModifyBooking()
    {
        Console.Write("Adja meg a módosítani kívánt foglalás nevét: ");
        string name = Console.ReadLine();

        Booking booking = bookings.FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (booking == null)
        {
            Console.WriteLine("Nincs ilyen névvel rendelkező foglalás.");
            return;
        }

        Console.WriteLine($"Régi foglalás: {booking}");

        Console.Write("Hány szabad helyet szeretne választani a módosított foglaláshoz? ");
        int numSeats = int.Parse(Console.ReadLine());

        List<Seat> availableSeats = GetAvailableSeats();
        if (numSeats > availableSeats.Count)
        {
            Console.WriteLine($"Nincs ennyi szabad hely. Jelenleg csak {availableSeats.Count} szabad hely van.");
            return;
        }

        Console.WriteLine("Elérhető szabad helyek:");
        for (int i = 0; i < availableSeats.Count; i++)
        {
            Console.WriteLine($"{i + 1}. Sor: {availableSeats[i].Row}, Szék: {availableSeats[i].Number}");
        }

        Console.Write("Válasszon helyeket (pl.: 1,3,5): ");
        string selectedSeatsInput = Console.ReadLine();
        string[] selectedSeatsArray = selectedSeatsInput.Split(',');

        List<Seat> selectedSeats = new List<Seat>();
        foreach (string seatIndex in selectedSeatsArray)
        {
            int index = int.Parse(seatIndex) - 1;
            selectedSeats.Add(availableSeats[index]);
        }

        booking.Seats = selectedSeats;

        Console.WriteLine("Foglalás sikeresen módosítva!");
    }

    static void CancelBooking()
    {
        Console.Write("Adja meg a törölni kívánt foglalás nevét: ");
        string name = Console.ReadLine();

        Booking booking = bookings.FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (booking == null)
        {
            Console.WriteLine("Nincs ilyen névvel rendelkező foglalás.");
            return;
        }

        bookings.Remove(booking);

        foreach (Seat seat in booking.Seats)
        {
            seats[seat.Row - 1, seat.Number - 1] = false;
        }

        Console.WriteLine("Foglalás sikeresen törölve!");
    }

    static void ExecuteBookings()
    {
        if (bookings.Count == 0)
        {
            Console.WriteLine("Nincs elmentett foglalás.");
            return;
        }

        Console.WriteLine("Elmentett foglalások:");
        foreach (Booking booking in bookings)
        {
            Console.WriteLine(booking);
        }
    }

    static List<Seat> GetAvailableSeats()
    {
        List<Seat> availableSeats = new List<Seat>();
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < SeatsPerRow; j++)
            {
                if (!seats[i, j])
                {
                    availableSeats.Add(new Seat(i + 1, j + 1));
                }
            }
        }
        return availableSeats;
    }

    static void LoadData()
    {
        if (File.Exists(DataFileName))
        {
            using (FileStream fileStream = new FileStream(DataFileName, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Booking>));
                bookings = (List<Booking>)serializer.Deserialize(fileStream);
            }

            foreach (Booking booking in bookings)
            {
                foreach (Seat seat in booking.Seats)
                {
                    seats[seat.Row - 1, seat.Number - 1] = true;
                }
            }
        }
        else
        {
            RandomlyOccupySeats();
        }
    }

    static void SaveData()
    {
        using (FileStream fileStream = new FileStream(DataFileName, FileMode.Create))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Booking>));
            serializer.Serialize(fileStream, bookings);
        }
    }

    static void RandomlyOccupySeats()
    {
        Random random = new Random();
        int numOccupiedSeats = (int)(Rows * SeatsPerRow * RandomOccupiedPercentage);

        for (int i = 0; i < numOccupiedSeats; i++)
        {
            int row = random.Next(1, Rows + 1);
            int seat = random.Next(1, SeatsPerRow + 1);
            seats[row - 1, seat - 1] = true;
        }
    }
}