using Microsoft.AspNetCore.DataProtection;
using System.Net;

namespace ASM_1.Services
{
    public class TableCodeService
    {
        private readonly IDataProtector _protector;

        public TableCodeService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("TableCode.v1");
        }

        public string EncryptTableId(int tableId)
        {
            string plain = tableId.ToString();
            string protectedText = _protector.Protect(plain);
            return WebUtility.UrlEncode(protectedText);
        }

        public int? DecryptTableCode(string code)
        {
            try
            {
                string decoded = WebUtility.UrlDecode(code);
                string plain = _protector.Unprotect(decoded);
                return int.Parse(plain);
            }
            catch
            {
                return null;
            }
        }
    }
}
