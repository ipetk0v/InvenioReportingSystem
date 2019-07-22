﻿using System.Collections.Generic;
using System.Web.Mvc;
//using Invenio.Services.Payments;

namespace Invenio.Web.Framework.Controllers
{
    /// <summary>
    /// Base controller for payment plugins
    /// </summary>
    public abstract class BasePaymentController : BasePluginController
    {
        public abstract IList<string> ValidatePaymentForm(FormCollection form);
        //public abstract ProcessPaymentRequest GetPaymentInfo(FormCollection form);
    }
}
