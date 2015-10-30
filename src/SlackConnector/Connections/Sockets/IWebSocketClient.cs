﻿using System;
using SlackConnector.Connections.Sockets.Messages;

namespace SlackConnector.Connections.Sockets
{
    internal interface IWebSocketClient
    {
        bool IsAlive { get; }

        event EventHandler OnOpen;
        event EventHandler<InboundMessage> OnMessage;
        event EventHandler OnClose;

        void Connect();
        void Close();
    }
}