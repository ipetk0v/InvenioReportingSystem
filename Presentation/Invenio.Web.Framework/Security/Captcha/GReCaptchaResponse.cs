﻿using System.Collections.Generic;

namespace Invenio.Web.Framework.Security.Captcha
{
    public class GReCaptchaResponse
    {
        public bool IsValid { get; set; }
        public List<string> ErrorCodes { get; set; }

        public GReCaptchaResponse()
        {
            ErrorCodes = new List<string>();
        }
    }
}