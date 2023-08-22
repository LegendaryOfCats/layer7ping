using System.Diagnostics;
using System.Net;

namespace l7ping
{
    class Program
    {
        private static bool stp = false;
        private static int sentRequests = 0;
        private static Dictionary<HttpStatusCode, int> statusCodeCounts = new Dictionary<HttpStatusCode, int>();

        static async Task Main(string[] args)
        {
            string executableName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            if (args.Length < 1)
            {
                Console.WriteLine($"Usage: {executableName} <URL> [HTTP METHOD]");
                return;
            }

            string url = args[0];
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }
            string httpMethod = args.Length > 1 ? args[1].ToUpper() : "GET";

            Console.CancelKeyPress += (sender, e) => {
                stp = true;
                e.Cancel = true;
            };

            await PingWebsiteContinuouslyAsync(url, httpMethod);

            DisplayStatistics(url);
        }

        static async Task PingWebsiteContinuouslyAsync(string url, string httpMethod)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
                httpClient.DefaultRequestVersion = HttpVersion.Version20;
                Console.WriteLine($"layer7ping v1.4 - Copyright (c) 2023 NetworkLayer\n\nEstablishing connection to {url} using {httpMethod}\n");

                try
                {
                    while (!stp)
                    {
                        sentRequests++;

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        HttpResponseMessage response;
                        if (httpMethod == "GET")
                            response = await ExecuteRequestAsync(httpClient, url, HttpMethod.Get);
                        else if (httpMethod == "HEAD")
                            response = await ExecuteRequestAsync(httpClient, url, HttpMethod.Head);
                        else
                        {
                            Console.WriteLine($"Invalid HTTP method: {httpMethod}");
                            return;
                        }

                        stopwatch.Stop();

                        if (statusCodeCounts.ContainsKey(response.StatusCode))
                        {
                            statusCodeCounts[response.StatusCode]++;
                        }
                        else
                        {
                            statusCodeCounts[response.StatusCode] = 1;
                        }

                        ConsoleColor statusCodeColor = GetStatusCodeColor(response.StatusCode);

                        PrintStatus("Connected to: ", ConsoleColor.Green, $"{url}");
                        PrintStatus(" status=", statusCodeColor, $"{(int)response.StatusCode}/{response.ReasonPhrase}");
                        PrintStatus(" method=", ConsoleColor.Green, httpMethod);
                        PrintStatus(" time=", ConsoleColor.Green, $"{stopwatch.ElapsedMilliseconds}ms");
                        PrintStatus(" bytes=", ConsoleColor.Green, response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value.ToString() : "0\n");

                        await Task.Delay(1000);
                        if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Q && (ConsoleModifiers.Control & ConsoleModifiers.Control) != 0)
                        {
                            stp = true;
                            break;
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    stp = true;
                }
            }
        }

        static void DisplayStatistics(string url)
        {
            Console.WriteLine($"Ping statistics for {url}");
            foreach (var kvp in statusCodeCounts)
            {
                double percentage = (double)kvp.Value / sentRequests * 100;
                Console.WriteLine($"    {kvp.Key}: Count = {kvp.Value}, Percentage = {percentage:F2}%");
            }
        }

        static ConsoleColor GetStatusCodeColor(HttpStatusCode statusCode)
        {
            if ((int)statusCode >= 200 && (int)statusCode < 300)
                return ConsoleColor.Green;
            else if ((int)statusCode >= 300 && (int)statusCode < 400)
                return ConsoleColor.Yellow;
            else
                return ConsoleColor.Red;
        }

        static void PrintStatus(string statusName, ConsoleColor col, string status)
        {

            Console.Write(statusName);
            Console.ForegroundColor = col;
            Console.Write(status);
            Console.ResetColor();
        }

        static async Task<HttpResponseMessage> ExecuteRequestAsync(HttpClient client, string url, HttpMethod method)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                HttpRequestMessage request = new HttpRequestMessage(method, url);

                return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
        }
    }

}
