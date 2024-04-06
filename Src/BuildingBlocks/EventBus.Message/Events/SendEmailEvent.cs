﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Message.Events
{
    public class SendEmailEvent : IntegrationBaseEvent
    {
        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string To { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Subject { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Body { get; set; }
    }
}
