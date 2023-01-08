using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuthClient
{
    public class AuthClient
    {
        public static readonly string IDENTITY_PROVIDER_DEFAULT = "https://identity.ic0.app";
        public static readonly string IDENTITY_PROVIDER_ENDPOINT = "#authorize";
        public static readonly int INTERRUPT_CHECK_INTERVAL = 500;
        public static readonly string ERROR_USER_INTERRUPT = "UserInterrupt";

        public static bool IsDelegationValid(Delegation delegation)
        {
            // implementation for isDelegationValid goes here
        }

        public interface AuthClientCreateOptions
        {
            SignIdentity identity { get; set; }
            AuthClientStorage storage { get; set; }
            IdleOptions idleOptions { get; set; }
        }

        public interface IdleOptions
        {
            bool disableIdle { get; set; }
            bool disableDefaultIdleCallback { get; set; }
        }

        public interface AuthClientLoginOptions
        {
            string identityProvider { get; set; }
            BigInteger maxTimeToLive { get; set; }
            string derivationOrigin { get; set; }
            string windowOpenerFeatures { get; set; }
            Action onSuccess { get; set; }
            Action<string> onError { get; set; }
        }

        public interface InternetIdentityAuthRequest
        {
            string kind { get; set; }
            byte[] sessionPublicKey { get; set; }
            BigInteger maxTimeToLive { get; set; }
            string derivationOrigin { get; set; }
        }

        public interface InternetIdentityAuthResponseSuccess
        {
            string kind { get; set; }
            Delegation[] delegations { get; set; }
            byte[] userPublicKey { get; set; }
        }

        public interface InternetIdentityAuthResponseError
        {
            string kind { get; set; }
            string message { get; set; }
        }

        public class Delegation
        {
            public byte[] pubkey { get; set; }
            public BigInteger expiration { get; set; }
            public Principal[] targets { get; set; }
        }
    }
}
