using System.Net;
using System.Net.Sockets;

const int port = 7500;
const int bufferSize = 1024;

// Создаем TCP-сокет
var listener = new TcpListener(IPAddress.Any, port);
listener.Start();
Console.WriteLine("Ожидание клиента...");

while (true)
{
    try
    {
        using var client = listener.AcceptTcpClient();
        using var networkStream = client.GetStream();
        Console.WriteLine("\nКлиент подключен");

        // Получаем длину имени файла
        var lengthBuffer = new byte[4];
        if (networkStream.Read(lengthBuffer, 0, lengthBuffer.Length) != lengthBuffer.Length)
        {
            Console.WriteLine("Ошибка при получении длины имени файла.");
            continue;
        }

        // Читаем длину имени файла
        var fileNameLength = BitConverter.ToInt32(lengthBuffer, 0);
        
        // Получаем имя файла
        var fileNameBuffer = new byte[fileNameLength];
        if (networkStream.Read(fileNameBuffer, 0, fileNameBuffer.Length) != fileNameBuffer.Length)
        {
            Console.WriteLine("Ошибка при получении имени файла.");
            continue;
        }

        var fileName = System.Text.Encoding.UTF8.GetString(fileNameBuffer);
        
        if (fileName.Equals("qs", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Получена команда на завершение работы сервера.");
            break;
        }

        Console.WriteLine("Получено имя файла: " + fileName);

        using var memoryStream = new MemoryStream();
        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;

        // Чтение содержимого файла
        while (true)
        {
            var bytesRead = networkStream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) break;
            memoryStream.Write(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;
        }

        memoryStream.Position = 0;

        using var reader = new StreamReader(memoryStream);
        var fileContent = reader.ReadToEnd();
        Console.WriteLine($"Получен файл '{fileName}' размером {totalBytesRead} байт.");
        Console.WriteLine("Содержимое файла:");
        Console.WriteLine(fileContent);
    }
    catch (Exception e)
    {
        Console.WriteLine("Ошибка: " + e.Message);
    }
}

listener.Stop();
