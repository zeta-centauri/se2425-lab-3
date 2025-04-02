using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoServer;

class EchoServer
{
    private static int equationsSolved = 0;
    private static ConcurrentDictionary<Guid, int> clientCounters =
        new ConcurrentDictionary<Guid, int>();
    private static object consoleLock = new object();

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
            LogServerMessage($"Сервер запущен на {ipAddress}:{port}");

            while (true)
            {
                LogServerMessage("Ожидание подключения...");
                TcpClient client = server.AcceptTcpClient();

                // Создаем новый поток для обработки клиента
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
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

    private static void HandleClient(object? obj)
    {
        if (obj is not TcpClient client)
            return;

        Guid clientId = Guid.NewGuid();
        NetworkStream? stream = null;

        try
        {
            stream = client.GetStream();
            clientCounters.TryAdd(clientId, 0);

            LogClientMessage(clientId, "Клиент подключен!");

            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                LogClientMessage(clientId, $"Получены коэффициенты: {receivedMessage}");

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
                    SendResponse(stream, errorMessage);
                    LogClientMessage(clientId, errorMessage);
                    continue;
                }

                string solution = SolveQuadraticEquation(a, b, c);
                Interlocked.Increment(ref equationsSolved);
                clientCounters[clientId]++;

                LogClientMessage(clientId, $"Решено уравнение: {a}x^2 + {b}x + {c} = 0");
                LogClientMessage(clientId, $"Решение: {solution}");
                LogClientMessage(clientId, $"Клиент решил уравнений: {clientCounters[clientId]}");
                LogServerMessage($"Всего решено уравнений: {equationsSolved}");

                SendResponse(stream, solution);
            }
        }
        catch (Exception ex)
        {
            LogClientMessage(clientId, $"Ошибка при обработке клиента: {ex.Message}");
        }
        finally
        {
            stream?.Close();
            client.Close();
            clientCounters.TryRemove(clientId, out _);
            LogClientMessage(clientId, "Клиент отключен");
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

    private static void SendResponse(NetworkStream stream, string message)
    {
        byte[] response = Encoding.UTF8.GetBytes(message);
        stream.Write(response, 0, response.Length);
    }

    private static void LogServerMessage(string message)
    {
        lock (consoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SERVER][{DateTime.Now:HH:mm:ss}] {message}");
            Console.ResetColor();
        }
    }

    private static void LogClientMessage(Guid clientId, string message)
    {
        lock (consoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(
                $"[CLIENT {clientId.ToString().Substring(0, 8)}][{DateTime.Now:HH:mm:ss}] {message}"
            );
            Console.ResetColor();
        }
    }
}
