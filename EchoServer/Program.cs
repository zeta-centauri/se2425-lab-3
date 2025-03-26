using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoServer;

class EchoServer
{
    public static void Main()
    {
        StartServer();
    }

    public static void StartServer()
    {
        // Устанавливаем IP-адрес и порт
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 8000;
        // Создаем TCP listener
        TcpListener server = new TcpListener(ipAddress, port);
        try
        {
            // Запускаем сервер
            server.Start();
            Console.WriteLine("Сервер запущен на {0}:{1}", ipAddress, port);
            while (true)
            {
                Console.WriteLine("Ожидание подключения...");
                // Принимаем клиента
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Клиент подключен!");
                // Получаем поток для чтения и записи
                NetworkStream stream = client.GetStream();
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    // Читаем данные от клиента
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Получено: {0}", receivedMessage);
                        // Отправляем эхо обратно клиенту
                        byte[] response = Encoding.UTF8.GetBytes(receivedMessage);
                        stream.Write(response, 0, response.Length);
                        Console.WriteLine("Отправлено обратно: {0}", receivedMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при обработке клиента: {0}", ex.Message);
                }
                finally
                {
                    // Закрываем соединение с клиентом
                    stream.Close();
                    client.Close();
                    Console.WriteLine("Клиент отключен");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка сервера: {0}", ex.Message);
        }
        finally
        {
            server.Stop();
        }
    }
}
