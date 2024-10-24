using System.Net.Sockets;

const string serverIp = "127.0.0.1";
const int port = 7500;
const int bufferSize = 1024;
const string stopClient = "qc";
const string stopServer = "qs";

while (true)
{
    Console.WriteLine("Введите полный путь к файлу, который хотите отправить \n(или 'qc' для завершения работы клиента, 'qs' для завершения работы сервера):");
    var input = Console.ReadLine();

    if (input != null && input.Equals(stopClient, StringComparison.OrdinalIgnoreCase))
    {
        break; 
    }

    if (input != null && input.Equals(stopServer, StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            using var client = new TcpClient(serverIp, port);
            using var networkStream = client.GetStream();

            var shutdownMessage = stopServer;
            var shutdownMessageBytes = System.Text.Encoding.UTF8.GetBytes(shutdownMessage);
            var shutdownMessageLengthBytes = BitConverter.GetBytes(shutdownMessageBytes.Length);

            networkStream.Write(shutdownMessageLengthBytes, 0, shutdownMessageLengthBytes.Length);
            networkStream.Write(shutdownMessageBytes, 0, shutdownMessageBytes.Length);

            Console.WriteLine("Команда на завершение работы сервера отправлена.");
            continue; 
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка при отправке команды на завершение сервера: " + ex.Message);
            continue;
        }
    }

    // Проверка существования файла
    if (!File.Exists(input))
    {
        Console.WriteLine("Файл не найден! Проверьте, что файл находится по указанному пути: " + input);
        continue; // Запросить путь снова
    }

    while (true)
    {
        try
        {
            using var client = new TcpClient(serverIp, port);
            using var networkStream = client.GetStream();

            // Отправляем имя файла
            var fileName = Path.GetFileName(input);
            var fileNameBytes = System.Text.Encoding.UTF8.GetBytes(fileName);
            var fileNameLengthBytes = BitConverter.GetBytes(fileNameBytes.Length);

            // Сначала отправляем длину имени файла
            networkStream.Write(fileNameLengthBytes, 0, fileNameLengthBytes.Length);

            // Затем отправляем само имя файла
            networkStream.Write(fileNameBytes, 0, fileNameBytes.Length);

            // Отправляем содержимое файла
            using (var fileStream = new FileStream(input, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[bufferSize];
                int bytesRead;

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    networkStream.Write(buffer, 0, bytesRead);
                }
            }

            Console.WriteLine("Файл успешно отправлен.\n");
            break;
        }
        catch (SocketException)
        {
            Console.WriteLine("Не удалось подключиться к серверу. Сервер может быть не запущен. Повторная попытка отправки.");
            Thread.Sleep(5000);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка при отправке файла: " + ex.Message);
            break; 
        }
    }
}
