using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;
using System.IdentityModel.Selectors;

namespace SAMLValidation
{
    class AuthorizeToken
    {
        private string token;
        private string metaDataXMLPath;
        public string UserName { get; set; }
        public DateTime ValidTo { get; set; }

        public AuthorizeToken(string securityToken, string path)
        {
            token = securityToken;
            metaDataXMLPath = path;
            UserName = GetUserID(securityToken).ToLower();
        }

        public bool isValid()
        {
            var metaCertificate = GetMetaCertificate();
            string samlAssertionXml = SamlToXmlString(token);
            bool isVerified = VerifySignature(samlAssertionXml, metaCertificate);
            SecurityToken samlToken = GetSecurityToken(samlAssertionXml);

            if (isVerified)
            {
                var identity = ValidateSamlToken(samlToken);
                ValidTo = samlToken.ValidTo;
            }
            return isVerified;
        }

        private X509Certificate2 GetMetaCertificate()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(metaDataXMLPath);

            //XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            //manager.AddNamespace("", "urn:oasis:names:tc:SAML:1.0:metadata");
            //manager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            //XmlNodeList nodeList = xmlDoc.SelectNodes("//Assertion/AuthenticationStatement/Subject/SubjectConfirmation/ds:KeyInfo/ds:X509Data/ds:X509Certificate", manager);
            //var cert = nodeList[0].InnerText;

            var cert = "MIIFvjCCA6agAwIBAgIIe/S3zN2RUSgwDQYJKoZIhvcNAQELBQAwcjELMAkGA1UEBhMCQkUxETAPBgNVBAoMCFpFVEVTIFNBMQwwCgYDVQQFEwMwMDExQjBABgNVBAMMOVpldGVzQ29uZmlkZW5zIFByaXZhdGUgVHJ1c3QgUEtJIC0gZUhlYWx0aCBpc3N1aW5nIENBIDAwMTAeFw0yMTAxMTExMDQxNTZaFw0yNDAxMTIxMDQxNTZaMIGzMQswCQYDVQQGEwJCRTEbMBkGA1UECgwSRmVkZXJhbCBHb3Zlcm5tZW50MQ8wDQYDVQQLDAZJQU1BQ0MxFzAVBgNVBAsMDkNCRT0wODA5Mzk0NDI3MRkwFwYDVQQLDBBFSEVBTFRILVBMQVRGT1JNMSEwHwYDVQQLDBhlSGVhbHRoLXBsYXRmb3JtIEJlbGdpdW0xHzAdBgNVBAMMFkNCRT0wODA5Mzk0NDI3LCBJQU1BQ0MwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCcMZjkxDhOpjDeSZKIi1loyMCIjAqcCLjZnPPzgpWqk1waKadWyen6VwP/SCPHBqOFgqSOAkFajD+qgkDHDFOmAt3o+l85FfgSGxhCRlake/OLUpIeOJjAHTCqhEhaYMfynn1bS8xJ7DQZwhfto6OCnYb0w81QrnSad5evPTHDz9tDgxzOB+rjBGvYceTN/lJ2G+MHQbFaptjpPG0tIwSeIpuWWuysSybr6cqHf+RLXCDh6t09upr70HsBnzIBci2AF9HhRa8P8Tb8nf9Rfjeb2rAmpw66kjFeEVACpehIjYs8zy4nDVzw7O7FZ7Ox9NnH0FhHiPhH4b6T3OjznLPXAgMBAAGjggEUMIIBEDBEBggrBgEFBQcBAQQ4MDYwNAYIKwYBBQUHMAGGKGh0dHA6Ly9vY3NwLWVoLXB0cGtpLmNvbmZpZGVucy56ZXRlcy5jb20wHQYDVR0OBBYEFD1fEqwj0Q2DouE6m0Q5pfViGh3rMAwGA1UdEwEB/wQCMAAwHwYDVR0jBBgwFoAUK3GvVjPR3TPnIR1LIRGsm7cKDxkwSwYDVR0fBEQwQjBAoD6gPIY6aHR0cDovL2NybC1laC1wdHBraS5jb25maWRlbnMuemV0ZXMuY29tL1pDRUhQVFBLSUNBMDAxLmNybDAOBgNVHQ8BAf8EBAMCBeAwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMEMA0GCSqGSIb3DQEBCwUAA4ICAQBWN/fd7jNn6c1upxkFs4bm4P9MiZ4vXWHx7RIPPMUJs4yICgY9dG5gn8a23asBZYiDQv41SmrMuYSYnOpxGsfmXgCidhP4P4c2Y8waS0YTHomcv3pDKsV+tNQX27SV6HNfxHITKpkPOuBiqvrKhMOGekQmsDh1C0VaLnzxnlBFIgNEcseEwa7z9EbTFZzYBcvgAJM0waR+vNAMS5zWmkhw4hqSYvk4+IHJfZdlYcDCBm8j97dCsZnPEfGf1H5ImOjmhGECBRuzSLI/WI5yh3vQ/5/jwGW3l9d7J0TP4nS0Bba1tEcw0OO84VrrDXYKmvdnsaHUwC7JZilY2E1UuxMuY8tIkWtTbIKItEpAN4ZMYi8ZL+Lr4byqxEP2D0OKslKoZ0r/uK1OOI340/+Fd1KzcdBCPSE2Hdy5jh2fh48/8AnzzJGxTibfdXwaUeIjkSVdCqRV+Cw0cFFp6ub3qXMMkiP4YOl8WOnmeLbB8fEMMSPP9LERYTPE3jKFl/yKPPE28rHz9OCkqCkXdjX3upjjWUUt1O9J2IjX6VZVqCDz8Cab8FLfnXPH2/K8abEAMe2i9IoqH1mXwqXLWhCrtC6TETnrg3xIOD7ndyBbR/d5cN6cJ4koVvSANE3rpWzB10jKUEU+P7VXxCw0QIxjMlYX8lYlc71JX8bzyWdc9OIZIw==";

            byte[] data = Convert.FromBase64String(cert);
            var x509 = new X509Certificate2(data);
            return x509;
        }

        private static string SamlToXmlString(string EncodedResponse)
        {
            byte[] decoded = Convert.FromBase64String(EncodedResponse);
            string deflated = Encoding.UTF8.GetString(decoded);
            return deflated;
        }

        private bool VerifySignature(string samlAssertionXml, X509Certificate2 certificate)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(samlAssertionXml);

            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            XmlNodeList nodeList = doc.SelectNodes("//ds:Signature", manager);

            SignedXml signedXml = new SignedXml(doc);

            if (nodeList != null && nodeList.Count > 0)
            {
                signedXml.LoadXml((XmlElement)nodeList[0]);
                return signedXml.CheckSignature(certificate, true);
            }
            return false;
        }

        private SecurityToken GetSecurityToken(string samlAssertionXml)
        {
            StringReader stringReader = new StringReader(samlAssertionXml);
            XmlTextReader reader = new XmlTextReader(stringReader);

            Saml2SecurityTokenHandler handler = new Saml2SecurityTokenHandler();
            handler.Configuration = new SecurityTokenHandlerConfiguration();

            SecurityToken samlToken = handler.ReadToken(reader);
            return samlToken;
        }

        private ClaimsIdentity ValidateSamlToken(SecurityToken securityToken)
        {
            var registry = new ConfigurationBasedIssuerNameRegistry();
            //For Webapp //var thumbprint = WebConfigurationManager.AppSettings["Thumbprint"];
            var thumbprint = System.Configuration.ConfigurationManager.AppSettings["Thumbprint"];
            var issuer = System.Configuration.ConfigurationManager.AppSettings["Issuer"];
            registry.AddTrustedIssuer(thumbprint, issuer);

            TokenHandler handler = new TokenHandler();
            handler.Configuration = new SecurityTokenHandlerConfiguration();
            handler.Configuration.AudienceRestriction.AudienceMode = AudienceUriMode.Never;
            handler.Configuration.IssuerNameRegistry = registry;
            var identity = handler.ValidateToken(securityToken).First();
            return identity;
        }

        /*public static string GetNameID(HttpRequestMessage request)
        {
            var cwpCredential = request.Headers.FirstOrDefault(x => x.Key == @"CWP-Credential").Value.FirstOrDefault();
            return GetNameIDFromToken(cwpCredential);

        }*/

        private static string GetNameIDFromToken(String token)
        {
            try
            {
                var cwpCredential = token;
                var samlAssertionXml = SamlToXmlString(cwpCredential);
                var xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.XmlResolver = null;
                xmlDoc.LoadXml(samlAssertionXml);

                XmlNodeList nodeList = xmlDoc.GetElementsByTagName("NameIdentifier");
                var nameId = nodeList[0].InnerText;
                return nameId.ToLower();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException(ex.Message);
            }
        }

        public string GetUserID(String token)
        {
            return GetNameIDFromToken(token);
        }
    }

        class TokenHandler : Saml2SecurityTokenHandler
    {
        public TokenHandler() : base() { }
        protected override void ValidateConfirmationData(Saml2SubjectConfirmationData confirmationData) { }
    }
}
