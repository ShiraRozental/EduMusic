using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class CustomExceptions
    {
        // שגיאה עבור מצב שבו פריט לא נמצא (מחזיר בדרך כלל 404)
        public class NotFoundException(string message) : Exception(message);

        // שגיאה עבור נתונים לא תקינים שנשלחו מהמשתמש (מחזיר בדרך כלל 400)
        public class BadRequestException(string message) : Exception(message);

        // שגיאה עבור התנגשות בנתונים, כמו משתמש שכבר קיים (מחזיר בדרך כלל 409)
        public class ConflictException(string message) : Exception(message);

        // שגיאה עבור בעיות אימות או טוקן לא תקין (מחזיר בדרך כלל 401)
        public class UnauthorizedException(string message) : Exception(message);

        // שגיאה עבור מצב שבו אין למשתמש הרשאה לביצוע פעולה ספציפית (מחזיר בדרך כלל 403)
        public class ForbiddenException(string message) : Exception(message);
    }
}

