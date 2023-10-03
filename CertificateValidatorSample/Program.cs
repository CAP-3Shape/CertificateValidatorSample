// <copyright>3Shape A/S</copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ClausAppel.CertificateValidatorSample
{
    public class Program
    {
        internal static void Main(string[] args) => new Program().InstanceMain();

        private void InstanceMain()
        {
            var collection = GetCertificateCollectionFromResource(X509KeyStorageFlags.EphemeralKeySet);
            var leaf = collection[0];
            var parents = new[] { collection[1], collection[2] };
            var chain = CreateChain(parents);
            var valid = chain.Build(leaf);
            if (!valid)
                throw new Exception("Chain.Build returned false");

            // We expect the root to be untrusted. It is a self-signed certificate. So the UntrustedRoot flag is harmless.
            var illegalFlags = chain.ChainStatus.Where(f => f.Status != X509ChainStatusFlags.UntrustedRoot).ToList();
            if (illegalFlags.Any())
            {
                var firstIllegalFlag = illegalFlags.First();
                throw new Exception($"Illegal status flag found: {firstIllegalFlag.Status} ({firstIllegalFlag.StatusInformation})");
            }
            else
            {
                Console.WriteLine("Certificate collection is valid!");
            }
        }

        private X509Certificate2Collection GetCertificateCollectionFromResource(X509KeyStorageFlags storageFlags)
        {
            var assembly = GetType().Assembly;
            var certificateStream = assembly.GetManifestResourceStream(
                "ClausAppel.CertificateValidatorSample.Resources.staging-certificate-example01.txt");
            using var streamReader = new StreamReader(certificateStream!);
            var certificateString = streamReader.ReadToEnd();
            var collection = new X509Certificate2Collection();
            collection.Import(Convert.FromBase64String(certificateString), null, storageFlags);
            return collection;
        }

        /// <summary> Creates a <see cref="X509Chain"/> from a collection of <see cref="X509Certificate2"/>. </summary>
        /// <param name="parents">A chain of "parent" certificates that have issued each other, with the root as the last element.</param>
        /// <returns>A chain which may be valid or not.</returns>
        public static X509Chain CreateChain(IEnumerable<X509Certificate2> parents)
        {
            var chain = new X509Chain(useMachineContext: false)
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
                }
            };
            foreach (var parent in parents) chain.ChainPolicy.ExtraStore.Add(parent);
            return chain;
        }
    }
}
