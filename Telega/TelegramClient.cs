﻿using System;
using System.Net;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.SomeHelp;
using Telega.CallMiddleware;
using Telega.Connect;
using Telega.Rpc.Dto;

namespace Telega
{
    public sealed class TelegramClient : IDisposable
    {
        readonly TgBellhop _bellhop;
        readonly SessionStoreSync _storeSync;

        public readonly TelegramClientAuth Auth;
        public readonly TelegramClientContacts Contacts;
        public readonly TelegramClientMessages Messages;
        public readonly TelegramClientUpload Upload;

        static readonly IPEndPoint DefaultEndpoint = new IPEndPoint(IPAddress.Parse("149.154.167.50"), 443);

        TelegramClient(
            TgBellhop bellhop,
            ISessionStore sessionStore
        ) {
            _bellhop = bellhop;
            _storeSync = SessionStoreSync.Init(_bellhop.SessionVar.ToSome(), sessionStore.ToSome());

            Auth = new TelegramClientAuth(_bellhop);
            Contacts = new TelegramClientContacts(_bellhop);
            Messages = new TelegramClientMessages(_bellhop);
            Upload = new TelegramClientUpload(_bellhop);
        }

        public void Dispose()
        {
            _bellhop.ConnectionPool.Dispose();
            _storeSync.Stop();
        }


        static async Task<TelegramClient> Connect(
            ConnectInfo connectInfo,
            ISessionStore store,
            TgCallMiddlewareChain callMiddlewareChain = null,
            TcpClientConnectionHandler tcpClientConnectionHandler = null
        ) {
            var bellhop = await TgBellhop.Connect(
                connectInfo,
                callMiddlewareChain,
                tcpClientConnectionHandler
            ).ConfigureAwait(false);
            return new TelegramClient(bellhop, store);
        }

        public static async Task<TelegramClient> Connect(
            int apiId,
            ISessionStore store = null,
            IPEndPoint endpoint = null,
            TgCallMiddlewareChain callMiddlewareChain = null,
            TcpClientConnectionHandler tcpClientConnectionHandler = null
        ) {
            store = store ?? new FileSessionStore("session.dat");
            var ep = endpoint ?? DefaultEndpoint;
            var connectInfo = (await store.Load().ConfigureAwait(false))
                .Map(SomeExt.ToSome).Map(ConnectInfo.FromSession)
                .IfNone(ConnectInfo.FromInfo(apiId, ep));

            return await Connect(connectInfo, store, callMiddlewareChain, tcpClientConnectionHandler);
        }

        public static async Task<TelegramClient> Connect(
            Some<Session> session,
            ISessionStore store = null,
            TgCallMiddlewareChain callMiddlewareChain = null,
            TcpClientConnectionHandler tcpClientConnectionHandler = null
        ) {
            store = store ?? new FileSessionStore("session.dat");
            var connectInfo = ConnectInfo.FromSession(session);

            return await Connect(connectInfo, store, callMiddlewareChain, tcpClientConnectionHandler);
        }

        public Task<T> Call<T>(ITgFunc<T> func) =>
            _bellhop.Call(func);
    }
}
