using MailKit;
using MimeKit;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MailClient
{
    [AddINotifyPropertyChangedInterface]
    class ShowMessage 
    {
        public ShowMessage() { }
        public UniqueId ID { get; set; }
        public string From { get; set; }
        public string TextBody { get; set; }
        public string Subject { get; set; }
        public string attachedFile { get; set; }
        public IEnumerable<MimeEntity> AttachedFiles { get; set; }
        public string time { get; set; }
        //public DateTime time { get; set; }
        public int Count { get; set; }

    }
}