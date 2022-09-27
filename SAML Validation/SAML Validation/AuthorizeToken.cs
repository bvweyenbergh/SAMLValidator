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
using System.Security.Cryptography;

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
            var issuer1 = new X509Certificate2("issuer1.cer");
            var issuer2 = new X509Certificate2("issuer2.cer");
            var issuer3 = new X509Certificate2("issuer3.cer");
            var issuer4 = new X509Certificate2("issuer4.cer");
            var samlcert1 = new X509Certificate2("samlcert1.cer");
            var samlcert2 = new X509Certificate2("samlcert2.cer");

            var metaCertificate = GetMetaCertificate();
            string samlAssertionXml = SamlToXmlString(token);
            bool isVerified = VerifySignature(samlAssertionXml, samlcert2);
            SecurityToken samlToken = GetSecurityToken(samlAssertionXml);

            //if (isVerified)
            //{
                var identity = ValidateSamlToken(samlToken);
                ValidTo = samlToken.ValidTo;
            //}
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
            var cert = nodeList[1].InnerText;

            byte[] data = Convert.FromBase64String(cert);
            var x509 = new X509Certificate2(data);
            return x509;
        }

        private static string SamlToXmlString(string EncodedResponse)
        {
            byte[] decoded = Convert.FromBase64String(EncodedResponse);
            string deflated = Encoding.Default.GetString(decoded);
            return deflated;
        }

        private bool VerifySignature(string samlAssertionXml, X509Certificate2 cert)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(samlAssertionXml);

            //var publicRsa = cert.GetRSAPublicKey();
            //var publicEcdsa = cert.GetECDsaPublicKey();

                SignedXml signedXml = new SignedXml(doc);

                XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
                manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
                XmlNodeList nodeList = doc.SelectNodes("//ds:Signature", manager);

                if (nodeList != null && nodeList.Count > 0)
                {
                    signedXml.LoadXml((XmlElement)nodeList[0]);
                    return signedXml.CheckSignature(cert, true);

                }

            return false;


            //if (publicRsa != null)
            //{
            //    signedXml.SigningKey = publicRsa;
            //}
            //else
            //{
            //    signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha1"; //required for ECDSA
            //    signedXml.SigningKey = publicEcdsa;
            //}

            //Reference reference = new Reference();
            //reference.Uri = "#_ba6921ea3ec62ee46b6728180c893d3a"; //hardcoded now but will be different for every token
            //reference.DigestMethod = signedXml.SignatureMethod;


            //var transform = new XmlDsigExcC14NTransform();
            //reference.AddTransform(transform);

            //signedXml.AddReference(reference);
            //signedXml.KeyInfo.AddClause(new KeyInfoX509Data(cert));



            //reference.DigestMethod = signedXml.SignatureMethod;

            //signedXml.ComputeSignature();

            //XmlElement xmlDigitalSignature = signedXml.GetXml();

            //doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
        }

        private SecurityToken GetSecurityToken(string samlAssertionXml)
        {
            StringReader stringReader = new StringReader(samlAssertionXml);
            XmlTextReader reader = new XmlTextReader(stringReader);

            SamlSecurityTokenHandler handler = new SamlSecurityTokenHandler();
            handler.Configuration = new SecurityTokenHandlerConfiguration();

            SecurityToken samlToken = handler.ReadToken(reader);
            return samlToken;
        }

        private ClaimsIdentity ValidateSamlToken(SecurityToken securityToken)
        {
            var registry = new ConfigurationBasedIssuerNameRegistry();
            //For Webapp //var thumbprint = WebConfigurationManager.AppSettings["Thumbprint"];

            //Not sure where to find thumbprint? Still on default thumbprint
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

        class TokenHandler : SamlSecurityTokenHandler
    {
        public TokenHandler() : base() { }
        //protected override void ValidateConfirmationData(Saml2SubjectConfirmationData confirmationData) { }
    }
}
