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
                Console.Write(
                    "Введите коэффициенты квадратного уравнения: a, b, c (или 'exit' для выхода): "
                );
                string? input = Console.ReadLine();
                // Проверяем условие выхода
                if (input!.ToLower() == "exit")
                    break;

                try
                {
                    var parts = input.Split(" ");
                    if (parts.Length != 3)
                    {
                        Console.WriteLine(
                            "Ошибка: необходимо ввести 3 числа, разделенных пробелами"
                        );
                        continue;
                    }

                    double a = double.Parse(parts[0]);
                    double b = double.Parse(parts[1]);
                    double c = double.Parse(parts[2]);

                    byte[] data = Encoding.UTF8.GetBytes(input);
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine($"Отправка уравнения: {a}x^2 + {b}x + {c} = 0");

                    // Чтение ответа от сервера
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Получено от сервера: {0}", response);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Ошибка: введены нечисловые значения - {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }
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
