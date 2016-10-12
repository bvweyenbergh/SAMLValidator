using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAMLValidation
{
    class Validate
    {
        static void Main(string[] args)
        {
            var samlToken = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz48QXNzZXJ0aW9uIHhtbG5zPSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoyLjA6YXNzZXJ0aW9uIiBJRD0iXzZjOGU2ZmE1LTkwNDQtNGNjMy05ZGUyLTc3ZTRhMWQ5NzQ4ZiIgSXNzdWVJbnN0YW50PSIyMDE2LTEwLTEyVDIxOjM4OjI3LjExM1oiIFZlcnNpb249IjIuMCI+PElzc3Vlcj5odHRwOi8vc3RzLm15bm92dGVzdC5jb20vYWRmcy9zZXJ2aWNlcy90cnVzdDwvSXNzdWVyPjxkczpTaWduYXR1cmUgeG1sbnM6ZHM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvMDkveG1sZHNpZyMiPjxkczpTaWduZWRJbmZvPjxkczpDYW5vbmljYWxpemF0aW9uTWV0aG9kIEFsZ29yaXRobT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS8xMC94bWwtZXhjLWMxNG4jIi8+PGRzOlNpZ25hdHVyZU1ldGhvZCBBbGdvcml0aG09Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvMDkveG1sZHNpZyNyc2Etc2hhMSIvPjxkczpSZWZlcmVuY2UgVVJJPSIjXzZjOGU2ZmE1LTkwNDQtNGNjMy05ZGUyLTc3ZTRhMWQ5NzQ4ZiI+PGRzOlRyYW5zZm9ybXM+PGRzOlRyYW5zZm9ybSBBbGdvcml0aG09Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvMDkveG1sZHNpZyNlbnZlbG9wZWQtc2lnbmF0dXJlIi8+PGRzOlRyYW5zZm9ybSBBbGdvcml0aG09Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvMTAveG1sLWV4Yy1jMTRuIyIvPjwvZHM6VHJhbnNmb3Jtcz48ZHM6RGlnZXN0TWV0aG9kIEFsZ29yaXRobT0iaHR0cDovL3d3dy53My5vcmcvMjAwMC8wOS94bWxkc2lnI3NoYTEiLz48ZHM6RGlnZXN0VmFsdWU+cm14WXNqVFdFaGVadkVpYnZlbUtnbmNhSStvPTwvZHM6RGlnZXN0VmFsdWU+PC9kczpSZWZlcmVuY2U+PC9kczpTaWduZWRJbmZvPjxkczpTaWduYXR1cmVWYWx1ZT5WaHdmWlFDa0VQemFIZVNsWUl4NnROa1dYQkt6VkZZZkZtbk9oT2NyeGJwZlYxOTBjZGFEUVMzOVQwNFdSeldxTm5tUGIxR2grZlJMd01Vckh5WXo0OWNxbWdrWm1GQzlsR3hwOUZIRUpZeU0xa1pQbzlaMWFSeVpYdTE3cnlVVVRpYlIwUmc1S3ZDRmZmdngzZW9oT2R3NTVsVkNRT29CTTBrZ1owQzVmM09HUU04SVVUcGgxakVlelhjRVVyaTlrTzg2d1FWTUs4eGZ2eHBpclBnb0FsdEUyK3c0UXFpMXc2di9GSGQraUVYYXE5YjlOdFBldjV6ZXRlWnJyQkRIbG9NelJtQkM5YTVqWTBYc1ZBSkZ5d1B5U3hhbTFTaWNjOXhUV2RNa1liQjdoYm91Y1J6WWZtQTJQWjlPVU8yMlBJL1ZXMnJoSlhRS3hpQ0UvNUpocXc9PTwvZHM6U2lnbmF0dXJlVmFsdWU+PEtleUluZm8geG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvMDkveG1sZHNpZyMiPjxkczpYNTA5RGF0YT48ZHM6WDUwOUNlcnRpZmljYXRlPk1JSUMzakNDQWNhZ0F3SUJBZ0lRS3M1am85ZkVuN0ZFR1BoYVVCRGlBVEFOQmdrcWhraUc5dzBCQVFzRkFEQXJNU2t3SndZRFZRUURFeUJCUkVaVElGTnBaMjVwYm1jZ0xTQnpkSE11YlhsdWIzWjBaWE4wTG1OdmJUQWVGdzB4TlRBeU1EUXhOelEyTVRSYUZ3MHhPREF5TURReE56UTJNVFJhTUNzeEtUQW5CZ05WQkFNVElFRkVSbE1nVTJsbmJtbHVaeUF0SUhOMGN5NXRlVzV2ZG5SbGMzUXVZMjl0TUlJQklqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUEwZWZiancwZEtNUTd3YTRNdTVNNmNhUm04OUVPUDNqQXlwK3cxOHNMMTUzczlyMi94YkNhMmFiQ0dnYmR2M0lWSWREOEFScDNBc0VndkN6YXNwMC9XNG5WeHBkR0dodlNSbWM0QTlIZ1BTYkZOZmQxcHZPdG5KdFVNR1Ywd2NieFJPemZ5SUlndkxIRUN6R0VxVjZqUjZmTWVkWmpzTFpsRU42Qjk2UWdsZ1NwdVdSWGphQmF4YjdqSEF3RzRha3FzMGxMOFIrZ29taXNHeGptZTFaVzVFVHI0aFlzWEhjMXRQSWFLTzJNc1JEL2tHYkJOcjhTV2ViQ280L2xMZUw4YW5XNGtMSHFOdlRldEhVRk84WHVZOFg0b01KN3pZZHlYVlZOYko3U1FlV1EwTjVNWVVaNDE0SkY0dmh2cW9SV3dzNTJDbExUZ3lwQWxWazg1QXJKVndJREFRQUJNQTBHQ1NxR1NJYjNEUUVCQ3dVQUE0SUJBUUNmcWVnR1FnN0MxTWRIQ1FEMGZiZTZXZ1Q2MjhvQXVHeVhpZkZXWm4xRE5DaFE0R0FNa0NBWUJQc3NGb1h1b1RtUnRRN0lZK1ZqNmdNa0V4REFxSHE1b203OEVmd2NPY214SUt3ek9FSHlVWjh6VzlRZDU0VEtqSllYUGhMZGd0L3IvV2Vadk9tWExSK09sLzdKbFZaZUcrSXc1RFhTeTIwb1cvVXh2azVUNkp2ZkExdWo0cmM5NUdralJQMVNuMDhGT09xSFduUmVyaEN2YkNocFZkUDJtcG1jOGlHeUlpWld3WDR1S3FEdUloQ0c1SkIycldjblhzbEs2QWgzYktPZWNjRTRPMGN2c1lPS0JpWGdRWmtnY3hsN082VHF4TjRMUHpQNzBrK0FYVVQzRzFiVlV5aWdCSkp5dHgvZzR4QlVQaXp3cjAwU1hiM0RnVzZFejFzRDwvZHM6WDUwOUNlcnRpZmljYXRlPjwvZHM6WDUwOURhdGE+PC9LZXlJbmZvPjwvZHM6U2lnbmF0dXJlPjxTdWJqZWN0PjxOYW1lSUQgRm9ybWF0PSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoxLjE6bmFtZWlkLWZvcm1hdDplbWFpbEFkZHJlc3MiPlJhbWppLkFpbmFwdXJhcHVAbm92LmNvbTwvTmFtZUlEPjxTdWJqZWN0Q29uZmlybWF0aW9uIE1ldGhvZD0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmNtOmJlYXJlciI+PFN1YmplY3RDb25maXJtYXRpb25EYXRhIEluUmVzcG9uc2VUbz0iYTNkN2JlMTgyM2FnZzFqNzIzaWllZGM3YzEzY2ozaiIgTm90T25PckFmdGVyPSIyMDE2LTEwLTEyVDIxOjQzOjI3LjExM1oiIFJlY2lwaWVudD0iaHR0cHM6Ly9jd3BkZXYubm92LmNvbS9jd3Avc2FtbC9TU08iLz48L1N1YmplY3RDb25maXJtYXRpb24+PC9TdWJqZWN0PjxDb25kaXRpb25zIE5vdEJlZm9yZT0iMjAxNi0xMC0xMlQyMTozODoyNy4wNjlaIiBOb3RPbk9yQWZ0ZXI9IjIwMTYtMTAtMTJUMjI6Mzg6MjcuMDY5WiI+PEF1ZGllbmNlUmVzdHJpY3Rpb24+PEF1ZGllbmNlPm5vdjpjZXQ6Y3dwOmRldjwvQXVkaWVuY2U+PC9BdWRpZW5jZVJlc3RyaWN0aW9uPjwvQ29uZGl0aW9ucz48QXR0cmlidXRlU3RhdGVtZW50PjxBdHRyaWJ1dGUgTmFtZT0iaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIiBhOk9yaWdpbmFsSXNzdWVyPSJodHRwOi8vc3RzdGVzdC5ub3YuY29tL2FkZnMvc2VydmljZXMvdHJ1c3QiIHhtbG5zOmE9Imh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDkvMDkvaWRlbnRpdHkvY2xhaW1zIj48QXR0cmlidXRlVmFsdWU+UmFtamkuQWluYXB1cmFwdUBub3YuY29tPC9BdHRyaWJ1dGVWYWx1ZT48L0F0dHJpYnV0ZT48QXR0cmlidXRlIE5hbWU9Imh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL2NsYWltcy9GaXJzdE5hbWUiIGE6T3JpZ2luYWxJc3N1ZXI9Imh0dHA6Ly9zdHN0ZXN0Lm5vdi5jb20vYWRmcy9zZXJ2aWNlcy90cnVzdCIgeG1sbnM6YT0iaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwOS8wOS9pZGVudGl0eS9jbGFpbXMiPjxBdHRyaWJ1dGVWYWx1ZT5TYXR5YSBSYW1qaTwvQXR0cmlidXRlVmFsdWU+PC9BdHRyaWJ1dGU+PEF0dHJpYnV0ZSBOYW1lPSJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy9jbGFpbXMvTGFzdE5hbWUiIGE6T3JpZ2luYWxJc3N1ZXI9Imh0dHA6Ly9zdHN0ZXN0Lm5vdi5jb20vYWRmcy9zZXJ2aWNlcy90cnVzdCIgeG1sbnM6YT0iaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwOS8wOS9pZGVudGl0eS9jbGFpbXMiPjxBdHRyaWJ1dGVWYWx1ZT5BaW5hcHVyYXB1PC9BdHRyaWJ1dGVWYWx1ZT48L0F0dHJpYnV0ZT48L0F0dHJpYnV0ZVN0YXRlbWVudD48QXV0aG5TdGF0ZW1lbnQgQXV0aG5JbnN0YW50PSIyMDE2LTEwLTEyVDIxOjM4OjI1Ljc4MVoiIFNlc3Npb25JbmRleD0iXzZjOGU2ZmE1LTkwNDQtNGNjMy05ZGUyLTc3ZTRhMWQ5NzQ4ZiI+PEF1dGhuQ29udGV4dD48QXV0aG5Db250ZXh0Q2xhc3NSZWY+dXJuOmZlZGVyYXRpb246YXV0aGVudGljYXRpb246d2luZG93czwvQXV0aG5Db250ZXh0Q2xhc3NSZWY+PC9BdXRobkNvbnRleHQ+PC9BdXRoblN0YXRlbWVudD48L0Fzc2VydGlvbj4=";

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = appDirectory + System.Configuration.ConfigurationManager.AppSettings["MetaCertificateFile"];

            AuthorizeToken token = new AuthorizeToken(samlToken, filePath);
            try
            {
                if (!token.isValid())
                {
                    Console.WriteLine("Invalid Token, SAML Validation Failed");
                    //throw new UnauthorizedAccessException("Invalid Token, SAML Validation Failed");
                }
                else
                {
                    var userName = token.UserName;
                    var tokenExpiry = token.ValidTo;

                    Console.WriteLine("User is: " + userName + " and Token expires at: " + tokenExpiry);
                }
                    
            }
            catch (SecurityTokenExpiredException)
            {
                Console.WriteLine("Security token Expired");
                //throw new UnauthorizedAccessException("Security Token Expired");
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Invalid Token, Exception occured: " + ex.Message);
                //throw new UnauthorizedAccessException(ex.Message); 
            }

            Console.ReadLine();
        }
    }
}
