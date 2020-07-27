﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Fleck;
using Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Console = Colorful.Console;

namespace DummyClient
{
    public class ClientConnection
    {
        public IWebSocketConnection Socket { get; }
        public Dictionary<Guid, string> SubscribedCatalog = new Dictionary<Guid, string>();
        FileSystemWatcher watcher = new FileSystemWatcher();

        public ClientConnection(IWebSocketConnection socket)
        {
            Socket = socket;
            socket.OnOpen = () => Console.WriteLine("Open new connection!");
            socket.OnClose = () => Console.WriteLine("Conncetion closed!");
            socket.OnMessage = message => HandleMessage(message);

            socket.OnError = ex => HandleError(ex);
        }       

        public event EventHandler<string> OnMessage;
       
        public void HandleError(Exception exception)
        {
            System.Console.WriteLine(exception.Message);
            
        }

        public void HandleMessage(string message)
        {
            /*var json = JObject.Parse(message);
            string type = (string)json["TypeName"];
            Type t = Type.GetType(type, true);*/
            
            OnMessage?.Invoke(this, message);
            System.Console.WriteLine(message);
           /* var request = JsonConvert.DeserializeObject<Request>(message);
            System.Console.WriteLine($"[{request.ID}] [{request.Command}] ", Color.Cornsilk);


            if (request.Command == ValidCommand.ServerTime)
            {
                var r = new Request()
                {
                    Command = ValidCommand.ServerTime,
                    Message = $"Server time: {DateTime.Now}"
                };
                SendMessage(r);
            }
            else if (request.Command == ValidCommand.Ping)
            {
                var r = new Request()
                {
                    Command = ValidCommand.Pong,
                    Message = "pong"
                };
                SendMessage(r);
            }
            else if (request.Command == ValidCommand.WaitForFolderChange)
            {
                string path = request.Message;
                var id = request.ID;
                SubscribedCatalog.Add(id, path);

               


                watcher.Path = path;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                                                | NotifyFilters.FileName | NotifyFilters.DirectoryName;


                watcher.Filter = "*.txt";
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnCreated);
                watcher.Deleted += new FileSystemEventHandler(OnCreated);
                watcher.EnableRaisingEvents = true;
                
            }
            else if (request.Command == ValidCommand.Division)
            {
                try
                {
                    var tokens = request.Message.Split();

                    var a = tokens[1];
                    var b = tokens[2];
                    Int32.TryParse(a, out int resA);
                    Int32.TryParse(b, out int resB);
                    var div = resA / resB;

                    Socket.Send(div.ToString());
                }
                catch (Exception e)
                {
                    Socket.Send(e.Message + "\n " + e.StackTrace);
                }
            }
            else
            {
                var r = new Request()
                {
                    Command = ValidCommand.Unknown,
                };
                SendMessage(r);
            }*/
        }

        private void SomeHapens(FileSystemEventArgs e)
        {
            List<Guid> listOfGuids = new List<Guid>();
            foreach (var data in SubscribedCatalog)
            {
                if (data.Value == Path.GetDirectoryName(e.FullPath))
                {
                    listOfGuids.Add(data.Key);
                }
            }

            if (listOfGuids.Any())
            {


                foreach (var guid in listOfGuids)
                {
                    FileSystemEvent fileSystemEvent = new FileSystemEvent
                    {
                        FileName = e.Name,
                        FullPath = e.FullPath,
                        ChangesType = e.ChangeType
                    };

                   

                    Socket.Send(JsonConvert.SerializeObject(fileSystemEvent));
                }
            }

        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            SomeHapens(e);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            SomeHapens(e);
        }

        private void SendMessage(Request request)
        {
            request.ID = Guid.NewGuid();
            var json = JsonConvert.SerializeObject(request);
            Socket.Send(json);
        }
    }
}
