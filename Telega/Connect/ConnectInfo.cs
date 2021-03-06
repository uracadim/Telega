using System.Net;
using LanguageExt;
using Telega.Auth;

namespace Telega.Connect
{
    sealed class ConnectInfo
    {
        readonly Session _original;
        readonly int _apiId;
        readonly IPEndPoint _endpoint;
        Step3Res _auth;

        ConnectInfo(Session original, int apiId, IPEndPoint endpoint)
        {
            _original = original;
            _apiId = apiId;
            _endpoint = endpoint;
        }

        public static ConnectInfo FromSession(Some<Session> session) =>
            new ConnectInfo(session, 0, null);

        public static ConnectInfo FromInfo(int apiId, Some<IPEndPoint> endpoint) =>
            new ConnectInfo(null, apiId, endpoint);

        public bool NeedsInAuth => _original == null || !_original.IsAuthorized;
        public void SetAuth(Step3Res auth) => _auth = auth;

        public IPEndPoint Endpoint => _original?.Endpoint ?? _endpoint;

        public Session ToSession() =>
            _original ?? Session.New(_apiId, _endpoint, _auth.AuthKey, _auth.TimeOffset);
    }
}
