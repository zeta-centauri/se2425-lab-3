using System;
using System.Net.Sockets;
using System.Text;

class EchoClient
{
    public static void Main(string[] args)
    {
        try
        {
            // Подключаемся к серверу
            TcpClient client = new TcpClient("127.0.0.1", 8000);
            NetworkStream stream = client.GetStream();
            // Бесконечный цикл для ввода сообщений
            while (true)
            {
                Console.Write("Введите сообщение (или 'exit' для выхода): ");
                string? message = Console.ReadLine();
                // Проверяем условие выхода
                if (message!.ToLower() == "exit")
                    break;
                // От // Отправляем сообщение
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Отправлено: {0}", message);
                // Читаем ответ от сервера
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Получено от сервера: {0}", response);
            }
            // Закрываем соединение
            stream.Close();
            client.Close();
            Console.WriteLine("Соединение закрыто");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка клиента: {0}", ex.Message);
        }
    }
}
