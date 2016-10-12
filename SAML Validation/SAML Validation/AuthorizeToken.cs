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

            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("NS1", "urn:oasis:names:tc:SAML:2.0:metadata");
            manager.AddNamespace("NS2", "http://www.w3.org/2000/09/xmldsig#");
            XmlNodeList nodeList = xmlDoc.SelectNodes("//NS1:IDPSSODescriptor/NS1:KeyDescriptor[@use='signing']/NS2:KeyInfo/NS2:X509Data/NS2:X509Certificate", manager);
            var cert = nodeList[0].InnerText;

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

                XmlNodeList nodeList = xmlDoc.GetElementsByTagName("NameID");
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
