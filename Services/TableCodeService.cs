using HashidsNet;
using Microsoft.AspNetCore.DataProtection;
using System.Net;

namespace ASM_1.Services
{
    public class TableCodeService
    {
        private readonly Hashids _hashids;

        public TableCodeService()
        {
            _hashids = new Hashids("TableCodeSalt_v1", 6);
        }

        public string EncryptTableId(int tableId)
        {
            return _hashids.Encode(tableId);
        }

        public int? DecryptTableCode(string code)
        {
            try
            {
                int[] numbers = _hashids.Decode(code);
                if (numbers.Length > 0)
                    return numbers[0];
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
