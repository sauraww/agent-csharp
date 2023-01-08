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

        public static async Task<AuthClient> Create(
        AuthClientCreateOptions options = null
    )
    {
        options = options ?? new AuthClientCreateOptions();
        var storage = options.Storage ?? new IdbStorage();

        SignIdentity key = null;
        if (options.Identity != null)
        {
            key = options.Identity;
        }
        else
        {
            var maybeIdentityStorage = await storage.Get(KEY_STORAGE_KEY);
            if (!maybeIdentityStorage && IsBrowser)
            {
                // Attempt to migrate from localstorage
                try
                {
                    var fallbackLocalStorage = new LocalStorage();
                    var localChain = await fallbackLocalStorage.Get(KEY_STORAGE_DELEGATION);
                    var localKey = await fallbackLocalStorage.Get(KEY_STORAGE_KEY);
                    if (localChain != null && localKey != null)
                    {
                        Console.WriteLine("Discovered an identity stored in localstorage. Migrating to IndexedDB");
                        await storage.Set(KEY_STORAGE_DELEGATION, localChain);
                        await storage.Set(KEY_STORAGE_KEY, localKey);
                        maybeIdentityStorage = localChain;
                        // clean up
                        await fallbackLocalStorage.Remove(KEY_STORAGE_DELEGATION);
                        await fallbackLocalStorage.Remove(KEY_STORAGE_KEY);
                    }
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine("error while attempting to recover localstorage: " + error);
                }
            }

            if (maybeIdentityStorage != null)
            {
                try
                {
                    key = Ed25519KeyIdentity.FromJson(maybeIdentityStorage);
                }
                catch (Exception e)
                {
                    // Ignore this, this means that the localStorage value isn't a valid Ed25519KeyIdentity
                    // serialization.
                }
            }
        }

        Identity identity = new AnonymousIdentity();
        DelegationChain chain = null;

        if (key != null)
        {
            try
            {
                var chainStorage = await storage.Get(KEY_STORAGE_DELEGATION);

                if (options.Identity != null)
                {
                    identity = options.Identity;
                }
                else if (chainStorage != null)
                {
                    chain = DelegationChain.FromJson(chainStorage);

                    // Verify that the delegation isn't expired.
                    if (!IsDelegationValid(chain))
                    {
                        await _deleteStorage(storage);
                        key = null;
                    }
                    else
                    {
                        identity = DelegationIdentity.FromDelegation(key, chain);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                // If there was a problem loading the chain, delete the key.
                await _deleteStorage(storage);
               
            }


    }
}
