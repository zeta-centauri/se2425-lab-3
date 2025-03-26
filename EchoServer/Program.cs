using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoServer;

class EchoServer
{
    private static int equationsSolved = 0;

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
                        Console.WriteLine("Получены коэффициенты: {0}", receivedMessage);
                        // Парсинг коэффициентов
                        string[] coefficients = receivedMessage.Split(' ');
                        if (
                            coefficients.Length != 3
                            || !double.TryParse(coefficients[0], out double a)
                            || !double.TryParse(coefficients[1], out double b)
                            || !double.TryParse(coefficients[2], out double c)
                        )
                        {
                            string errorMessage =
                                "Ошибка: Неверный формат коэффициентов. Ожидается три числа, разделенных пробелами.";
                            byte[] errorResponse = Encoding.UTF8.GetBytes(errorMessage);
                            stream.Write(errorResponse, 0, errorResponse.Length);
                            Console.WriteLine(errorMessage);
                            continue;
                        }

                        string solution = SolveQuadraticEquation(a, b, c);
                        equationsSolved++;
                        Console.WriteLine($"Решено уравнение: {a}x^2 + {b}x + {c} = 0");
                        Console.WriteLine($"Решение: {solution}");
                        Console.WriteLine($"Всего решено уравнений: {equationsSolved}");

                        byte[] response = Encoding.UTF8.GetBytes(solution);
                        stream.Write(response, 0, response.Length);
                        Console.WriteLine("Отправлено обратно: {0}", solution);
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

    private static string SolveQuadraticEquation(double a, double b, double c)
    {
        if (a == 0)
        {
            return "Коэффициент 'a' не может быть нулем. Это не квадратное уравнение.";
        }

        double discriminant = b * b - 4 * a * c;

        if (discriminant > 0)
        {
            double x1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
            double x2 = (-b - Math.Sqrt(discriminant)) / (2 * a);
            return $"Два действительных корня: x1 = {x1:F2}, x2 = {x2:F2}";
        }
        else if (discriminant == 0)
        {
            double x = -b / (2 * a);
            return $"Один действительный корень: x = {x:F2}";
        }
        else
        {
            return "Действительных корней нет (дискриминант отрицательный).";
        }
    }
}
