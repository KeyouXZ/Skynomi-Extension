using System.Net;
using System.Net.WebSockets;
using System.Text;
using TShockAPI;

namespace Skynomi.Web;

public abstract class WebServer
{
    private static HttpListener listener = new HttpListener();
    private static CancellationTokenSource cts = new CancellationTokenSource();
    private static List<WebSocket> clients = new List<WebSocket>();

    public static void Start()
    {
        listener.Prefixes.Add(Web.hostPort!);
        listener.Start();
        Utils.Log.Info($"- WebServer: Running on {Web.hostPort}");

        if (TShock.Config.Settings.RestApiEnabled && Web.config.EnableReverseProxy)
        {
            Utils.Log.Info($"- WebServer: Reverse proxy enabled");
        }

        cts = new CancellationTokenSource();
        Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = HandleWebSocket(context);
                    }
                    else
                    {
                        ServeHttpOrProxy(context).GetAwaiter();
                    }
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 995) break;
                    Utils.Log.Error($"- WebServer: Error: {ex.Message}");
                }
            }
        }, cts.Token);
    }

    public static void Stop(bool reload = false)
    {
        Utils.Log.Info($"- WebServer: Stopping...");
        lock (clients)
        {
            foreach (var ws in clients)
                ws.Dispose();
        }

        cts.Cancel();
        listener.Stop();
        listener.Close();
        
        if (!reload) return;
        listener = new HttpListener();
        cts = new CancellationTokenSource();
        clients = new List<WebSocket>();
    }

    #region THIS

    private static async Task ServeHttpOrProxy(HttpListenerContext context)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        string path = context.Request.Url.AbsolutePath.TrimStart('/');
#pragma warning restore CS8602 // Dereference of a possibly null reference.


        if (string.IsNullOrEmpty(path))
        {
            var response = context.Response;
            response.ContentType = "text/html";

            byte[] buffer = Encoding.UTF8.GetBytes(WebPage.HomePage());
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }
        else if (path == "script.js")
        {
            bool dev = false;
            if (dev)
            {
                string filePath = Path.Combine(Path.Combine(TShock.SavePath, "Skynomi", "WebPage"), path);
                byte[] buffer = await File.ReadAllBytesAsync(filePath);
                context.Response.ContentType = "application/javascript";
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.Close();
            }
            else
            {
                var response = context.Response;
                response.ContentType = "application/javascript";

                byte[] buffer = Encoding.UTF8.GetBytes(WebPage.Script);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
        }
        else
        {
            if (TShock.Config.Settings.RestApiEnabled && Web.config.EnableReverseProxy)
            {
                string targetUrl = "http://localhost:" + TShock.Config.Settings.RestApiPort + context.Request.RawUrl;
                if (Web.config.DebugLogs)
                    Console.WriteLine(
                        $"{Utils.Messages.Name} - Proxy: Proxying request: {context.Request.Url} -> {targetUrl}");

                try
                {
                    var client = new HttpClient();
                    var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.HttpMethod), targetUrl);

                    if (context.Request.HasEntityBody)
                    {
                        using var reader =
                            new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                        requestMessage.Content = new StringContent(await reader.ReadToEndAsync(), Encoding.UTF8,
                            context.Request.ContentType);
                    }

                    var response = await client.SendAsync(requestMessage);
                    context.Response.StatusCode = (int)response.StatusCode;

                    foreach (var header in response.Headers)
                    {
                        context.Response.Headers[header.Key] = string.Join(",", header.Value);
                    }

                    await response.Content.CopyToAsync(context.Response.OutputStream);
                }
                catch (HttpRequestException ex)
                {
                    Utils.Log.Error($"{Utils.Messages.Name} - Proxy: {ex.Message}");
                    context.Response.StatusCode = 502;
                    byte[] buffer = Encoding.UTF8.GetBytes("502 Bad Gateway: Proxy request failed.");
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                finally
                {
                    context.Response.Close();
                }
            }
            else
            {
                context.Response.StatusCode = 404;
                byte[] buffer = Encoding.UTF8.GetBytes("404 Not Found");
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }

    #endregion


    private static async Task HandleWebSocket(HttpListenerContext context)
    {
        HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
        WebSocket ws = wsContext.WebSocket;

        lock (clients)
        {
            clients.Add(ws);
        }

        if (Web.config.DebugLogs) Utils.Log.Info($"- WebSocket: Client connected!");

        byte[] buffer = new byte[1024];
        try
        {
            while (ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result =
                    await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (Web.config.DebugLogs) Utils.Log.Info($"- WebSocket: Received: {msg}");
            }
        }
        catch (Exception ex)
        {
            Utils.Log.Error($"- WebSocket: Error: {ex.Message}");
        }
        finally
        {
            lock (clients)
            {
                clients.Remove(ws);
            }

            if (ws.State == WebSocketState.Open)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }

            ws.Dispose();
        }
    }

    public static void SendWsData(string? eventType = null, object? data = null)
    {
        try
        {
            dynamic? message;
            if (!string.IsNullOrWhiteSpace(eventType))
            {
                message = new Dictionary<string, object?>
                {
                    { "eventType", eventType },
                    { "data", data }
                };
            }
            else if (data != null)
            {
                message = data;
            }
            else
            {
                return;
            }

            string json = System.Text.Json.JsonSerializer.Serialize(message);

            lock (clients)
            {
                foreach (var ws in clients)
                {
                    if (ws.State == WebSocketState.Open)
                    {
                        _ = ws.SendAsync(
                            Encoding.UTF8.GetBytes(json),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Utils.Log.Info(ex.ToString());
        }
    }
}